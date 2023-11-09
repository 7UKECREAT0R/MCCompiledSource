using mc_compiled.MCC.Compiler;

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

        /// <summary>
        /// Returns a new instance of the subcommand tied to the given keyword. Case insensitive.
        /// </summary>
        /// <param name="keyword">The case-insensitive keyword to use.param>
        /// <param name="forExceptions">The calling statement, only used for exceptions.</param>
        /// <param name="throwForTerminators">If this should throw if a subcommand is given that terminates the chain.</param>
        /// <returns></returns>
        public static Subcommand GetSubcommandForKeyword(string keyword, Statement forExceptions)
        {
            switch (keyword.ToUpper())
            {
                case "ALIGN":
                    return new SubcommandAlign();
                case "ANCHORED":
                    return new SubcommandAnchored();
                case "AS":
                    return new SubcommandAs();
                case "AT":
                    return new SubcommandAt();
                case "FACING":
                    return new SubcommandFacing();
                case "IF":
                    return new SubcommandIf();
                case "IN":
                    return new SubcommandIn();
                case "POSITIONED":
                    return new SubcommandPositioned();
                case "ROTATED":
                    return new SubcommandRotated();
                case "RUN":
                    return new SubcommandRun();
                case "UNLESS":
                    return new SubcommandUnless();
                default:
                    throw new StatementException(forExceptions, $"Unknown execute subcommand '{keyword}'.");
            }
        }
    }
}
