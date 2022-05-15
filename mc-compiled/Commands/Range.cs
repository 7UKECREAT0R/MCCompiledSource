using System;
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
        public Range(Range other)
        {
            invert = other.invert;
            single = other.single;

            if (other.min.HasValue)
                min = other.min.Value;
            else min = null;

            if (other.max.HasValue)
                max = other.max.Value;
            else max = null;
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
            {
                if (invert)
                    return "!" + min;
                else
                    return min.ToString();
            }
            else
            {
                return (invert ? "!" : "") +
                    (min.HasValue ? min.Value.ToString() : "") + ".." +
                    (max.HasValue ? max.Value.ToString() : "");
            }
        }

        public static Range operator +(Range a, Range b)
        {
            if (b.single)
                return a + b.min.Value;

            Range copy = new Range(a);
            if (copy.single)
                copy.min += b.min;
            else
            {
                copy.min += b.min;
                copy.max += b.max;
            }

            return copy;
        }
        public static Range operator -(Range a, Range b)
        {
            if (b.single)
                return a - b.min.Value;

            Range copy = new Range(a);
            if (copy.single)
                copy.min -= b.min;
            else
            {
                copy.min -= b.min;
                copy.max -= b.max;
            }

            return copy;
        }
        public static Range operator *(Range a, Range b)
        {
            if (b.single)
                return a * b.min.Value;

            Range copy = new Range(a);
            if (copy.single)
                copy.min *= b.min;
            else
            {
                copy.min *= b.min;
                copy.max *= b.max;
            }

            return copy;
        }
        public static Range operator /(Range a, Range b)
        {
            if (b.single)
                return a / b.min.Value;

            Range copy = new Range(a);
            if (copy.single)
                copy.min /= b.min;
            else
            {
                copy.min /= b.min;
                copy.max /= b.max;
            }

            return copy;
        }
        public static Range operator %(Range a, Range b)
        {
            if (b.single)
                return a % b.min.Value;

            Range copy = new Range(a);
            if (copy.single)
                copy.min %= b.min;
            else
            {
                copy.min %= b.min;
                copy.max %= b.max;
            }

            return copy;
        }
        public static bool operator <(Range a, Range b)
        {
            if (b.single)
                return a < b.min.Value;

            return a.min < b.min;
        }
        public static bool operator >(Range a, Range b)
        {
            if (b.single)
                return a < b.min.Value;

            int maxA = a.single ? a.min.Value : a.max.Value;
            int maxB = b.single ? b.min.Value : b.max.Value;
            return maxA > maxB;
        }
        public static bool operator ==(Range a, Range b)
        {
            if (a.invert != b.invert || a.single != b.single || a.min != b.min)
                return false;

            if (!a.single && !b.single)
                return a.max == b.max;

            return true;
        }
        public static bool operator !=(Range a, Range b)
        {
            if (!a.single && !b.single)
                return a.invert != b.invert && a.single != b.single && a.min != b.min && a.max != b.max;
            else
                return a.invert != b.invert && a.single != b.single && a.min != b.min;
        }

        public static Range operator +(Range a, int number)
        {
            Range copy = new Range(a);
            copy.min += number;

            if(!copy.single)
                copy.max += number;

            return copy;
        }
        public static Range operator -(Range a, int number)
        {
            Range copy = new Range(a);
            copy.min -= number;

            if (!copy.single)
                copy.max -= number;

            return copy;
        }
        public static Range operator *(Range a, int number)
        {
            Range copy = new Range(a);
            copy.min *= number;

            if (!copy.single)
                copy.max *= number;

            return copy;
        }
        public static Range operator /(Range a, int number)
        {
            Range copy = new Range(a);
            copy.min /= number;

            if (!copy.single)
                copy.max /= number;

            return copy;
        }
        public static Range operator %(Range a, int number)
        {
            Range copy = new Range(a);
            copy.min %= number;

            if (!copy.single)
                copy.max %= number;

            return copy;
        }
        public static bool operator <(Range a, int number)
        {
            if (a.invert)
                return !(a.min < number);
            else
                return a.min < number;
        }
        public static bool operator >(Range a, int number)
        {
            if (a.invert)
                return !(a.min > number);
            else
                return a.min > number;
        }
        public static bool operator ==(Range a, int number)
        {
            if (a.single)
            {
                if (a.invert)
                    return a.min != number;
                else
                    return a.min == number;
            }

            if (a.invert)
                return number < a.min || number > a.max;
            else
                return number >= a.min && number <= a.max;
        }
        public static bool operator !=(Range a, int number)
        {
            if (a.single)
            {
                if (a.invert)
                    return a.min == number;
                else
                    return a.min != number;
            }

            if (a.invert)
                return number >= a.min && number <= a.max;
            else
                return number < a.min || number > a.max;
        }

        public static Range operator +(Range a, float number)
        {
            Range copy = new Range(a);
            copy.min += (int)number;

            if (!copy.single)
                copy.max += (int)number;

            return copy;
        }
        public static Range operator -(Range a, float number)
        {
            Range copy = new Range(a);
            copy.min -= (int)number;

            if (!copy.single)
                copy.max -= (int)number;

            return copy;
        }
        public static Range operator *(Range a, float number)
        {
            Range copy = new Range(a);
            copy.min *= (int)number;

            if (!copy.single)
                copy.max *= (int)number;

            return copy;
        }
        public static Range operator /(Range a, float number)
        {
            Range copy = new Range(a);
            copy.min /= (int)number;

            if (!copy.single)
                copy.max /= (int)number;

            return copy;
        }
        public static Range operator %(Range a, float number)
        {
            Range copy = new Range(a);
            copy.min %= (int)number;

            if (!copy.single)
                copy.max %= (int)number;

            return copy;
        }
        public static bool operator <(Range a, float number)
        {
            if (a.invert)
                return !(a.min < number);
            else
                return a.min < number;
        }
        public static bool operator >(Range a, float number)
        {
            if (a.invert)
                return !(a.min > number);
            else
                return a.min > number;
        }
        public static bool operator ==(Range a, float number)
        {
            if (a.single)
            {
                if (a.invert)
                    return a.min != number;
                else
                    return a.min == number;
            }

            if (a.invert)
                return number < a.min || number > a.max;
            else
                return number >= a.min && number <= a.max;
        }
        public static bool operator !=(Range a, float number)
        {
            if (a.single)
            {
                if (a.invert)
                    return a.min == number;
                else
                    return a.min != number;
            }

            if (a.invert)
                return number >= a.min && number <= a.max;
            else
                return number < a.min || number > a.max;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (obj is Range)
                return this == (Range)obj;
            if (obj is int)
                return this == (int)obj;
            if (obj is float)
                return this == (float)obj;


            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
