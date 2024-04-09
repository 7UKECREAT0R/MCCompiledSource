using System;

namespace mc_compiled.Commands
{
    /// <summary>
    /// Represents a range value in selector options. Examples:
    /// "3..999", "0..10", "50..100", "10", "!1", "!9..100", "!..3"
    /// </summary>
    public struct Range
    {
        /// <summary>
        /// A range which matches only 0.
        /// </summary>
        public static readonly Range zero = Of(0);
        /// <summary>
        /// A range which matches everything but 0.
        /// </summary>
        public static readonly Range notZero = new Range(0, true);

        public bool invert, single;
        public int? min;
        public int? max;

        public bool IsUnbounded => this.invert || !this.min.HasValue || !this.max.HasValue;

        public Range(int? min, int? max, bool not = false)
        {
            // ReSharper disable once MergeSequentialChecks
            if (min != null && max != null && min > max)
            {
                this.max = min;
                this.min = max;
            }
            else
            {
                this.min = min;
                this.max = max;
            }

            this.invert = not;
            this.single = false;
        }
        public Range(int number, bool not)
        {
            this.min = number;
            this.max = null;
            this.single = true;
            this.invert = not;
        }
        public Range(Range other)
        {
            this.invert = other.invert;
            this.single = other.single;

            if (other.min.HasValue)
                this.min = other.min.Value;
            else
                this.min = null;

            if (other.max.HasValue)
                this.max = other.max.Value;
            else
                this.max = null;
        }

        /// <summary>
        /// Returns a range which matches only the given input number.
        /// </summary>
        public static Range Of(int number)
        {
            return new Range(number, false);
        }
        /// <summary>
        /// Parse a range input into a Range structure.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Null if the parse failed.</returns>
        public static Range? Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            bool not = str.StartsWith("!");
            if (not)
                str = str.Substring(1);

