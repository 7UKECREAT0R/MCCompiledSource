using System.Linq;

namespace mc_compiled.NBT.Structures;

public struct PaletteNBT
{
    /// <summary>
    ///     The basic palette to use inside the structure.
    /// </summary>
    public PaletteEntryNBT[] blockPalette;
    /// <summary>
    ///     The extra positional data for block entities with more advanced features, like command blocks or chests.
    /// </summary>
    public BlockPositionDataNBT blockPositionData;

    public PaletteNBT(params PaletteEntryNBT[] entries) { this.blockPalette = entries; }
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
                        new NBTCompound
                        {
                            name = "block_position_data",
                            values = [new NBTEnd()]
                        },
                        new NBTEnd()
                    ]
                },
                new NBTEnd()
            ]
        };
    }
}