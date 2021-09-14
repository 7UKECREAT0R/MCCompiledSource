using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public class NBTEnd : NBTNode
    {
        public NBTEnd() => tagType = TAG.End;

        public override void Write(BinaryWriter writer) {}
    }
}
