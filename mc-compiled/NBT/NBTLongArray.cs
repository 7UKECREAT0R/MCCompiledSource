using System.IO;

namespace mc_compiled.NBT;

public class NBTLongArray : NBTNode
{
    public long[] values;

    public NBTLongArray()
    {
        this.tagType = TAG.LongArray;
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(this.values.Length);
        foreach (long value in this.values)
            writer.Write(value);
    }
}