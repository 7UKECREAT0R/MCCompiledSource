using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public override string AsString() => $"[]";
        public TokenIndexer(int lineNumber) : base(lineNumber) { }

        /// <summary>
        /// Get the token inside this indexer.
        /// </summary>
        /// <returns></returns>
        public abstract Token GetIndexerToken();

        /// <summary>
        /// Creates an indexer based on the type of token given.
        /// Throws a exception if there's no valid indexer for the given token.
        /// </summary>
        /// <param name="token">The token to wrap in an indexer.</param>
        /// <param name="forExceptions">Statement that would be considered the one throwing this exception. If null, will throw a tokenizer exception.</param>
        /// <returns></returns>
        public static TokenIndexer CreateIndexer(Token token, Statement forExceptions = null)
        {
            if(token is TokenLiteral literal)
                return CreateIndexer(literal, forExceptions);

            if (token is TokenUnresolvedPPV unresolvedPPV)
                return new TokenIndexerUnresolvedPPV(unresolvedPPV, token.lineNumber);

            // throw exception
            if(forExceptions == null)
                throw new TokenizerException($"Cannot index/scope with a token: " + token.DebugString(), token.lineNumber);
            else
                throw new StatementException(forExceptions, $"Cannot index/scope with a token: " + token.DebugString());
        }
        /// <summary>
        /// Creates an indexer based on the type of literal given.
        /// Throws an exception if there's no valid indexer for the given literal.
        /// </summary>
        /// <param name="literal">The literal to wrap in an indexer.</param>
        /// <param name="forExceptions">Statement that would be considered the one throwing this exception. If null, will throw a tokenizer exception.</param>
        /// <returns></returns>
        public static TokenIndexer CreateIndexer(TokenLiteral literal, Statement forExceptions = null)
        {
            int lineNumber = literal.lineNumber;

            if (literal is TokenIntegerLiteral intLiteral)
                return new TokenIndexerInteger(intLiteral, lineNumber);
            else if (literal is TokenStringLiteral stringLiteral)
                return new TokenIndexerString(stringLiteral, lineNumber);
            else if (literal is TokenSelectorLiteral selectorLiteral)
                return new TokenIndexerSelector(selectorLiteral, lineNumber);

            // throw exception
            if (forExceptions == null)
                throw new TokenizerException($"Cannot index/scope with a token: " + literal.DebugString(), literal.lineNumber);
            else
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
    /// An indexer using an unresolved PPV.
    /// </summary>
    public sealed class TokenIndexerUnresolvedPPV : TokenIndexer
    {
        public TokenUnresolvedPPV token;
        public TokenIndexerUnresolvedPPV(TokenUnresolvedPPV token, int lineNumber) : base(lineNumber)
        {
            this.token = token;
        }
        public override Token GetIndexerToken() => token;

        /// <summary>
        /// Resolve the PPV inside this indexer and remap to the right indexer.
        /// </summary>
        /// <returns>The indexer wrapping the newly resolved PPV.</returns>
        /// <exception cref="StatementException" />
        public TokenIndexer Resolve(Executor executor, Statement runningStatement)
        {
            // resolve the contained PPV.
            TokenLiteral[] resolvedValues = executor.ResolvePPV(token, runningStatement);

            // ResolvePPV returns null/empty if it can't resolve.
            if (resolvedValues == null || resolvedValues.Length < 1)
                throw new StatementException(runningStatement, $"Preprocessor variable '{token.word}' either doesn't exist or didn't have a valid value.");

            // only need the first value, since indexers can only hold one.
            TokenLiteral _value = resolvedValues[0];

            // return one of the primary allowed indexer types.
            return TokenIndexer.CreateIndexer(_value, runningStatement);
        }
    }
    /// <summary>
    /// An indexer giving an integer. Defaulted to this class with the value 0 when [] is given to the tokenizer.
    /// </summary>
    public sealed class TokenIndexerInteger : TokenIndexer
    {
        public TokenIntegerLiteral token;
        public TokenIndexerInteger(TokenIntegerLiteral token, int lineNumber) : base(lineNumber)
        {
            this.token = token;
        }
        public override Token GetIndexerToken() => token;

        internal Exception GetIndexOutOfBounds(int min, int max, Statement thrower) =>
            new StatementException(thrower, $"Index {token.number} was out of bounds. Min: {min}, Max: {max}");
    }
    /// <summary>
    /// An indexer giving an integer.
    /// </summary>
    public sealed class TokenIndexerString : TokenIndexer
    {
        public TokenStringLiteral token;
        public TokenIndexerString(TokenStringLiteral token, int lineNumber) : base(lineNumber)
        {
            this.token = token;
        }
        public override Token GetIndexerToken() => token;
    }
    /// <summary>
    /// An indexer giving a selector.
    /// </summary>
    public sealed class TokenIndexerSelector : TokenIndexer
    {
        public TokenSelectorLiteral token;
        public TokenIndexerSelector(TokenSelectorLiteral token, int lineNumber) : base(lineNumber)
        {
            this.token = token;
        }
        public override Token GetIndexerToken() => token;
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
    public abstract class TokenArithmatic : TokenOperator
    {
        public enum Type
        {
            ADD, SUBTRACT, MULTIPLY, DIVIDE, MODULO, SWAP
        }

        public override string AsString() => "<? arithmatic>";
        public TokenArithmatic(int lineNumber) : base(lineNumber) { }

        public abstract Type GetArithmaticType();
    }
    public abstract class TokenArithmaticFirst : TokenArithmatic
    {
        public override string AsString() => "<? arithmatic first>";
        public TokenArithmaticFirst(int lineNumber) : base(lineNumber) { }

    }
    public abstract class TokenArithmaticSecond : TokenArithmatic
    {
        public override string AsString() => "<? arithmatic second>";
        public TokenArithmaticSecond(int lineNumber) : base(lineNumber) { }
    }

    /// <summary>
    /// Represents a generic comparison operator.
    /// </summary>
    public abstract class TokenCompare : TokenOperator
    {
        public enum Type
        {
            EQUAL,
            NOT_EQUAL,
            LESS_THAN,
            LESS_OR_EQUAL,
            GREATER_THAN,
            GREATER_OR_EQUAL
        }

        public override string AsString() => "<? compare>";
        public TokenCompare(int lineNumber) : base(lineNumber) { }

        public abstract Type GetCompareType();
    }

    /// <summary>
    /// Used to indicate that this operator assigns the identifier to the left of it.
    /// </summary>
    public interface IAssignment { }
    /// <summary>
    /// Used to indicate when a token should terminate the Assembler's token collector and start a new line.
    /// </summary>
    public interface ITerminating { }

    public sealed class TokenOpenParenthesis : TokenOpenBracket
    {
        public bool hasBeenSquashed = false; // used to prevent function squashing from recursing infinitely
        public override string AsString() => "(";
        public TokenOpenParenthesis(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenCloseParenthesis : TokenOpenBracket
    {
        public override string AsString() => ")";
        public TokenCloseParenthesis(int lineNumber) : base(lineNumber) { }
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
    public abstract class TokenContinue : TokenOperator
    {
        public TokenContinue(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenAnd : TokenContinue
    {
        public override string AsString() => "and";
        public TokenAnd(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenOr : TokenContinue
    {
        public override string AsString() => "or";
        public TokenOr(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenNot : TokenOperator
    {
        public override string AsString() => "not";
        public TokenNot(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenAssignment : TokenOperator, IAssignment
    {
        public override string AsString() => "=";
        public TokenAssignment(int lineNumber) : base(lineNumber) { }
    }
    public sealed class TokenAdd : TokenArithmaticSecond
    {
        public override string AsString() => "+";
        public TokenAdd(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.ADD;
    }
    public sealed class TokenSubtract : TokenArithmaticSecond
    {
        public override string AsString() => "-";
        public TokenSubtract(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.SUBTRACT;
    }
    public sealed class TokenMultiply : TokenArithmaticFirst
    {
        public override string AsString() => "*";
        public TokenMultiply(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.MULTIPLY;
    }
    public sealed class TokenDivide : TokenArithmaticFirst
    {
        public override string AsString() => "/";
        public TokenDivide(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.DIVIDE;
    }
    public sealed class TokenModulo : TokenArithmaticFirst
    {
        public override string AsString() => "%";
        public TokenModulo(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.MODULO;
    }
    public sealed class TokenAddAssignment : TokenArithmaticSecond, IAssignment
    {
        public override string AsString() => "+=";
        public TokenAddAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.ADD;
    }
    public sealed class TokenSubtractAssignment : TokenArithmaticSecond, IAssignment
    {
        public override string AsString() => "-=";
        public TokenSubtractAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.SUBTRACT;
    }
    public sealed class TokenMultiplyAssignment : TokenArithmaticFirst, IAssignment
    {
        public override string AsString() => "*=";
        public TokenMultiplyAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.MULTIPLY;
    }
    public sealed class TokenDivideAssignment : TokenArithmaticFirst, IAssignment
    {
        public override string AsString() => "/=";
        public TokenDivideAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.DIVIDE;
    }
    public sealed class TokenModuloAssignment : TokenArithmaticFirst, IAssignment
    {
        public override string AsString() => "%=";
        public TokenModuloAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.MODULO;
    }
    public sealed class TokenSwapAssignment : TokenArithmaticFirst, IAssignment
    {
        public override string AsString() => "><";
        public TokenSwapAssignment(int lineNumber) : base(lineNumber) { }

        public override Type GetArithmaticType() => Type.SWAP;
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

        public override Type GetCompareType() => Type.LESS_THAN;
    }
    public sealed class TokenGreaterThan : TokenCompare
    {
        public override string AsString() => ">";
        public TokenGreaterThan(int lineNumber) : base(lineNumber) { }

        public override Type GetCompareType() => Type.GREATER_THAN;
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