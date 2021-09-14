using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTLong : NBTNode
    {
        public long value;

        public NBTLong() => tagType = TAG.Long;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
