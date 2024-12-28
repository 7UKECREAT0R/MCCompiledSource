namespace mc_compiled.MCC.Language;

public readonly struct Keyword(string name, string description = Keyword.NO_DESCRIPTION)
{
    private const string NO_DESCRIPTION = "";
    /// <summary>
    ///     The name/identifier of this keyword. Physically what the user has to type to specify it.
    /// </summary>
    public readonly string name = name;
    /// <summary>
    ///     The details of this keyword.
    /// </summary>
    public readonly string description = description;
}