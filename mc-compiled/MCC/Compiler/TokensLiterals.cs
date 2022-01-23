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
        public abstract double GetNumber();

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
    public sealed class TokenDecimalLiteral : TokenNumberLiteral
    {
        public readonly double number;
        public override string AsString() => number.ToString();
        public TokenDecimalLiteral(float number, int lineNumber) : base(lineNumber)
        {
            this.number = number;
        }
        public override object GetObject() => number;
        public override double GetNumber()
        {
            return number;
        }

        public static implicit operator double(TokenDecimalLiteral literal) => literal.number;
    }
    public class TokenIntegerLiteral : TokenNumberLiteral
    {
        public readonly int number;
        public override string AsString() => number.ToString();
        public TokenIntegerLiteral(int number, int lineNumber) : base(lineNumber)
        {
            this.number = number;
        }
        public override object GetObject() => number;
        public override double GetNumber()
        {
            return number;
        }

        public static implicit operator int(TokenIntegerLiteral literal) => literal.number;
    }
    public sealed class TokenCoordinateLiteral : TokenNumberLiteral
    {
        public readonly Coord coordinate;
        public override string AsString() => coordinate.ToString();
        public TokenCoordinateLiteral(Coord coordinate, int lineNumber) : base(lineNumber)
        {
            this.coordinate = coordinate;
        }
        public override double GetNumber()
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
}