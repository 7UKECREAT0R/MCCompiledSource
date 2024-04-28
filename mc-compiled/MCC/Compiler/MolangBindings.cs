using mc_compiled.Commands;
using mc_compiled.Modding.Behaviors;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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
        public readonly string[] targetFiles;    // The files that are targeted, by default, for this binding. Nullable.
        public readonly string description;      // A description of the query, used for documentation.

        /// <summary>
        /// Get the type that this binding falls under.
        /// </summary>
        public abstract BindingType Type { get; }
        /// <summary>
        /// Creates an array of <see cref="ControllerState"/>s that represent this binding.
        /// </summary>
        /// <param name="value">The scoreboard value to change.</param>
        /// <param name="callingStatement">The calling statement, for exceptions.</param>
        /// <param name="defaultState">[OUT] The default state to use.</param>
        /// <returns>An array of <see cref="ControllerState"/>s that represent this binding.</returns>
        public abstract ControllerState[] GetControllerStates(ScoreboardValue value, Statement callingStatement, out string defaultState);
        public override string ToString() => $"Binding ({this.Type}): \"{this.molangQuery}\" :: target(s): {(this.targetFiles == null ? "none" : string.Join(" ", this.targetFiles))} :: desc: {this.description}";
    }
    /// <summary>
    /// A definition describing how to bind to a Molang query that evaluates to a boolean.
    /// </summary>
    public sealed class MolangBindingBool : MolangBinding
    {
        public MolangBindingBool(string molangQuery, string[] targetFiles, string description)
            : base(molangQuery, targetFiles, description) { }
        
        public override BindingType Type => BindingType.boolean;
        public override ControllerState[] GetControllerStates(ScoreboardValue value, Statement callingStatement, out string defaultState)
        {
            defaultState = "off";

            return new[]
            {
                new ControllerState()
                {
                    name = "off",
                    onEntryCommands = new string[]
                    {
                       Command.ForJSON(Command.ScoreboardSet(value, 0))
                    },
                    transitions = new ControllerState.Transition[]
                    {
                        new ControllerState.Transition("on", (MolangValue)this.molangQuery)
                    }
                },
                new ControllerState()
                {
                    name = "on",
                    onEntryCommands = new string[]
                    {
                        Command.ForJSON(Command.ScoreboardSet(value, 1))
                    },
                    transitions = new ControllerState.Transition[]
                    {
                        new ControllerState.Transition("off", (MolangValue)$"!({this.molangQuery})")
                    }
                }
            };
        }
    }
        
    /// <summary>
    /// A definition describing how to bind to a Molang query that evaluates to a custom bool expression.
    /// </summary>
    public sealed class MolangBindingCustomBool : MolangBinding
    {
        private readonly string customQuery;
        
        public MolangBindingCustomBool(string molangQuery, string[] targetFiles, string description, string customQuery)
            : base(molangQuery, targetFiles, description)
        {
            this.customQuery = customQuery;
        }

        public override BindingType Type => BindingType.custom_bool;
        public override ControllerState[] GetControllerStates(ScoreboardValue value, Statement callingStatement, out string defaultState)
        {
            defaultState = "off";

            return new[]
            {
                new ControllerState()
                {
                    name = "off",
                    onEntryCommands = new[]
                    {
                        Command.ForJSON(Command.ScoreboardSet(value, 0))    
                    },
                    transitions = new[]
                    {
                        new ControllerState.Transition("on", (MolangValue)$"{this.customQuery}")
                    }
                },
                new ControllerState()
                {
                    name = "on",
                    onEntryCommands = new[]
                    {
                        Command.ForJSON(Command.ScoreboardSet(value, 1))
                    },
                    transitions = new[]
                    {
                        new ControllerState.Transition("off", (MolangValue)$"!({this.customQuery})")
                    }
                }
            };
        }
    }
    
    /// <summary>
    /// A definition describing how to bind to a Molang query that evaluates to an integer, within a given range.
    /// </summary>
    public sealed class MolangBindingInt : MolangBinding
    {
        private readonly int min;
        private readonly int max;

        public Range AsRange
        {
            get => new Range(this.min, this.max);
        }

        public MolangBindingInt(string molangQuery, string[] targetFiles, string description, int min, int max)
            : base(molangQuery, targetFiles, description)
        {
            if (min > max)
            {
                this.min = max;
                this.max = min;
            }
            else
            {
                this.min = min;
                this.max = max;
            }
        }
        public MolangBindingInt(string molangQuery, string[] targetFiles, string description, Range range)
            : base(molangQuery, targetFiles, description)
        {
            this.min = range.min.GetValueOrDefault();
            this.max = range.max.GetValueOrDefault();
        }

        public override BindingType Type => BindingType.integer;
        public override ControllerState[] GetControllerStates(ScoreboardValue value, Statement callingStatement, out string defaultState)
        {
            defaultState = "direct";
            int numberOfStates = this.max - this.min + 1;
            var states = new ControllerState[numberOfStates + 1]; // + 1 for the director
            var director = new ControllerState()
            {
                name = defaultState,
                transitions = new ControllerState.Transition[numberOfStates]
            };

            for (int i = 0; i < numberOfStates; i++)
            {
                int currentState = this.min + i;
                string stateName = "when_" + currentState;

                var state = new ControllerState()
                {
                    name = stateName,
                    onEntryCommands = new[]
                    {
                        Command.ForJSON(Command.ScoreboardSet(value, currentState))
                    },
                    transitions = new[]
                    {
                        new ControllerState.Transition(director, (MolangValue)$"{this.molangQuery} != {currentState}")
                    }
                };

                director.transitions[i] = new ControllerState.Transition(state, (MolangValue)$"{this.molangQuery} == {currentState}");
                states[i + 1] = state;
            }

            states[0] = director;
            return states;
        }
    }
    /// <summary>
    /// A definition describing how to bind to a Molang query that evaluates to an integer, within a given range.
    /// </summary>
    public sealed class MolangBindingFloat : MolangBinding
    {
        private readonly float min;
        private readonly float max;
        private readonly float step;

        public MolangBindingFloat(string molangQuery, string[] targetFiles, string description, float min, float max, float step)
            : base(molangQuery, targetFiles, description)
        {
            this.min = min;
            this.max = max;
            this.step = step;
        }

        public override BindingType Type => BindingType.floating_point;
        public override ControllerState[] GetControllerStates(ScoreboardValue value, Statement callingStatement, out string defaultState)
        {
            defaultState = "direct";

            float distance = this.max - this.min;
            float normalizedStep = distance / this.step;
            int numberOfStates = ((int)normalizedStep); // floor

            var states = new ControllerState[numberOfStates + 1]; // + 1 for the director
            var director = new ControllerState()
            {
                name = defaultState,
                transitions = new ControllerState.Transition[numberOfStates]
            };

            float Lerp(float t)
            {
                return (1 - t) * this.min + t * this.max;
            }

            for (int i = 0; i < numberOfStates; i++)
            {
                float currentValue = this.min + Lerp(i * normalizedStep);
                float lowerBound = currentValue - (normalizedStep / 2);
                float upperBound = currentValue + (normalizedStep / 2);

                string stateName = "state_" + i;

                var state = new ControllerState()
                {
                    name = stateName,

                    onEntryCommands = value.AssignLiteral(
                        new TokenDecimalLiteral((decimal)currentValue, callingStatement.Lines[0]), callingStatement
                    ).Select(Command.ForJSON).ToArray(),

                    transitions = new[]
                    {
                        new ControllerState.Transition(director,
                            (MolangValue)$"{this.molangQuery} < {lowerBound} && {this.molangQuery} >= {upperBound}")
                    }
                };

                director.transitions[i] = new ControllerState.Transition(state, (MolangValue)$"{this.molangQuery} >= {lowerBound} && {this.molangQuery} < {upperBound}");
                states[i + 1] = state;
            }

            states[0] = director;
            return states;
        }
    }

    public enum BindingType
    {
        /// <summary>
        /// A boolean.
        /// </summary>
        boolean,
        /// <summary>
        /// An integer with a minimum and maximum value. Will check every index between min and max inclusive.
        /// </summary>
        integer,
        floating_point,
        custom_bool
    }

    /// <summary>
    /// Manages the holding and loading of the language-defined <see cref="MolangBinding"/>s. Static.
    /// </summary>
    public static class MolangBindings
    {
        private static bool _isLoaded = false;

        public static string LAST_MC_VERSION = "unknown";
        public static readonly Dictionary<string, MolangBinding> BINDINGS = new Dictionary<string, MolangBinding>(StringComparer.OrdinalIgnoreCase);

        private static void Load(JObject json, bool debug)
        {
            int numberRegistered = 0;
            LAST_MC_VERSION = json["last_mc_version"].ToString();

            var bindingsDict = json["bindings"] as JObject;
            
            Debug.Assert(bindingsDict != null, "Field 'bindings' (at root) was missing in bindings.json");
            
            foreach(JProperty property in bindingsDict.Properties())
            {
                string query = property.Name;
                var body = property.Value as JObject;

                Debug.Assert(body != null, "Field 'body' (under 'bindings') was missing in bindings.json");

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
                    JArray casted = value.Value<JArray>();

                    if (casted != null)
                    {
                        targets = casted
                            .Select(jt => jt.ToString())
                            .ToArray();
                    }
                }

                MolangBinding binding;

                switch (type.ToUpper())
                {
                    case "BOOL":
                        binding = new MolangBindingBool(query, targets, desc);
                        break;
                    case "INT":
                        int min = (body["min"] ?? throw new Exception($"Missing field 'min' in binding '{query}'.")).Value<int>();
                        int max = (body["max"] ?? throw new Exception($"Missing field 'max' in binding '{query}'.")).Value<int>();
                        binding = new MolangBindingInt(query, targets, desc, min, max);
                        break;
                    case "FLOAT":
                        float minF = (body["min"] ?? throw new Exception($"Missing field 'min' in binding '{query}'.")).Value<float>();
                        float maxF = (body["max"] ?? throw new Exception($"Missing field 'max' in binding '{query}'.")).Value<float>();
                        float stepF = (body["step"] ?? throw new Exception($"Missing field 'step' in binding '{query}'.")).Value<float>();
                        binding = new MolangBindingFloat(query, targets, desc, minF, maxF, stepF);
                        break;
                    case "CUSTOM_BOOL":
                        string customQuery = (body["condition"] ?? throw new Exception($"Missing field 'condition' in binding '{query}'.")).Value<string>();
                        binding = new MolangBindingCustomBool(query, targets, desc, customQuery);
                        break;
                    default:
                        throw new Exception($"Invalid type '{type}' specified in binding '{query}'.");
                }

                // check if the query is already registered
                if(BINDINGS.TryGetValue(query, out _))
                    throw new Exception($"Duplicate binding '{query}'.");

                // add the binding to the cache
                BINDINGS[query] = binding;
                numberRegistered++;

                // display it to debug users
                if (debug)
                    Console.WriteLine("Loaded {0}", binding);
            }

            // display number of successes to debug users
            if (debug)
                Console.WriteLine("Successfully registered {0} molang bindings.", numberRegistered);
        }
        private static void Load(bool debug)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Debug.Assert(assemblyDir != null, "Application is in an inaccessible directory.");
            string path = Path.Combine(assemblyDir, Executor.BINDINGS_FILE);

            if (!File.Exists(path))
            {
                ConsoleColor colorPrevious = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WARNING: Missing bindings.json file at '{0}'. Execution cannot continue.", path);
                Console.ForegroundColor = colorPrevious;
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
