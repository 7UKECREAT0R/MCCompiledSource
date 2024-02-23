using System.IO;

namespace mc_compiled.NBT
{
    public class NBTDouble : NBTNode
    {
        public double value;

        public NBTDouble() => this.tagType = TAG.Double;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(this.value);
        }
    }
}
