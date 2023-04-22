using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.Modding.Behaviors;
using mc_compiled.Modding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mc_compiled.Commands;

namespace mc_compiled.MCC.Attributes
{
    public class AttributeBind : IAttribute
    {
        public string query;
        public string entity;

        internal AttributeBind(string query, string entity)
        {
            this.query = query;
            this.entity = entity;
        }
        public string GetDebugString() => "bind";

        public void OnAddedValue(ScoreboardValue value, Statement causingStatement)
        {
            if (value.valueType != ScoreboardManager.ValueType.BOOL)
                throw new StatementException(causingStatement, "Bindings can only be applied to 'bool' values.");

            Executor executor = causingStatement.executor;

            // use EntityBehavior to construct an accurate output location
            EntityBehavior givenEntity = new EntityBehavior(entity);
            string outputFile = executor.project.GetOutputFileLocationFull(givenEntity, true);
            int outputFileHash = outputFile.GetHashCode();

            // the JSON of the entity file to write to
            JToken entityRoot;

            // find the user-defined entity file, or default to the vanilla pack provided by Microsoft
            if (executor.loadedFiles.TryGetValue(outputFileHash, out object jValue))
                entityRoot = jValue as JToken;
            else
            {
                if (System.IO.File.Exists(outputFile))
                    entityRoot = executor.LoadJSONFile(outputFile, null);
                else
                {
                    string[] filePath = new[] { "entities", entity + ".json" };
                    string downloadedFile = DefaultPackManager.Get(DefaultPackManager.PackType.BehaviorPack, filePath);
                    entityRoot = executor.LoadJSONFile(downloadedFile, outputFileHash, null);
                }
            }

            // create animation controller for driving the variable
            string driverName = value.Name.ToLower() + "_driver";
            string scriptName = value.Name.ToLower() + "_controller";

            AnimationController driver = new AnimationController(driverName);
            executor.AddExtraFile(driver);

            driver.states.Add(new ControllerState()
            {
                name = "off",
                onEntryCommands = new string[]
                {
                    '/' + Command.ScoreboardSet(value, 0)
                },
                transitions = new ControllerState.Transition[]
                {
                    new ControllerState.Transition("on", new MolangValue(query))
                }
            });
            driver.states.Add(new ControllerState()
            {
                name = "on",
                onEntryCommands = new string[]
                {
                    '/' + Command.ScoreboardSet(value, 1)
                },
                transitions = new ControllerState.Transition[]
                {
                    new ControllerState.Transition("off", new MolangValue($"!({query})"))
                }
            });

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
            scriptsToAnimate.Add(scriptName);

            scripts["animate"] = scriptsToAnimate;
            description["animations"] = animations;
            description["scripts"] = scripts;
            entityBase["description"] = description;
            entityRoot["minecraft:entity"] = entityBase;

            RawFile file = new RawFile(outputFile, (entityRoot as JObject).ToString());
            executor.OverwriteExtraFile(file);
        }

        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement) => throw new StatementException(causingStatement, "Cannot apply attribute 'bind' to a function.");
        public void OnCalledFunction(RuntimeFunction function, List<string> commandBuffer, Executor executor, Statement statement) { }
    }
}
