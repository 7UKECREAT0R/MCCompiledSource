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
    public struct Coord
    {
        public static readonly Coord zero = new Coord(0, false, false, false);
        public static readonly Coord here = new Coord(0, false, true, false);
        public static readonly Coord herefacing = new Coord(0, false, false, true);

        public float valuef;
        public int valuei;

        public bool isFloat, isRelative, isFacingOffset;

        public static implicit operator Coord(int convert) => new Coord(convert, false, false, false);
        public static implicit operator Coord(float convert) => new Coord(convert, true, false, false);
        public Coord(float value, bool isFloat, bool isRelative, bool isFacingOffset)
        {
            valuef = value;
            valuei = (int)Math.Round(value);
            this.isFloat = isFloat;
            this.isRelative = isRelative;
            this.isFacingOffset = isFacingOffset;
        }
        public Coord(int value, bool isFloat, bool isRelative, bool isFacingOffset)
        {
            valuef = value;
            valuei = value;
            this.isFloat = isFloat;
            this.isRelative = isRelative;
            this.isFacingOffset = isFacingOffset;
        }
        public Coord(Coord other)
        {
            valuef = other.valuef;
            valuei = other.valuei;
            isFloat = other.isFloat;
            isRelative = other.isRelative;
            isFacingOffset = other.isFacingOffset;
        }

        /// <summary>
        /// Parse this coordinate value. Returns null if not succeeded.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Coord? Parse(string str)
        {
            if (str == null)
                return null;

            str = str.Trim();

            bool rel, off;
            str = str.TrimEnd('f');
            if ((rel = str.StartsWith("~")))
                str = str.Substring(1);
            if ((off = str.StartsWith("^")))
                str = str.Substring(1);

            if (int.TryParse(str, out int i))
                return new Coord(i, false, rel, off);
            else if (float.TryParse(str, out float f))
                return new Coord(f, true, rel, off);
            else
                return new Coord(0, false, rel, off); // default to 0
        }
        /// <summary>
        /// Determine if the size of these corner points can be determined at compile-time
        /// by ensuring every coordinate is either relative or exact.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public static bool SizeKnown(params Coord[] coords)
        {
            for(int i = 1; i < coords.Length; i++)
            {
                Coord a = coords[i - 1];
                Coord b = coords[i];
                if (a.isRelative != b.isRelative)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get a Minecraft-command supported string for this coordinate.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string s;
            bool anyRelative = isRelative | isFacingOffset;

            if (isFloat)
                s = (valuef == 0f && anyRelative) ? "" : valuef.ToString();
            else
                s = (valuei == 0 && anyRelative) ? "" : valuei.ToString();

            if (isRelative)
                return '~' + s;
            if (isFacingOffset)
                return '^' + s;

            return s;
        }
        /// <summary>
        /// Get a Minecraft-command supported string for this coordinate, optionally requesting that the 
        /// </summary>
        /// <param name="requestInteger"></param>
        /// <returns></returns>
        public string ToString(bool requestInteger)
        {
            string s = (requestInteger ? valuei : isFloat ? valuef : valuei).ToString();

            if (isRelative)
                return '~' + s;
            if (isFacingOffset)
                return '^' + s;

            return s;
        }

        /// <summary>
        /// Return the smaller of the two coords.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Coord Min(Coord a, Coord b)
        {
            if (a.valuef < b.valuef)
                return a;
            if (a.valuef > b.valuef)
                return b;
            return a; // default
        }
        /// <summary>
        /// Return the larger of the two coords.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Coord Max(Coord a, Coord b)
        {
            if (a.valuef < b.valuef)
                return b;
            if (a.valuef > b.valuef)
                return a;
            return a; // default
        }

        public override bool Equals(object obj)
        {
            return obj is Coord coord &&
                   valuef == coord.valuef &&
                   valuei == coord.valuei &&
                   isFloat == coord.isFloat &&
                   isRelative == coord.isRelative &&
                   isFacingOffset == coord.isFacingOffset;
        }

        public override int GetHashCode()
        {
            int hashCode = 1648134579;
            hashCode = hashCode * -1521134295 + valuef.GetHashCode();
            hashCode = hashCode * -1521134295 + valuei.GetHashCode();
            hashCode = hashCode * -1521134295 + isFloat.GetHashCode();
            hashCode = hashCode * -1521134295 + isRelative.GetHashCode();
            hashCode = hashCode * -1521134295 + isFacingOffset.GetHashCode();
            return hashCode;
        }

        public static Coord operator -(Coord a)
        {
            a.valuei *= -1;
            a.valuef *= -1f;
            return a;
        }

        public static Coord operator +(Coord a, Coord b)
        {
            if (a.isFloat || b.isFloat)
                return new Coord(a.valuef + b.valuef, true, a.isRelative, a.isFacingOffset);
            else
                return new Coord(a.valuei + b.valuei, false, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator -(Coord a, Coord b)
        {
            if (a.isFloat || b.isFloat)
                return new Coord(a.valuef - b.valuef, true, a.isRelative, a.isFacingOffset);
            else
                return new Coord(a.valuei - b.valuei, false, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator *(Coord a, Coord b)
        {
            if (a.isFloat || b.isFloat)
                return new Coord(a.valuef * b.valuef, true, a.isRelative, a.isFacingOffset);
            else
                return new Coord(a.valuei * b.valuei, false, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator /(Coord a, Coord b)
        {
            if (a.isFloat || b.isFloat)
                return new Coord(a.valuef / b.valuef, true, a.isRelative, a.isFacingOffset);
            else
                return new Coord(a.valuei / b.valuei, false, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator %(Coord a, Coord b)
        {
            if (a.isFloat || b.isFloat)
                return new Coord(a.valuef % b.valuef, true, a.isRelative, a.isFacingOffset);
            else
                return new Coord(a.valuei % b.valuei, false, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator +(Coord a, int b)
        {
            if (a.isFloat)
                return new Coord(a.valuef + b, true, a.isRelative, a.isFacingOffset);
            else
                return new Coord(a.valuei + b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator -(Coord a, int b)
        {
            if (a.isFloat)
                return new Coord(a.valuef - b, true, a.isRelative, a.isFacingOffset);
            else
                return new Coord(a.valuei - b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator *(Coord a, int b)
        {
            if (a.isFloat)
                return new Coord(a.valuef * b, true, a.isRelative, a.isFacingOffset);
            else
                return new Coord(a.valuei * b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator /(Coord a, int b)
        {
            if (a.isFloat)
                return new Coord(a.valuef / b, true, a.isRelative, a.isFacingOffset);
            else
                return new Coord(a.valuei / b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator %(Coord a, int b)
        {
            if (a.isFloat)
                return new Coord(a.valuef % b, true, a.isRelative, a.isFacingOffset);
            else
                return new Coord(a.valuei % b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator +(Coord a, float b)
        {
            return new Coord(a.valuef + b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator -(Coord a, float b)
        {
            return new Coord(a.valuef - b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator *(Coord a, float b)
        {
            return new Coord(a.valuef * b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator /(Coord a, float b)
        {
            return new Coord(a.valuef / b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coord operator %(Coord a, float b)
        {
            return new Coord(a.valuef % b, true, a.isRelative, a.isFacingOffset);
        }


    }
}
