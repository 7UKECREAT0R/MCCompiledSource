using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    public abstract class SelectorField
    {
        public readonly Selector parent;
        public readonly string name;

        protected SelectorField(string name, Selector parent)
        {
            this.HasValue = false;
            this.name = name;
            this.parent = parent;
        }

        protected bool _inverted;
        public bool Inverted
        {
            get => _inverted;

            protected set
            {
                _inverted = value;

            }
        }
        public bool HasValue
        {
            get; protected set;
        }

        /// <summary>
        /// Get the resulting entries in the selector, if any, for this field.
        /// e.g., [ "name=Jonah" ]
        /// </summary>
        /// <returns>new string[0] if no entries are to be given.</returns>
        public abstract string[] GetResult();
    }
    /// <summary>
    /// A field in a selector.
    /// </summary>
    public class SelectorField<T> : SelectorField
    {
        public static readonly bool isGeneric;
        public static readonly Type fieldType;

        Func<SelectorField<T>, string[]> resultProvider;
        Action<SelectorField<T>> inverter;

        T value;

        static SelectorField()
        {
            fieldType = typeof(T);
            isGeneric = fieldType.IsGenericType;
        }
        public SelectorField(string name, Selector parent) : base(name, parent) { }
        public SelectorField<T> WithResultProvider(Func<SelectorField<T>, string[]> provider)
        {
            this.resultProvider = provider;
            return this;
        }

        /// <summary>
        /// Set the value of this field.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetValue(T value)
        {
            if (value == null && !isGeneric)
            {
                SetValue();
                return;
            } else 
               this.HasValue = true;

            this.value = value;

            if (this.ToString().StartsWith("!"))
                this._inverted = !this._inverted;
        }
        /// <summary>
        /// Reset this field back to nothing. Will not be included in the ToString'd selector.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetValue()
        {
            this.HasValue = false;
            this.value = default;
        }
        /// <summary>
        /// Get the value of this field.
        /// </summary>
        public T Value
        {
            get => this.value;
        }

        public override string[] GetResult()
        {
            if (!HasValue)
                return new string[0];
            if (resultProvider != null)
                return resultProvider(this);

            // Default result provider
            return new string[]
            {
                name + "=" + value.ToString()
            };
        }
    }
}
