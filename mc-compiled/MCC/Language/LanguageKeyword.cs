using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC.Language;

/// <summary>
///     A language keyword with optional documentation.
/// </summary>
/// <param name="identifier">The written identifier of this keyword.</param>
/// <param name="docs">The documentation attached to this keyword. Never null.</param>
public readonly struct LanguageKeyword(string identifier, string docs = null)
{
    /// <summary>
    ///     The written identifier of this keyword.
    /// </summary>
    public readonly string identifier = identifier;
    /// <summary>
    ///     The documentation attached to this keyword. Never null.
    /// </summary>
    public readonly string docs = docs ?? "No documentation available for v1." + Executor.MCC_VERSION;

    public override string ToString() { return this.identifier; }
}