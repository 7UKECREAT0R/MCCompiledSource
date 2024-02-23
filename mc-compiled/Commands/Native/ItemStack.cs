using System;
using mc_compiled.NBT;
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

            this.bookData = null;
            this.customColor = null;
        }

        public bool Equals(ItemStack other)
        {
            return this.id == other.id && this.count == other.count && this.damage == other.damage && this.displayName == other.displayName && Equals(this.lore, other.lore) &&
                   Equals(this.enchantments, other.enchantments) && this.keep == other.keep &&
                   Equals(this.canPlaceOn, other.canPlaceOn) && Equals(this.canDestroy, other.canDestroy) && this.lockMode == other.lockMode && Nullable.Equals(this.bookData, other.bookData) &&
                   Nullable.Equals(this.customColor, other.customColor);
        }

        public override bool Equals(object obj)
        {
            return obj is ItemStack other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (this.id != null ? this.id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.count;
                hashCode = (hashCode * 397) ^ this.damage;
                hashCode = (hashCode * 397) ^ (this.displayName != null ? this.displayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.lore != null ? this.lore.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.enchantments != null ? this.enchantments.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.keep.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.canPlaceOn != null ? this.canPlaceOn.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.canDestroy != null ? this.canDestroy.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) this.lockMode;
                hashCode = (hashCode * 397) ^ this.bookData.GetHashCode();
                hashCode = (hashCode * 397) ^ this.customColor.GetHashCode();
                return hashCode;
            }
        }
    }
}
