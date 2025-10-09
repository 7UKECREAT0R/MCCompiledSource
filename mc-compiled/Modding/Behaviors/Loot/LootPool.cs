using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Loot;

/// <summary>
///     Represents a pool in a loot table. Use LootPoolWeighted or LootPoolTiered.
/// </summary>
public class LootPool
{
    public readonly List<LootEntry> entries;
    public readonly int? rollsMax;
    public readonly int rollsMin;

    public LootPool(int rolls, params LootEntry[] entries)
    {
        this.rollsMin = rolls;
        this.rollsMax = null;

        if (entries == null || entries.Length < 1)
            this.entries = [];
        else
            this.entries = [..entries];
    }
    public LootPool(int rollsMin, int rollsMax, params LootEntry[] entries)
    {
        this.rollsMin = rollsMin;
        this.rollsMax = rollsMax;

        if (entries == null || entries.Length < 1)
            this.entries = [];
        else
            this.entries = [..entries];
    }
    public void AddLoot(LootEntry entry) { this.entries.Add(entry); }

    public JObject ToJSON()
    {
        var pool = new JArray(this.entries.Select(entry => entry.ToJSON()));
        if (this.rollsMax.HasValue)
            return new JObject
            {
                ["rolls"] = new JObject
                {
                    ["min"] = this.rollsMin,
                    ["max"] = this.rollsMax.Value
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