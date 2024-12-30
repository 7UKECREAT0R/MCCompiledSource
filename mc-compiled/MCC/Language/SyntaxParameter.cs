using System;
using JetBrains.Annotations;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Language;

public readonly struct SyntaxParameter
{
    /// <summary>
    ///     The user-facing name of this parameter. Whitespace is allowed.
    /// </summary>
    public readonly string name;

    /// <summary>
    ///     If this parameter is a block constraint rather than a type constraint.
    ///     This field being <c>true</c> is the only case where <see cref="typeConstraint" /> is allowed to be null.
    /// </summary>
    internal readonly bool blockConstraint;
    /// <summary>
    ///     The type constraint imposed by this parameter. If <see cref="blockConstraint" />, this may be null.
    /// </summary>
    [CanBeNull]
    internal readonly Type typeConstraint;

    /// <summary>
    ///     If this parameter is optional.
    /// </summary>
    internal readonly bool optional;

    /// <summary>
    ///     If this parameter is variadic.
    /// </summary>
    internal readonly bool variadic;
    /// <summary>
    ///     If <see cref="variadic" /> is true, this may be non-null. If so, this is the number of values
    ///     (min, max, or both) that this parameter must accept to be valid.
    /// </summary>
    internal readonly Range? variadicRange;

    /// <summary>
    ///     Extra suggestions for a language server to make for this parameter.
    /// </summary>
    internal readonly string[] languageServerSuggestions;

    private SyntaxParameter(string name, bool blockConstraint, [CanBeNull] Type typeConstraint, bool optional,
        bool variadic, Range? variadicRange, string[] languageServerSuggestions = null)
    {
        this.name = name;
        this.blockConstraint = blockConstraint;
        this.typeConstraint = typeConstraint;
        this.optional = optional;
        this.variadic = variadic;
        this.variadicRange = variadicRange;
        this.languageServerSuggestions = languageServerSuggestions ?? [];
    }
    public static bool TryParse(string input, out SyntaxParameter parsed)
    {
        int colon = input.IndexOf(':');
        parsed = default;

        if (colon == -1)
            return false;

        string name = input[..colon];
        string info = input[(colon + 1)..];

        bool optional = info.EndsWith('?');
        if (optional)
            info = info[..^1];
    }
}