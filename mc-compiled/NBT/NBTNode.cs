using System.IO;

namespace mc_compiled.NBT
{
    /// <summary>
    /// A node in an NBT data structure.
    /// </summary>
    public abstract class NBTNode
    {
        public TAG tagType = TAG.End;
        public string name = null;
        public abstract void Write(BinaryWriter writer);
    }
    public enum TAG : byte
    {
        End = 0,
        Byte = 1,
        Short = 2,
        Int = 3,
        Long = 4,
        Float = 5,
        Double = 6,
        ByteArray = 7,
        String = 8,
        List = 9,
        Compound = 10,
        IntArray = 11,
        LongArray = 12
    }
}
