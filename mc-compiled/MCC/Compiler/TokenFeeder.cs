using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A class which allows the "feeding" of tokens and quick checking/casting of their types.
    /// </summary>
    public class TokenFeeder
    {
        public Executor executor;
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
        /// Set the line of source this feeder relates to. Used in "errors."
        /// </summary>
        /// <param name="lines">An array of integers representing the line numbers associated with the line.</param>
        /// <param name="code">A string representing the source code</param>
        public void SetSource(int[] lines, string code)
        {
            this.Lines = lines;
            this.Source = code;
        }

        public int[] Lines
        {
            get; private set;
        }
        public bool DecorateInSource
        {
            get; protected set;
        }
        public string Source
        {
            get; private set;
        }

        /// <summary>
        /// Returns if this feeder has another available token.
        /// </summary>
        public bool HasNext
        {
            get => this.currentToken < this.tokens.Length;
        }
        /// <summary>
        /// Pulls the next token in the feeder.
        /// </summary>
        /// <exception cref="FeederException"></exception>
        public Token Next()
        {
            if (this.currentToken >= this.tokens.Length)
                throw new FeederException(this, $"Expected token at end of line.");
            return this.tokens[this.currentToken++];
        }
        /// <summary>
        /// Peeks at the next token in the feeder, but doesn't pull it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FeederException"></exception>
        protected Token Peek()
        {
            if (this.currentToken >= this.tokens.Length)
                throw new FeederException(this, $"Token expected at end of line.");
            return this.tokens[this.currentToken];
        }
        /// <summary>
        /// Pulls the next token in the feeder, casting it to the given type. Implements MCCompiled implicit conversions.
        /// </summary>
        /// <typeparam name="T">The type to cast.</typeparam>
        /// <param name="parameterHint">
        /// The name of the parameter that this token will fill. Errors will display this name as a hint to the user.
        /// You may pass null to this parameter if you have checked it beforehand via <see cref="NextIs{T}"/>.
        /// </param>
        /// <returns></returns>
        /// <exception cref="FeederException"></exception>
        public T Next<T>(string parameterHint) where T : class
        {
            if (this.currentToken >= this.tokens.Length)
                throw new FeederException(this, $"Expected parameter '{parameterHint}' at end of line, type {typeof(T).Name}");

            Token token = this.tokens[this.currentToken++];
            
            if (token is T castedToken)
                return castedToken;
            if (!(token is IImplicitToken implicitToken))
                throw new FeederException(this, $"Invalid token type for parameter '{parameterHint}'. Expected {typeof(T).Name} but got {token.GetType().Name}");

            Type[] otherTypes = implicitToken.GetImplicitTypes();

            for (int i = 0; i < otherTypes.Length; i++)
                if (typeof(T).IsAssignableFrom(otherTypes[i]))
                    return implicitToken.Convert(this.executor, i) as T;
                
            throw new FeederException(this, $"Invalid token type for parameter '{parameterHint}'. Expected {typeof(T).Name} but got {implicitToken.GetType().Name}");
        }
        /// <summary>
        /// Peeks at the next token in the feeder, casting it to the given type. Implements MCCompiled implicit conversions.
        /// </summary>
        /// <typeparam name="T">The type to cast.</typeparam>
        /// <returns></returns>
        /// <exception cref="FeederException"></exception>
        public T Peek<T>() where T : class
        {
            if (this.currentToken >= this.tokens.Length)
                throw new FeederException(this, $"Token expected at end of line, type {typeof(T).Name}");
            Token token = this.tokens[this.currentToken];
            
            if (token is T validToken)
                return validToken;
            
            if (!(token is IImplicitToken implicitToken))
                throw new FeederException(this, $"Invalid token type. Expected {typeof(T).Name} but got {token.GetType()}");
                
            Type[] otherTypes = implicitToken.GetImplicitTypes();

            for (int i = 0; i < otherTypes.Length; i++)
                if (typeof(T).IsAssignableFrom(otherTypes[i]))
                    return implicitToken.Convert(this.executor, i) as T;
            throw new FeederException(this, $"Invalid token type. Expected {typeof(T).Name} but got {implicitToken.GetType()}");
        }
        /// <summary>
        /// Returns if the next parameter (if any) is able to be casted to a certain type. Implements MCCompiled implicit conversions.
        /// </summary>
        /// <typeparam name="T">The type to check for. If <paramref name="enforceType"/> is true, the token (if any) must match the type or it will throw an error.</typeparam>
        /// <param name="enforceType">If true and there is a token at the end of the statement, an exception will be thrown if the type doesn't match.</param>
        /// <param name="allowImplicit">Allow implicit conversion of tokens.</param>
        /// <returns></returns>
        [AssertionMethod]
        public bool NextIs<T>(bool enforceType, bool allowImplicit = true)
        {
            if (!this.HasNext)
                return false;

            Token token = this.tokens[this.currentToken];

            if (token is T)
                return true;

            if (!allowImplicit || !(token is IImplicitToken implicitToken))
            {
                TryEnforceType();
                return false;
            }

            Type[] otherTypes = implicitToken.GetImplicitTypes();
            bool canDoImplicitConversion = otherTypes.Any(t => typeof(T).IsAssignableFrom(t));

            if (!canDoImplicitConversion)
                TryEnforceType();
            
            return canDoImplicitConversion;

            // throws if enforceType is enabled.
            void TryEnforceType()
            {
                if(enforceType)
                    throw new FeederException(this, $"Parameter here must be '{typeof(T).Name}', but got '{token.GetType().Name}'.");
            }
        }

        /// <summary>
        /// Return the remaining tokens in this feeder, excluding comments. Does not actually modify the reader's location.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Token> GetRemainingTokens()
        {
            if (this.tokens == null)
                return Array.Empty<Token>();
            if (this.tokens.Length <= this.currentToken)
                return Array.Empty<Token>();

            return this.tokens.Skip(this.currentToken)
                .Where(t => !(t is IUselessInformation));
        }

        /// <summary>
        /// Returns the number of remaining tokens in this feeder, excluding comments.
        /// </summary>
        public int RemainingTokens
        {
            get
            {
                if (this.tokens == null)
                    return 0;
                return this.tokens.Skip(this.currentToken)
                    .Count(t => !(t is IUselessInformation));
            }
        }
    }
}
