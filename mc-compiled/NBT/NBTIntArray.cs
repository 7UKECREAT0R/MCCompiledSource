using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTIntArray : NBTNode
    {
        public int[] values;

        public NBTIntArray() => tagType = TAG.IntArray;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(values.Length);
            foreach (var value in values)
                writer.Write(value);
        }
    }
}
