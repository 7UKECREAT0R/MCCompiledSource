using mc_compiled.MCC.SyntaxHighlighting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A directive that is the root of a statement.
    /// </summary>
    public class Directive
    {
        /// <summary>
        /// An implementation of a directive call.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="tokens"></param>
        public delegate void DirectiveImpl(Executor executor, Statement tokens);


        private static short nextIndex = 0;
        public Directive(DirectiveImpl call, string identifier, string[] aliases, string description, string documentation, string wikiLink, string category, params TypePattern[] patterns)
        {
            nextIndex++;
            this.call = call;
            this.identifier = identifier;
            this.aliases = aliases;
            this.description = description;
            this.documentation = documentation;
            this.wikiLink = wikiLink;
            this.category = category;
            this.patterns = patterns;

            // cache if this directive overlaps an enum
            if (Commands.CommandEnumParser.TryParse(identifier, out Commands.ParsedEnumValue result)) this.enumValue = result;
        }
        public Directive WithAttribute(DirectiveAttribute attribute)
        {
            this.attributes |= attribute;
            return this;
        }
        public Directive WithAttributes(params DirectiveAttribute[] attributes)
        {
            foreach (DirectiveAttribute attribute in attributes)
                this.attributes |= attribute;
            return this;
        }

        /// <summary>
        /// Get the key that should be used in a dictionary.
        /// </summary>
        public string[] DictKeys
        {
            get
            {
                if (this.aliases != null && this.aliases.Length > 0)
                {
                    List<string> values = new List<string>() {this.identifier };
                    values.AddRange(this.aliases);
                    return values.ToArray();
                }

                return new string[] {this.identifier };
            }
        }
        public string DirectiveOverview
        {
            get
            {
                if (this.aliases != null && this.aliases.Length > 0)
                {
                    List<string> values = new List<string>() {this.identifier };
                    values.AddRange(this.aliases);
                    return string.Join("/", values);
                }

                return this.identifier;
            }
        }

        /// <summary>
        /// Returns if this directive is a preprocessor directive based on whether it begins with a dollar sign ($).
        /// </summary>
        public bool IsPreprocessor
        {
            get => this.identifier.Length > 0 ? this.identifier[0].Equals('$') : false;
        }

        // Directive might overlap an enum value.
        // In the case this happens, it can use this field to help convert itself.
        public readonly Commands.ParsedEnumValue? enumValue;

        public readonly string identifier;
        public readonly string[] aliases;
        public readonly string description;
        public readonly string documentation;
        public readonly string wikiLink;
        public readonly string category;
        public readonly DirectiveImpl call;
        public readonly TypePattern[] patterns;
        public DirectiveAttribute attributes;

        public override int GetHashCode() => this.identifier.GetHashCode();
        public override string ToString() =>
            $"{this.DirectiveOverview} - patterns: {this.patterns.Length} - desc: {this.description}";
    }
    /// <summary>
    /// Attributes used to modify how directive statements behave.
    /// </summary>
    [Flags]
    [UsedImplicitly]
    public enum DirectiveAttribute
    {
        DONT_DEREFERENCE = 1 << 0,       // Won't expand any explicit PPV identifiers. Used in $macro to allow passing in parameters.
        DONT_FLATTEN_ARRAYS = 1 << 1,   // Won't attempt to flatten JSON arrays to their root values.
        DONT_RESOLVE_STRINGS = 1 << 2,  // Won't resolve PPV entries in string parameters.
        USES_FSTRING = 1 << 3,          // Indicates support for format-strings.
        INVERTS_COMPARISON = 1 << 4,    // Inverts a comparison that was previously run on this scope. Used by ELSE and ELIF.
        DONT_DECORATE = 1 << 5,         // Won't decorate this directive in the compiled file when decoration is enabled.
        DOCUMENTABLE = 1 << 6,          // This directive is documentable by placing a comment before it.
    }
    public static class Directives
    {
        private static readonly Dictionary<string, DirectiveAttribute> attributeLookup;
        private static readonly Dictionary<string, Directive> directiveLookup;

        public static List<Directive> REGISTRY = new List<Directive>();
        public static IEnumerable<Directive> PreprocessorDirectives => REGISTRY.Where(directive => directive.identifier[0] == '$');
        public static IEnumerable<Directive> RegularDirectives => REGISTRY.Where(directive => directive.identifier[0] != '$');

        static Directives()
        {
            attributeLookup = new Dictionary<string, DirectiveAttribute>();
            directiveLookup = new Dictionary<string, Directive>();

            foreach(object attrib in Enum.GetValues(typeof(DirectiveAttribute)))
                attributeLookup[attrib.ToString()] = (DirectiveAttribute)attrib;
        }
        
        /// <summary>
        /// Query for a directive that matches this token contents. Case insensitive.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Directive Query(string token)
        {
            if (directiveLookup.TryGetValue(token.ToUpper(), out Directive directive))
                return directive;
            return null;
        }
        /// <summary>
        /// Add a directive to the registry and the lookup dictionary.
        /// </summary>
        /// <param name="directive">The directive to add.</param>
        private static void RegisterDirective(Directive directive)
        {
            REGISTRY.Add(directive);

            foreach(string dictKey in directive.DictKeys)
                directiveLookup[dictKey.ToUpper()] = directive;
        }
        /// <summary>
        /// Sorts all the directives by name.
        /// </summary>
        private static void SortDirectives()
        {
            REGISTRY = REGISTRY.OrderBy(directive => directive.identifier).ToList();
        }

        public static void LoadFromLanguage(bool debug)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Debug.Assert(assemblyDir != null, nameof(assemblyDir) + " was null");
            
            string path = Path.Combine(assemblyDir, Executor.LANGUAGE_FILE);

            if (!File.Exists(path))
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WARNING: Missing language.json file at '{0}'. Execution cannot continue.", path);
                Console.ForegroundColor = oldColor;
                Console.ReadLine();
                throw new Exception("Missing file 'language.json' in executable directory.");
            }

            if(debug)
                Console.WriteLine("Parsing language.json...");

            string _json = File.ReadAllText(path);
            JObject json = JObject.Parse(_json);
            ReadJSON(json);

            if (!debug)
                return;
            
            Console.WriteLine("Parsed {0} directives from language.json:", REGISTRY.Count);
            foreach (Directive directive in REGISTRY)
            {
                if (directive.enumValue.HasValue)
                {
                    ConsoleColor oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    string className = directive.enumValue.Value.enumType.Name;
                    Console.WriteLine($"\t\tOverlaps with {className}.{directive.identifier}:");
                    Console.ForegroundColor = oldColor;
                }
                Console.WriteLine("\t{0}", directive);
            }
        }

        /// <summary>
        /// Read all directives from the language.json root object.
        /// </summary>
        /// <param name="root">The root object of the language.json file.</param>
        private static void ReadJSON(JObject root)
        {
            const string IDENTIFIER_PREFIX = "mc_compiled.MCC.Compiler.";

            // read type mappings
            var mappings = new Dictionary<string, NamedType>();
            var mappingsToken = root["mappings"] as JObject;

            Debug.Assert(mappingsToken != null, "language.json/mappings was null.");
            
            IEnumerable<JProperty> properties = mappingsToken.Properties();
            foreach (JProperty field in properties)
            {
                string key = field.Name;
                var value = Type.GetType(IDENTIFIER_PREFIX + field.Value, true, false);
                mappings[key] = new NamedType(value, key);
            }
            Syntax.mappings = mappings;

            // read categories
            var categories = new Dictionary<string, string>();
            JToken categoriesToken = root["categories"];
            Debug.Assert(categoriesToken != null, "language.json/categories was null.");
            foreach(KeyValuePair<string, JToken> property in (JObject)categoriesToken)
            {
                string name = property.Key;
                string description = property.Value.ToString();
                categories[name] = description;
            }
            Syntax.categories = categories;

            // DirectiveImplementations type for looking up methods
            Type impls = typeof(DirectiveImplementations);

            // parse directives
            var allDirectivesJSON = root["directives"] as JObject;
            Debug.Assert(allDirectivesJSON != null, "language.json/directives was null.");
            
            foreach(KeyValuePair<string, JToken> directiveJSON in allDirectivesJSON)
            {
                string identifier = directiveJSON.Key;
                var body = (JObject)directiveJSON.Value;
                
                string[] aliases = null;

                if (body.TryGetValue("aliases", out JToken aliasesToken))
                {
                    Debug.Assert(aliasesToken != null, $"language.json/directives/{identifier}/aliases was null.");
                    Debug.Assert(aliasesToken is JArray, $"language.json/directives/{identifier}/aliases was not an array.");
                    aliases = ((JArray)aliasesToken).Select(t => t.ToString()).ToArray();
                }

                JToken descriptionToken = body["description"];
                Debug.Assert(descriptionToken != null, $"language.json/directives/{identifier}/description was null.");
                string description = descriptionToken.Value<string>();
                
                JToken categoryToken = body["category"];
                Debug.Assert(categoryToken != null, $"language.json/directives/{identifier}/category was null.");
                string category = categoryToken.Value<string>();

                string wikiLink = null;
                JToken _wikiLink = body["wiki_link"];
                if (_wikiLink != null)
                    wikiLink = _wikiLink.Value<string>();
                
                string documentation = null;
                if (body.TryGetValue("details", out JToken detailsToken))
                {
                    Debug.Assert(detailsToken != null, $"language.json/directives/{identifier}/details was null.");
                    documentation = detailsToken.Value<string>();
                }

                string _function = identifier;
                if (body.TryGetValue("function", out JToken functionToken))
                {
                    Debug.Assert(functionToken != null, $"language.json/directives/{identifier}/function was null.");
                    _function = functionToken.Value<string>();
                }

                // collect all patterns
                var patterns = new List<TypePattern>();
                if(body.TryGetValue("patterns", out JToken patternsToken))
                {
                    Debug.Assert(patternsToken is JArray, $"language.json/directives/{identifier}/patterns was not an array.");
                    IEnumerable<JArray> patternsJSON = ((JArray)patternsToken)
                        .Select(token =>
                        {
                            Debug.Assert(token is JArray, $"language.json/directives/{identifier}/patterns contained an item that was not an array.");
                            return (JArray)token;
                        });

                    foreach(JArray patternJSON in patternsJSON)
                    {
                        string[] args = patternJSON.Select
                            (jt => jt.Value<string>()).ToArray();

                        var pattern = new TypePattern();

                        for (int i = 0; i < args.Length; i++)
                        {
                            string arg = args[i];
                            int colon = arg.IndexOf(':');
                            string _type, name;

                            // get argument typename and name
                            if (colon == -1)
                            {
                                name = "arg" + i;
                                _type = arg;
                            }
                            else
                            {
                                name = arg.Substring(colon + 1);
                                _type = arg.Substring(0, colon); 
                            }

                            // check if optional
                            bool optional = _type[0] == '?';
                            if (optional)
                                _type = _type.Substring(1);

                            // look through mappings for type
                            if(!mappings.TryGetValue(_type, out NamedType type))
                                throw new Exception($"Invalid type mapping '{_type}'.");

                            if (optional)
                                pattern.Optional(type, name);
                            else
                                pattern.And(type, name);
                        }

                        patterns.Add(pattern);
                    }
                }


                // find call function
                MethodInfo info = impls.GetMethod(_function, new[] {
                    typeof(Executor),
                    typeof(Statement)
                });
                
                Debug.Assert(info != null, $"Missing implementation for: DirectiveImplementations.{_function}(executor, statement)");
                
                // create delegate
                var function = (Directive.DirectiveImpl)Delegate
                    .CreateDelegate(typeof(Directive.DirectiveImpl), info);

                // construct directive
                var directive = new Directive(function, identifier, aliases, description,
                    documentation, wikiLink, category, patterns.ToArray());

                // attributes, if any
                if(body.TryGetValue("attributes", out JToken attributesToken))
                {
                    Debug.Assert(attributesToken != null, $"language.json/directives/{identifier}/attributes was null.");
                    Debug.Assert(attributesToken is JArray, $"language.json/directives/{identifier}/attributes was not an array.");

                    var array = (JArray)attributesToken;
                    IEnumerable<string> strings = array
                        .Select(jt => jt.Value<string>());
                    DirectiveAttribute[] attributes = strings
                        .Select(str =>
                        {
                            if (!attributeLookup.TryGetValue(str, out DirectiveAttribute a))
                                Debug.Assert(false, $"In language.json/directives/{identifier}/attributes: Attribute '{str}' is not a valid attribute.");
                            return a;
                        })
                        .ToArray();
                    directive.WithAttributes(attributes);
                }

                RegisterDirective(directive);
            }

            SortDirectives();
        }
    }
}
