using System.IO;

namespace mc_compiled.NBT
{
    public class NBTInt : NBTNode
    {
        public int value;

        public NBTInt() => tagType = TAG.Int;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
