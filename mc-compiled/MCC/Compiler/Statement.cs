using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A fully qualified statement which can be run.
    /// </summary>
    public abstract class Statement : ICloneable
    {
        private readonly TypePattern[] patterns;
        public Statement(Token[] tokens)
        {
            this.tokens = tokens;
            patterns = GetValidPatterns();
        }

        protected Token[] tokens;
        protected int currentToken;

        public bool HasNext
        {
            get => currentToken < tokens.Length;
        }
        public Token Next()
        {
            return tokens[currentToken++];
        }
        public Token Peek()
        {
            return tokens[currentToken];
        }
        public T Next<T>() where T : class
        {
            Token token = tokens[currentToken++];
            if (!(token is T))
            {
                if (token is IImplicitToken)
                {
                    IImplicitToken implicitToken = token as IImplicitToken;
                    Type otherType = implicitToken.GetImplicitType();

                    if (token.GetType().IsAssignableFrom(otherType))
                        return implicitToken.Convert() as T;
                }
                throw new StatementException(this, $"Invalid token type. Expected {typeof(T)} but got {token.GetType()}");
            }
            else
                return token as T;
        }
        public T Peek<T>() where T : class
        {
            Token token = tokens[currentToken];
            if(!(token is T))
            {
                if (token is IImplicitToken)
                {
                    IImplicitToken implicitToken = token as IImplicitToken;
                    Type otherType = implicitToken.GetImplicitType();

                    if (token.GetType().IsAssignableFrom(otherType))
                        return implicitToken.Convert() as T;
                }
                throw new StatementException(this, $"Invalid token type. Expected {typeof(T)} but got {token.GetType()}");
            } else
                return token as T;
        }
        public bool NextIs<T>()
        {
            if (!HasNext)
                return false;
            return tokens[currentToken] is T;
        }

        /// <summary>
        /// Return the remaining tokens in this statement.
        /// </summary>
        /// <returns></returns>
        public Token[] GetRemainingTokens()
        {
            Token[] ret = new Token[tokens.Length - currentToken];
            for (int i = currentToken; i < tokens.Length; i++)
                ret[i] = tokens[i];
            return ret;
        }
        protected abstract TypePattern[] GetValidPatterns();
        /// <summary>
        /// Run this statement/continue where it left off.
        /// </summary>
        protected abstract void Run(Executor executor);

        /// <summary>
        /// Run this statement from square one.
        /// </summary>
        public void Run0(Executor executor)
        {
            currentToken = 0;
            Run(executor);
        }
        /// <summary>
        /// Clone this statement and resolve its unidentified tokens based off the current executor's state.
        /// After this is finished, squash and process any intermediate math or functional operations.
        /// </summary>
        /// <returns>A shallow clone of this Statement which has its tokens resolved.</returns>
        public Statement CloneResolve(Executor executor)
        {
            Statement statement = MemberwiseClone() as Statement;
            int length = statement.tokens.Length;
            Token[] allUnresolved = statement.tokens;
            Token[] allResolved = new Token[length];

            for(int i = 0; i < length; i++)
            {
                Token unresolved = allUnresolved[i];
                Token resolved = unresolved;
                int line = unresolved.lineNumber;

                if (unresolved is TokenStringLiteral)
                    resolved = new TokenStringLiteral(executor.ResolveString(unresolved as TokenStringLiteral), line);
                else if (unresolved is TokenUnresolvedPPV)
                    resolved = (executor.ResolvePPV(unresolved as TokenUnresolvedPPV) ?? unresolved);

                if(unresolved is TokenIdentifier)
                {
                    string word = (unresolved as TokenIdentifier).word;
                    if (executor.scoreboard.TryGetByAccessor(word, out ScoreboardValue value))
                        resolved = new TokenIdentifierValue(word, value, line);
                    else if (executor.scoreboard.TryGetStruct(word, out StructDefinition @struct))
                        resolved = new TokenIdentifierStruct(word, @struct, line);
                    else if (executor.TryLookupMacro(word, out Macro? macro))
                        resolved = new TokenIdentifierMacro(macro.Value, line);
                    else if (executor.TryLookupFunction(word, out Function function))
                        resolved = new TokenIdentifierFunction(function, line);
                }

                allResolved[i] = resolved;
            }

            // TODO squash intermediate operations
            List<Token> tokens = new List<Token>(allResolved);



            statement.tokens = tokens.ToArray();
            return statement;
        }
        public void Squash<T>(ref List<Token> tokens, Executor executor)
        {
            for (int i = 1; i < tokens.Count() - 1; i++)
            {
                Token selected = tokens[i];
                if (!(selected is T))
                    continue;

                Token _left = tokens[i - 1];
                Token _right = tokens[i + 1];

                bool leftIsLiteral = _left is TokenLiteral;
                bool rightIsLiteral = _right is TokenLiteral;
                bool leftIsValue = _left is TokenIdentifierValue;
                bool rightIsValue = _right is TokenIdentifierValue;

                if(leftIsLiteral & rightIsLiteral)
                {
                    TokenLiteral left = _left as TokenLiteral;
                    TokenLiteral right = _right as TokenLiteral;

                }
                else if(leftIsValue & rightIsValue)
                {
                    TokenIdentifierValue left = _left as TokenIdentifierValue;
                    TokenIdentifierValue right = _right as TokenIdentifierValue;
                    string leftAccessor = left.Accessor;
                    string rightAccessor = right.Accessor;
                    ScoreboardValue a = left.value;
                    ScoreboardValue b = right.value;

                }
                else
                {

                }
            }
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// If the tokens inside this statement match its pattern, if any.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (patterns.Length < 1)
                    return true;
                return patterns.All(tp => tp.Check(tokens));
            }
        }
    }

    /// <summary>
    /// Indicates something has blown up while executing a statement.
    /// </summary>
    public class StatementException : Exception
    {
        public readonly Statement statement;
        public StatementException(Statement statement, string message) : base(message)
        {
            this.statement = statement;
        }
    }
}