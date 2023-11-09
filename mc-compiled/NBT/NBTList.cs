using System.IO;

namespace mc_compiled.NBT
{
    public class NBTList : NBTNode
    {
        public TAG listType;
        public NBTNode[] values;

        public NBTList() => tagType = TAG.List;

        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte)listType);
            writer.Write(values.Length);
            foreach (NBTNode node in values)
                node.Write(writer);
        }
    }
}
