namespace mc_compiled.MCC.Language;

/// <summary>
///     Represents an error related to a <see cref="SyntaxParameter" />. Used for LSP linting.
/// </summary>
/// <param name="error">A description of the error to show to the user.</param>
public struct SyntaxValidationError(string error)
{
    public readonly string error = error;
}