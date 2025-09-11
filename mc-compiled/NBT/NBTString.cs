using System.IO;
using System.Text;

namespace mc_compiled.NBT;

public class NBTString : NBTNode
{
    public string value;

    public NBTString() { this.tagType = TAG.String; }

    public override void Write(BinaryWriter writer)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(this.value);
        ushort len = (ushort) bytes.Length;

        writer.Write(len);
        writer.Write(bytes);
    }

    protected bool Equals(NBTString other) { return this.value == other.value; }
    public override bool Equals(object obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((NBTString) obj);
    }
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return this.value != null ? this.value.GetHashCode() : 0;
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }
}