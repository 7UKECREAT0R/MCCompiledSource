using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.Commands;
using mc_compiled.Commands.Native;
using mc_compiled.Json;
using mc_compiled.NBT;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Loot;

/// <summary>
///     Sets the count of the item.
/// </summary>
public sealed class LootFunctionCount : LootFunction
{
    public readonly int max;
    public readonly int min;
    public readonly bool useRandomCount;
    public LootFunctionCount(int count)
    {
        this.min = count;
        this.max = 0;
        this.useRandomCount = false;
    }
    public LootFunctionCount(int min, int max)
    {
        this.min = min;
        this.max = max;
        this.useRandomCount = true;
    }

    public override JObject[] GetFunctionFields()
    {
        if (this.useRandomCount)
            return
            [
                new JObject
                {
                    ["count"] = new JObject
                    {
                        ["min"] = this.min,
                        ["max"] = this.max
                    }
                }
            ];

        return
        [
            new JObject
            {
                ["count"] = this.min
            }
        ];
    }
    public override string GetFunctionName() { return "set_count"; }
}

/// <summary>
///     Sets the display name of the item.
/// </summary>
public sealed class LootFunctionName(string name) : LootFunction
{
    public readonly string name = name;

    public override JObject[] GetFunctionFields()
    {
        return
        [
            new JObject
            {
                ["name"] = this.name
            }
        ];
    }
    public override string GetFunctionName() { return "set_name"; }
}

/// <summary>
///     Sets the lore lines attached to the item.
/// </summary>
public sealed class LootFunctionLore(params string[] lore) : LootFunction
{
    public readonly string[] lore = lore;

    public LootFunctionLore(IEnumerable<string> lore) : this() { this.lore = lore.ToArray(); }
    public override JObject[] GetFunctionFields()
    {
        return
        [
            new JObject
            {
                ["lore"] = new JArray(this.lore.Cast<object>().ToArray())
            }
        ];
    }
    public override string GetFunctionName() { return "set_lore"; }
}

/// <summary>
///     Sets the data of the item.
/// </summary>
public sealed class LootFunctionData : LootFunction
{
    public readonly int dataMax;
    public readonly int dataMin;
    public readonly bool useRandomData;

    public LootFunctionData(int data)
    {
        this.dataMin = data;
        this.dataMax = 0;
        this.useRandomData = false;
    }
    public LootFunctionData(int dataMin, int dataMax)
    {
        this.dataMin = dataMin;
        this.dataMax = dataMax;
        this.useRandomData = true;
    }

    public override JObject[] GetFunctionFields()
    {
        if (this.useRandomData)
            return
            [
                new JObject
                {
                    ["data"] = new JObject
                    {
                        ["min"] = this.dataMin,
                        ["max"] = this.dataMax
                    }
                }
            ];

        return
        [
            new JObject
            {
                ["data"] = this.dataMin
            }
        ];
    }
    public override string GetFunctionName() { return "set_data"; }
}

/// <summary>
///     Sets a random/static block state. The block state MUST be an <see cref="BlockPropertyType.@int" /> type.
/// </summary>
public sealed class LootFunctionBlockState : LootFunction
{
    public readonly BlockPropertyDefinition state;
    public readonly bool useRandomValue;
    public readonly int valueMax;
    public readonly int valueMin;

    /// <summary>
    ///     Sets a static block state. The block state MUST be an <see cref="BlockPropertyType.@int" /> type.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     If the <paramref name="state" /> property does not have the type
    ///     <see cref="BlockPropertyType.@int" />.
    /// </exception>
    public LootFunctionBlockState(BlockPropertyDefinition state, int value)
    {
        if (state.Type != BlockPropertyType.@int)
            throw new ArgumentException("Block state must be an int type", nameof(state));
        this.state = state;
        this.useRandomValue = false;
        this.valueMin = value;
        this.valueMax = 0;
    }
    /// <summary>
    ///     Sets a random block state. The block state MUST be an <see cref="BlockPropertyType.@int" /> type.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     If the <paramref name="state" /> property does not have the type
    ///     <see cref="BlockPropertyType.@int" />.
    /// </exception>
    public LootFunctionBlockState(BlockPropertyDefinition state, int valueMin, int valueMax)
    {
        if (state.Type != BlockPropertyType.@int)
            throw new ArgumentException("Block state must be an int type", nameof(state));
        this.state = state;
        this.useRandomValue = true;
        this.valueMin = valueMin;
        this.valueMax = valueMax;
    }

