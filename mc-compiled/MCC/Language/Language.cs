using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.Language;

/// <summary>
///     Wrapper and loader for `language.json`, as well as an API for accessing language information.
/// </summary>
public static class Language
{
    private const string FILE = "language.json";

    public static string[] builtinPreprocessorVariables;
    public static ImmutableList<FeatureDefinition> features;

    public static Dictionary<string, NamedType> nameToTypeMappings;
    public static Dictionary<Type, NamedType> namedEntries;

    public static Dictionary<string, Keyword[]> nameToEnumMappings;

    public static Dictionary<string, string> categories;
    public static LanguageSyntax syntax;

    static Language()
    {
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

        if (Program.DEBUG)
            Console.WriteLine("Loading language.json...");

        string _json = File.ReadAllText(path);
        JObject json = JObject.Parse(_json);
        LoadFromJSON(json);

        IsLoaded = true;
    }
    public static bool IsLoaded { get; }
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
        nameToTypeMappings = new Dictionary<string, NamedType>();
        namedEntries = new Dictionary<Type, NamedType>();
        JObject typeMappings = json["mappings"] as JObject ??
                               throw new Exception("language.json/mappings was null.");
        foreach ((string key, JToken mappingToken) in typeMappings)
        {
            var value = Type.GetType(TYPE_PREFIX + mappingToken, true, false);
            var namedType = new NamedType(value, key);
            nameToTypeMappings[key] = namedType;
            namedEntries[value] = namedType;
        }

        // load enum mappings
        nameToEnumMappings = new Dictionary<string, Keyword[]>();
        JObject enumMappings = json["enums"] as JObject ??
                               throw new Exception("language.json/enums was null.");
        foreach ((string enumName, JToken value) in enumMappings)
            if (value.Type == JTokenType.Array)
            {
                IEnumerable<JToken> values = (value as JArray)!.Values<JToken>();
                nameToEnumMappings[enumName] = values.Select(jt =>
                {
                    if (jt.Type == JTokenType.String)
                        return new Keyword(jt.Value<string>());
                    throw new Exception($"Unexpected type in enum definition's array: {jt.Type}");
                }).ToArray();
            }
            else if (value.Type == JTokenType.Object)
            {
                var keywords = new List<Keyword>();
                foreach ((string entryName, JToken entryValue) in (value as JObject)!)
                    keywords.Add(new Keyword(entryName, entryValue.ToString()));
                nameToEnumMappings[enumName] = keywords.ToArray();
            }
            else if (value.Type == JTokenType.String)
            {
                string typeIdentifier = value.ToString();
                Array valuesRaw = Enum.GetValues(Type.GetType(typeIdentifier, true, true));
                var values = new Keyword[valuesRaw.Length];
                for (int i = 0; i < values.Length; i++)
                    values[i] = new Keyword(valuesRaw.GetValue(i)?.ToString() ??
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
    }
}