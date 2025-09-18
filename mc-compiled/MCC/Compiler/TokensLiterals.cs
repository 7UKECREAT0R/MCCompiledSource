using System;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.Commands.Native;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Compiler.TypeSystem.Implementations;
using Newtonsoft.Json.Linq;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     Represents a literal in code.
/// </summary>
public abstract class TokenLiteral(int lineNumber) : Token(lineNumber)
{
    protected const string DEFAULT_ERROR = "Invalid literal operation.";

    public abstract TokenLiteral Clone();
    public override string AsString() { return "<? literal>"; }
    public abstract override string ToString();

    /// <summary>
    ///     Return this literal's Scoreboard value type. Used when defining a variable with type inference.
    ///     Returns null if the literal cannot be stored in a scoreboard objective.
    /// </summary>
    /// <returns>null if the literal cannot be stored in a scoreboard objective.</returns>
    public abstract Typedef GetTypedef();

    /// <summary>
    ///     Creates a new <see cref="ScoreboardValue" /> to hold this literal.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="global"></param>
    /// <param name="tokens"></param>
    /// <returns></returns>
    public abstract ScoreboardValue CreateValue(string name,
        bool global,
        Statement tokens);

    /// <summary>
    ///     Return a NEW token literal that is the result of adding these two literals in the order THIS + OTHER.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public abstract TokenLiteral AddWithOther(TokenLiteral other);
    /// <summary>
    ///     Return a NEW token literal that is the result of subtracting these two literal in the order THIS - OTHER.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public abstract TokenLiteral SubWithOther(TokenLiteral other);
    /// <summary>
    ///     Return a NEW token literal that is the result of multiplying these two literals in the order THIS * OTHER.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public abstract TokenLiteral MulWithOther(TokenLiteral other);
    /// <summary>
    ///     Return a NEW token literal that is the result of dividing these two literals in the order THIS / OTHER.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public abstract TokenLiteral DivWithOther(TokenLiteral other);
    /// <summary>
    ///     Return a NEW token literal that is the result of modulo-ing these two literals in the order THIS % OTHER.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public abstract TokenLiteral ModWithOther(TokenLiteral other);

    /// <summary>
    ///     Return a NEW token literal that is the result of comparing these two literals using a comparison operator in the
    ///     order THIS, OTHER.
    /// </summary>
    /// <param name="cType">The type of comparison to perform.</param>
    /// <param name="other">The other literal.</param>
    /// <returns></returns>
    public abstract bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other);
}

/// <summary>
///     Represents a value which has no determined value, but is defined as 0 under every type.
/// </summary>
[TokenFriendlyName("")]
public class TokenNullLiteral(int lineNumber) : TokenLiteral(lineNumber), IPreprocessor
{
    public override string FriendlyTypeName => "null";
    public object GetValue() { return 0; }
    public override string AsString() { return "null"; }
    public override string ToString() { return "null"; }
    public override TokenLiteral Clone() { return new TokenNullLiteral(this.lineNumber); }

    public override Typedef GetTypedef() { return Typedef.INTEGER; }
    public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
    {
        return new ScoreboardValue(name, global, Typedef.INTEGER, tokens.executor.scoreboard);
    }

