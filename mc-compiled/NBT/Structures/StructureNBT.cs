using mc_compiled.Commands.Native;

namespace mc_compiled.NBT.Structures;

/// <summary>
///     A Minecraft structure represented in NBT format.
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
        return
        [
            new NBTInt {name = "format_version", value = this.formatVersion}, this.size.ToNBT("size"),
            new NBTCompound
            {
                name = "structure",
                values =
                [
                    this.indices.ToNBT(),
                    this.entities.ToNBT(),
                    this.palette.ToNBT(),
                    new NBTEnd()
                ]
            },
            this.worldOrigin.ToNBT("structure_world_origin"),
            new NBTEnd()
        ];
    }

    /// <summary>
    ///     Create a structure which holds only a single item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static StructureNBT SingleItem(ItemStack item)
    {
        return new StructureNBT
        {
            formatVersion = 1,
            size = new VectorIntNBT(1, 1, 1),
            worldOrigin = new VectorIntNBT(0, 0, 0),

            indices = new BlockIndicesNBT(new int[1, 1, 1]),
            palette = new PaletteNBT
            {
                blockPalette = [PaletteEntryNBT.Air]
            },

            entities = new EntityListNBT(new EntityNBT(
                pos: new VectorNBT(0.5f, 1f, 0.5f),
                health: 5,
                invulnerable: true,
                item: new ItemNBT(item),
                identifier: "minecraft:item"))
        };
    }
    /// <summary>
    ///     Creates a new empty structure with a size of 0, 0, 0.
    /// </summary>
    /// <returns></returns>
    public static StructureNBT Empty()
    {
        return new StructureNBT
        {
            formatVersion = 1,
            size = new VectorIntNBT(0, 0, 0),
            worldOrigin = new VectorIntNBT(0, 0, 0),
            indices = new BlockIndicesNBT(new int[0, 0, 0]),
            palette = new PaletteNBT(),
            entities = new EntityListNBT()
        };
    }
    /// <summary>
    ///     Creates a new empty structure with the given size, filled with air.
    /// </summary>
    /// <param name="sizeX">The X size.</param>
    /// <param name="sizeY">The Y size.</param>
    /// <param name="sizeZ">The Z size.</param>
    /// <returns></returns>
    public static StructureNBT Empty(int sizeX, int sizeY, int sizeZ)
    {
        return new StructureNBT
        {
            formatVersion = 1,
            size = new VectorIntNBT(sizeX, sizeY, sizeZ),
            worldOrigin = new VectorIntNBT(0, 0, 0),

            indices = new BlockIndicesNBT(new int[sizeX, sizeY, sizeZ]),
            palette = new PaletteNBT
            {
                blockPalette = [PaletteEntryNBT.Air]
            },
            entities = new EntityListNBT()
        };
    }
}