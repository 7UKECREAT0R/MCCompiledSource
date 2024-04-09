using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using mc_compiled.Commands.Native;
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
        public override TokenLiteral Clone() => new TokenNullLiteral(this.lineNumber);
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
                    return new TokenDecimalLiteral(decimalLiteral.number, this.lineNumber);
                case TokenBooleanLiteral boolLiteral:
                    return new TokenBooleanLiteral(boolLiteral.boolean, this.lineNumber);
                case TokenIntegerLiteral intLiteral:
                    return new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier, this.lineNumber);
                default:
                    throw new TokenException(this, DEFAULT_ERROR);
            }
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenDecimalLiteral decimalLiteral:
                    return new TokenDecimalLiteral(decimalLiteral.number, this.lineNumber);
                case TokenBooleanLiteral boolLiteral:
                    return new TokenBooleanLiteral(boolLiteral.boolean, this.lineNumber);
                case TokenIntegerLiteral intLiteral:
                    return new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier, this.lineNumber);
                default:
                    throw new TokenException(this, DEFAULT_ERROR);
            }
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenDecimalLiteral decimalLiteral:
                    return new TokenDecimalLiteral(decimalLiteral.number, this.lineNumber);
                case TokenBooleanLiteral boolLiteral:
                    return new TokenBooleanLiteral(boolLiteral.boolean, this.lineNumber);
                case TokenIntegerLiteral intLiteral:
                    return new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier, this.lineNumber);
                default:
                    throw new TokenException(this, DEFAULT_ERROR);
            }
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenDecimalLiteral decimalLiteral:
                    return new TokenDecimalLiteral(decimalLiteral.number, this.lineNumber);
                case TokenBooleanLiteral boolLiteral:
                    return new TokenBooleanLiteral(boolLiteral.boolean, this.lineNumber);
                case TokenIntegerLiteral intLiteral:
                    return new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier, this.lineNumber);
                default:
                    throw new TokenException(this, DEFAULT_ERROR);
            }
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenDecimalLiteral decimalLiteral:
                    return new TokenDecimalLiteral(decimalLiteral.number, this.lineNumber);
                case TokenBooleanLiteral boolLiteral:
                    return new TokenBooleanLiteral(boolLiteral.boolean, this.lineNumber);
                case TokenIntegerLiteral intLiteral:
                    return new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier, this.lineNumber);
                default:
                    throw new TokenException(this, DEFAULT_ERROR);
            }
        }
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral numberLiteral))
                throw new TokenException(this, DEFAULT_ERROR);
            
            decimal number = numberLiteral.GetNumber();
            switch (cType)
            {
                case TokenCompare.Type.EQUAL:
                    return decimal.Zero == number;
                case TokenCompare.Type.NOT_EQUAL:
                    return decimal.Zero != number;
                case TokenCompare.Type.LESS:
                    return decimal.Zero < number;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    return decimal.Zero <= number;
                case TokenCompare.Type.GREATER:
                    return decimal.Zero > number;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    return decimal.Zero >= number;
                default:
                    break;
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

        public override string AsString() => $"[Attribute: {this.attribute.GetDebugString()}]";

        public object GetValue()
        {
            return this.attribute;
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
        public abstract decimal GetNumber();
        public abstract object GetValue();

        public override Typedef GetTypedef() => Typedef.INTEGER;
    }
    public sealed class TokenStringLiteral : TokenLiteral, IPreprocessor, IImplicitToken, IIndexable, IDocumented
    {
        public readonly string text;

        public override string AsString() => '"' + this.text + '"';
        [UsedImplicitly]
        private TokenStringLiteral() : base(-1) { }
        public TokenStringLiteral(string text, int lineNumber) : base(lineNumber)
        {
            this.text = text;
        }
        public override TokenLiteral Clone() => new TokenStringLiteral(this.text, this.lineNumber);
        public override string ToString() => this.text;
        public object GetValue() => this.text;

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
            return new TokenStringLiteral(this.text + append, this.lineNumber);
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            switch (other)
            {
                case TokenStringLiteral literal1:
                {
                    string str = literal1;
                    if (this.text.EndsWith(str))
                        str = this.text.Substring(0, this.text.Length - str.Length);
                    return new TokenStringLiteral(str, this.lineNumber);
                }
                case TokenNumberLiteral literal2:
                {
                    int number = literal2.GetNumberInt();
                    string str = number > this.text.Length ? "" : this.text.Substring(0, this.text.Length - number);
                    return new TokenStringLiteral(str, this.lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal number = literal.GetNumber();
            int length = (int)decimal.Round(this.text.Length * number);

            int sample = 0;
            char[] characters = new char[length];
            for(int i = 0; i < length; i++)
            {
                characters[i] = this.text[sample++];
                if (sample >= this.text.Length)
                    sample = 0;
            }

            return new TokenStringLiteral(new string(characters), this.lineNumber);

        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal number = literal.GetNumber();
            int length = (int)decimal.Round(this.text.Length / number);

            return new TokenStringLiteral(this.text.Substring(0, length), this.lineNumber);

        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            throw new TokenException(this, "Invalid literal operation.");
        }
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            int a = this.text.Length;
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
                    int len = this.text.Length;

                    if (len == 0)
                        return new TokenSelectorLiteral(Selector.Core.s, this.lineNumber);

                    if(len > 1 && this.text[0] == '@')
                    {
                        char _core = this.text[1];
                        Selector.Core core = Selector.ParseCore(_core);

                        if (len > 2)
                        {
                            Selector parsed = Selector.Parse(core, this.text.Substring(2));
                            return new TokenSelectorLiteral(parsed, this.lineNumber);
                        }

                        var single = new Selector { core = core };
                        return new TokenSelectorLiteral(single, this.lineNumber);
                    }

                    string name = this.text;

                    // try finding by managed entity name
                    if (executor.entities.Search(name, out Selector find))
                        return new TokenSelectorLiteral(find, this.lineNumber);

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
                    }, this.lineNumber);
                }
                case 1:
                    return new TokenIdentifier(this.text, this.lineNumber);
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
                    int length = this.text.Length;
                    if (value >= length || value < 0)
                        throw integer.GetIndexOutOfBoundsException(0, length - 1, forExceptions);

                    string newString = this.text[value].ToString();
                    return new TokenStringLiteral(newString, this.lineNumber);
                }
                case TokenIndexerRange range:
                {
                    Range value = range.token.range;
                    int length = this.text.Length;
                
                    if (value.min.HasValue && value.min < 0)
                        throw range.GetIndexOutOfBoundsException(0, length - 1, forExceptions);
                    if (value.max.HasValue && value.max >= length)
                        throw range.GetIndexOutOfBoundsException(0, length - 1, forExceptions);

                    int start = value.min ?? 0;
                    int end = value.max ?? (length - 1);

                    string newString = this.text.Substring(start, end + 1);
                    return new TokenStringLiteral(newString, this.lineNumber);
                }
                default:
                    throw indexer.GetException(this, forExceptions);
            }
        }

        public string GetDocumentation() => "A block of text on a single line, surrounded with either 'single quotes' or \"double quotes.\"";
    }
    /// <summary>
    /// Represents a block state that has been explicitly specified.
    /// </summary>
    public sealed class TokenBlockStateLiteral : TokenLiteral
    {
        public override string AsString() => this.blockState.ToString();
        public override string ToString() => this.blockState.ToString();

        private readonly BlockState blockState;

        public TokenBlockStateLiteral(string fieldName, TokenLiteral fieldValue, int lineNumber) : base(lineNumber)
        {
            this.blockState = BlockState.FromLiteral(fieldName, fieldValue);
        }
        private TokenBlockStateLiteral(BlockState blockState, int lineNumber) : base(lineNumber)
        {
            this.blockState = blockState;
        }
        
        public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
        {
            throw new StatementException(tokens, "Value type cannot be inferred from a block-state entry.");
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
            throw new TokenException(this, "Invalid literal operation.");
        }
        
        public override Typedef GetTypedef()
        {
            return null;
        }
        public override TokenLiteral Clone()
        {
            return new TokenBlockStateLiteral(this.blockState, this.lineNumber);
        }
    }
    public sealed class TokenBooleanLiteral : TokenNumberLiteral, IPreprocessor, IDocumented
    {
        public readonly bool boolean;
        public override string AsString() => this.boolean.ToString();
        internal TokenBooleanLiteral() : base(-1) { }
        public TokenBooleanLiteral(bool boolean, int lineNumber) : base(lineNumber)
        {
            this.boolean = boolean;
        }
        public override TokenLiteral Clone() => new TokenBooleanLiteral(this.boolean, this.lineNumber);
        public override string ToString() => this.boolean.ToString();
        public override object GetValue() => this.boolean;
        public override decimal GetNumber()
        {
            return this.boolean ? 1M : 0M;
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
        private readonly Coordinate coordinate;
        
        public override string AsString() => this.coordinate.ToString();
        public override string ToString() => this.coordinate.ToString();
        internal TokenCoordinateLiteral() : base(-1) { }
        public TokenCoordinateLiteral(Coordinate coordinate, int lineNumber) : base(lineNumber)
        {
            this.coordinate = coordinate;
        }
        public override TokenLiteral Clone() => new TokenCoordinateLiteral(new Coordinate(this.coordinate), this.lineNumber);
        public override decimal GetNumber()
        {
            if (this.coordinate.isDecimal)
                return this.coordinate.valueDecimal;
            else
                return this.coordinate.valueInteger;
        }
        public override object GetValue() => this.coordinate;
        public override Typedef GetTypedef() => null;
        public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
        {
            throw new StatementException(tokens, $"Cannot create a value to hold the literal '{AsString()}'");
        }

        public static implicit operator Coordinate(TokenCoordinateLiteral literal) => literal.coordinate;
        public static implicit operator int(TokenCoordinateLiteral literal) => literal.coordinate.valueInteger;
        public static implicit operator decimal(TokenCoordinateLiteral literal) => literal.coordinate.valueDecimal;

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal number = literal.GetNumber();
            var coordinate = new Coordinate(this.coordinate);
            coordinate.valueDecimal += number;
            coordinate.valueInteger = (int)decimal.Round(coordinate.valueDecimal);
            return new TokenCoordinateLiteral(coordinate, this.lineNumber);
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal number = literal.GetNumber();
            var coordinate = new Coordinate(this.coordinate);
            coordinate.valueDecimal -= number;
            coordinate.valueInteger = (int)decimal.Round(coordinate.valueDecimal);
            return new TokenCoordinateLiteral(coordinate, this.lineNumber);

        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal number = literal.GetNumber();
            var coordinate = new Coordinate(this.coordinate);
            coordinate.valueDecimal *= number;
            coordinate.valueInteger = (int)decimal.Round(coordinate.valueDecimal);
            return new TokenCoordinateLiteral(coordinate, this.lineNumber);

        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal number = literal.GetNumber();
            var coordinate = new Coordinate(this.coordinate);
            coordinate.valueDecimal /= number;
            coordinate.valueInteger = (int)decimal.Round(coordinate.valueDecimal);
            return new TokenCoordinateLiteral(coordinate, this.lineNumber);

        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal number = literal.GetNumber();
            var coordinate = new Coordinate(this.coordinate);
            coordinate.valueDecimal %= number;
            coordinate.valueInteger = (int)decimal.Round(coordinate.valueDecimal);
            return new TokenCoordinateLiteral(coordinate, this.lineNumber);

        }
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            decimal a = this.coordinate.valueDecimal;
            decimal b;

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

        public string GetDocumentation() => "A Minecraft coordinate value that can optionally be both relative and facing offset, like ~10, 40, or ^5.";
    }
    public enum IntMultiplier
    {
        [UsedImplicitly]
        none = 1,
        [UsedImplicitly]
        s = 20,
        [UsedImplicitly]
        m = 1200,
        [UsedImplicitly]
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
            if (this.multiplier == IntMultiplier.none)
                return this.number;

            // scale matches multiplier
            if (scale == this.multiplier)
                return this.number;

            return this.number / (int)scale;
        }

        public override string AsString() => this.number.ToString();
        [UsedImplicitly]
        public TokenIntegerLiteral() : base() { }
        public TokenIntegerLiteral(int number, IntMultiplier multiplier, int lineNumber) :
            base(new Coordinate(number, false, false, false), lineNumber)
        {
            this.number = number;
            this.multiplier = multiplier;
        }
        public override TokenLiteral Clone() => new TokenIntegerLiteral(this.number, this.multiplier, this.lineNumber);
        public override string ToString() => this.number.ToString();
        public override object GetValue() => this.number;
        public override decimal GetNumber()
        {
            return this.number;
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
                    return new TokenIntegerLiteral(this.number + i, IntMultiplier.none, this.lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    int i = this.number;
                    i += (int)literal.number;
                    return new TokenIntegerLiteral(i, IntMultiplier.none, this.lineNumber);
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
                    return new TokenIntegerLiteral(this.number - i, IntMultiplier.none, this.lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    int i = this.number;
                    i -= (int)literal.number;
                    return new TokenIntegerLiteral(i, IntMultiplier.none, this.lineNumber);
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
                    return new TokenIntegerLiteral(this.number * i, IntMultiplier.none, this.lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    int i = this.number;
                    i *= (int)literal.number;
                    return new TokenIntegerLiteral(i, IntMultiplier.none, this.lineNumber);
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
                    return new TokenIntegerLiteral(this.number / i, IntMultiplier.none, this.lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    int i = this.number;
                    i /= (int)literal.number;
                    return new TokenIntegerLiteral(i, IntMultiplier.none, this.lineNumber);
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
                    return new TokenIntegerLiteral(this.number % i, IntMultiplier.none, this.lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    int i = this.number;
                    i %= (int)literal.number;
                    return new TokenIntegerLiteral(i, IntMultiplier.none, this.lineNumber);
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
        public override string AsString() => this.range.ToString();
        public override string ToString() => this.range.ToString();
        internal TokenRangeLiteral() : base(-1) { }
        public TokenRangeLiteral(Range range, int lineNumber) : base(lineNumber)
        {
            this.range = range;
        }
        public override TokenLiteral Clone() => new TokenRangeLiteral(new Range(this.range), this.lineNumber);

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
                    return new TokenRangeLiteral(this.range + r, this.lineNumber);
                }
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenRangeLiteral(this.range + i, this.lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    decimal value = literal.number;
                    return new TokenRangeLiteral(this.range + value, this.lineNumber);
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
                    return new TokenRangeLiteral(this.range - r, this.lineNumber);
                }
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenRangeLiteral(this.range - i, this.lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    decimal value = literal.number;
                    return new TokenRangeLiteral(this.range - value, this.lineNumber);
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
                    return new TokenRangeLiteral(this.range * r, this.lineNumber);
                }
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenRangeLiteral(this.range * i, this.lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    decimal value = literal.number;
                    return new TokenRangeLiteral(this.range * value, this.lineNumber);
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
                    return new TokenRangeLiteral(this.range / r, this.lineNumber);
                }
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenRangeLiteral(this.range / i, this.lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    decimal value = literal.number;
                    return new TokenRangeLiteral(this.range / value, this.lineNumber);
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
                    return new TokenRangeLiteral(this.range % r, this.lineNumber);
                }
                case TokenIntegerLiteral literal:
                {
                    int i = literal.number;
                    return new TokenRangeLiteral(this.range % i, this.lineNumber);
                }
                case TokenDecimalLiteral literal:
                {
                    decimal value = literal.number;
                    return new TokenRangeLiteral(this.range % value, this.lineNumber);
                }
                default:
                    throw new TokenException(this, "Invalid literal operation.");
            }
        }
        public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
        {
            int thisMin = this.range.min ?? int.MinValue;
            int thisMax = this.range.max ?? int.MaxValue;

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
            if (!this.range.single || this.range.invert)
                return this.range;
            
            if (this.range.min != null)
                return this.range.min.Value; // dereference to an integer

            return this.range;
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
                    if(this.range.single)
                        return new TokenIntegerLiteral(this.range.min ?? 0, IntMultiplier.none, this.lineNumber);

                    switch (value)
                    {
                        // fetch min/max for 0/1
                        case 0:
                            return new TokenIntegerLiteral(this.range.min ?? 0, IntMultiplier.none, this.lineNumber);
                        case 1:
                            return new TokenIntegerLiteral(this.range.max ?? 0, IntMultiplier.none, this.lineNumber);
                        case 2:
                            return new TokenBooleanLiteral(this.range.invert, this.lineNumber);
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
                            return new TokenIntegerLiteral(this.range.min ?? 0, IntMultiplier.none, this.lineNumber);
                        case "MAX":
                        case "MAXIMUM":
                            return new TokenIntegerLiteral(this.range.max ?? 0, IntMultiplier.none, this.lineNumber);
                        case "INVERTED":
                        case "INVERT":
                            return new TokenBooleanLiteral(this.range.invert, this.lineNumber);
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
        public readonly decimal number;
        public override string AsString() => this.number.ToString();
        public TokenDecimalLiteral(decimal number, int lineNumber) :
            base(new Coordinate(number, true, false, false), lineNumber)
        {
            this.number = number;
        }
        public override TokenLiteral Clone() => new TokenDecimalLiteral(this.number, this.lineNumber);
        public override string ToString() => this.number.ToString();
        public override object GetValue() => this.number;
        public override decimal GetNumber()
        {
            return this.number;
        }

        public static implicit operator decimal(TokenDecimalLiteral literal) => literal.number;

        public override Typedef GetTypedef() => Typedef.FIXED_DECIMAL;
        public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
        {
            var data = new FixedDecimalData(this.number.GetPrecision());
            return new ScoreboardValue(name, global, Typedef.FIXED_DECIMAL, data, tokens.executor.scoreboard);
        }

        public override TokenLiteral AddWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal d = literal.GetNumber();
            return new TokenDecimalLiteral(this.number + d, this.lineNumber);
        }
        public override TokenLiteral SubWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal d = literal.GetNumber();
            return new TokenDecimalLiteral(this.number - d, this.lineNumber);
        }
        public override TokenLiteral MulWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal d = literal.GetNumber();
            return new TokenDecimalLiteral(this.number * d, this.lineNumber);
        }
        public override TokenLiteral DivWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal d = literal.GetNumber();
            return new TokenDecimalLiteral(this.number / d, this.lineNumber);
        }
        public override TokenLiteral ModWithOther(TokenLiteral other)
        {
            if (!(other is TokenNumberLiteral literal))
                throw new TokenException(this, "Invalid literal operation.");
            
            decimal d = literal.GetNumber();
            return new TokenDecimalLiteral(this.number % d, this.lineNumber);
        }
    }

    /// <summary>
    /// A selector.
    /// </summary>
    public class TokenSelectorLiteral : TokenLiteral, IPreprocessor, IDocumented
    {
        public readonly bool simple;
        public readonly Selector selector;

        public override string AsString() => this.selector.ToString();
        internal TokenSelectorLiteral() : base(-1) { }
        public TokenSelectorLiteral(Selector selector, int lineNumber) : base(lineNumber)
        {
            this.simple = false;
            this.selector = selector;
        }
        public TokenSelectorLiteral(Selector.Core core, int lineNumber) : base(lineNumber)
        {
            this.simple = true;
            this.selector = new Selector()
            {
                core = core
            };
        }
        /// <summary>
        /// Validates the selector that is contained within this token and throws a <see cref="StatementException"/> if
        /// it does not pass. 
        /// </summary>
        /// <param name="callingStatement">The statement to blame for an exception, if any.</param>
        /// <exception cref="StatementException">If the selector doesn't pass basic validation.</exception>
        /// <returns>This token.</returns>
        public TokenSelectorLiteral Validate(Statement callingStatement)
        {
            this.selector.Validate(callingStatement);
            return this;
        }
        public override TokenLiteral Clone() => new TokenSelectorLiteral(new Selector(this.selector), this.lineNumber);
        public override string ToString() => this.selector.ToString();

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

        public object GetValue() => this.selector;

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
            get => this.token.Type == JTokenType.Object;
        }
        public bool IsArray
        {
            get => this.token.Type == JTokenType.Array;
        }

        public override string AsString() => this.token.ToString();
        public override string ToString() => this.token.ToString();
        internal TokenJSONLiteral() : base(-1) { }
        public TokenJSONLiteral(JToken token, int lineNumber) : base(lineNumber)
        {
            this.token = token;
        }
        public override TokenLiteral Clone() => new TokenJSONLiteral(this.token, this.lineNumber);
        public static implicit operator JToken(TokenJSONLiteral t) => t.token;

        public override Typedef GetTypedef() => null;
        public override ScoreboardValue CreateValue(string name, bool global, Statement forExceptions)
        {
            throw new StatementException(forExceptions, $"Cannot create a value to hold the literal '{AsString()}'");
        }
        
        public object GetValue() => this.token;
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
                case TokenIndexerInteger _ when !(this.token is JArray):
                    throw new StatementException(forExceptions, "JSON type cannot be indexed using a number: " + this.token.Type);
                case TokenIndexerInteger integer:
                {
                    var array = (JArray) this.token;
                    int value = integer.token.number;
                    int len = array.Count;

                    if (value >= len || value < 0)
                        throw integer.GetIndexOutOfBoundsException(0, len - 1, forExceptions);

                    JToken indexedItem = array[value];
                    if(PreprocessorUtils.TryGetLiteral(indexedItem, this.lineNumber, out TokenLiteral output))
                        return output;
                    throw new StatementException(forExceptions, "Couldn't load JSON value: " + indexedItem);
                }
                case TokenIndexerString _ when !(this.token is JObject):
                    throw new StatementException(forExceptions, "JSON type cannot be indexed using a string: " + this.token.Type);
                case TokenIndexerString @string:
                {
                    var json = (JObject) this.token;
                    string word = @string.token.text;

                    JToken gottenToken = json[word];

                    if (gottenToken == null)
                        throw new StatementException(forExceptions, $"No JSON property found with the name '{word}'");

                    if (PreprocessorUtils.TryGetLiteral(gottenToken, this.lineNumber, out TokenLiteral output))
                        return output;
                    throw new StatementException(forExceptions, "Couldn't load JSON value: " + gottenToken);
                }
                default:
                    throw indexer.GetException(this, forExceptions);
            }
        }

        public string GetDocumentation() => "A JSON object achieved by $dereferencing a preprocessor variable holding one.";
    }
}