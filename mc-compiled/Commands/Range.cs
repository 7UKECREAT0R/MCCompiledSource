﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands
{
    /// <summary>
    /// Represents a range value in selector options. Examples:
    /// "3..999", "0..10", "50..100", "10", "!1", "!9..100"
    /// </summary>
    public struct Range
    {
        public bool invert, single;
        public int? min;
        public int? max;

        public Range(int? min, int? max, bool not = false)
        {
            this.min = min;
            this.max = max;
            invert = not;
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

            bool not;
            if ((not = str.StartsWith("!")))
                str = str.Substring(1);

            if (str.Contains(".."))
            {
                int index = str.IndexOf("..");
                if (index == 0) // ..10
                    return new Range(null, int.Parse(str.Substring(index + 2)), not);
                if (index + 2 >= str.Length) // 10..
                    return new Range(int.Parse(str.Substring(0, index)), null, not);

                string _a = str.Substring(0, index);
                string _b = str.Substring(index + 2);
                int a = int.Parse(_a);
                int b = int.Parse(_b);
                return new Range(a, b, not);
            } else
            {
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