    public override JObject[] GetFunctionFields()
    {
        if (this.useRandomValue)
            return
            [
                new JObject
                {
                    ["values"] = new JObject
                    {
                        ["min"] = this.valueMin,
                        ["max"] = this.valueMax
                    }
                }
            ];

        return
        [
            new JObject
            {
                ["values"] = this.valueMin
            }
        ];
    }
    public override string GetFunctionName() { return "random_block_state"; }
}

/// <summary>
///     Sets the durability of the item. 0 is max damage and 1 is pristine.
/// </summary>
public sealed class LootFunctionDurability : LootFunction
{
    public float? maxDurability;
    public float minDurability;

    /// <summary>
    ///     0 is max damage and 1 is pristine.
    /// </summary>
    /// <param name="durability"></param>
    public LootFunctionDurability(float durability)
    {
        this.minDurability = durability;
        this.maxDurability = null;
    }
    /// <summary>
    ///     0 is max damage and 1 is pristine.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public LootFunctionDurability(float min, float max)
    {
        this.minDurability = min;
        this.maxDurability = max;
    }

    public override JObject[] GetFunctionFields()
    {
        if (this.maxDurability.HasValue)
            return
            [
                new JObject
                {
                    ["damage"] = new JObject
                    {
                        ["min"] = this.minDurability,
                        ["max"] = this.maxDurability.Value
                    }
                }
            ];

        return
        [
            new JObject
            {
                ["damage"] = this.minDurability
            }
        ];
    }
    public override string GetFunctionName() { return "set_damage"; }
}

/// <summary>
///     Sets the entity associated with the item, given it's a spawn egg.
/// </summary>
/// <param name="entity">The entity to set the spawn egg to.</param>
public sealed class LootFunctionSetSpawnEggEntity(string entity) : LootFunction
{
    public readonly string entity = Command.Util.RequireNamespace(entity);

    public override JObject[] GetFunctionFields()
    {
        return
        [
            new JObject
            {
                ["id"] = this.entity
            }
        ];
    }
    public override string GetFunctionName() { return "set_actor_id"; }
}

/// <summary>
///     Only works when this is an entity loot table and the item is a spawn egg.
///     Sets the spawn egg's entity to the same as the entity that was killed to spawn the loot.
/// </summary>
public sealed class LootFunctionSetSpawnEggInherit : LootFunction
{
    public override JObject[] GetFunctionFields() { return []; }
    public override string GetFunctionName() { return "set_actor_id"; }
}

/// <summary>
///     Sets book contents if it's a book.
/// </summary>
public sealed class LootFunctionBook : LootFunction
{
    public const string ITEM_ID = "minecraft:written_book";

    public const int MAX_PAGES = 50;
    public const int MAX_CHARS_PER_PAGE = 798;
    public const int MAX_CHARS = 12800;
    public string author;
    public string[] pages;
    public string title;

    /// <summary>
    ///     Sets book contents if it's a book. The pages are in string format.
    /// </summary>
    /// <param name="title">The title of the book.</param>
    /// <param name="author">The name of the book's author.</param>
    /// <param name="pages">
    ///     The pages inside the book in text format. Pages do support rawtext in string format, but you should
    ///     use the other overload of this constructor instead for clarity.
    /// </param>
    public LootFunctionBook(string title, string author, params string[] pages)
    {
        this.title = title;
        this.author = author;
        this.pages = pages;
    }
    /// <summary>
    ///     Sets book contents if it's a book. The pages are in JSON rawtext format.
    /// </summary>
    /// <param name="title">The title of the book.</param>
    /// <param name="author">The name of the book's author.</param>
    /// <param name="pages">The pages inside the book in rawtext format.</param>
    public LootFunctionBook(string title, string author, params RawText[] pages)
    {
        this.title = title;
        this.author = author;
        this.pages = pages.Select(p => p.BuildString()).ToArray();
    }