    public override TokenLiteral AddWithOther(TokenLiteral other)
    {
        return other switch
        {
            TokenDecimalLiteral decimalLiteral => new TokenDecimalLiteral(decimalLiteral.number, this.lineNumber),
            TokenBooleanLiteral boolLiteral => new TokenBooleanLiteral(boolLiteral.boolean, this.lineNumber),
            TokenIntegerLiteral intLiteral => new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier,
                this.lineNumber),
            _ => throw new TokenException(this, DEFAULT_ERROR)
        };
    }
    public override TokenLiteral SubWithOther(TokenLiteral other)
    {
        return other switch
        {
            TokenDecimalLiteral decimalLiteral => new TokenDecimalLiteral(decimalLiteral.number, this.lineNumber),
            TokenBooleanLiteral boolLiteral => new TokenBooleanLiteral(boolLiteral.boolean, this.lineNumber),
            TokenIntegerLiteral intLiteral => new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier,
                this.lineNumber),
            _ => throw new TokenException(this, DEFAULT_ERROR)
        };
    }
    public override TokenLiteral MulWithOther(TokenLiteral other)
    {
        return other switch
        {
            TokenDecimalLiteral decimalLiteral => new TokenDecimalLiteral(decimalLiteral.number, this.lineNumber),
            TokenBooleanLiteral boolLiteral => new TokenBooleanLiteral(boolLiteral.boolean, this.lineNumber),
            TokenIntegerLiteral intLiteral => new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier,
                this.lineNumber),
            _ => throw new TokenException(this, DEFAULT_ERROR)
        };
    }
    public override TokenLiteral DivWithOther(TokenLiteral other)
    {
        return other switch
        {
            TokenDecimalLiteral decimalLiteral => new TokenDecimalLiteral(decimalLiteral.number, this.lineNumber),
            TokenBooleanLiteral boolLiteral => new TokenBooleanLiteral(boolLiteral.boolean, this.lineNumber),
            TokenIntegerLiteral intLiteral => new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier,
                this.lineNumber),
            _ => throw new TokenException(this, DEFAULT_ERROR)
        };
    }
    public override TokenLiteral ModWithOther(TokenLiteral other)
    {
        return other switch
        {
            TokenDecimalLiteral decimalLiteral => new TokenDecimalLiteral(decimalLiteral.number, this.lineNumber),
            TokenBooleanLiteral boolLiteral => new TokenBooleanLiteral(boolLiteral.boolean, this.lineNumber),
            TokenIntegerLiteral intLiteral => new TokenIntegerLiteral(intLiteral.number, intLiteral.multiplier,
                this.lineNumber),
            _ => throw new TokenException(this, DEFAULT_ERROR)
        };
    }
    public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
    {
        if (other is not TokenNumberLiteral numberLiteral)
            throw new TokenException(this, DEFAULT_ERROR);

        decimal number = numberLiteral.GetNumber();
        return cType switch
        {
            TokenCompare.Type.EQUAL => decimal.Zero == number,
            TokenCompare.Type.NOT_EQUAL => decimal.Zero != number,
            TokenCompare.Type.LESS => decimal.Zero < number,
            TokenCompare.Type.LESS_OR_EQUAL => decimal.Zero <= number,
            TokenCompare.Type.GREATER => decimal.Zero > number,
            TokenCompare.Type.GREATER_OR_EQUAL => decimal.Zero >= number,
            _ => throw new TokenException(this, DEFAULT_ERROR)
        };
    }
}

/// <summary>
///     A token which holds a completely constructed attribute. See <see cref="Attributes.IAttribute" />.
/// </summary>
[TokenFriendlyName("attribute")]
public sealed class TokenAttribute(IAttribute attribute, int lineNumber) : TokenLiteral(lineNumber), IPreprocessor
{
    public readonly IAttribute attribute = attribute;

    public override string FriendlyTypeName => "attribute";

    public object GetValue() { return this.attribute; }
    public override string AsString() { return $"[Attribute: {this.attribute.GetDebugString()}]"; }

    public override TokenLiteral Clone() { throw new TokenException(this, "Cannot clone an attribute."); }

    public override string ToString()
    {
        if (this.attribute == null)
            return "[attribute: none]";
        return "[attribute: " + this.attribute.GetCodeRepresentation() + ']';
    }

    public override Typedef GetTypedef() { return null; }
    public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
    {
        throw new StatementException(tokens, $"Cannot create a value to hold the literal '{AsString()}'");
    }

    public override TokenLiteral AddWithOther(TokenLiteral other) { throw new TokenException(this, DEFAULT_ERROR); }
    public override TokenLiteral SubWithOther(TokenLiteral other) { throw new TokenException(this, DEFAULT_ERROR); }
    public override TokenLiteral MulWithOther(TokenLiteral other) { throw new TokenException(this, DEFAULT_ERROR); }
    public override TokenLiteral DivWithOther(TokenLiteral other) { throw new TokenException(this, DEFAULT_ERROR); }
    public override TokenLiteral ModWithOther(TokenLiteral other) { throw new TokenException(this, DEFAULT_ERROR); }
    public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
    {
        throw new TokenException(this, DEFAULT_ERROR);
    }
}

