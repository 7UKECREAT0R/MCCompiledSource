using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.ServerWebSocket;

/// <summary>
///     MCCompiled Language Server
///     Communicates with the webapp in order to extend functionality.
///     - Gives code diagnostics
///     - Compiles code into the user's filesystem.
///     - Saves/loads data from the filesystem.
/// </summary>
public class MCCServer : IDisposable
{
    public const int
        PORT = 11830; // The port that the server will be opened on. The Minecraft version at the time of the first working prototype (1.18.30).
    public const float
        STANDARD_VERSION = 5.8f; // The version of the standard this implementation of the server follows.
    public const int CHUNK_SIZE = 0x100000; // 1MB
    public const int READ_SIZE = 0x80;

    public const string WEBSOCKET_MAGIC = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

    public static readonly Encoding ENCODING = Encoding.UTF8;
    private readonly ManualResetEvent connectionEstablished;
    private readonly IPEndPoint ip;
    private readonly List<byte[]> multiparts;

    private readonly string
        outputResourcePack,
        outputBehaviorPack;
    private readonly MCCServerProject project;
    private readonly SHA1 sha1;
    private readonly Socket socket;
    private readonly WorkspaceManager workspaceManager;

    private bool _isDisposed;

    internal bool debug;

    private WebSocketFrame multipartHeader;

    /// <summary>
    ///     Creates a new MCCServer and sets up Socket for opening.
    /// </summary>
    /// <param name="outputResourcePack">
    ///     The output location for the resource pack. Use '?project' to denote the name of the
    ///     project.
    /// </param>
    /// <param name="outputBehaviorPack">
    ///     The output location for the behavior pack. Use '?project' to denote the name of the
    ///     project.
    /// </param>
    public MCCServer(string outputResourcePack, string outputBehaviorPack)
    {
        this.outputResourcePack = outputResourcePack;
        this.outputBehaviorPack = outputBehaviorPack;
        this.connectionEstablished = new ManualResetEvent(false);

        this.project = new MCCServerProject(this);
        this.workspaceManager = new WorkspaceManager();
        this.ip = new IPEndPoint(IPAddress.Loopback, PORT);
        this.socket = new Socket(this.ip.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);
        this.sha1 = SHA1.Create();
        this.multiparts = [];
    }
    public void Dispose()
    {
        if (this._isDisposed)
            return;

        this.socket?.Dispose();
        this._isDisposed = true;
    }
    private WebSocketFrame PopMultipart()
    {
        if (this.multipartHeader == null)
            return null;
        if (this.multiparts.Count < 0)
            return this.multipartHeader;

        long baseLength = this.multipartHeader.data.LongLength;
        long length = baseLength + this.multiparts.Sum(array => array.LongLength);

        byte[] merge = new byte[length];
        long index = 0;

        Array.Copy(this.multipartHeader.data, 0L, merge, index, baseLength);
        index += baseLength;

        foreach (byte[] buffer in this.multiparts)
        {
            long bufferLength = buffer.LongLength;
            Array.Copy(buffer, 0, merge, index, bufferLength);
            index += bufferLength;
        }

        this.multipartHeader.data = merge;
        this.multipartHeader.fin = true;
        this.multiparts.Clear();

        GC.Collect(); // that's a lot of bytes that just got tossed

        return this.multipartHeader;
    }

