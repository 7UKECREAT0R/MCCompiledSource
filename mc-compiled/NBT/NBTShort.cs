using System.IO;

namespace mc_compiled.NBT;

public class NBTShort : NBTNode
{
    public short value;

    public NBTShort()
    {
        this.tagType = TAG.Short;
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write(this.value);
    }
}