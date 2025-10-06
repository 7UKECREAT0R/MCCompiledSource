using System;
using System.Text;
using JetBrains.Annotations;
using mc_compiled.NBT;

namespace mc_compiled.Commands.Native;

/// <summary>
///     Represents an item stack in-game.
/// </summary>
public struct ItemStack : IEquatable<ItemStack>
{
    public string id;
    public int count;
    public int damage;

    public string displayName;
    public string[] lore;
    public EnchantmentEntry[] enchantments;
    public bool keep;
    public string[] canPlaceOn;
    public string[] canDestroy;
    public ItemLockMode lockMode;

    public ItemTagBookData? bookData;
    public ItemTagCustomColor? customColor;

    [PublicAPI]
    public ItemStack(string id,
        int count = 1,
        int damage = 0,
        string displayName = null,
        string[] lore = null,
        EnchantmentEntry[] enchantments = null,
        bool keep = false,
        string[] canPlaceOn = null,
        string[] canDestroy = null,
        ItemLockMode lockMode = ItemLockMode.NONE)
    {
        // namespace required
        this.id = Command.Util.RequireNamespace(id);
        this.count = count;
        this.damage = damage;
        this.displayName = displayName;
        this.lore = lore;
        this.enchantments = enchantments;
        this.keep = keep;
        this.canPlaceOn = canPlaceOn;
        this.canDestroy = canDestroy;
        this.lockMode = lockMode;

        this.bookData = null;
        this.customColor = null;
    }

    public string DescriptiveFileName
    {
        get
        {
            var baseName = new StringBuilder();
            if (string.IsNullOrWhiteSpace(this.displayName))
            {
                baseName.Append("item_");
                baseName.Append(Command.Util.StripNamespace(this.id).ToLower());
                baseName.Append('_');
            }
            else
            {
                bool lastWasUnderscore = false;
                bool lastWasColorSymbol = false;
                const char colorSymbol = '§';
                foreach (char c in this.displayName)
                {
                    // handle color symbols
                    if (lastWasColorSymbol)
                    {
                        // skip character after the color symbol as well
                        lastWasColorSymbol = false;
                        continue;
                    }

                    if (c == colorSymbol)
                    {
                        lastWasColorSymbol = true;
                        continue;
                    }

                    // letters/digits are fine, anything else gets turned into an underscore.
                    if (char.IsLetter(c) || char.IsDigit(c))
                    {
                        baseName.Append(c);
                        lastWasUnderscore = false;
                    }
                    else
                    {
                        baseName.Append('_');
                        lastWasUnderscore = true;
                    }
                }

                if (!lastWasUnderscore)
                    baseName.Append('_');
            }

            // append count?
            if (this.count != 1)
            {
                baseName.Append('x');
                baseName.Append(this.count);
                baseName.Append('_');
            }

            if (this.keep)
                baseName.Append("keepOnDeath_");
            if (this.lockMode == ItemLockMode.LOCK_IN_SLOT)
                baseName.Append("lockSlot_");
            else if (this.lockMode == ItemLockMode.LOCK_IN_INVENTORY)
                baseName.Append("lockInventory_");
            if (this.bookData.HasValue)
            {
                baseName.Append(this.bookData.Value.pages.Length);
                baseName.Append("pages_");
            }

            if (this.customColor.HasValue)
                baseName.Append("dyed_");

            int len = baseName.Length;
            return baseName.ToString()[..(len - 1)];
        }
    }
    public bool Equals(ItemStack other)
    {
        return this.id == other.id && this.count == other.count && this.damage == other.damage &&
               this.displayName == other.displayName && Equals(this.lore, other.lore) &&
               Equals(this.enchantments, other.enchantments) && this.keep == other.keep &&
               Equals(this.canPlaceOn, other.canPlaceOn) && Equals(this.canDestroy, other.canDestroy) &&
               this.lockMode == other.lockMode && Nullable.Equals(this.bookData, other.bookData) &&
               Nullable.Equals(this.customColor, other.customColor);
    }
    public override bool Equals(object obj) { return obj is ItemStack other && Equals(other); }
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = this.id != null ? this.id.GetHashCode() : 0;
            hashCode = (hashCode * 397) ^ this.count;
            hashCode = (hashCode * 397) ^ this.damage;
            hashCode = (hashCode * 397) ^ (this.displayName != null ? this.displayName.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (this.lore != null ? this.lore.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (this.enchantments != null ? this.enchantments.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ this.keep.GetHashCode();
            hashCode = (hashCode * 397) ^ (this.canPlaceOn != null ? this.canPlaceOn.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (this.canDestroy != null ? this.canDestroy.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (int) this.lockMode;
            hashCode = (hashCode * 397) ^ this.bookData.GetHashCode();
            hashCode = (hashCode * 397) ^ this.customColor.GetHashCode();
            return hashCode;
        }
    }
}