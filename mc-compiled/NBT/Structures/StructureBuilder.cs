using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.Commands.Native;
using mc_compiled.MCC.Compiler;
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
    private const string VOID_IDENTIFIER = "minecraft:structure_void";
    /// <summary>
    ///     All blocks in the structure.
    /// </summary>
    public readonly List<Block> blocks;
    private readonly Stack<ContainerBuilder> currentContainer = new();
    /// <summary>
    ///     The filler block for any area that hasn't been specified in <see cref="blocks" />.
    ///     If null, structure voids are used.
    /// </summary>
    public PaletteEntryNBT? background = null;
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
    }

    /// <summary>
    ///     Returns if a container is currently being built.
    /// </summary>
    public bool IsInContainer => this.currentContainer.Count != 0;

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

    public void DirectiveSetblock(Executor executor, Statement tokens)
    {
        int x = tokens.Next<TokenCoordinateLiteral>("x").GetNumberInt();
        int y = tokens.Next<TokenCoordinateLiteral>("y").GetNumberInt();
        int z = tokens.Next<TokenCoordinateLiteral>("z").GetNumberInt();
        string block = tokens.Next<TokenStringLiteral>("block");
        block = Command.Util.RequireNamespace(block);
        block.ThrowIfWhitespace("block", tokens);

        BlockState[] blockStates = null;
        if (tokens.NextIs<TokenBlockStatesLiteral>(false))
        {
            blockStates = tokens.Next<TokenBlockStatesLiteral>("block states");
            blockStates.ValidateIfKnownVanillaBlock(block, false, tokens);
        }

        if (executor.emission.isLinting)
            return;

        if (block.Equals(VOID_IDENTIFIER))
        {
            this.blocks.RemoveAll(b => b.position.x == x && b.position.y == y && b.position.z == z);
            this.blocks.Add(Block.Void(x, y, z));
        }
        else
        {
            this.blocks.Add(new Block(x, y, z, new PaletteEntryNBT(block, blockStates.ToNBT())));
        }
    }
    public void DirectiveFill(Executor executor, Statement tokens)
    {
        int x1 = tokens.Next<TokenCoordinateLiteral>("x1").GetNumberInt();
        int y1 = tokens.Next<TokenCoordinateLiteral>("y1").GetNumberInt();
        int z1 = tokens.Next<TokenCoordinateLiteral>("z1").GetNumberInt();
        int x2 = tokens.Next<TokenCoordinateLiteral>("x2").GetNumberInt();
        int y2 = tokens.Next<TokenCoordinateLiteral>("y2").GetNumberInt();
        int z2 = tokens.Next<TokenCoordinateLiteral>("z2").GetNumberInt();
        if (x1 > x2)
            (x2, x1) = (x1, x2);
        if (y1 > y2)
            (y2, y1) = (y1, y2);
        if (z1 > z2)
            (z2, z1) = (z1, z2);

        string block = tokens.Next<TokenStringLiteral>("block");
        block = Command.Util.RequireNamespace(block);
        block.ThrowIfWhitespace("block", tokens);

        BlockState[] blockStates = null;
        if (tokens.NextIs<TokenBlockStatesLiteral>(false))
        {
            blockStates = tokens.Next<TokenBlockStatesLiteral>("block states");
            blockStates.ValidateIfKnownVanillaBlock(block, false, tokens);
        }

        if (executor.emission.isLinting)
            return;

        bool isVoid = block.Equals(VOID_IDENTIFIER);
        PaletteEntryNBT paletteEntry = isVoid ? default : new PaletteEntryNBT(block, blockStates.ToNBT());

        if (isVoid)
            this.blocks.RemoveAll(b =>
                b.position.x >= x1 && b.position.y >= y1 && b.position.z >= z1 && b.position.x <= x2 &&
                b.position.y <= y2 && b.position.z <= z2);

        // fill the blocks in the range.
        for (int x = x1; x <= x2; x++)
        for (int y = y1; y <= y2; y++)
        for (int z = z1; z <= z2; z++)
            if (isVoid)
                this.blocks.Add(Block.Void(x, y, z));
            else
                this.blocks.Add(new Block(x, y, z, paletteEntry));
    }
    public void DirectiveReplace(Executor executor, Statement tokens)
    {
        int x1 = tokens.Next<TokenCoordinateLiteral>("x1").GetNumberInt();
        int y1 = tokens.Next<TokenCoordinateLiteral>("y1").GetNumberInt();
        int z1 = tokens.Next<TokenCoordinateLiteral>("z1").GetNumberInt();
        int x2 = tokens.Next<TokenCoordinateLiteral>("x2").GetNumberInt();
        int y2 = tokens.Next<TokenCoordinateLiteral>("y2").GetNumberInt();
        int z2 = tokens.Next<TokenCoordinateLiteral>("z2").GetNumberInt();
        if (x1 > x2)
            (x2, x1) = (x1, x2);
        if (y1 > y2)
            (y2, y1) = (y1, y2);
        if (z1 > z2)
            (z2, z1) = (z1, z2);

        string sourceBlock = tokens.Next<TokenStringLiteral>("source block");
        sourceBlock = Command.Util.RequireNamespace(sourceBlock);
        sourceBlock.ThrowIfWhitespace("source block", tokens);

        BlockState[] sourceBlockStates = null;
        if (tokens.NextIs<TokenBlockStatesLiteral>(false))
        {
            sourceBlockStates = tokens.Next<TokenBlockStatesLiteral>("source block states");
            sourceBlockStates.ValidateIfKnownVanillaBlock(sourceBlock, false, tokens);
        }

        string destinationBlock = tokens.Next<TokenStringLiteral>("destination block");
        destinationBlock = Command.Util.RequireNamespace(destinationBlock);
        destinationBlock.ThrowIfWhitespace("destination block", tokens);

        BlockState[] destinationBlockStates = null;
        if (tokens.NextIs<TokenBlockStatesLiteral>(false))
        {
            destinationBlockStates = tokens.Next<TokenBlockStatesLiteral>("destination block states");
            destinationBlockStates.ValidateIfKnownVanillaBlock(destinationBlock, false, tokens);
        }

        if (executor.emission.isLinting)
            return;

        bool sourceIsVoid = sourceBlock.Equals(VOID_IDENTIFIER);
        bool destinationIsVoid = destinationBlock.Equals(VOID_IDENTIFIER);

        if (sourceIsVoid && destinationIsVoid)
            throw new StatementException(tokens, "This operation does nothing. (replacing void with void)");

        // check and replace the blocks in the range, if any.
        if (sourceIsVoid)
        {
            // take a different approach, since lack of block is also void.
            bool[,,] isSomething = new bool[x2 - x1 + 1, y2 - y1 + 1, z2 - z1 + 1];

            // loop over every block and slowly modify the mask
            foreach (Block currentBlock in this.blocks)
            {
                int x = currentBlock.position.x;
                int y = currentBlock.position.y;
                int z = currentBlock.position.z;
                if (x < x1 || x > x2 || y < y1 || y > y2 || z < z1 || z > z2)
                    continue;
                if (!currentBlock.isVoid)
                    isSomething[x, y, z] = true;
            }

            // now, we can check for `false` values in `isSomething` to know if the destination block should be placed.
            for (int x = x1; x <= x2; x++)
            for (int y = y1; y <= y2; y++)
            for (int z = z1; z <= z2; z++)
            {
                if (isSomething[x, y, z])
                    continue;

                // `destinationIsVoid` will never be true here
                this.blocks.Add(new Block(x, y, z,
                    new PaletteEntryNBT(destinationBlock, destinationBlockStates.ToNBT())));
            }
        }
        else
        {
            for (int i = this.blocks.Count - 1; i >= 0; i--)
            {
                Block currentBlock = this.blocks[i];
                int x = currentBlock.position.x;
                int y = currentBlock.position.y;
                int z = currentBlock.position.z;

                if (x < x1 || x > x2 || y < y1 || y > y2 || z < z1 || z > z2)
                    continue;

                // check for equality with the source block

                // self-explanatory
                if (currentBlock.isVoid)
                    continue;
                // block identifier
                string currentBlockName = Command.Util.RequireNamespace(currentBlock.paletteEntry.name);
                if (!currentBlockName.Equals(sourceBlock))
                    continue;
                // block states
                if (sourceBlockStates is {Length: > 0})
                {
                    bool blockStatesMatch = true;
                    NBTNode[] currentBlockStates = currentBlock.paletteEntry.states;
                    foreach (BlockState requiredBlockState in sourceBlockStates)
                    {
                        NBTNode matchingNode = currentBlockStates.FirstOrDefault
                            (node => node.name.Equals(requiredBlockState.propertyName));

                        if (matchingNode == null)
                            // no matching block state was specified, but check if the requested value is the default
                            if (!requiredBlockState.IsDefault)
                            {
                                blockStatesMatch = false;
                                break;
                            }

                        if (!requiredBlockState.IsEqualToNode(matchingNode))
                        {
                            blockStatesMatch = false;
                            break;
                        }
                    }

                    if (!blockStatesMatch)
                        continue;
                }

                if (destinationIsVoid)
                    this.blocks.RemoveAt(i); // remove it, which does the same thing as setting it to void
                else
                    this.blocks[i] = new Block(x, y, z,
                        new PaletteEntryNBT(destinationBlock, destinationBlockStates.ToNBT()));
            }
        }
    }
    public void DirectiveScatter(Executor executor, Statement tokens)
    {
        int x1 = tokens.Next<TokenCoordinateLiteral>("x1").GetNumberInt();
        int y1 = tokens.Next<TokenCoordinateLiteral>("y1").GetNumberInt();
        int z1 = tokens.Next<TokenCoordinateLiteral>("z1").GetNumberInt();
        int x2 = tokens.Next<TokenCoordinateLiteral>("x2").GetNumberInt();
        int y2 = tokens.Next<TokenCoordinateLiteral>("y2").GetNumberInt();
        int z2 = tokens.Next<TokenCoordinateLiteral>("z2").GetNumberInt();
        if (x1 > x2)
            (x2, x1) = (x1, x2);
        if (y1 > y2)
            (y2, y1) = (y1, y2);
        if (z1 > z2)
            (z2, z1) = (z1, z2);

        string block = tokens.Next<TokenStringLiteral>("block");
        block = Command.Util.RequireNamespace(block);
        block.ThrowIfWhitespace("block", tokens);

        BlockState[] blockStates = null;
        if (tokens.NextIs<TokenBlockStatesLiteral>(false))
        {
            blockStates = tokens.Next<TokenBlockStatesLiteral>("block states");
            blockStates.ValidateIfKnownVanillaBlock(block, false, tokens);
        }

        int scatterPercentage = tokens.Next<TokenIntegerLiteral>("scatter percentage");
        if (scatterPercentage < 0)
            scatterPercentage = 0;
        if (scatterPercentage > 100)
            scatterPercentage = 100;

        double placeChance = scatterPercentage / 100.0;

        Random random;
        if (tokens.NextIs<TokenStringLiteral>(false))
            random = new Random(tokens.Next<TokenStringLiteral>("seed").text.GetHashCode());
        else
            random = new Random(); // seeded based on current time

        if (executor.emission.isLinting)
            return;

        bool isVoid = block.Equals(VOID_IDENTIFIER);

        // fill the blocks in the range.
        for (int x = x1; x <= x2; x++)
        for (int y = y1; y <= y2; y++)
        for (int z = z1; z <= z2; z++)
            if (random.NextDouble() < placeChance) // compile-time place chance
            {
                if (isVoid)
                {
                    this.blocks.RemoveAll(b => b.position.x == x && b.position.y == y && b.position.z == z);
                    this.blocks.Add(Block.Void(x, y, z));
                }
                else
                {
                    this.blocks.Add(new Block(x, y, z, new PaletteEntryNBT(block, blockStates.ToNBT())));
                }
            }
    }
    public void DirectiveContainer(Executor executor, Statement tokens)
    {
        if (this.IsInContainer)
            throw new StatementException(tokens, "A container is already being defined.");

        int x = tokens.Next<TokenCoordinateLiteral>("x").GetNumberInt();
        int y = tokens.Next<TokenCoordinateLiteral>("y").GetNumberInt();
        int z = tokens.Next<TokenCoordinateLiteral>("z").GetNumberInt();
        string block = tokens.Next<TokenStringLiteral>("block");
        block.ThrowIfWhitespace("block", tokens);
        block = Command.Util.RequireNamespace(block);

        var direction = tokens.NextEnum<Block.FacingDirection>("facing direction");
        bool useCardinalDirection = block.Equals("minecraft:chest") || block.Equals("minecraft:furnace");
        Block baseBlock;

        // if the block is a chest or furnace, only cardinal directions are allowed.
        if (useCardinalDirection)
        {
            Block.CardinalDirection cardinalDirection = Block.FacingToCardinalDirection(direction, tokens);
            baseBlock = new Block(x, y, z, new PaletteEntryNBT(block, [
                new NBTString {name = "minecraft:cardinal_direction", value = cardinalDirection.ToString()}
            ]));
        }
        else
        {
            baseBlock = new Block(x, y, z, new PaletteEntryNBT(block, [
                new NBTInt {name = "facing_direction", value = (int) direction}
            ]));
        }

        var containerBuilder = new ContainerBuilder(baseBlock);
        if (!executor.NextIs<StatementOpenBlock>())
            throw new StatementException(tokens,
                "Expected a block to follow this statement, defining the container's items.");

        // have the block push/pop the container on and off of the stack
        var codeBlock = executor.Peek<StatementOpenBlock>();
        codeBlock.openAction = _ => this.currentContainer.Push(containerBuilder);
        codeBlock.CloseAction = e =>
        {
            ContainerBuilder populatedContainerBuilder = this.currentContainer.Pop();
            if (!e.emission.isLinting)
                this.blocks.Add(populatedContainerBuilder.Build());
        };
    }
    public void DirectiveItem(Executor executor, Statement tokens)
    {
        if (!this.IsInContainer)
            throw new StatementException(tokens,
                "This command can only be used when defining a container. See the 'container' command.");
        ContainerBuilder container = this.currentContainer.Peek();

        string itemName = tokens.Next<TokenStringLiteral>("item");
        itemName.ThrowIfWhitespace("item", tokens);
        itemName = Command.Util.RequireNamespace(itemName);

        byte? slot = null;
        int count = 1;
        int data = 0;
        bool keep = false;
        bool lockInventory = false;
        bool lockSlot = false;
        var loreLines = new List<string>();
        var canPlaceOn = new List<string>();
        var canDestroy = new List<string>();
        var enchants = new List<Tuple<Enchantment, int>>();
        string displayName = null;

        ItemTagBookData? book = null;
        List<string> bookPages = null;
        ItemTagCustomColor? color = null;

        if (tokens.NextIs<TokenIntegerLiteral>(false))
        {
            count = tokens.Next<TokenIntegerLiteral>("count");
            if (count < 1)
                throw new StatementException(tokens, "Item count cannot be less than 1.");

            if (tokens.NextIs<TokenIntegerLiteral>(false))
                data = tokens.Next<TokenIntegerLiteral>("data");
        }

        while (executor.NextBuilderField(ref tokens, out TokenBuilderIdentifier builderIdentifier))
        {
            string builderField = builderIdentifier.BuilderField;

            switch (builderField)
            {
                case "AT":
                    int intSlot = tokens.Next<TokenIntegerLiteral>("slot").number;
                    if (intSlot is < byte.MinValue or > byte.MaxValue)
                        throw new StatementException(tokens,
                            $"Slot number must be between {byte.MinValue} and {byte.MaxValue}");
                    slot = (byte) intSlot;
                    break;
                case "KEEP":
                    keep = true;
                    break;
                case "LOCKINVENTORY":
                    lockInventory = true;
                    break;
                case "LOCKSLOT":
                    lockSlot = true;
                    break;
                case "CANPLACEON":
                    canPlaceOn.Add(tokens.Next<TokenStringLiteral>("can place on block"));
                    break;
                case "CANDESTROY":
                    canDestroy.Add(tokens.Next<TokenStringLiteral>("can destroy block"));
                    break;
                case "ENCHANT":
                    var enchantment = tokens.NextEnum<Enchantment>("enchantment");
                    int level = tokens.Next<TokenIntegerLiteral>("level");
                    if (level < 1)
                        throw new StatementException(tokens, "Enchantment level cannot be less than 1.");
                    enchants.Add(new Tuple<Enchantment, int>(enchantment, level));
                    break;
                case "NAME":
                    displayName = tokens.Next<TokenStringLiteral>("display name");
                    break;
                case "LORE":
                    loreLines.Add(tokens.Next<TokenStringLiteral>("lore line"));
                    break;
                case "TITLE":
                    if (!itemName.Equals("minecraft:written_book", StringComparison.OrdinalIgnoreCase))
                        throw new StatementException(tokens,
                            "Property 'title' can only be used on the item 'written_book'.");
                    book ??= new ItemTagBookData();
                    ItemTagBookData bookData0 = book.Value;
                    bookData0.title = tokens.Next<TokenStringLiteral>("title");
                    book = bookData0;
                    break;
                case "AUTHOR":
                    if (!itemName.Equals("minecraft:written_book", StringComparison.OrdinalIgnoreCase))
                        throw new StatementException(tokens,
                            "Property 'author' can only be used on the item 'written_book'.");
                    book ??= new ItemTagBookData();
                    ItemTagBookData bookData1 = book.Value;
                    bookData1.author = tokens.Next<TokenStringLiteral>("author");
                    book = bookData1;
                    break;
                case "PAGE":
                    if (!itemName.Equals("minecraft:written_book", StringComparison.OrdinalIgnoreCase))
                        throw new StatementException(tokens,
                            "Property 'page' can only be used on the item 'written_book'.");
                    book ??= new ItemTagBookData();
                    bookPages ??= [];
                    bookPages.Add(tokens.Next<TokenStringLiteral>("page contents").text.Replace("\\n", "\n"));
                    break;
                case "DYE" when itemName.StartsWith("minecraft:leather_", StringComparison.OrdinalIgnoreCase):
                    if (!itemName.StartsWith("minecraft:leather_", StringComparison.OrdinalIgnoreCase))
                        throw new StatementException(tokens, "Property 'dye' can only be used on leather items.");
                    color = new ItemTagCustomColor
                    {
                        r = (byte) tokens.Next<TokenIntegerLiteral>("red"),
                        g = (byte) tokens.Next<TokenIntegerLiteral>("green"),
                        b = (byte) tokens.Next<TokenIntegerLiteral>("blue")
                    };
                    break;
                default:
                    throw new StatementException(tokens, $"Invalid property for item: '{builderIdentifier.word}'");
            }
        }

        if (bookPages != null)
        {
            ItemTagBookData bookData = book.Value;
            bookData.pages = bookPages.ToArray();
            book = bookData;
        }

        if (executor.emission.isLinting)
            return;

        var item = new ItemStack
        {
            id = itemName,
            count = count,
            damage = data,
            keep = keep,
            lockMode = lockInventory ? ItemLockMode.LOCK_IN_INVENTORY :
                lockSlot ? ItemLockMode.LOCK_IN_SLOT : ItemLockMode.NONE,
            displayName = displayName,
            lore = loreLines.ToArray(),
            enchantments = enchants.Select(e => new EnchantmentEntry(e.Item1, e.Item2)).ToArray(),
            canPlaceOn = canPlaceOn.ToArray(),
            canDestroy = canDestroy.ToArray(),
            bookData = book,
            customColor = color
        };
        var itemNbt = new ItemNBT(item, slot);
        container.AddItem(itemNbt);
    }
    public void DirectiveSign(Executor executor, Statement tokens)
    {
        int x = tokens.Next<TokenCoordinateLiteral>("x").GetNumberInt();
        int y = tokens.Next<TokenCoordinateLiteral>("y").GetNumberInt();
        int z = tokens.Next<TokenCoordinateLiteral>("z").GetNumberInt();

        string signBlock = tokens.Next<TokenStringLiteral>("block");
        signBlock = Command.Util.RequireNamespace(signBlock);
        signBlock.ThrowIfWhitespace("block", tokens);

        if (!signBlock.Contains("sign", StringComparison.OrdinalIgnoreCase))
            throw new StatementException(tokens, $"The given block ('{signBlock}') must be a valid type of sign.");

        BlockState[] blockStates = tokens.Next<TokenBlockStatesLiteral>("block states");

        // compatibility for the jacked-up metadata of hanging signs.
        // they require `attached_bit` and `ground_sign_direction` to be present, even though they don't do anything.
        if (signBlock.Contains("_hanging_sign", StringComparison.OrdinalIgnoreCase))
        {
            const string GROUND_SIGN_DIRECTION = "ground_sign_direction";
            const string ATTACHED_BIT = "attached_bit";

            if (!blockStates.Any(s =>
                    s.propertyName.Equals(GROUND_SIGN_DIRECTION, StringComparison.OrdinalIgnoreCase)))
            {
                BlockPropertyDefinition property = VanillaBlockProperties.GetProperty(GROUND_SIGN_DIRECTION);
                blockStates = [..blockStates, new BlockState(property, 0)];
            }

            if (!blockStates.Any(s =>
                    s.propertyName.Equals(ATTACHED_BIT, StringComparison.OrdinalIgnoreCase)))
            {
                BlockPropertyDefinition property = VanillaBlockProperties.GetProperty(ATTACHED_BIT);
                blockStates = [..blockStates, new BlockState(property, false)];
            }
        }

        blockStates.ValidateIfKnownVanillaBlock(signBlock, true, tokens);

        bool isEditable = true;
        if (tokens.NextIs<TokenBooleanLiteral>(false))
            isEditable = tokens.Next<TokenBooleanLiteral>("is editable?").boolean;
        string signText = tokens.Next<TokenStringLiteral>("sign text");

        // I really can't be asked to write escape code for this. Here's some simple code that makes linebreaks work.
        signText = signText.Replace("\\n", "\n").Replace("~LINEBREAK~", "\n");

        var sign = new Block(x, y, z, new PaletteEntryNBT(signBlock, blockStates.ToNBT()),
            new SignBlockEntityDataNBT(x, y, z, isEditable, signText, string.Empty)
        );

        if (!executor.emission.isLinting)
            this.blocks.Add(sign);
    }
    public void DirectiveCommandBlock(Executor executor, Statement tokens)
    {
        int x = tokens.Next<TokenCoordinateLiteral>("x").GetNumberInt();
        int y = tokens.Next<TokenCoordinateLiteral>("y").GetNumberInt();
        int z = tokens.Next<TokenCoordinateLiteral>("z").GetNumberInt();

        var commandBlockType = tokens.NextEnum<CommandBlockType>("command block type");

        bool alwaysActive = commandBlockType == CommandBlockType.chain;
        if (commandBlockType == CommandBlockType.repeating)
            alwaysActive = tokens.Next<TokenBooleanLiteral>("always active?").boolean;

        int delay = 0;
        if (tokens.NextIs<TokenIntegerLiteral>(false))
        {
            delay = tokens.Next<TokenIntegerLiteral>("tick delay").Scaled(IntMultiplier.none);
            if (delay < 0)
                throw new StatementException(tokens, "Tick delay cannot be less than 0.");
        }

        var facingDirection = tokens.NextEnum<Block.FacingDirection>("facing direction");

        bool conditional = false;
        if (tokens.NextIs<TokenBooleanLiteral>(false))
            conditional = tokens.Next<TokenBooleanLiteral>("conditional").boolean;

        string command = tokens.Next<TokenStringLiteral>("command").text;
        if (command.StartsWith('/'))
            command = command[1..];

        if (executor.emission.isLinting)
            return;

        var commandBlock = new Block(x, y, z, new PaletteEntryNBT(
                CommandBlockEntityDataNBT.GetCommandBlockIdentifier(commandBlockType), [
                    new NBTByte {name = "conditional_bit", value = conditional ? (byte) 1 : (byte) 0},
                    new NBTInt {name = "facing_direction", value = (int) facingDirection}
                ]), new CommandBlockEntityDataNBT(command, false, x, y, z)
            {
                type = commandBlockType,
                alwaysActive = alwaysActive,
                isConditional = conditional,
                tickDelay = delay
            }
        );
        this.blocks.Add(commandBlock);
    }

    public class ContainerBuilder(Block block)
    {
        private readonly List<ItemNBT> _items = [];
        private int LowestAvailableSlot
        {
            get
            {
                if (this._items == null || this._items.Count == 0)
                    return 0;

                List<byte> usedSlots = this._items
                    .Where(s => s.slot.HasValue)
                    .Select(s => s.slot.Value)
                    .OrderBy(s => s)
                    .ToList();

                // find first gap in sequence starting from 0
                for (int i = 0; i < usedSlots.Count; i++)
                    if (usedSlots[i] != i)
                        return i;

                // if no gaps are found, return the next available slot
                return usedSlots.Count;
            }
        }
        /// <summary>
        ///     Adds the specified <see cref="ItemNBT" /> to the container.
        /// </summary>
        /// <param name="item">
        ///     The <see cref="ItemNBT" /> instance to add. If no slot is specified, the lowest available slot in
        ///     the container will be assigned.
        /// </param>
        public void AddItem(ItemNBT item)
        {
            item.slot ??= (byte) this.LowestAvailableSlot;
            this._items.Add(item);
        }

        private ContainerBlockEntityDataNBT CreateContainerNBT()
        {
            // get the block entity identifier for the container
            string blockEntityIdentifier =
                CommonBlockEntityIdentifiers.ConvertBlockToBlockEntity(block.paletteEntry.name);

            var container = new ContainerBlockEntityDataNBT(blockEntityIdentifier,
                block.position.x, block.position.y, block.position.z);
            container.AddItems(this._items);
            return container;
        }

        /// <summary>
        ///     Creates a block using the specified container properties and state configuration.
        /// </summary>
        /// <returns>The constructed <see cref="Block" /> instance based on the builder's configuration.</returns>
        public Block Build()
        {
            return new Block(block.position.x, block.position.y, block.position.z,
                block.paletteEntry,
                CreateContainerNBT()
            );
        }
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

        public static Block Void(int x, int y, int z)
        {
            return new Block(x, y, z, new PaletteEntryNBT()) {isVoid = true};
        }
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
                type = CommandBlockType.impulse
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
                type = CommandBlockType.chain
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
                type = CommandBlockType.chain
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
                type = CommandBlockType.repeating
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
                type = CommandBlockType.repeating
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
        ///     Converts a <see cref="CardinalDirection" /> to its corresponding <see cref="FacingDirection" />.
        /// </summary>
        /// <param name="direction">The <see cref="CardinalDirection" /> to convert.</param>
        /// <param name="callingStatement">The <see cref="Statement" /> that invoked this method, used for error context.</param>
        /// <returns>The equivalent <see cref="FacingDirection" /> for the provided <paramref name="direction" />.</returns>
        /// <exception cref="StatementException">
        ///     Thrown if the provided <paramref name="direction" /> is invalid or unrecognized.
        /// </exception>
        public static FacingDirection CardinalToFacingDirection(CardinalDirection direction, Statement callingStatement)
        {
            return direction switch
            {
                CardinalDirection.north => FacingDirection.north,
                CardinalDirection.south => FacingDirection.south,
                CardinalDirection.east => FacingDirection.east,
                CardinalDirection.west => FacingDirection.west,
                _ => throw new StatementException(callingStatement, "Unknown cardinal direction: " + direction)
            };
        }
        /// <summary>
        ///     Converts a <see cref="FacingDirection" /> to its corresponding <see cref="CardinalDirection" />.
        /// </summary>
        /// <param name="direction">The <see cref="FacingDirection" /> to convert.</param>
        /// <param name="callingStatement">The <see cref="Statement" /> that initiated the conversion, used for error context.</param>
        /// <returns>The equivalent <see cref="CardinalDirection" /> for the given <paramref name="direction" />.</returns>
        /// <exception cref="StatementException">
        ///     Thrown when <paramref name="direction" /> is <see cref="FacingDirection.down" /> or
        ///     <see cref="FacingDirection.up" />
        ///     or if an unknown value is provided.
        /// </exception>
        public static CardinalDirection FacingToCardinalDirection(FacingDirection direction, Statement callingStatement)
        {
            return direction switch
            {
                FacingDirection.down => throw new StatementException(callingStatement,
                    "Cannot convert 'down' to a cardinal direction"),
                FacingDirection.up => throw new StatementException(callingStatement,
                    "Cannot convert 'up' to a cardinal direction"),
                FacingDirection.negativez => CardinalDirection.north,
                FacingDirection.positivez => CardinalDirection.south,
                FacingDirection.negativex => CardinalDirection.west,
                FacingDirection.positivex => CardinalDirection.east,
                _ => throw new StatementException(callingStatement, "Unknown facing direction: " + direction)
            };
        }

        /// <summary>
        ///     An integer representing a facing direction of a block.
        /// </summary>
        [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
        [UsableInMCC]
        public enum FacingDirection
        {
            down = 0,
            up = 1,
            negativez = 2,
            north = 2,
            positivez = 3,
            south = 3,
            negativex = 4,
            west = 4,
            positivex = 5,
            east = 5
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