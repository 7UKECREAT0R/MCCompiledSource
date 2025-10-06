using System;

namespace mc_compiled.Commands.Native;

public readonly struct EnchantmentEntry(Enchantment enchant, int level) : IEquatable<EnchantmentEntry>
{
    public readonly Enchantment id = enchant;
    public readonly int level = level;

    public string IdAsLookup => this.id.ToString().Replace('_', ' ').ToUpper();

    public bool Equals(EnchantmentEntry other) { return this.id == other.id && this.level == other.level; }
    public override bool Equals(object obj) { return obj is EnchantmentEntry other && Equals(other); }
    public override int GetHashCode()
    {
        unchecked
        {
            return (this.id.GetHashCode() * 397) ^ this.level;
        }
    }
    public static bool operator ==(EnchantmentEntry left, EnchantmentEntry right) { return left.Equals(right); }

    public static bool operator !=(EnchantmentEntry left, EnchantmentEntry right) { return !(left == right); }
}