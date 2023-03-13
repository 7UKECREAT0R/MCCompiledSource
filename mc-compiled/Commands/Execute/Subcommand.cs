using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Execute
{
    public abstract class Subcommand
    {
        /// <summary>
        /// The pattern(s) that must be matched to complete this subcommand. Return null to disable pattern matching.
        /// </summary>
        public abstract TypePattern[] Pattern { get; }
        /// <summary>
        /// The keyword used to invoke this subcommand.
        /// </summary>
        public abstract string Keyword { get; }

        /// <summary>
        /// Returns if this subcommand terminates the execute chain.
        /// </summary>
        public abstract bool TerminatesChain { get; }

        /// <summary>
        /// Attempt to load this subcommand's properties from a set of input tokens.
        /// </summary>
        /// <param name="feeder"></param>
        public abstract void FromTokens(Statement tokens);
        /// <summary>
        /// Returns this subcommand as actual command output.
        /// </summary>
        /// <returns></returns>
        public abstract string ToMinecraft();
    }
}
