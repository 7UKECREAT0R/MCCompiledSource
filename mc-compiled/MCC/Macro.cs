using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC;

/// <summary>
///     A macro definition that can be called.
/// </summary>
public struct Macro
{
    public readonly string name;
    public readonly string documentation;
    public readonly string[] argNames;
    public readonly Statement[] statements;

    public Macro(string name, string documentation, string[] argNames, Statement[] statements)
    {
        this.name = name;
        this.documentation = documentation;
        this.argNames = argNames;
        this.statements = statements;
    }
    /// <summary>
    ///     Case-insensitive-match this macro's name.
    /// </summary>
    /// <param name="otherName"></param>
    /// <returns></returns>
    public bool Matches(string otherName)
    {
        return this.name.ToUpper().Trim().Equals
            (otherName.ToUpper().Trim());
    }
}