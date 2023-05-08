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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using static mc_compiled.Commands.Selectors.Selector;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.LinkLabel;

namespace mc_compiled.MCC.Server
{
    /// <summary>
    /// MCCompiled Language Server
    /// Communicates with the webapp in order to extend functionality.
    /// - Gives code diagnostics
    /// - Compiles code into the user's filesystem.
    /// - Saves/loads data from the filesystem.
    /// - 
    /// </summary>
    public class MCCServer : IDisposable
    {
        public const int PORT = 11830;
        public const float VERSION = 5.7f;
        public const int CHUNK_SIZE = 0x100000; // 1MB
        public const int BUFFER_SIZE = 0x10000; // 64K

        public const string WEBSOCKET_MAGIC = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public static readonly Encoding ENCODING = Encoding.ASCII;

        private bool debug;
        private readonly MCCServerProject project;
        private readonly IPEndPoint ip;
        private readonly Socket socket;
        private readonly ManualResetEvent connectionEstablished;
        private readonly SHA1 sha1;

        private readonly string
            outputResourcePack,
            outputBehaviorPack;

        /// <summary>
        /// Creates a new MCCServer and sets up Socket for opening.
        /// </summary>
        /// <param name="outputResourcePack">The output location for the resource pack. Use '?project' to denote the name of the project.</param>
        /// <param name="outputBehaviorPack">The output location for the behavior pack. Use '?project' to denote the name of the project.</param>
        public MCCServer(string outputResourcePack, string outputBehaviorPack, bool debug)
        {
            this.outputResourcePack = outputResourcePack;
            this.outputBehaviorPack = outputBehaviorPack;
            this.debug = debug;
            this.connectionEstablished = new ManualResetEvent(false);

            this.project = new MCCServerProject(this);
            this.ip = new IPEndPoint(IPAddress.Loopback, PORT);
            this.socket = new Socket(ip.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            this.sha1 = SHA1CryptoServiceProvider.Create();
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
            Console.WriteLine("Language Server Version {0}", VERSION);
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
            Socket server = state as Socket;
            Socket client = server.EndAccept(result);

            // create a package for sending to the read thread
            WebSocketPackage package = new WebSocketPackage
                (debug, server, client, this, new AsyncCallback(OnReceiveTCP));

            // begin receiving a packet
            package.BeginReceive();
        }
        /// <summary>
        /// Called when an open connection is sent a TCP packet to be processed.
        /// </summary>
        /// <param name="result"></param>
        public void OnReceiveTCP(IAsyncResult result)
        {
            WebSocketPackage package = result.AsyncState as WebSocketPackage;

            int bytesRead = package.EndReceive(result);

            // only process if bytes were read.
            if (bytesRead > 0)
            {
                string str = package.ReadStringASCII(bytesRead);

                // if the handshake has been done, process this as a regular WebSocket frame.
                if (package.didHandshake)
                {
                    var info = WebSocketFrame.ParseByte0(package.buffer[0]);

                    // construct frame from the data
                    WebSocketFrame frame = WebSocketFrame.FromFrame(package.buffer);
                    
                    // if the frame is incomplete, pass it to the cache.
                    if(info.fin)
                    {
                        package.cache.Add(frame);
                        frame = package.PullMergeCache();

                        bool close = HandleFrame(package, frame);

                        // close the connection with the client.
                        if(close)
                        {
                            package.Close(false);
                            return;
                        }
                    } else
                        package.cache.Add(frame);
                }

                // the only HTTP used by WebSocket is when initiating the handshake.
                if (str.StartsWith("GET"))
                    ProcessWebsocketUpgrade(package, str);
            }

            // start looking for next data chunk
            package.BeginReceive();
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
            if (!value.Equals("Upgrade"))
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

            string responseData = http.ToHTTP(HANDSHAKE_HEADER);
            package.SendStringASCII(responseData);
            package.didHandshake = true;

            // send version info
            JObject json = new JObject();
            json["action"] = "version";
            json["version"] = (int)(Executor.MCC_VERSION * 1000);
            package.SendFrame(WebSocketFrame.JSON(json));
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

            if (debug)
                Console.WriteLine("Received {0}:\n\t{1}", frame.opcode, Encoding.UTF8.GetString(frame.data));

            if (frame.opcode == WebSocketOpCode.CLOSE)
            {
                WebSocketFrame response = WebSocketFrame.Close();
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
            if (action.Equals("debug"))
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
            if (action.Equals("close"))
                return true;
            if(action.Equals("info"))
            {
                JObject info = new JObject();
                info["action"] = "menu";
                info["html"] = CreateGenericMenu("Server Info",
                    "Language Server Version: " + VERSION,
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
                        toOpen = outputBehaviorPack.Replace("?project", ""); // no project
                        break;
                    case "rp":
                        toOpen = outputResourcePack.Replace("?project", ""); // no project
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
            const string META = "// $meta ";

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
                        // write metadata, if any
                        if(metadata != null)
                        {
                            // get bytes for the metadata header and footer
                            byte[] header = Encoding.UTF8.GetBytes(META_HEADER + '\n');

                            // start by writing the header
                            stream.Write(header, 0, header.Length);

                            // write the data block
                            string block = metadata.ToString(Newtonsoft.Json.Formatting.None).Base64Encode();
                            string blockString = META + block + '\n';
                            byte[] blockBytes = Encoding.UTF8.GetBytes(blockString);
                            stream.Write(blockBytes, 0, blockBytes.Length);
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

                            string line = reader.ReadLine();

                            // if we've reached the footer, break out of the loop.
                            if (!line.StartsWith(META))
                            {
                                var oldColor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Malformed metadata (garbage field) in file \"{file}\". Skipping.");
                                Console.ForegroundColor = oldColor;
                            } else
                            {
                                string dataBlock = line.Substring(META.Length);
                                metadata = JObject.Parse(dataBlock.Base64Decode());

                                // the code is the rest of the segment without the metadata.
                                code = reader.ReadToEnd();
                            }

                            reader.Close();
                            reader.Dispose();
                        }

                        JObject send = new JObject();
                        send["action"] = "postload";
                        send["code"] = code.Base64Encode();
                        send["meta"] = metadata;
                        package.SendFrame(WebSocketFrame.JSON(send));

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
                executor = new Executor(statements, new Program.InputPPV[0], "lint", outputBehaviorPack, outputResourcePack);
                executor.Linter().Execute();

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
                return;
            }
            catch (StatementException exc)
            {
                if (debug)
                    Console.WriteLine("\tError. " + exc.Message);
                string json = ErrorStructure.Wrap(exc).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
                return;
            }
            catch (FeederException exc)
            {
                if (debug)
                    Console.WriteLine("\tError. " + exc.Message);
                string json = ErrorStructure.Wrap(exc).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
                return;
            }
            catch (Exception exc)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    throw;
                if (debug)
                {
                    Console.WriteLine("\tFatal Error:\n\n" + exc.ToString());
                    Console.WriteLine(exc.ToString());
                }
                string json = ErrorStructure.Wrap(exc, new[] { 0 }).ToJSON();
                package.SendFrame(WebSocketFrame.String(json));
                return;
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
            bool success = Program.RunMCCompiledCode(code, projectName + ".mcc", new Program.InputPPV[0], outputBehaviorPack, outputResourcePack, projectName);

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
