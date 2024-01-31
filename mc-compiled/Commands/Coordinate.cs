using System;

namespace mc_compiled.Commands
{
    /// <summary>
    /// An integer or float with the option of being relative or facing offset.
    /// </summary>
    public struct Coordinate
    {
        public static readonly Coordinate zero = new Coordinate(0, false, false, false);
        public static readonly Coordinate here = new Coordinate(0, false, true, false);
        public static readonly Coordinate facingHere = new Coordinate(0, false, false, true);

        public float valueFloat;
        public int valueInteger;

        public readonly bool isFloat;
        private readonly bool isRelative;
        private readonly bool isFacingOffset;

        public static implicit operator Coordinate(int convert) => new Coordinate(convert, false, false, false);
        public static implicit operator Coordinate(float convert) => new Coordinate(convert, true, false, false);
        public Coordinate(float value, bool isFloat, bool isRelative, bool isFacingOffset)
        {
            valueFloat = value;
            valueInteger = (int)Math.Round(value);
            this.isFloat = isFloat;
            this.isRelative = isRelative;
            this.isFacingOffset = isFacingOffset;
        }
        public Coordinate(int value, bool isFloat, bool isRelative, bool isFacingOffset)
        {
            valueFloat = value;
            valueInteger = value;
            this.isFloat = isFloat;
            this.isRelative = isRelative;
            this.isFacingOffset = isFacingOffset;
        }
        public Coordinate(Coordinate other)
        {
            valueFloat = other.valueFloat;
            valueInteger = other.valueInteger;
            isFloat = other.isFloat;
            isRelative = other.isRelative;
            isFacingOffset = other.isFacingOffset;
        }

        /// <summary>
        /// Parse this coordinate value. Returns null if not succeeded.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Coordinate? Parse(string str)
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
                return new Coordinate(i, false, rel, off);
            else if (float.TryParse(str, out float f))
                return new Coordinate(f, true, rel, off);
            else
                return new Coordinate(0, false, rel, off); // default to 0
        }
        /// <summary>
        /// Determine if the size of these corner points can be determined at compile-time
        /// by ensuring every coordinate is either relative or exact.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public static bool SizeKnown(params Coordinate[] coords)
        {
            for(int i = 1; i < coords.Length; i++)
            {
                Coordinate a = coords[i - 1];
                Coordinate b = coords[i];
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
                s = (valueFloat == 0f && anyRelative) ? "" : valueFloat.ToString();
            else
                s = (valueInteger == 0 && anyRelative) ? "" : valueInteger.ToString();

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
            string s = (requestInteger ? valueInteger : isFloat ? valueFloat : valueInteger).ToString();

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
        public static Coordinate Min(Coordinate a, Coordinate b)
        {
            if (a.valueFloat < b.valueFloat)
                return a;
            if (a.valueFloat > b.valueFloat)
                return b;
            return a; // default
        }
        /// <summary>
        /// Return the larger of the two coords.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Coordinate Max(Coordinate a, Coordinate b)
        {
            if (a.valueFloat < b.valueFloat)
                return b;
            if (a.valueFloat > b.valueFloat)
                return a;
            return a; // default
        }

        /// <summary>
        /// Returns if this coord has any effect on the resulting location. (aka, non-relative and non-zero)
        /// </summary>
        public bool HasEffect
        {
            get
            {
                if (isRelative || isFacingOffset)
                    return isFloat ? valueFloat != 0f : valueInteger != 0;

                return true;
            }
        }
        public override bool Equals(object obj)
        {
            return obj is Coordinate coord &&
                   valueFloat == coord.valueFloat &&
                   valueInteger == coord.valueInteger &&
                   isFloat == coord.isFloat &&
                   isRelative == coord.isRelative &&
                   isFacingOffset == coord.isFacingOffset;
        }

        public override int GetHashCode()
        {
            int hashCode = 1648134579;
            hashCode = hashCode * -1521134295 + valueFloat.GetHashCode();
            hashCode = hashCode * -1521134295 + valueInteger.GetHashCode();
            hashCode = hashCode * -1521134295 + isFloat.GetHashCode();
            hashCode = hashCode * -1521134295 + isRelative.GetHashCode();
            hashCode = hashCode * -1521134295 + isFacingOffset.GetHashCode();
            return hashCode;
        }

        public static Coordinate operator -(Coordinate a)
        {
            a.valueInteger *= -1;
            a.valueFloat *= -1f;
            return a;
        }

        public static Coordinate operator +(Coordinate a, Coordinate b)
        {
            if (a.isFloat || b.isFloat)
                return new Coordinate(a.valueFloat + b.valueFloat, true, a.isRelative, a.isFacingOffset);
            else
                return new Coordinate(a.valueInteger + b.valueInteger, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator -(Coordinate a, Coordinate b)
        {
            if (a.isFloat || b.isFloat)
                return new Coordinate(a.valueFloat - b.valueFloat, true, a.isRelative, a.isFacingOffset);
            else
                return new Coordinate(a.valueInteger - b.valueInteger, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator *(Coordinate a, Coordinate b)
        {
            if (a.isFloat || b.isFloat)
                return new Coordinate(a.valueFloat * b.valueFloat, true, a.isRelative, a.isFacingOffset);
            else
                return new Coordinate(a.valueInteger * b.valueInteger, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator /(Coordinate a, Coordinate b)
        {
            if (a.isFloat || b.isFloat)
                return new Coordinate(a.valueFloat / b.valueFloat, true, a.isRelative, a.isFacingOffset);
            else
                return new Coordinate(a.valueInteger / b.valueInteger, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator %(Coordinate a, Coordinate b)
        {
            if (a.isFloat || b.isFloat)
                return new Coordinate(a.valueFloat % b.valueFloat, true, a.isRelative, a.isFacingOffset);
            else
                return new Coordinate(a.valueInteger % b.valueInteger, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator +(Coordinate a, int b)
        {
            if (a.isFloat)
                return new Coordinate(a.valueFloat + b, true, a.isRelative, a.isFacingOffset);
            else
                return new Coordinate(a.valueInteger + b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator -(Coordinate a, int b)
        {
            if (a.isFloat)
                return new Coordinate(a.valueFloat - b, true, a.isRelative, a.isFacingOffset);
            else
                return new Coordinate(a.valueInteger - b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator *(Coordinate a, int b)
        {
            if (a.isFloat)
                return new Coordinate(a.valueFloat * b, true, a.isRelative, a.isFacingOffset);
            else
                return new Coordinate(a.valueInteger * b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator /(Coordinate a, int b)
        {
            if (a.isFloat)
                return new Coordinate(a.valueFloat / b, true, a.isRelative, a.isFacingOffset);
            else
                return new Coordinate(a.valueInteger / b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator %(Coordinate a, int b)
        {
            if (a.isFloat)
                return new Coordinate(a.valueFloat % b, true, a.isRelative, a.isFacingOffset);
            else
                return new Coordinate(a.valueInteger % b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator +(Coordinate a, float b)
        {
            return new Coordinate(a.valueFloat + b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator -(Coordinate a, float b)
        {
            return new Coordinate(a.valueFloat - b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator *(Coordinate a, float b)
        {
            return new Coordinate(a.valueFloat * b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator /(Coordinate a, float b)
        {
            return new Coordinate(a.valueFloat / b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator %(Coordinate a, float b)
        {
            return new Coordinate(a.valueFloat % b, true, a.isRelative, a.isFacingOffset);
        }


    }
}
