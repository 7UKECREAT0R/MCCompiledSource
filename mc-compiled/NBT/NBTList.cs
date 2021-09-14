using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
