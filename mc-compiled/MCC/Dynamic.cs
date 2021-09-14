using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    public struct Dynamic
    {
        public enum Type
        {
            INTEGER, DECIMAL, STRING
        }
        public struct Data
        {
            public long i;
            public double d;
            public string s;
        }

        public Type type;
        public Data data;

        private Dynamic(Type type, Data data)
        {
            this.type = type;
            this.data = data;
        }
        public Dynamic(int i)
        {
            type = Type.INTEGER;
            data.i = i;
            data.d = i;
            data.s = i.ToString();
        }
        public Dynamic(long i)
        {
            type = Type.INTEGER;
            data.i = i;
            data.d = i;
            data.s = i.ToString();
        }
        public Dynamic(double d)
        {
            type = Type.DECIMAL;
            data.d = d;
            data.i = (int)d;
            data.s = d.ToString();
        }
        public Dynamic(string str)
        {
            type = Type.STRING;
            data.s = str;
            data.i = 0;
            data.d = 0;
        }

        public static Dynamic operator ++(Dynamic a)
        {
            if (a.type == Type.STRING)
                throw new Exception("Cannot PPINC a string variable.");
            a.data.i++;
            a.data.d += 1.0;
            a.data.s = a.data.i.ToString();
            return a;
        }
        public static Dynamic operator --(Dynamic a)
        {
            if (a.type == Type.STRING)
                throw new Exception("Cannot PPDEC a string variable.");
            a.data.i--;
            a.data.d -= 1.0;
            a.data.s = a.data.i.ToString();
            return a;
        }

        public static Dynamic operator +(Dynamic a, Dynamic b)
        {
            bool str1 = a.type == Type.STRING;
            bool str2 = b.type == Type.STRING;
            if (str1 & str2)
                return new Dynamic(a.data.s + b.data.s);
            else if (str1 | str2)
                throw new Exception("PPADD incompatible types.");

            if (a.type == Type.INTEGER && b.type == Type.INTEGER)
                return new Dynamic(a.data.i + b.data.i);
            else
                return new Dynamic(a.data.d + b.data.d);
        }
        public static Dynamic operator -(Dynamic a, Dynamic b)
        {
            bool str1 = a.type == Type.STRING;
            bool str2 = b.type == Type.STRING;

            if (str1 | str2)
                throw new Exception("PPSUB incompatible types.");

            if (a.type == Type.INTEGER && b.type == Type.INTEGER)
                return new Dynamic(a.data.i - b.data.i);
            else
                return new Dynamic(a.data.d - b.data.d);
        }
        public static Dynamic operator *(Dynamic a, Dynamic b)
        {
            bool str1 = a.type == Type.STRING;
            bool str2 = b.type == Type.STRING;

            if (str1 | str2)
                throw new Exception("PPMUL incompatible types.");

            if (a.type == Type.INTEGER && b.type == Type.INTEGER)
                return new Dynamic(a.data.i * b.data.i);
            else
                return new Dynamic(a.data.d * b.data.d);
        }
        public static Dynamic operator /(Dynamic a, Dynamic b)
        {
            bool str1 = a.type == Type.STRING;
            bool str2 = b.type == Type.STRING;

            if (str1 | str2)
                throw new Exception("PPDIV incompatible types.");

            if (a.type == Type.INTEGER && b.type == Type.INTEGER)
                return new Dynamic(a.data.i / b.data.i);
            else
                return new Dynamic(a.data.d / b.data.d);
        }
        public static Dynamic operator %(Dynamic a, Dynamic b)
        {
            bool str1 = a.type == Type.STRING;
            bool str2 = b.type == Type.STRING;

            if (str1 | str2)
                throw new Exception("PPMOD incompatible types.");

            if (a.type == Type.INTEGER && b.type == Type.INTEGER)
                return new Dynamic(a.data.i % b.data.i);
            else
                return new Dynamic(a.data.d % b.data.d);
        }

        public static bool operator ==(Dynamic a, Dynamic b)
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
        public static bool operator !=(Dynamic a, Dynamic b)
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
        public static bool operator >(Dynamic a, Dynamic b)
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
        public static bool operator <(Dynamic a, Dynamic b)
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
            if (obj is Dynamic dynamic)
                return this == dynamic;
            else return false;
        }
        public override string ToString()
        {
            return data.s;
        }

        public static Dynamic Parse(string str)
        {
            if (long.TryParse(str, out long l))
                return new Dynamic(l);
            else if (double.TryParse(str, out double d))
                return new Dynamic(d);
            else return new Dynamic(str);
        }
    }
}
