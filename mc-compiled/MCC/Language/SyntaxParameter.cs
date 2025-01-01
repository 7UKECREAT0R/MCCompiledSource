using System;
using System.Collections.Generic;
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

    private SyntaxParameter(string name, bool blockConstraint, [CanBeNull] Type typeConstraint, bool optional,
        bool variadic, Range? variadicRange, IReadOnlyList<EnumerationKeyword> languageServerSuggestions = null,
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
        string _type = new(info.TakeWhile(c => c == '_' || char.IsLetter(c)).ToArray());
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
}