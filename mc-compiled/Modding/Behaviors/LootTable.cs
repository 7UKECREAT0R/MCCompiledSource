using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using mc_compiled.Commands.Native;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors
{
    /// <summary>
    /// Represents a loot table.
    /// </summary>
    public sealed class LootTable(string name, string subdirectory = null) : IAddonFile
    {
        public string name = name;
        public string subdirectory = subdirectory;
        public List<LootPool> pools = [];

        public string CommandReference
        {
            get
            {
                if (this.subdirectory == null)
                    return this.name;

                return this.subdirectory + '/' + this.name;
            }
        }

        /// <summary>
        /// Get the path meant to be used in a command.
        /// </summary>
        public string CommandPath
        {
            get => Path.Combine(GetExtendedDirectory(), GetOutputFile());
        }
        /// <summary>
        /// Get the path used in JSON to reference this loot table.
        /// </summary>
        public string ResourcePath
        {
            get => Path.Combine("loot_tables", GetExtendedDirectory(), GetOutputFile());
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
        public string GetExtendedDirectory() => this.subdirectory;
        public string GetOutputFile() => this.name + ".json";
        public OutputLocation GetOutputLocation() =>
            OutputLocation.b_LOOT_TABLES;
    }

    /// <summary>
    /// Represents a pool in a loot table. Use LootPoolWeighted or LootPoolTiered.
    /// </summary>
    public class LootPool
    {
        public readonly int rollsMin;
        public readonly int? rollsMax;
        public readonly List<LootEntry> entries;

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
        public void AddLoot(LootEntry entry) => this.entries.Add(entry);

        public JObject ToJSON()
        {
            JArray pool = new JArray(this.entries.Select(entry => entry.ToJSON()));
            if (this.rollsMax.HasValue)
            {
                return new JObject
                {
                    ["rolls"] = new JObject
                    {
                        ["min"] = this.rollsMin,
                        ["max"] = this.rollsMax.Value
                    },
                    ["entries"] = pool
                };
            }

            return new JObject
            {
                ["rolls"] = this.rollsMin,
                ["entries"] = pool
            };
        }
    }

    /// <summary>
    /// Represents an item drop in a loot pool.
    /// </summary>
    public class LootEntry(LootEntry.EntryType type, string name, int weight = 1, params LootFunction[] functions)
    {
        public enum EntryType
        {
            item
        }

        public readonly EntryType type = type;
        public readonly string name = name;
        public readonly int weight = weight;
        public List<LootFunction> functions = [..functions];

        /// <summary>
        /// Add a function to this loot entry.
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
            JObject json = new JObject
            {
                ["type"] = this.type.ToString(),
                ["name"] = this.name,
                ["weight"] = this.weight
            };

            if (this.functions.Any())
            {
                JArray jsonFunctions = new JArray(this.functions.Select(f => f.ToJSON()));
                json["functions"] = jsonFunctions;
            }

            return json;
        }
    }

    /// <summary>
    /// Function which will run on loot before being dropped.
    /// </summary>
    public abstract class LootFunction
    {
        /// <summary>
        /// Convert this function to JSON.
        /// </summary>
        /// <returns></returns>
        public JObject ToJSON()
        {
            JObject json = new JObject();
            json["function"] = GetFunctionName();
            foreach (JObject function in GetFunctionFields())
            {
                foreach (JProperty property in function.Properties())
                    json.Add(property);
            }

            return json;
        }

        public abstract string GetFunctionName();
        public abstract JObject[] GetFunctionFields();
    }

    /// <summary>
    /// Sets the count of the item.
    /// </summary>
    public sealed class LootFunctionCount : LootFunction
    {
        public readonly int min;
        public readonly int? max;
        public LootFunctionCount(int count)
        {
            this.min = count;
            this.max = null;
        }
        public LootFunctionCount(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public override JObject[] GetFunctionFields()
        {
            if (this.max.HasValue)
            {
                return
                [
                    new JObject
                    {
                        ["count"] = new JObject
                        {
                            ["min"] = this.min,
                            ["max"] = this.max.Value
                        }
                    }
                ];
            }
            else
            {
                return
                [
                    new JObject
                    {
                        ["count"] = this.min
                    }
                ];
            }
        }
        public override string GetFunctionName() =>
            "set_count";
    }

    /// <summary>
    /// Sets the display name of the item.
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
        public override string GetFunctionName() =>
            "set_name";
    }

    /// <summary>
    /// Sets the lore of the item.
    /// </summary>
    public sealed class LootFunctionLore(params object[] lore) : LootFunction
    {
        public readonly object[] lore = lore;

        public override JObject[] GetFunctionFields()
        {
            return
            [
                new JObject
                {
                    ["lore"] = new JArray(this.lore)
                }
            ];
        }
        public override string GetFunctionName() =>
            "set_lore";
    }

    /// <summary>
    /// Sets the data of the item.
    /// </summary>
    public sealed class LootFunctionData : LootFunction
    {
        public bool block; // if this item is a block
        public int dataMin;
        public int? dataMax;

        public LootFunctionData(bool block, int data)
        {
            this.block = block;
            this.dataMin = data;
            this.dataMax = null;
        }
        public LootFunctionData(bool block, int dataMin, int dataMax)
        {
            this.block = block;
            this.dataMin = dataMin;
            this.dataMax = dataMax;
        }

        public override JObject[] GetFunctionFields()
        {
            if (this.dataMax.HasValue)
            {
                return
                [
                    new JObject
                    {
                        ["data"] = new JObject
                        {
                            ["min"] = this.dataMin,
                            ["max"] = this.dataMax.Value
                        }
                    }
                ];
            }
            else
            {
                return
                [
                    new JObject
                    {
                        ["data"] = this.dataMin
                    }
                ];
            }
        }
        public override string GetFunctionName() =>
            "set_data";
    }

    /// <summary>
    /// Sets the durability of the item. 0 is max damage and 1 is pristeen.
    /// </summary>
    public sealed class LootFunctionDurability : LootFunction
    {
        public float minDurability;
        public float? maxDurability;

        /// <summary>
        /// 0 is max damage and 1 is pristeen.
        /// </summary>
        /// <param name="durability"></param>
        public LootFunctionDurability(float durability)
        {
            this.minDurability = durability;
            this.maxDurability = null;
        }
        /// <summary>
        /// 0 is max damage and 1 is pristeen.
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
            {
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
            }
            else
            {
                return
                [
                    new JObject
                    {
                        ["damage"] = this.minDurability
                    }
                ];
            }
        }
        public override string GetFunctionName() =>
            "set_damage";
    }

    /// <summary>
    /// Sets book contents if it's a book.
    /// </summary>
    public sealed class LootFunctionBook : LootFunction
    {
        public const int MAX_PAGES = 50;
        public const int MAX_CHARS_PER_PAGE = 798;
        public const int MAX_CHARS = 12800;

        public string title;
        public string author;
        public string[] pages;
        public LootFunctionBook(string title, string author, params string[] pages)
        {
            this.title = title;
            this.author = author;
            this.pages = pages;

            if (pages.Length > MAX_PAGES)
                throw new Exception($"Book cannot contain more than {MAX_PAGES} pages.");
            if (pages.Any(page => page.Length > MAX_CHARS_PER_PAGE))
                throw new Exception($"A page held more than {MAX_CHARS_PER_PAGE} characters.");
            if (pages.Sum(page => page.Length) > MAX_CHARS)
                throw new Exception($"Book cannot contain more than {MAX_CHARS} characters total.");
        }

        public override JObject[] GetFunctionFields()
        {
            return
            [
                new JObject {["title"] = this.title},
                new JObject {["author"] = this.author},
                new JObject {["pages"] = new JArray(this.pages)}
            ];
        }
        public override string GetFunctionName() =>
            "set_book_contents";
    }

    /// <summary>
    /// Randomly dyes the item if it is leather armor.
    /// </summary>
    public sealed class LootFunctionRandomDye : LootFunction
    {
        public LootFunctionRandomDye() { }

        public override JObject[] GetFunctionFields()
        {
            return [];
        }
        public override string GetFunctionName() =>
            "random_dye";
    }

    /// <summary>
    /// Give specific enchants to the item.
    /// </summary>
    public sealed class LootFunctionEnchant(params EnchantmentEntry[] enchantments) : LootFunction
    {
        public readonly List<EnchantmentEntry> enchantments = [..enchantments];

        public override JObject[] GetFunctionFields()
        {
            JArray json = [];
            this.enchantments.ForEach(e => json.Add(new JObject
            {
                ["id"] = e.id,
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
        public override string GetFunctionName() =>
            "specific_enchants";
    }

    /// <summary>
    /// Simulates enchanting on an enchantment table.
    /// </summary>
    public sealed class LootFunctionSimulateEnchant(int minLevel, int maxLevel, bool treasure = false) : LootFunction
    {
        public readonly int minLevel = minLevel;
        public readonly int maxLevel = maxLevel;
        /// <summary>
        /// If this should include "treasure" enchants (curses, soul speed, frost walker, etc...)
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
        public override string GetFunctionName() =>
            "enchant_with_levels";
    }

    /// <summary>
    /// Puts a random compatible enchant on this item.
    /// </summary>
    public sealed class LootFunctionRandomEnchant(bool treasure = false) : LootFunction
    {
        /// <summary>
        /// If this should include "treasure" enchants (curses, soul speed, frost walker, etc...)
        /// </summary>
        public readonly bool treasure = treasure;

        public override JObject[] GetFunctionFields()
        {
            return
            [
                new JObject {["treasure"] = this.treasure}
            ];
        }
        public override string GetFunctionName() =>
            "enchant_randomly";
    }

    /// <summary>
    /// Puts a random gear enchantment on this, if armor. Algorithm used by randomly spawning mobs.
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
        public override string GetFunctionName() =>
            "enchant_random_gear";
    }
}