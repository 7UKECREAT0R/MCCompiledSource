using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Native
{
    public struct EnchantmentEntry
    {
        public readonly string id;
        public readonly int level;

        public EnchantmentEntry(string id, int level)
        {
            this.id = id;
            this.level = level;
        }
        public EnchantmentEntry(Enchantment enchant, int level)
        {
            id = enchant.ToString();
            this.level = level;
        }
    }
}
