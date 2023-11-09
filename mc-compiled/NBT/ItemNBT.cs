using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.NBT
{
    public struct ItemNBT
    {
        public string item;            // The actual id of the item.
                                       //
        public string[] canDestroy;    // The blocks this item can destroy.
        public string[] canPlaceOn;    // The blocks this item can be placed on.
        public byte count;             // The amount of items in this.
        public short damage;           // The damage on this item if a tool.
        public ItemTagNBT tag;         // Extra item data like enchants and display name.

        public ItemNBT(Commands.Native.ItemStack fromStack)
        {
            item = fromStack.id;
            canDestroy = fromStack.canDestroy;
            canPlaceOn = fromStack.canPlaceOn;
            count = (byte)fromStack.count;
            damage = (short)fromStack.damage;

            tag = new ItemTagNBT();
            tag.displayName = fromStack.displayName;
            tag.lore = fromStack.lore;
            tag.damage = fromStack.damage;
            if (fromStack.enchantments != null)
                tag.ench = (from e in fromStack.enchantments select new EnchantNBT(e)).ToArray();
            tag.keepOnDeath = fromStack.keep;
            tag.lockMode = fromStack.lockMode;

            tag.bookData = fromStack.bookData;
            tag.customColor = fromStack.customColor;
        }
        public NBTCompound ToNBT()
        {
            List<NBTNode> nodes = new List<NBTNode>();

            if (canDestroy != null && canDestroy.Length > 0)
                nodes.Add(new NBTList() {
                    name = "CanDestroy",
                    listType = TAG.String,
                    values = (from cd in canDestroy select new NBTString()
                    {
                        name = "",
                        value = cd
                    }).ToArray()
                });
            if (canPlaceOn != null && canPlaceOn.Length > 0)
                nodes.Add(new NBTList()
                {
                    name = "CanPlaceOn",
                    listType = TAG.String,
                    values = (from cpo in canPlaceOn select new NBTString()
                    {
                        name = "",
                        value = cpo
                    }).ToArray()
                });

            nodes.Add(new NBTByte() { name = "Count", value = count });
            nodes.Add(new NBTShort() { name = "Damage", value = damage });
            nodes.Add(new NBTString() { name = "Name", value = item });
            nodes.Add(new NBTByte() { name = "WasPickedUp", value = 0 });
            nodes.Add(tag.ToNBT());
            nodes.Add(new NBTEnd());

            return new NBTCompound()
            {
                name = "Item",
                values = nodes.ToArray()
            };
        }
    }
}
