namespace mc_compiled.Commands.Native
{
    public readonly struct EnchantmentEntry
    {
        public readonly string id;
        public readonly int level;

        public string IdAsLookup => this.id.Replace('_', ' ').ToUpper();

        public EnchantmentEntry(string id, int level)
        {
            this.id = id;
            this.level = level;
        }
        public EnchantmentEntry(Enchantment enchant, int level)
        {
            this.id = enchant.ToString();
            this.level = level;
        }

        private bool Equals(EnchantmentEntry other)
        {
            return this.id == other.id && this.level == other.level;
        }
        public override bool Equals(object obj)
        {
            return obj is EnchantmentEntry other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.id != null ? this.id.GetHashCode() : 0) * 397) ^ this.level;
            }
        }
    }
}
