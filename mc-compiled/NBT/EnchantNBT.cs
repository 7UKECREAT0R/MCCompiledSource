using mc_compiled.Commands.Native;
using mc_compiled.MCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public struct EnchantNBT
    {
        public short id;
        public short level;

        public EnchantNBT(EnchantmentEntry fromEnchantment)
        {
            Definitions defs = Definitions.GLOBAL_DEFS;
            id = short.Parse(defs.defs["ENCHANT:" + fromEnchantment.IDAsLookup]);
            level = (short)fromEnchantment.level;
        }
        public NBTCompound ToNBT()
        {
            return new NBTCompound()
            {
                name = "",
                values = new NBTNode[]
                {
                    new NBTShort() { name = "id", value = id },
                    new NBTShort() { name = "lvl", value = level },
                    new NBTEnd()
                }
            };
        }
    }
}
