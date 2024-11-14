using System.Linq;

namespace mc_compiled.NBT
{
    public struct PaletteNBT
    {
        public PaletteEntryNBT[] block_palette;

        // NBTCompound blockPositionData;    // Unimplemented. This will break more complex structures!

        public PaletteNBT(params PaletteEntryNBT[] entries)
        {
            this.block_palette = entries;
        }
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
                                values = (from bp in this.block_palette select bp.ToNBT("")).ToArray<NBTNode>()
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
}
