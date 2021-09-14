using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTLongArray : NBTNode
    {
        public long[] values;

        public NBTLongArray() => tagType = TAG.LongArray;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(values.Length);
            foreach (var value in values)
                writer.Write(value);
        }
    }
}
