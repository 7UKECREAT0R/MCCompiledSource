using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A class which allows the "feeding" of tokens and quick checking/casting of their types.
    /// </summary>
    public class TokenFeeder
    {
        protected Executor executor;
        protected Token[] tokens;
        protected int currentToken;

        public TokenFeeder(Token[] tokens)
        {
            this.tokens = tokens;
            this.currentToken = 0;
        }
        /// <summary>
        /// Set the executor of this TokenFeeder.
        /// </summary>
        /// <param name="executor"></param>
        public void SetExecutor(Executor executor)
        {
            this.executor = executor;
        }

        /// <summary>
        /// Returns if this feeder has another available token.
        /// </summary>
        public bool HasNext
        {
            get => currentToken < tokens.Length;
        }
        /// <summary>
        /// Pulls the next token in the feeder.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FeederException"></exception>
        public Token Next()
        {
            if (currentToken >= tokens.Length)
                throw new FeederException(this, $"Token expected at end of line.");
            return tokens[currentToken++];
        }
        /// <summary>
        /// Peeks at the next token in the feeder, but doesn't pull it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FeederException"></exception>
        public Token Peek()
        {
            if (currentToken >= tokens.Length)
                throw new FeederException(this, $"Token expected at end of line.");
            return tokens[currentToken];
        }
        /// <summary>
        /// Pulls the next token in the feeder, casting it to the given type. Implements MCCompiled implicit conversions.
        /// </summary>
        /// <typeparam name="T">The type to cast.</typeparam>
        /// <returns></returns>
        /// <exception cref="FeederException"></exception>
        public T Next<T>() where T : class
        {
            if (currentToken >= tokens.Length)
                throw new FeederException(this, $"Token expected at end of line, type {typeof(T).Name}");

            Token token = tokens[currentToken++];
            if (!(token is T))
            {
                if (token is IImplicitToken)
                {
                    IImplicitToken implicitToken = token as IImplicitToken;
                    Type[] otherTypes = implicitToken.GetImplicitTypes();

                    for (int i = 0; i < otherTypes.Length; i++)
                        if (typeof(T).IsAssignableFrom(otherTypes[i]))
                            return implicitToken.Convert(executor, i) as T;
                }
                throw new FeederException(this, $"Invalid token type. Expected {typeof(T).Name} but got {token.GetType().Name}");
            }
            else
                return token as T;
        }
        /// <summary>
        /// Peeks at the next token in the feeder, casting it to the given type. Implements MCCompiled implicit conversions.
        /// </summary>
        /// <typeparam name="T">The type to cast.</typeparam>
        /// <returns></returns>
        /// <exception cref="FeederException"></exception>
        public T Peek<T>() where T : class
        {
            if (currentToken >= tokens.Length)
                throw new FeederException(this, $"Token expected at end of line, type {typeof(T).Name}");
            Token token = tokens[currentToken];
            if (!(token is T))
            {
                if (token is IImplicitToken)
                {
                    IImplicitToken implicitToken = token as IImplicitToken;
                    Type[] otherTypes = implicitToken.GetImplicitTypes();

                    for (int i = 0; i < otherTypes.Length; i++)
                        if (typeof(T).IsAssignableFrom(otherTypes[i]))
                            return implicitToken.Convert(executor, i) as T;
                }
                throw new FeederException(this, $"Invalid token type. Expected {typeof(T)} but got {token.GetType()}");
            }
            else
                return token as T;
        }
        /// <summary>
        /// Returns if the next parameter is able to be casted to a certain type. Implements MCCompiled implicit conversions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allowImplicit"></param>
        /// <returns></returns>
        public bool NextIs<T>(bool allowImplicit = true)
        {
            if (!HasNext)
                return false;

            Token token = tokens[currentToken];

            if (token is T)
                return true;

            if (allowImplicit)
            {
                if (token is IImplicitToken)
                {
                    IImplicitToken implicitToken = token as IImplicitToken;
                    Type[] otherTypes = implicitToken.GetImplicitTypes();

                    for (int i = 0; i < otherTypes.Length; i++)
                        if (typeof(T).IsAssignableFrom(otherTypes[i]))
                            return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Return the remaining tokens in this feeder. Does not actually modify the reader's location.
        /// </summary>
        /// <returns></returns>
        public Token[] GetRemainingTokens()
        {
            Token[] ret = new Token[tokens.Length - currentToken];
            for (int i = currentToken; i < tokens.Length; i++)
                ret[i] = tokens[i];
            return ret;
        }
    }
}
