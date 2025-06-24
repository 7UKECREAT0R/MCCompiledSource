using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC.Language;

/// <summary>
/// A language keyword with optional documentation.
/// </summary>
/// <param name="identifier">The written identifier of this keyword.</param>
/// <param name="docs"></param>
public record LanguageKeyword(string identifier, string docs = null)
{
    public readonly string identifier = identifier;
    private readonly string docs = docs ?? "No documentation available for v" + Executor.MCC_VERSION;
}