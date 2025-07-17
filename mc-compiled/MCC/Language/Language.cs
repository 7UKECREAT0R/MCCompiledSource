using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.Language;

/// <summary>
///     Wrapper and loader for `language.json`, as well as an API for accessing language information.
/// </summary>
public static class Language
{
    private const string FILE = "language.json";

    public static readonly LanguageKeyword[] KEYWORDS_OPERATORS =
    [
        new("<", "Checks if the current value is less than the next one."),
        new(">", "Checks if the current value is greater than the next one."),
        new("<=", "Checks if the current value is less than or equal to the next one."),
        new(">=", "Checks if the current value is greater than or equal to the next one."),
        new("{", "Opens a code block."),
        new("}", "Closes a code block."),
        new("=", "Assigns a value to whatever's on the left-hand side."),
        new("==", "Checks if the current value is equal to the next one."),
        new("!=", "Checks if the current value is not equal to the next one."),
        new("(", "Open parenthesis."),
        new(")", "Close parenthesis."),
        new("+", "Adds the left and right values."),
        new("-", "Subtracts the right value from the left value."),
        new("*", "Multiplies the left and right values."),
        new("/", "Divides the left value by the right value."),
        new("%", "Divides the left value by the right value and returns the remainder."),
        new("+=", "Adds the left and right values. Assigns the result to the left value."),
        new("-=", "Subtracts the right value from the left value. Assigns the result to the left value."),
        new("*=", "Multiplies the left and right values. Assigns the result to the left value."),
        new("/=", "Divides the left value by the right value. Assigns the result to the left value."),
        new("%=",
            "Divides the left value by the right value and returns the remainder. Assigns the result to the left value."),
        new("~", "Coordinate relative to the executing position."),
        new("^", "Coordinate relative to where the executor is facing.")
    ];
    public static readonly LanguageKeyword[] KEYWORDS_SELECTORS =
    [
        new("@e", "Reference all entities."),
        new("@a", "Reference all players."),
        new("@s", "Reference the executing entity/player."),
        new("@p", "Reference the nearest player."),
        new("@r", "Reference a random entity.")
    ];
    public static readonly LanguageKeyword[] KEYWORDS_LITERALS =
    [
        new("true", "The boolean value 'true'."),
        new("false", "The boolean value 'false'."),
        new("null", "Defaults to 0, false, or null depending on the context. Represents nothing generically.")
    ];
    public static readonly List<LanguageKeyword> KEYWORDS_TYPES =
    [
        new("global", "Defines something that's global and not attached to any specific entity."),
        new("local", "Defines something that's local to an entity."),
        new("extern",
            "Function attribute which makes it use an existing .mcfunction file as its source. Parameters will be passed verbatim."),
        new("export", "Function attribute which forces it to be exported whether it's in use or not."),
        new("bind",
            "Value attribute which binds a MoLang query to the value. The value will be updated automatically whenever the query result changes."),
        new("auto", "Function attribute which makes it automatically run every tick; or, if specified, every N ticks."),
        new("partial",
            "Function attribute which makes it able to be defined more than once, with each definition appending commands to it instead of overwriting it."),
        new("async",
            "Function attribute which makes it run asynchronously. Allows the use of the 'await' command for sequences which don't finish in a single tick.")
    ];
    public static readonly List<LanguageKeyword> KEYWORDS_COMPARISONS =
    [
        /* defined in language.json */
    ];
    public static readonly List<LanguageKeyword> KEYWORDS_COMMAND_OPTIONS =
    [
        /* defined in language.json */
    ];

    public static string[] builtinPreprocessorVariables;
    public static ImmutableList<FeatureDefinition> features;

    public static Dictionary<string, Type> nameToTypeMappings;
    public static Dictionary<Type, string> typeToNameMappings;

    public static Dictionary<string, EnumerationKeyword[]> nameToEnumMappings;

    public static Dictionary<string, string> categories;
    public static LanguageSyntax syntax;

