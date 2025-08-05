using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using mc_compiled.Commands.Selectors;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     A token that represents some kind of operator like =, +=, +, %, [n], etc...
/// </summary>
[TokenFriendlyName("operator")]
public class TokenOperator : Token
{
    public TokenOperator(int lineNumber) : base(lineNumber) { }
    public override string FriendlyTypeName => "generic operator";
    public override string AsString() { return "<? generic>"; }
}

/// <summary>
///     An indexer, identified by a token surrounded by [square brackets]. Used to index/scope things like values and PPVs.
/// </summary>
public abstract class TokenIndexer : Token
{
    public readonly Token[] innerTokens;
    protected TokenIndexer(IEnumerable<Token> innerTokens, int lineNumber) : base(lineNumber)
    {
        this.innerTokens = innerTokens.ToArray();
    }
    protected TokenIndexer(Token[] innerTokens, int lineNumber) : base(lineNumber) { this.innerTokens = innerTokens; }
    protected TokenIndexer(Token innerToken, int lineNumber) : base(lineNumber) { this.innerTokens = [innerToken]; }
    public override string AsString() { return $"[{string.Join(", ", this.innerTokens.Select(t => t.AsString()))}]"; }

    /// <summary>
    ///     Get the primary token inside this indexer, or null if there is no primary token.
    /// </summary>
    /// <returns></returns>
    public abstract Token GetPrimaryToken();

    /// <summary>
    ///     Determines whether the indexer actually indexes/scope something. Block states do not index things.
    /// </summary>
    /// <returns>Returns true if the indexer actually indexes/scope something; otherwise, false.</returns>
    public abstract bool ActuallyIndexes();

    /// <summary>
    ///     Creates an instance of <see cref="TokenIndexer" /> based on the input string,
    ///     interpreting the string to determine the appropriate derived type.
    /// </summary>
    /// <param name="input">
    ///     The string representing the content to be parsed into a <see cref="TokenIndexer" />.
    ///     This value should not be null, empty, or whitespace.
    ///     Valid patterns include:
    ///     <ul>
    ///         <li>"*" (creates a <see cref="TokenIndexerAsterisk" />)</li>
    ///         <li>Integer values (creates a <see cref="TokenIndexerInteger" />)</li>
    ///         <li>Ranges in a valid format (creates a <see cref="TokenIndexerRange" />)</li>
    ///         <li>
    ///             Quoted strings (e.g., enclosed in ' or " and matching both ends; creates a
    ///             <see cref="TokenIndexerString" />)
    ///         </li>
    ///         <li>Selector strings starting with "@" (creates a <see cref="TokenIndexerSelector" />).</li>
    ///         <li>Any other text results in <c>null</c>.</li>
    ///     </ul>
    /// </param>
    /// <param name="lineNumber">
    ///     The line number associated with the source of the provided input for debugging or tracking purposes.
    ///     Defaults to -1 if not provided.
    /// </param>
    /// <returns>
    ///     A derived instance of <see cref="TokenIndexer" /> based on the content of the <paramref name="input" /> string:
    ///     <ul>
    ///         <li><see cref="TokenIndexerAsterisk" />: If the input is "*".</li>
    ///         <li><see cref="TokenIndexerInteger" />: If the input is an integer.</li>
    ///         <li><see cref="TokenIndexerRange" />: If the input is a valid range.</li>
    ///         <li><see cref="TokenIndexerString" />: If the input is a quoted string.</li>
    ///         <li><see cref="TokenIndexerSelector" />: If the input represents a valid selector.</li>
    ///         <li><see cref="TokenIndexerUnknown" />: For inputs that do not match any of the above.</li>
    ///         <li>Returns <c>null</c> if the input doesn't match any of the above.</li>
    ///     </ul>
    /// </returns>
    public static TokenIndexer CreateIndexerFromString(string input, int lineNumber = -1)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        input = input.Trim();

        if (input == "*")
            return new TokenIndexerAsterisk(lineNumber);
        if (int.TryParse(input, out int integer))
            return new TokenIndexerInteger(new TokenIntegerLiteral(integer, IntMultiplier.none, lineNumber),
                lineNumber);
        if (Range.TryParse(input, out Range range))
            return new TokenIndexerRange(new TokenRangeLiteral(range, lineNumber), lineNumber);
        if ((input[0] == '"' || input[0] == '\'') && input[0] == input[^1])
        {
            string text = input.Substring(1, input.Length - 2);
            return new TokenIndexerString(new TokenStringLiteral(text, lineNumber), lineNumber);
        }

