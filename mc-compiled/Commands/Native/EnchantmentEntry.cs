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

        public string IDAsLookup
        {
            get => id.Replace('_', ' ').ToUpper();
        }

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

        public override bool Equals(object obj)
        {
            return obj is EnchantmentEntry entry &&
                   id == entry.id &&
                   level == entry.level;
        }
        public override int GetHashCode()
        {
            int hashCode = 192898493;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(id);
            hashCode = hashCode * -1521134295 + level.GetHashCode();
            return hashCode;
        }
    }
}
