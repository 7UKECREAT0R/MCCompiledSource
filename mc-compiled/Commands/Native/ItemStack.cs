using System;
using mc_compiled.NBT;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace mc_compiled.Commands.Native
{
    /// <summary>
    /// Represents an item stack in-game.
    /// </summary>
    public struct ItemStack
    {
        public string id;
        public int count;
        public int damage;

        public string displayName;
        public string[] lore;
        public EnchantmentEntry[] enchantments;
        public bool keep;
        public string[] canPlaceOn;
        public string[] canDestroy;
        public ItemLockMode lockMode;

        public ItemTagBookData? bookData;
        public ItemTagCustomColor? customColor;

        [PublicAPI]
        public ItemStack(string id, int count = 1, int damage = 0, string displayName = null, string[] lore = null, EnchantmentEntry[] enchantments = null,
            bool keep = false, string[] canPlaceOn = null, string[] canDestroy = null, ItemLockMode lockMode = ItemLockMode.NONE)
        {
            // namespace required
            if (id.Contains(':'))
                this.id = id;
            else
                this.id = "minecraft:" + id;

            this.count = count;
            this.damage = damage;
            this.displayName = displayName;
            this.lore = lore;
            this.enchantments = enchantments;
            this.keep = keep;
            this.canPlaceOn = canPlaceOn;
            this.canDestroy = canDestroy;
            this.lockMode = lockMode;

            bookData = null;
            customColor = null;
        }

        public bool Equals(ItemStack other)
        {
            return id == other.id && count == other.count && damage == other.damage &&
                   displayName == other.displayName && Equals(lore, other.lore) &&
                   Equals(enchantments, other.enchantments) && keep == other.keep &&
                   Equals(canPlaceOn, other.canPlaceOn) && Equals(canDestroy, other.canDestroy) &&
                   lockMode == other.lockMode && Nullable.Equals(bookData, other.bookData) &&
                   Nullable.Equals(customColor, other.customColor);
        }

        public override bool Equals(object obj)
        {
            return obj is ItemStack other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (id != null ? id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ count;
                hashCode = (hashCode * 397) ^ damage;
                hashCode = (hashCode * 397) ^ (displayName != null ? displayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (lore != null ? lore.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (enchantments != null ? enchantments.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ keep.GetHashCode();
                hashCode = (hashCode * 397) ^ (canPlaceOn != null ? canPlaceOn.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (canDestroy != null ? canDestroy.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) lockMode;
                hashCode = (hashCode * 397) ^ bookData.GetHashCode();
                hashCode = (hashCode * 397) ^ customColor.GetHashCode();
                return hashCode;
            }
        }
    }
}
