using mc_compiled.Commands.Execute;
using mc_compiled.Json;
using System;
using System.Collections.Generic;

namespace mc_compiled.MCC.Compiler.TypeSystem
{
    /// <summary>
    /// A type definition. Extend <see cref="Typedef{T}"/> instead if this type has a data structure (being T).
    /// </summary>
    public abstract partial class Typedef
    {
        /// <summary>
        /// A scoreboard exception with formatted text: "Literal [{src}] could not be converted to {dst}"
        /// </summary>
        /// <returns></returns>
        protected static StatementException LiteralConversionError(ScoreboardValue value, TokenLiteral literal,
            Statement callingStatement)
        {
            return new StatementException(callingStatement, $"Literal [{literal.AsString()}] could not be converted to {value.type.TypeKeyword}");
        }
        /// <summary>
        /// A scoreboard exception with formatted text: "Operation '{operation}' is unsupported by type '{src}'."
        /// </summary>
        /// <returns></returns>
        protected static StatementException UnsupportedOperationError(ScoreboardValue value,
            UnsupportedOperationType operation, Statement callingStatement)
        {
            return new StatementException(callingStatement, $"Operation '{operation}' is unsupported by type {value.type.TypeKeyword}.");
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

        public override bool Equals(object obj)
        {
            return (obj is Typedef def) && def.TypeEnum == this.TypeEnum;
        }
        /// <summary>
        /// Returns the HashCode of this type. Short for: <code>this.TypeEnum.GetHashCode();</code>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.TypeEnum.GetHashCode();
        }
        
        /// <summary>
        /// Returns the enum for this type.
        /// </summary>
        public abstract ScoreboardManager.ValueType TypeEnum { get; }
        /// <summary>
        /// Three character uppercase shortcode for when this type is used in temp values. e.g., "BLN", "INT", "DEC".
        /// </summary>
        public abstract string TypeShortcode { get; }
        /// <summary>
        /// Returns the keyword used to refer to this type, in UPPERCASE for comparison.
        /// </summary>
        public abstract string TypeKeyword { get; }
        /// <summary>
        /// Can be inserted into an if-statement alone and be compared.
        /// </summary>
        public abstract bool CanCompareAlone { get; }
        /// <summary>
        /// The pattern needed after using this typedef keyword to populate its data, such as with `decimal N` where `N` is the pattern.<br />
        /// If <b>null</b> is returned, then this Typedef does not use data, and its <see cref="AcceptPattern"/> method should not be called.
        /// </summary>
        public virtual TypePattern SpecifyPattern => null;
        /// <summary>
        /// Returns if the given literal value can be accepted to construct data from.
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        public virtual bool CanAcceptLiteralForData(TokenLiteral literal) { return false; }

        /// <summary>
        /// Accepts the given statement as input for this type's pattern.
        /// See <see cref="SpecifyPattern"/>. Do not call this method if it is null.
        /// </summary>
        /// <param name="statement">The statement to pull tokens from.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual ITypeStructure AcceptPattern(Statement statement) => throw new NotImplementedException();
        /// <summary>
        /// Accepts the given literal as input for this type's pattern.
        /// See <see cref="CanAcceptLiteralForData(TokenLiteral)"/>.
        /// </summary>
        /// <param name="literal">The literal to interpret into a type structure.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual ITypeStructure AcceptLiteral(TokenLiteral literal) => throw new NotImplementedException();

        /// <summary>
        /// Deep-clone the given data object as per this Typedef instance.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract ITypeStructure CloneData(ITypeStructure data);

        /// <summary>
        /// Returns all internal scoreboard objectives that this type uses, with the given <see cref="ScoreboardValue"/>.
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
        internal abstract IEnumerable<string> ConvertTo(ScoreboardValue src, ScoreboardValue dst);
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
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        internal abstract IEnumerable<string> AssignLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement);

        /// <summary>
        /// Returns the commands needed to add a literal to this type.
        /// </summary>
        /// <param name="self">The scoreboard value that has this type.</param>
        /// <param name="literal">The literal to assign.</param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        internal abstract IEnumerable<string> AddLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement);

        /// <summary>
        /// Returns the commands needed to subtract a literal from this type.
        /// </summary>
        /// <param name="self">The scoreboard value that has this type.</param>
        /// <param name="literal">The literal to assign.</param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        internal abstract IEnumerable<string> SubtractLiteral(ScoreboardValue self, TokenLiteral literal,
            Statement callingStatement);

        /// <summary>
        /// Returns the commands needed to assign another value to self, given that they are both the same type and compatible.
        /// <code>
        ///     self = other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        internal abstract IEnumerable<string> _Assign(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement);

        /// <summary>
        /// Returns the range needed to compare this type alone, given that <see cref="CanCompareAlone"/> is <b>true</b>.
        /// </summary>
        /// <param name="invert">Whether to invert the range.</param>
        /// <param name="value">The value holding this type.</param>
        /// <returns></returns>
        internal abstract ConditionalSubcommandScore[] CompareAlone(bool invert, ScoreboardValue value);

        /// <summary>
        /// Compare a value with this type to a literal value. Returns both the setup commands needed, and the score comparisons needed.
        /// <br/>
        /// <b>Either field may be null, in which it will count as empty.</b>
        /// </summary>
        /// <param name="comparisonType">The comparison type.</param>
        /// <param name="self"></param>
        /// <param name="literal"></param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        internal abstract Tuple<string[], ConditionalSubcommandScore[]> CompareToLiteral(
            TokenCompare.Type comparisonType, ScoreboardValue self, TokenLiteral literal, Statement callingStatement);

        /// <summary>
        /// Returns the commands needed to add another value to self, given that they are both the same type and compatible.
        /// <code>
        ///     self += other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        internal abstract IEnumerable<string> _Add(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement);

        /// <summary>
        /// Returns the commands needed to subtract another value from self, given that they are both the same type and compatible.
        /// <code>
        ///     self -= other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        internal abstract IEnumerable<string> _Subtract(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement);

        /// <summary>
        /// Returns the commands needed to multiply self by another value, given that they are both the same type and compatible.
        /// <code>
        ///     self *= other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        internal abstract IEnumerable<string> _Multiply(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement);

        /// <summary>
        /// Returns the commands needed to divide self by another value, given that they are both the same type and compatible.
        /// <code>
        ///     self /= other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        internal abstract IEnumerable<string> _Divide(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement);

        /// <summary>
        /// Returns the commands needed to get the remainder of self after division with another value, given that they are both the same type and compatible.
        /// <code>
        ///     self %= other
        /// </code>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        internal abstract IEnumerable<string> _Modulo(ScoreboardValue self, ScoreboardValue other,
            Statement callingStatement);
    }
    /// <summary>
    /// A type definition.
    /// </summary>
    /// <typeparam name="T">The data structure held inside the ScoreboardValues that are of this type.</typeparam>
    public abstract class Typedef<T> : Typedef where T: ITypeStructure
    {
        public override ITypeStructure CloneData(ITypeStructure data)
        {
            // data should be T
            var convert = (T)data;

            // call deep clone implementation
            return (T)convert.DeepClone();
        }
    }
}
