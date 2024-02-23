namespace mc_compiled.NBT
{
    /// <summary>
    /// A Minecraft structure represented in NBT format.
    /// </summary>
    public struct StructureNBT
    {
        public int formatVersion;
        public VectorIntNBT size;
        public VectorIntNBT worldOrigin;

        // for 'structure' compound
        internal BlockIndicesNBT indices;
        internal EntityListNBT entities;
        internal PaletteNBT palette;

        public NBTNode[] ToNBT()
        {
            return new NBTNode[]
            {
                new NBTInt() { name = "format_version", value = this.formatVersion }, this.size.ToNBT("size"),
                new NBTCompound()
                {
                    name = "structure",
                    values = new NBTNode[]
                    {
                        this.indices.ToNBT(), this.entities.ToNBT(), this.palette.ToNBT(),
                        new NBTEnd()
                    }
                },
                this.worldOrigin.ToNBT("structure_world_origin"),
                new NBTEnd()
            };
        }

        /// <summary>
        /// Create a structure which holds only a single item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static StructureNBT SingleItem(Commands.Native.ItemStack item)
        {
            return new StructureNBT()
            {
                formatVersion = 1,
                size = new VectorIntNBT(1, 1, 1),
                worldOrigin = new VectorIntNBT(0, 0, 0),

                indices = new BlockIndicesNBT(new int[1, 1, 1]),
                palette = new PaletteNBT()
                {
                    block_palette = new PaletteEntryNBT[] { new PaletteEntryNBT("minecraft:air") }
                },

                entities = new EntityListNBT(new EntityNBT(
                    pos: new VectorNBT(0.5f, 1f, 0.5f),
                    health: 5,
                    invulnerable: true,
                    item: new ItemNBT(item),
                    identifier: "minecraft:item"))
            };
        }
    }
}
