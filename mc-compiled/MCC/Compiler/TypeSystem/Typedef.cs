using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        /// A scoreboard exception with formatted text: "Literal [{src}] could not be converted to {dst}"
        /// </summary>
        /// <param name="causer"></param>
        /// <param name="literal"></param>
        /// <returns></returns>
        protected static ScoreboardException LiteralConversionError(ScoreboardValue causer, TokenLiteral literal)
        {
            return new ScoreboardException($"Literal [{literal.AsString()}] could not be converted to {causer.type.TypeKeyword}", causer);
        }
        /// <summary>
        /// A scoreboard exception with formatted text: "Operation '{operation}' is unsupported by type '{src}'."
        /// </summary>
        /// <param name="causer"></param>
        /// <param name="literal"></param>
        /// <returns></returns>
        protected static ScoreboardException UnsupportedOperationError(ScoreboardValue src, UnsupportedOperationType operation)
        {
            return new ScoreboardException($"Operation '{operation}' is unsupported by type {src.type.TypeKeyword}.", src);
        }

        protected enum UnsupportedOperationType
        {
            AssignLiteral,
            AddLiteral,
            SubtractLiteral,
            Assign,
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulo,
            Swap
        }

        /// <summary>
        /// Returns the enum for this type.
        /// </summary>
        public abstract ScoreboardManager.ValueType TypeEnum { get; }
        /// <summary>
        /// Returns the keyword used to refer to this type.
        /// </summary>
        public abstract string TypeKeyword { get; }
        /// <summary>
        /// Can be inserted into an if-statement alone and be compared.
        /// </summary>
        public abstract bool CanCompareAlone { get; }
        /// <summary>
        /// The pattern needed after using this typedef keyword to populate its data, such as with `decimal N` where `N` is the pattern.<br />
        /// If <b>null</b> is returned, then this Typedef does not use data, and its <see cref="AcceptPattern(TokenLiteral[])"/> method should not be called.
        /// </summary>
        public virtual TypePattern SpecifyPattern { get => null; }

        /// <summary>
        /// Accepts the input pattern given by the user and returns a data object containing it.
        /// See <see cref="SpecifyPattern"/>. Do not call this method if it is null.
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual object AcceptPattern(TokenLiteral[] inputs) => null;

        /// <summary>
        /// Deep-clone the given data object as per this Typedef instance. You should probably be using <see cref="Typedef{T}.CloneData(T)"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract object CloneData(object data);

        /// <summary>
        /// Returns all scoreboard objectives that this type uses, with the given <see cref="ScoreboardValue"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal abstract string[] GetObjectives(ScoreboardValue input);
        /// <summary>
        /// Returns if this type can convert to the given type.
        /// </summary>
        /// <param name="type">The destination type.</param>
        /// <returns></returns>
        internal abstract bool CanConvertTo(Typedef type);

        /// <summary>
        /// Clones the given scoreboard value and returns the commands needed to perform it.
        /// Only should be called after checking beforehand via. <see cref="CanConvertTo(Typedef)"/>.
        /// </summary>
        /// <param name="src">The source value to convert. Its value will not be modified, rather it a clone will be made.</param>
        /// <param name="dst">The type/data that the value will be converted to; its destination.</param>
        /// <returns></returns>
        internal abstract string[] ConvertTo(ScoreboardValue src, ScoreboardValue dst);
        /// <summary>
        /// This is only used in cases where a type's internal data needs to be compared, like decimal values with mismatched precisions.<br />
        /// Use this as the comparison before running <see cref="CanConvertTo(Typedef)"/> and then <see cref="ConvertTo"/>. kthxbye
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        internal virtual bool NeedsToBeConvertedTo(ScoreboardValue src, ScoreboardValue dst)
        {
            return src.type.TypeEnum != dst.type.TypeEnum;
        }

        /// <summary>
        /// Setup temporary variables, return the commands to set it up, and return the JSON raw terms needed to display a value under this type.
        /// <br/>
        /// <b>Either field may be null, in which it will count as empty.</b>
        /// </summary>
        /// <param name="value">The value to get the JSON raw terms for. </param>
        /// <param name="index">The index of the rawtext value, used to uniquely identify the temp variables and whatnot so they don't collide with other rawtexts.</param>
        /// <returns></returns>
        internal abstract Tuple<string[], JSONRawTerm[]> ToRawText(ScoreboardValue value, ref int index);

        /// <summary>
        /// Returns the commands needed to assign a literal to this type.
        /// </summary>
        /// <param name="self">The scoreboard value that has this type.</param>
        /// <param name="literal">The literal to assign.</param>
        /// <returns></returns>
        internal abstract string[] AssignLiteral(ScoreboardValue self, TokenLiteral literal);
        /// <summary>
        /// Returns the commands needed to add a literal to this type.
        /// </summary>
        /// <param name="self">The scoreboard value that has this type.</param>
        /// <param name="literal">The literal to assign.</param>
        /// <returns></returns>
        internal abstract string[] AddLiteral(ScoreboardValue self, TokenLiteral literal);
        /// <summary>
        /// Returns the commands needed to subtract a literal from this type.
        /// </summary>
        /// <param name="self">The scoreboard value that has this type.</param>
        /// <param name="literal">The literal to assign.</param>
        /// <returns></returns>
        internal abstract string[] SubtractLiteral(ScoreboardValue self, TokenLiteral literal);

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
        internal abstract Range CompareAlone(bool invert);
        /// <summary>
        /// Compare a value with this type to a literal value. Returns both the setup commands needed, and the score comparisons needed.
        /// <br/>
        /// <b>Either field may be null, in which it will count as empty.</b>
        /// </summary>
        /// <param name="comparisonType">The comparison type.</param>
        /// <param name="self"></param>
        /// <param name="literal"></param>
        /// <returns></returns>
        internal abstract Tuple<string[], ConditionalSubcommandScore[]> CompareToLiteral(TokenCompare.Type comparisonType, ScoreboardValue self, TokenLiteral literal);

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
        protected abstract string[] _Modulo(ScoreboardValue self, ScoreboardValue other);
    }
    /// <summary>
    /// A type definition.
    /// </summary>
    /// <typeparam name="T">The data structure held inside the ScoreboardValues that are of this type.</typeparam>
    internal abstract class Typedef<T> : Typedef where T: ITypeStructure
    {
        protected override object CloneData(object data)
        {
            // data should be T
            var convert = (T)data;

            // call deep clone implementation
            return (T)convert.DeepClone();
        }
    }
}
