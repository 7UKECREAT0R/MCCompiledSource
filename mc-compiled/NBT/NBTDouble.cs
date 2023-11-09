using System.IO;

namespace mc_compiled.NBT
{
    public class NBTDouble : NBTNode
    {
        public double value;

        public NBTDouble() => tagType = TAG.Double;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
