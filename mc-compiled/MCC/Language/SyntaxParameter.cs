using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
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

    internal readonly string languageServerSuggestionsId;
    /// <summary>
    ///     Extra suggestions for a language server to make for this parameter.
    /// </summary>
    internal readonly IReadOnlyList<EnumerationKeyword> languageServerSuggestions;

    private SyntaxParameter(string name,
        bool blockConstraint,
        [CanBeNull] Type typeConstraint,
        bool optional,
        bool variadic,
        Range? variadicRange,
        IReadOnlyList<EnumerationKeyword> languageServerSuggestions = null,
        string languageServerSuggestionsId = null)
    {
        this.name = name;
        this.blockConstraint = blockConstraint;
        this.typeConstraint = typeConstraint;
        this.optional = optional;
        this.variadic = variadic;
        this.variadicRange = variadicRange;
        this.languageServerSuggestions = languageServerSuggestions ?? [];
        this.languageServerSuggestionsId = languageServerSuggestionsId;
    }

    private static IReadOnlyList<EnumerationKeyword> ResolveLanguageServerSuggestions(string input)
    {
        switch (input.ToUpper())
        {
            case "FEATURE":
                return Language.features.Select(f => new EnumerationKeyword(f.name, f.details)).ToList();
            case "MINECRAFT_BLOCK":
                return VanillaData.Blocks();
            case "MINECRAFT_ENTITY":
                return VanillaData.Entities();
            case "MINECRAFT_ITEM":
                return VanillaData.Items();
            case "MINECRAFT_EFFECT":
                return VanillaData.MobEffects();
            case "MINECRAFT_ENCHANTMENT":
                return VanillaData.Enchantments();
            case "MINECRAFT_CAMERA_PRESET":
                return VanillaData.CameraPresets();
            default:
            {
                // Try to look up based on the pre-defined enum table
                if (Language.nameToEnumMappings.TryGetValue(input, out EnumerationKeyword[] enumKeywords))
                    return enumKeywords;
                throw new ArgumentException($"Unknown enum referenced in language.json syntax parameter: '{input}'.");
            }
        }
    }
    /// Attempts to parse the given input into a SyntaxParameter instance.
    /// <param name="input">
    ///     The string input to be parsed.
    ///     This should contain parameter information in a specific format.
    ///     See documentation for information about the format.
    /// </param>
    /// <param name="parsed">
    ///     When this method returns, contains the parsed SyntaxParameter instance if parsing was successful;
    ///     otherwise, contains the default value for SyntaxParameter.
    /// </param>
    /// <returns>
    ///     Returns true if the input was successfully parsed into a SyntaxParameter instance;
    ///     otherwise, returns false.
    /// </returns>
    [PublicAPI]
    public static bool TryParse(string input, out SyntaxParameter parsed)
    {
        int colon = input.IndexOf(':');
        parsed = default;

        if (colon == -1)
            return false;

        string name = input[..colon];
        string info = input[(colon + 1)..];
        string _type = new(info.TakeWhile(c => c == '_' || c == '*' || char.IsAsciiLetter(c) || char.IsAsciiDigit(c))
            .ToArray());
        if (string.IsNullOrWhiteSpace(_type))
            return false;
        Type type = Language.nameToTypeMappings[_type];
        bool blockConstraint = type == typeof(StatementOpenBlock);
        if (blockConstraint)
            type = null;

        bool optional = info.EndsWith('?');
        if (optional)
            info = info[..^1];

        string languageServerSuggestionsId = null;
        IReadOnlyList<EnumerationKeyword> languageServerSuggestions = null;
        int openBracketIndex = info.IndexOf('<');
        if (openBracketIndex != -1)
        {
            int closeBracketIndex = info.IndexOf('>');
            if (closeBracketIndex == -1)
                return false;
            if (openBracketIndex >= closeBracketIndex)
                return false;

            string enumName = info[(openBracketIndex + 1)..closeBracketIndex];
            languageServerSuggestionsId = enumName;
            languageServerSuggestions = ResolveLanguageServerSuggestions(enumName);
        }

        bool variadic = false;
        Range? variadicRange = null;
        int arrayOpenIndex = info.IndexOf('[');
        if (arrayOpenIndex != -1)
        {
            int arrayCloseIndex = info.IndexOf(']');
            if (arrayCloseIndex == -1)
                return false;
            if (arrayOpenIndex >= arrayCloseIndex)
                return false;

            variadic = true;

            if (arrayCloseIndex - arrayOpenIndex > 1) // not []
            {
                string contents = info[(arrayOpenIndex + 1)..arrayCloseIndex];
                variadicRange = Range.Parse(contents);
            }
            else
            {
                variadicRange = new Range(0, null);
            }
        }

        parsed = new SyntaxParameter(name, blockConstraint, type, optional, variadic, variadicRange,
            languageServerSuggestions, languageServerSuggestionsId);
        return true;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        string originalTypeId = this.blockConstraint ? "block" : Language.typeToNameMappings[this.typeConstraint!];

        sb.Append(this.name);
        sb.Append(':');
        sb.Append(originalTypeId);

        if (this.languageServerSuggestionsId != null)
        {
            sb.Append('<');
            sb.Append(this.languageServerSuggestionsId);
            sb.Append('>');
        }

        if (this.variadic)
        {
            sb.Append('[');
            if (this.variadicRange != null)
                sb.Append(this.variadicRange.Value.ToString());
            sb.Append(']');
        }

        if (this.optional)
            sb.Append('?');

        return sb.ToString();
    }
    /// <summary>
    ///     Gets a string representation of the parameter's usage, indicating its type and name.
    ///     If the parameter is a block constraint, the returned string will be "code block".
    ///     Otherwise, it combines type information and the parameter name, optionally including
    ///     details for variadic parameters with a <see cref="Range" />.
    /// </summary>
    /// <remarks>
    ///     This property constructs the string based on several factors:
    ///     - For block constraints (<see cref="SyntaxParameter.blockConstraint" />), returns "code block".
    ///     - Maps the type constraint (<see cref="SyntaxParameter.typeConstraint" />) to its corresponding name
    ///     using <see cref="Language.typeToNameMappings" />.
    ///     - If the parameter is variadic (<see cref="SyntaxParameter.variadic" />) and has an associated
    ///     <see cref="Range" />, the range's prefix (<see cref="Range.AsPrefix" />) is included
    ///     in the string representation.
    ///     - Combines the determined type and the parameter's <see cref="SyntaxParameter.name" />.
    /// </remarks>
    /// <returns>
    ///     A string representing the usage of the parameter.
    /// </returns>
    public string AsUsageString
    {
        get
        {
            if (this.blockConstraint)
                return "code block";

            string type = this.languageServerSuggestionsId?.Replace("_", " ") ??
                          this.typeConstraint!.GetFriendlyTokenNameOrDefault() ??
                          Language.typeToNameMappings[this.typeConstraint!];

            if (this.variadic && this.variadicRange.HasValue)
                type = this.variadicRange.Value.AsPrefix + ' ' + type;
            return $"{type}: {this.name}";
        }
    }

    /// <summary>
    ///     Returns a simple <see cref="SyntaxParameter" /> that matches the given type.
    /// </summary>
    /// <param name="type">The type to match.</param>
    /// <param name="name">The name of this parameter, or null to default to the input type's name.</param>
    /// <param name="optional">If this parameter is optional. Defaults to false.</param>
    public static SyntaxParameter Simple(Type type, string name = null, bool optional = false)
    {
        return new SyntaxParameter(name ?? type.Name, type == typeof(StatementOpenBlock), type, optional, false, null);
    }
    /// <summary>
    ///     Returns a simple <see cref="SyntaxParameter" /> that matches the given type one or more times.
    /// </summary>
    /// <param name="type">The type to match.</param>
    /// <param name="variadicRange">
    ///     The constraint on the number of parameters
    ///     that can/must match this parameter.
    /// </param>
    /// <param name="name">The name of this parameter, or null to default to the input type's name.</param>
    /// <param name="optional">If this parameter is optional. Defaults to false.</param>
    public static SyntaxParameter SimpleVariadic(Type type,
        Range? variadicRange = null,
        string name = null,
        bool optional = false)
    {
        return new SyntaxParameter(name ?? type.Name, type == typeof(StatementOpenBlock), type, optional, true,
            variadicRange);
    }

    /// <summary>
    ///     Returns a simple <see cref="SyntaxParameter" /> that matches the given type.
    /// </summary>
    /// <param name="name">The name of this parameter, or null to default to the input type's name.</param>
    /// <param name="optional">If this parameter is optional. Defaults to false.</param>
    /// <typeparam name="T">The type to match.</typeparam>
    /// <returns></returns>
    public static SyntaxParameter Simple<T>(string name = null, bool optional = false)
    {
        return new SyntaxParameter(name ?? nameof(T), typeof(T) == typeof(StatementOpenBlock), typeof(T), optional,
            false, null);
    }
    /// <summary>
    ///     Returns a simple <see cref="SyntaxParameter" /> that matches the given type one or more times.
    /// </summary>
    /// <param name="variadicRange">
    ///     The constraint on the number of parameters
    ///     that can/must match this parameter.
    /// </param>
    /// <param name="name">The name of this parameter, or null to default to the input type's name.</param>
    /// <param name="optional">If this parameter is optional. Defaults to false.</param>
    /// <typeparam name="T">The type to match.</typeparam>
    /// <returns></returns>
    public static SyntaxParameter SimpleVariadic<T>(Range? variadicRange = null,
        string name = null,
        bool optional = false)
    {
        return new SyntaxParameter(name ?? nameof(T), typeof(T) == typeof(StatementOpenBlock), typeof(T), optional,
            true, variadicRange);
    }
    /// <summary>
    ///     Returns if this <see cref="SyntaxParameter" /> matches <paramref name="other" /> in everything except name.
    /// </summary>
    /// <param name="other">The other syntax parameter to check against.</param>
    /// <returns><c>true</c> if both <see cref="SyntaxParameter" /> instances are equal except for name.</returns>
    public bool FunctionallyEquals(SyntaxParameter other)
    {
        if (this.optional != other.optional)
            return false;
        if (this.blockConstraint != other.blockConstraint)
            return false;
        if (this.blockConstraint)
            return true; // don't need any more checks, since block constraints are very simple like that

        // these should never be hit, but here we are
        Debug.Assert(this.typeConstraint != null);
        Debug.Assert(other.typeConstraint != null);

        if (this.typeConstraint.GUID != other.typeConstraint.GUID)
            return false;
        if (this.variadic != other.variadic)
            return false;
        if (this.variadic)
            if (this.variadicRange != other.variadicRange)
                return false;
        return true;
    }
}