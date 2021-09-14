using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTInt : NBTNode
    {
        public int value;

        public NBTInt() => tagType = TAG.Int;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
