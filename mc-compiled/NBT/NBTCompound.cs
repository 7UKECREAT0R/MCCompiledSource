using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTCompound : NBTNode
    {
        public NBTNode[] values;

        public NBTCompound() => tagType = TAG.Compound;

        public override void Write(BinaryWriter writer)
        {
            if (values == null || values.Length < 1)
                throw new ArgumentException("No contents in NBTCompound.");

            FileWriterNBT.WriteToExisting(values, writer);
        }
    }
}
