using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace mc_compiled.MCC.ServerWebSocket
{
    /// <summary>
    /// MCCompiled Language Server
    /// Communicates with the webapp in order to extend functionality.
    /// - Gives code diagnostics
    /// - Compiles code into the user's filesystem.
    /// - Saves/loads data from the filesystem.
    /// </summary>
    public class MCCServer : IDisposable
    {
        public const int PORT = 11830;              // The port that the server will be opened on. The Minecraft version at the time of the first working prototype (1.18.30).
        public const float STANDARD_VERSION = 5.8f; // The version of the standard this implementation of the server follows.
        public const int CHUNK_SIZE = 0x100000;     // 1MB
        public const int READ_SIZE = 0x80;

        public const string WEBSOCKET_MAGIC = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public static readonly Encoding ENCODING = Encoding.ASCII;
       
        internal bool debug;
        private readonly MCCServerProject project;
        private readonly IPEndPoint ip;
        private readonly Socket socket;
        private readonly ManualResetEvent connectionEstablished;
        private readonly SHA1 sha1;

        private WebSocketFrame multipartHeader;
        private readonly List<byte[]> multiparts;
        private WebSocketFrame PopMultipart()
        {
            if (multipartHeader == null)
                return null;
            if (multiparts.Count < 0)
                return multipartHeader;

            long baseLength = multipartHeader.data.LongLength;
            long length = baseLength + multiparts.Sum(array => array.LongLength);

            byte[] merge = new byte[length];
            long index = 0;

            Array.Copy(multipartHeader.data, 0L, merge, index, baseLength);
            index += baseLength;

            foreach (byte[] buffer in multiparts)
            {
                long bufferLength = buffer.LongLength;
                Array.Copy(buffer, 0, merge, index, bufferLength);
                index += bufferLength;
            }

            multipartHeader.data = merge;
            multipartHeader.fin = true;
            multiparts.Clear();

            GC.Collect(); // that's a lot of bytes that just got tossed

            return multipartHeader;
        }


        private readonly string
            outputResourcePack,
            outputBehaviorPack;

        /// <summary>
        /// Creates a new MCCServer and sets up Socket for opening.
        /// </summary>
        /// <param name="outputResourcePack">The output location for the resource pack. Use '?project' to denote the name of the project.</param>
        /// <param name="outputBehaviorPack">The output location for the behavior pack. Use '?project' to denote the name of the project.</param>
        public MCCServer(string outputResourcePack, string outputBehaviorPack)
        {
            this.outputResourcePack = outputResourcePack;
            this.outputBehaviorPack = outputBehaviorPack;
            this.connectionEstablished = new ManualResetEvent(false);

            this.project = new MCCServerProject(this);
            this.ip = new IPEndPoint(IPAddress.Loopback, PORT);
            this.socket = new Socket(ip.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            this.sha1 = SHA1.Create();
            this.multiparts = new List<byte[]>();
        }

        bool _isDisposed = false;
        public void Dispose()
        {
            if (_isDisposed)
                return;

            socket?.Dispose();
            _isDisposed = true;
        }

        /// <summary>
        /// Starts the synchronous loop that listens for clients to connect.
        /// </summary>
        public void StartServer()
        {
            socket.Bind(ip);
            socket.Listen(100); // 100 packet queue

            Console.WriteLine("Now listening for socket connection on " + ip + ".");
            Console.WriteLine("Language Server Version {0}", STANDARD_VERSION);
            Console.WriteLine("MCCompiled Version {0}", Executor.MCC_VERSION);
            
            // begin accepting clients
            while (true)
            {
                // reset connectionEstablished
                connectionEstablished.Reset();

                // wait for a client to connect
                var callback = new AsyncCallback(OnConnectionOpened);
                socket.BeginAccept(callback, socket);

                // wait for connectionEstablished to be set, meaning we can now wait for the next incoming connection
                connectionEstablished.WaitOne();
            }
        }
        /// <summary>
        /// Called when a new connection is opened with a client.
        /// </summary>
        /// <param name="result"></param>
        public void OnConnectionOpened(IAsyncResult result)
        {
            // Set connectionEstablished, which triggers the StartServer() to loop again.
            // Essentially starts waiting for another client connection after this one is established.
            connectionEstablished.Set();

            // get client/server sockets
            object state = result.AsyncState;

            var server = (Socket)state;
            server.ReceiveBufferSize = MCCServer.CHUNK_SIZE;

            Socket client = server.EndAccept(result);

            // create a package for sending to the read thread
            var package = new WebSocketPackage
                (server, client, this);

            // run the receive loop for the connected client. 
            var thread = new Thread(ReceiveLoop);
            thread.Start(package);
        }
        /// <summary>
        /// Called when an open connection is sent a TCP packet to be processed.
        /// </summary>
        /// <param name="result"></param>
        public void ReceiveLoop(object result)
        {
            var package = (WebSocketPackage)result;
            byte[] tempBuffer = new byte[READ_SIZE];

            Debug.WriteLine("Started thread! Client handle " + package.client.Handle);

            while (true)
            {
                int bytesReadTotal = 0;
                int bytesRead = 0;

                Array.Clear(tempBuffer, 0, READ_SIZE);

                if(package.didHandshake)
                {
                    // read the first two bytes of the header.
                    if ((bytesRead = package.client.Receive(tempBuffer, 0, 2, SocketFlags.None)) < 2)
                        throw new Exception("Client sent incomplete frame header to the server.");

                    // parse bytes 0 and 1.
                    WebSocketByte0Info byte0 = WebSocketFrame.ParseByte0(tempBuffer[0]);
                    WebSocketByte1Info byte1 = WebSocketFrame.ParseByte1(tempBuffer[1]);

                    // figure out how many more bytes will be needed based on the information
                    // from bytes 0/1. mask is 4 bytes, extensions are 0/2/8 bytes respectively.
                    int additionalBytesNeeded = (int)byte1.extension + (byte1.mask ? 4 : 0);

                    if (additionalBytesNeeded > 0)
                    {
                        if ((bytesRead = package.client.Receive(tempBuffer, 2, additionalBytesNeeded, SocketFlags.None)) < additionalBytesNeeded)
                            throw new Exception("Client sent incomplete frame header to the server.");
                    }
                    
                    // constructs a WebSocketFrame from the remainder of the header that was just read.
                    WebSocketFrame frame = WebSocketFrame.FromFrameHeader(byte0, byte1, tempBuffer);
                    int length = (int)frame.length;

                    byte[] content;

                    if (length > 0)
                    {
                        // get the numbr of requested bytes by the frame header.
                        content = new byte[length];
                        bytesRead = 0;

                        int remaining = length - bytesRead;

                        while (remaining > 0)
                        {
                            bytesRead += package.client.Receive(content, bytesRead, remaining, SocketFlags.None);
                            remaining = length - bytesRead;
                        }

                        if (bytesRead < (int)length)
                            throw new Exception("Client did not fulfill WebSocket length promise.");
                    } else
                        content = Array.Empty<byte>();
                    
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
                    while (package.client.Available > 0) {
                        bytesRead = package.client.Receive(tempBuffer);
                        Array.Copy(tempBuffer, 0, package.buffer, bytesReadTotal, bytesRead);
                        bytesReadTotal += bytesRead;
                    }
                    
                    string str = package.ReadStringASCII(bytesReadTotal);
                    Debug.WriteLine(str);
                    
                    // the only HTTP used by WebSocket is when initiating the handshake.
                    if (str.StartsWith("GET"))
                        ProcessWebsocketUpgrade(package, str);
                    
                    bytesReadTotal = 0;
                }
                
                Array.Clear(tempBuffer, 0, tempBuffer.Length);
            }
        }
        /// <summary>
        /// Handle a potential websocket HTTP request sent through TCP.
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
            byte[] secretHash = sha1.ComputeHash(secretBytes);
            string acceptKey = Convert.ToBase64String(secretHash);

            // create response data
            const string HANDSHAKE_HEADER = "HTTP/1.1 101 Switching Protocols";
            Dictionary<string, string> http = new Dictionary<string, string>()
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
            JObject json = new JObject();
            json["action"] = "version";
            json["version"] = (int)(Executor.MCC_VERSION * 1000);
            package.SendFrame(WebSocketFrame.JSON(json));

            // send current property info
            JArray _properties = new JArray(
                project.properties.Select(kv => new JObject()
                {
                    ["name"] = kv.Key.Base64Encode(),
                    ["value"] = kv.Value.Base64Encode()
                })
            );
            JObject properties = new JObject()
            {
                ["action"] = "properties",
                ["properties"] = _properties
            };
            package.SendFrame(WebSocketFrame.JSON(properties));

            // reset the file because the client is no longer in the loop
            string file = project.File;
            if (file != null)
            {
                package.SendFrame(CreateNotificationFrame($"Closed file '{file}'.", "gray"));
                project.File = null;
            }
        }

        /// <summary>
        /// Handle an incoming WebSocketFrame from the client.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="frame"></param>
        /// <returns>If the connection with this client should be aborted.</returns>
        public bool HandleFrame(WebSocketPackage package, WebSocketFrame frame)
        {
            if (frame == null)
                return false;

            // handle multipart/continuation
            if(frame.fin == false)
            {
                if (frame.opcode == WebSocketOpCode.CONTINUATION)
                {
                    multiparts.Add(frame.data);
                    return false;
                }
                else
                {
                    multipartHeader = frame;
                    multiparts.Clear();
                    return false;
                }
            }
            else if(frame.opcode == WebSocketOpCode.CONTINUATION)
            {
                // this is the last in the continuation sequence
                multiparts.Add(frame.data);
                frame = PopMultipart(); // merge all the data packets into one
                multipartHeader = null;

                if (debug)
                    Console.WriteLine("multipart: fin (see below)");
            }

            if (debug)
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
                string text = Encoding.UTF8.GetString(frame.data);

                // this is probably JSON
                if (text.StartsWith("{") && text.EndsWith("}"))
                {
                    JObject json = JObject.Parse(text);
                    return HandleJSON(package, json);
                }

                // unknown text
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Got unknown text?:\n\t{0}", text);
                Console.ForegroundColor = oldColor;
            }

            return false;
        }
        /// <summary>
        /// Handle an incoming JSON message from the client.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="json"></param>
        /// <returns>If the connection with this client should be aborted.</returns>
        public bool HandleJSON(WebSocketPackage package, JObject json)
        {
            if (!json.TryGetValue("action", out JToken value))
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received JSON with no action property:\n{0}", json.ToString());
                Console.ForegroundColor = color;
                return false;
            }

            string action = value.Value<string>();

            if (action.Equals("ping"))
            {
                var response = CreateNotificationFrame("Pong! -MCCCompiled", "white");
                package.SendFrame(response);
                return false;
            }
            if (action.Equals("debug")) // Deprecated, use project properties
            {
                bool enable = json["debug"].Value<bool>();
                this.debug = enable;
                Program.DEBUG = enable;

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
            if(action.Equals("property"))
            {
                string propertyName = json["name"].ToString().Base64Decode();
                string propertyValue = json["value"].ToString().Base64Decode();
                project.SetProperty(propertyName, propertyValue);
            }

            if (action.Equals("close"))
                return true;
            if(action.Equals("info"))
            {
                JObject info = new JObject();
                info["action"] = "menu";
                info["html"] = CreateGenericMenu("Server Info",
                    "Language Server Version: " + STANDARD_VERSION,
                    "MCCompiled Version: " + Executor.MCC_VERSION,
                    "Made for Minecraft Version: " + Executor.MINECRAFT_VERSION,
                    "",
                    "Fakeplayer Name: " + Executor.FAKEPLAYER_NAME,
                    "Maximum Code Depth: " + Executor.MAXIMUM_DEPTH)
                    .Base64Encode();

                WebSocketFrame frame = WebSocketFrame.JSON(info);
                package.SendFrame(frame);
                return false;
            }
            if (action.Equals("openfolder"))
            {
                string folder = json["folder"].ToString();
                string toOpen = null;

                switch (folder)
                {
                    case "current":
                        toOpen = Directory.GetCurrentDirectory();
                        break;
                    case "user":
                        toOpen = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        break;
                    case "bp":
                        toOpen = outputBehaviorPack.Replace("?project_BP", ""); // no project
                        break;
                    case "rp":
                        toOpen = outputResourcePack.Replace("?project_RP", ""); // no project
                        break;
                    case "install":
                        toOpen = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        break;
                    default:
                        break;
                }

                if (debug)
                    Console.WriteLine("Opening folder: {0}", toOpen);

                Process.Start("explorer.exe", '"' + toOpen + '"');
                return false;
            }

            // Code Actions
            if (action.Equals("lint"))
            {
                string encoded = json["code"].Value<string>();
                string code = encoded.Base64Decode();

                if (debug)
                    Console.WriteLine("Linting...");

                Lint(code, package);
                return false;
            }
            if(action.Equals("compile"))
            {
                string encodedCode = json["code"].Value<string>();
                string code = encodedCode.Base64Decode();
                string encodedProject = json["project"].Value<string>();
                string project = encodedProject.Base64Decode();

                if (debug)
                    Console.WriteLine("Compiling project '{0}'...", project);

                Compile(code, project, package);
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
                Thread thread = new Thread(() =>
                {
                    string encodedCode = json["code"].Value<string>();
                    string code = encodedCode.Base64Decode();
                    JObject metadata = json["meta"] as JObject;

                    if (!project.hasFile)
                    {
                        if (!project.RunSaveFileDialog())
                        {
                            package.SendFrame(CreateNotificationFrame("Save cancelled by user.", "#DDDDDD"));
                            package.SendFrame(CreateBusyFrame(false));
                            return; // stop thread
                        }
                    }

                    string file = project.File;
                    string fileName = Path.GetFileName(file);

                    if (System.IO.File.Exists(file))
                        System.IO.File.Delete(file);

                    using (var stream = System.IO.File.OpenWrite(file))
                    {
                        bool hasMetadata = metadata != null;
                        bool hasProperties = project.properties.Any();

                        if(hasMetadata || hasProperties)
                        {
                            // get bytes for the metadata header and footer
                            byte[] header = Encoding.UTF8.GetBytes(META_HEADER + '\n');
                            byte[] footer = Encoding.UTF8.GetBytes(META_FOOTER + '\n');

                            // start by writing the header
                            stream.Write(header, 0, header.Length);

                            if (hasMetadata)
                            {
                                string block = metadata.ToString(Newtonsoft.Json.Formatting.None).Base64Encode();
                                string blockString = META_FIELD_METADATA + block + '\n';
                                byte[] blockBytes = Encoding.UTF8.GetBytes(blockString);
                                stream.Write(blockBytes, 0, blockBytes.Length);
                            }
                            if(hasProperties)
                            {
                                string block = project.PropertiesBase64;
                                string blockString = META_FIELD_PROPERTIES + block + '\n';
                                byte[] blockBytes = Encoding.UTF8.GetBytes(blockString);
                                stream.Write(blockBytes, 0, blockBytes.Length);
                            }

                            // write footer now.
                            stream.Write(footer, 0, footer.Length);
                        }

                        byte[] bytes = Encoding.UTF8.GetBytes(code);
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                    }

                    package.SendFrame(CreateNotificationFrame($"File \"{fileName}\" saved.", "#3cc741"));
                    package.SendFrame(CreateBusyFrame(false));
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                return false;
            }
            if (action.Equals("load"))
            {
                Thread thread = new Thread(() =>
                {
                    if (!project.RunLoadFileDialog())
                    {
                        package.SendFrame(CreateNotificationFrame("Load canceled by user.", "#DDDDDD"));
                        package.SendFrame(CreateBusyFrame(false));
                        return;
                    }

                    string file = project.File;

                    if (System.IO.File.Exists(file))
                    {
                        string code = System.IO.File.ReadAllText(file);
                        JObject metadata = new JObject();

                        if(code.StartsWith(META_HEADER))
                        {
                            // begin reading metadata.
                            StringReader reader = new StringReader(code);
                            _ = reader.ReadLine(); // skip the header
                            code = "";

                            // read until encountering a footer.
                            string line;
                            while((line = reader.ReadLine()) != META_FOOTER)
                            {
                                if(line.StartsWith(META_FIELD_METADATA))
                                {
                                    string dataBlock = line.Substring(META_FIELD_METADATA.Length);
                                    metadata = JObject.Parse(dataBlock.Base64Decode());
                                } else if(line.StartsWith(META_FIELD_PROPERTIES))
                                {
                                    string dataBlock = line.Substring(META_FIELD_PROPERTIES.Length);
                                    project.PropertiesBase64 = dataBlock;
                                } else
                                {
                                    var oldColor = Console.ForegroundColor;
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"Malformed metadata in file \"{file}\". Treating as beginning of code.");
                                    Console.ForegroundColor = oldColor;
                                    code = line + '\n';
                                    break;
                                }
                            }

                            // the code is the rest of the segment without the metadata.
                            code += reader.ReadToEnd();

                            reader.Close();
                            reader.Dispose();
                        }

                        JObject send = new JObject()
                        {
                            ["action"] = "postload",
                            ["code"] = code.Base64Encode(),
                            ["meta"] = metadata
                        };
                        package.SendFrame(WebSocketFrame.JSON(send));


                        JArray _properties = new JArray(
                            project.properties.Select(kv => new JObject()
                            {
                                ["name"] = kv.Key.Base64Encode(),
                                ["value"] = kv.Value.Base64Encode()
                            })
                        );
                        JObject properties = new JObject()
                        {
                            ["action"] = "properties",
                            ["properties"] = _properties
                        };
                        package.SendFrame(WebSocketFrame.JSON(properties));

                        Lint(code, package);

                        if (debug)
                            Console.WriteLine("\nGot metadata from load:\n{0}\n", metadata.ToString());
                        return;
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                return false;
            }

            return false;
        }

        void Lint(string code, WebSocketPackage package)
        {
            Executor executor = null;
            try
            {
                Program.DEBUG = debug;
                Program.PrepareToCompile();
                Token[] tokens = new Tokenizer(code).Tokenize();
                Statement[] statements = Assembler.AssembleTokens(tokens);
                executor = new Executor(statements, Array.Empty<Program.InputPPV>(), "lint", outputBehaviorPack,
                    outputResourcePack);
                executor.Linter();
                executor.Execute();

                // gather information.
                LintStructure lint = LintStructure.Harvest(executor);

                if (debug)
                    Console.WriteLine("\tLint success. " + lint.ToString());

                string json = lint.ToJSON();
                package.SendFrame(WebSocketFrame.String(json));

                // tell the client that there are no more errors
                package.SendFrame(WebSocketFrame.String(
                    @"{""action"":""seterrors"",""errors"":[]}"
                ));

                executor.Cleanup(); // free executor resources now
            }
            catch (TokenizerException exc)
            {
                if (debug)
                    Console.WriteLine("\tError. " + exc.Message);
                string json = ErrorStructure.Wrap(exc).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
            }
            catch (StatementException exc)
            {
                if (debug)
                    Console.WriteLine("\tError. " + exc.Message);
                string json = ErrorStructure.Wrap(exc).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
            }
            catch (FeederException exc)
            {
                if (debug)
                    Console.WriteLine("\tError. " + exc.Message);
                string json = ErrorStructure.Wrap(exc).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
            }
            catch (Exception exc)
            {
                if (Debugger.IsAttached)
                    throw;
                if (debug)
                {
                    Console.WriteLine("\tFatal Error:\n\n" + exc.ToString());
                    Console.WriteLine(exc.ToString());
                }
                string json = ErrorStructure.Wrap(exc, new[] { 0 }).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
            } finally
            {
                // no longer busy
                package.SendFrame(CreateBusyFrame(false));
            }
        }
        void Compile(string code, string projectName, WebSocketPackage package)
        {
            Program.DEBUG = debug;
            Program.PrepareToCompile();
            bool success = Program.RunMCCompiledCode(code, projectName + ".mcc", Array.Empty<Program.InputPPV>(), outputBehaviorPack, outputResourcePack, projectName);

            if (debug)
                Console.WriteLine("Compilation Success: {0}", success);

            if (success)
                package.SendFrame(CreateNotificationFrame("Compilation Completed", "#3cc741"));
            else
                package.SendFrame(CreateNotificationFrame("Compilation Failed", "#db4b35"));

            // signal not busy anymore
            package.SendFrame(CreateBusyFrame(false));
        }

        /// <summary>
        /// Creates a WebSocketFrame to hold a MCCompiled protocol 'notification' action.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static WebSocketFrame CreateNotificationFrame(string text, string color)
        {
            JObject json = new JObject();
            json["action"] = "notification";
            json["text"] = text.Base64Encode();
            json["color"] = color;
            return WebSocketFrame.JSON(json);
        }
        /// <summary>
        /// Creates a WebSocketFrame to hold a MCCompiled protocol 'busy' action.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static WebSocketFrame CreateBusyFrame(bool busy)
        {
            JObject json = new JObject();
            json["action"] = "busy";
            json["busy"] = busy;
            return WebSocketFrame.JSON(json);
        }
        /// <summary>
        /// Creates HTML for a generic menu with a close button.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string CreateGenericMenu(string title, params string[] lines)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<h1>" + title + "</h1>");
            sb.Append("<h2 style=max-width:400px>");
            foreach (string line in lines)
            {
                if(!string.IsNullOrEmpty(line))
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
    internal struct STAPackage
    {
        internal readonly WebSocketPackage package;
        internal readonly MCCServerProject project;

        internal STAPackage(WebSocketPackage package, MCCServerProject project)
        {
            this.package = package;
            this.project = project;
        }
    }
}
