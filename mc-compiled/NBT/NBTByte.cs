using System.IO;

namespace mc_compiled.NBT;

public class NBTByte : NBTNode
{
    public byte value;

    public NBTByte() { this.tagType = TAG.Byte; }

    public override void Write(BinaryWriter writer) { writer.Write(this.value); }

    protected bool Equals(NBTByte other) { return this.value == other.value; }
    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((NBTByte) obj);
    }
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() { return this.value.GetHashCode(); }
}