    /// <summary>
    ///     Starts the synchronous loop that listens for clients to connect.
    /// </summary>
    public void StartServer()
    {
        this.socket.Bind(this.ip);
        this.socket.Listen(100); // 100 packet queue

        Console.WriteLine("Now listening for socket connection on " + this.ip + ".");
        Console.WriteLine("Language Server Version {0}", STANDARD_VERSION);
        Console.WriteLine("MCCompiled Version 1.{0}", Executor.MCC_VERSION);

        // begin accepting clients
        while (true)
        {
            // reset connectionEstablished
            this.connectionEstablished.Reset();

            // wait for a client to connect
            var callback = new AsyncCallback(OnConnectionOpened);
            this.socket.BeginAccept(callback, this.socket);

            // wait for connectionEstablished to be set, meaning we can now wait for the next incoming connection
            this.connectionEstablished.WaitOne();
        }
    }
    /// <summary>
    ///     Called when a new connection is opened with a client.
    /// </summary>
    /// <param name="result"></param>
    public void OnConnectionOpened(IAsyncResult result)
    {
        // Set connectionEstablished, which triggers the StartServer() to loop again.
        // Essentially starts waiting for another client connection after this one is established.
        this.connectionEstablished.Set();

        // get client/server sockets
        object state = result.AsyncState;

        var server = (Socket) state;
        Debug.Assert(server != null, nameof(server) + " != null");
        server!.ReceiveBufferSize = CHUNK_SIZE;

        Socket client = server.EndAccept(result);

        // create a package for sending to the read thread
        var package = new WebSocketPackage
            (server, client, this);

        // run the reception loop for the connected client.
        Task.Factory.StartNew(() =>
        {
            ReceiveLoop(package);
        }, TaskCreationOptions.LongRunning);
    }
    /// <summary>
    ///     Called when an open connection is sent a TCP packet to be processed.
    /// </summary>
    /// <param name="package">The WebSocket-related context for the executing thread to use.</param>
    public void ReceiveLoop(WebSocketPackage package)
    {
        byte[] tempBuffer = new byte[READ_SIZE];

        Debug.WriteLine("Started thread! Client handle " + package.client.Handle);

        while (true)
        {
            int bytesReadTotal = 0;
            int bytesRead;

            Array.Clear(tempBuffer, 0, READ_SIZE);

            if (package.didHandshake)
            {
                // read the first two bytes of the header.
                if (package.client.Receive(tempBuffer, 0, 2, SocketFlags.None) < 2)
                    throw new Exception("Client sent incomplete frame header to the server.");

                // parse bytes 0 and 1.
                WebSocketByte0Info byte0 = WebSocketFrame.ParseByte0(tempBuffer[0]);
                WebSocketByte1Info byte1 = WebSocketFrame.ParseByte1(tempBuffer[1]);

                // figure out how many more bytes will be needed based on the information
                // from bytes 0/1. mask is 4 bytes, extensions are 0/2/8 bytes respectively.
                int additionalBytesNeeded = (int) byte1.extension + (byte1.mask ? 4 : 0);

                if (additionalBytesNeeded > 0)
                    if (package.client.Receive(tempBuffer, 2, additionalBytesNeeded, SocketFlags.None) <
                        additionalBytesNeeded)
                        throw new Exception("Client sent incomplete frame header to the server.");

                // constructs a WebSocketFrame from the remainder of the header that was just read.
                WebSocketFrame frame = WebSocketFrame.FromFrameHeader(byte0, byte1, tempBuffer);
                int length = (int) frame.length;

                byte[] content;

                if (length > 0)
                {
                    // get the number of requested bytes by the frame header.
                    content = new byte[length];
                    bytesRead = 0;

                    int remaining = length - bytesRead;

                    while (remaining > 0)
                    {
                        bytesRead += package.client.Receive(content, bytesRead, remaining, SocketFlags.None);
                        remaining = length - bytesRead;
                    }

                    if (bytesRead < length)
                        throw new Exception("Client did not fulfill WebSocket length promise.");
                }
                else
                {
                    content = [];
                }

                Debug.WriteLine($"Got frame: {frame}");

                // set the newly read data.
                frame.SetData(content);
                bool close = HandleFrame(package, frame);

                if (close)
                {
                    // close the connection with the client.
                    package.Close(false);
                    return;
                }
            }
            else
            {
                // read a standard string
                while (package.client.Available > 0)
                {
                    bytesRead = package.client.Receive(tempBuffer);
                    Array.Copy(tempBuffer, 0, package.buffer, bytesReadTotal, bytesRead);
                    bytesReadTotal += bytesRead;
                }

                string str = package.ReadStringASCII(bytesReadTotal);
                Debug.WriteLine(str);

                // the only HTTP used by WebSocket is when initiating the handshake.
                if (str.StartsWith("GET"))
                    ProcessWebsocketUpgrade(package, str);
            }

            Array.Clear(tempBuffer, 0, tempBuffer.Length);
        }
    }
    /// <summary>
    ///     Handle a potential websocket HTTP request sent through TCP.
    /// </summary>
    /// <param name="package"></param>
    /// <param name="data"></param>
    public void ProcessWebsocketUpgrade(WebSocketPackage package, string data)
    {
        // parse request
        Dictionary<string, string> entries = data.ParseHTTP();

        // look for websocket upgrade request
        if (!entries.TryGetValue("Connection", out string value))
            return;
        if (!value.Contains("Upgrade"))
            return;

        // get secret websocket key
        if (!entries.TryGetValue("Sec-WebSocket-Key", out string secret))
            return;

        // calculate response key
        string bigSecret = secret + WEBSOCKET_MAGIC;
        byte[] secretBytes = Encoding.ASCII.GetBytes(bigSecret);
        byte[] secretHash = this.sha1.ComputeHash(secretBytes);
        string acceptKey = Convert.ToBase64String(secretHash);

        // create response data
        const string HANDSHAKE_HEADER = "HTTP/1.1 101 Switching Protocols";
        var http = new Dictionary<string, string>
        {
            ["Upgrade"] = "Websocket",
            ["Connection"] = "Upgrade",
            ["Sec-WebSocket-Accept"] = acceptKey
        };
        Debug.WriteLine("Sec-WebSocket-Accept: " + acceptKey);

        string responseData = http.ToHTTP(HANDSHAKE_HEADER);
        package.SendStringASCII(responseData);
        package.didHandshake = true;

        // send version info
        var json = new JObject();
        json["action"] = "version";
        json["version"] = 1000 + Executor.MCC_VERSION * 10;
        package.SendFrame(WebSocketFrame.JSON(json));

        // send current property info
        var _properties = new JArray(this.project.properties.Select(kv => new JObject
            {
                ["name"] = kv.Key.Base64Encode(),
                ["value"] = kv.Value.Base64Encode()
            })
        );
        var properties = new JObject
        {
            ["action"] = "properties",
            ["properties"] = _properties
        };
        package.SendFrame(WebSocketFrame.JSON(properties));

        // reset the file because the client is no longer in the loop
        string file = this.project.File;
        if (file != null)
        {
            package.SendFrame(CreateNotificationFrame($"Closed file '{file}'.", "gray"));
            this.project.File = null;
        }
    }