/// <summary>
///     Represents a generic number literal.
/// </summary>
public abstract class TokenNumberLiteral(int lineNumber) : TokenLiteral(lineNumber), IPreprocessor
{
    public abstract object GetValue();
    public int GetNumberInt()
    {
        return (int) GetNumber(); // floor
    }

    /// <summary>
    ///     Get the number stored in this literal.
    /// </summary>
    /// <returns></returns>
    public abstract decimal GetNumber();

    public override Typedef GetTypedef() { return Typedef.INTEGER; }
}

[TokenFriendlyName("string")]
public sealed class TokenStringLiteral(string text, int lineNumber)
    : TokenLiteral(lineNumber), IPreprocessor, IImplicitToken, IIndexable, IDocumented
{
    public readonly string text = text;
    [UsedImplicitly]
    private TokenStringLiteral() : this(null, -1) { }

    public override string FriendlyTypeName => "string";

    public string GetDocumentation()
    {
        return "A block of text on a single line, surrounded with either 'single quotes' or \"double quotes.\"";
    }

    public Type[] GetImplicitTypes() { return [typeof(TokenIdentifier)]; }
    public Token Convert(Executor executor, int index)
    {
        return index switch
        {
            0 => new TokenIdentifier(this.text, this.lineNumber),
            _ => (Token) null
        };
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
                {
                    if (forExceptions == null)
                        return null;
                    throw integer.GetIndexOutOfBoundsException(0, length - 1, forExceptions);
                }

                string newString = this.text[value].ToString();
                return new TokenStringLiteral(newString, this.lineNumber);
            }
            case TokenIndexerRange range:
            {
                Range value = range.token.range;
                int length = this.text.Length;

                if (value.min is < 0 || (value.max.HasValue && value.max >= length))
                {
                    if (forExceptions == null)
                        return null;
                    throw range.GetIndexOutOfBoundsException(0, length - 1, forExceptions);
                }

                int startInclusive = value.min ?? 0;
                int endInclusive = (value.max ?? length - 1) + 1;

                string newString = this.text.Substring(startInclusive, endInclusive - startInclusive);
                return new TokenStringLiteral(newString, this.lineNumber);
            }
            default:
            {
                if (forExceptions == null)
                    return null;
                throw indexer.GetException(this, forExceptions);
            }
        }
    }
    public object GetValue() { return this.text; }
    public override string AsString() { return '"' + this.text + '"'; }
    public override TokenLiteral Clone() { return new TokenStringLiteral(this.text, this.lineNumber); }
    public override string ToString() { return this.text; }

    public static implicit operator string(TokenStringLiteral literal) { return literal.text; }

    public override Typedef GetTypedef() { return null; }
    public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
    {
        throw new StatementException(tokens, $"Cannot create a value to hold the literal '{AsString()}'");
    }

    public override TokenLiteral AddWithOther(TokenLiteral other)
    {
        if (other is not IPreprocessor preprocessor)
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
                    str = this.text[..^str.Length];
                return new TokenStringLiteral(str, this.lineNumber);
            }
            case TokenNumberLiteral literal2:
            {
                int number = literal2.GetNumberInt();
                string str = number > this.text.Length ? "" : this.text[..^number];
                return new TokenStringLiteral(str, this.lineNumber);
            }
            default:
                throw new TokenException(this, "Invalid literal operation.");
        }
    }
    public override TokenLiteral MulWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal number = literal.GetNumber();
        int length = (int) decimal.Round(this.text.Length * number);

        int sample = 0;
        char[] characters = new char[length];
        for (int i = 0; i < length; i++)
        {
            characters[i] = this.text[sample++];
            if (sample >= this.text.Length)
                sample = 0;
        }

        return new TokenStringLiteral(new string(characters), this.lineNumber);
    }
    public override TokenLiteral DivWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal number = literal.GetNumber();
        int length = (int) decimal.Round(this.text.Length / number);

        return new TokenStringLiteral(this.text[..length], this.lineNumber);
    }
    public override TokenLiteral ModWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Invalid literal operation.");
    }
    public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
    {
        int a = this.text.Length;

        int b = other switch
        {
            TokenNumberLiteral literal1 => literal1.GetNumberInt(),
            TokenStringLiteral literal2 => literal2.text.Length,
            _ => throw new Exception("TokenStringLiteral being compared to " + other.GetType().Name)
        };

        return cType switch
        {
            TokenCompare.Type.EQUAL => a == b,
            TokenCompare.Type.NOT_EQUAL => a != b,
            TokenCompare.Type.LESS => a < b,
            TokenCompare.Type.LESS_OR_EQUAL => a <= b,
            TokenCompare.Type.GREATER => a > b,
            TokenCompare.Type.GREATER_OR_EQUAL => a >= b,
            _ => throw new Exception("Unknown comparison type: " + cType)
        };
    }
}

