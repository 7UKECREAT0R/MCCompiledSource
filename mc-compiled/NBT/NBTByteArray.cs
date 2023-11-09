using System.IO;

namespace mc_compiled.NBT
{
    public class NBTByteArray : NBTNode
    {
        public byte[] values;

        public NBTByteArray() => tagType = TAG.ByteArray;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(values.Length);
            foreach(var value in values)
                writer.Write(value);
        }
    }
}
