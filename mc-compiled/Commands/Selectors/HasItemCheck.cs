using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// HasItem field in selector.
    /// https://minecraft.fandom.com/wiki/Target_selectors#Selecting_targets_by_items
    /// </summary>
    public class HasItemCheck
    {
        public string item;
        public int? data;
        public Range? quantity;
        public ItemSlot? location;
        public Range? slot;

        public HasItemCheck(string item, int? data, Range? quantity, ItemSlot? location, Range? slot)
        {
            this.item = item;
            this.data = data;
            this.quantity = quantity;
            this.location = location;
            this.slot = slot;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{item=" + item);

            if (data.HasValue)
                sb.Append(",data=" + data.Value.ToString());

            if (quantity.HasValue)
                sb.Append(",quantity=" + quantity.Value.ToString());

            if (location.HasValue)
                sb.Append(",location=" + location.Value.String());

            if (slot.HasValue)
                sb.Append(",slot=" + slot.Value.ToString());

            sb.Append('}');
            return sb.ToString();
        }
    }
}
