using System.IO;

namespace mc_compiled.NBT
{
    /// <summary>
    /// Exports to the proper Minecraft color format used in-game.
    /// </summary>
    public class NBTColor : NBTNode
    {
        public byte a = 0xFF;
        public byte r;
        public byte g;
        public byte b;

        public NBTColor() => tagType = TAG.Int;
        public NBTColor(uint decode)
        {
            tagType = TAG.Int;
            a = (byte)((decode & 0xFF000000) >> 24);
            r = (byte)((decode & 0x00FF0000) >> 16);
            g = (byte)((decode & 0x0000FF00) >> 8);
            b = (byte)(decode & 0x000000FF);
        }
        public override void Write(BinaryWriter writer)
        {
            uint value = 0;
            value |= (((uint)a) << 24);
            value |= (((uint)r) << 16);
            value |= (((uint)g) << 8);
            value |= b;
            writer.Write(value);
        }
    }
}
