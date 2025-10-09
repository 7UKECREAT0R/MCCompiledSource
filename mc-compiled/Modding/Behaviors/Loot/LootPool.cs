using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Loot;

/// <summary>
///     Represents a pool in a loot table, with weighted items.
/// </summary>
public class LootPool
{
    public readonly List<LootEntry> entries;
    public readonly int rollsMax;
    public readonly int rollsMin;
    public readonly bool useRandomRolls;

    public LootPool(int rolls, params LootEntry[] entries)
    {
        this.rollsMin = rolls;
        this.rollsMax = 0;
        this.useRandomRolls = false;

        if (entries == null || entries.Length < 1)
            this.entries = [];
        else
            this.entries = [..entries];
    }
    public LootPool(int rollsMin, int rollsMax, params LootEntry[] entries)
    {
        this.rollsMin = rollsMin;
        this.rollsMax = rollsMax;
        this.useRandomRolls = true;

        if (entries == null || entries.Length < 1)
            this.entries = [];
        else
            this.entries = [..entries];
    }
    public LootPool WithEntry(LootEntry entry)
    {
        this.entries.Add(entry);
        return this;
    }

    /// <summary>
    ///     Creates a loot pool with a single entry that's the only guaranteed roll in the pool.
    /// </summary>
    /// <param name="item">The identifier of the item to be present in the pool.</param>
    /// <returns>
    ///     A tuple containing both the newly created <see cref="LootPool" />, and a reference to its only item for
    ///     convenience.
    /// </returns>
    public static (LootPool pool, LootEntry entry) CreateWithSingleEntry(string item)
    {
        var entry = new LootEntry(LootEntry.EntryType.item, item);
        var pool = new LootPool(1, entry);
        return (pool, entry);
    }

    public JObject ToJSON()
    {
        var pool = new JArray(this.entries.Select(entry => entry.ToJSON()));
        if (this.useRandomRolls)
            return new JObject
            {
                ["rolls"] = new JObject
                {
                    ["min"] = this.rollsMin,
                    ["max"] = this.rollsMax
                },
                ["entries"] = pool
            };

        return new JObject
        {
            ["rolls"] = this.rollsMin,
            ["entries"] = pool
        };
    }
}