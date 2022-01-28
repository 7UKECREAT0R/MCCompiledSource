using mc_compiled.Commands;
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
    public abstract class TokenNumberLiteral : TokenLiteral, IObjectable
    {
        public TokenNumberLiteral(int lineNumber) : base(lineNumber) { }

        public int GetNumberInt()
        {
            return (int)Math.Round(GetNumber());
        }

        /// <summary>
        /// Get the number stored in this literal.
        /// </summary>
        /// <returns></returns>
        public abstract float GetNumber();

        public abstract object GetObject();
    }
    public sealed class TokenStringLiteral : TokenLiteral, IObjectable
    {
        public readonly string text;

        public override string AsString() => '"' + text + '"';
        public TokenStringLiteral(string text, int lineNumber) : base(lineNumber)
        {
            this.text = text;
        }
        public object GetObject() =>  text;

        public static implicit operator string(TokenStringLiteral literal) => literal.text;
    }
    public sealed class TokenBooleanLiteral : TokenLiteral, IObjectable
    {
        public readonly bool boolean;
        public override string AsString() => boolean.ToString();
        public TokenBooleanLiteral(bool boolean, int lineNumber) : base(lineNumber)
        {
            this.boolean = boolean;
        }
        public object GetObject() => boolean;

        public static implicit operator bool(TokenBooleanLiteral literal) => literal.boolean;
    }

    public class TokenCoordinateLiteral : TokenNumberLiteral
    {
        public readonly Coord coordinate;
        public override string AsString() => coordinate.ToString();
        public TokenCoordinateLiteral(Coord coordinate, int lineNumber) : base(lineNumber)
        {
            this.coordinate = coordinate;
        }
        public override float GetNumber()
        {
            if (coordinate.isFloat)
                return coordinate.valuef;
            else
                return coordinate.valuei;
        }
        public override object GetObject() => coordinate;

        public static implicit operator Coord(TokenCoordinateLiteral literal) => literal.coordinate;
        public static implicit operator int(TokenCoordinateLiteral literal) => literal.coordinate.valuei;
        public static implicit operator float(TokenCoordinateLiteral literal) => literal;
    }
    public class TokenIntegerLiteral : TokenCoordinateLiteral
    {
        public readonly int number;
        public override string AsString() => number.ToString();
        public TokenIntegerLiteral(int number, int lineNumber) :
            base(new Coord(number, false, false, false), lineNumber)
        {
            this.number = number;
        }
        public override object GetObject() => number;
        public override float GetNumber()
        {
            return number;
        }

        public static implicit operator int(TokenIntegerLiteral literal) => literal.number;
    }
    public sealed class TokenDecimalLiteral : TokenCoordinateLiteral
    {
        public readonly float number;
        public override string AsString() => number.ToString();
        public TokenDecimalLiteral(float number, int lineNumber) :
            base(new Coord(number, true, false, false), lineNumber)
        {
            this.number = number;
        }
        public override object GetObject() => number;
        public override float GetNumber()
        {
            return number;
        }

        public static implicit operator float(TokenDecimalLiteral literal) => literal.number;
    }

    public class TokenSimpleSelectorLiteral : TokenLiteral
    {
        public readonly Selector.Core core;
        public TokenSimpleSelectorLiteral(Selector.Core core, int lineNumber) : base(lineNumber)
        {
            this.core = core;
        }
    }
    public sealed class TokenSelectorLiteral : TokenSimpleSelectorLiteral
    {
        public readonly Selector selector;

        public TokenSelectorLiteral(Selector selector, int lineNumber) : base(selector.core, lineNumber)
        {
            this.selector = selector;
        }
    }
}