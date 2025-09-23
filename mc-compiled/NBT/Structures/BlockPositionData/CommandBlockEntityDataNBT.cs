using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using mc_compiled.Commands;

namespace mc_compiled.NBT.Structures.BlockPositionData;

/// <summary>
///     Block entity data for a command block.
///     <br />
///     Keep in mind, the root behavior is still determined by the block itself, not this data.
///     So make sure in the palette you've defined whether this is a <c>command_block</c>, <c>chain_command_block</c>, or
///     <c>repeating_command_block</c>.
/// </summary>
/// <param name="command">The command inside the command block.</param>
/// <param name="isPowered">If the command block is pre-powered.</param>
/// <param name="x">The X coordinate of the command block.</param>
/// <param name="y">The Y coordinate of the command block.</param>
/// <param name="z">The Z coordinate of the command block.</param>
public class CommandBlockEntityDataNBT(string command, bool isPowered, int x, int y, int z)
    : PowerableBlockEntityDataNBT(CommonBlockEntityIdentifiers.CommandBlock, x, y, z, isPowered)
{
    private const int COMMAND_BLOCK_VERSION = 44; // harvested from a .mcstructure file as of 1.21.100

    /// <summary>
    ///     The command inside this command block.
    /// </summary>
    public readonly string command = command;

    /// <summary>
    ///     If this command block is marked as "Always Active". Usually required for chain command blocks to work as intended.
    /// </summary>
    public bool alwaysActive = false;

    /// <summary>
    ///     If this is a repeating command block, should it execute on the first tick of its existence or wait its delay first?
    /// </summary>
    public bool executeOnFirstTick = false;
    /// <summary>
    ///     If specified, the note to show on this command block when hovered over by a player.
    /// </summary>
    [CanBeNull]
    public string hoverNote = null;

    /// <summary>
    ///     If this command block is conditional based on the result of the one behind it.
    /// </summary>
    public bool isConditional;
    /// <summary>
    ///     The delay in ticks before this command block executes. If it's a repeating command block, this is the delay between
    ///     each execution.
    ///     Use <see cref="executeOnFirstTick" /> to control if the wait occurs after the first execution or not.
    /// </summary>
    public int tickDelay;
    /// <summary>
    ///     If the command block should store the text output of each command it executes.
    /// </summary>
    public bool trackOutput = false;
    /// <summary>
    ///     The type of command block this is. Keep in mind, the root behavior is still determined by the block itself, not
    ///     this data.
    ///     So make sure in the palette you've defined whether this is a <c>command_block</c>, <c>chain_command_block</c>, or
    ///     <c>repeating_command_block</c>.
    /// </summary>
    public CommandBlockType type = CommandBlockType.impulse;
    /// <summary>
    ///     Returns the identifier string for the specified <paramref name="type" /> of command block.
    /// </summary>
    /// <param name="type">The type of the command block to get the identifier for.</param>
    /// <returns>The identifier string of the specified <paramref name="type" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if the provided <paramref name="type" /> is not a valid <see cref="CommandBlockType" />.
    /// </exception>
    public static string GetCommandBlockIdentifier(CommandBlockType type)
    {
        return type switch
        {
            CommandBlockType.impulse => "minecraft:command_block",
            CommandBlockType.chain => "minecraft:chain_command_block",
            CommandBlockType.repeating => "minecraft:repeating_command_block",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    protected override List<NBTNode> GetNodes()
    {
        List<NBTNode> root = base.GetNodes();
        root.Add(new NBTString {name = "Command", value = this.command});
        root.Add(new NBTString {name = "CustomName", value = this.hoverNote ?? string.Empty});
        root.Add(new NBTByte {name = "ExecuteOnFirstTick", value = this.executeOnFirstTick ? (byte) 1 : (byte) 0});
        root.Add(new NBTInt
        {
            name = "LPCommandMode", value = this.type == CommandBlockType.repeating ? 1 : 0
        }); // for some reason 1 when it's a repeating command block.
        root.Add(new NBTInt {name = "LPConditionalMode", value = 0});
        root.Add(new NBTInt {name = "LPRedstoneMode", value = 0});
        root.Add(new NBTLong {name = "LastExecution", value = 0L});
        root.Add(new NBTString {name = "LastOutput", value = string.Empty});
        root.Add(new NBTList
        {
            name = "LastOutputParams", values = [],
            listType = TAG.String /* unknown actually. this may cause loading issues. */
        });
        root.Add(new NBTInt {name = "SuccessCount", value = 0});
        root.Add(new NBTInt {name = "TickDelay", value = this.tickDelay});
        root.Add(new NBTByte {name = "TrackOutput", value = this.trackOutput ? (byte) 1 : (byte) 0});
        root.Add(new NBTInt {name = "Version", value = COMMAND_BLOCK_VERSION});
        root.Add(new NBTByte {name = "auto", value = this.alwaysActive ? (byte) 1 : (byte) 0});
        root.Add(new NBTByte {name = "conditionMet", value = 1});
        root.Add(new NBTByte {name = "conditionalMode", value = this.isConditional ? (byte) 1 : (byte) 0});
        return root;
    }
}