    /// <summary>
    ///     Creates a new <see cref="LootFunctionBook" /> instance from the given NBT book data.
    /// </summary>
    /// <param name="nbt">The NBT data representing the book.</param>
    /// <returns>A new <see cref="LootFunctionBook" /> initialized with the NBT book data.</returns>
    public static LootFunctionBook FromNBT(ItemTagBookData nbt)
    {
        return new LootFunctionBook(nbt.title, nbt.author, nbt.pages);
    }

    public override JObject[] GetFunctionFields()
    {
        return
        [
            new JObject {["title"] = this.title},
            new JObject {["author"] = this.author},
            new JObject {["pages"] = new JArray(this.pages.Cast<object>().ToArray())}
        ];
    }
    public override string GetFunctionName() { return "set_book_contents"; }
}

/// <summary>
///     Randomly dyes the item if it is leather armor.
/// </summary>
public sealed class LootFunctionRandomDye : LootFunction
{
    public override JObject[] GetFunctionFields() { return []; }
    public override string GetFunctionName() { return "random_dye"; }
}

/// <summary>
///     Give specific enchants to the item.
/// </summary>
public sealed class LootFunctionEnchant(params EnchantmentEntry[] enchantments) : LootFunction
{
    public readonly List<EnchantmentEntry> enchantments = [..enchantments];

    public LootFunctionEnchant(IEnumerable<EnchantmentEntry> enchantments) : this()
    {
        this.enchantments = enchantments.ToList();
    }
    public override JObject[] GetFunctionFields()
    {
        JArray json = [];
        this.enchantments.ForEach(e => json.Add(new JObject
        {
            ["id"] = (short) e.id,
            ["level"] = e.level
        }));
        return
        [
            new JObject
            {
                ["enchants"] = json
            }
        ];
    }
    public override string GetFunctionName() { return "specific_enchants"; }
}

/// <summary>
///     Simulates enchanting on an enchantment table.
/// </summary>
public sealed class LootFunctionSimulateEnchant(int minLevel, int maxLevel, bool treasure = false) : LootFunction
{
    public readonly int maxLevel = maxLevel;
    public readonly int minLevel = minLevel;
    /// <summary>
    ///     If this should include "treasure" enchants (curses, soul speed, frost walker, etc...)
    /// </summary>
    public readonly bool treasure = treasure;

    public override JObject[] GetFunctionFields()
    {
        return
        [
            new JObject {["treasure"] = this.treasure},
            new JObject
            {
                ["min"] = this.minLevel,
                ["max"] = this.maxLevel
            }
        ];
    }
    public override string GetFunctionName() { return "enchant_with_levels"; }
}

/// <summary>
///     Puts a random compatible enchant on this item.
/// </summary>
public sealed class LootFunctionRandomEnchant(bool treasure = false) : LootFunction
{
    /// <summary>
    ///     If this should include "treasure" enchants (curses, soul speed, frost walker, etc...)
    /// </summary>
    public readonly bool treasure = treasure;

    public override JObject[] GetFunctionFields()
    {
        return
        [
            new JObject {["treasure"] = this.treasure}
        ];
    }
    public override string GetFunctionName() { return "enchant_randomly"; }
}

/// <summary>
///     Puts a random gear enchantment on this, if armor. Algorithm used by randomly spawning mobs.
/// </summary>
public sealed class LootFunctionRandomEnchantGear(float chance) : LootFunction
{
    // Difficulty can affect this. 2.0 will always enchant even on normal mode.
    public readonly float chance = chance;

    public override JObject[] GetFunctionFields()
    {
        return
        [
            new JObject {["chance"] = this.chance}
        ];
    }
    public override string GetFunctionName() { return "enchant_random_gear"; }
}