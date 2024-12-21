using System.IO;

namespace mc_compiled.NBT;

/// <summary>
///     Exports to the proper Minecraft color format used in-game.
/// </summary>
public class NBTColor : NBTNode
{
    public byte a = 0xFF;
    public byte b;
    public byte g;
    public byte r;

    public NBTColor()
    {
        this.tagType = TAG.Int;
    }
    public NBTColor(uint decode)
    {
        this.tagType = TAG.Int;
        this.a = (byte) ((decode & 0xFF000000) >> 24);
        this.r = (byte) ((decode & 0x00FF0000) >> 16);
        this.g = (byte) ((decode & 0x0000FF00) >> 8);
        this.b = (byte) (decode & 0x000000FF);
    }
    public override void Write(BinaryWriter writer)
    {
        uint value = 0;
        value |= (uint) this.a << 24;
        value |= (uint) this.r << 16;
        value |= (uint) this.g << 8;
        value |= this.b;
        writer.Write(value);
    }
}