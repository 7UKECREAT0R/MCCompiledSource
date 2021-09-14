using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTDouble : NBTNode
    {
        public double value;

        public NBTDouble() => tagType = TAG.Double;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