        if (input[0] == '@' && Selector.TryParse(input, out Selector selector))
            return new TokenIndexerSelector(new TokenSelectorLiteral(selector, lineNumber), lineNumber);

        return null;
    }
    /// <summary>
    ///     Creates an indexer based on the type of token given.
    ///     Throws an exception if there's no valid indexer for the given token.
    /// </summary>
    /// <param name="tokens">The tokens to create the indexer for.</param>
    /// <param name="forExceptions">
    ///     Statement that would be considered the one throwing this exception. If null, will throw a
    ///     tokenizer exception.
    /// </param>
    /// <returns></returns>
    public static TokenIndexer CreateIndexer(Token[] tokens, Statement forExceptions = null)
    {
        switch (tokens.Length)
        {
            case 0:
                return new TokenIndexerUnknown([], forExceptions?.Lines[0] ?? -1);
            case 1:
            {
                Token token = tokens[0];

                switch (token)
                {
                    case TokenLiteral literal:
                        return CreateIndexerSingle(literal, forExceptions);
                    case TokenMultiply _:
                        return new TokenIndexerAsterisk(token.lineNumber);
                }

                break;
            }
        }

        // block states?
        if (tokens.All(token => token is TokenBlockStateLiteral))
            return new TokenIndexerBlockStates(tokens.Cast<TokenBlockStateLiteral>().ToArray(),
                forExceptions?.Lines[0] ?? tokens[0].lineNumber);

        // no clue, return TokenIndexerUnknown containing the tokens
        return new TokenIndexerUnknown(tokens, forExceptions?.Lines[0] ?? tokens[0].lineNumber);
    }
    /// <summary>
    ///     Creates an indexer based on the type of literal given.
    ///     Throws an exception if there's no valid indexer for the given literal.
    /// </summary>
    /// <param name="literal">The literal to wrap in an indexer.</param>
    /// <param name="forExceptions">
    ///     Statement that would be considered the one throwing this exception. If null, will throw a
    ///     tokenizer exception.
    /// </param>
    /// <returns></returns>
    // ReSharper disable once SuggestBaseTypeForParameter
    private static TokenIndexer CreateIndexerSingle(TokenLiteral literal, Statement forExceptions = null)
    {
        int lineNumber = literal.lineNumber;

        switch (literal)
        {
            case TokenIntegerLiteral intLiteral:
                return new TokenIndexerInteger(intLiteral, lineNumber);
            case TokenStringLiteral stringLiteral:
                return new TokenIndexerString(stringLiteral, lineNumber);
            case TokenSelectorLiteral selectorLiteral:
                return new TokenIndexerSelector(selectorLiteral, lineNumber);
            case TokenRangeLiteral rangeLiteral:
                return new TokenIndexerRange(rangeLiteral, lineNumber);
            case TokenBlockStateLiteral blockStateLiteral:
                return new TokenIndexerBlockStates([blockStateLiteral], lineNumber);
        }

        if (forExceptions == null)
            throw new TokenizerException("Cannot index/scope with a token: " + literal.DebugString(), [
                literal.lineNumber
            ]);

        throw new StatementException(forExceptions, "Cannot index/scope with a token: " + literal.DebugString());
    }

    /// <summary>
    ///     Get an exception from this indexer, implying it cannot index the calling object.
    /// </summary>
    internal Exception GetException(IIndexable caller, Statement thrower)
    {
        string callerName = caller.GetType().Name;
        return new StatementException(thrower, $"Cannot index '{callerName}' using indexer: {AsString()}");
    }
}

/// <summary>
///     Represents an unknown token indexer with some arbitrary number of tokens in it.
/// </summary>
[TokenFriendlyName("indexer [?]")]
public sealed class TokenIndexerUnknown : TokenIndexer
{
    public TokenIndexerUnknown(Token[] innerTokens, int lineNumber) : base(innerTokens, lineNumber) { }
    public override string FriendlyTypeName => "indexer [?]";
    public override Token GetPrimaryToken() { return null; }
    public override bool ActuallyIndexes() { return false; }
}

