using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTShort : NBTNode
    {
        public short value;

        public NBTShort() => tagType = TAG.Short;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
