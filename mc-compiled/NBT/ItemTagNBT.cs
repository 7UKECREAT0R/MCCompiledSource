using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public struct ItemTagNBT
    {
        public int damage;
        public EnchantNBT[] ench;
        public ItemLockMode lockMode;
        public bool keepOnDeath;
        public string displayName;
        public string[] lore;

        public ItemTagBookData? bookData;
        public ItemTagCustomColor? customColor;

        public ItemTagNBT(int damage, EnchantNBT[] ench, ItemLockMode lockMode, bool keepOnDeath, string displayName, string[] lore)
        {
            this.damage = damage;
            this.ench = ench;
            this.lockMode = lockMode;
            this.keepOnDeath = keepOnDeath;
            this.displayName = displayName;
            this.lore = lore;

            bookData = null;
            customColor = null;
        }
        public NBTCompound ToNBT()
        {
            List<NBTNode> nodes = new List<NBTNode>();
            nodes.Add(new NBTInt() { name = "Damage", value = damage });

            if (ench != null && ench.Length > 0)
            {
                nodes.Add(new NBTInt() { name = "RepairCost", value = ench.Length });
                nodes.Add(new NBTList()
                {
                    name = "ench",
                    listType = TAG.Compound,
                    values = (from e in ench select e.ToNBT()).ToArray()
                });
            }

            // All display related stuff.
            if (displayName != null || lore != null)
            {
                List<NBTNode> displayValues = new List<NBTNode>();

                // Display Name
                if (displayName != null)
                    displayValues.Add(new NBTString() { name = "Name", value = "§r§f" + displayName });

                // Lore
                if (lore != null)
                    displayValues.Add(new NBTList()
                    {
                        listType = TAG.String,
                        name = "Lore",
                        values = lore.Select(str => new NBTString()
                        {
                            name = null,
                            value = str
                        }).ToArray()
                    });

                displayValues.Add(new NBTEnd());
                nodes.Add(new NBTCompound()
                {
                    name = "display",
                    values = displayValues.ToArray()
                });
            }

            if (bookData != null)
            {
                nodes.Add(new NBTString()
                {
                    name = "author",
                    value = bookData.Value.author
                });
                nodes.Add(bookData.Value.GetPagesNBT());
                nodes.Add(new NBTString()
                {
                    name = "title",
                    value = bookData.Value.title
                });
            }
            if (customColor != null) {
                nodes.Add(new NBTColor()
                {
                    name = "customColor",
                    a = 255,
                    r = customColor.Value.r,
                    g = customColor.Value.g,
                    b = customColor.Value.b
                });
            }

            if (lockMode != ItemLockMode.NONE)
                nodes.Add(new NBTByte() { name = "minecraft:item_lock", value = (byte)lockMode });
            if (keepOnDeath)
                nodes.Add(new NBTByte() { name = "minecraft:keep_on_death", value = 1 });
            nodes.Add(new NBTEnd());

            return new NBTCompound()
            {
                name = "tag",
                values = nodes.ToArray()
            };
        }
    }
    public enum ItemLockMode : byte
    {
        NONE = 0,
        LOCK_IN_SLOT = 1,
        LOCK_IN_INVENTORY = 2
    }
    public struct ItemTagCustomColor
    {
        public byte r, g, b;
    }
    public struct ItemTagBookData
    {
        public string author;
        public string title;
        public string[] pages;

        public NBTList GetPagesNBT()
        {
            NBTCompound[] compounds = new NBTCompound[pages.Length];
            for (int i = 0; i < pages.Length; i++)
            {
                compounds[i] = new NBTCompound()
                {
                    name = null,
                    values = new NBTNode[]
                    {
                        new NBTString() { name = "text", value = pages[i] },
                        new NBTEnd()
                    }
                };
            }

            return new NBTList()
            {
                listType = TAG.Compound,
                name = "pages",
                values = compounds
            };
        }
    }
}