    /// <summary>
    ///     Handle an incoming WebSocketFrame from the client.
    /// </summary>
    /// <param name="package"></param>
    /// <param name="frame"></param>
    /// <returns>If the connection with this client should be aborted.</returns>
    public bool HandleFrame(WebSocketPackage package, WebSocketFrame frame)
    {
        if (frame == null)
            return false;

        // handle multipart/continuation
        if (frame.fin == false)
        {
            if (frame.opcode == WebSocketOpCode.CONTINUATION)
            {
                this.multiparts.Add(frame.data);
                return false;
            }

            this.multipartHeader = frame;
            this.multiparts.Clear();
            return false;
        }

        if (frame.opcode == WebSocketOpCode.CONTINUATION)
        {
            // this is the last in the continuation sequence
            this.multiparts.Add(frame.data);
            frame = PopMultipart(); // merge all the data packets into one
            this.multipartHeader = null;

            if (this.debug)
                Console.WriteLine("multipart: fin (see below)");
        }

        if (this.debug)
            Console.WriteLine("Received {0}:\n\t{1}", frame.opcode, Encoding.UTF8.GetString(frame.data));

        if (frame.opcode == WebSocketOpCode.CLOSE)
        {
            WebSocketFrame response = WebSocketFrame.Close();
            Debug.WriteLine("Closing...");
            package.SendFrame(response);
            return true;
        }

        if (frame.opcode == WebSocketOpCode.PING)
        {
            WebSocketFrame pong = WebSocketFrame.Pong();
            package.SendFrame(pong);
            return false;
        }

        if (frame.opcode == WebSocketOpCode.TEXT)
        {
            // get the text
            string text = ENCODING.GetString(frame.data);

            // this is probably JSON
            if (text.StartsWith("{") && text.EndsWith("}"))
            {
                JObject json = JObject.Parse(text);
                return HandleJSON(package, json);
            }

            // unknown text
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Got unknown text?:\n\t{0}", text);
            Console.ForegroundColor = oldColor;
        }

        return false;
    }
    /// <summary>
    ///     Handle an incoming JSON message from the client.
    /// </summary>
    /// <param name="package"></param>
    /// <param name="json"></param>
    /// <returns>If the connection with this client should be aborted.</returns>
    public bool HandleJSON(WebSocketPackage package, JObject json)
    {
        if (!json.TryGetValue("action", out JToken value))
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Received JSON with no action property:\n{0}", json);
            Console.ForegroundColor = color;
            return false;
        }

        string action = value.Value<string>();

        if (action.Equals("ping"))
        {
            WebSocketFrame response = CreateNotificationFrame("Pong! -MCCCompiled", "white");
            package.SendFrame(response);
            return false;
        }

