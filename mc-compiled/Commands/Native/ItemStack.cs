using mc_compiled.NBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// Generate a unique identifier for this item stack.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = -1625500939;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(id);
            hashCode = hashCode * -1521134295 + count.GetHashCode();
            hashCode = hashCode * -1521134295 + damage.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(displayName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(lore);
            hashCode = hashCode * -1521134295 + EqualityComparer<EnchantmentEntry[]>.Default.GetHashCode(enchantments);
            hashCode = hashCode * -1521134295 + keep.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(canPlaceOn);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(canDestroy);
            hashCode = hashCode * -1521134295 + lockMode.GetHashCode();
            return hashCode;
        }
    }
}
