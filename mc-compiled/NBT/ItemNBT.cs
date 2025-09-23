using System.Collections.Generic;
using System.Linq;
using mc_compiled.Commands.Native;

namespace mc_compiled.NBT;

public struct ItemNBT
{
    /// <summary>The actual id of the item.</summary>
    public string item;
    /// <summary>The slot of the container holding this item. Not used on item entities.</summary>
    public byte? slot;

    /// <summary>The blocks this item can destroy.</summary>
    public string[] canDestroy;
    /// <summary>The blocks this item can be placed on.</summary>
    public string[] canPlaceOn;
    /// <summary>The amount of items in this stack.</summary>
    public byte count;
    /// <summary>The damage on this item if a tool.</summary>
    public short damage;
    /// <summary>Extra item data like enchants and display name.</summary>
    public ItemTagNBT tag;

    public ItemNBT(ItemStack fromStack, byte? slot = null)
    {
        this.item = fromStack.id;
        this.slot = slot;
        this.canDestroy = fromStack.canDestroy;
        this.canPlaceOn = fromStack.canPlaceOn;
        this.count = (byte) fromStack.count;
        this.damage = (short) fromStack.damage;

        this.tag = new ItemTagNBT();
        this.tag.displayName = fromStack.displayName;
        this.tag.lore = fromStack.lore;
        this.tag.damage = fromStack.damage;
        if (fromStack.enchantments != null)
            this.tag.enchantment = (from e in fromStack.enchantments select new EnchantNBT(e)).ToArray();
        this.tag.keepOnDeath = fromStack.keep;
        this.tag.lockMode = fromStack.lockMode;

        this.tag.bookData = fromStack.bookData;
        this.tag.customColor = fromStack.customColor;
    }
    public NBTCompound ToNBT()
    {
        List<NBTNode> nodes = [];

        if (this.canDestroy != null && this.canDestroy.Length > 0)
            nodes.Add(new NBTList
            {
                name = "CanDestroy",
                listType = TAG.String,
                values = (from cd in this.canDestroy
                    select new NBTString
                    {
                        name = "",
                        value = cd
                    }).ToArray<NBTNode>()
            });
        if (this.canPlaceOn != null && this.canPlaceOn.Length > 0)
            nodes.Add(new NBTList
            {
                name = "CanPlaceOn",
                listType = TAG.String,
                values = (from cpo in this.canPlaceOn
                    select new NBTString
                    {
                        name = "",
                        value = cpo
                    }).ToArray<NBTNode>()
            });

        nodes.Add(new NBTByte {name = "Count", value = this.count});
        if (this.slot.HasValue)
            nodes.Add(new NBTByte {name = "Slot", value = this.slot.Value});
        nodes.Add(new NBTShort {name = "Damage", value = this.damage});
        nodes.Add(new NBTString {name = "Name", value = this.item});
        nodes.Add(new NBTByte {name = "WasPickedUp", value = 0});
        nodes.Add(this.tag.ToNBT());
        nodes.Add(new NBTEnd());

        return new NBTCompound
        {
            name = "Item",
            values = nodes.ToArray()
        };
    }
}