/// <summary>
///     Represents one or more block states that have been specified.
/// </summary>
[TokenFriendlyName("block state")]
public sealed class TokenBlockStatesLiteral(BlockState[] states, int lineNumber) : TokenLiteral(lineNumber)
{
    public readonly BlockState[] states =
        states ?? throw new ArgumentException("Parameter cannot be null.", nameof(states));

    public override string FriendlyTypeName => this.states.Length > 1 ? "block states" : "block state";
    public override string AsString() { return '[' + string.Join(",", this.states) + ']'; }
    public override string ToString() { return '[' + string.Join(",", this.states) + ']'; }

    public static implicit operator BlockState[](TokenBlockStatesLiteral literal) { return literal.states; }

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

    public override Typedef GetTypedef() { return null; }
    public override TokenLiteral Clone() { return new TokenBlockStatesLiteral(this.states, this.lineNumber); }
}

[TokenFriendlyName("true/false")]
public sealed class TokenBooleanLiteral(bool boolean, int lineNumber)
    : TokenNumberLiteral(lineNumber), IPreprocessor, IDocumented
{
    public readonly bool boolean = boolean;
    internal TokenBooleanLiteral() : this(false, -1) { }
    public override string FriendlyTypeName => "true/false";

    public string GetDocumentation() { return "A value that can be either 'true' or 'false.'"; }
    public override object GetValue() { return this.boolean; }
    public override string AsString() { return this.boolean.ToString(); }
    public override TokenLiteral Clone() { return new TokenBooleanLiteral(this.boolean, this.lineNumber); }
    public override string ToString() { return this.boolean.ToString(); }
    public override decimal GetNumber() { return this.boolean ? 1M : 0M; }

    public static implicit operator bool(TokenBooleanLiteral literal) { return literal.boolean; }

    public override Typedef GetTypedef() { return Typedef.BOOLEAN; }
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
        throw new TokenException(this, "Cannot compare a boolean to another type.");
    }
}

