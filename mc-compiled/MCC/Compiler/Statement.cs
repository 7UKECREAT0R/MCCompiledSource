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
    public abstract class Statement
    {
        private readonly TypePattern[] patterns;
        public Statement(Token[] tokens)
        {
            this.tokens = tokens;
            patterns = GetValidPatterns();
        }

        Token[] tokens;
        int currentToken;

        public bool HasNext
        {
            get => currentToken < tokens.Length;
        }
        public Token NextToken() => tokens[currentToken++];
        public Token PeekToken() => tokens[currentToken];
        public T NextToken<T>() where T: class => tokens[currentToken++] as T;
        public T PeekToken<T>() where T: class => tokens[currentToken] as T;
        public bool NextIs<T>() => tokens[currentToken] is T;

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