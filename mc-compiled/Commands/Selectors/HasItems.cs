using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace mc_compiled.Commands.Selectors
{
    public struct HasItems
    {
        private static readonly Regex MATCHER = new Regex(@"hasitem=\[?([\w\d=:,.{}]+)\]?");
        public List<HasItemEntry> entries;

        public HasItems(params HasItemEntry[] start)
        {
            this.entries = new List<HasItemEntry>(start);
        }
        public HasItems(List<HasItemEntry> start)
        {
            this.entries = new List<HasItemEntry>(start);
        }

        public string GetSection()
        {
            if (this.entries == null || this.entries.Count < 1)
                return null;

            if (this.entries.Count == 1)
                return "hasitem=" + this.entries[0].ToString();
            else
                return "hasitem=[" + string.Join(",", (from e in this.entries select e.ToString())) + "]";
        }
        public string AsStoreIn(string selector, string objective)
        {
            return $"execute {selector}[{GetSection()}] ~~~ scoreboard players set @s {objective} 1";
        }
        
        public static HasItems Parse(string fullSelector)
        {
            if (!MATCHER.IsMatch(fullSelector))
                return new HasItems(new List<HasItemEntry>());

            // Example input:
            // hasitem=[{item=zingus,quantity=1..5},{item=goongongy,location=slot.armor.head,slot=0}]

            HasItems hasitem = new HasItems(new List<HasItemEntry>());

            Match match = MATCHER.Match(fullSelector);
            Group group = match.Groups[1];
            string str = group.Value;
            // {item=zingus,quantity=1..5},{item=goongongy,location=slot.armor.head,slot=0}

            // Reduce down to trimmed pieces.
            string[] split = str.Split(new[] { "},{" }, StringSplitOptions.None);
            var parts = from part in split select part.Trim('{', '}');

            // item=goongongy,location=slot.armor.head,slot=0
            foreach (string part in parts)
            {
                string[] sections = part.Split(',');
                HasItemEntry entry = new HasItemEntry();
                foreach (string section in sections)
                {
                    int index = section.IndexOf('=');
                    if (index == -1)
                        continue;
                    string key = section.Substring(0, index).Trim();
                    string _value = section.Substring(index + 1).Trim();

                    switch (key.ToUpper())
                    {
                        case "I":
                        case "ITEM":
                            entry.item = _value;
                            break;
                        case "L":
                        case "LOCATION":
                            if (CommandEnumParser.TryParse(_value, out ParsedEnumValue enumValue))
                            {
                                if(enumValue.IsType<ItemSlot>())
                                    entry.location = (ItemSlot)enumValue.value;
                            }
                            break;
                        case "Q":
                        case "QUANTITY":
                            Range? range = Range.Parse(_value);
                            if (range.HasValue)
                                entry.quantity = range;
                            break;
                        case "S":
                        case "SLOT":
                            if (int.TryParse(_value, out int slot))
                                entry.slot = slot;
                            break;
                        case "D":
                        case "DATA":
                            if (int.TryParse(_value, out int data))
                                entry.data = data;
                            break;
                    }
                }
                hasitem.entries.Add(entry);
            }

            return hasitem;
        }

        public bool Equals(HasItems other)
        {
            return Equals(this.entries, other.entries);
        }
        public override bool Equals(object obj)
        {
            return obj is HasItems other && Equals(other);
        }
        public override int GetHashCode()
        {
            return this.entries != null ? this.entries.GetHashCode() : 0;
        }

        public static HasItems operator +(HasItems a, HasItems other)
        {
            var clone = (HasItems)a.MemberwiseClone();
            clone.entries = new List<HasItemEntry>(a.entries);
            clone.entries.AddRange(other.entries);
            return clone;
        }
    }
    public struct HasItemEntry
    {
        public string item;

        public int? data;
        public ItemSlot? location;
        public int? slot;
        public Range? quantity;

        public HasItemEntry(string item,
            int? data = null,
            ItemSlot? location = null,
            int? slot = null,
            Range? quantity = null)
        {
            this.item = item;
            this.data = data;
            this.location = location;
            this.slot = slot;
            this.quantity = quantity;
        }
        /// <summary>
        /// Returns if this entry is just a bare item check. e.g., hasitem={item=apple} (nothing more)
        /// </summary>
        public bool IsBare
        {
            get => !this.location.HasValue && !this.slot.HasValue && !this.data.HasValue && !this.quantity.HasValue;
        }

        public override bool Equals(object obj)
        {
            return obj is HasItemEntry entry && this.item == entry.item && this.data == entry.data && this.location == entry.location && this.slot == entry.slot &&
                   EqualityComparer<Range?>.Default.Equals(this.quantity, entry.quantity) && this.IsBare == entry.IsBare;
        }
        public override int GetHashCode()
        {
            int hashCode = 1388543879;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.item);
            hashCode = hashCode * -1521134295 + this.data.GetHashCode();
            hashCode = hashCode * -1521134295 + this.location.GetHashCode();
            hashCode = hashCode * -1521134295 + this.slot.GetHashCode();
            hashCode = hashCode * -1521134295 + this.quantity.GetHashCode();
            hashCode = hashCode * -1521134295 + this.IsBare.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            List<string> parts = new List<string>();
            parts.Add("item=" + this.item);

            if (this.data.HasValue)
                parts.Add("data=" + this.data.Value);
            if (this.location.HasValue)
                parts.Add("location=" + this.location.Value.String());
            if (this.slot.HasValue)
                parts.Add("slot=" + this.slot.Value);
            if (this.quantity.HasValue)
                parts.Add("quantity=" + this.quantity.Value.ToString());

            return $@"{{{string.Join(",", parts)}}}";
        }
    }
}
