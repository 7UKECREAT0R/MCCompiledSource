using System.IO;

namespace mc_compiled.NBT;

public class NBTInt : NBTNode
{
    public int value;

    public NBTInt() { this.tagType = TAG.Int; }

    public override void Write(BinaryWriter writer) { writer.Write(this.value); }

    protected bool Equals(NBTInt other) { return this.value == other.value; }
    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((NBTInt) obj);
    }
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() { return this.value; }
}