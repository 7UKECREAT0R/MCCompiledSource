using System.IO;

namespace mc_compiled.NBT;

public class NBTList : NBTNode
{
    public TAG listType;
    public NBTNode[] values;

    public NBTList()
    {
        this.tagType = TAG.List;
    }

    public override void Write(BinaryWriter writer)
    {
        writer.Write((byte) this.listType);
        writer.Write(this.values.Length);
        foreach (NBTNode node in this.values)
            node.Write(writer);
    }
}