        if (action.Equals("debug")) // Deprecated, use project properties
        {
            bool enable = json["debug"].Value<bool>();
            this.debug = enable;
            GlobalContext.Current.debug = enable;

            if (enable)
            {
                package.SendFrame(CreateNotificationFrame("Enabled debugging on server.", "#3cc741"));
                Console.WriteLine("Enabled debugging from remote client.");
            }
            else
            {
                package.SendFrame(CreateNotificationFrame("Disabled debugging on server.", "#3cc741"));
                Console.WriteLine("Disabled debugging from remote client.");
            }

            return false;
        }

        if (action.Equals("property"))
        {
            string propertyName = json["name"].ToString().Base64Decode();
            string propertyValue = json["value"].ToString().Base64Decode();
            this.project.SetProperty(propertyName, propertyValue);
        }

        if (action.Equals("close"))
            return true;
        if (action.Equals("info"))
        {
            var info = new JObject
            {
                ["action"] = "menu",
                ["html"] = CreateGenericMenu("Server Info",
                        "Language Server Version: " + STANDARD_VERSION,
                        "MCCompiled Version: 1." + Executor.MCC_VERSION,
                        "Made for Minecraft Version: " + Executor.MINECRAFT_VERSION,
                        "",
                        "Fakeplayer Name: " + Executor.FAKE_PLAYER_NAME,
                        "Maximum Code Depth: " + Executor.MAXIMUM_DEPTH)
                    .Base64Encode()
            };

            WebSocketFrame frame = WebSocketFrame.JSON(info);
            package.SendFrame(frame);
            return false;
        }

        if (action.Equals("openfolder"))
        {
            string folder = json["folder"].ToString();

            string toOpen = folder switch
            {
                "current" => Directory.GetCurrentDirectory(),
                "user" => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "bp" => this.outputBehaviorPack.Replace("?project_BP", ""), // no project
                "rp" => this.outputResourcePack.Replace("?project_RP", ""), // no project
                "install" => Path.GetDirectoryName(AppContext.BaseDirectory),
                _ => null
            };

            if (this.debug)
                Console.WriteLine("Opening folder: {0}", toOpen);

            Process.Start("explorer.exe", '"' + toOpen + '"');
            return false;
        }

        // Code Actions
        if (action.Equals("lint"))
        {
            string encoded = json["code"].Value<string>();
            string code = encoded.Base64Decode();

            if (this.debug)
                Console.WriteLine("Linting...");

            Lint(code, package);
            return false;
        }

        if (action.Equals("compile"))
        {
            string encodedCode = json["code"].Value<string>();
            string code = encodedCode.Base64Decode();
            string encodedProject = json["project"].Value<string>();
            string decodedProject = encodedProject.Base64Decode();

            if (this.debug)
                Console.WriteLine("Compiling project '{0}'...", decodedProject);

            Compile(code, decodedProject, package);
            return false;
        }

        // I/O
        // Requires STA thread.
        const string META_HEADER = "// $meta-start";
        const string META_FIELD_METADATA = "// $meta ";
        const string META_FIELD_PROPERTIES = "// $meta-props ";
        const string META_FOOTER = "// $meta-end";

        if (action.Equals("save"))
        {
            var thread = new Thread(() =>
            {
                string encodedCode = json["code"].Value<string>();
                string code = encodedCode.Base64Decode();
                var metadata = json["meta"] as JObject;

                if (!this.project.hasFile)
                    if (!this.project.RunSaveFileDialog(out bool unsupported))
                    {
                        package.SendFrame(unsupported
                            ? CreateNotificationFrame("Saving files is unsupported on non-Windows platforms.",
                                "#DDDDDD")
                            : CreateNotificationFrame("Save was canceled.", "#DDDDDD"));
                        package.SendFrame(CreateBusyFrame(false));
                        return; // stop thread
                    }

                string file = this.project.File;
                Debug.Assert(file != null, nameof(file) + " != null");

                string fileName = Path.GetFileName(file);

                if (File.Exists(file))
                    File.Delete(file);

                using (FileStream stream = File.OpenWrite(file))
                {
                    bool hasMetadata = metadata != null;
                    bool hasProperties = this.project.properties.Count != 0;

                    if (hasMetadata || hasProperties)
                    {
                        // get bytes for the metadata header and footer
                        byte[] header = ENCODING.GetBytes(META_HEADER + '\n');
                        byte[] footer = ENCODING.GetBytes(META_FOOTER + '\n');

                        // start by writing the header
                        stream.Write(header, 0, header.Length);

                        if (hasMetadata)
                        {
                            string block = metadata.ToString(Formatting.None).Base64Encode();
                            string blockString = META_FIELD_METADATA + block + '\n';
                            byte[] blockBytes = ENCODING.GetBytes(blockString);
                            stream.Write(blockBytes, 0, blockBytes.Length);
                        }

                        if (hasProperties)
                        {
                            string block = this.project.PropertiesBase64;
                            string blockString = META_FIELD_PROPERTIES + block + '\n';
                            byte[] blockBytes = ENCODING.GetBytes(blockString);
                            stream.Write(blockBytes, 0, blockBytes.Length);
                        }

                        // write footer now.
                        stream.Write(footer, 0, footer.Length);
                    }

                    byte[] bytes = ENCODING.GetBytes(code);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }

                package.SendFrame(CreateNotificationFrame($"File \"{fileName}\" saved.", "#3cc741"));
                package.SendFrame(CreateBusyFrame(false));
            });

            if (OperatingSystem.IsWindows())
                thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return false;
        }

