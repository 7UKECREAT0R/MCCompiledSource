using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace mc_compiled.NBT.Structures.BlockPositionData;

/// <summary>
///     Represents a block entity NBT structure for containers, such as chests or barrels, allowing for management of items
///     within the container.
/// </summary>
/// <remarks>
///     Inherits from <see cref="BasicBlockEntityDataNBT" /> to include positional data and generic block entity
///     properties.
/// </remarks>
public class ContainerBlockEntityDataNBT(string id, int x, int y, int z, bool isMovable = true)
    : BasicBlockEntityDataNBT(id, x, y, z, isMovable)
{
    private readonly List<ItemNBT> items = [];

    /// <summary>
    ///     Adds the specified <see cref="ItemNBT" /> to the container's internal item list.
    /// </summary>
    /// <param name="item">
    ///     The item to add to the container. The <paramref name="item" /> must have its
    ///     <see cref="ItemNBT.slot" /> property assigned to a valid value. If the slot value is null,
    ///     it may result in unexpected behavior during serialization.
    /// </param>
    /// <returns>
    ///     Returns the current instance of <see cref="ContainerBlockEntityDataNBT" /> to allow for method chaining.
    /// </returns>
    public ContainerBlockEntityDataNBT AddItem(ItemNBT item)
    {
        this.items.Add(item);
        return this;
    }
    /// <summary>
    ///     Adds multiple <see cref="ItemNBT" /> instances to the container's internal item list.
    /// </summary>
    /// <param name="toAdd">
    ///     A collection of items to add to the container. Each item within <paramref name="toAdd" /> must have
    ///     its <see cref="ItemNBT.slot" /> property assigned to a valid value. If any item's slot value is null,
    ///     it may result in unexpected behavior during serialization.
    /// </param>
    /// <returns>
    ///     Returns the current instance of <see cref="ContainerBlockEntityDataNBT" /> to allow for method chaining.
    /// </returns>
    public ContainerBlockEntityDataNBT AddItems(IEnumerable<ItemNBT> toAdd)
    {
        this.items.AddRange(toAdd);
        return this;
    }
    /// <summary>
    ///     Clears all items from the container's internal item list, removing any previously added <see cref="ItemNBT" />
    ///     instances.
    /// </summary>
    /// <returns>
    ///     Returns the current instance of <see cref="ContainerBlockEntityDataNBT" /> to allow for method chaining.
    /// </returns>
    public ContainerBlockEntityDataNBT ClearItems()
    {
        this.items.Clear();
        return this;
    }

    protected override List<NBTNode> GetNodes()
    {
        List<NBTNode> root = base.GetNodes();

        Debug.Assert(this.items.TrueForAll(item => item.slot.HasValue),
            "All items inside a container must have a slot assigned to them.");

        root.Insert(0, new NBTList
        {
            name = "Items",
            listType = TAG.Compound,
            values = this.items.Select(item => item.ToNBT()).ToArray<NBTNode>()
        });

        return root;
    }
}