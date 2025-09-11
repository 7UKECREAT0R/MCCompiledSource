using System;

namespace mc_compiled.NBT;

public struct VectorIntNBT : IEquatable<VectorIntNBT>
{
    public int x, y, z;

    public VectorIntNBT(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public NBTList ToNBT(string name)
    {
        return new NBTList
        {
            name = name,
            listType = TAG.Int,
            values =
            [
                new NBTInt {name = "", value = this.x},
                new NBTInt {name = "", value = this.y},
                new NBTInt {name = "", value = this.z}
            ]
        };
    }

    public bool Equals(VectorIntNBT other) { return this.x == other.x && this.y == other.y && this.z == other.z; }
    public override bool Equals(object obj) { return obj is VectorIntNBT other && Equals(other); }
    public override int GetHashCode() { return HashCode.Combine(this.x, this.y, this.z); }
}