﻿using System;

namespace mc_compiled.Commands
{
    /// <summary>
    /// An integer or float with the option of being relative or facing offset.
    /// </summary>
    public struct Coordinate : IComparable<Coordinate>
    {
        public static readonly Coordinate zero = new Coordinate(0, false, false, false);
        public static readonly Coordinate here = new Coordinate(0, false, true, false);
        public static readonly Coordinate facingHere = new Coordinate(0, false, false, true);

        public decimal valueDecimal;
        public int valueInteger;

        public readonly bool isDecimal;
        private readonly bool isRelative;
        private readonly bool isFacingOffset;

        public static implicit operator Coordinate(int convert) => new Coordinate(convert, false, false, false);
        public static implicit operator Coordinate(decimal convert) => new Coordinate(convert, true, false, false);
        public Coordinate(decimal value, bool isDecimal, bool isRelative, bool isFacingOffset)
        {
            valueDecimal = value;
            valueInteger = (int)Math.Round(value);
            this.isDecimal = isDecimal;
            this.isRelative = isRelative;
            this.isFacingOffset = isFacingOffset;
        }
        public Coordinate(int value, bool isDecimal, bool isRelative, bool isFacingOffset)
        {
            valueDecimal = value;
            valueInteger = value;
            this.isDecimal = isDecimal;
            this.isRelative = isRelative;
            this.isFacingOffset = isFacingOffset;
        }
        public Coordinate(Coordinate other)
        {
            valueDecimal = other.valueDecimal;
            valueInteger = other.valueInteger;
            isDecimal = other.isDecimal;
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

            bool relative = str.StartsWith("~");
            bool lookOffset = str.StartsWith("^");
            
            str = str.TrimEnd('f');
            
            if (relative)
                str = str.Substring(1);
            if (lookOffset)
                str = str.Substring(1);

            if (int.TryParse(str, out int i))
                return new Coordinate(i, false, relative, lookOffset);
            if (decimal.TryParse(str, out decimal d))
                return new Coordinate(d, true, relative, lookOffset);
            
            return new Coordinate(0, false, relative, lookOffset);
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

            if (isDecimal)
                s = (valueDecimal == decimal.Zero && anyRelative) ? "" : valueDecimal.ToString();
            else
                s = (valueInteger == 0 && anyRelative) ? "" : valueInteger.ToString();

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
            if (a <= b)
                return a;
            return b;
        }
        /// <summary>
        /// Return the larger of the two coords.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Coordinate Max(Coordinate a, Coordinate b)
        {
            if (a >= b)
                return a;
            return b;
        }

        /// <summary>
        /// Returns if this coord has any effect on the resulting location. (aka, non-relative and non-zero)
        /// </summary>
        public bool HasEffect
        {
            get
            {
                if (isRelative || isFacingOffset)
                    return isDecimal ? valueDecimal != decimal.Zero : valueInteger != 0;

                return true;
            }
        }
        public override bool Equals(object obj)
        {
            return obj is Coordinate coordinate &&
                   valueDecimal == coordinate.valueDecimal &&
                   valueInteger == coordinate.valueInteger &&
                   isDecimal == coordinate.isDecimal &&
                   isRelative == coordinate.isRelative &&
                   isFacingOffset == coordinate.isFacingOffset;
        }
        public override int GetHashCode()
        {
            int hashCode = 1648134579;
            hashCode = hashCode * -1521134295 + valueDecimal.GetHashCode();
            hashCode = hashCode * -1521134295 + valueInteger.GetHashCode();
            hashCode = hashCode * -1521134295 + isDecimal.GetHashCode();
            hashCode = hashCode * -1521134295 + isRelative.GetHashCode();
            hashCode = hashCode * -1521134295 + isFacingOffset.GetHashCode();
            return hashCode;
        }
        public int CompareTo(Coordinate other)
        {
            if (!isDecimal && !other.isDecimal)
                return valueInteger.CompareTo(other.valueInteger);
            return valueDecimal.CompareTo(other.valueDecimal);
        }
        