        if (action.Equals("load"))
        {
            var thread = new Thread(() =>
            {
                if (!this.project.RunLoadFileDialog(out bool unsupported))
                {
                    package.SendFrame(unsupported
                        ? CreateNotificationFrame(
                            "Loading files in the editor is unsupported on non-Windows platforms.", "#DDDDDD")
                        : CreateNotificationFrame("Load canceled by user.", "#DDDDDD"));
                    package.SendFrame(CreateBusyFrame(false));
                    return;
                }

                string file = this.project.File;

                if (!File.Exists(file))
                    return;

                string code = File.ReadAllText(file, ENCODING);
                var metadata = new JObject();

                if (code.StartsWith(META_HEADER))
                {
                    // begin reading metadata.
                    var reader = new StringReader(code);
                    _ = reader.ReadLine(); // skip the header
                    code = "";

                    // read until encountering a footer.
                    string line;
                    while ((line = reader.ReadLine()) != META_FOOTER)
                        if (line.StartsWith(META_FIELD_METADATA))
                        {
                            string dataBlock = line[META_FIELD_METADATA.Length..];
                            metadata = JObject.Parse(dataBlock.Base64Decode());
                        }
                        else if (line.StartsWith(META_FIELD_PROPERTIES))
                        {
                            string dataBlock = line[META_FIELD_PROPERTIES.Length..];
                            this.project.PropertiesBase64 = dataBlock;
                        }
                        else
                        {
                            ConsoleColor oldColor = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Malformed metadata in file \"{file}\". Treating as beginning of code.");
                            Console.ForegroundColor = oldColor;
                            code = line + '\n';
                            break;
                        }

                    // the code is the rest of the segment without the metadata.
                    code += reader.ReadToEnd();

                    reader.Close();
                    reader.Dispose();
                }

                var send = new JObject
                {
                    ["action"] = "postload",
                    ["code"] = code.Base64Encode(),
                    ["meta"] = metadata
                };
                package.SendFrame(WebSocketFrame.JSON(send));

                var _properties = new JArray(this.project.properties.Select(kv => new JObject
                    {
                        ["name"] = kv.Key.Base64Encode(),
                        ["value"] = kv.Value.Base64Encode()
                    })
                );
                var properties = new JObject
                {
                    ["action"] = "properties",
                    ["properties"] = _properties
                };
                package.SendFrame(WebSocketFrame.JSON(properties));

                Lint(code, package);

                if (this.debug)
                    Console.WriteLine("\nGot metadata:\n{0}\n", metadata);
            });

