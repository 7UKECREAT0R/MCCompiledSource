using System.IO;

namespace mc_compiled.NBT
{
    public class NBTIntArray : NBTNode
    {
        public int[] values;

        public NBTIntArray() => tagType = TAG.IntArray;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(values.Length);
            foreach (var value in values)
                writer.Write(value);
        }
    }
}
