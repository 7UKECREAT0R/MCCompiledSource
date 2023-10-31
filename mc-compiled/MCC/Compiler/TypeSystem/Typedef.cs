using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler.TypeSystem
{
    /// <summary>
    /// A type definition. Please extend <see cref="Typedef{T}"/> though.
    /// </summary>
    public abstract class Typedef
    {
        /// <summary>
        /// Please don't use this if you need inheritance support.
        /// </summary>
        public enum Type
        {
            Integer,
            FixedDecimal,
            Boolean,
            Time
        }

        /// <summary>
        /// Returns the enum for this type.
        /// </summary>
        public abstract Type TypeEnum { get; }
        /// <summary>
        /// Returns the keyword used to refer to this type.
        /// </summary>
        public abstract string TypeKeyword { get; }
        /// <summary>
        /// Can be inserted into an if-statement alone and be compared.
        /// </summary>
        public abstract bool CanCompareAlone { get; }

        /// <summary>
        /// Deep-clone the given data object as per this Typedef instance. You should probably be using <see cref="Typedef{T}.CloneData(T)"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract object CloneData(object data);

        /// <summary>
        /// Returns if this type can convert to the given type.
        /// </summary>
        /// <param name="type">The destination type.</param>
        /// <returns></returns>
        internal abstract bool CanConvertTo(Typedef type);
        /// <summary>
        /// Returns if the given value needs to convert to this type.
        /// Calls <see cref="NeedsConvertToSelf"/>
        /// </summary>
        internal virtual bool NeedsConvertTo(ScoreboardValue value)
        {
            if (this.TypeEnum == value.type.TypeEnum)
                return NeedsConvertToSelf(value);
            return false;
        }
        /// <summary>
        /// Returns if the given value is capable of converting to this type, given that it's the exact same type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal abstract bool NeedsConvertToSelf(ScoreboardValue value);
        /// <summary>
        /// Clones the given scoreboard value and returns the commands needed to perform it.
        /// </summary>
        /// <param name="type">The source type/data that the value will be converted to.</param>
        /// <param name="value">The value to convert. Its value will not be modified, rather it a clone will be made.</param>
        /// <param name="newValue">(out) The new, converted value which will contain the converted value after the commands run.</param>
        /// <returns></returns>
        internal abstract string[] ConvertTo(ScoreboardValue type, ScoreboardValue value, out ScoreboardValue newValue);

        /// <summary>
        /// Setup temporary variables, return the commands to set it up, and return the JSON raw terms needed to display a value under this type.
        /// </summary>
        /// <param name="value">The value to get the JSON raw terms for.</param>
        /// <param name="index">The index of the rawtext value, used to uniquely identify the temp variables and whatnot so they don't collide with other rawtexts.</param>
        /// <returns></returns>
        internal abstract Tuple<string[], JSONRawTerm[]> ToRawText(ScoreboardValue value, int index);

        /// <summary>
        /// Returns the commands needed to assign a literal to this type.
        /// </summary>
        /// <param name="self">The scoreboard value that has this type.</param>
        /// <param name="literal">The literal to assign.</param>
        /// <returns></returns>
        internal abstract string[] AssignLiteral(ScoreboardValue self, TokenLiteral literal);
        /// <summary>
        /// Returns the commands needed to assign another value to self, given that they are both the same type and compatible.
        /// <code>
        ///     self = other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        protected abstract string[] _Assign(ScoreboardValue self, ScoreboardValue other);

        /// <summary>
        /// Returns the range needed to compare this type alone, given that <see cref="CanCompareAlone"/> is <b>true</b>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal abstract Range CompareAlone(ScoreboardValue value);
        /// <summary>
        /// Compare a value with this type to a literal value. Returns both the setup commands needed, and the scores entries needed to perform the comparison.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="literal"></param>
        /// <returns></returns>
        internal abstract Tuple<string[], ScoresEntry[]> CompareToLiteral(ScoreboardValue self, TokenLiteral literal);

        /// <summary>
        /// Returns the commands needed to add another value to self, given that they are both the same type and compatible.
        /// <code>
        ///     self += other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        protected abstract string[] _Add(ScoreboardValue self, ScoreboardValue other);
        /// <summary>
        /// Returns the commands needed to subtract another value from self, given that they are both the same type and compatible.
        /// <code>
        ///     self -= other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        protected abstract string[] _Subtract(ScoreboardValue self, ScoreboardValue other);
        /// <summary>
        /// Returns the commands needed to multiply self by another value, given that they are both the same type and compatible.
        /// <code>
        ///     self *= other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        protected abstract string[] _Multiply(ScoreboardValue self, ScoreboardValue other);
        /// <summary>
        /// Returns the commands needed to divide self by another value, given that they are both the same type and compatible.
        /// <code>
        ///     self /= other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        protected abstract string[] _Divide(ScoreboardValue self, ScoreboardValue other);
        /// <summary>
        /// Returns the commands needed to get the remainder of self after division with another value, given that they are both the same type and compatible.
        /// <code>
        ///     self %= other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal abstract string[] _Modulo(ScoreboardValue self, ScoreboardValue other);
        /// <summary>
        /// Returns the commands needed to subtract another value from self, given that they are both the same type and compatible.
        /// <code>
        ///     self >< other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        internal abstract string[] _Swap(ScoreboardValue self, ScoreboardValue other);
    }
    /// <summary>
    /// A type definition.
    /// </summary>
    /// <typeparam name="T">The data structure held inside the ScoreboardValues that are of this type.</typeparam>
    internal abstract class Typedef<T> : Typedef where T: ITypeStructure
    {
        internal abstract T CloneData(T data);
        protected override object CloneData(object data)
        {
            // data should be T
            T convert = (T)data;

            // call deep clone implementation
            return (T)convert.DeepClone();
        }
    }
}
