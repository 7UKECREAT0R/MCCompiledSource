using System.IO;

namespace mc_compiled.NBT;

public class NBTLong : NBTNode
{
    public long value;

    public NBTLong()
    {
        this.tagType = TAG.Long;
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(this.value);
    }
}