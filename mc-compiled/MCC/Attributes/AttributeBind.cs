using System;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.Modding.Behaviors;
using mc_compiled.Modding;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.Attributes
{
    public class AttributeBind : IAttribute
    {
        private readonly MolangBinding binding;
        private readonly string[] givenTargets; // if the query has no targets.

        internal AttributeBind(MolangBinding binding, string[] givenTargets)
        {
            this.binding = binding;
            this.givenTargets = givenTargets;
        }
        public string GetDebugString() => this.givenTargets == null ? $"bind: [{this.binding}]" : $"bind to ({string.Join(", ", this.givenTargets)}): [{this.binding}]";

        public string GetCodeRepresentation()
        {
            if (this.givenTargets == null)
                return $"bind(\"{this.binding.molangQuery}\")";
            return $"bind(\"{this.binding.molangQuery}\", {string.Join(", ", this.givenTargets.Select(t => $"\"{t}\""))})";
        }

        public void OnAddedValue(ScoreboardValue value, Statement callingStatement)
        {
            switch(this.binding.Type)
            {
                case BindingType.boolean:
                case BindingType.custom_bool:
                    if(value.type.TypeEnum != ScoreboardManager.ValueType.BOOL)
                        throw new StatementException(callingStatement, $"Binding '{this.binding.molangQuery}' can only be applied to 'bool' values.");
                    break;
                case BindingType.integer:
                    if (value.type.TypeEnum != ScoreboardManager.ValueType.INT)
                        throw new StatementException(callingStatement, $"Binding '{this.binding.molangQuery}' can only be applied to 'int' values.");
                    break;
                case BindingType.floating_point:
                    // supports all values
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Executor executor = callingStatement.executor;

            // list of targets in the format "entity_name.json"
            string[] targets = this.binding.targetFiles ?? this.givenTargets ??
                throw new StatementException(callingStatement, $"Binding '{this.binding.molangQuery}' requires target entities to be specified.");

            if (executor.linting) // will fail if the executor is linting, because project name is not present.
                return;
            
            foreach (string entity in targets)
            {
                // use EntityBehavior to construct an accurate output location
                var givenEntity = new EntityBehavior(entity);
                JToken entityRoot = executor.Fetch(givenEntity, callingStatement);

                // create animation controller for driving the variable
                string driverName = value.Name.ToLower() + "_driver_" + value.Name.GetHashCode().ToString().Replace('-', '0');
                string scriptName = value.Name.ToLower() + "_controller_" + value.Name.GetHashCode().ToString().Replace('-', '0');

                var driver = new AnimationController(driverName);
                executor.AddExtraFile(driver);

                // catch cases where there are no targetFiles for the binding
                if (this.binding.targetFiles == null || this.binding.targetFiles.Length == 0)
                    throw new StatementException(callingStatement,
                        $"Binding '{this.binding.molangQuery}' requires target entities to be specified.");
                
                // the binding's implementation of the controller states is invoked here
                // we support bool, int, and float
                ControllerState[] states = this.binding.GetControllerStates(value, callingStatement, out string defaultState);
                driver.states.AddRange(states);
                driver.defaultState = defaultState;

                // implement the new AC into the entity file, but don't destroy any of the data
                var entityBase = entityRoot["minecraft:entity"] as JObject;
                var description = entityBase["description"] as JObject;

                JObject animations;
                if ((animations = description["animations"] as JObject) == null)
                    animations = new JObject();

                JObject scripts;
                if ((scripts = description["scripts"] as JObject) == null)
                    scripts = new JObject();

                JArray scriptsToAnimate;
                if ((scriptsToAnimate = scripts["animate"] as JArray) == null)
                    scriptsToAnimate = new JArray();

                animations[scriptName] = driver.Identifier;
                if(!scriptsToAnimate.Any(jt => jt.ToString().Equals(scriptName)))
                    scriptsToAnimate.Add(scriptName);

                scripts["animate"] = scriptsToAnimate;
                description["animations"] = animations;
                description["scripts"] = scripts;
                entityBase["description"] = description;
                entityRoot["minecraft:entity"] = entityBase;

                // since we don't have parsed entity data, just use a RawFile
                // to export the newly changed entity. it's hack, but it works
                string outputFile = executor.project.GetOutputFileLocationFull
                    (OutputLocation.b_ENTITIES, entity);
                var file = new RawFile(outputFile, (entityRoot as JObject)?.ToString() ?? $@"{{ ""error"": ""Bad entity file: {entity}"" }}");
                executor.OverwriteExtraFile(file);
            }
        }

        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement) => throw new StatementException(causingStatement, "Cannot apply attribute 'bind' to a function.");

        public void OnCalledFunction(RuntimeFunction function,
            List<string> commands, Executor executor, Statement statement) {}
    }
}
