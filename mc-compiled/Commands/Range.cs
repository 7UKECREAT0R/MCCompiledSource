using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands
{
    /// <summary>
    /// Represents a range value in selector options. Examples:
    /// "3..999", "0..10", "50..100", "10", "!1"
    /// </summary>
    public struct Range
    {
        public bool invert, single;
        public int? min;
        public int? max;

        public Range(int? min, int? max)
        {
            this.min = min;
            this.max = max;
            invert = false;
            single = false;
        }
        public Range(int number, bool not)
        {
            min = number;
            max = null;
            single = true;
            invert = not;
        }

        /// <summary>
        /// Parse a range input into a Range structure.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Null if the parse failed.</returns>
        public static Range? Parse(string str)
        {
            if (str == null)
                return null;
            if(str.Contains(".."))
            {
                int index = str.IndexOf("..");
                if (index == -1)
                    return null;
                string _a = str.Substring(0, index);
                string _b = str.Substring(index + 2);
                int a = int.Parse(_a);
                int b = int.Parse(_b);
                return new Range(a, b);
            } else
            {
                bool not;
                if ((not = str.StartsWith("!")))
                    str = str.Substring(1);
                int parse = int.Parse(str);
                return new Range(parse, not);
            }
        }

        public override string ToString()
        {
            if (single)
                if (invert)
                    return "!" + min;
                else return min.ToString();
            else
                return (min.HasValue ? min.Value.ToString() : "") + ".." +
                    (max.HasValue ? max.Value.ToString() : "");
        }
    }
}
