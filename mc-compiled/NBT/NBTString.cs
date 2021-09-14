using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTString : NBTNode
    {
        public string value;

        public NBTString() => tagType = TAG.String;

        public override void Write(BinaryWriter writer)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            ushort len = (ushort)bytes.Length;

            writer.Write(len);
            writer.Write(bytes);
        }
    }
}
