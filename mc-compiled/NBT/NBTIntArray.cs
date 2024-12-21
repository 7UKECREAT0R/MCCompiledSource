using System.IO;

namespace mc_compiled.NBT;

public class NBTIntArray : NBTNode
{
    public int[] values;

    public NBTIntArray()
    {
        this.tagType = TAG.IntArray;
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(this.values.Length);
        foreach (int value in this.values)
            writer.Write(value);
    }
}