using System.IO;

namespace mc_compiled.NBT
{
    public class NBTLongArray : NBTNode
    {
        public long[] values;

        public NBTLongArray() => tagType = TAG.LongArray;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(values.Length);
            foreach (var value in values)
                writer.Write(value);
        }
    }
}
