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
    public class TokenCompareOperator : TokenOperator
    {
        public override string AsString() => "<? compare>";
        public TokenCompareOperator(int lineNumber) : base(lineNumber) { }
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
    
    public sealed class TokenEqualityOperator : TokenCompareOperator
    {
        public override string AsString() => "==";
        public TokenEqualityOperator(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenInequalityOperator : TokenCompareOperator
    {
        public override string AsString() => "!=";
        public TokenInequalityOperator(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenLessThanOperator : TokenCompareOperator
    {
        public override string AsString() => "<";
        public TokenLessThanOperator(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenGreaterThanOperator : TokenCompareOperator
    {
        public override string AsString() => ">";
        public TokenGreaterThanOperator(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenLessThanEqualOperator : TokenCompareOperator
    {
        public override string AsString() => "<=";
        public TokenLessThanEqualOperator(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenGreaterThanEqualOperator : TokenCompareOperator
    {
        public override string AsString() => ">=";
        public TokenGreaterThanEqualOperator(int lineNumber) : base(lineNumber) { }
    }
}