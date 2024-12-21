using System.Linq;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Execute;

/// <summary>
///     Represents a conditional subcommand that falls under the "if" and "unless" execute subcommands.
/// </summary>
internal abstract class ConditionalSubcommand : Subcommand
{
    public static readonly ConditionalSubcommand[] CONDITIONAL_EXAMPLES =
    [
        new ConditionalSubcommandBlock(),
        new ConditionalSubcommandBlocks(),
        new ConditionalSubcommandEntity(),
        new ConditionalSubcommandScore()
    ];
    public override bool TerminatesChain => false;

    /// <summary>
    ///     Returns a new instance of the subcommand tied to the given keyword. Case insensitive.
    /// </summary>
    /// <param name="keyword">The case-insensitive keyword to use.</param>
    /// <param name="forExceptions">The calling statement, only used for exceptions.</param>
    /// <returns></returns>
    public static ConditionalSubcommand GetConditionalSubcommandForKeyword(string keyword, Statement forExceptions)
    {
        return keyword.ToUpper() switch
        {
            "SCORE" => new ConditionalSubcommandScore(),
            "ENTITY" => new ConditionalSubcommandEntity(),
            "BLOCKS" => new ConditionalSubcommandBlocks(),
            "BLOCK" => new ConditionalSubcommandBlock(),
            _ => throw new StatementException(forExceptions,
                $"Conditional subcommand '{keyword}' does not exist. Valid options include: " +
                string.Join(", ", CONDITIONAL_EXAMPLES.Select(c => c.Keyword.ToLower())))
        };
    }
}