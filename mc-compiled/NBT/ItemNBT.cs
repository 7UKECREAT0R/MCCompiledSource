using System.Collections.Generic;
using System.Linq;
using mc_compiled.Commands.Native;

namespace mc_compiled.NBT;

public struct ItemNBT
{
    public string item; // The actual id of the item.
    //
    public string[] canDestroy; // The blocks this item can destroy.
    public string[] canPlaceOn; // The blocks this item can be placed on.
    public byte count; // The amount of items in this.
    public short damage; // The damage on this item if a tool.
    public ItemTagNBT tag; // Extra item data like enchants and display name.

    public ItemNBT(ItemStack fromStack)
    {
        this.item = fromStack.id;
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