[TokenFriendlyName("coordinate")]
public class TokenCoordinateLiteral(Coordinate coordinate, int lineNumber)
    : TokenNumberLiteral(lineNumber), IDocumented
{
    private readonly Coordinate coordinate = coordinate;
    internal TokenCoordinateLiteral() : this(new Coordinate(), -1) { }

    public override string FriendlyTypeName => "coordinate";

    public string GetDocumentation()
    {
        return
            "A Minecraft coordinate value that can optionally be both relative and facing offset, like ~10, 40, or ^5.";
    }
    public override string AsString() { return this.coordinate.ToString(); }
    public override string ToString() { return this.coordinate.ToString(); }
    public override TokenLiteral Clone()
    {
        return new TokenCoordinateLiteral(new Coordinate(this.coordinate), this.lineNumber);
    }
    public override decimal GetNumber()
    {
        if (this.coordinate.isDecimal)
            return this.coordinate.valueDecimal;
        return this.coordinate.valueInteger;
    }
    public override object GetValue() { return this.coordinate; }
    public override Typedef GetTypedef() { return null; }
    public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
    {
        throw new StatementException(tokens, $"Cannot create a value to hold the literal '{AsString()}'");
    }

    public static implicit operator Coordinate(TokenCoordinateLiteral literal) { return literal.coordinate; }
    public static implicit operator int(TokenCoordinateLiteral literal) { return literal.coordinate.valueInteger; }
    public static implicit operator decimal(TokenCoordinateLiteral literal) { return literal.coordinate.valueDecimal; }

    public override TokenLiteral AddWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal number = literal.GetNumber();
        var modifiedCoordinate = new Coordinate(this.coordinate);
        modifiedCoordinate.valueDecimal += number;
        modifiedCoordinate.valueInteger = (int) decimal.Round(modifiedCoordinate.valueDecimal);
        return new TokenCoordinateLiteral(modifiedCoordinate, this.lineNumber);
    }
    public override TokenLiteral SubWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal number = literal.GetNumber();
        var modifiedCoordinate = new Coordinate(this.coordinate);
        modifiedCoordinate.valueDecimal -= number;
        modifiedCoordinate.valueInteger = (int) decimal.Round(modifiedCoordinate.valueDecimal);
        return new TokenCoordinateLiteral(modifiedCoordinate, this.lineNumber);
    }
    public override TokenLiteral MulWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal number = literal.GetNumber();
        var modifiedCoordinate = new Coordinate(this.coordinate);
        modifiedCoordinate.valueDecimal *= number;
        modifiedCoordinate.valueInteger = (int) decimal.Round(modifiedCoordinate.valueDecimal);
        return new TokenCoordinateLiteral(modifiedCoordinate, this.lineNumber);
    }
    public override TokenLiteral DivWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal number = literal.GetNumber();
        var modifiedCoordinate = new Coordinate(this.coordinate);
        modifiedCoordinate.valueDecimal /= number;
        modifiedCoordinate.valueInteger = (int) decimal.Round(modifiedCoordinate.valueDecimal);
        return new TokenCoordinateLiteral(modifiedCoordinate, this.lineNumber);
    }
    public override TokenLiteral ModWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal number = literal.GetNumber();
        var modifiedCoordinate = new Coordinate(this.coordinate);
        modifiedCoordinate.valueDecimal %= number;
        modifiedCoordinate.valueInteger = (int) decimal.Round(modifiedCoordinate.valueDecimal);
        return new TokenCoordinateLiteral(modifiedCoordinate, this.lineNumber);
    }
    public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
    {
        decimal a = this.coordinate.valueDecimal;

        decimal b = other switch
        {
            TokenNumberLiteral literal => literal.GetNumber(),
            TokenStringLiteral literal => literal.text.Length,
            _ => throw new Exception("TokenCoordinateLiteral being compared to " +
                                     other.GetType().Name)
        };

        return cType switch
        {
            TokenCompare.Type.EQUAL => a == b,
            TokenCompare.Type.NOT_EQUAL => a != b,
            TokenCompare.Type.LESS => a < b,
            TokenCompare.Type.LESS_OR_EQUAL => a <= b,
            TokenCompare.Type.GREATER => a > b,
            TokenCompare.Type.GREATER_OR_EQUAL => a >= b,
            _ => throw new Exception("Unknown comparison type: " + cType)
        };
    }
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

[TokenFriendlyName("integer")]
public class TokenIntegerLiteral : TokenCoordinateLiteral, IDocumented
{
    public static readonly IntMultiplier[] ALL_MULTIPLIERS = (IntMultiplier[]) Enum.GetValues(typeof(IntMultiplier));
    /// <summary>
    ///     The multiplier that was applied to this integer.
    /// </summary>
    public readonly IntMultiplier multiplier;

    /// <summary>
    ///     The number that has already been multiplied.
    /// </summary>
    public readonly int number;
    [UsedImplicitly]
    public TokenIntegerLiteral() { }
    public TokenIntegerLiteral(int number, IntMultiplier multiplier, int lineNumber) :
        base(new Coordinate(number, false, false, false), lineNumber)
    {
        this.number = number;
        this.multiplier = multiplier;
    }

    public override string FriendlyTypeName => "integer";
    public new string GetDocumentation()
    {
        return
            "Any integral number, like 5, 10, 5291, or -40. Use time suffixes to scale the integer accordingly, like with 4s -> 80.";
    }

    /// <summary>
    ///     Get the value of this number in a certain scale.
    ///     If this number has no multiplier given <see cref="IntMultiplier.none" />, then this method always returns the plain
    ///     number.
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

