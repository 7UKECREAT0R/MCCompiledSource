using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Server
{
    /// <summary>
    /// Represents a WebSocket structure passed into a thread for networking.
    /// Contains useful methods for managing the state in a thread-safe way.
    /// </summary>
    public class WebSocketPackage : IDisposable
    {
        public readonly bool debug;
        public readonly Socket server;
        public readonly Socket client;
        public readonly AsyncCallback receiveCallback;
        public bool didHandshake = false;
        public byte[] buffer;
        public List<WebSocketFrame> cache;
        internal readonly MCCServerProject project;

        /// <summary>
        /// Creates a new ServerAsyncPackage with a new byte buffer.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        public WebSocketPackage(bool debug, Socket server, Socket client, MCCServer mcc, AsyncCallback receiveCallback)
        {
            this.debug = debug;
            this.server = server;
            this.client = client;
            this.buffer = new byte[MCCServer.CHUNK_SIZE];
            this.cache = new List<WebSocketFrame>(8);
            this.receiveCallback = receiveCallback;

            this.project = new MCCServerProject(mcc);
        }

        /// <summary>
        /// Pulls and merges all frames in the cache.
        /// </summary>
        /// <returns></returns>
        public WebSocketFrame PullMergeCache()
        {
            if (cache.Count == 0)
                return null;
            if (cache.Count == 1)
            {
                var item = cache[0];
                cache.Clear();
                return item;
            }

            long length = cache.Sum(c => c.data.LongLength);
            byte[] newArray = new byte[length];

            long offset = 0;
            WebSocketOpCode opcode = 0;

            foreach(var frame in cache)
            {
                frame.data.CopyTo(newArray, offset);
                offset += frame.data.LongLength;
                opcode = frame.opcode;
            }

            cache.Clear();
            return new WebSocketFrame(opcode, newArray);
        }

        public void BeginReceive() =>
            client.BeginReceive(buffer, 0, MCCServer.CHUNK_SIZE, SocketFlags.None, receiveCallback, this);
        public int EndReceive(IAsyncResult result) =>
            client.EndReceive(result);

        /// <summary>
        /// Reads an ASCII string from the buffer with the specified number of bytes.
        /// </summary>
        /// <returns></returns>
        public string ReadStringASCII(int bytes) =>
            Encoding.ASCII.GetString(buffer, 0, bytes);
        /// <summary>
        /// Sends an ASCII string to the client.
        /// </summary>
        /// <param name="str"></param>
        public void SendStringASCII(string str)
        {
            if (debug)
                Console.WriteLine("Sending string '{0}'", str);

            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(str);
                client.Send(bytes);
            }
            catch (SocketException)
            {
                // terminate connection, client died unexpectedly
                Close(true);
            }
        }

        /// <summary>
        /// Send a raw WebSocketFrame to the client.
        /// </summary>
        /// <param name="frame"></param>
        public void SendFrame(WebSocketFrame frame)
        {
            if (debug)
                Console.WriteLine("Sending frame {0}", frame.ToString());

            try
            {
                byte[] bytes = frame.GetBytes();
                client.Send(bytes);
            } catch(SocketException)
            {
                // terminate connection, client died unexpectedly
                Close(true);
            }
        }

        bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed)
                return;

            client?.Dispose();
            this.buffer = null;

            _isDisposed = true;
        }
        /// <summary>
        /// Close the connection associated with this package and dispose this instance.
        /// </summary>
        /// <param name="clientTerminated">If the client is already gone, so a CLOSE packet cannot be sent to it.</param>
        public void Close(bool clientTerminated)
        {
            if (server == null)
                return;
            if (client == null)
                return;

            if (client != null)
            {
                if(!clientTerminated)
                    SendFrame(WebSocketFrame.Close());
                client.Close();
            }
            Dispose();
        }
    }
}
