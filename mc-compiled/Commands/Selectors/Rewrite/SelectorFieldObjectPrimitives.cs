using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Rewrite
{
    class GenericFieldObject<T> : ISelectorFieldObject<T>
    {
        T wrap;

        Action<GenericFieldObject<T>, string> parseFunction = null;
        Func<T, T> cloneFunction = null;

        public GenericFieldObject(T wrap)
        {
            this.wrap = wrap;
        }
        public GenericFieldObject<T> WithParseFunction(Action<GenericFieldObject<T>, string> function)
        {
            this.parseFunction = function;
            return this;
        }
        public GenericFieldObject<T> WithCloneFunction(Func<T, T> function)
        {
            this.cloneFunction = function;
            return this;
        }

        public void Parse(string str)
        {
            if(parseFunction == null)
                throw new NotImplementedException();
        }
        public object Clone()
        {
            if (cloneFunction == null)
                throw new NotImplementedException();

            T copy = cloneFunction(wrap);

            return (object)new GenericFieldObject<T>(copy)
                .WithParseFunction(parseFunction)
                .WithCloneFunction(cloneFunction);
        }
        public void SetValue(T value) => this.wrap = value;
        public void SetValue() => this.wrap = default;
        public T GetValue() => this.wrap;
        public string GetString() => this.wrap.ToString();
    }
    class CoordFieldObject : ISelectorFieldObject<Coord>
    {
        Coord wrap;
        public CoordFieldObject(Coord wrap)
        {
            this.wrap = wrap;
        }
        public static implicit operator Coord(CoordFieldObject me)
        {
            return me.wrap;
        }
        public static implicit operator CoordFieldObject(Coord me)
        {
            return new CoordFieldObject(me);
        }

        public void Parse(string str)
        {
            Coord? coord = Coord.Parse(str);

            if (coord.HasValue)
                SetValue(coord.Value);
            else
                SetValue();
        }
        public object Clone()
        {
            return (object)(new CoordFieldObject(new Coord(wrap)));
        }
        public void SetValue(Coord value) => this.wrap = value;
        public void SetValue() => this.wrap = default;
        public Coord GetValue() => this.wrap;
        public string GetString() => this.wrap.ToString();
    }
    class IntFieldObject : ISelectorFieldObject<int>
    {
        int wrap;
        public IntFieldObject(int wrap)
        {
            this.wrap = wrap;
        }
        public static implicit operator int(IntFieldObject me)
        {
            return me.wrap;
        }
        public static implicit operator IntFieldObject(int me)
        {
            return new IntFieldObject(me);
        }

        public void Parse(string str)
        {
            SetValue(int.Parse(str));
        }
        public object Clone()
        {
            return (object)(new IntFieldObject(wrap));
        }
        public void SetValue(int value) => this.wrap = value;
        public void SetValue() => this.wrap = default;
        public int GetValue() => this.wrap;
        public string GetString() => this.wrap.ToString();
    }
    class FloatFieldObject : ISelectorFieldObject<float>
    {
        float wrap;
        public FloatFieldObject(float wrap)
        {
            this.wrap = wrap;
        }
        public static implicit operator float(FloatFieldObject me)
        {
            return me.wrap;
        }
        public static implicit operator FloatFieldObject(float me)
        {
            return new FloatFieldObject(me);
        }

        public void Parse(string str)
        {
            SetValue(float.Parse(str));
        }
        public object Clone()
        {
            return (object)(new FloatFieldObject(wrap));
        }
        public void SetValue(float value) => this.wrap = value;
        public void SetValue() => this.wrap = default;
        public float GetValue() => this.wrap;
        public string GetString() => this.wrap.ToString();
    }
}
