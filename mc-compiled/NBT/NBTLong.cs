using System.IO;

namespace mc_compiled.NBT
{
    public class NBTLong : NBTNode
    {
        public long value;

        public NBTLong() => tagType = TAG.Long;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
