using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.NBT.Structures.BlockPositionData;

namespace mc_compiled.NBT.Structures;

/// <summary>
///     A helper class for generating NBT structures. When built, results in a <see cref="StructureNBT" />.
///     Can consume a good bit of memory depending on how large the structure is, so just keep that in mind.
/// </summary>
/// <remarks>
///     This is optimized for building structures dynamically because it supports dynamic sizing and stores each block
///     individually rather than as a list of blocks. The palette is created at build-time.
/// </remarks>
public class StructureBuilder
{
    /// <summary>
    ///     All blocks in the structure.
    /// </summary>
    public readonly List<Block> blocks;
    /// <summary>
    ///     The filler block for any area that hasn't been specified in <see cref="blocks" />.
    ///     If null, structure voids are used.
    /// </summary>
    public PaletteEntryNBT? background = PaletteEntryNBT.Air;
    /// <summary>
    ///     All entities in the structure.
    /// </summary>
    public List<EntityNBT> entities;

    /// <summary>
    ///     Create a new empty structure builder.
    /// </summary>
    public StructureBuilder()
    {
        this.blocks = [];
        this.entities = [];
        this.blocks = [];
    }

    /// <summary>
    ///     Build the final structure into a <see cref="StructureNBT" />.
    /// </summary>
    public StructureNBT Build()
    {
        bool hasBlocks = this.blocks.Count > 0;
        bool hasEntities = this.entities.Count > 0;

        if (!hasBlocks & !hasEntities)
            return StructureNBT.Empty();

        StructureNBT structure;

        // process blocks, if any.
        if (hasBlocks)
        {
            Block first = this.blocks[0];
            int maxX, maxY, maxZ;
            int minX = maxX = first.position.x;
            int minY = maxY = first.position.y;
            int minZ = maxZ = first.position.z;

            foreach (Block block in this.blocks.Skip(1))
            {
                if (block.position.x < minX)
                    minX = block.position.x;
                if (block.position.y < minY)
                    minY = block.position.y;
                if (block.position.z < minZ)
                    minZ = block.position.z;
                if (block.position.x > maxX)
                    maxX = block.position.x;
                if (block.position.y > maxY)
                    maxY = block.position.y;
                if (block.position.z > maxZ)
                    maxZ = block.position.z;
            }

            int sizeX = maxX - minX + 1;
            int sizeY = maxY - minY + 1;
            int sizeZ = maxZ - minZ + 1;

            var blockPositionData = new BlockPositionDataNBT();
            Dictionary<PaletteEntryNBT, int> paletteEntries = [];
            int numberOfPaletteEntries;
            if (this.background.HasValue)
            {
                paletteEntries.Add(this.background.Value, 0);
                numberOfPaletteEntries = 1;
            }
            else
            {
                numberOfPaletteEntries = 0;
            }

            int GetPaletteIndex(PaletteEntryNBT entry)
            {
                if (paletteEntries.TryGetValue(entry, out int index))
                    return index;
                index = numberOfPaletteEntries++;
                paletteEntries.Add(entry, index);
                return index;
            }

            structure = new StructureNBT
            {
                formatVersion = 1,
                size = new VectorIntNBT(sizeX, sizeY, sizeZ),
                worldOrigin = new VectorIntNBT(minX, minY, minZ)
            };

            // initialized to zeroes, so every entry references the background block by default
            int[,,] indices = new int[sizeX, sizeY, sizeZ];

            // ...unless void is going to be used
            if (!this.background.HasValue)
                for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                for (int z = 0; z < sizeZ; z++)
                    indices[x, y, z] = -1;

            // fill the indices array with the palette indices.
            foreach (Block block in this.blocks)
            {
                int x = block.position.x - minX;
                int y = block.position.y - minY;
                int z = block.position.z - minZ;
                if (block.isVoid)
                {
                    indices[x, y, z] = -1;
                }
                else
                {
                    int paletteIndex = GetPaletteIndex(block.paletteEntry);
                    indices[x, y, z] = paletteIndex;
                }

                // if the block has block-entity data, we'll need to do a second pass.
                if (block.blockEntityData != null)
                {
                    int flattenedIndex = x * sizeY * sizeZ + y * sizeZ + z;
                    blockPositionData.Add(flattenedIndex, block.blockEntityData);
                }
            }

            // flatten the palette out
            var palette = new PaletteEntryNBT[numberOfPaletteEntries];
            foreach ((PaletteEntryNBT entry, int index) in paletteEntries)
                palette[index] = entry;

            structure.indices = new BlockIndicesNBT(indices);
            structure.palette = new PaletteNBT(palette)
            {
                blockPositionData = blockPositionData
            };
        }
        else
        {
            structure = StructureNBT.Empty(1, 1, 1);
        }

        // process entities, if any.
        if (hasEntities)
            structure.entities = new EntityListNBT(this.entities.ToArray());
        else
            structure.entities = new EntityListNBT();

        // return the finished structure.
        return structure;
    }

