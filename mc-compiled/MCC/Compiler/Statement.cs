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

        public Token NextToken() => tokens[currentToken++];
        public Token PeekToken() => tokens[currentToken];
        public bool PeekIs<T>() => tokens[currentToken] is T;

        protected abstract TypePattern[] GetValidPatterns();
        /// <summary>
        /// Run this statement/continue where it left off.
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// Run this statement from square one.
        /// </summary>
        public void Run0()
        {
            currentToken = 0;
            Run();
        }
        public bool IsValid
        {
            get
            {
                return patterns.All(tp => tp.Check(tokens));
            }
        }
    }
}