        return this.number / (int) scale;
    }

    public override string AsString() { return this.number.ToString(); }
    public override TokenLiteral Clone()
    {
        return new TokenIntegerLiteral(this.number, this.multiplier, this.lineNumber);
    }
    public override string ToString() { return this.number.ToString(); }
    public override object GetValue() { return this.number; }
    public override decimal GetNumber() { return this.number; }

    public override Typedef GetTypedef() { return Typedef.INTEGER; }
    public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
    {
        return new ScoreboardValue(name, global, Typedef.INTEGER, tokens.executor.scoreboard);
    }

    public static implicit operator int(TokenIntegerLiteral literal) { return literal.number; }

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
                i += (int) literal.number;
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
                i -= (int) literal.number;
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
                i *= (int) literal.number;
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
                i /= (int) literal.number;
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
                i %= (int) literal.number;
                return new TokenIntegerLiteral(i, IntMultiplier.none, this.lineNumber);
            }
            default:
                throw new TokenException(this, "Invalid literal operation.");
        }
    }
}

[TokenFriendlyName("range")]
public sealed class TokenRangeLiteral(Range range, int lineNumber)
    : TokenLiteral(lineNumber), IPreprocessor, IIndexable, IDocumented
{
    public Range range = range;
    internal TokenRangeLiteral() : this(new Range(), -1) { }
    public override string FriendlyTypeName => "range";

    public string GetDocumentation()
    {
        return
            "A Minecraft number that specifies a range of integers (inclusive). Omitting a number from one side makes the number unbounded. 4.. means four and up. 1..5 means one through five.";
    }
    public Token Index(TokenIndexer indexer, Statement forExceptions)
    {
        switch (indexer)
        {
            case TokenIndexerInteger integer:
            {
                int value = integer.token.number;
                if (value is > 2 or < 0)
                {
                    if (forExceptions == null)
                        return null;
                    throw integer.GetIndexOutOfBoundsException(0, 2, forExceptions);
                }

                // if its a single number both should return the same value
                if (this.range.single)
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
                    {
                        if (forExceptions == null)
                            return null;
                        throw new StatementException(forExceptions, $"Invalid indexer for range: '{value}'");
                    }
                }
            }
            case TokenIndexerString indexerString:
            {
                string input = indexerString.token.text.ToUpper();
                switch (input)
                {
                    case "MIN" or "MINIMUM":
                        return new TokenIntegerLiteral(this.range.min ?? 0, IntMultiplier.none, this.lineNumber);
                    case "MAX" or "MAXIMUM":
                        return new TokenIntegerLiteral(this.range.max ?? 0, IntMultiplier.none, this.lineNumber);
                    case "INVERTED" or "INVERT":
                        return new TokenBooleanLiteral(this.range.invert, this.lineNumber);
                    default:
                    {
                        if (forExceptions == null)
                            return null;
                        throw new StatementException(forExceptions, $"Invalid indexer for range: '{input}'");
                    }
                }
            }
            default:
            {
                if (forExceptions == null)
                    return null;
                throw indexer.GetException(this, forExceptions);
            }
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
    public override string AsString() { return this.range.ToString(); }
    public override string ToString() { return this.range.ToString(); }
    public override TokenLiteral Clone() { return new TokenRangeLiteral(new Range(this.range), this.lineNumber); }

    public override Typedef GetTypedef() { return null; }
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

        int b = other switch
        {
            TokenNumberLiteral literal => literal.GetNumberInt(),
            TokenStringLiteral literal => literal.text.Length,
            _ => throw new Exception("TokenRangeLiteral being compared to " + other.GetType().Name)
        };

        return cType switch
        {
            TokenCompare.Type.EQUAL => b <= thisMax && b >= thisMin,
            TokenCompare.Type.NOT_EQUAL => b < thisMin || b > thisMax,
            TokenCompare.Type.LESS => b < thisMin,
            TokenCompare.Type.LESS_OR_EQUAL => b <= thisMin,
            TokenCompare.Type.GREATER => b > thisMax,
            TokenCompare.Type.GREATER_OR_EQUAL => b >= thisMax,
            _ => throw new Exception("Unknown comparison type: " + cType)
        };
    }
}

