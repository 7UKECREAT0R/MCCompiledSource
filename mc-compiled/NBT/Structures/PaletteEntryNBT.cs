using System;

namespace mc_compiled.NBT.Structures;

public struct PaletteEntryNBT : IEquatable<PaletteEntryNBT>
{
    private const int VERSION = 18168865;
    public string name;
    public NBTNode[] states;
    public int version = VERSION;

    public PaletteEntryNBT(string name, NBTNode[] states = null, int version = VERSION /* last updated 1.21.100 */)
    {
        this.name = name;
        this.states = states;
        this.version = version;
    }
    /// <summary>
    ///     Preset containing "minecraft:air".
    /// </summary>
    public static readonly PaletteEntryNBT Air = new("minecraft:air");

    public NBTCompound ToNBT(string compoundTagName)
    {
        if (this.states == null)
            this.states = [];

        var state = new NBTCompound
        {
            name = "states",
            values = [..this.states, new NBTEnd()]
        };

        return new NBTCompound
        {
            name = compoundTagName,
            values =
            [
                new NBTString {name = "name", value = this.name},
                state,
                new NBTInt {name = "version", value = this.version},
                new NBTEnd()
            ]
        };
    }

    public bool Equals(PaletteEntryNBT other) { return this.name == other.name && Equals(this.states, other.states); }
    public override bool Equals(object obj) { return obj is PaletteEntryNBT other && Equals(other); }
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(this.name);

        if (this.states != null)
            foreach (NBTNode state in this.states)
                hash.Add(state);

        return hash.ToHashCode();
    }
    public static bool operator ==(PaletteEntryNBT left, PaletteEntryNBT right) { return left.Equals(right); }
    public static bool operator !=(PaletteEntryNBT left, PaletteEntryNBT right) { return !(left == right); }
}