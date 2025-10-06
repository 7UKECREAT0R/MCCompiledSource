using mc_compiled.Commands;
using mc_compiled.Commands.Native;

namespace mc_compiled.NBT;

public readonly struct EnchantNBT(EnchantmentEntry fromEnchantment)
{
    private readonly Enchantment id = fromEnchantment.id;
    private readonly short level = (short) fromEnchantment.level;

    public NBTCompound ToNBT()
    {
        return new NBTCompound
        {
            name = "",
            values =
            [
                new NBTShort {name = "id", value = (short) this.id},
                new NBTShort {name = "lvl", value = this.level},
                new NBTEnd()
            ]
        };
    }
}