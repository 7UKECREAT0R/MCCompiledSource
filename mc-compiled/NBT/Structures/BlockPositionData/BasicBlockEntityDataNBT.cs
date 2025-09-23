using System.Collections.Generic;
using mc_compiled.Commands;

namespace mc_compiled.NBT.Structures.BlockPositionData;

/// <summary>
///     Represents shared properties of all block entities.
/// </summary>
/// <param name="id">
///     The identifier of the block entity; not a typical block identifier. See
///     <see cref="CommonBlockEntityIdentifiers" />
///     for common identifiers verbatim.
/// </param>
/// <param name="x">The X position of the block entity.</param>
/// <param name="y">The Y position of the block entity.</param>
/// <param name="z">The Z position of the block entity.</param>
/// <param name="isMovable">Unsure of what this does, but it's generally true, even on immovable block entities like signs.</param>
public class BasicBlockEntityDataNBT(string id, int x, int y, int z, bool isMovable = true)
{
    /// <summary>
    ///     The identifier of the block entity; not a typical block identifier. See <see cref="CommonBlockEntityIdentifiers" />
    ///     for common identifiers verbatim.
    /// </summary>
    private readonly string id = id;
    /// <summary>
    ///     Unsure of what this does, but it's generally true, even on immovable block entities like signs.
    /// </summary>
    private readonly bool isMovable = isMovable;
    /// <summary>
    ///     The position of the block entity.
    /// </summary>
    /// <remarks>
    ///     Technically, this is already encoded in the index information that this entity data is stored under, but it's
    ///     required nonetheless.
    /// </remarks>
    private readonly int x = x, y = y, z = z;

    /// <summary>
    ///     Gets the nodes associated with this block entity data. Can be overridden to add more functionality.
    /// </summary>
    /// <returns></returns>
    protected virtual List<NBTNode> GetNodes()
    {
        var nodes = new List<NBTNode>
        {
            new NBTString {name = "id", value = this.id},
            new NBTByte {name = "isMovable", value = this.isMovable ? (byte) 1 : (byte) 0},
            new NBTInt {name = "x", value = this.x},
            new NBTInt {name = "y", value = this.y},
            new NBTInt {name = "z", value = this.z}
        };
        return nodes;
    }
    /// <summary>
    ///     Returns the NBT representation of this entity data. Returned as a compound tag with the name "
    ///     <c>block_entity_data</c>". Comes with an <see cref="NBTEnd" /> tag already.
    /// </summary>
    /// <returns></returns>
    public NBTCompound ToNBT()
    {
        List<NBTNode> nodes = GetNodes();
        nodes.Add(new NBTEnd());

        return new NBTCompound
        {
            name = "block_entity_data",
            values = nodes.ToArray()
        };
    }
}

/// <summary>
///     Contains common identifiers for block entities, since they vary from vanilla block identifiers for whatever reason.
/// </summary>
public static class CommonBlockEntityIdentifiers
{
    public const string Barrel = "Barrel";
    public const string Chest = "Chest";
    /// <remarks>
    ///     Any kind of command block, including chain/repeating!
    /// </remarks>
    public const string CommandBlock = "CommandBlock";
    public const string Dispenser = "Dispenser";
    public const string Dropper = "Dropper";
    public const string Furnace = "Furnace";
    public const string Hopper = "Hopper";
    public const string Sign = "Sign";

    /// <summary>
    ///     Converts a block identifier into its corresponding block entity identifier, if applicable.
    /// </summary>
    /// <param name="blockIdentifier">The block identifier, optionally with or without namespace.</param>
    /// <returns>The block entity identifier if a match is found, otherwise the original <paramref name="blockIdentifier" />.</returns>
    public static string ConvertBlockToBlockEntity(string blockIdentifier)
    {
        blockIdentifier = Command.Util.RequireNamespace(blockIdentifier).ToLower();
        return blockIdentifier switch
        {
            "minecraft:chest" => Chest,
            "minecraft:barrel" => Barrel,
            "minecraft:command_block" => CommandBlock,
            "minecraft:chain_command_block" => CommandBlock,
            "minecraft:repeating_command_block" => CommandBlock,
            "minecraft:dispenser" => Dispenser,
            "minecraft:dropper" => Dropper,
            "minecraft:furnace" => Furnace,
            "minecraft:hopper" => Hopper,
            "minecraft:sign" => Sign,
            _ => blockIdentifier
        };
    }
}