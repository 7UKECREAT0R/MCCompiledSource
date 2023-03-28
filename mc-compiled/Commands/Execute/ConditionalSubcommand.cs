using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Execute
{
    /// <summary>
    /// Represents a conditional subcommand that falls under the "if" and "unless" execute subcommands.
    /// </summary>
    internal abstract class ConditionalSubcommand : Subcommand
    {
        public override bool TerminatesChain => false;

        public static readonly ConditionalSubcommand[] CONDITIONAL_EXAMPLES =
        {
           new ConditionalSubcommandBlock(),
           new ConditionalSubcommandBlocks(),
           new ConditionalSubcommandEntity(),
           new ConditionalSubcommandScore()
        };

        /// <summary>
        /// Returns a new instance of the subcommand tied to the given keyword. Case insensitive.
        /// </summary>
        /// <param name="keyword">The case-insensitive keyword to use.param>
        /// <param name="forExceptions">The calling statement, only used for exceptions.</param>
        /// <returns></returns>
        public static ConditionalSubcommand GetConditionalSubcommandForKeyword(string keyword, Statement forExceptions)
        {
            switch (keyword.ToUpper())
            {
                case "SCORE":
                    return new ConditionalSubcommandScore();
                case "ENTITY":
                    return new ConditionalSubcommandEntity();
                case "BLOCKS":
                    return new ConditionalSubcommandBlocks();
                case "BLOCK":
                    return new ConditionalSubcommandBlock();
                default:
                    throw new StatementException(forExceptions, $"Conditional subcommand '{keyword}' does not exist. Valid options include: " +
                        string.Join(", ", CONDITIONAL_EXAMPLES.Select(c => c.Keyword.ToLower())));
            }
        }
    }
}
