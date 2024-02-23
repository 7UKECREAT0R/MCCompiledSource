using System.IO;

namespace mc_compiled.NBT
{
    public class NBTByte : NBTNode
    {
        public byte value;

        public NBTByte() => this.tagType = TAG.Byte;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(this.value);
        }
    }
}
