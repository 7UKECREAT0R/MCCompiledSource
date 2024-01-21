using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Windows.Forms;
using JetBrains.Annotations;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Compiler.TypeSystem.Implementations;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Represents a literal in code.
    /// </summary>
    public abstract class TokenLiteral : Token
    {
        protected const string DEFAULT_ERROR = "Invalid literal operation.";

        public abstract TokenLiteral Clone();
        public override string AsString() => "<? literal>";
        public abstract override string ToString();
        protected TokenLiteral(int lineNumber) : base(lineNumber) { }

        /// <summary>
        /// Return this literal's Scoreboard value type. Used when defining a variable with type inference.
        /// Returns null if the literal cannot be stored in a scoreboard objective.
        /// </summary>
        /// <returns>null if the literal cannot be stored in a scoreboard objective.</returns>
        public abstract Typedef GetTypedef();

        /// <summary>
        /// Creates a new <see cref="ScoreboardValue"/> to hold this literal.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="global"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public abstract ScoreboardValue CreateValue(string name, bool global,
            Statement tokens);

        /// <summary>
        /// Return a NEW token literal that is the result of adding these two literals in the order THIS + OTHER.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract TokenLiteral AddWithOther(TokenLiteral other);
        /// <summary>
        /// Return a NEW token literal that is the result of subtracting these two literal in the order THIS - OTHER.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract TokenLiteral SubWithOther(TokenLiteral other);
        /// <summary>
        /// Return a NEW token literal that is the result of multiplying these two literals in the order THIS * OTHER.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract TokenLiteral MulWithOther(TokenLiteral other);
        /// <summary>
        /// Return a NEW token literal that is the result of dividing these two literals in the order THIS / OTHER.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract TokenLiteral DivWithOther(TokenLiteral other);
        /// <summary>
        /// Return a NEW token literal that is the result of modulo-ing these two literals in the order THIS % OTHER.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract TokenLiteral ModWithOther(TokenLiteral other);

        /// <summary>
        /// Return a NEW token literal that is the result of comparing these two literals using a comparison operator in the order THIS, OTHER.
        /// </summary>
        /// <param name="cType">The type of comparison to perform.</param>
        /// <param name="other">The other literal.</param>
        /// <returns></returns>
        public abstract bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other);
    }

    /// <summary>
    /// Represents a value which has no determined value, but is defined as 0 under every type.
    /// </summary>
    public class TokenNullLiteral : TokenLiteral, IPreprocessor
    {
        public override string AsString() => "null";
        public override string ToString() => "null";
        public TokenNullLiteral(int lineNumber) : base(lineNumber) { }
        public override TokenLiteral Clone() => new TokenNullLiteral(lineNumber);
        public object GetValue()
        {
            return (int)0;
        }

        public override Typedef GetTypedef() => Typedef.INTEGER;
        public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
        {
            return new ScoreboardValue(name, global, Typedef.INTEGER, tokens.executor.scoreboard);
        }

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenDecimalLiteral decimalLiteral:
                    return new TokenDecimalLiteral(decimalLiteral.number, lineNumber);
                case TokenBooleanLiteral boolLiteral:
                    return new TokenBooleanLiteral(boolLiteral.boolean, lineNumber);
                case TokenIntegerLiteral intLiteral:
                    return new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier, lineNumber);
                default:
                    throw new TokenException(this, DEFAULT_ERROR);
            }
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenDecimalLiteral decimalLiteral:
                    return new TokenDecimalLiteral(decimalLiteral.number, lineNumber);
                case TokenBooleanLiteral boolLiteral:
                    return new TokenBooleanLiteral(boolLiteral.boolean, lineNumber);
                case TokenIntegerLiteral intLiteral:
                    return new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier, lineNumber);
                default:
                    throw new TokenException(this, DEFAULT_ERROR);
            }
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenDecimalLiteral decimalLiteral:
                    return new TokenDecimalLiteral(decimalLiteral.number, lineNumber);
                case TokenBooleanLiteral boolLiteral:
                    return new TokenBooleanLiteral(boolLiteral.boolean, lineNumber);
                case TokenIntegerLiteral intLiteral:
                    return new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier, lineNumber);
                default:
                    throw new TokenException(this, DEFAULT_ERROR);
            }
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenDecimalLiteral decimalLiteral:
                    return new TokenDecimalLiteral(decimalLiteral.number, lineNumber);
                case TokenBooleanLiteral boolLiteral:
                    return new TokenBooleanLiteral(boolLiteral.boolean, lineNumber);
                case TokenIntegerLiteral intLiteral:
                    return new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier, lineNumber);
                default:
                    throw new TokenException(this, DEFAULT_ERROR);
            }
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenDecimalLiteral decimalLiteral:
                    return new TokenDecimalLiteral(decimalLiteral.number, lineNumber);
                case TokenBooleanLiteral boolLiteral:
                    return new TokenBooleanLiteral(boolLiteral.boolean, lineNumber);
                case TokenIntegerLiteral intLiteral:
                    return new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier, lineNumber);
                default:
                    throw new TokenException(this, DEFAULT_ERROR);
            }
        }
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            if (other is TokenNumberLiteral numberLiteral)
            {
                float number = numberLiteral.GetNumber();
                switch (cType)
                {
                    case TokenCompare.Type.EQUAL:
                        return 0.0f == number;
                    case TokenCompare.Type.NOT_EQUAL:
                        return 0.0f != number;
                    case TokenCompare.Type.LESS:
                        return 0.0f < number;
                    case TokenCompare.Type.LESS_OR_EQUAL:
                        return 0.0f <= number;
                    case TokenCompare.Type.GREATER:
                        return 0.0f > number;
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                        return 0.0f >= number;
                    default:
                        break;
                }
            }

            throw new TokenException(this, DEFAULT_ERROR);
        }
    }
    /// <summary>
    /// A token which holds a completely constructed attribute. See <see cref="Attributes.IAttribute"/>.
    /// </summary>
    public sealed class TokenAttribute : TokenLiteral, IPreprocessor
    {
        public readonly IAttribute attribute;

        public override string AsString() => $"[Attribute: {attribute.GetDebugString()}]";

        public object GetValue()
        {
            return attribute;
        }

        public override TokenLiteral Clone()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public override Typedef GetTypedef() => null;
        public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
        {
            throw new StatementException(tokens, $"Cannot create a value to hold the literal '{AsString()}'");
        }

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            throw new TokenException(this, DEFAULT_ERROR);
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            throw new TokenException(this, DEFAULT_ERROR);
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            throw new TokenException(this, DEFAULT_ERROR);
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            throw new TokenException(this, DEFAULT_ERROR);
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            throw new TokenException(this, DEFAULT_ERROR);
        }
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            throw new TokenException(this, DEFAULT_ERROR);
        }
        public TokenAttribute(IAttribute attribute, int lineNumber) : base(lineNumber)
        {
            this.attribute = attribute;
        }
    }
    /// <summary>
    /// Represents a generic number literal.
    /// </summary>
    public abstract class TokenNumberLiteral : TokenLiteral, IPreprocessor
    {
        public TokenNumberLiteral(int lineNumber) : base(lineNumber) { }

        public int GetNumberInt()
        {
            return (int)GetNumber(); // floor
        }

        /// <summary>
        /// Get the number stored in this literal.
        /// </summary>
        /// <returns></returns>
        public abstract float GetNumber();
        public abstract object GetValue();

        public override Typedef GetTypedef() => Typedef.INTEGER;
    }
    public sealed class TokenStringLiteral : TokenLiteral, IPreprocessor, IImplicitToken, IIndexable, IDocumented
    {
        public readonly string text;

        public override string AsString() => '"' + text + '"';
        [UsedImplicitly]
        private TokenStringLiteral() : base(-1) { }
        public TokenStringLiteral(string text, int lineNumber) : base(lineNumber)
        {
            this.text = text;
        }
        public override TokenLiteral Clone() => new TokenStringLiteral(text, lineNumber);
        public override string ToString() => text;
        public object GetValue() =>  text;

        public static implicit operator string(TokenStringLiteral literal) => literal.text;

        public override Typedef GetTypedef() => null;
        public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
        {
            throw new StatementException(tokens, $"Cannot create a value to hold the literal '{AsString()}'");
        }

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            if (!(other is IPreprocessor preprocessor))
                throw new TokenException(this, "Invalid literal operation.");

            string append = preprocessor.GetValue().ToString();
            return new TokenStringLiteral(text + append, lineNumber);
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenStringLiteral literal1:
                {
                    string str = literal1;
                    if (text.EndsWith(str))
                        str = text.Substring(0, text.Length - str.Length);
                    return new TokenStringLiteral(str, lineNumber);
                }
                case TokenNumberLiteral literal2:
                {
                    int number = literal2.GetNumberInt();
                    string str = number > text.Length ? "" : text.Substring(0, text.Length - number);
                    return new TokenStringLiteral(str, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float number = literal.GetNumber();
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
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float number = literal.GetNumber();
            int length = (int)Math.Round(text.Length / number);

            return new TokenStringLiteral(text.Substring(0, length), lineNumber);

        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            throw new TokenException(this, "Invalid literal operation.");
        }
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            int a = text.Length;
            int b;

            switch (other)
            {
                case TokenNumberLiteral literal1:
                    b = literal1.GetNumberInt();
                    break;
                case TokenStringLiteral literal2:
                    b = literal2.text.Length;
                    break;
                default:
                    throw new NotImplementedException("TokenStringLiteral being compared to " + other.GetType().Name);
            }

            switch (cType)
            {
                case TokenCompare.Type.EQUAL:
                    return a == b;
                case TokenCompare.Type.NOT_EQUAL:
                    return a != b;
                case TokenCompare.Type.LESS:
                    return a < b;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    return a <= b;
                case TokenCompare.Type.GREATER:
                    return a > b;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    return a >= b;
                default:
                    throw new Exception("Unknown comparison type: " + cType);
            }
        }

        public Type[] GetImplicitTypes()
        {
            return new[] { typeof(TokenSelectorLiteral), typeof(TokenIdentifier) };
        }
        public Token Convert(Executor executor, int index)
        {
            switch (index)
            {
                case 0:
                {
                    // try parsing selector from string
                    int len = text.Length;

                    if (len == 0)
                        return new TokenSelectorLiteral(Selector.Core.s, lineNumber);

                    if(len > 1 && text[0] == '@')
                    {
                        char _core = text[1];
                        Selector.Core core = Selector.ParseCore(_core);

                        if (len > 2)
                        {
                            Selector parsed = Selector.Parse(core, text.Substring(2));
                            return new TokenSelectorLiteral(parsed, lineNumber);
                        }

                        var single = new Selector { core = core };
                        return new TokenSelectorLiteral(single, lineNumber);
                    }

                    string name = text;

                    // try finding by managed entity name
                    if (executor.entities.Search(name, out Selector find))
                        return new TokenSelectorLiteral(find, lineNumber);

                    // use name:type format
                    string type = null;
                    int colon = name.IndexOf(':');
                    if (colon != -1)
                    {
                        string old = name;
                        name = old.Substring(0, colon);
                        if (old.Length >= colon)
                            type = old.Substring(colon + 1);
                    }
                    if (string.IsNullOrEmpty(name))
                        name = null;
                    if (string.IsNullOrEmpty(type))
                        type = null;

                    return new TokenSelectorLiteral(new Selector()
                    {
                        core = Selector.Core.e,
                        entity = new Entity(name, type, new List<string>())
                    }, lineNumber);
                }
                case 1:
                    return new TokenIdentifier(text, lineNumber);
                default:
                    return null;
            }
        }

        public Token Index(TokenIndexer indexer, Statement forExceptions)
        {
            switch (indexer)
            {
                case TokenIndexerInteger integer:
                {
                    int value = integer.token.number;
                    int length = text.Length;
                    if (value >= length || value < 0)
                        throw integer.GetIndexOutOfBoundsException(0, length - 1, forExceptions);

                    string newString = text[value].ToString();
                    return new TokenStringLiteral(newString, lineNumber);
                }
                case TokenIndexerRange range:
                {
                    Range value = range.token.range;
                    int length = text.Length;
                
                    if (value.min.HasValue && value.min < 0)
                        throw range.GetIndexOutOfBoundsException(0, length - 1, forExceptions);
                    if (value.max.HasValue && value.max >= length)
                        throw range.GetIndexOutOfBoundsException(0, length - 1, forExceptions);

                    int start = value.min ?? 0;
                    int end = value.max ?? (length - 1);

                    string newString = text.Substring(start, end + 1);
                    return new TokenStringLiteral(newString, lineNumber);
                }
                default:
                    throw indexer.GetException(this, forExceptions);
            }
        }

        public string GetDocumentation() => "A block of text on a single line, surrounded with either 'single quotes' or \"double quotes.\"";
    }
    public sealed class TokenBooleanLiteral : TokenNumberLiteral, IPreprocessor, IDocumented
    {
        public readonly bool boolean;
        public override string AsString() => boolean.ToString();
        internal TokenBooleanLiteral() : base(-1) { }
        public TokenBooleanLiteral(bool boolean, int lineNumber) : base(lineNumber)
        {
            this.boolean = boolean;
        }
        public override TokenLiteral Clone() => new TokenBooleanLiteral(boolean, lineNumber);
        public override string ToString() => boolean.ToString();
        public override object GetValue() => boolean;
        public override float GetNumber()
        {
            return boolean ? 1f : 0f;
        }

        public static implicit operator bool(TokenBooleanLiteral literal) => literal.boolean;

        public override Typedef GetTypedef() => Typedef.BOOLEAN;
        public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
        {
            return new ScoreboardValue(name, global, Typedef.BOOLEAN, tokens.executor.scoreboard);
        }

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
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            throw new NotImplementedException("Cannot compare boolean to another type.");
        }

        public string GetDocumentation() => "A value that can be either 'true' or 'false.'";
    }

    public class TokenCoordinateLiteral : TokenNumberLiteral, IDocumented
    {
        private readonly Coord coordinate;
        
        public override string AsString() => coordinate.ToString();
        public override string ToString() => coordinate.ToString();
        internal TokenCoordinateLiteral() : base(-1) { }
        public TokenCoordinateLiteral(Coord coordinate, int lineNumber) : base(lineNumber)
        {
            this.coordinate = coordinate;
        }
        public override TokenLiteral Clone() => new TokenCoordinateLiteral(new Coord(coordinate), lineNumber);
        public override float GetNumber()
        {
            if (coordinate.isFloat)
                return coordinate.valuef;
            else
                return coordinate.valuei;
        }
        public override object GetValue() => coordinate;
        public override Typedef GetTypedef() => null;
        public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
        {
            throw new StatementException(tokens, $"Cannot create a value to hold the literal '{AsString()}'");
        }

        public static implicit operator Coord(TokenCoordinateLiteral literal) => literal.coordinate;
        public static implicit operator int(TokenCoordinateLiteral literal) => literal.coordinate.valuei;
        public static implicit operator float(TokenCoordinateLiteral literal) => literal.coordinate.valuef;

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            if(other is TokenNumberLiteral literal)
            {
                float number = literal.GetNumber();
                Coord coord = new Coord(coordinate);
                coord.valuef += number;
                coord.valuei = (int)Math.Round(coord.valuef);
                return new TokenCoordinateLiteral(coord, lineNumber);
            }

            throw new TokenException(this, "Invalid literal operation.");
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float number = literal.GetNumber();
            var coord = new Coord(coordinate);
            coord.valuef -= number;
            coord.valuei = (int)Math.Round(coord.valuef);
            return new TokenCoordinateLiteral(coord, lineNumber);

        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float number = literal.GetNumber();
            var coord = new Coord(coordinate);
            coord.valuef *= number;
            coord.valuei = (int)Math.Round(coord.valuef);
            return new TokenCoordinateLiteral(coord, lineNumber);

        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float number = literal.GetNumber();
            var coord = new Coord(coordinate);
            coord.valuef /= number;
            coord.valuei = (int)Math.Round(coord.valuef);
            return new TokenCoordinateLiteral(coord, lineNumber);

        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float number = literal.GetNumber();
            var coord = new Coord(coordinate);
            coord.valuef %= number;
            coord.valuei = (int)Math.Round(coord.valuef);
            return new TokenCoordinateLiteral(coord, lineNumber);

        }
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            float a = coordinate.valuef;
            float b;

            switch (other)
            {
                case TokenNumberLiteral literal:
                    b = literal.GetNumber();
                    break;
                case TokenStringLiteral literal:
                    b = literal.text.Length;
                    break;
                default:
                    throw new NotImplementedException("TokenCoordinateLiteral being compared to " + other.GetType().Name);
            }

            const float TOLERANCE = 0.0001f;
            switch (cType)
            {
                case TokenCompare.Type.EQUAL:
                    return Math.Abs(a - b) < TOLERANCE;
                case TokenCompare.Type.NOT_EQUAL:
                    return Math.Abs(a - b) > TOLERANCE;
                case TokenCompare.Type.LESS:
                    return a < b;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    return a <= b;
                case TokenCompare.Type.GREATER:
                    return a > b;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    return a >= b;
                default:
                    throw new Exception("Unknown comparison type: " + cType);
            }
        }

        public string GetDocumentation() => "A Minecraft coordinate value that can optionally be both relative and facing offset, like ~10, 40, or ^5.";
    }
    public enum IntMultiplier : int
    {
        none = 1,
        s = 20,
        m = 1200,
        h = 72000
    }
    public class TokenIntegerLiteral : TokenCoordinateLiteral, IDocumented
    {
        public static readonly IntMultiplier[] ALL_MULTIPLIERS = (IntMultiplier[])Enum.GetValues(typeof(IntMultiplier));

        /// <summary>
        /// The number that has already been multiplied.
        /// </summary>
        public readonly int number;
        /// <summary>
        /// The multiplier that was applied to this integer.
        /// </summary>
        public readonly IntMultiplier multiplier;
        
        /// <summary>
        /// Get the value of this number in a certain scale.
        /// If this number has no multiplier given <see cref="IntMultiplier.none"/>, then this method always returns the plain number.
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public int Scaled(IntMultiplier scale)
        {
            // no multiplier given, they probably read the documentation
            if (multiplier == IntMultiplier.none)
                return number;

            // scale matches multiplier
            if (scale == multiplier)
                return number;

            return number / (int)scale;
        }

        public override string AsString() => number.ToString();
        public TokenIntegerLiteral() : base() { }
        public TokenIntegerLiteral(int number, IntMultiplier multiplier, int lineNumber) :
            base(new Coord(number, false, false, false), lineNumber)
        {
            this.number = number;
            this.multiplier = multiplier;
        }
        public override TokenLiteral Clone() => new TokenIntegerLiteral(number, multiplier, lineNumber);
        public override string ToString() => number.ToString();
        public override object GetValue() => number;
        public override float GetNumber()
        {
            return number;
        }

        public override Typedef GetTypedef() => Typedef.INTEGER;
        public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
        {
            return new ScoreboardValue(name, global, Typedef.INTEGER, tokens.executor.scoreboard);
        }

        public static implicit operator int(TokenIntegerLiteral literal) => literal.number;

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenIntegerLiteral(number + i, IntMultiplier.none, lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    float value = literal.number;
                    value += number;
                    return new TokenDecimalLiteral(value, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenIntegerLiteral(number - i, IntMultiplier.none, lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    float value = literal.number;
                    value -= number;
                    return new TokenDecimalLiteral(value, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenIntegerLiteral(number * i, IntMultiplier.none, lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    float value = literal.number;
                    value *= number;
                    return new TokenDecimalLiteral(value, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenIntegerLiteral(number / i, IntMultiplier.none, lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    float value = literal.number;
                    value /= number;
                    return new TokenDecimalLiteral(value, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenIntegerLiteral(number % i, IntMultiplier.none, lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    float value = literal.number;
                    value %= number;
                    return new TokenDecimalLiteral(value, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }

        public new string GetDocumentation() => "Any integral number, like 5, 10, 5291, or -40. Use time suffixes to scale the integer accordingly, like with 4s -> 80.";
    }
    public sealed class TokenRangeLiteral : TokenLiteral, IPreprocessor, IIndexable, IDocumented
    {
        public Range range;
        public override string AsString() => range.ToString();
        public override string ToString() => range.ToString();
        internal TokenRangeLiteral() : base(-1) { }
        public TokenRangeLiteral(Range range, int lineNumber) : base(lineNumber)
        {
            this.range = range;
        }
        public override TokenLiteral Clone() => new TokenRangeLiteral(new Range(range), lineNumber);

        public override Typedef GetTypedef() => null;
        public override ScoreboardValue CreateValue(string name, bool global, Statement forExceptions)
        {
            throw new StatementException(forExceptions, $"Cannot create a value to hold the literal '{AsString()}'");
        }

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenRangeLiteral literal:
                {
                    Range r = literal.range;
                    return new TokenRangeLiteral(range + r, lineNumber);
                }
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenRangeLiteral(range + i, lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    float value = literal.number;
                    return new TokenRangeLiteral(range + value, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenRangeLiteral literal:
                {
                    Range r = literal.range;
                    return new TokenRangeLiteral(range - r, lineNumber);
                }
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenRangeLiteral(range - i, lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    float value = literal.number;
                    return new TokenRangeLiteral(range - value, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenRangeLiteral literal:
                {
                    Range r = literal.range;
                    return new TokenRangeLiteral(range * r, lineNumber);
                }
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenRangeLiteral(range * i, lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    float value = literal.number;
                    return new TokenRangeLiteral(range * value, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenRangeLiteral literal:
                {
                    Range r = literal.range;
                    return new TokenRangeLiteral(range / r, lineNumber);
                }
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenRangeLiteral(range / i, lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    float value = literal.number;
                    return new TokenRangeLiteral(range / value, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenRangeLiteral literal:
                {
                    Range r = literal.range;
                    return new TokenRangeLiteral(range % r, lineNumber);
                }
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenRangeLiteral(range % i, lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    float value = literal.number;
                    return new TokenRangeLiteral(range % value, lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            int thisMin = range.min ?? int.MinValue;
            int thisMax = range.max ?? int.MaxValue;

            int b;

            switch (other)
            {
                case TokenNumberLiteral literal:
                    b = literal.GetNumberInt();
                    break;
                case TokenStringLiteral literal:
                    b = literal.text.Length;
                    break;
                default:
                    throw new NotImplementedException("TokenRangeLiteral being compared to " + other.GetType().Name);
            }

            switch (cType)
            {
                case TokenCompare.Type.EQUAL:
                    return b <= thisMax && b >= thisMin;
                case TokenCompare.Type.NOT_EQUAL:
                    return b < thisMin || b > thisMax;
                case TokenCompare.Type.LESS:
                    return b < thisMin;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    return b <= thisMin;
                case TokenCompare.Type.GREATER:
                    return b > thisMax;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    return b >= thisMax;
                default:
                    throw new Exception("Unknown comparison type: " + cType);
            }
        }

        public object GetValue()
        {
            if (!range.single || range.invert)
                return range;
            
            if (range.min != null)
                return range.min.Value; // dereference to an integer

            return range;
        }
        public Token Index(TokenIndexer indexer, Statement forExceptions)
        {
            switch (indexer)
            {
                case TokenIndexerInteger integer:
                {
                    int value = integer.token.number;
                    if (value > 2 || value < 0)
                        throw integer.GetIndexOutOfBoundsException(0, 2, forExceptions);

                    // if its a single number both should return the same value
                    if(range.single)
                        return new TokenIntegerLiteral(range.min ?? 0, IntMultiplier.none, lineNumber);

                    switch (value)
                    {
                        // fetch min/max for 0/1
                        case 0:
                            return new TokenIntegerLiteral(range.min ?? 0, IntMultiplier.none, lineNumber);
                        case 1:
                            return new TokenIntegerLiteral(range.max ?? 0, IntMultiplier.none, lineNumber);
                        case 2:
                            return new TokenBooleanLiteral(range.invert, lineNumber);
                        default:
                            throw new Exception($"Invalid indexer for range: '{value}'");
                    }
                }
                case TokenIndexerString indexerString:
                {
                    string input = indexerString.token.text.ToUpper();
                    switch (input)
                    {
                        case "MIN":
                        case "MINIMUM":
                            return new TokenIntegerLiteral(range.min ?? 0, IntMultiplier.none, lineNumber);
                        case "MAX":
                        case "MAXIMUM":
                            return new TokenIntegerLiteral(range.max ?? 0, IntMultiplier.none, lineNumber);
                        case "INVERTED":
                        case "INVERT":
                            return new TokenBooleanLiteral(range.invert, lineNumber);
                        default:
                            throw new Exception($"Invalid indexer for range: '{input}'");
                    }
                }
                default:
                    throw indexer.GetException(this, forExceptions);
            }
        }

        public string GetDocumentation() => "A Minecraft number that specifies a range of integers (inclusive). Omitting a number from one side makes the number unbounded. 4.. means four and up. 1..5 means one through five.";
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
        public override TokenLiteral Clone() => new TokenDecimalLiteral(number, lineNumber);
        public override string ToString() => number.ToString();
        public override object GetValue() => number;
        public override float GetNumber()
        {
            return number;
        }

        public static implicit operator float(TokenDecimalLiteral literal) => literal.number;

        public override Typedef GetTypedef() => Typedef.FIXED_DECIMAL;
        public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
        {
            var data = new FixedDecimalData(number.GetPrecision());
            return new ScoreboardValue(name, global, Typedef.FIXED_DECIMAL, data, tokens.executor.scoreboard);
        }

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float f = literal.GetNumber();
            return new TokenDecimalLiteral(number + f, lineNumber);
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float f = literal.GetNumber();
            return new TokenDecimalLiteral(number - f, lineNumber);
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float f = literal.GetNumber();
            return new TokenDecimalLiteral(number * f, lineNumber);
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float f = literal.GetNumber();
            return new TokenDecimalLiteral(number / f, lineNumber);
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            float f = literal.GetNumber();
            return new TokenDecimalLiteral(number % f, lineNumber);
        }
    }

    /// <summary>
    /// A selector.
    /// </summary>
    public class TokenSelectorLiteral : TokenLiteral, IPreprocessor, IDocumented
    {
        public readonly bool simple;
        public readonly Selector selector;

        public override string AsString() => selector.ToString();
        internal TokenSelectorLiteral() : base(-1) { }
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
        public override TokenLiteral Clone() => new TokenSelectorLiteral(new Selector(selector), lineNumber);
        public override string ToString() => selector.ToString();

        public static implicit operator Selector(TokenSelectorLiteral t) => t.selector;
        public static implicit operator Selector.Core(TokenSelectorLiteral t) => t.selector.core;

        public override Typedef GetTypedef() => null;
        public override ScoreboardValue CreateValue(string name, bool global, Statement forExceptions)
        {
            throw new StatementException(forExceptions, $"Cannot create a value to hold the literal '{AsString()}'");
        }

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
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            throw new NotImplementedException("Cannot compare selector to another type.");
        }

        public object GetValue() => selector;

        public string GetDocumentation() => "A Minecraft selector that targets a specific entity or set of entities. Example: `@e[type=cow]`";
    }
    /// <summary>
    /// A literal holding a JSON value. Mostly used for indexing purposes.
    /// </summary>
    public class TokenJSONLiteral : TokenLiteral, IPreprocessor, IIndexable, IDocumented
    {
        public readonly JToken token;

        public bool IsObject
        {
            get => token.Type == JTokenType.Object;
        }
        public bool IsArray
        {
            get => token.Type == JTokenType.Array;
        }

        public override string AsString() => token.ToString();
        public override string ToString() => token.ToString();
        internal TokenJSONLiteral() : base(-1) { }
        public TokenJSONLiteral(JToken token, int lineNumber) : base(lineNumber)
        {
            this.token = token;
        }
        public override TokenLiteral Clone() => new TokenJSONLiteral(token, lineNumber);
        public static implicit operator JToken(TokenJSONLiteral t) => t.token;

        public override Typedef GetTypedef() => null;
        public override ScoreboardValue CreateValue(string name, bool global, Statement forExceptions)
        {
            throw new StatementException(forExceptions, $"Cannot create a value to hold the literal '{AsString()}'");
        }
        
        public object GetValue() => token;
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
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            throw new NotImplementedException();
        }

        public Token Index(TokenIndexer indexer, Statement forExceptions)
        {
            switch (indexer)
            {
                case TokenIndexerInteger integer when !(token is JArray):
                    throw new StatementException(forExceptions, "JSON type cannot be indexed using a number: " + token.Type);
                case TokenIndexerInteger integer:
                {
                    JArray array = (JArray)token;
                    int value = integer.token.number;
                    int len = array.Count;

                    if (value >= len || value < 0)
                        throw integer.GetIndexOutOfBoundsException(0, len - 1, forExceptions);

                    JToken indexedItem = array[value];
                    if(PreprocessorUtils.TryGetLiteral(indexedItem, lineNumber, out TokenLiteral output))
                        return output;
                    else
                        throw new StatementException(forExceptions, "Couldn't load JSON value: " + indexedItem.ToString());
                }
                case TokenIndexerString @string when !(token is JObject):
                    throw new StatementException(forExceptions, "JSON type cannot be indexed using a string: " + token.Type);
                case TokenIndexerString @string:
                {
                    JObject json = (JObject)token;
                    string word = @string.token.text;

                    JToken gottenToken = json[word];

                    if (gottenToken == null)
                        throw new StatementException(forExceptions, $"No JSON property found with the name '{word}'");

                    if (PreprocessorUtils.TryGetLiteral(gottenToken, lineNumber, out TokenLiteral output))
                        return output;
                    else
                        throw new StatementException(forExceptions, "Couldn't load JSON value: " + gottenToken.ToString());
                }
                default:
                    throw indexer.GetException(this, forExceptions);
            }
        }

        public string GetDocumentation() => "A JSON object achieved by $dereferencing a preprocessor variable holding one.";
    }
}