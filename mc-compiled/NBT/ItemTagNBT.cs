using System;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.NBT
{
    public struct ItemTagNBT
    {
        public int damage;
        public EnchantNBT[] enchantment;
        public ItemLockMode lockMode;
        public bool keepOnDeath;
        public string displayName;
        public string[] lore;

        public ItemTagBookData? bookData;
        public ItemTagCustomColor? customColor;

        public ItemTagNBT(int damage, EnchantNBT[] enchantment, ItemLockMode lockMode, bool keepOnDeath, string displayName, string[] lore)
        {
            this.damage = damage;
            this.enchantment = enchantment;
            this.lockMode = lockMode;
            this.keepOnDeath = keepOnDeath;
            this.displayName = displayName;
            this.lore = lore;

            this.bookData = null;
            this.customColor = null;
        }
        public NBTCompound ToNBT()
        {
            var nodes = new List<NBTNode>
            {
                new NBTInt() { name = "Damage", value = this.damage }
            };

            if (this.enchantment != null && this.enchantment.Length > 0)
            {
                nodes.Add(new NBTInt() { name = "RepairCost", value = this.enchantment.Length });
                nodes.Add(new NBTList()
                {
                    name = "ench",
                    listType = TAG.Compound,
                    values = (from e in this.enchantment select e.ToNBT()).ToArray<NBTNode>()
                });
            }

            // All display related stuff.
            if (this.displayName != null || this.lore != null)
            {
                var displayValues = new List<NBTNode>();

                // Display Name
                if (this.displayName != null)
                    displayValues.Add(new NBTString() { name = "Name", value = "§r§f" + this.displayName });

                // Lore
                if (this.lore != null)
                    displayValues.Add(new NBTList()
                    {
                        listType = TAG.String,
                        name = "Lore",
                        values = this.lore.Select(str => new NBTString()
                        {
                            name = null,
                            value = str
                        }).ToArray<NBTNode>()
                    });

                displayValues.Add(new NBTEnd());
                nodes.Add(new NBTCompound()
                {
                    name = "display",
                    values = displayValues.ToArray()
                });
            }

            if (this.bookData != null)
            {
                nodes.Add(new NBTString()
                {
                    name = "author",
                    value = this.bookData.Value.author
                });
                nodes.Add(this.bookData.Value.GetPagesNBT());
                nodes.Add(new NBTString()
                {
                    name = "title",
                    value = this.bookData.Value.title
                });
            }
            if (this.customColor != null) {
                nodes.Add(new NBTColor()
                {
                    name = "customColor",
                    a = 255,
                    r = this.customColor.Value.r,
                    g = this.customColor.Value.g,
                    b = this.customColor.Value.b
                });
            }

            if (this.lockMode != ItemLockMode.NONE)
                nodes.Add(new NBTByte() { name = "minecraft:item_lock", value = (byte) this.lockMode });
            if (this.keepOnDeath)
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
    public struct ItemTagCustomColor : IEquatable<ItemTagCustomColor>
    {
        public byte r, g, b;
        
        public bool Equals(ItemTagCustomColor other)
        {
            return this.r == other.r && this.g == other.g && this.b == other.b;
        }
        public override bool Equals(object obj)
        {
            return obj is ItemTagCustomColor other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.r.GetHashCode();
                hashCode = (hashCode * 397) ^ this.g.GetHashCode();
                hashCode = (hashCode * 397) ^ this.b.GetHashCode();
                return hashCode;
            }
        }
    }
    public struct ItemTagBookData : IEquatable<ItemTagBookData>
    {
        public string author;
        public string title;
        public string[] pages;

        public NBTList GetPagesNBT()
        {
            var compounds = new NBTNode[this.pages.Length];
            
            for (int i = 0; i < this.pages.Length; i++)
            {
                compounds[i] = new NBTCompound()
                {
                    name = null,
                    values = new NBTNode[]
                    {
                        new NBTString() { name = "text", value = this.pages[i] },
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
        
        public bool Equals(ItemTagBookData other)
        {
            return this.author == other.author && this.title == other.title && Equals(this.pages, other.pages);
        }
        public override bool Equals(object obj)
        {
            return obj is ItemTagBookData other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (this.author != null ? this.author.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.title != null ? this.title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.pages != null ? this.pages.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
