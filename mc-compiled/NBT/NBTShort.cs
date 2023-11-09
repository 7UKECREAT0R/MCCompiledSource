using System.IO;

namespace mc_compiled.NBT
{
    public class NBTShort : NBTNode
    {
        public short value;

        public NBTShort() => tagType = TAG.Short;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
