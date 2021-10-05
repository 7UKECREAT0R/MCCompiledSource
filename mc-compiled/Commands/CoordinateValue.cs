using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands
{
    /// <summary>
    /// An integer or float with the option of being relative or facing offset.
    /// </summary>
    public struct CoordinateValue
    {
        public static readonly CoordinateValue zero = new CoordinateValue(0, false, false, false);
        public static readonly CoordinateValue here = new CoordinateValue(0, false, true, false);

        public float valuef;
        public int valuei;

        public bool isFloat, isRelative, isFacingOffset;

        private CoordinateValue(float value, bool isFloat, bool isRelative, bool isFacingOffset)
        {
            valuef = value;
            valuei = (int)Math.Round(value);
            this.isFloat = isFloat;
            this.isRelative = isRelative;
            this.isFacingOffset = isFacingOffset;
        }
        private CoordinateValue(int value, bool isFloat, bool isRelative, bool isFacingOffset)
        {
            valuef = value;
            valuei = value;
            this.isFloat = isFloat;
            this.isRelative = isRelative;
            this.isFacingOffset = isFacingOffset;
        }

        /// <summary>
        /// Parse this coordinate value. Returns null if not succeeded.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static CoordinateValue? Parse(string str)
        {
            if (str == null)
                return null;

            str = str.Trim();

            bool rel, off;
            str = str.TrimEnd('f');
            if ((rel = str.StartsWith("~")))
                str = str.TrimStart('~');
            if ((off = str.StartsWith("^")))
                str = str.TrimStart('^');

            if (int.TryParse(str, out int i))
                return new CoordinateValue(i, false, rel, off);
            else if (float.TryParse(str, out float f))
                return new CoordinateValue(f, false, rel, off);
            else
                return new CoordinateValue(0, false, rel, off); // default to 0
        }

        public override string ToString()
        {
            string s;
            if (isFloat)
                s = (valuef == 0f) ? "" : valuef.ToString();
            else
                s = (valuei == 0) ? "" : valuei.ToString();

            if (isRelative)
                return '~' + s;
            if (isFacingOffset)
                return '^' + s;

            return s;
        }
        public string ToString(bool requestInteger)
        {
            string s = (requestInteger ? valuei : isFloat ? valuef : valuei).ToString();

            if (isRelative)
                return '~' + s;
            if (isFacingOffset)
                return '^' + s;

            return s;
        }
    }
}
