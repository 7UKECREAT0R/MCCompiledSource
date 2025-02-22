﻿namespace mc_compiled.MCC.Language;

/// <summary>
///     A keyword defined in the <c>enums</c> property of the 'language.json' file.
/// </summary>
/// <param name="name">The name/identifier of this keyword. Physically what the user has to type to specify it.</param>
/// <param name="description">The details of this keyword, generally only displayed by LSP-enabled clients.</param>
public readonly struct EnumerationKeyword(string name, string description = null)
{
    /// <summary>
    ///     The name/identifier of this keyword. Physically what the user has to type to specify it.
    /// </summary>
    public readonly string name = name;
    /// <summary>
    ///     The details of this keyword, generally only displayed by LSP-enabled clients.
    /// </summary>
    public readonly string description = description;
}