/// <summary>
///     An indexer giving an integer. Defaulted to this class with the value 0 when [] is given to the tokenizer.
/// </summary>
[TokenFriendlyName("indexer [integer]")]
public sealed class TokenIndexerInteger : TokenIndexer
{
    public readonly TokenIntegerLiteral token;
    public TokenIndexerInteger(TokenIntegerLiteral token, int lineNumber) : base(token, lineNumber)
    {
        this.token = token;
    }
    public override string FriendlyTypeName => "indexer [integer]";
    public override string AsString() { return $"[{this.token.number}]"; }
    public override Token GetPrimaryToken() { return this.token; }
    public override bool ActuallyIndexes() { return true; }

    internal Exception GetIndexOutOfBoundsException(int min, int max, Statement thrower)
    {
        return new StatementException(thrower, $"Index {this.token.number} was out of bounds. Min: {min}, Max: {max}");
    }
}

/// <summary>
///     An indexer giving a string.
/// </summary>
[TokenFriendlyName("indexer [string]")]
public sealed class TokenIndexerString : TokenIndexer
{
    public readonly TokenStringLiteral token;
    public TokenIndexerString(TokenStringLiteral token, int lineNumber) : base(token, lineNumber)
    {
        this.token = token;
    }
    public override string FriendlyTypeName => "indexer [string]";
    public override string AsString() { return $"[\"{this.token.text}\"]"; }
    public override Token GetPrimaryToken() { return this.token; }
    public override bool ActuallyIndexes() { return true; }
}

/// <summary>
///     An indexer indicating a range value.
/// </summary>
[TokenFriendlyName("indexer [range]")]
public sealed class TokenIndexerRange : TokenIndexer
{
    public readonly TokenRangeLiteral token;
    public TokenIndexerRange(TokenRangeLiteral token, int lineNumber) : base(token, lineNumber) { this.token = token; }
    public override string FriendlyTypeName => "indexer [range]";
    public override string AsString() { return $"[\"{this.token.range.ToString()}\"]"; }
    public override Token GetPrimaryToken() { return this.token; }
    public override bool ActuallyIndexes() { return true; }

    internal Exception GetIndexOutOfBoundsException(int min, int max, Statement thrower)
    {
        return new StatementException(thrower,
            $"Range {this.token.range.ToString()} was out of bounds. Min: {min}, Max: {max}");
    }
}

