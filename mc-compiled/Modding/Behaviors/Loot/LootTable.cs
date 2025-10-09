using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Loot;

/// <summary>
///     Represents a loot table.
/// </summary>
public sealed class LootTable(string name, string subdirectory = null) : IAddonFile
{
    public string name = name;
    public List<LootPool> pools = [];
    public string subdirectory = subdirectory;

    /// <summary>
    ///     Get the path meant to be used in a command.
    /// </summary>
    public string CommandPath => Path.Combine(GetExtendedDirectory(), GetOutputFile());
    /// <summary>
    ///     Get the path used in JSON to reference this loot table.
    /// </summary>
    public string ResourcePath => Path.Combine("loot_tables", GetExtendedDirectory(), GetOutputFile());

    public string CommandReference
    {
        get
        {
            if (this.subdirectory == null)
                return this.name;

            return this.subdirectory + '/' + this.name;
        }
    }
    public byte[] GetOutputData()
    {
        var jsonPools = new JArray(this.pools.Select(pool => pool.ToJSON()));
        var root = new JObject
        {
            ["pools"] = jsonPools
        };
        return Encoding.UTF8.GetBytes(root.ToString());
    }
    public string GetExtendedDirectory() { return this.subdirectory; }
    public string GetOutputFile() { return this.name + ".json"; }
    public OutputLocation GetOutputLocation() { return OutputLocation.b_LOOT_TABLES; }
}