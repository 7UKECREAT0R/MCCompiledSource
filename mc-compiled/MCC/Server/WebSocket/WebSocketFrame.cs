﻿using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.ServerWebSocket;

/// <summary>
///     Represents a WebSocket data frame (RFC-6455).
/// </summary>
public class WebSocketFrame
{
    public byte[] data;
    public bool fin;
    public long length;

    public byte[] mask;
    public WebSocketOpCode opcode;

    public WebSocketFrame(WebSocketOpCode opcode, byte[] data, bool fin, byte[] mask)
    {
        this.opcode = opcode;
        this.length = data.Length;
        this.data = data;
        this.fin = fin;
        this.mask = mask;
    }
    public WebSocketFrame(WebSocketOpCode opcode, long length, bool fin, byte[] mask)
    {
        this.opcode = opcode;
        this.length = length;
        this.fin = fin;
        this.mask = mask;
    }
    public WebSocketFrame AsContinuation(bool fin)
    {
        this.opcode = WebSocketOpCode.CONTINUATION;
        this.fin = fin;
        return this;
    }

    /// <summary>
    ///     Creates a WebSocketFrame from a complete frame payload.
    /// </summary>
    /// <param name="frame"></param>
    public static WebSocketFrame FromFrame(byte[] frame)
    {
        WebSocketByte0Info byte0Info = ParseByte0(frame[0]);
        WebSocketByte1Info byte1Info = ParseByte1(frame[1]);

        WebSocketOpCode opcode = byte0Info.opcode;

        int offset = 2;
        ulong messageLength = 0;

        if (byte1Info.extension == WebSocketPayloadLength.NORMAL)
        {
            messageLength = byte1Info.payloadLength;
        }
        else if (byte1Info.extension == WebSocketPayloadLength.EXTENDED)
        {
            // get 2 more bytes
            byte a = frame[offset++];
            byte b = frame[offset++];
            messageLength = BitConverter.ToUInt16([b, a], 0);
        }
        else if (byte1Info.extension == WebSocketPayloadLength.MASSIVE)
        {
            // get 8 more bytes
            byte a = frame[offset++];
            byte b = frame[offset++];
            byte c = frame[offset++];
            byte d = frame[offset++];
            byte e = frame[offset++];
            byte f = frame[offset++];
            byte g = frame[offset++];
            byte h = frame[offset++];
            messageLength = BitConverter.ToUInt64([h, g, f, e, d, c, b, a], 0);
        }

        // get/unmask the message data
        byte[] messageData = new byte[messageLength];

        if (byte1Info.mask)
        {
            // unmask the message.
            byte maskA = frame[offset++];
            byte maskB = frame[offset++];
            byte maskC = frame[offset++];
            byte maskD = frame[offset++];
            byte[] masks = [maskA, maskB, maskC, maskD];

            ulong longOffset = (ulong) offset;

            for (ulong i = 0; i < messageLength; i++)
                messageData[i] = (byte) (frame[i + longOffset] ^ masks[i % 4]);
        }
        else
        {
            // just do a bulk copy
            ulong longOffset = (ulong) offset;

            for (ulong i = 0; i < messageLength; i++)
                messageData[i] = frame[i + longOffset];
        }

        return new WebSocketFrame(opcode, messageData, byte0Info.fin, null);
    }
    /// <summary>
    ///     Creates a WebSocketFrame from a complete header but without the payload.
    /// </summary>
    public static WebSocketFrame FromFrameHeader(WebSocketByte0Info byte0Info, WebSocketByte1Info byte1Info,
        byte[] header, int startOffset = 2)
    {
        WebSocketOpCode opcode = byte0Info.opcode;

        int offset = startOffset;
        long messageLength = 0;

        if (byte1Info.extension == WebSocketPayloadLength.NORMAL)
        {
            messageLength = byte1Info.payloadLength;
        }
        else if (byte1Info.extension == WebSocketPayloadLength.EXTENDED)
        {
            // get 2 more bytes
            byte a = header[offset++];
            byte b = header[offset++];
            messageLength = BitConverter.ToUInt16([b, a], 0);
        }
        else if (byte1Info.extension == WebSocketPayloadLength.MASSIVE)
        {
            // get 8 more bytes
            byte a = header[offset++];
            byte b = header[offset++];
            byte c = header[offset++];
            byte d = header[offset++];
            byte e = header[offset++];
            byte f = header[offset++];
            byte g = header[offset++];
            byte h = header[offset++];
            messageLength = (long) BitConverter.ToUInt64([h, g, f, e, d, c, b, a], 0);
        }

        // get/unmask the message data
        byte[] masks = null;

        if (byte1Info.mask)
        {
            // get the mask
            byte maskA = header[offset++];
            byte maskB = header[offset++];
            byte maskC = header[offset++];
            byte maskD = header[offset];
            masks = [maskA, maskB, maskC, maskD];
        }

        return new WebSocketFrame(opcode, messageLength, byte0Info.fin, masks);
    }

