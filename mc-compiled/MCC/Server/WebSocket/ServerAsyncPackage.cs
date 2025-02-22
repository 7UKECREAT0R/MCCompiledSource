﻿using System;
using System.Net.Sockets;
using System.Text;

namespace mc_compiled.MCC.ServerWebSocket;

/// <summary>
///     Represents a WebSocket structure passed into a thread for networking.
///     Contains useful methods for managing the state in a thread-safe way.
/// </summary>
public class WebSocketPackage : IDisposable
{
    public readonly Socket client;
    internal readonly MCCServerProject project;
    public readonly Socket server;

    private bool _isDisposed;
    public byte[] buffer;
    public bool didHandshake = false;

    /// <summary>
    ///     Represents a WebSocket structure passed into a thread for networking.
    ///     Contains useful methods for managing the state in a thread-safe way.
    /// </summary>
    /// <param name="server">The server socket instance.</param>
    /// <param name="client">The client socket instance.</param>
    /// <param name="mcc">The MCCServer instance managing the server.</param>
    public WebSocketPackage(Socket server, Socket client, MCCServer mcc)
    {
        this.server = server;
        this.client = client;
        this.buffer = new byte[MCCServer.CHUNK_SIZE];

        this.project = new MCCServerProject(mcc);
    }
    public void Dispose()
    {
        if (this._isDisposed)
            return;

        this.client?.Dispose();
        this.buffer = null;

        this._isDisposed = true;
    }

    /// <summary>
    ///     Reads an ASCII string from the buffer with the specified number of bytes.
    /// </summary>
    /// <returns></returns>
    public string ReadStringASCII(int bytes) { return Encoding.ASCII.GetString(this.buffer, 0, bytes); }
    /// <summary>
    ///     Sends an ASCII string to the client.
    /// </summary>
    /// <param name="str"></param>
    public void SendStringASCII(string str)
    {
        if (GlobalContext.Debug)
            Console.WriteLine("Sending string '{0}'", str);

        try
        {
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            this.client.Send(bytes);
        }
        catch (SocketException)
        {
            // terminate connection, client died unexpectedly
            Close(true);
        }
    }

    /// <summary>
    ///     Send a raw WebSocketFrame to the client.
    /// </summary>
    /// <param name="frame"></param>
    public void SendFrame(WebSocketFrame frame)
    {
        if (GlobalContext.Debug)
            Console.WriteLine("Sending frame {0}", frame);

        if (this._isDisposed || this.client == null)
        {
            Console.WriteLine("Socket closed; Cancelled sending frame.");
            return;
        }

        try
        {
            byte[] bytes = frame.GetBytes();
            this.client.Send(bytes);
        }
        catch (SocketException)
        {
            // terminate connection, client died unexpectedly
            Close(true);
        }
    }
    /// <summary>
    ///     Close the connection associated with this package and dispose this instance.
    /// </summary>
    /// <param name="clientTerminated">If the client is already gone, so a CLOSE packet cannot be sent to it.</param>
    public void Close(bool clientTerminated)
    {
        if (this.server == null)
            return;
        if (this.client == null)
            return;

        if (this.client != null)
        {
            if (!clientTerminated)
                SendFrame(WebSocketFrame.Close());
            this.client.Close();
        }

        Dispose();
    }
}