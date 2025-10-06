using System.Collections.Generic;
using mc_compiled.NBT.Structures.BlockPositionData;

namespace mc_compiled.NBT.Structures;

/// <summary>
///     Block-entity specific data given a positional index.
/// </summary>
public readonly struct BlockPositionDataNBT()
{
    private readonly Dictionary<int, BasicBlockEntityDataNBT> data = new();

    public void Add(int index, BasicBlockEntityDataNBT toAdd) { this.data.Add(index, toAdd); }

    private List<NBTNode> GetEntries()
    {
        List<NBTNode> entries = [];
        if (this.data != null)
            foreach ((int key, BasicBlockEntityDataNBT value) in this.data)
            {
                var entry = new NBTCompound
                {
                    name = key.ToString(), // the index is the name for dictionary-like lookup
                    values = [value.ToNBT(), new NBTEnd()]
                };
                entries.Add(entry);
            }

        entries.Add(new NBTEnd());
        return entries;
    }
    public NBTCompound ToNBT()
    {
        return new NBTCompound
        {
            name = "block_position_data",
            values = GetEntries().ToArray()
        };
    }
}