    public struct Block
    {
        /// <summary>
        ///     Is this block a structure void?
        /// </summary>
        public bool isVoid;
        /// <summary>
        ///     The position of the block.
        /// </summary>
        public VectorIntNBT position;
        /// <summary>
        ///     The palette entry for the block, including any possible block states.
        /// </summary>
        public readonly PaletteEntryNBT paletteEntry;
        /// <summary>
        ///     The block-entity data tied to this block, if any. e.g., command block command, chest contents, etc...
        /// </summary>
        [CanBeNull]
        public readonly BasicBlockEntityDataNBT blockEntityData = null;

        public Block(int x, int y, int z, PaletteEntryNBT entry, BasicBlockEntityDataNBT data = null)
        {
            this.position = new VectorIntNBT(x, y, z);
            this.paletteEntry = entry;
            this.blockEntityData = data;
        }

        #region Presets

        public static Block Air(int x, int y, int z) { return new Block(x, y, z, PaletteEntryNBT.Air); }
        public static Block CommandBlockImpulse(int x,
            int y,
            int z,
            string command,
            FacingDirection facingDirection,
            bool conditional = false,
            int tickDelay = 0)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:command_block",
                states =
                [
                    new NBTByte {name = "conditional_bit", value = conditional ? (byte) 1 : (byte) 0},
                    new NBTInt {name = "facing_direction", value = (int) facingDirection}
                ]
            }, new CommandBlockEntityDataNBT(command, false, x, y, z)
            {
                isConditional = conditional,
                tickDelay = tickDelay,
                type = CommandBlockType.Impulse
            });
        }
        public static Block CommandBlockChain(int x,
            int y,
            int z,
            string command,
            FacingDirection facingDirection,
            bool conditional = false,
            int tickDelay = 0)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:chain_command_block",
                states =
                [
                    new NBTByte {name = "conditional_bit", value = conditional ? (byte) 1 : (byte) 0},
                    new NBTInt {name = "facing_direction", value = (int) facingDirection}
                ]
            }, new CommandBlockEntityDataNBT(command, false, x, y, z)
            {
                isConditional = conditional,
                alwaysActive = true,
                tickDelay = tickDelay,
                type = CommandBlockType.Chain
            });
        }
        public static Block CommandBlockChainOnlyWhenPowered(int x,
            int y,
            int z,
            string command,
            FacingDirection facingDirection,
            bool conditional = false,
            int tickDelay = 0)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:chain_command_block",
                states =
                [
                    new NBTByte {name = "conditional_bit", value = conditional ? (byte) 1 : (byte) 0},
                    new NBTInt {name = "facing_direction", value = (int) facingDirection}
                ]
            }, new CommandBlockEntityDataNBT(command, false, x, y, z)
            {
                isConditional = conditional,
                alwaysActive = false,
                tickDelay = tickDelay,
                type = CommandBlockType.Chain
            });
        }
        public static Block CommandBlockRepeatingAlways(int x,
            int y,
            int z,
            string command,
            FacingDirection facingDirection,
            bool conditional = false,
            int tickDelay = 0,
            bool runOnFirstTick = true)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:repeating_command_block",
                states =
                [
                    new NBTByte {name = "conditional_bit", value = conditional ? (byte) 1 : (byte) 0},
                    new NBTInt {name = "facing_direction", value = (int) facingDirection}
                ]
            }, new CommandBlockEntityDataNBT(command, false, x, y, z)
            {
                isConditional = conditional,
                alwaysActive = true,
                tickDelay = tickDelay,
                executeOnFirstTick = runOnFirstTick,
                type = CommandBlockType.Repeating
            });
        }
        public static Block CommandBlockRepeatingOnlyWhenPowered(int x,
            int y,
            int z,
            string command,
            FacingDirection facingDirection,
            bool conditional = false,
            int tickDelay = 0,
            bool runOnFirstTick = true)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:repeating_command_block",
                states =
                [
                    new NBTByte {name = "conditional_bit", value = conditional ? (byte) 1 : (byte) 0},
                    new NBTInt {name = "facing_direction", value = (int) facingDirection}
                ]
            }, new CommandBlockEntityDataNBT(command, false, x, y, z)
            {
                isConditional = conditional,
                alwaysActive = false,
                tickDelay = tickDelay,
                executeOnFirstTick = runOnFirstTick,
                type = CommandBlockType.Repeating
            });
        }
        public static Block Barrel(int x,
            int y,
            int z,
            FacingDirection direction,
            ContainerBlockEntityDataNBT blockEntity)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:barrel",
                states =
                [
                    new NBTInt {name = "facing_direction", value = (int) direction},
                    new NBTByte {name = "open_bit", value = 0}
                ]
            }, blockEntity);
        }
        public static Block Chest(int x,
            int y,
            int z,
            CardinalDirection direction,
            ContainerBlockEntityDataNBT blockEntity)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:chest",
                states =
                [
                    new NBTString {name = "minecraft:cardinal_direction", value = direction.ToString()}
                ]
            }, blockEntity);
        }
        public static Block Furnace(int x,
            int y,
            int z,
            CardinalDirection direction,
            ContainerBlockEntityDataNBT blockEntity)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:furnace",
                states =
                [
                    new NBTString {name = "minecraft:cardinal_direction", value = direction.ToString()}
                ]
            }, blockEntity);
        }
        public static Block Dropper(int x,
            int y,
            int z,
            FacingDirection direction,
            ContainerBlockEntityDataNBT blockEntity)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:dropper",
                states =
                [
                    new NBTInt {name = "facing_direction", value = (int) direction},
                    new NBTByte {name = "triggered_bit", value = 0}
                ]
            }, blockEntity);
        }
        public static Block Dispenser(int x,
            int y,
            int z,
            FacingDirection direction,
            ContainerBlockEntityDataNBT blockEntity)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:dispenser",
                states =
                [
                    new NBTInt {name = "facing_direction", value = (int) direction},
                    new NBTByte {name = "triggered_bit", value = 0}
                ]
            }, blockEntity);
        }
        public static Block Hopper(int x,
            int y,
            int z,
            FacingDirection direction,
            ContainerBlockEntityDataNBT blockEntity)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:hopper",
                states =
                [
                    new NBTInt {name = "facing_direction", value = (int) direction},
                    new NBTByte {name = "toggle_bit", value = 0}
                ]
            }, blockEntity);
        }
        public static Block StandingSign(int x,
            int y,
            int z,
            string frontText,
            string backText,
            bool isEditable,
            GroundSignDirection direction)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:standing_sign",
                states =
                [
                    new NBTInt {name = "ground_sign_direction", value = (int) direction}
                ]
            }, new SignBlockEntityDataNBT(x, y, z, isEditable, frontText, backText));
        }
        public static Block WallSign(int x,
            int y,
            int z,
            string frontText,
            string backText,
            bool isEditable,
            FacingDirection direction)
        {
            return new Block(x, y, z, new PaletteEntryNBT
            {
                name = "minecraft:wall_sign",
                states =
                [
                    new NBTInt {name = "facing_direction", value = (int) direction}
                ]
            }, new SignBlockEntityDataNBT(x, y, z, isEditable, frontText, backText));
        }

        /// <summary>
        ///     An integer representing a facing direction of a block.
        /// </summary>
        [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
        public enum FacingDirection
        {
            Down = 0,
            Up = 1,
            NegativeZ = 2,
            North = 2,
            PositiveZ = 3,
            South = 3,
            NegativeX = 4,
            West = 4,
            PositiveX = 5,
            East = 5
        }

        /// <summary>
        ///     A regular cardinal direction. Use <see cref="CardinalDirection.ToString()" />.
        /// </summary>
        public enum CardinalDirection
        {
            north,
            south,
            east,
            west
        }

        /// <summary>
        ///     The direction a sign can face while on the ground.
        /// </summary>
        public enum GroundSignDirection
        {
            south = 0,
            south_southwest = 1,
            southwest = 2,
            west_southwest = 3,
            west = 4,
            west_northwest = 5,
            northwest = 6,
            north_northwest = 7,
            north = 8,
            north_northeast = 9,
            northeast = 10,
            east_northeast = 11,
            east = 12,
            east_southeast = 13,
            southeast = 14,
            south_southeast = 15
        }

        #endregion
    }
}