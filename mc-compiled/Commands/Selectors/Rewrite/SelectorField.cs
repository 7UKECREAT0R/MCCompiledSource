using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Rewrite
{
    /// <summary>
    /// A generic SelectorField with no specific given type. To properly create a field for a class, use <see cref="SelectorField{T}"/>
    /// </summary>
    public abstract class SelectorField
    {
        public readonly Selector parent;
        public readonly string name;


        public bool HasValue
        {
            get; protected set;
        }

        protected SelectorField(string name, Selector parent)
        {
            this.HasValue = false;
            this.name = name;
            this.parent = parent;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Base">The type this object holds.</typeparam>
    public interface ISelectorFieldObject<Base>
    {
        /// <summary>
        /// Parse a string into this field.
        /// </summary>
        /// <param name="input"></param>
        void Parse(string input);
        /// <summary>
        /// Perform a deep clone of this object.
        /// </summary>
        /// <returns>The cloned instance of this object.</returns>
        object Clone();

        /// <summary>
        /// Set the value of this selector field.
        /// </summary>
        /// <param name="value"></param>
        void SetValue(Base value);
        /// <summary>
        /// Set the value of this selector field to nothing.
        /// </summary>
        void SetValue();

        /// <summary>
        /// Get the value of this field.
        /// </summary>
        /// <returns></returns>
        Base GetValue();
        /// <summary>
        /// Get the value of this field as a string being formatted in a selector.
        /// </summary>
        /// <returns></returns>
        string GetString();
    }

    /// <summary>
    /// A field in a selector.
    /// </summary>
    /// <typeparam name="Wrapper">The type of the wrapper class that holds the object.</typeparam>
    /// <typeparam name="Internal">The type of the internal, base value which the wrapper itself holds.</typeparam>
    public abstract class SelectorField<Wrapper, Internal> : SelectorField where Wrapper : ISelectorFieldObject<Internal>, new()
    {
        static Type fieldType;
        static bool isGeneric;

        readonly Wrapper valueWrapper;

        static SelectorField()
        {
            fieldType = typeof(Internal);
            isGeneric = fieldType.IsGenericType;
        }
        public SelectorField(string name, Selector parent) : base(name, parent)
        {
            this.valueWrapper = new Wrapper();
        }

        /// <summary>
        /// Set the value of this field.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetValue(Internal value)
        {
            if (!isGeneric && value == null)
            {
                SetValue();
                return;
            }
            else
                this.HasValue = true;

            this.valueWrapper.SetValue(value);
        }
        /// <summary>
        /// Reset this field back to nothing. Will not be included in the ToString'd selector.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public void SetValue()
        {
            this.HasValue = false;
            this.valueWrapper.SetValue();
        }
        /// <summary>
        /// Get the value of this field.
        /// </summary>
        public Wrapper WrapperValue
        {
            get => this.valueWrapper;
        }
        public Internal Value
        {
            get => this.valueWrapper.GetValue();
            set => this.valueWrapper.SetValue(value);
        }

        /// <summary>
        /// Get the resulting Minecraft selector fields e.g., [ "name=Gerald", "name=!Boogey" ]
        /// </summary>
        /// <returns>An array, or <b>null</b>.</returns>
        public string[] GetResult()
        {
            if(!HasValue)
                return null;

             return new[] { name + '=' + valueWrapper.GetString() };
        }
        /// <summary>
        /// Parse this string into this field.
        /// </summary>
        /// <param name="str"></param>
        public void Parse(string str)
        {
            this.valueWrapper.Parse(str);
        }
    }
}
