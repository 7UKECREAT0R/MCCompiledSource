using System.IO;

namespace mc_compiled.NBT
{
    public class NBTFloat : NBTNode
    {
        public float value;

        public NBTFloat() => tagType = TAG.Float;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
