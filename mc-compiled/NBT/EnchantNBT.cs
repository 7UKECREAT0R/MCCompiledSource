using mc_compiled.Commands.Native;
using mc_compiled.MCC;

namespace mc_compiled.NBT
{
    public struct EnchantNBT
    {
        public short id;
        public short level;

        public EnchantNBT(EnchantmentEntry fromEnchantment)
        {
            Definitions defs = Definitions.GLOBAL_DEFS;
            this.id = short.Parse(defs.defs["ENCHANT:" + fromEnchantment.IdAsLookup]);
            this.level = (short)fromEnchantment.level;
        }
        public NBTCompound ToNBT()
        {
            return new NBTCompound
            {
                name = "",
                values =
                [
                    new NBTShort { name = "id", value = this.id },
                    new NBTShort { name = "lvl", value = this.level },
                    new NBTEnd()
                ]
            };
        }
    }
}
