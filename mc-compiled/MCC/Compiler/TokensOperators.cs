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
    public abstract class TokenArithmatic : TokenOperator
    {
        public enum Type
        {
            ADD, SUBTRACT, MULTIPLY, DIVIDE, MODULO, SWAP
        }

        public override string AsString() => "<? arithmatic>";
        public TokenArithmatic(int lineNumber) : base(lineNumber) { }

        public abstract Type GetArithmaticType();
    }
    public abstract class TokenArithmaticFirst : TokenArithmatic
    {
        public override string AsString() => "<? */%>";
        public TokenArithmaticFirst(int lineNumber) : base(lineNumber) { }

    }
    public abstract class TokenArithmaticSecond : TokenArithmatic
    {
        public override string AsString() => "<? +->";
        public TokenArithmaticSecond(int lineNumber) : base(lineNumber) { }
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
    /// Used to indicate that this operator assigns the identifier to the left of it.
    /// </summary>
    public interface IAssignment { }
    /// <summary>
    /// Used to indicate when a token should terminate the Assembler's token collector and start a new line.
    /// </summary>
    public interface ITerminating { }

    public sealed class TokenOpenParenthesis : TokenOpenBracket
    {
        public bool hasBeenSquashed = false; // used to prevent function squashing from recursing infinitely
        public override string AsString() => "(";
        public TokenOpenParenthesis(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenCloseParenthesis : TokenOpenBracket
    {
        public override string AsString() => ")";
        public TokenCloseParenthesis(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenOpenBlock : TokenOpenBracket, ITerminating
    {
        public override string AsString() => "{";
        public TokenOpenBlock(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenCloseBlock : TokenCloseBracket, ITerminating
    {
        public override string AsString() => "}";
        public TokenCloseBlock(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// The two dots in a range argument. 123..456
    /// </summary>
    public sealed class TokenRangeDots : Token
    {
        public override string AsString() => "..";
        public TokenRangeDots(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenRangeInvert : Token
    {
        public override string AsString() => "!";
        public TokenRangeInvert(int lineNumber) : base(lineNumber) { }
    }

    public sealed class TokenAnd : TokenOperator
    {
        public override string AsString() => "&";
        public TokenAnd(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenAssignment : TokenOperator, IAssignment
    {
        public override string AsString() => "=";
        public TokenAssignment(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenAdd : TokenArithmaticSecond
    {
        public override string AsString() => "+";
        public TokenAdd(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.ADD;
    }
    public sealed class TokenSubtract : TokenArithmaticSecond
    {
        public override string AsString() => "-";
        public TokenSubtract(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.SUBTRACT;
    }
    public sealed class TokenMultiply : TokenArithmaticFirst
    {
        public override string AsString() => "*";
        public TokenMultiply(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.MULTIPLY;
    }
    public sealed class TokenDivide : TokenArithmaticFirst
    {
        public override string AsString() => "/";
        public TokenDivide(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.DIVIDE;
    }
    public sealed class TokenModulo : TokenArithmaticFirst
    {
        public override string AsString() => "%";
        public TokenModulo(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.MODULO;
    }
    public sealed class TokenAddAssignment : TokenArithmaticSecond, IAssignment
    {
        public override string AsString() => "+=";
        public TokenAddAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.ADD;
    }
    public sealed class TokenSubtractAssignment : TokenArithmaticSecond, IAssignment
    {
        public override string AsString() => "-=";
        public TokenSubtractAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.SUBTRACT;
    }
    public sealed class TokenMultiplyAssignment : TokenArithmaticFirst, IAssignment
    {
        public override string AsString() => "*=";
        public TokenMultiplyAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.MULTIPLY;
    }
    public sealed class TokenDivideAssignment : TokenArithmaticFirst, IAssignment
    {
        public override string AsString() => "/=";
        public TokenDivideAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.DIVIDE;
    }
    public sealed class TokenModuloAssignment : TokenArithmaticFirst, IAssignment
    {
        public override string AsString() => "%=";
        public TokenModuloAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.MODULO;
    }
    public sealed class TokenSwapAssignment : TokenArithmaticFirst, IAssignment
    {
        public override string AsString() => "><";
        public TokenSwapAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.SWAP;
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