using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Loot;

/// <summary>
///     Represents an item drop in a loot pool.
/// </summary>
public class LootEntry(LootEntry.EntryType type, string name, int weight = 1, params LootFunction[] functions)
{
    public enum EntryType
    {
        item
    }

    public readonly string name = name;

    public readonly EntryType type = type;
    public readonly int weight = weight;
    public List<LootFunction> functions = [..functions];

    /// <summary>
    ///     Add a function to this loot entry.
    /// </summary>
    /// <param name="function">The function to add.</param>
    /// <returns>this for chaining convenience.</returns>
    public LootEntry WithFunction(LootFunction function)
    {
        this.functions.Add(function);
        return this;
    }

    public JObject ToJSON()
    {
        var json = new JObject
        {
            ["type"] = this.type.ToString(),
            ["name"] = this.name,
            ["weight"] = this.weight
        };

        if (this.functions.Any())
        {
            var jsonFunctions = new JArray(this.functions.Select(f => f.ToJSON()));
            json["functions"] = jsonFunctions;
        }

        return json;
    }
}