/// <summary>
///     An indexer indicating a set of block states.
/// </summary>
[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
[TokenFriendlyName("indexer [block state]")]
public sealed class TokenIndexerBlockStates : TokenIndexer
{
    public readonly List<TokenBlockStateLiteral> blockStates;
    public TokenIndexerBlockStates(IEnumerable<TokenBlockStateLiteral> addStates, int lineNumber) : base(addStates,
        lineNumber)
    {
        this.blockStates = [];
        this.blockStates.AddRange(addStates);
    }
    public override string FriendlyTypeName =>
        this.blockStates.Count == 1 ? "indexer [block state]" : "indexer [block states]";
    public override string AsString() { return $"[{string.Join(", ", this.blockStates.Select(bs => bs.AsString()))}]"; }
    public override Token GetPrimaryToken() { return null; }
    public override bool ActuallyIndexes() { return false; }
}

/// <summary>
///     An indexer giving a selector.
/// </summary>
[TokenFriendlyName("indexer [selector]")]
public sealed class TokenIndexerSelector : TokenIndexer
{
    public readonly TokenSelectorLiteral token;
    public TokenIndexerSelector(TokenSelectorLiteral token, int lineNumber) : base(token, lineNumber)
    {
        this.token = token;
    }
    public override string FriendlyTypeName => "indexer [selector]";
    public override string AsString() { return $"[{this.token.selector}]"; }
    public override Token GetPrimaryToken() { return this.token; }
    public override bool ActuallyIndexes() { return true; }
}

/// <summary>
///     An indexer for a single asterisk character (*).
/// </summary>
[TokenFriendlyName("indexer [*]")]
public sealed class TokenIndexerAsterisk : TokenIndexer
{
    public TokenIndexerAsterisk(int lineNumber) : base(new TokenMultiply(lineNumber), lineNumber) { }
    public override string FriendlyTypeName => "indexer [*]";
    public override string AsString() { return "*"; }

    public override Token GetPrimaryToken() { return new TokenMultiply(this.lineNumber); }
    public override bool ActuallyIndexes() { return true; }
}

/// <summary>
///     Represents a generic bracket, not open or closed.
/// </summary>
[TokenFriendlyName("bracket")]
public class TokenBracket : TokenOperator
{
    public TokenBracket(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "<? bracket>"; }
}

/// <summary>
///     Represents an opening bracket, extends TokenBracket.
/// </summary>
[TokenFriendlyName("open bracket")]
public class TokenOpenBracket : TokenBracket
{
    public TokenOpenBracket(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "<? bracket open>"; }
}

/// <summary>
///     Represents a closing bracket, extends TokenBracket.
/// </summary>
[TokenFriendlyName("close bracket")]
public class TokenCloseBracket : TokenBracket
{
    public TokenCloseBracket(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "<? bracket close>"; }
}

/// <summary>
///     Represents an opening bracket which groups tokens, extends TokenOpenBracket.
/// </summary>
[TokenFriendlyName("open bracket")]
public abstract class TokenOpenGroupingBracket : TokenOpenBracket
{
    public bool hasBeenSquashed = false; // used to prevent function squashing from recursively running forever
    public TokenOpenGroupingBracket(int lineNumber) : base(lineNumber) { }
    public override string FriendlyTypeName => "opening bracket";
    public abstract bool IsAssociated(TokenBracket bracket);
    public override string AsString() { return "<? grouping bracket open>"; }
}

/// <summary>
///     Represents a closing bracket which groups tokens, extends TokenCloseBracket.
/// </summary>
[TokenFriendlyName("close bracket")]
public abstract class TokenCloseGroupingBracket : TokenCloseBracket
{
    public TokenCloseGroupingBracket(int lineNumber) : base(lineNumber) { }
    public override string FriendlyTypeName => "closing bracket";
    public override string AsString() { return "<? grouping bracket close>"; }
}

/// <summary>
///     Represents an arithmetic operator token.
/// </summary>
public abstract class TokenArithmetic : TokenOperator
{
    public enum Type
    {
        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE,
        MODULO,
        SWAP
    }

    public TokenArithmetic(int lineNumber) : base(lineNumber) { }

    public override string AsString() { return "<? arithmatic>"; }

    public abstract Type GetArithmeticType();
}

public abstract class TokenArithmeticFirst : TokenArithmetic
{
    public TokenArithmeticFirst(int lineNumber) : base(lineNumber) { }
    public override string FriendlyTypeName => "math operator";
    public override string AsString() { return "<? arithmatic first>"; }
}

public abstract class TokenArithmeticSecond : TokenArithmetic
{
    public TokenArithmeticSecond(int lineNumber) : base(lineNumber) { }
    public override string FriendlyTypeName => "math operator";
    public override string AsString() { return "<? arithmatic second>"; }
}

/// <summary>
///     Extensions for <see cref="TokenCompare.Type" />.
/// </summary>
public static class TokenCompareTypeExtensions
{
    public static Range AsRange(this TokenCompare.Type type, int comparingTo)
    {
        return type switch
        {
            TokenCompare.Type.EQUAL => new Range(comparingTo, false),
            TokenCompare.Type.NOT_EQUAL => new Range(comparingTo, true),
            TokenCompare.Type.LESS => new Range(null, comparingTo - 1),
            TokenCompare.Type.LESS_OR_EQUAL => new Range(null, comparingTo),
            TokenCompare.Type.GREATER => new Range(comparingTo + 1, null),
            TokenCompare.Type.GREATER_OR_EQUAL => new Range(comparingTo, null),
            _ => new Range()
        };
    }
}

/// <summary>
///     Represents a generic comparison operator.
/// </summary>
public abstract class TokenCompare : TokenOperator, IDocumented
{
    public enum Type
    {
        EQUAL,
        NOT_EQUAL,
        LESS,
        LESS_OR_EQUAL,
        GREATER,
        GREATER_OR_EQUAL
    }

    internal TokenCompare() : base(-1) { }
    public TokenCompare(int lineNumber) : base(lineNumber) { }

    public override string FriendlyTypeName => "comparison operator";

    public string GetDocumentation() { return "Any comparison operator. Allowed values are: <, >, <=, >=, ==, !="; }
    /// <summary>
    ///     Returns the minecraft operator for the given TokenCompare.Type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetMinecraftOperator(Type type)
    {
        return type switch
        {
            Type.EQUAL => "=",
            Type.NOT_EQUAL => "DOESNT EXIST MOJANG",
            Type.LESS => "<",
            Type.LESS_OR_EQUAL => "<=",
            Type.GREATER => ">",
            Type.GREATER_OR_EQUAL => ">=",
            _ => "??"
        };
    }
    public override string AsString() { return "<? compare>"; }

    public abstract Type GetCompareType();
}

/// <summary>
///     Used to indicate that this operator assigns the identifier to the left of it.
/// </summary>
public interface IAssignment { }

/// <summary>
///     Used to indicate when a token should terminate the Assembler's token collector and start a new line.
/// </summary>
public interface ITerminating { }

/// <summary>
///     Used to indicate when a token holds no useful information for the compiler e.g., a comment.
/// </summary>
public interface IUselessInformation { }

[TokenFriendlyName("open parenthesis")]
public sealed class TokenOpenParenthesis : TokenOpenGroupingBracket
{
    public TokenOpenParenthesis(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "("; }
    public override bool IsAssociated(TokenBracket bracket)
    {
        return bracket is TokenOpenParenthesis || bracket is TokenCloseParenthesis;
    }
}

[TokenFriendlyName("close parenthesis")]
public sealed class TokenCloseParenthesis : TokenCloseGroupingBracket
{
    public TokenCloseParenthesis(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return ")"; }
}

[TokenFriendlyName("open indexer")]
public sealed class TokenOpenIndexer : TokenOpenGroupingBracket
{
    public TokenOpenIndexer(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "["; }
    public override bool IsAssociated(TokenBracket bracket)
    {
        return bracket is TokenOpenIndexer || bracket is TokenCloseIndexer;
    }
}

[TokenFriendlyName("close indexer")]
public sealed class TokenCloseIndexer : TokenCloseGroupingBracket
{
    public TokenCloseIndexer(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "]"; }
}

[TokenFriendlyName("open block")]
public sealed class TokenOpenBlock : TokenOpenBracket, ITerminating
{
    public TokenOpenBlock(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "{"; }
}

[TokenFriendlyName("close block")]
public sealed class TokenCloseBlock : TokenCloseBracket, ITerminating
{
    public TokenCloseBlock(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "}"; }
}

/// <summary>
///     The two dots in a range argument. 123..456
/// </summary>
[TokenFriendlyName("range dots (..)")]
public sealed class TokenRangeDots : Token
{
    public TokenRangeDots(int lineNumber) : base(lineNumber) { }
    public override string FriendlyTypeName => "range dots (..)";
    public override string AsString() { return ".."; }
}

/// <summary>
///     The inverter signaling to invert a range argument.
/// </summary>
[TokenFriendlyName("range invert (!)")]
public sealed class TokenRangeInvert : Token
{
    public TokenRangeInvert(int lineNumber) : base(lineNumber) { }
    public override string FriendlyTypeName => "range invert (!)";
    public override string AsString() { return "!"; }
}

/// <summary>
///     An AND/OR identifier. Continues a selector transformation.
/// </summary>
public abstract class TokenContinueCompareChain : TokenOperator
{
    public TokenContinueCompareChain(int lineNumber) : base(lineNumber) { }
}

[TokenFriendlyName("and")]
public sealed class TokenAnd : TokenContinueCompareChain
{
    public TokenAnd(int lineNumber) : base(lineNumber) { }
    public override string FriendlyTypeName => "and";
    public override string AsString() { return "and"; }
}

[TokenFriendlyName("or")]
public sealed class TokenOr : TokenContinueCompareChain
{
    public TokenOr(int lineNumber) : base(lineNumber) { }
    public override string FriendlyTypeName => "or";
    public override string AsString() { return "or"; }
}

[TokenFriendlyName("not")]
public sealed class TokenNot : TokenOperator
{
    public TokenNot(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "not"; }
}

[TokenFriendlyName("dereference operator")]
public sealed class TokenDeref : TokenOperator
{
    public TokenDeref(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "$"; }
}

[TokenFriendlyName("assignment operator")]
public sealed class TokenAssignment : TokenOperator, IAssignment
{
    public TokenAssignment(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "="; }
}

[TokenFriendlyName("add operator")]
public sealed class TokenAdd : TokenArithmeticSecond
{
    public TokenAdd(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "+"; }

    public override Type GetArithmeticType() { return Type.ADD; }
}

[TokenFriendlyName("subtract operator")]
public sealed class TokenSubtract : TokenArithmeticSecond
{
    public TokenSubtract(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "-"; }

    public override Type GetArithmeticType() { return Type.SUBTRACT; }
}

[TokenFriendlyName("multiply operator")]
public sealed class TokenMultiply : TokenArithmeticFirst
{
    public TokenMultiply(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "*"; }

    public override Type GetArithmeticType() { return Type.MULTIPLY; }
}

[TokenFriendlyName("divide operator")]
public sealed class TokenDivide : TokenArithmeticFirst
{
    public TokenDivide(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "/"; }

    public override Type GetArithmeticType() { return Type.DIVIDE; }
}

[TokenFriendlyName("modulo operator")]
public sealed class TokenModulo : TokenArithmeticFirst
{
    public TokenModulo(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "%"; }

    public override Type GetArithmeticType() { return Type.MODULO; }
}

[TokenFriendlyName("add/assignment operator")]
public sealed class TokenAddAssignment : TokenArithmeticSecond, IAssignment
{
    public TokenAddAssignment(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "+="; }

    public override Type GetArithmeticType() { return Type.ADD; }
}

[TokenFriendlyName("subtract/assignment operator")]
public sealed class TokenSubtractAssignment : TokenArithmeticSecond, IAssignment
{
    public TokenSubtractAssignment(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "-="; }

    public override Type GetArithmeticType() { return Type.SUBTRACT; }
}

[TokenFriendlyName("multiply/assignment operator")]
public sealed class TokenMultiplyAssignment : TokenArithmeticFirst, IAssignment
{
    public TokenMultiplyAssignment(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "*="; }

    public override Type GetArithmeticType() { return Type.MULTIPLY; }
}

[TokenFriendlyName("divide/assignment operator")]
public sealed class TokenDivideAssignment : TokenArithmeticFirst, IAssignment
{
    public TokenDivideAssignment(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "/="; }

    public override Type GetArithmeticType() { return Type.DIVIDE; }
}

[TokenFriendlyName("modulo/assignment operator")]
public sealed class TokenModuloAssignment : TokenArithmeticFirst, IAssignment
{
    public TokenModuloAssignment(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "%="; }

    public override Type GetArithmeticType() { return Type.MODULO; }
}

[TokenFriendlyName("swap/assignment operator")]
public sealed class TokenSwapAssignment : TokenArithmeticFirst, IAssignment
{
    public TokenSwapAssignment(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "><"; }

    public override Type GetArithmeticType() { return Type.SWAP; }
}

[TokenFriendlyName("equality operator")]
public sealed class TokenEquality : TokenCompare
{
    public TokenEquality(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "=="; }

    public override Type GetCompareType() { return Type.EQUAL; }
}

[TokenFriendlyName("inequality operator")]
public sealed class TokenInequality : TokenCompare
{
    public TokenInequality(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "!="; }

    public override Type GetCompareType() { return Type.NOT_EQUAL; }
}

[TokenFriendlyName("less than operator")]
public sealed class TokenLessThan : TokenCompare
{
    public TokenLessThan(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "<"; }

    public override Type GetCompareType() { return Type.LESS; }
}

[TokenFriendlyName("greater than operator")]
public sealed class TokenGreaterThan : TokenCompare
{
    public TokenGreaterThan(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return ">"; }

    public override Type GetCompareType() { return Type.GREATER; }
}

[TokenFriendlyName("less than or equal operator")]
public sealed class TokenLessThanEqual : TokenCompare
{
    public TokenLessThanEqual(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return "<="; }

    public override Type GetCompareType() { return Type.LESS_OR_EQUAL; }
}

[TokenFriendlyName("greater than or equal operator")]
public sealed class TokenGreaterThanEqual : TokenCompare
{
    public TokenGreaterThanEqual(int lineNumber) : base(lineNumber) { }
    public override string AsString() { return ">="; }

    public override Type GetCompareType() { return Type.GREATER_OR_EQUAL; }
}