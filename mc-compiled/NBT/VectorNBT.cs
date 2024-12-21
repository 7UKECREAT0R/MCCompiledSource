namespace mc_compiled.NBT;

/// <summary>
///     A 3d vector.
/// </summary>
public struct VectorNBT
{
    public float x, y, z;

    public VectorNBT(float x, float y, float z)
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
            listType = TAG.Float,
            values =
            [
                new NBTFloat {name = "", value = this.x},
                new NBTFloat {name = "", value = this.y},
                new NBTFloat {name = "", value = this.z}
            ]
        };
    }
}