using System.Linq;

namespace mc_compiled.NBT.Structures;

public struct PaletteNBT(params PaletteEntryNBT[] entries)
{
    /// <summary>
    ///     The basic palette to use inside the structure.
    /// </summary>
    public PaletteEntryNBT[] blockPalette = entries;
    /// <summary>
    ///     The extra positional data for block entities with more advanced features, like command blocks or chests.
    /// </summary>
    public BlockPositionDataNBT blockPositionData = new();

    public NBTCompound ToNBT()
    {
        return new NBTCompound
        {
            name = "palette",
            values =
            [
                new NBTCompound
                {
                    name = "default",
                    values =
                    [
                        new NBTList
                        {
                            name = "block_palette",
                            listType = TAG.Compound,
                            values = (from bp in this.blockPalette select bp.ToNBT("")).ToArray<NBTNode>()
                        },
                        this.blockPositionData.ToNBT(),
                        new NBTEnd()
                    ]
                },
                new NBTEnd()
            ]
        };
    }
}