    public static Dictionary<string, Directive> directives;
    public static bool IsLoaded { get; private set; }
    /// <summary>
    ///     Gets all defined directives in the language.
    /// </summary>
    /// <remarks>
    ///     This property returns an <see cref="IEnumerable{T}" /> of <see cref="Directive" /> objects
    ///     representing all directives currently loaded in the language configuration.
    ///     The returned collection includes both preprocessor and runtime directives.
    /// </remarks>
    public static IEnumerable<Directive> AllDirectives => directives.Values;
    /// <summary>
    ///     Gets all preprocessor directives defined in the language configuration.
    /// </summary>
    /// <remarks>
    ///     This property returns an <see cref="IEnumerable{T}" /> of <see cref="Directive" /> objects
    ///     that are identified as preprocessor directives. A directive is considered a preprocessor directive
    ///     if its <see cref="Directive.IsPreprocessor" /> property is <c>true</c>.
    /// </remarks>
    public static IEnumerable<Directive> AllPreprocessorDirectives =>
        directives.Values
            .DistinctBy(d => d.name)
            .Where(d => d.IsPreprocessor);
    /// <summary>
    ///     Gets all runtime directives defined in the language configuration.
    /// </summary>
    /// <remarks>
    ///     This property returns an <see cref="IEnumerable{T}" /> of <see cref="Directive" /> objects,
    ///     representing all directives that are not preprocessor directives within the language configuration.
    ///     Runtime directives are identified as directives whose <see cref="Directive.IsPreprocessor" /> property is
    ///     <c>false</c>.
    /// </remarks>
    public static IEnumerable<Directive> AllRuntimeDirectives =>
        directives.Values
            .DistinctBy(d => d.name)
            .Where(d => !d.IsPreprocessor);
    /// <summary>
    ///     Gets the names of all directives that are classified as preprocessor directives.
    /// </summary>
    /// <remarks>
    ///     This property filters the currently loaded <see cref="Directive" /> instances
    ///     stored in the <see cref="Language.directives" /> dictionary to identify only those that
    ///     are marked as preprocessor directives. A directive is considered a preprocessor directive
    ///     if the <see cref="Directive.IsPreprocessor" /> property evaluates to <c>true</c>.
    ///     The resulting collection contains only the <see langword="string" /> names
    ///     of these preprocessor directives.
    /// </remarks>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> of <see langword="string" /> containing the keys (names)
    ///     of directives defined as preprocessor directives.
    /// </returns>
    public static IEnumerable<string> AllPreprocessorDirectiveNames =>
        directives.Where(kv => kv.Value.IsPreprocessor).Select(kv => kv.Key);
    /// <summary>
    ///     Gets the names of all runtime directives defined in the language.
    /// </summary>
    /// <remarks>
    ///     This property returns an <see cref="IEnumerable{T}" /> of <see cref="string" /> values
    ///     representing the keys of directives that are not preprocessor directives.
    ///     The distinction between runtime and preprocessor directives is determined
    ///     by the <see cref="Directive.IsPreprocessor" /> property.
    /// </remarks>
    public static IEnumerable<string> AllRuntimeDirectiveNames =>
        directives.Where(kv => !kv.Value.IsPreprocessor).Select(kv => kv.Key);
    /// <summary>
    ///     Retrieves a collection of <see cref="Directive" /> instances that belong to the specified category.
    /// </summary>
    /// <param name="category">
    ///     A <see cref="string" /> representing the category by which to filter the <see cref="Directive" /> instances.
    ///     This parameter must match the <see cref="Directive.category" /> property of the desired directives.
    /// </param>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> of <see cref="Directive" /> objects whose <see cref="Directive.category" />
    ///     matches
    ///     the specified <paramref name="category" />. If no matching directives exist, returns an empty collection.
    /// </returns>
    public static IEnumerable<Directive> DirectivesByCategory(string category)
    {
        return directives.Values.Where(d => d.category != null && d.category.Equals(category));
    }

    /// <summary>
    ///     Retrieves a <see cref="Directive" /> instance associated with the specified <paramref name="token" />.
    /// </summary>
    /// <param name="token">
    ///     A <see cref="string" /> representing the token used to identify the desired <see cref="Directive" />.
    ///     This is typically the command or keyword for the directive.
    /// </param>
    /// <returns>
    ///     Returns the <see cref="Directive" /> associated with the given <paramref name="token" />,
    ///     if found in the <c>directives</c> dictionary; otherwise, <c>null</c>.
    /// </returns>
    public static Directive QueryDirective(string token) { return directives.GetValueOrDefault(token); }

    /// <summary>
    ///     Loads `language.json` if it's not already.
    /// </summary>
    /// <exception cref="Exception"></exception>
    internal static void TryLoad()
    {
        if (IsLoaded)
            return;

        string baseDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
        Debug.Assert(baseDirectory != null, nameof(baseDirectory) + " != null");
        string path = Path.Combine(baseDirectory, FILE);

        if (!File.Exists(path))
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("WARNING: Missing language.json file at '{0}'.", path);
            Console.Error.WriteLine("WARNING: Missing language.json file at '{0}'.", path);
            Console.ForegroundColor = oldColor;
            throw new Exception("Missing file 'language.json' in executable directory.");
        }

        if (GlobalContext.Debug)
            Console.WriteLine("Loading language.json...");

