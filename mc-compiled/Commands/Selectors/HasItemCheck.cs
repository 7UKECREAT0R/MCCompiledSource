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

        private HasItemCheck() { }
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

        /// <summary>
        /// Parse a HasItemCheck. Expected format: <code>{item=gold_ingot,quantity=5..,location=slot.hotbar,slot:0..3}</code>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static HasItemCheck Parse(string str)
        {
            str = str.Trim('{', '}');
            string[] parts = str.Split(',');

            HasItemCheck check = new HasItemCheck();

            foreach (string part in parts)
            {
                int equals = str.IndexOf('=');

                if (equals == -1)
                    throw new MCC.Compiler.TokenizerException("Expected equals sign in hasitem property: " + part);

                string section = str.Substring(0, equals).ToUpper();
                string value = str.Substring(equals + 1);

                switch(section)
                {
                    case "ITEM":
                        check.item = value;
                        break;
                    case "DATA":
                        if (int.TryParse(value, out int _data))
                            check.data = _data;
                        break;
                    case "QUANTITY":
                        check.quantity = Range.Parse(value);
                        break;
                    case "LOCATION":
                        if(CommandEnumParser.TryParse(value, out ParsedEnumValue locationEnum))
                        {
                            if (locationEnum.IsType<ItemSlot>())
                                check.location = (ItemSlot)locationEnum.value;
                        }
                        break;
                    case "SLOT":
                        check.slot = Range.Parse(value);
                        break;
                }
            }

            return check;
        }
    }
}
