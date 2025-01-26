using System;
using JetBrains.Annotations;

namespace mc_compiled.MCC.Language;

/// <summary>
///     Represents a possible parameter the compiler will accept in a specific place in code.
/// </summary>
public readonly struct PossibleParameter(
    Type argumentType = null,
    string argumentName = null,
    EnumerationKeyword[] enumerationKeywords = null) : IEquatable<PossibleParameter>
{
    /// <summary>
    ///     If present, the runtime type the user can specify, facilitated via <see cref="Language.nameToTypeMappings" />
    /// </summary>
    [CanBeNull]
    [PublicAPI]
    public readonly Type argumentType = argumentType;
    /// <summary>
    ///     If present, the user-friendly name of this particular argument.
    /// </summary>
    [CanBeNull]
    [PublicAPI]
    public readonly string argumentName = argumentName;
    /// <summary>
    ///     The explicit keywords the user could type to fulfill this parameter.
    /// </summary>
    [CanBeNull]
    [PublicAPI]
    public readonly EnumerationKeyword[] enumerationKeywords = enumerationKeywords;

    public bool Equals(PossibleParameter other)
    {
        return this.argumentType == other.argumentType && this.argumentName == other.argumentName &&
               Equals(this.enumerationKeywords, other.enumerationKeywords);
    }
    public override bool Equals(object obj) { return obj is PossibleParameter other && Equals(other); }
    public override int GetHashCode()
    {
        return HashCode.Combine(this.argumentType, this.argumentName, this.enumerationKeywords?.Length ?? 0);
    }
}