using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// The most basic processed unit in the file.
    /// Gives basic parsed information and generally assists in the assembly of a Statement
    /// </summary>
    public abstract class Token
    {
        public abstract string AsString();

        public int lineNumber;
        public Token(int lineNumber)
        {
            this.lineNumber = lineNumber;
        }
    }
    public struct TokenDefinition
    {
        public readonly Type type;
        public readonly string keyword;

        public TokenDefinition(Type type, string keyword)
        {
            this.type = type;
            this.keyword = keyword;
        }
    }
}
