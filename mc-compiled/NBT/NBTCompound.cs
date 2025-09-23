using System;
using System.IO;

namespace mc_compiled.NBT;

public class NBTCompound : NBTNode
{
    public NBTNode[] values;

    public NBTCompound() { this.tagType = TAG.Compound; }

    public override void Write(BinaryWriter writer)
    {
        if (this.values == null || this.values.Length < 1)
            throw new ArgumentException("No contents in NBTCompound.");
        if (this.values[^1].tagType != TAG.End)
            throw new ArgumentException("NBTCompound does not end with an End tag.");

        FileWriterNBT.WriteToExisting(this.values, writer);
    }
}