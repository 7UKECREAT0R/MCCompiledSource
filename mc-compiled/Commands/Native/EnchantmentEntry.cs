using System.Collections.Generic;

namespace mc_compiled.Commands.Native
{
    public readonly struct EnchantmentEntry
    {
        public readonly string id;
        public readonly int level;

        public string IdAsLookup => id.Replace('_', ' ').ToUpper();

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

        private bool Equals(EnchantmentEntry other)
        {
            return id == other.id && level == other.level;
        }
        public override bool Equals(object obj)
        {
            return obj is EnchantmentEntry other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((id != null ? id.GetHashCode() : 0) * 397) ^ level;
            }
        }
    }
}
