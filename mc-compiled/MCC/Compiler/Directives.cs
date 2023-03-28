using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        public Directive(DirectiveImpl call, string identifier, string[] aliases, string description, string documentation, params TypePattern[] patterns)
        {
            index = nextIndex++;
            this.call = call;
            this.identifier = identifier;
            this.aliases = aliases;
            this.description = description;
            this.documentation = documentation;
            this.patterns = patterns;

            // cache if this directive overlaps an enum
            if (Commands.CommandEnumParser.TryParse(identifier, out Commands.ParsedEnumValue result))
                enumValue = result;
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
                if (aliases != null && aliases.Length > 0)
                {
                    List<string> values = new List<string>() { identifier };
                    values.AddRange(aliases);
                    return values.ToArray();
                }

                return new string[] { identifier };
            }
        }
        public string DirectiveOverview
        {
            get
            {
                if (aliases != null && aliases.Length > 0)
                {
                    List<string> values = new List<string>() { identifier };
                    values.AddRange(aliases);
                    return string.Join("/", values);
                }

                return identifier;
            }
        }

        // Directive might overlap an enum value.
        // In the case this happens, it can use this field to help convert itself.
        public readonly Commands.ParsedEnumValue? enumValue;

        public readonly short index;
        public readonly string identifier;
        public readonly string[] aliases;
        public readonly string description;
        public readonly string documentation;
        public readonly DirectiveImpl call;
        public readonly TypePattern[] patterns;
        public DirectiveAttribute attributes;

        public override int GetHashCode() => identifier.GetHashCode();
        public override string ToString() =>
            $"{DirectiveOverview} - patterns: {patterns.Length} - desc: {description}";
    }
    /// <summary>
    /// Attributes used to modify how directive statements behave.
    /// </summary>
    [Flags]
    public enum DirectiveAttribute : int
    {
        DONT_EXPAND_PPV = 1 << 0,       // Won't expand any explicit PPV identifiers. Used in $macro to allow passing in parameters.
        DONT_FLATTEN_ARRAYS = 1 << 1,   // Won't attempt to flatten JSON arrays to their root values.
        DONT_RESOLVE_STRINGS = 1 << 2,  // Won't resolve PPV entries in string parameters.
        USES_FSTRING = 1 << 3,          // Reserved.
    }
    public static class Directives
    {
        // old hardcoded directives. uses language.json now
        /*
        public static List<Directive> REGISTRY = new List<Directive>(new[]
        {
            new Directive(DirectiveImplementations._var, "$var", "Set Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._inc, "$inc", "Increment Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._dec, "$dec", "Decrement Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._add, "$add", "Add to Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._sub, "$sub", "Subtract from Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._mul, "$mul", "Multiply with Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._div, "$div", "Divide Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._mod, "$mod", "Modulo Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._pow, "$pow", "Exponentiate Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._swap, "$swap", "Swap Preprocessor Variables",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._if, "$if", "Preprocessor If",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenCompare), typeof(IObjectable))),
            new Directive(DirectiveImplementations._else, "$else", "Preprocessor Else"),
            new Directive(DirectiveImplementations._repeat, "$repeat", "Preprocessor Repeat",
                new TypePattern(typeof(TokenIntegerLiteral)).Optional<TokenIdentifier>()),
            new Directive(DirectiveImplementations._log, "$log", "Preprocessor Log to Console",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations._macro, "$macro", "Define/Call Preprocessor Macro",
                new TypePattern(typeof(TokenIdentifier)))
                .WithAttribute(DirectiveAttribute.DONT_EXPAND_PPV),
            new Directive(DirectiveImplementations._include, "$include", "Include other File",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations._strfriendly, "$strfriendly", "Preprocessor String Friendly Name",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._strupper, "$strupper", "Preprocessor String Uppercase",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._strlower, "$strlower", "Preprocessor String Lowercase",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._sum, "$sum", "Preprocessor Array Sum",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._median, "$median", "Preprocessor Get Median",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._mean, "$mean", "Preprocessor Get Mean",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._iterate, "$iterate", "Iterate Preprocessor Array",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._get, "$get", "Preprocessor Get at Index",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIntegerLiteral), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._len, "$len", "Preprocessor Get Array Length",
                new TypePattern( typeof(TokenIdentifier),  typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._json, "$json", "Preprocessor Load JSON Value",
                new TypePattern(typeof(TokenStringLiteral), typeof(TokenIdentifier), typeof(TokenStringLiteral))),

            new Directive(DirectiveImplementations.mc, "mc", "Minecraft Command",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.select, "select", "Select Target",
                new TypePattern(typeof(TokenSelectorLiteral)),
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.globalprint, "globalprint", "Global Print",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.print, "print", "Print to Selected Entity",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.define, "define", "Define Variable",
                new TypePattern(typeof(TokenStringLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenStringLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIntegerLiteral), typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.init, "init", "Initialize Variable to 0",
                new TypePattern(typeof(TokenIdentifierValue)),
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.@if, "if", "If Directive",
                new TypePattern(typeof(TokenIdentifierValue)),
                new TypePattern(typeof(TokenIdentifierValue), typeof(TokenCompare), typeof(Token)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifierValue)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifierValue), typeof(TokenCompare), typeof(Token)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenStringLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenIntegerLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenStringLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifierEnum)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIntegerLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenStringLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenIntegerLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenStringLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenIdentifierEnum)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenIntegerLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral))),
            new Directive(DirectiveImplementations.@else, "else", "Else Directive"),
            new Directive(DirectiveImplementations.give, "give", "Give Item to Selected",
                new TypePattern(typeof(TokenStringLiteral)).Optional<TokenIntegerLiteral>().Optional<TokenIntegerLiteral>()),
            new Directive(DirectiveImplementations.tp, "tp", "Teleport Selected Entity",
                new TypePattern(typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral)),
                new TypePattern(typeof(TokenSelectorLiteral)),
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.tphere, "tphere", "Teleport Entity to Selected",
                new TypePattern(typeof(TokenSelectorLiteral)).Optional<TokenCoordinateLiteral>().Optional<TokenCoordinateLiteral>().Optional<TokenCoordinateLiteral>(),
                new TypePattern(typeof(TokenStringLiteral)).Optional<TokenCoordinateLiteral>().Optional<TokenCoordinateLiteral>().Optional<TokenCoordinateLiteral>()),
            new Directive(DirectiveImplementations.move, "move", "Move Selected Entity",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenNumberLiteral))),
            new Directive(DirectiveImplementations.face, "face", "Face Selected Entity",
                new TypePattern(typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral)),
                new TypePattern(typeof(TokenSelectorLiteral)),
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.facehere, "facehere", "Face Entity Towards Selected",
                new TypePattern(typeof(TokenSelectorLiteral)),
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.rotate, "rotate", "Rotate Selected Entity",
                new TypePattern(typeof(TokenIntegerLiteral)).Optional<TokenIntegerLiteral>()),
            new Directive(DirectiveImplementations.block, "block", "Place Block",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenStringLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenStringLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral)).Optional<TokenIntegerLiteral>()),
            new Directive(DirectiveImplementations.fill, "fill", "Fill Region of Blocks",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenStringLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral)),
                new TypePattern(typeof(TokenStringLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral))),
            new Directive(DirectiveImplementations.scatter, "scatter", "Scatter Region with Random Blocks",
                new TypePattern(typeof(TokenStringLiteral), typeof(TokenIntegerLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral),typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral)).Optional<TokenStringLiteral>()),
            new Directive(DirectiveImplementations.replace, "replace", "Replace Region of Blocks",
                new TypePattern(typeof(TokenStringLiteral)).Optional<TokenIntegerLiteral>().And<TokenCoordinateLiteral>().And<TokenCoordinateLiteral>().And<TokenCoordinateLiteral>().And<TokenCoordinateLiteral>().And<TokenCoordinateLiteral>().And<TokenCoordinateLiteral>().And<TokenStringLiteral>().Optional<TokenIntegerLiteral>()),
            new Directive(DirectiveImplementations.kill, "kill", "Kill Selected Entity",
                new TypePattern().Optional<TokenSelectorLiteral>(),
                new TypePattern().Optional<TokenStringLiteral>()),
            new Directive(DirectiveImplementations.remove, "remove", "Remove Selected Entity",
                new TypePattern().Optional<TokenSelectorLiteral>(),
                new TypePattern().Optional<TokenStringLiteral>()),
            new Directive(DirectiveImplementations.globaltitle, "globaltitle", "Show Global Title",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenStringLiteral)),
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.title, "title", "Show Title",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenStringLiteral)),
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.globalactionbar, "globalactionbar", "Show Global Action Bar",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral)),
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.actionbar, "actionbar", "Show Action Bar",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral)),
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.say, "say", "Say As Selected Entity",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.halt, "halt", "Halt Execution"),
            new Directive(DirectiveImplementations.damage, "damage", "Damage Selected Entity",
                new TypePattern(typeof(TokenIntegerLiteral)).Optional<TokenIdentifierEnum>().Optional<TokenSelectorLiteral>(),
                new TypePattern(typeof(TokenIntegerLiteral)).Optional<TokenIdentifierEnum>().Optional<TokenStringLiteral>(),
                new TypePattern(typeof(TokenIntegerLiteral)).Optional<TokenIdentifierEnum>().Optional<TokenCoordinateLiteral>().Optional<TokenCoordinateLiteral>().Optional<TokenCoordinateLiteral>()),
            new Directive(DirectiveImplementations.@null, "null", "Null Action",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenStringLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral)).Optional<TokenCoordinateLiteral>().Optional<TokenCoordinateLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenStringLiteral)),
                new TypePattern(typeof(TokenIdentifier))),

            new Directive(DirectiveImplementations.intent, "intent", "Allow Intent",
                new TypePattern(typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations.function, "function", "Define Function"),
            new Directive(DirectiveImplementations.@return, "return", "Set Return Value",
                new TypePattern(typeof(TokenIdentifierValue)),
                new TypePattern(typeof(TokenLiteral))),
            new Directive(DirectiveImplementations.@struct, "struct", "Define Struct",
                new TypePattern(typeof(TokenIdentifier))),
        });
        */

        public static Dictionary<string, DirectiveAttribute> attributeLookup;
        static Directives()
        {
            attributeLookup = new Dictionary<string, DirectiveAttribute>();

            foreach(object attrib in Enum.GetValues(typeof(DirectiveAttribute)))
                attributeLookup[attrib.ToString()] = (DirectiveAttribute)attrib;
        }

        public static List<Directive> REGISTRY = new List<Directive>();
        static readonly Dictionary<string, Directive> directiveLookup = new Dictionary<string, Directive>();

        public static IEnumerable<Directive> PreprocessorDirectives
        {
            get => REGISTRY.Where(directive => directive.identifier[0] == '$');
        }
        public static IEnumerable<Directive> RegularDirectives
        {
            get => REGISTRY.Where(directive => directive.identifier[0] != '$');
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
        /// <param name="directive"></param>
        public static void RegisterDirective(Directive directive)
        {
            REGISTRY.Add(directive);

            foreach(string dictKey in directive.DictKeys)
                directiveLookup[dictKey.ToUpper()] = directive;
        }

        const string FILE = "language.json";
        public static void LoadFromLanguage(bool debug)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(assemblyDir, FILE);
            if (!File.Exists(path))
            {
                ConsoleColor errprevious = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WARNING: Missing language.json file at '{0}'. Execution cannot continue.", path);
                Console.ForegroundColor = errprevious;
                Console.ReadLine();
                throw new Exception("missing language.json");
            }

            if(debug)
                Console.WriteLine("Parsing language.json...");

            string _json = File.ReadAllText(path);
            JObject json = JObject.Parse(_json);
            ReadJSON(json);

            if (debug)
            {
                Console.WriteLine("Parsed {0} directives from language.json:", REGISTRY.Count);
                foreach (Directive directive in REGISTRY)
                {
                    if (directive.enumValue.HasValue)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        string className = directive.enumValue.Value.enumType.Name;
                        Console.WriteLine($"\t\tOverlaps with {className}.{directive.identifier}:");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.WriteLine("\t{0}", directive.ToString());
                }
            }
        }
        /// <summary>
        /// Read all directives from the language.json root object.
        /// </summary>
        /// <param name="root"></param>
        public static void ReadJSON(JObject root)
        {
            const string IDENTIFIER_PREFIX = "mc_compiled.MCC.Compiler.";

            // read type mappings
            Dictionary<string, NamedType> mappings = new Dictionary<string, NamedType>();
            var properties = (root["mappings"] as JObject).Properties();
            foreach (var field in (root["mappings"] as JObject).Properties())
            {
                string key = field.Name;
                Type value = Type.GetType(IDENTIFIER_PREFIX + field.Value.ToString(), true, false);
                mappings[key] = new NamedType(value, key);
            }

            // DirectiveImplementations type for looking up methods
            Type impls = typeof(DirectiveImplementations);

            // parse directives
            var allDirectivesJSON = root["directives"] as JObject;
            foreach(var directiveJSON in allDirectivesJSON)
            {
                string identifier = directiveJSON.Key;
                JObject body = directiveJSON.Value as JObject;

                string[] aliases = null;
                if(body.ContainsKey("aliases"))
                    aliases = (body["aliases"] as JArray).Select(t => t.ToString()).ToArray();

                string description = body["description"].Value<string>();

                string documentation = null;
                if(body.ContainsKey("details"))
                    documentation = body["details"].Value<string>();

                string _function = identifier;
                if (body.ContainsKey("function"))
                    _function = body["function"].Value<string>();

                // collect all patterns
                List<TypePattern> patterns = new List<TypePattern>();
                if(body.ContainsKey("patterns"))
                {
                    IEnumerable<JArray> patternsJSON = (body["patterns"] as JArray)
                        .Select(token => token as JArray);

                    foreach(JArray patternJSON in patternsJSON)
                    {
                        string[] args = patternJSON.Select
                            (jt => jt.Value<string>()).ToArray();

                        TypePattern pattern = new TypePattern();

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
                var info = impls.GetMethod(_function, new[] {
                    typeof(Executor),
                    typeof(Statement)
                });

                // create delegate
                // if this is erroring, a language.json directive is missing its associated method
                var function = (Directive.DirectiveImpl)Delegate.CreateDelegate
                    (typeof(Directive.DirectiveImpl), info);

                // construct directive
                Directive directive = new Directive(function, identifier,
                    aliases, description, documentation, patterns.ToArray());

                // attributes, if any
                if(body.ContainsKey("attributes"))
                {
                    JArray array = body["attributes"] as JArray;
                    IEnumerable<string> strings = array
                        .Select(jt => jt.Value<string>());
                    DirectiveAttribute[] attributes = strings
                        .Select(str => attributeLookup[str])
                        .ToArray();
                    directive.WithAttributes(attributes);
                }

                RegisterDirective(directive);
            }

            return;
        }
    }
}
