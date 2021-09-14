using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTFloat : NBTNode
    {
        public float value;

        public NBTFloat() => tagType = TAG.Float;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
