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
        public Enchantment[] enchantments;
        public bool keep;
        public string[] canPlaceOn;
        public string[] canDestroy;
        public ItemLockMode lockMode;

        public ItemStack(string id, int count = 1, int damage = 0, string displayName = null, Enchantment[] enchantments = null,
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
            this.enchantments = enchantments;
            this.keep = keep;
            this.canPlaceOn = canPlaceOn;
            this.canDestroy = canDestroy;
            this.lockMode = lockMode;
        }
        
        /// <summary>
        /// Generate a unique identifier for this item stack.
        /// </summary>
        /// <returns></returns>
        public string GenerateUID()
        {
            int id = this.id.GetHashCode();
            id += count;
            id -= damage;
            id ^= displayName == null ? 0 : displayName.GetHashCode();
            if (enchantments != null)
                foreach (Enchantment ench in enchantments)
                    id ^= ench.GetHashCode();
            if (keep) id *= -1;
            if (canPlaceOn != null)
                foreach (string str in canPlaceOn)
                    id ^= str.GetHashCode();
            if (canDestroy != null)
                foreach (string str in canDestroy)
                    id ^= str.GetHashCode();
            id += (byte)lockMode;
            if (id < 0) id *= -7;
            return "item_" + id;
        }
    }
}