[TokenFriendlyName("decimal")]
public sealed class TokenDecimalLiteral(decimal number, int lineNumber)
    : TokenCoordinateLiteral(new Coordinate(number, true, false, false), lineNumber)
{
    public readonly decimal number = number;
    public override string FriendlyTypeName => "decimal";
    public override string AsString() { return this.number.ToString(); }
    public override TokenLiteral Clone() { return new TokenDecimalLiteral(this.number, this.lineNumber); }
    public override string ToString() { return this.number.ToString(); }
    public override object GetValue() { return this.number; }
    public override decimal GetNumber() { return this.number; }

    public static implicit operator decimal(TokenDecimalLiteral literal) { return literal.number; }

    public override Typedef GetTypedef() { return Typedef.FIXED_DECIMAL; }
    public override ScoreboardValue CreateValue(string name, bool global, Statement tokens)
    {
        var data = new FixedDecimalData(this.number.GetPrecision());
        return new ScoreboardValue(name, global, Typedef.FIXED_DECIMAL, data, tokens.executor.scoreboard);
    }

    public override TokenLiteral AddWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal d = literal.GetNumber();
        return new TokenDecimalLiteral(this.number + d, this.lineNumber);
    }
    public override TokenLiteral SubWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal d = literal.GetNumber();
        return new TokenDecimalLiteral(this.number - d, this.lineNumber);
    }
    public override TokenLiteral MulWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal d = literal.GetNumber();
        return new TokenDecimalLiteral(this.number * d, this.lineNumber);
    }
    public override TokenLiteral DivWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal d = literal.GetNumber();
        return new TokenDecimalLiteral(this.number / d, this.lineNumber);
    }
    public override TokenLiteral ModWithOther(TokenLiteral other)
    {
        if (other is not TokenNumberLiteral literal)
            throw new TokenException(this, "Invalid literal operation.");

        decimal d = literal.GetNumber();
        return new TokenDecimalLiteral(this.number % d, this.lineNumber);
    }
}

[TokenFriendlyName("selector")]
public class TokenSelectorLiteral : TokenLiteral, IPreprocessor, IDocumented
{
    public readonly Selector selector;
    public readonly bool simple;
    internal TokenSelectorLiteral() : base(-1) { }
    public TokenSelectorLiteral(Selector selector, int lineNumber) : base(lineNumber)
    {
        this.simple = false;
        this.selector = selector;
    }
    public TokenSelectorLiteral(Selector.Core core, int lineNumber) : base(lineNumber)
    {
        this.simple = true;
        this.selector = new Selector
        {
            core = core
        };
    }

    public override string FriendlyTypeName => "selector";

    public string GetDocumentation()
    {
        return "A Minecraft selector that targets a specific entity or set of entities. Example: `@e[type=cow]`";
    }

    public object GetValue() { return this.selector; }
    public override string AsString() { return this.selector.ToString(); }
    /// <summary>
    ///     Validates the selector that is contained within this token and throws a <see cref="StatementException" /> if
    ///     it does not pass.
    /// </summary>
    /// <param name="callingStatement">The statement to blame for an exception, if any.</param>
    /// <exception cref="StatementException">If the selector doesn't pass basic validation.</exception>
    /// <returns>This token.</returns>
    public TokenSelectorLiteral Validate(Statement callingStatement)
    {
        this.selector.Validate(callingStatement);
        return this;
    }
    public override TokenLiteral Clone()
    {
        return new TokenSelectorLiteral(new Selector(this.selector), this.lineNumber);
    }
    public override string ToString() { return this.selector.ToString(); }

    public static implicit operator Selector(TokenSelectorLiteral t) { return t.selector; }
    public static implicit operator Selector.Core(TokenSelectorLiteral t) { return t.selector.core; }

    public override Typedef GetTypedef() { return null; }
    public override ScoreboardValue CreateValue(string name, bool global, Statement forExceptions)
    {
        throw new StatementException(forExceptions, $"Cannot create a value to hold the literal '{AsString()}'");
    }

