using System.IO;

namespace mc_compiled.NBT
{
    public class NBTEnd : NBTNode
    {
        public NBTEnd() => this.tagType = TAG.End;

        public override void Write(BinaryWriter writer) {}
    }
}
