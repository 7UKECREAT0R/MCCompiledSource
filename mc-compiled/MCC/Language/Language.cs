﻿using System;
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

        IsLoaded = true;
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
    }

    /// <summary>
    ///     Query a syntax group from a directive using dots (<c>.</c>) to separate paths.
    /// </summary>
    /// <param name="query">
    ///     The query string, starting with the directive name, and then optionally traversing its children with
    ///     dots.
    /// </param>
    /// <returns>A reference to the located syntax group, or <c>null</c> if it couldn't be found.</returns>
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

        return currentGroup;
    }
}