    public override TokenLiteral AddWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform operations like this with selectors.");
    }
    public override TokenLiteral SubWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform operations like this with selectors.");
    }
    public override TokenLiteral MulWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform operations like this with selectors.");
    }
    public override TokenLiteral DivWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform operations like this with selectors.");
    }
    public override TokenLiteral ModWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform operations like this with selectors.");
    }
    public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
    {
        throw new TokenException(this, "Cannot compare a selector to another type.");
    }
}

/// <summary>
///     A literal holding a JSON value. Mostly used for indexing purposes.
/// </summary>
[TokenFriendlyName("JSON")]
public class TokenJSONLiteral(JToken token, int lineNumber)
    : TokenLiteral(lineNumber), IPreprocessor, IIndexable, IDocumented
{
    public readonly JToken token = token;
    internal TokenJSONLiteral() : this(null, -1) { }

    public bool IsObject => this.token.Type == JTokenType.Object;
    public bool IsArray => this.token.Type == JTokenType.Array;

    public override string FriendlyTypeName => "JSON";

    public string GetDocumentation()
    {
        return "A JSON object achieved by $dereferencing a preprocessor variable holding one.";
    }

    public Token Index(TokenIndexer indexer, Statement forExceptions)
    {
        switch (indexer)
        {
            case TokenIndexerInteger _ when this.token is not JArray:
            {
                if (forExceptions == null)
                    return null;
                throw new StatementException(forExceptions,
                    "JSON type cannot be indexed using a number: " + this.token.Type);
            }
            case TokenIndexerInteger integer:
            {
                var array = (JArray) this.token;
                int value = integer.token.number;
                int len = array.Count;

                if (value >= len || value < 0)
                {
                    if (forExceptions == null)
                        return null;
                    throw integer.GetIndexOutOfBoundsException(0, len - 1, forExceptions);
                }

                JToken indexedItem = array[value];
                if (PreprocessorUtils.TryGetLiteral(indexedItem, this.lineNumber, out TokenLiteral output))
                    return output;

                if (forExceptions == null)
                    return null;
                throw new StatementException(forExceptions, "Couldn't load JSON value: " + indexedItem);
            }
            case TokenIndexerString _ when this.token is not JObject:
            {
                if (forExceptions == null)
                    return null;
                throw new StatementException(forExceptions,
                    "JSON type cannot be indexed using a string: " + this.token.Type);
            }
            case TokenIndexerString @string:
            {
                var json = (JObject) this.token;
                string word = @string.token.text;

                JToken gottenToken = json[word];

                if (gottenToken == null)
                {
                    if (forExceptions == null)
                        return null;
                    throw new StatementException(forExceptions, $"No JSON property found with the name '{word}'");
                }

                if (PreprocessorUtils.TryGetLiteral(gottenToken, this.lineNumber, out TokenLiteral output))
                    return output;

                if (forExceptions == null)
                    return null;
                throw new StatementException(forExceptions, "Couldn't load JSON value: " + gottenToken);
            }
            default:
            {
                if (forExceptions == null)
                    return null;
                throw indexer.GetException(this, forExceptions);
            }
        }
    }

    public object GetValue() { return this.token; }
    public override string AsString() { return this.token.ToString(); }
    public override string ToString() { return this.token.ToString(); }
    public override TokenLiteral Clone() { return new TokenJSONLiteral(this.token, this.lineNumber); }
    public static implicit operator JToken(TokenJSONLiteral t) { return t.token; }

    public override Typedef GetTypedef() { return null; }
    public override ScoreboardValue CreateValue(string name, bool global, Statement forExceptions)
    {
        throw new StatementException(forExceptions, $"Cannot create a value to hold the literal '{AsString()}'");
    }
    public override TokenLiteral AddWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform operations like this with JSON tokens.");
    }
    public override TokenLiteral SubWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform operations like this with JSON tokens.");
    }
    public override TokenLiteral MulWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform operations like this with JSON tokens.");
    }
    public override TokenLiteral DivWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform operations like this with JSON tokens.");
    }
    public override TokenLiteral ModWithOther(TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform operations like this with JSON tokens.");
    }
    public override bool CompareWithOther(TokenCompare.Type cType, TokenLiteral other)
    {
        throw new TokenException(this, "Cannot perform comparisons with JSON tokens.");
    }
}