            if (OperatingSystem.IsWindows())
                thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        return false;
    }

    private void Lint(string code, WebSocketPackage package)
    {
        bool debuggerAttached = Debugger.IsAttached;
        GlobalContext.Current.debug = this.debug;
        GlobalContext.Current.projectName = null;

        // lint the code
        WorkspaceManager.ResetStaticStates();
        string virtualFileName = this.project.hasFile ? this.project.fileLocation : "anonymous.mcc";
        Exception exception1 = this.workspaceManager.OpenCodeAsFileAndCaptureExceptions(virtualFileName, code);
        Exception mainException = exception1;
        string json = null;
        if (exception1 == null)
        {
            Exception exception2 = this.workspaceManager.CompileFileAndCaptureExceptions
                (virtualFileName, true, !debuggerAttached, false, out Emission emission);
            mainException ??= exception2;

            // gather information.
            LegacyLintStructure lint = LegacyLintStructure.Harvest(emission);

            if (this.debug)
                Console.WriteLine("\tLint success. " + lint);

            json = lint.ToJSON();
        }

        if (mainException == null)
            // tell the client that there are no more errors
        {
            package.SendFrame(WebSocketFrame.String(
                @"{""action"":""seterrors"",""errors"":[]}"
            ));
        }
        else
        {
            if (mainException is TokenizerException tokenizerException)
            {
                if (this.debug)
                    Console.Error.WriteLine("\tError. " + tokenizerException.Message);
                json = LegacyErrorStructure.Wrap(tokenizerException).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
            }
            else if (mainException is StatementException statementException)
            {
                if (this.debug)
                    Console.Error.WriteLine("\tError. " + statementException.Message);
                json = LegacyErrorStructure.Wrap(statementException).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
            }
            else if (mainException is FeederException feederException)
            {
                if (this.debug)
                    Console.Error.WriteLine("\tError. " + feederException.Message);
                json = LegacyErrorStructure.Wrap(feederException).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
            }
            else
            {
                if (this.debug)
                {
                    Console.Error.WriteLine("\tFatal Error:");
                    Console.Error.WriteLine();
                    Console.Error.WriteLine(mainException.ToString());
                }

                json = LegacyErrorStructure.Wrap(mainException, [0]).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
            }
        }

        if (json != null)
            package.SendFrame(WebSocketFrame.String(json));

        // no longer busy
        package.SendFrame(CreateBusyFrame(false));
    }
    private void Compile(string code, string projectName, WebSocketPackage package)
    {
        WorkspaceManager.ResetStaticStates();
        GlobalContext.Current.debug = this.debug;
        GlobalContext.Current.projectName = projectName;
        string virtualFileName = this.project.hasFile ? this.project.fileLocation : projectName + ".mcc";

        bool success1 = this.workspaceManager.OpenCodeAsFileWithSimpleErrorHandler(virtualFileName, code, false);
        if (success1)
        {
            bool success2 = this.workspaceManager.CompileFileWithSimpleErrorHandler
                (virtualFileName, false, false, false, out Emission emission);

            if (this.debug)
                Console.WriteLine("Compilation Succeeded: {0}", success2);

            if (success2)
            {
                emission.WriteAllFiles();
                package.SendFrame(CreateNotificationFrame("Compilation Succeeded", "#3cc741"));
            }
            else
            {
                package.SendFrame(CreateNotificationFrame("Compilation Failed", "#db4b35"));
            }
        }
        else
        {
            Console.WriteLine("Compilation Succeeded: false");
            package.SendFrame(CreateNotificationFrame("Compilation Failed", "#db4b35"));
        }

        // signal that we're not busy anymore
        package.SendFrame(CreateBusyFrame(false));
    }

    /// <summary>
    ///     Creates a WebSocketFrame to hold an MCCompiled protocol 'notification' action.
    /// </summary>
    /// <param name="text">The text to display in the notification.</param>
    /// <param name="color">The color of the text; name or hexadecimal code.</param>
    /// <returns></returns>
    public static WebSocketFrame CreateNotificationFrame(string text, string color)
    {
        var json = new JObject
        {
            ["action"] = "notification",
            ["text"] = text.Base64Encode(),
            ["color"] = color
        };
        return WebSocketFrame.JSON(json);
    }
    /// <summary>
    ///     Creates a WebSocketFrame to hold an MCCompiled protocol 'busy' action.
    /// </summary>
    /// <param name="busy">If the server is busy.</param>
    /// <returns></returns>
    public static WebSocketFrame CreateBusyFrame(bool busy)
    {
        var json = new JObject
        {
            ["action"] = "busy",
            ["busy"] = busy
        };
        return WebSocketFrame.JSON(json);
    }
    /// <summary>
    ///     Creates HTML for a generic menu with a close button.
    /// </summary>
    /// <param name="title">The title of the menu.</param>
    /// <param name="lines">The lines of text to display in the menu.</param>
    /// <returns></returns>
    public static string CreateGenericMenu(string title, params string[] lines)
    {
        var sb = new StringBuilder();
        sb.Append("<h1>" + title + "</h1>");
        sb.Append("<h2 style=max-width:400px>");
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
                sb.Append(line);
            sb.Append("<br />");
        }

        sb.Append("</h2>");
        sb.Append(@"<div>");
        sb.Append(@"<button onclick=""removeTakeover()"">CLOSE</button>");
        sb.Append(@"</div>");

        return sb.ToString();
    }
}