        string _json = File.ReadAllText(path);
        JObject json = JObject.Parse(_json, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore,
            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
            LineInfoHandling = LineInfoHandling.Load
        });
        LoadFromJSON(json);
    }
    private static void LoadFromJSON(JObject json)
    {
        if (IsLoaded)
            return;

        // load built-in preprocessor variables
        builtinPreprocessorVariables = json["preprocessor_variables"]?.ToObject<string[]>() ??
                                       throw new Exception("language.json/preprocessor_variables was null.");

        // load syntax
        syntax = json["syntax"]?.ToObject<LanguageSyntax>() ??
                 throw new Exception("language.json/syntax was null.");

        // load features
        features = (json["features"] as JObject)?
                   .Properties()
                   .Select(FeatureDefinition.FromJSONProperty)
                   .ToImmutableList() ??
                   throw new Exception("language.json/features was null.");

        // load type mappings
        const string TYPE_PREFIX = "mc_compiled.MCC.Compiler.";
        nameToTypeMappings = new Dictionary<string, Type>();
        typeToNameMappings = new Dictionary<Type, string>();
        JObject typeMappings = json["mappings"] as JObject ??
                               throw new Exception("language.json/mappings was null.");
        foreach ((string key, JToken mappingToken) in typeMappings)
        {
            string mapping = mappingToken.Value<string>();
            Type value = mapping.Equals("_block")
                ? typeof(StatementOpenBlock)
                : Type.GetType(TYPE_PREFIX + mapping, true, false);
            nameToTypeMappings[key] = value;
            typeToNameMappings[value] = key;
        }

        // load enum mappings
        nameToEnumMappings = new Dictionary<string, EnumerationKeyword[]>();
        JObject enumMappings = json.Value<JObject>("syntax").Value<JObject>("enums") ??
                               throw new Exception("language.json/syntax/enums was null.");

        foreach ((string enumName, JToken value) in enumMappings)
            if (value.Type == JTokenType.Array)
            {
                IEnumerable<JToken> values = (value as JArray)!.Values<JToken>();
                nameToEnumMappings[enumName] = values.Select(jt =>
                {
                    if (jt.Type == JTokenType.String)
                        return new EnumerationKeyword(jt.Value<string>());
                    throw new Exception($"Unexpected type in enum {enumName}'s definition: {jt.Type}");
                }).ToArray();
            }
            else if (value.Type == JTokenType.Object)
            {
                var keywords = new List<EnumerationKeyword>();
                foreach ((string entryName, JToken entryValue) in (value as JObject)!)
                    keywords.Add(new EnumerationKeyword(entryName, entryValue.ToString()));
                nameToEnumMappings[enumName] = keywords.ToArray();
            }
            else if (value.Type == JTokenType.String)
            {
                string typeIdentifier = value.ToString();
                Debug.WriteLine($"Reflecting enumeration '{typeIdentifier}'...");
                Array valuesRaw = Enum.GetValues(Type.GetType(typeIdentifier, true, true));
                Debug.WriteLine($"\tGot {valuesRaw.Length} values in it.");
                var values = new EnumerationKeyword[valuesRaw.Length];
                for (int i = 0; i < values.Length; i++)
                    values[i] = new EnumerationKeyword(valuesRaw.GetValue(i)?.ToString() ??
                                                       throw new Exception("Everything blew up."));
                nameToEnumMappings[enumName] = values;
            }
            else
            {
                throw new Exception($"Unexpected type in enum definition: {value.Type}");
            }

        // directive categories
        categories = new Dictionary<string, string>();
        JObject categoriesToken = json["categories"] as JObject ??
                                  throw new Exception("language.json/categories was null.");
        foreach ((string name, JToken value) in categoriesToken)
        {
            string description = value.ToString();
            categories[name] = description;
        }

        // parse directives
        directives = new Dictionary<string, Directive>();
        foreach (JProperty property in (json["directives"] as JObject ??
                                        throw new Exception("language.json/directives was null.")).Properties())
        {
            Directive directive = Directive.Parse(property);
            if (!directives.TryAdd(directive.name, directive))
                throw new Exception($"Duplicate directive '{directive.name}'.");

            if (directive.aliases != null)
                foreach (string alias in directive.aliases)
                    if (!directives.TryAdd(alias, directive))
                        throw new Exception($"Duplicate directive alias '{alias}'.");
        }

        // harvest keywords from directives
        foreach (Directive directive in directives.Values)
        {
            // type keywords are defined under the "define" command
            if (directive.name.Equals("define"))
            {
                KEYWORDS_TYPES.AddRange(directive.CollectKeywords());
                continue;
            }

            // comparison-related keywords are defined under the "if" command.
            if (directive.name.Equals("if"))
            {
                KEYWORDS_COMPARISONS.AddRange(directive.CollectKeywords());
                continue;
            }

            // everything else
            KEYWORDS_COMMAND_OPTIONS.AddRange(directive.CollectKeywords());
        }

        IsLoaded = true;
    }

    /// <summary>
    ///     Query a syntax group from a directive using dots (<c>.</c>) to separate paths.
    /// </summary>
    /// <param name="query">
    ///     The query string, starting with the directive name, and then optionally traversing its children with
    ///     dots.
    /// </param>
    /// <returns>A reference to the located syntax group, or <c>null</c> if it could not be found.</returns>
    /// <example>
    ///     <code>
    /// gloaltitle.subcommand
    /// if
    /// if.comparison
    /// define
    /// </code>
    /// </example>
    [CanBeNull]
    public static SyntaxGroup QuerySyntaxGroup(string query)
    {
        string[] _chunks = query.Split('.');
        if (_chunks.Length == 0)
            return null;

        string directiveName = _chunks[0];
        Directive directive = directives[directiveName];
        if (directive == null)
            return null;

        SyntaxGroup currentGroup = directive.Syntax;
        for (int i = 1; i < _chunks.Length; i++)
        {
            string chunk = _chunks[i];
            currentGroup = currentGroup.QueryChild(chunk);
            if (currentGroup == null)
                return null;
        }

        return (SyntaxGroup) currentGroup.Clone();
    }
}