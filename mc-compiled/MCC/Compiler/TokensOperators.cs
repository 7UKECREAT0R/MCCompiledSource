using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A token that represents some kind of operator like =, +=, +, %, etc...
    /// </summary>
    public class TokenOperator : Token
    {
        public override string AsString() => "<? generic>";
        public TokenOperator(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// Represents a generic bracket, not open or closed.
    /// </summary>
    public class TokenBracket : TokenOperator
    {
        public override string AsString() => "<? bracket>";
        public TokenBracket(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// Represents an opening bracket, extends TokenBracket.
    /// </summary>
    public class TokenOpenBracket : TokenBracket
    {
        public override string AsString() => "<? bracket open>";
        public TokenOpenBracket(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// Represents a closing bracket, extends TokenBracket.
    /// </summary>
    public class TokenCloseBracket : TokenBracket
    {
        public override string AsString() => "<? bracket close>";
        public TokenCloseBracket(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// Represents an arithmatic operator token.
    /// </summary>
    public class TokenArithmatic : Token
    {
        public override string AsString() => "<? arithmatic>";
        public TokenArithmatic(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// Represents a generic comparison operator.
    /// </summary>
    public abstract class TokenCompare : TokenOperator
    {
        public enum Type
        {
            EQUAL,
            NOT_EQUAL,
            LESS_THAN,
            LESS_OR_EQUAL,
            GREATER_THAN,
            GREATER_OR_EQUAL
        }

        public override string AsString() => "<? compare>";
        public TokenCompare(int lineNumber) : base(lineNumber) { }

        public abstract Type GetCompareType();
    }

    /// <summary>
    /// Used to indicate a math operator is also compounded with an assignment e.g. += -= *= or /=
    /// </summary>
    public interface CompoundAssignment { }


    public sealed class TokenOpenParenthesis : TokenOpenBracket
    {
        public override string AsString() => "(";
        public TokenOpenParenthesis(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenCloseParenthesis : TokenOpenBracket
    {
        public override string AsString() => ")";
        public TokenCloseParenthesis(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenOpenBlock : TokenOpenBracket
    {
        public override string AsString() => "{";
        public TokenOpenBlock(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenCloseBlock : TokenOpenBracket
    {
        public override string AsString() => "}";
        public TokenCloseBlock(int lineNumber) : base(lineNumber) { }
    }

    public sealed class TokenAssignment : TokenOperator
    {
        public override string AsString() => "=";
        public TokenAssignment(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenAdd : TokenArithmatic
    {
        public override string AsString() => "+";
        public TokenAdd(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenSubtract : TokenArithmatic
    {
        public override string AsString() => "-";
        public TokenSubtract(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenMultiply : TokenArithmatic
    {
        public override string AsString() => "*";
        public TokenMultiply(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenDivide : TokenArithmatic
    {
        public override string AsString() => "/";
        public TokenDivide(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenModulo : TokenArithmatic
    {
        public override string AsString() => "%";
        public TokenModulo(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenAddAssignment : TokenArithmatic, CompoundAssignment
    {
        public override string AsString() => "+=";
        public TokenAddAssignment(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenSubtractAssignment : TokenArithmatic, CompoundAssignment
    {
        public override string AsString() => "-=";
        public TokenSubtractAssignment(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenMultiplyAssignment : TokenArithmatic, CompoundAssignment
    {
        public override string AsString() => "*=";
        public TokenMultiplyAssignment(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenDivideAssignment : TokenArithmatic, CompoundAssignment
    {
        public override string AsString() => "/=";
        public TokenDivideAssignment(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenModuloAssignment : TokenArithmatic, CompoundAssignment
    {
        public override string AsString() => "%=";
        public TokenModuloAssignment(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenSwapAssignment : TokenArithmatic, CompoundAssignment
    {
        public override string AsString() => "><";
        public TokenSwapAssignment(int lineNumber) : base(lineNumber) { }
    }

    public sealed class TokenEquality : TokenCompare
    {
        public override string AsString() => "==";
        public TokenEquality(int lineNumber) : base(lineNumber) { }

        public override Type GetCompareType() => Type.EQUAL;
    }
    public sealed class TokenInequality : TokenCompare
    {
        public override string AsString() => "!=";
        public TokenInequality(int lineNumber) : base(lineNumber) { }

        public override Type GetCompareType() => Type.NOT_EQUAL;
    }
    public sealed class TokenLessThan : TokenCompare
    {
        public override string AsString() => "<";
        public TokenLessThan(int lineNumber) : base(lineNumber) { }

        public override Type GetCompareType() => Type.LESS_THAN;
    }
    public sealed class TokenGreaterThan : TokenCompare
    {
        public override string AsString() => ">";
        public TokenGreaterThan(int lineNumber) : base(lineNumber) { }

        public override Type GetCompareType() => Type.GREATER_THAN;
    }
    public sealed class TokenLessThanEqual : TokenCompare
    {
        public override string AsString() => "<=";
        public TokenLessThanEqual(int lineNumber) : base(lineNumber) { }

        public override Type GetCompareType() => Type.LESS_OR_EQUAL;
    }
    public sealed class TokenGreaterThanEqual : TokenCompare
    {
        public override string AsString() => ">=";
        public TokenGreaterThanEqual(int lineNumber) : base(lineNumber) { }

        public override Type GetCompareType() => Type.GREATER_OR_EQUAL;
    }
}