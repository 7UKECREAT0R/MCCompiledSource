﻿using mc_compiled.MCC.Compiler;
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
        readonly MolangBinding binding;
        readonly string[] givenTargets; // if the query has no targets.

        internal AttributeBind(MolangBinding binding, string[] givenTargets)
        {
            this.binding = binding;
            this.givenTargets = givenTargets;
        }
        public string GetDebugString() => "bind";

        public void OnAddedValue(ScoreboardValue value, Statement callingStatement)
        {
            switch(binding.Type)
            {
                case BindingType.boolean:
                    if(!(value is ScoreboardValueBoolean))
                        throw new StatementException(callingStatement, $"Binding '{binding.molangQuery}' can only be applied to 'bool' values.");
                    break;
                case BindingType.integer:
                    if (!(value is ScoreboardValueInteger))
                        throw new StatementException(callingStatement, $"Binding '{binding.molangQuery}' can only be applied to 'int' values.");
                    break;
                case BindingType.floating_point:
                    // supports all values
                    break;

            }

            Executor executor = callingStatement.executor;

            // list of targets in the format "entity_name.json"
            string[] targets =
                binding.targetFiles ??
                givenTargets ??
                throw new StatementException(callingStatement, $"Binding '{binding.molangQuery}' requires target entities to be specified.");

            if (!executor.linting) // will fail if the executor is linting, because project name is not present.
            {
                foreach (string entity in targets)
                {
                    // use EntityBehavior to construct an accurate output location
                    EntityBehavior givenEntity = new EntityBehavior(entity);
                    JToken entityRoot = executor.Fetch(givenEntity, callingStatement);

                    // create animation controller for driving the variable
                    string driverName = value.Name.ToLower() + "_driver";
                    string scriptName = value.Name.ToLower() + "_controller";

                    AnimationController driver = new AnimationController(driverName);
                    executor.AddExtraFile(driver);

                    // the binding's implementation of the controller states is invoked here
                    // we support bool, int, and float
                    ControllerState[] states = this.binding.GetControllerStates(value, callingStatement, out string defaultState);
                    driver.states.AddRange(states);
                    driver.defaultState = defaultState;

                    // implement the new AC into the entity file, but don't destroy any of the data
                    JObject entityBase = entityRoot["minecraft:entity"] as JObject;
                    JObject description = entityBase["description"] as JObject;

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
                    RawFile file = new RawFile(outputFile, (entityRoot as JObject).ToString());
                    executor.OverwriteExtraFile(file);
                }
            }
        }

        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement) => throw new StatementException(causingStatement, "Cannot apply attribute 'bind' to a function.");
        public void OnCalledFunction(RuntimeFunction function, List<string> commandBuffer, Executor executor, Statement statement) { }
    }
}