    /// <summary>
    ///     Creates a WebSocketFrame that wraps a UTF-8 string.
    /// </summary>
    public static WebSocketFrame String(string text, bool fin = true)
    {
        byte[] data = Encoding.UTF8.GetBytes(text);
        return new WebSocketFrame(WebSocketOpCode.TEXT, data, fin, null);
    }
    /// <summary>
    ///     Creates a WebSocketFrame that wraps a JSON object.
    /// </summary>
    public static WebSocketFrame JSON(JObject json, bool fin = true)
    {
        string str = json.ToString(Formatting.None);
        byte[] data = Encoding.UTF8.GetBytes(str);
        return new WebSocketFrame(WebSocketOpCode.TEXT, data, fin, null);
    }
    /// <summary>
    ///     Creates a WebSocketFrame that initiates a close.
    /// </summary>
    public static WebSocketFrame Close(bool fin = true)
    {
        return new WebSocketFrame(WebSocketOpCode.CLOSE, [], fin, null);
    }
    /// <summary>
    ///     Creates a WebSocketFrame that serves as a response to a "Ping"
    /// </summary>
    public static WebSocketFrame Pong(bool fin = true)
    {
        return new WebSocketFrame(WebSocketOpCode.PONG, [], fin, null);
    }

    /// <summary>
    ///     Returns the bytes needed to represent this websocket frame (outbound to client).
    /// </summary>
    public byte[] GetBytes()
    {
        long dataLengthTotal = this.data.LongLength;
        var byte0Info = new WebSocketByte0Info(this.opcode, true);
        var byte1Info = new WebSocketByte1Info(false, DeterminePayloadLength(dataLengthTotal));

        if (byte1Info.extension == WebSocketPayloadLength.NORMAL)
            byte1Info.payloadLength = (byte) dataLengthTotal;

        byte byte0 = byte0Info.Encode();
        byte byte1 = byte1Info.Encode();

        ulong numBytesNeeded = (ulong) dataLengthTotal + 2L + (ulong) (int) byte1Info.extension;
        byte[] bytes = new byte[numBytesNeeded];

        bytes[0] = byte0;
        bytes[1] = byte1;

        bool littleEndian = BitConverter.IsLittleEndian;
        int step = littleEndian ? -1 : 1;
        int pull;
        int offset = 2;

        if (byte1Info.extension == WebSocketPayloadLength.EXTENDED)
        {
            byte[] converted = BitConverter.GetBytes((ushort) dataLengthTotal);
            pull = littleEndian ? 1 : 0;
            bytes[offset++] = converted[pull];
            pull += step;
            bytes[offset++] = converted[pull];
        }
        else if (byte1Info.extension == WebSocketPayloadLength.MASSIVE)
        {
            byte[] converted = BitConverter.GetBytes((ulong) dataLengthTotal);
            pull = littleEndian ? 7 : 0;

            // RFC 6455 § 5.2, "Payload Length"
            // Most significant bit must be 0.
            converted[pull] &= 0b01111111;

            bytes[offset++] = converted[pull];
            pull += step;
            bytes[offset++] = converted[pull];
            pull += step;
            bytes[offset++] = converted[pull];
            pull += step;
            bytes[offset++] = converted[pull];
            pull += step;
            bytes[offset++] = converted[pull];
            pull += step;
            bytes[offset++] = converted[pull];
            pull += step;
            bytes[offset++] = converted[pull];
            pull += step;
            bytes[offset++] = converted[pull];
        }

        Array.Copy(this.data, 0L, bytes, offset, dataLengthTotal);
        return bytes;
    }

