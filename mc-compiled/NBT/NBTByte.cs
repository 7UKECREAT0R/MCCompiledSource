using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTByte : NBTNode
    {
        public byte value;

        public NBTByte() => tagType = TAG.Byte;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}