        public static bool operator ==(Coordinate a, Coordinate b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Coordinate a, Coordinate b)
        {
            return !a.Equals(b);
        }
        public static bool operator <=(Coordinate a, Coordinate b)
        {
            return a < b || a == b;
        }
        public static bool operator >=(Coordinate a, Coordinate b)
        {
            return a > b || a == b;
        }
        public static bool operator <(Coordinate a, Coordinate b)
        {
            if (!a.isDecimal && !b.isDecimal)
                return a.valueInteger < b.valueInteger;
            return a.valueDecimal < b.valueDecimal;
        }
        public static bool operator >(Coordinate a, Coordinate b)
        {
            if (!a.isDecimal && !b.isDecimal)
                return a.valueInteger > b.valueInteger;
            return a.valueDecimal > b.valueDecimal;
        }
        public static bool operator ==(Coordinate a, int b)
        {
            return a.valueInteger == b;
        }
        public static bool operator !=(Coordinate a, int b)
        {
            return a.valueInteger != b;
        }
        public static bool operator <=(Coordinate a, int b)
        {
            return a < b || a == b;
        }
        public static bool operator >=(Coordinate a, int b)
        {
            return a > b || a == b;
        }
        public static bool operator <(Coordinate a, int b)
        {
            return a.valueInteger < b;
        }
        public static bool operator >(Coordinate a, int b)
        {
            return a.valueInteger < b;
        }
        public static bool operator ==(Coordinate a, decimal b)
        {
            return a.valueDecimal == b;
        }
        public static bool operator !=(Coordinate a, decimal b)
        {
            return a.valueDecimal != b;
        }
        public static bool operator <=(Coordinate a, decimal b)
        {
            return a < b || a == b;
        }
        public static bool operator >=(Coordinate a, decimal b)
        {
            return a > b || a == b;
        }
        public static bool operator <(Coordinate a, decimal b)
        {
            return a.valueDecimal < b;
        }
        public static bool operator >(Coordinate a, decimal b)
        {
            return a.valueDecimal < b;
        }
        
        public static Coordinate operator -(Coordinate a)
        {
            a.valueInteger *= -1;
            a.valueDecimal *= decimal.MinusOne;
            return a;
        }
        public static Coordinate operator +(Coordinate a, Coordinate b)
        {
            if (a.isDecimal || b.isDecimal)
                return new Coordinate(a.valueDecimal + b.valueDecimal, true, a.isRelative, a.isFacingOffset);
            return new Coordinate(a.valueInteger + b.valueInteger, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator -(Coordinate a, Coordinate b)
        {
            if (a.isDecimal || b.isDecimal)
                return new Coordinate(a.valueDecimal - b.valueDecimal, true, a.isRelative, a.isFacingOffset);
            return new Coordinate(a.valueInteger - b.valueInteger, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator *(Coordinate a, Coordinate b)
        {
            if (a.isDecimal || b.isDecimal)
                return new Coordinate(a.valueDecimal * b.valueDecimal, true, a.isRelative, a.isFacingOffset);
            return new Coordinate(a.valueInteger * b.valueInteger, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator /(Coordinate a, Coordinate b)
        {
            if (a.isDecimal || b.isDecimal)
                return new Coordinate(a.valueDecimal / b.valueDecimal, true, a.isRelative, a.isFacingOffset);
            return new Coordinate(a.valueInteger / b.valueInteger, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator %(Coordinate a, Coordinate b)
        {
            if (a.isDecimal || b.isDecimal)
                return new Coordinate(a.valueDecimal % b.valueDecimal, true, a.isRelative, a.isFacingOffset);
            return new Coordinate(a.valueInteger % b.valueInteger, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator +(Coordinate a, int b)
        {
            if (a.isDecimal)
                return new Coordinate(a.valueDecimal + b, true, a.isRelative, a.isFacingOffset);
            return new Coordinate(a.valueInteger + b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator -(Coordinate a, int b)
        {
            if (a.isDecimal)
                return new Coordinate(a.valueDecimal - b, true, a.isRelative, a.isFacingOffset);
            return new Coordinate(a.valueInteger - b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator *(Coordinate a, int b)
        {
            if (a.isDecimal)
                return new Coordinate(a.valueDecimal * b, true, a.isRelative, a.isFacingOffset);
            return new Coordinate(a.valueInteger * b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator /(Coordinate a, int b)
        {
            if (a.isDecimal)
                return new Coordinate(a.valueDecimal / b, true, a.isRelative, a.isFacingOffset);
            return new Coordinate(a.valueInteger / b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator %(Coordinate a, int b)
        {
            if (a.isDecimal)
                return new Coordinate(a.valueDecimal % b, true, a.isRelative, a.isFacingOffset);
            return new Coordinate(a.valueInteger % b, false, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator +(Coordinate a, decimal b)
        {
            return new Coordinate(a.valueDecimal + b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator -(Coordinate a, decimal b)
        {
            return new Coordinate(a.valueDecimal - b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator *(Coordinate a, decimal b)
        {
            return new Coordinate(a.valueDecimal * b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator /(Coordinate a, decimal b)
        {
            return new Coordinate(a.valueDecimal / b, true, a.isRelative, a.isFacingOffset);
        }
        public static Coordinate operator %(Coordinate a, decimal b)
        {
            return new Coordinate(a.valueDecimal % b, true, a.isRelative, a.isFacingOffset);
        }
    }
}