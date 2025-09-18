using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Execute;

public abstract class Subcommand
{
    /// <summary>
    ///     The keyword used to invoke this subcommand.
    /// </summary>
    public abstract string Keyword { get; }

    /// <summary>
    ///     Returns if this subcommand terminates the execute chain.
    /// </summary>
    public abstract bool TerminatesChain { get; }

    /// <summary>
    ///     Attempt to load this subcommand's properties from a set of input tokens.
    /// </summary>
    /// <param name="tokens">The tokens to load.</param>
    public abstract void FromTokens(Statement tokens);
    /// <summary>
    ///     Returns this subcommand as actual command output.
    /// </summary>
    /// <returns></returns>
    public abstract string ToMinecraft();

    /// <summary>
    ///     Returns a new instance of the subcommand tied to the given keyword. Case-insensitive.
    /// </summary>
    /// <param name="keyword">The case-insensitive keyword to use.</param>
    /// <param name="forExceptions">The calling statement, only used for exceptions.</param>
    /// <returns>The newly created subcommand instance.</returns>
    /// <exception cref="StatementException">Thrown if provided an unknown execute subcommand.</exception>
    public static Subcommand GetSubcommandForKeyword(string keyword, Statement forExceptions)
    {
        return keyword.ToUpper() switch
        {
            "ALIGN" => new SubcommandAlign(),
            "ANCHORED" => new SubcommandAnchored(),
            "AS" => new SubcommandAs(),
            "AT" => new SubcommandAt(),
            "FACING" => new SubcommandFacing(),
            "IF" => new SubcommandIf(),
            "IN" => new SubcommandIn(),
            "POSITIONED" => new SubcommandPositioned(),
            "ROTATED" => new SubcommandRotated(),
            "RUN" => new SubcommandRun(),
            "UNLESS" => new SubcommandUnless(),
            _ => throw new StatementException(forExceptions, $"Unknown execute subcommand '{keyword}'.")
        };
    }
}