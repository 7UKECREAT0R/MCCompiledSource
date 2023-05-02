using mc_compiled.Commands;
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
    /// A definition describing how to bind to a Molang query.
    /// </summary>
    public abstract class MolangBinding
    {
        protected MolangBinding(string molangQuery, string[] targetFiles, string description)
        {
            this.molangQuery = molangQuery;
            this.targetFiles = targetFiles;
            this.description = description;
        }

        public readonly string molangQuery;      // The Molang query to use for this binding.
        public readonly string[] targetFiles;    // The files that are targetted, by default, for this binding. Nullable.
        public readonly string description;      // A description of the query, used for documentation.

        public abstract BindingType Type { get; }
        public override string ToString() =>
$"Binding ({Type}): \"{molangQuery}\" :: target(s): {(targetFiles == null ? "none" : string.Join(" ", targetFiles))} :: desc: {description}";
    }
    /// <summary>
    /// A definition describing how to bind to a Molang query that evaluates to a boolean.
    /// </summary>
    public sealed class MolangBindingBool : MolangBinding
    {
        public MolangBindingBool(string molangQuery, string[] targetFiles, string description)
            : base(molangQuery, targetFiles, description) { }

        public override BindingType Type => BindingType.boolean;
    }
    /// <summary>
    /// A definition describing how to bind to a Molang query that evaluates to an integer, within a given range.
    /// </summary>
    public sealed class MolangBindingInt : MolangBinding
    {
        public readonly int min;
        public readonly int max;

        public Range AsRange
        {
            get => new Range(min, max);
        }

        public MolangBindingInt(string molangQuery, string[] targetFiles, string description, int min, int max)
            : base(molangQuery, targetFiles, description)
        {
            this.min = min;
            this.max = max;
        }
        public MolangBindingInt(string molangQuery, string[] targetFiles, string description, Range range)
            : base(molangQuery, targetFiles, description)
        {
            this.min = range.min.GetValueOrDefault();
            this.max = range.max.GetValueOrDefault();
        }

        public override BindingType Type => BindingType.integer;
    }
    public enum BindingType
    {
        boolean,
        integer
    }

    public static class MolangBindings
    {
        private static bool _isLoaded = false;

        public static string LAST_MC_VERSION = "unknown";
        public static readonly Dictionary<string, MolangBinding> BINDINGS = new Dictionary<string, MolangBinding>(StringComparer.OrdinalIgnoreCase);

        private static void Load(JObject json, bool debug)
        {
            int numberRegistered = 0;
            LAST_MC_VERSION = json["last_mc_version"].ToString();

            JObject bindingsDict = json["bindings"] as JObject;
            foreach(JProperty property in bindingsDict.Properties())
            {
                string query = property.Name.ToString();
                JObject body = property.Value as JObject;

                // get the type of the binding.
                string type;
                if(body.TryGetValue("type", out JToken _type))
                    type = _type.ToString();
                else
                {
                    if (debug)
                        Console.WriteLine("No type specified under binding '{0}'. Skipping.", query);
                    continue;
                }

                string desc = "No documentation attached.";
                string[] targets = null;

                // try to fetch description
                if(body.TryGetValue("desc", out JToken _desc))
                    desc = _desc.ToString();

                // try to fetch targets
                if (body.TryGetValue("targets", out JToken value))
                {
                    if (value == null)
                        continue;
                    targets = (value as JArray)
                        .Select(jt => jt.ToString())
                        .ToArray();
                }

                MolangBinding binding;

                switch (type.ToUpper())
                {
                    case "BOOL":
                        binding = new MolangBindingBool(query, targets, desc);
                        break;
                    case "INT":
                        int min = body["min"].Value<int>();
                        int max = body["max"].Value<int>();
                        binding = new MolangBindingInt(query, targets, desc, min, max);
                        break;
                    default:
                        if (debug)
                            Console.WriteLine("Invalid type '{0}' specified under binding '{1}'. Skipping.", type, query);
                        continue;
                }

                // check if the query is already registered
                if(BINDINGS.TryGetValue(query, out _))
                {
                    if (debug)
                        Console.WriteLine("Duplicate binding for molang query '{0}'. Skipping.", query);
                    continue;
                }

                // add the binding to the cache
                BINDINGS[query] = binding;
                numberRegistered++;

                // display it to debug users
                if (debug)
                    Console.WriteLine("Loaded {0}", binding.ToString());
            }

            // display number of successes to debug users
            if (debug)
                Console.WriteLine("Successfully registered {0} molang bindings.", numberRegistered);
        }
        private static void Load(bool debug)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(assemblyDir, Executor.BINDINGS_FILE);

            if (!File.Exists(path))
            {
                ConsoleColor errprevious = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WARNING: Missing bindings.json file at '{0}'. Execution cannot continue.", path);
                Console.ForegroundColor = errprevious;
                Console.ReadLine();
                throw new Exception("missing bindings.json");
            }

            if (debug)
                Console.WriteLine("Parsing bindings.json...");

            string _json = File.ReadAllText(path);
            JObject json = JObject.Parse(_json);
            Load(json, debug);
        }
        /// <summary>
        /// Ensure that the bindings file has been parsed and loaded. Does nothing if it has already been loaded.
        /// </summary>
        /// <param name="debug"></param>
        public static void EnsureLoaded(bool debug)
        {
            if (_isLoaded)
                return;

            Load(debug);
            _isLoaded = true;
        }


    }
}
