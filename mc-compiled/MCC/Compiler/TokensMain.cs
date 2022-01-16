using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Represents a literal in code.
    /// </summary>
    public class TokenLiteral : Token
    {
        public override string AsString() => "<? literal>";
        public TokenLiteral(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// Represents a generic number literal.
    /// </summary>
    public class TokenNumberLiteral : TokenLiteral
    {
        public override string AsString() => "<? number literal>";
        public TokenNumberLiteral(int lineNumber) : base(lineNumber)
        {

        }
    }

    public sealed class TokenStringLiteral : TokenLiteral
    {
        public readonly string text;

        public override string AsString() => text;
        public TokenStringLiteral(string text, int lineNumber) : base(lineNumber)
        {
            this.text = text;
        }
    }
    public sealed class TokenIntegerLiteral : TokenNumberLiteral
    {
        public readonly int number;
        public override string AsString() => number.ToString();
        public TokenIntegerLiteral(int number, int lineNumber) : base(lineNumber)
        {
            this.number = number;
        }
    }
    public sealed class TokenDecimalLiteral : TokenNumberLiteral
    {
        public readonly float number;
        public override string AsString() => number.ToString();
        public TokenDecimalLiteral(float number, int lineNumber) : base(lineNumber)
        {
            this.number = number;
        }
    }
    public sealed class TokenBooleanLiteral : TokenLiteral
    {
        public readonly bool boolean;
        public override string AsString() => boolean.ToString();
        public TokenBooleanLiteral(bool boolean, int lineNumber) : base(lineNumber)
        {
            this.boolean = boolean;
        }
    }
}
