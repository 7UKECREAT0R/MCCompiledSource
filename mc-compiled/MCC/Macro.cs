using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// A predefined set of tokens that is intended to be repeated or duplicated.
    /// </summary>
    public class Macro
    {
        public readonly string name;
        public readonly string[] args;
        public readonly Token[] execute;

        public Macro(string name, string[] args, Token[] execute)
        {
            this.name = name;
            this.args = args;
            this.execute = execute;
        }
    }
}
