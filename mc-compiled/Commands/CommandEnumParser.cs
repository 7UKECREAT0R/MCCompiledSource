using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Language;

namespace mc_compiled.Commands;

// ReSharper disable once PartialTypeWithSinglePart
public static partial class CommandEnumParser
{
    private static readonly bool isInitialized;
    private static readonly Dictionary<string, RecognizedEnumValue> parser = new(StringComparer.OrdinalIgnoreCase);

    static CommandEnumParser()
    {
        if (!isInitialized)
            Init();
        isInitialized = true;
    }

    /// <summary>
    ///     Attempts to parse a given input string into a <see cref="RecognizedEnumValue" />.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <param name="result">
    ///     When this method returns, contains the parsed <see cref="RecognizedEnumValue" /> if parsing was successful;
    ///     otherwise, contains the default value of <see cref="RecognizedEnumValue" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the input string was successfully parsed; otherwise, <see langword="false" />.
    /// </returns>
    public static bool TryParse(string input, out RecognizedEnumValue result)
    {
        bool resultInitial = parser.TryGetValue(input, out result);
        if (!resultInitial && input.Contains(':'))
        {
            string stripped = Command.Util.StripNamespace(input);
            return parser.TryGetValue(stripped, out result);
        }

        return resultInitial;
    }

    public static void Put(RecognizedEnumValue value) { parser[value.value.ToString()!] = value; }
    public static void Put(string key, RecognizedEnumValue value) { parser[key] = value; }
    /// <summary>
    ///     Initialize the global CommandEnumParser.
    /// </summary>
    public static void Init()
    {
        parser.Clear();

        if (GlobalContext.Debug)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Initializing enum parser...");
            Console.ForegroundColor = ConsoleColor.White;
        }

        InitializeGenerated();

        // ReSharper disable once InvertIf
        if (GlobalContext.Debug)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Registered " + parser.Count + " unique parser identifiers.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}

/// <summary>
///     Represents an enum value recognized from an identifier.
/// </summary>
public readonly struct RecognizedEnumValue
{
    public static RecognizedEnumValue None(string word) { return new RecognizedEnumValue(null, word, null); }

    internal readonly bool isNone = false;
    public readonly Type enumType;
    public readonly object value;

    [CanBeNull]
    public readonly string documentation;

    /// <summary>
    ///     Returns this <see cref="RecognizedEnumValue" /> as a language keyword.
    /// </summary>
    public LanguageKeyword AsKeyword => new(this.value.ToString()!, this.documentation);

    public RecognizedEnumValue(Type enumType,
        object value,
        [CanBeNull] string documentation)
    {
        this.isNone = value == null;
        this.enumType = enumType;
        this.documentation = documentation;
        this.value = value;
    }
    /// <summary>
    ///     Returns if this enum value is of a certain type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns></returns>
    public bool IsType<T>() where T : Enum
    {
        if (this.isNone)
            return false;

        Guid src = typeof(T).GUID;
        return this.enumType.GUID.Equals(src);
    }

    /// <summary>
    ///     Converts a PascalCase or camelCase string into a friendly name by inserting spaces before uppercase
    ///     letters and converting all characters to lowercase.
    /// </summary>
    /// <param name="name">
    ///     The input string in PascalCase or camelCase format that needs to be converted into a human-readable format.
    ///     For example, "SourceNameToFriendlyName" would be converted to "source name to friendly name".
    /// </param>
    /// <param name="prependArticle">
    ///     A boolean indicating whether to prepend an indefinite article ("a" or "an") before the generated friendly name.
    ///     If true, the method determines the appropriate article ("a" or "an") based on the first letter of the name.
    /// </param>
    /// <returns>
    ///     A string representing the friendly name version of the provided input, with spaces inserted before each uppercase
    ///     letter and all characters converted to lowercase. If <paramref name="prependArticle" /> is true, the string
    ///     will include an article ("a" or "an") at the beginning.
    /// </returns>
    private static string SourceNameToFriendlyName(string name, bool prependArticle)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "";

        StringBuilder sb = new();
        char firstLetter = name.Trim()[0];
        if (prependArticle)
            sb.Append(firstLetter is 'a' or 'e' or 'i' or 'o' or 'u'
                ? "an "
                : "a ");

        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
                sb.Append(' ');
            sb.Append(char.ToLower(name[i]));
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Requires this enum value to be of a certain type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="MCC.Compiler.StatementException">If the given enum is not of a certain type.</exception>
    public void RequireType<T>(Statement thrower) where T : Enum
    {
        if (!this.isNone && IsType<T>())
            return;

        string reqEnumName = SourceNameToFriendlyName(typeof(T).Name, true);
        string givenEnumName = this.enumType?.Name;
        if (givenEnumName == null)
            givenEnumName = this.value == null ? "(unknown)" : $"the text \"{this.value}\"";
        else
            givenEnumName = SourceNameToFriendlyName(givenEnumName, true);

        string[] possibleValues = Enum.GetNames(typeof(T));
        throw new StatementException(thrower,
            $"Must specify {reqEnumName}; Got {givenEnumName}. Possible values: {string.Join(", ", possibleValues)}");
    }
}

/// <summary>
///     Marks an enum as being able to be used in MCCompiled.
///     When marked with this attribute:
///     <ul>
///         <li>This enum can be used in <c>lanugage.json</c>'s <c>enums</c> property for LSP suggestions.</li>
///         <li>Its value names will be recognized in MCCompiled code and parsed appropriately.</li>
///     </ul>
///     This attribute doesn't do anything unless the source generators are running.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class UsableInMCCAttribute : Attribute;