            if (str.Contains(".."))
            {
                int index = str.IndexOf("..", StringComparison.Ordinal);
                if (index == 0) // ..10
                    return new Range(null, int.Parse(str.Substring(2)), not);
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

        /// <summary>
        /// Tries to parse a string and convert it into a Range.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="result">When this method returns, contains the parsed Range if successful; otherwise, null.</param>
        /// <returns>true if the string was successfully parsed; otherwise, false.</returns>
        public static bool TryParse(string str, out Range? result)
        {
            if (string.IsNullOrEmpty(str))
            {
                result = null;
                return false;
            }

            bool not = str.StartsWith("!");
            if (not)
                str = str.Substring(1);

            if (str.Contains(".."))
            {
                int index = str.IndexOf("..", StringComparison.Ordinal);
                if (index == 0) // ..10
                {
                    if (int.TryParse(str.Substring(2), out int maxVal))
                    {
                        result = new Range(null, maxVal, not);
                        return true;
                    }
                }
                else if (index + 2 >= str.Length) // 10..
                {
                    if (int.TryParse(str.Substring(0, index), out int minVal))
                    {
                        result = new Range(minVal, null, not);
                        return true;
                    }
                }
                else
                {
                    string _a = str.Substring(0, index);
                    string _b = str.Substring(index + 2);

                    if (int.TryParse(_a, out int a) && 
                        int.TryParse(_b, out int b))
                    {
                        result = new Range(a, b, not);
                        return true;
                    }
                }
            }
            else
            {
                if (int.TryParse(str, out int parse))
                {
                    result = new Range(parse, not);
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Returns this Range in the traditional minecraft-required format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.single)
            {
                if (this.invert)
                    return "!" + this.min;
                else
                    return this.min.ToString();
            }
            else
            {
                return (this.invert ? "!" : "") +
                    (this.min.HasValue ? this.min.Value.ToString() : "") + ".." +
                    (this.max.HasValue ? this.max.Value.ToString() : "");
            }
        }

        public static Range operator +(Range a, Range b)
        {
            if (b.single)
                return a + b.min.GetValueOrDefault(0);

            var copy = new Range(a);
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
                return a - b.min.GetValueOrDefault(0);

            var copy = new Range(a);
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
                return a * b.min.GetValueOrDefault(0);

            var copy = new Range(a);
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
                return a / b.min.GetValueOrDefault(0);

            var copy = new Range(a);
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
                return a % b.min.GetValueOrDefault(0);

            var copy = new Range(a);
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
                return a < b.min.GetValueOrDefault(0);

            return a.min < b.min;
        }
        public static bool operator >(Range a, Range b)
        {
            if (b.single)
                return a < b.min.GetValueOrDefault(0);

            int maxA = a.single ? a.min.GetValueOrDefault(0) : a.max.GetValueOrDefault(0);
            int maxB = b.max.GetValueOrDefault(0);
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
            var copy = new Range(a);
            copy.min += number;

            if(!copy.single)
                copy.max += number;

            return copy;
        }
        public static Range operator -(Range a, int number)
        {
            var copy = new Range(a);
            copy.min -= number;

            if (!copy.single)
                copy.max -= number;

            return copy;
        }
        public static Range operator *(Range a, int number)
        {
            var copy = new Range(a);
            copy.min *= number;

            if (!copy.single)
                copy.max *= number;

            return copy;
        }
        public static Range operator /(Range a, int number)
        {
            var copy = new Range(a);
            copy.min /= number;

            if (!copy.single)
                copy.max /= number;

            return copy;
        }
        public static Range operator %(Range a, int number)
        {
            var copy = new Range(a);
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

        public static Range operator +(Range a, decimal number)
        {
            var copy = new Range(a);
            copy.min += (int)number;

            if (!copy.single)
                copy.max += (int)number;

            return copy;
        }
        public static Range operator -(Range a, decimal number)
        {
            var copy = new Range(a);
            copy.min -= (int)number;

            if (!copy.single)
                copy.max -= (int)number;

            return copy;
        }
        public static Range operator *(Range a, decimal number)
        {
            var copy = new Range(a);
            copy.min *= (int)number;

            if (!copy.single)
                copy.max *= (int)number;

            return copy;
        }
        public static Range operator /(Range a, decimal number)
        {
            var copy = new Range(a);
            copy.min /= (int)number;

            if (!copy.single)
                copy.max /= (int)number;

            return copy;
        }
        public static Range operator %(Range a, decimal number)
        {
            var copy = new Range(a);
            copy.min %= (int)number;

            if (!copy.single)
                copy.max %= (int)number;

            return copy;
        }
        public static bool operator <(Range a, decimal number)
        {
            if (a.invert)
                return !(a.min < number);
            return a.min < number;
        }
        public static bool operator >(Range a, decimal number)
        {
            if (a.invert)
                return !(a.min > number);
            return a.min > number;
        }
        public static bool operator ==(Range a, decimal number)
        {
            if (a.single)
            {
                if (a.invert)
                    return a.min != number;
                return a.min == number;
            }

            if (a.invert)
                return number < a.min || number > a.max;
            return number >= a.min && number <= a.max;
        }
        public static bool operator !=(Range a, decimal number)
        {
            if (a.single)
            {
                if (a.invert)
                    return a.min == number;
                return a.min != number;
            }

            if (a.invert)
                return number >= a.min && number <= a.max;
            return number < a.min || number > a.max;
        }

        public bool Equals(Range other)
        {
            return this.invert == other.invert && this.single == other.single && this.min == other.min && this.max == other.max;
        }
        public override bool Equals(object obj)
        {
            return obj is Range other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.invert.GetHashCode();
                hashCode = (hashCode * 397) ^ this.single.GetHashCode();
                hashCode = (hashCode * 397) ^ this.min.GetHashCode();
                hashCode = (hashCode * 397) ^ this.max.GetHashCode();
                return hashCode;
            }
        }
    }
}
