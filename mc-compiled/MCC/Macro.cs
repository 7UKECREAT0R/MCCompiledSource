using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// A macro definition that can be called.
    /// </summary>
    public struct Macro
    {
        public readonly string name;
        public readonly string[] argNames;
        public readonly Statement[] statements;

        public Macro(string name, string[] argNames, Statement[] statements)
        {
            this.name = name;
            this.argNames = argNames;
            this.statements = statements;
        }
        /// <summary>
        /// Fuzzy-match this macro's name.
        /// </summary>
        /// <param name="otherName"></param>
        /// <returns></returns>
        public bool Matches(string otherName)
        {
            return name.ToUpper().Trim().Equals
                (otherName.ToUpper().Trim());
        }
    }
}
