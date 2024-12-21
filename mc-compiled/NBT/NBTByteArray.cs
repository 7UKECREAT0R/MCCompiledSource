using System.IO;

namespace mc_compiled.NBT;

public class NBTByteArray : NBTNode
{
    public byte[] values;

    public NBTByteArray()
    {
        this.tagType = TAG.ByteArray;
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(this.values.Length);
        foreach (byte value in this.values)
            writer.Write(value);
    }
}