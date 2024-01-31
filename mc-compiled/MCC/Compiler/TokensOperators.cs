using mc_compiled.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A token that represents some kind of operator like =, +=, +, %, [n], etc...
    /// </summary>
    public class TokenOperator : Token
    {
        public override string AsString() => "<? generic>";
        public TokenOperator(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// An indexer, identified by a token surrounded by [square brackets]. Used to index/scope things like values and PPVs.
    /// </summary>
    public abstract class TokenIndexer : Token
    {
        public readonly Token[] innerTokens;
        public override string AsString() => $"[{string.Join(", ", innerTokens.Select(t => t.AsString()))}]";
        protected TokenIndexer(IEnumerable<Token> innerTokens, int lineNumber) : base(lineNumber)
        {
            this.innerTokens = innerTokens.ToArray();
        }
        protected TokenIndexer(Token[] innerTokens, int lineNumber) : base(lineNumber)
        {
            this.innerTokens = innerTokens;
        }
        protected TokenIndexer(Token innerToken, int lineNumber) : base(lineNumber)
        {
            innerTokens = new[] { innerToken };
        }

        /// <summary>
        /// Get the primary token inside this indexer, or null if there is no primary token.
        /// </summary>
        /// <returns></returns>
        public abstract Token GetPrimaryToken();

        /// <summary>
        /// Determines whether the indexer actually indexes/scope something. Block states do not index things.
        /// </summary>
        /// <returns>Returns true if the indexer actually indexes/scope something; otherwise, false.</returns>
        public abstract bool ActuallyIndexes();

        /// <summary>
        /// Creates an indexer based on the type of token given.
        /// Throws a exception if there's no valid indexer for the given token.
        /// </summary>
        /// <param name="tokens">The tokens to create the indexer for.</param>
        /// <param name="forExceptions">Statement that would be considered the one throwing this exception. If null, will throw a tokenizer exception.</param>
        /// <returns></returns>
        public static TokenIndexer CreateIndexer(Token[] tokens, Statement forExceptions = null)
        {
            switch (tokens.Length)
            {
                case 0:
                    return new TokenIndexerUnknown(Array.Empty<Token>(), forExceptions?.Lines[0] ?? -1);
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
            {
                return new TokenIndexerBlockStates(tokens.Cast<TokenBlockStateLiteral>().ToArray(),
                    forExceptions?.Lines[0] ?? tokens[0].lineNumber);
            }
            
            // no clue, return TokenIndexerUnknown containing the tokens
            return new TokenIndexerUnknown(tokens, forExceptions?.Lines[0] ?? tokens[0].lineNumber);
        }
        /// <summary>
        /// Creates an indexer based on the type of literal given.
        /// Throws an exception if there's no valid indexer for the given literal.
        /// </summary>
        /// <param name="literal">The literal to wrap in an indexer.</param>
        /// <param name="forExceptions">Statement that would be considered the one throwing this exception. If null, will throw a tokenizer exception.</param>
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
                    return new TokenIndexerBlockStates(new[] {blockStateLiteral}, lineNumber);
            }

            if (forExceptions == null)
                throw new TokenizerException($"Cannot index/scope with a token: " + literal.DebugString(), new[] { literal.lineNumber } );
            
            throw new StatementException(forExceptions, $"Cannot index/scope with a token: " + literal.DebugString());
        }

        /// <summary>
        /// Get an exception from this indexer, implying it cannot index the calling object.
        /// </summary>
        internal Exception GetException(IIndexable caller, Statement thrower)
        {
            string callerName = caller.GetType().Name;
            return new StatementException(thrower, $"Cannot index '{callerName}' using indexer: {AsString()}");
        }
    }

    /// <summary>
    /// Represents an unknown token indexer with some arbitrary number of tokens in it.
    /// </summary>
    public sealed class TokenIndexerUnknown : TokenIndexer
    {
        public TokenIndexerUnknown(Token[] innerTokens, int lineNumber) : base(innerTokens, lineNumber) {}
        public override Token GetPrimaryToken() => null;
        public override bool ActuallyIndexes() => false;
    }
    /// <summary>
    /// An indexer giving an integer. Defaulted to this class with the value 0 when [] is given to the tokenizer.
    /// </summary>
    public sealed class TokenIndexerInteger : TokenIndexer
    {
        public override string AsString() => $"[{token.number}]";

        public readonly TokenIntegerLiteral token;
        public TokenIndexerInteger(TokenIntegerLiteral token, int lineNumber) : base(token, lineNumber)
        {
            this.token = token;
        }
        public override Token GetPrimaryToken() => token;
        public override bool ActuallyIndexes() => true;
        
        internal Exception GetIndexOutOfBoundsException(int min, int max, Statement thrower) =>
            new StatementException(thrower, $"Index {token.number} was out of bounds. Min: {min}, Max: {max}");
    }
    /// <summary>
    /// An indexer giving a string.
    /// </summary>
    public sealed class TokenIndexerString : TokenIndexer
    {
        public override string AsString() => $"[\"{token.text}\"]";

        public readonly TokenStringLiteral token;
        public TokenIndexerString(TokenStringLiteral token, int lineNumber) : base(token, lineNumber)
        {
            this.token = token;
        }
        public override Token GetPrimaryToken() => token;
        public override bool ActuallyIndexes() => true;
    }
    /// <summary>
    /// An indexer indicating a range value.
    /// </summary>
    public sealed class TokenIndexerRange : TokenIndexer
    {
        public override string AsString() => $"[\"{token.range.ToString()}\"]";

        public readonly TokenRangeLiteral token;
        public TokenIndexerRange(TokenRangeLiteral token, int lineNumber) : base(token, lineNumber)
        {
            this.token = token;
        }
        public override Token GetPrimaryToken() => token;
        public override bool ActuallyIndexes() => true;

        internal Exception GetIndexOutOfBoundsException(int min, int max, Statement thrower) =>
            new StatementException(thrower, $"Range {token.range.ToString()} was out of bounds. Min: {min}, Max: {max}");
    }
    /// <summary>
    /// An indexer indicating a set of block states.
    /// </summary>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public sealed class TokenIndexerBlockStates : TokenIndexer
    {
        public override string AsString() => $"[{string.Join(", ", blockStates.Select(bs => bs.AsString()))}]";

        public readonly List<TokenBlockStateLiteral> blockStates;
        public TokenIndexerBlockStates(IEnumerable<TokenBlockStateLiteral> addStates, int lineNumber) : base(addStates, lineNumber)
        {
            blockStates = new List<TokenBlockStateLiteral>();
            blockStates.AddRange(addStates);
        }
        public override Token GetPrimaryToken() => null;
        public override bool ActuallyIndexes() => false;
    }
    /// <summary>
    /// An indexer giving a selector.
    /// </summary>
    public sealed class TokenIndexerSelector : TokenIndexer
    {
        public override string AsString() => $"[{token.selector}]";

        public readonly TokenSelectorLiteral token;
        public TokenIndexerSelector(TokenSelectorLiteral token, int lineNumber) : base(token, lineNumber)
        {
            this.token = token;
        }
        public override Token GetPrimaryToken() => token;
        public override bool ActuallyIndexes() => true;
    }
    /// <summary>
    /// An indexer for a single asterisk character (*).
    /// </summary>
    public sealed class TokenIndexerAsterisk : TokenIndexer
    {
        public override string AsString() => "*";
        public TokenIndexerAsterisk(int lineNumber) : base(new TokenMultiply(lineNumber), lineNumber) { }
        
        public override Token GetPrimaryToken() => new TokenMultiply(lineNumber);
        public override bool ActuallyIndexes() => true;
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
    /// Represents an opening bracket which groups tokens, extends TokenOpenBracket.
    /// </summary>
    public abstract class TokenOpenGroupingBracket : TokenOpenBracket
    {
        public abstract bool IsAssociated(TokenBracket bracket);
        
        public bool hasBeenSquashed = false; // used to prevent function squashing from recursively running forever
        public override string AsString() => "<? grouping bracket open>";
        public TokenOpenGroupingBracket(int lineNumber) : base(lineNumber) { }
    }
    /// <summary>
    /// Represents a closing bracket which groups tokens, extends TokenCloseBracket.
    /// </summary>
    public abstract class TokenCloseGroupingBracket : TokenCloseBracket
    {
        public override string AsString() => "<? grouping bracket close>";
        public TokenCloseGroupingBracket(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// Represents an arithmetic operator token.
    /// </summary>
    public abstract class TokenArithmetic : TokenOperator
    {
        public enum Type
        {
            ADD, SUBTRACT, MULTIPLY, DIVIDE, MODULO, SWAP
        }

        public override string AsString() => "<? arithmatic>";
        public TokenArithmetic(int lineNumber) : base(lineNumber) { }

        public abstract Type GetArithmeticType();
    }
    public abstract class TokenArithmeticFirst : TokenArithmetic
    {
        public override string AsString() => "<? arithmatic first>";
        public TokenArithmeticFirst(int lineNumber) : base(lineNumber) { }

    }
    public abstract class TokenArithmeticSecond : TokenArithmetic
    {
        public override string AsString() => "<? arithmatic second>";
        public TokenArithmeticSecond(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// Extensions for <see cref="TokenCompare.Type"/>.
    /// </summary>
    public static class TokenCompareTypeExtensions
    {
        public static Range AsRange(this TokenCompare.Type type, int comparingTo)
        {
            switch (type)
            {
                case TokenCompare.Type.EQUAL:
                    return new Range(comparingTo, false);
                case TokenCompare.Type.NOT_EQUAL:
                    return new Range(comparingTo, true);
                case TokenCompare.Type.LESS:
                    return new Range(null, comparingTo - 1);
                case TokenCompare.Type.LESS_OR_EQUAL:
                    return new Range(null, comparingTo);
                case TokenCompare.Type.GREATER:
                    return new Range(comparingTo + 1, null);
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    return new Range(comparingTo, null);
                default:
                    return new Range();
            }
        }
    }
    /// <summary>
    /// Represents a generic comparison operator.
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
        /// <summary>
        /// Returns the minecraft operator for the given TokenCompare.Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetMinecraftOperator(Type type)
        {
            switch (type)
            {
                case Type.EQUAL:
                    return "=";
                case Type.NOT_EQUAL:
                    return "DOESNT EXIST MOJANG";
                case Type.LESS:
                    return "<";
                case Type.LESS_OR_EQUAL:
                    return "<=";
                case Type.GREATER:
                    return ">";
                case Type.GREATER_OR_EQUAL:
                    return ">=";
                default:
                    return "??";
            }
        }

        public override string AsString() => "<? compare>";

        internal TokenCompare() : base(-1) { }
        public TokenCompare(int lineNumber) : base(lineNumber) { }

        public abstract Type GetCompareType();

        public string GetDocumentation() => "Any comparison operator. Allowed values are: <, >, <=, >=, ==, !=";
    }

    /// <summary>
    /// Used to indicate that this operator assigns the identifier to the left of it.
    /// </summary>
    public interface IAssignment { }
    /// <summary>
    /// Used to indicate when a token should terminate the Assembler's token collector and start a new line.
    /// </summary>
    public interface ITerminating { }
    /// <summary>
    /// Used to indicate when a token holds no useful information for the compiler e.g., a comment.
    /// </summary>
    public interface IUselessInformation { }

    public sealed class TokenOpenParenthesis : TokenOpenGroupingBracket
    {
        public override string AsString() => "(";
        public override bool IsAssociated(TokenBracket bracket)
        {
            return bracket is TokenOpenParenthesis || bracket is TokenCloseParenthesis;
        }

        public TokenOpenParenthesis(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenCloseParenthesis : TokenCloseGroupingBracket
    {
        public override string AsString() => ")";
        public TokenCloseParenthesis(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenOpenIndexer : TokenOpenGroupingBracket
    {
        public override string AsString() => "[";
        public override bool IsAssociated(TokenBracket bracket)
        {
            return bracket is TokenOpenIndexer || bracket is TokenCloseIndexer;
        }
        public TokenOpenIndexer(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenCloseIndexer : TokenCloseGroupingBracket
    {
        public override string AsString() => "]";
        public TokenCloseIndexer(int lineNumber) : base(lineNumber) { }
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
    /// <summary>
    /// The inverter signaling to invert a range argument.
    /// </summary>
    public sealed class TokenRangeInvert : Token
    {
        public override string AsString() => "!";
        public TokenRangeInvert(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// An AND/OR identifier. Continues a selector transformation.
    /// </summary>
    public abstract class TokenContinueCompareChain : TokenOperator
    {
        public TokenContinueCompareChain(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenAnd : TokenContinueCompareChain
    {
        public override string AsString() => "and";
        public TokenAnd(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenOr : TokenContinueCompareChain
    {
        public override string AsString() => "or";
        public TokenOr(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenNot : TokenOperator
    {
        public override string AsString() => "not";
        public TokenNot(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenDeref : TokenOperator
    {
        public override string AsString() => "$";
        public TokenDeref(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenAssignment : TokenOperator, IAssignment
    {
        public override string AsString() => "=";
        public TokenAssignment(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenAdd : TokenArithmeticSecond
    {
        public override string AsString() => "+";
        public TokenAdd(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.ADD;
    }
    public sealed class TokenSubtract : TokenArithmeticSecond
    {
        public override string AsString() => "-";
        public TokenSubtract(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.SUBTRACT;
    }
    public sealed class TokenMultiply : TokenArithmeticFirst
    {
        public override string AsString() => "*";
        public TokenMultiply(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.MULTIPLY;
    }
    public sealed class TokenDivide : TokenArithmeticFirst
    {
        public override string AsString() => "/";
        public TokenDivide(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.DIVIDE;
    }
    public sealed class TokenModulo : TokenArithmeticFirst
    {
        public override string AsString() => "%";
        public TokenModulo(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.MODULO;
    }
    public sealed class TokenAddAssignment : TokenArithmeticSecond, IAssignment
    {
        public override string AsString() => "+=";
        public TokenAddAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.ADD;
    }
    public sealed class TokenSubtractAssignment : TokenArithmeticSecond, IAssignment
    {
        public override string AsString() => "-=";
        public TokenSubtractAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.SUBTRACT;
    }
    public sealed class TokenMultiplyAssignment : TokenArithmeticFirst, IAssignment
    {
        public override string AsString() => "*=";
        public TokenMultiplyAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.MULTIPLY;
    }
    public sealed class TokenDivideAssignment : TokenArithmeticFirst, IAssignment
    {
        public override string AsString() => "/=";
        public TokenDivideAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.DIVIDE;
    }
    public sealed class TokenModuloAssignment : TokenArithmeticFirst, IAssignment
    {
        public override string AsString() => "%=";
        public TokenModuloAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.MODULO;
    }
    public sealed class TokenSwapAssignment : TokenArithmeticFirst, IAssignment
    {
        public override string AsString() => "><";
        public TokenSwapAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmeticType() => Type.SWAP;
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

        public override Type GetCompareType() => Type.LESS;

    }
    public sealed class TokenGreaterThan : TokenCompare
    {
        public override string AsString() => ">";
        public TokenGreaterThan(int lineNumber) : base(lineNumber) { }

        public override Type GetCompareType() => Type.GREATER;
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