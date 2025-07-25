using System;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC;

/// <summary>
///     A macro definition that can be called.
/// </summary>
public readonly struct Macro
{
    public readonly string name;
    public readonly string documentation;
    public readonly string[] argNames;
    public readonly Statement[] statements;

    /// <summary>
    ///     True if this Macro was imported from a library outside the current file.
    ///     If so, errors shouldn't show for its contents.
    /// </summary>
    public readonly bool isFromLibrary;

    public Macro(string name,
        string documentation,
        string[] argNames,
        Statement[] statements,
        bool isFromLibrary = false)
    {
        this.name = name;
        this.documentation = documentation;
        this.argNames = argNames;
        this.statements = statements;
        this.isFromLibrary = isFromLibrary;
    }
    /// <summary>
    ///     Case-insensitive-match this macro's name.
    /// </summary>
    /// <param name="otherName"></param>
    /// <returns></returns>
    public bool Matches(string otherName) { return this.name.Equals(otherName, StringComparison.OrdinalIgnoreCase); }
}