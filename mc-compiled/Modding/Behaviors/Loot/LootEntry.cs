using System.Collections.Generic;
using System.Linq;
using mc_compiled.Commands;
using mc_compiled.Commands.Native;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Loot;

/// <summary>
///     Represents an item entry in a loot pool.
/// </summary>
public class LootEntry(LootEntry.EntryType type, string name, int weight = 1, params LootFunction[] functions)
{
    public enum EntryType
    {
        item
    }

    public readonly List<LootFunction> functions = [..functions];

    public readonly string name = Command.Util.RequireNamespace(name);
    public readonly EntryType type = type;
    public readonly int weight = weight;

    /// <summary>
    ///     Add a function to this loot entry.
    /// </summary>
    /// <param name="function">The function to add.</param>
    /// <returns><c>this</c> for chaining convenience.</returns>
    public LootEntry WithFunction(LootFunction function)
    {
        this.functions.Add(function);
        return this;
    }
    /// <summary>
    ///     Add multiple functions to this loot entry.
    /// </summary>
    /// <param name="functionsToAdd">The functions to add.</param>
    /// <returns><c>this</c> for chaining convenience.</returns>
    public LootEntry WithFunctions(params LootFunction[] functionsToAdd)
    {
        this.functions.AddRange(functionsToAdd);
        return this;
    }

    /// <summary>
    ///     Constructs a <see cref="LootEntry" /> from an <see cref="ItemStack" />. Adds functions as needed to mimic the
    ///     stack's properties.
    /// </summary>
    /// <remarks>
    ///     <b>NOTE! Unsupported fields include:</b>
    ///     <ul>
    ///         <li>
    ///             <see cref="ItemStack.damage" />
    ///         </li>
    ///         <li>
    ///             <see cref="ItemStack.keep" />
    ///         </li>
    ///         <li>
    ///             <see cref="ItemStack.canPlaceOn" />
    ///         </li>
    ///         <li>
    ///             <see cref="ItemStack.canDestroy" />
    ///         </li>
    ///         <li>
    ///             <see cref="ItemStack.lockMode" />
    ///         </li>
    ///         <li>
    ///             <see cref="ItemStack.customColor" />
    ///         </li>
    ///     </ul>
    /// </remarks>
    /// <param name="stack">The <see cref="ItemStack" /> to clone.</param>
    /// <param name="weight">The weight of the entry, defaults to 1.</param>
    /// <returns>The created <see cref="LootEntry" /> that mimics the input <paramref name="stack" />.</returns>
    public static LootEntry FromItemStack(ItemStack stack, int weight = 1)
    {
        string itemName = stack.id;
        var entry = new LootEntry(EntryType.item, itemName, weight);

        if (stack.count > 1)
            entry.functions.Add(new LootFunctionCount(stack.count));
        if (!string.IsNullOrEmpty(stack.displayName))
            entry.functions.Add(new LootFunctionName(stack.displayName));
        if (stack.lore is {Length: > 0})
            entry.functions.Add(new LootFunctionLore(stack.lore));
        if (stack.enchantments is {Length: > 0})
            entry.functions.Add(new LootFunctionEnchant(stack.enchantments));
        if (stack.bookData.HasValue)
            entry.functions.Add(LootFunctionBook.FromNBT(stack.bookData.Value));
        return entry;
    }

    public JObject ToJSON()
    {
        var json = new JObject
        {
            ["type"] = this.type.ToString(),
            ["name"] = this.name,
            ["weight"] = this.weight
        };

        if (this.functions.Count > 0)
        {
            var jsonFunctions = new JArray(this.functions.Select(f => f.ToJSON()));
            json["functions"] = jsonFunctions;
        }

        return json;
    }
}