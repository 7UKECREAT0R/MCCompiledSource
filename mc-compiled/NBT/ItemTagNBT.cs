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

        public ItemTagNBT(int damage, EnchantNBT[] ench, ItemLockMode lockMode, bool keepOnDeath, string displayName)
        {
            this.damage = damage;
            this.ench = ench;
            this.lockMode = lockMode;
            this.keepOnDeath = keepOnDeath;
            this.displayName = displayName;
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

            if (displayName != null)
            {
                nodes.Add(new NBTCompound()
                {
                    name = "display",
                    values = new NBTNode[]
                    {
                        new NBTString() { name = "Name", value = "§r§f" + displayName },
                        new NBTEnd()
                    }
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
}
