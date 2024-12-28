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

    public static Dictionary<string, string> categories;

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
    public static bool IsLoaded { get; private set; }
    private static void LoadFromJSON(JObject json)
    {
        // load built-in preprocessor variables
        builtinPreprocessorVariables = json["preprocessor_variables"]?.ToObject<string[]>();

        // load features
        features = (json["features"] as JObject)?
            .Properties()
            .Select(FeatureDefinition.FromJSONProperty)
            .ToImmutableList();

        // load type mappings
        const string TYPE_PREFIX = "mc_compiled.MCC.Compiler.";
        nameToTypeMappings = new Dictionary<string, NamedType>();
        namedEntries = new Dictionary<Type, NamedType>();
        JObject mappings = json["mappings"] as JObject ?? [];
        foreach ((string key, JToken mappingToken) in mappings)
        {
            var value = Type.GetType(TYPE_PREFIX + mappingToken, true, false);
            var namedType = new NamedType(value, key);
            nameToTypeMappings[key] = namedType;
            namedEntries[value] = namedType;
        }

        // directive categories
        categories = new Dictionary<string, string>();
        JObject categoriesToken = json["categories"] as JObject ?? [];
        foreach ((string name, JToken value) in categoriesToken)
        {
            string description = value.ToString();
            categories[name] = description;
        }

        // parse directives
    }
}