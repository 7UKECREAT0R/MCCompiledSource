using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTByteArray : NBTNode
    {
        public byte[] values;

        public NBTByteArray() => tagType = TAG.ByteArray;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(values.Length);
            foreach(var value in values)
                writer.Write(value);
        }
    }
}
