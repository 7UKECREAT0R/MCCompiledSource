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
    public abstract class TokenLiteral : Token
    {
        public override string AsString() => "<? literal>";
        public abstract override string ToString();
        public TokenLiteral(int lineNumber) : base(lineNumber) { }

        /// <summary>
        /// Return a NEW token literal that is the result of adding these two literals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract TokenLiteral AddWithOther(TokenLiteral other);
        /// <summary>
        /// Return a NEW token literal that is the result of subtracting these two literals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract TokenLiteral SubWithOther(TokenLiteral other);
        /// <summary>
        /// Return a NEW token literal that is the result of multiplying these two literals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract TokenLiteral MulWithOther(TokenLiteral other);
        /// <summary>
        /// Return a NEW token literal that is the result of dividing these two literals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract TokenLiteral DivWithOther(TokenLiteral other);
        /// <summary>
        /// Return a NEW token literal that is the result of modulo'ing these two literals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract TokenLiteral ModWithOther(TokenLiteral other);
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
    public sealed class TokenStringLiteral : TokenLiteral, IObjectable, IImplicitToken
    {
        public readonly string text;

        public override string AsString() => '"' + text + '"';
        public TokenStringLiteral(string text, int lineNumber) : base(lineNumber)
        {
            this.text = text;
        }
        public override string ToString() => text;
        public object GetObject() =>  text;

        public static implicit operator string(TokenStringLiteral literal) => literal.text;

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            if (!(other is IObjectable))
                throw new TokenException(this, "Invalid literal operation.");

            string append = (other as IObjectable).GetObject().ToString();
            return new TokenStringLiteral(text + append, lineNumber);
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            if(other is TokenStringLiteral)
            {
                string str = other as TokenStringLiteral;
                if (text.EndsWith(str))
                    str = text.Substring(0, text.Length - str.Length);
                return new TokenStringLiteral(str, lineNumber);
            } else if(other is TokenNumberLiteral)
            {
                int number = (other as TokenNumberLiteral).GetNumberInt();
                string str;

                if (number > text.Length)
                    str = "";
                else
                    str = text.Substring(0, text.Length - number);

                return new TokenStringLiteral(str, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            if (other is TokenNumberLiteral)
            {
                float number = (other as TokenNumberLiteral).GetNumber();
                int length = (int)Math.Round(text.Length * number);

                int sample = 0;
                char[] characters = new char[length];
                for(int i = 0; i < length; i++)
                {
                    characters[i] = text[sample++];
                    if (sample >= text.Length)
                        sample = 0;
                }

                return new TokenStringLiteral(new string(characters), lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            if (other is TokenNumberLiteral)
            {
                float number = (other as TokenNumberLiteral).GetNumber();
                int length = (int)Math.Round(text.Length / number);

                return new TokenStringLiteral(text.Substring(0, length), lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            throw new TokenException(this, "Invalid literal operation.");
        }

        public Type[] GetImplicitTypes()
        {
            return new[] { typeof(TokenSelectorLiteral) };
        }
        public Token Convert(Executor executor, int index)
        {
            if(index == 0)
            {
                string name = text;
                if (executor.entities.Search(name, out Selector find))
                    return new TokenSelectorLiteral(find, lineNumber);

                string type = null;
                if (name.Contains(':'))
                {
                    string[] strs = name.Split(':');
                    name = strs[0].Trim();
                    if (strs.Length > 1)
                        type = strs[1].Trim();
                }
                if (string.IsNullOrEmpty(name))
                    name = null;
                if (string.IsNullOrEmpty(type))
                    type = null;

                return new TokenSelectorLiteral(new Selector()
                {
                    core = Selector.Core.e,
                    entity = new Commands.Selectors.Entity(name, false, type, null)
                }, lineNumber);
            }

            return null;
        }
    }
    public sealed class TokenBooleanLiteral : TokenNumberLiteral, IObjectable
    {
        public readonly bool boolean;
        public override string AsString() => boolean.ToString();
        public TokenBooleanLiteral(bool boolean, int lineNumber) : base(lineNumber)
        {
            this.boolean = boolean;
        }
        public override string ToString() => boolean.ToString();
        public override object GetObject() => boolean;
        public override float GetNumber()
        {
            return boolean ? 1 : 0;
        }

        public static implicit operator bool(TokenBooleanLiteral literal) => literal.boolean;

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            throw new TokenException(this, "Invalid literal operation.");
        }
    }

    public class TokenCoordinateLiteral : TokenNumberLiteral
    {
        public readonly Coord coordinate;
        public override string AsString() => coordinate.ToString();
        public override string ToString() => coordinate.ToString();
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

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            if(other is TokenNumberLiteral)
            {
                float number = (other as TokenNumberLiteral).GetNumber();
                Coord coord = new Coord(coordinate);
                coord.valuef += number;
                coord.valuei = (int)Math.Round(coord.valuef);
                return new TokenCoordinateLiteral(coord, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            if (other is TokenNumberLiteral)
            {
                float number = (other as TokenNumberLiteral).GetNumber();
                Coord coord = new Coord(coordinate);
                coord.valuef -= number;
                coord.valuei = (int)Math.Round(coord.valuef);
                return new TokenCoordinateLiteral(coord, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            if (other is TokenNumberLiteral)
            {
                float number = (other as TokenNumberLiteral).GetNumber();
                Coord coord = new Coord(coordinate);
                coord.valuef *= number;
                coord.valuei = (int)Math.Round(coord.valuef);
                return new TokenCoordinateLiteral(coord, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            if (other is TokenNumberLiteral)
            {
                float number = (other as TokenNumberLiteral).GetNumber();
                Coord coord = new Coord(coordinate);
                coord.valuef /= number;
                coord.valuei = (int)Math.Round(coord.valuef);
                return new TokenCoordinateLiteral(coord, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            if (other is TokenNumberLiteral)
            {
                float number = (other as TokenNumberLiteral).GetNumber();
                Coord coord = new Coord(coordinate);
                coord.valuef %= number;
                coord.valuei = (int)Math.Round(coord.valuef);
                return new TokenCoordinateLiteral(coord, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
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
        public override string ToString() => number.ToString();
        public override object GetObject() => number;
        public override float GetNumber()
        {
            return number;
        }

        public static implicit operator int(TokenIntegerLiteral literal) => literal.number;

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            if(other is TokenIntegerLiteral)
            {
                int i = (other as TokenIntegerLiteral).number;
                return new TokenIntegerLiteral(number + i, lineNumber);
            } else if(other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral).number;
                value += number;
                return new TokenDecimalLiteral(value, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            if (other is TokenIntegerLiteral)
            {
                int i = (other as TokenIntegerLiteral).number;
                return new TokenIntegerLiteral(number - i, lineNumber);
            }
            else if (other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral).number;
                value -= number;
                return new TokenDecimalLiteral(value, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            if (other is TokenIntegerLiteral)
            {
                int i = (other as TokenIntegerLiteral).number;
                return new TokenIntegerLiteral(number * i, lineNumber);
            }
            else if (other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral).number;
                value *= number;
                return new TokenDecimalLiteral(value, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            if (other is TokenIntegerLiteral)
            {
                int i = (other as TokenIntegerLiteral).number;
                return new TokenIntegerLiteral(number / i, lineNumber);
            }
            else if (other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral).number;
                value /= number;
                return new TokenDecimalLiteral(value, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            if (other is TokenIntegerLiteral)
            {
                int i = (other as TokenIntegerLiteral).number;
                return new TokenIntegerLiteral(number % i, lineNumber);
            }
            else if (other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral).number;
                value %= number;
                return new TokenDecimalLiteral(value, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
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
        public override string ToString() => number.ToString();
        public override object GetObject() => number;
        public override float GetNumber()
        {
            return number;
        }

        public static implicit operator float(TokenDecimalLiteral literal) => literal.number;

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            if(other is TokenNumberLiteral)
            {
                float f = (other as TokenNumberLiteral).GetNumber();
                return new TokenDecimalLiteral(number + f, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            if (other is TokenNumberLiteral)
            {
                float f = (other as TokenNumberLiteral).GetNumber();
                return new TokenDecimalLiteral(number - f, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            if (other is TokenNumberLiteral)
            {
                float f = (other as TokenNumberLiteral).GetNumber();
                return new TokenDecimalLiteral(number * f, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            if (other is TokenNumberLiteral)
            {
                float f = (other as TokenNumberLiteral).GetNumber();
                return new TokenDecimalLiteral(number / f, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            if (other is TokenNumberLiteral)
            {
                float f = (other as TokenNumberLiteral).GetNumber();
                return new TokenDecimalLiteral(number % f, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
    }

    public sealed class TokenSelectorLiteral : TokenLiteral, IObjectable
    {
        public readonly bool simple;
        public readonly Selector selector;

        public override string AsString() => selector.ToString();
        public TokenSelectorLiteral(Selector selector, int lineNumber) : base(lineNumber)
        {
            simple = false;
            this.selector = selector;
        }
        public TokenSelectorLiteral(Selector.Core core, int lineNumber) : base(lineNumber)
        {
            simple = true;
            this.selector = new Selector()
            {
                core = core
            };
        }
        public override string ToString() => selector.ToString();

        public static implicit operator Selector(TokenSelectorLiteral t) => t.selector;
        public static implicit operator Selector.Core(TokenSelectorLiteral t) => t.selector.core;

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            throw new NotImplementedException();
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            throw new NotImplementedException();
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            throw new NotImplementedException();
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            throw new NotImplementedException();
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            throw new NotImplementedException();
        }

        public object GetObject() => selector;
    }
}