    public void UnmaskData()
    {
        if (this.mask != null)
            for (long i = 0; i < this.data.LongLength; i++)
                this.data[i] = (byte) (this.data[i] ^ this.mask[i % 4]);
    }
    public byte[] UnmaskData(byte[] dataToUnmask)
    {
        if (this.mask != null)
            for (long i = 0; i < dataToUnmask.LongLength; i++)
                dataToUnmask[i] = (byte) (dataToUnmask[i] ^ this.mask[i % 4]);
        return dataToUnmask;
    }
    public void SetData(byte[] newData)
    {
        this.data = newData;
        UnmaskData();
    }

    /// <summary>
    ///     Parses the first byte in a WebSocket frame.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static WebSocketByte0Info ParseByte0(byte data)
    {
        const byte FIN_MASK = 0b10000000;
        const byte RSV_MASK = 0b01110000;
        const byte OPCODE_MASK = 0b00001111;

        if ((data & RSV_MASK) != 0)
            throw new Exception("WebSocket frame contained RSV bits.");

        bool fin = (data & FIN_MASK) == FIN_MASK;
        sbyte opcode = (sbyte) (data & OPCODE_MASK);

        var info = new WebSocketByte0Info
        {
            fin = fin,
            opcode = (WebSocketOpCode) opcode
        };

        if (!Enum.IsDefined(typeof(WebSocketOpCode), info.opcode))
            throw new Exception("Bad WebSocket frame opcode: " + info.opcode);

        return info;
    }
    /// <summary>
    ///     Parses the first byte in a WebSocket frame.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static WebSocketByte1Info ParseByte1(byte data)
    {
        const byte MASK_MASK = 0b10000000;
        const byte PAYLOAD_MASK = 0b01111111;
        bool mask = (data & MASK_MASK) == MASK_MASK;

        if (mask)
            data &= PAYLOAD_MASK;

        return new WebSocketByte1Info(mask, data);
    }
    /// <summary>
    ///     Returns the necessary length in order to store the payload size in a frame.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static WebSocketPayloadLength DeterminePayloadLength(long bytes)
    {
        if (bytes < 126)
            return WebSocketPayloadLength.NORMAL;
        if (bytes <= ushort.MaxValue)
            return WebSocketPayloadLength.EXTENDED;
        return WebSocketPayloadLength.MASSIVE;
    }

    public override string ToString()
    {
        return this.fin
            ? $"{{[FIN] {this.opcode}: len '{this.length}'}}"
            : $"{{[NOT FIN] {this.opcode}: len '{this.length}'}}";
    }
}

public enum WebSocketOpCode : sbyte
{
    CONTINUATION = 0x0,
    TEXT = 0x1,
    BINARY = 0x2,
    CLOSE = 0x8,
    PING = 0x9,
    PONG = 0xA
}

public enum WebSocketPayloadLength
{
    // int represents number of bytes needed to store the number
    NORMAL = 0,
    EXTENDED = 2,
    MASSIVE = 8
}

public struct WebSocketByte0Info
{
    public bool fin;
    public WebSocketOpCode opcode;

    internal WebSocketByte0Info(WebSocketOpCode opcode, bool fin)
    {
        this.fin = fin;
        this.opcode = opcode;
    }

    internal byte Encode()
    {
        byte final = this.fin ? (byte) 1 : (byte) 0;
        final <<= 7;
        final |= (byte) this.opcode;
        return final;
    }
}

public struct WebSocketByte1Info
{
    public bool mask;
    public byte payloadLength;

    public WebSocketPayloadLength extension;

    internal WebSocketByte1Info(bool mask, byte payloadLength)
    {
        this.mask = mask;
        this.payloadLength = payloadLength;

        if (payloadLength < 126)
            this.extension = WebSocketPayloadLength.NORMAL;
        else if (payloadLength == 126)
            this.extension = WebSocketPayloadLength.EXTENDED;
        else // payloadLength == 127
            this.extension = WebSocketPayloadLength.MASSIVE;
    }
    internal WebSocketByte1Info(bool mask, WebSocketPayloadLength extension)
    {
        this.mask = mask;
        this.extension = extension;

        this.payloadLength = extension switch
        {
            WebSocketPayloadLength.EXTENDED => 126,
            WebSocketPayloadLength.MASSIVE => 127,
            _ => 0
        };
    }

    internal byte Encode()
    {
        byte final = this.mask ? (byte) 1 : (byte) 0;
        final <<= 7;
        final |= this.payloadLength;
        return final;
    }
}