using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    public struct LegacyDynamic
    {
        /// <summary>
        /// Base type of the variable data.
        /// </summary>
        public enum Type
        {
            INTEGER, DECIMAL, STRING
        }
        /// <summary>
        /// The data held in this variable.
        /// </summary>
        public struct Data
        {
            public long i;
            public float d;
            public string s;

            // Modifier data for AltType
            public long altData;
        }

        public Type type;
        public Data data;

        private LegacyDynamic(Type type, Data data)
        {
            this.type = type;
            this.data = data;
        }
        public LegacyDynamic(int i)
        {
            type = Type.INTEGER;
            data.i = i;
            data.d = i;
            data.s = i.ToString();
            data.altData = 0;

            object o = (object)29;

            if(o is int)
            {

            }

        }
        public LegacyDynamic(long i)
        {
            type = Type.INTEGER;
            data.i = i;
            data.d = i;
            data.s = i.ToString();
            data.altData = 0;
        }
        public LegacyDynamic(float d)
        {
            type = Type.DECIMAL;
            data.d = d;
            data.i = (int)d;
            data.s = d.ToString();
            data.altData = 0;
        }
        public LegacyDynamic(string str)
        {
            type = Type.STRING;
            data.s = str;
            data.i = 0;
            data.d = 0;
            data.altData = 0;
        }
        public LegacyDynamic Inverse()
        {
            switch (type)
            {
                case Type.INTEGER:
                    return new LegacyDynamic(data.i * -1);
                case Type.DECIMAL:
                    return new LegacyDynamic(data.d * -1.0f);
                case Type.STRING:
                    return new LegacyDynamic(new string(data.s.Reverse().ToArray()));
                default:
                    return this;
            }
        }

        public static LegacyDynamic operator ++(LegacyDynamic a)
        {
            if (a.type == Type.STRING)
                throw new Exception("Cannot increment a string variable.");
            a.data.i++;
            a.data.d += 1.0f;
            a.data.s = a.data.i.ToString();
            return a;
        }
        public static LegacyDynamic operator --(LegacyDynamic a)
        {
            if (a.type == Type.STRING)
                throw new Exception("Cannot decrement a string variable.");
            a.data.i--;
            a.data.d -= 1.0f;
            a.data.s = a.data.i.ToString();
            return a;
        }

        public static LegacyDynamic operator +(LegacyDynamic a, LegacyDynamic b)
        {
            bool str1 = a.type == Type.STRING;
            bool str2 = b.type == Type.STRING;
            if (str1 & str2)
                return new LegacyDynamic(a.data.s + b.data.s);
            else if (str1 | str2)
                throw new Exception("PPADD incompatible types.");

            if (a.type == Type.INTEGER && b.type == Type.INTEGER)
                return new LegacyDynamic(a.data.i + b.data.i);
            else
                return new LegacyDynamic(a.data.d + b.data.d);
        }
        public static LegacyDynamic operator -(LegacyDynamic a, LegacyDynamic b)
        {
            bool str1 = a.type == Type.STRING;
            bool str2 = b.type == Type.STRING;

            if (str1 | str2)
                throw new Exception("PPSUB incompatible types.");

            if (a.type == Type.INTEGER && b.type == Type.INTEGER)
                return new LegacyDynamic(a.data.i - b.data.i);
            else
                return new LegacyDynamic(a.data.d - b.data.d);
        }
        public static LegacyDynamic operator *(LegacyDynamic a, LegacyDynamic b)
        {
            bool str1 = a.type == Type.STRING;
            bool str2 = b.type == Type.STRING;

            if (str1 | str2)
                throw new Exception("PPMUL incompatible types.");

            if (a.type == Type.INTEGER && b.type == Type.INTEGER)
                return new LegacyDynamic(a.data.i * b.data.i);
            else
                return new LegacyDynamic(a.data.d * b.data.d);
        }
        public static LegacyDynamic operator /(LegacyDynamic a, LegacyDynamic b)
        {
            bool str1 = a.type == Type.STRING;
            bool str2 = b.type == Type.STRING;

            if (str1 | str2)
                throw new Exception("PPDIV incompatible types.");

            if (a.type == Type.INTEGER && b.type == Type.INTEGER)
                return new LegacyDynamic(a.data.i / b.data.i);
            else
                return new LegacyDynamic(a.data.d / b.data.d);
        }
        public static LegacyDynamic operator %(LegacyDynamic a, LegacyDynamic b)
        {
            bool str1 = a.type == Type.STRING;
            bool str2 = b.type == Type.STRING;

            if (str1 | str2)
                throw new Exception("PPMOD incompatible types.");

            if (a.type == Type.INTEGER && b.type == Type.INTEGER)
                return new LegacyDynamic(a.data.i % b.data.i);
            else
                return new LegacyDynamic(a.data.d % b.data.d);
        }

        public static bool operator ==(LegacyDynamic a, LegacyDynamic b)
        {
            if (a.type != b.type)
                return false;

            if (a.type == Type.INTEGER)
                return a.data.i == b.data.i;
            else if (a.type == Type.DECIMAL)
                return a.data.d == b.data.d;
            else
                return a.data.s.Equals(b.data.s);
        }
        public static bool operator !=(LegacyDynamic a, LegacyDynamic b)
        {
            if (a.type != b.type)
                return true;

            if (a.type == Type.INTEGER)
                return a.data.i != b.data.i;
            else if (a.type == Type.DECIMAL)
                return a.data.d != b.data.d;
            else
                return !a.data.s.Equals(b.data.s);
        }
        public static bool operator >(LegacyDynamic a, LegacyDynamic b)
        {
            if (a.type != b.type)
                return false;
            if (a.type == Type.STRING)
                return false;

            if (a.type == Type.INTEGER)
                return a.data.i > b.data.i;
            else
                return a.data.d > b.data.d;
        }
        public static bool operator <(LegacyDynamic a, LegacyDynamic b)
        {
            if (a.type != b.type)
                return false;
            if (a.type == Type.STRING)
                return false;

            if (a.type == Type.INTEGER)
                return a.data.i < b.data.i;
            else
                return a.data.d < b.data.d;
        }

        public override int GetHashCode()
        {
            return data.s.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is LegacyDynamic dynamic)
                return this == dynamic;
            else return false;
        }
        public override string ToString()
        {
            return data.s;
        }

        public static LegacyDynamic Parse(string str)
        {
            if (long.TryParse(str, out long l))
                return new LegacyDynamic(l);
            else if (float.TryParse(str, out float d))
                return new LegacyDynamic(d);
            else return new LegacyDynamic(str);
        }
        
        /// <summary>
        /// Get the whole number from this decimal.
        /// </summary>
        /// <returns></returns>
        public long GetWholePart()
        {
            if (type == Type.STRING)
                throw new Exception("Cannot perform arithmatic with a string value.");
            if (type == Type.INTEGER)
                return data.i;

            return (int)Math.Floor(data.d);
        }
        /// <summary>
        /// Get the decimal part from this number.
        /// </summary>
        /// <returns></returns>
        public float GetDecimalPart()
        {
            if (type == Type.STRING)
                throw new Exception("Cannot perform arithmatic with a string value.");
            if (type == Type.INTEGER)
                return 0.0f; // No decimal part on integers.

            return Math.Abs(data.d) % 1.0f;
        }
        /// <summary>
        /// Get the decimal part from this number to a certain precision (in digits after point).
        /// </summary>
        /// <returns></returns>
        public float GetDecimalPart(int withPrecision)
        {
            throw new NotImplementedException();
        }
    }
}
