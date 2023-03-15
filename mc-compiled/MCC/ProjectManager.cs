using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.CustomEntities;
using mc_compiled.Modding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Holds data about the project and formats/writes files.
    /// </summary>
    internal class ProjectManager
    {
        /// <summary>
        /// An identifier used in this project's entities/assets.
        /// </summary>
        public string Identifier
        {
            get => name.ToLower().Replace(' ', '_').Trim();
        }
        /// <summary>
        /// Namespace an identifier for this project.
        /// <code>this.Identifier + ':' + name</code>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Namespace(string name) =>
            Identifier + ':' + name;

        internal readonly string name;
        internal string description = "placeholder";

        readonly Executor parentExecutor;
        readonly OutputRegistry registry;
        readonly List<IAddonFile> files;
        private Feature features;
        internal bool linting;

        /// <summary>
        /// Create a new ProjectManager with default description.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="bpBase">e.g: development_behavior_packs/project_name/</param>
        /// <param name="rpBase">e.g: development_resource_packs/project_name/</param>
        internal ProjectManager(string name, string bpBase, string rpBase, Executor parent)
        {
            this.parentExecutor = parent;
            this.name = name;
            registry = new OutputRegistry(bpBase, rpBase);
            files = new List<IAddonFile>();
            features = 0;
        }
        /// <summary>
        /// Returns this project manager after setting it to lint mode, lowering memory usage
        /// </summary>
        /// <returns></returns>
        internal ProjectManager Linter()
        {
            this.linting = true;
            return this;
        }

        /// <summary>
        /// Creates the uninstall file.
        /// </summary>
        internal void CreateUninstallFile()
        {
            CommandFile file = new CommandFile("uninstall", Executor.MCC_GENERATED_FOLDER);
            this.AddFile(file);

            if(HasFeature(Feature.DUMMIES))
            {
                // remove all dummies from the world.
                file.Add(Commands.Command.Event($"@e[type={parentExecutor.entities.dummies.dummyType}]", DummyManager.DESTROY_EVENT_NAME));
            }

            foreach (string temp in parentExecutor.scoreboard.temps.DefinedTemps)
                file.Add(Commands.Command.ScoreboardRemoveObjective(temp));

            foreach (ScoreboardValue sb in parentExecutor.scoreboard.values)
            {
                file.Add(Commands.Command.ScoreboardRemoveObjective(sb.Name));
            }

            foreach (string tag in parentExecutor.definedTags)
            {
                file.Add(Commands.Command.TagRemove("*", tag));
            }
        }

        internal void AddFile(IAddonFile file) =>
            files.Add(file);
        internal void AddFiles(IAddonFile[] files) =>
            this.files.AddRange(files);
        /// <summary>
        /// Returns if this project has any file name containing a set of text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal bool HasFileContaining(string text)
        {
            return files.Any(file => file.GetOutputFile().Contains(text));
        }
        internal void WriteAllFiles()
        {
            if (linting)
            {
                files.Clear();
                return;
            }

            // uninstall feature
            if(HasFeature(Feature.UNINSTALL))
                CreateUninstallFile();

            // manifests
            string behaviorManifestLocation = Path.Combine(registry.bpBase, "manifest.json");
            string resourceManifestLocation = Path.Combine(registry.rpBase, "manifest.json");
            bool hasBehaviorManifest = File.Exists(behaviorManifestLocation);
            bool hasResourceManifest = File.Exists(resourceManifestLocation);
            bool needsBehaviorManifest = !hasBehaviorManifest & files.Any(file => file.GetOutputLocation().IsBehavior());
            bool needsResourceManifest = !hasResourceManifest & files.Any(file => !file.GetOutputLocation().IsBehavior());

            // creating manifests
            if (needsBehaviorManifest || needsResourceManifest)
            {
                // 1st pass, load existing guids
                Guid? behaviorGuid = null;
                Guid? resourceGuid = null;

                if (!hasBehaviorManifest)
                {
                    if(needsBehaviorManifest)
                        behaviorGuid = Guid.NewGuid();
                }
                else
                {
                    string contents = File.ReadAllText(behaviorManifestLocation);
                    JObject json = JObject.Parse(contents);
                    string guid = json["header"]["uuid"].Value<string>();
                    behaviorGuid = Guid.Parse(guid);
                }

                if (!hasResourceManifest)
                {
                    if(needsResourceManifest)
                        resourceGuid = Guid.NewGuid();
                }
                else
                {
                    string contents = File.ReadAllText(resourceManifestLocation);
                    JObject json = JObject.Parse(contents);
                    string guid = json["header"]["uuid"].Value<string>();
                    resourceGuid = Guid.Parse(guid);
                }
                
                // 2nd pass, create manifests if needed.
                if (needsBehaviorManifest)
                {
                    string projectDescription = "MCCompiled " + Executor.MCC_VERSION + " Project";
                    AddFile(new Manifest(OutputLocation.b_ROOT, Guid.NewGuid(), name, projectDescription, dependsOn: resourceGuid)
                        .WithModule(Manifest.Module.BehaviorData(name)));
                }
                if (needsResourceManifest)
                {
                    string projectDescription = "MCCompiled " + Executor.MCC_VERSION + " Project";
                    AddFile(new Manifest(OutputLocation.r_ROOT, Guid.NewGuid(), name, projectDescription, dependsOn: behaviorGuid)
                        .WithModule(Manifest.Module.ResourceData(name)));
                }
            }

            // actual writing
            foreach (IAddonFile file in files)
                WriteSingleFile(file);
            files.Clear();
        }
        internal void WriteSingleFile(IAddonFile file)
        {
            if (linting)
                return;

            OutputLocation baseLocation = file.GetOutputLocation();
            string folder = registry[baseLocation];
            string extend = file.GetExtendedDirectory();

            if (extend != null)
                folder = Path.Combine(folder, extend);

            // create folder if it doesn't exist
            Directory.CreateDirectory(folder);

            // write it
            string output = Path.Combine(folder, file.GetOutputFile());
            File.WriteAllBytes(output, file.GetOutputData());
        }

        /// <summary>
        /// Enable a feature for this project.
        /// </summary>
        /// <param name="feature"></param>
        internal void EnableFeature(Feature feature) =>
            features |= feature;
        /// <summary>
        /// Check if this project has a feature enabled.
        /// </summary>
        /// <param name="feature"></param>
        internal bool HasFeature(Feature feature) =>
            (features &= feature) != Feature.NO_FEATURES;
    }
    /// <summary>
    /// Generates and holds a "registry" for directing file outputs.
    /// </summary>
    internal struct OutputRegistry
    {
        internal readonly string bpBase; // e.g: development_behavior_packs/project_name/
        internal readonly string rpBase; // e.g: development_resource_packs/project_name/
        readonly Dictionary<OutputLocation, string> registry;

        internal OutputRegistry(string bpBase, string rpBase)
        {
            this.bpBase = bpBase;
            this.rpBase = rpBase;
            registry = new Dictionary<OutputLocation, string>();

            // BP Folders
            registry[OutputLocation.b_ROOT] = bpBase;
            registry[OutputLocation.b_ANIMATIONS] = Path.Combine(bpBase, "animations");
            registry[OutputLocation.b_ANIMATION_CONTROLLERS] = Path.Combine(bpBase, "animation_controllers");
            registry[OutputLocation.b_BLOCKS] = Path.Combine(bpBase, "blocks");
            registry[OutputLocation.b_BIOMES] = Path.Combine(bpBase, "biomes");
            registry[OutputLocation.b_ENTITIES] = Path.Combine(bpBase, "entities");
            registry[OutputLocation.b_FEATURES] = Path.Combine(bpBase, "features");
            registry[OutputLocation.b_FEATURE_RULES] = Path.Combine(bpBase, "feature_rules");
            registry[OutputLocation.b_FUNCTIONS] = Path.Combine(bpBase, "functions");
            registry[OutputLocation.b_ITEMS] = Path.Combine(bpBase, "items");
            registry[OutputLocation.b_LOOT_TABLES] = Path.Combine(bpBase, "loot_tables");
            registry[OutputLocation.b_RECIPES] = Path.Combine(bpBase, "recipes");
            registry[OutputLocation.b_SCRIPTS_CLIENT] = Path.Combine(bpBase, "scripts", "client");
            registry[OutputLocation.b_SCRIPTS_SERVER] = Path.Combine(bpBase, "scripts", "server");
            registry[OutputLocation.b_SCRIPTS_GAMETESTS] = Path.Combine(bpBase, "scripts", "gametests");
            registry[OutputLocation.b_SPAWN_RULES] = Path.Combine(bpBase, "spawn_rules");
            registry[OutputLocation.b_TEXTS] = Path.Combine(bpBase, "texts");
            registry[OutputLocation.b_TRADING] = Path.Combine(bpBase, "trading");
            registry[OutputLocation.b_STRUCTURES] = Path.Combine(bpBase, "structures");

            // RP Folders
            registry[OutputLocation.r_ROOT] = rpBase;
            registry[OutputLocation.r_ANIMATION_CONTROLLERS] = Path.Combine(rpBase, "animation_controllers");
            registry[OutputLocation.r_ANIMATIONS] = Path.Combine(rpBase, "animations");
            registry[OutputLocation.r_ATTACHABLES] = Path.Combine(rpBase, "attachables");
            registry[OutputLocation.r_ENTITY] = Path.Combine(rpBase, "entity");
            registry[OutputLocation.r_FOGS] = Path.Combine(rpBase, "fogs");
            registry[OutputLocation.r_MODELS_ENTITY] = Path.Combine(rpBase, "models", "entity");
            registry[OutputLocation.r_MODELS_BLOCKS] = Path.Combine(rpBase, "models", "blocks");
            registry[OutputLocation.r_PARTICLES] = Path.Combine(rpBase, "particles");
            registry[OutputLocation.r_ITEMS] = Path.Combine(rpBase, "items");
            registry[OutputLocation.r_RENDER_CONTROLLERS] = Path.Combine(rpBase, "render_controllers");
            registry[OutputLocation.r_SOUNDS] = Path.Combine(rpBase, "sounds");
            registry[OutputLocation.r_TEXTS] = Path.Combine(rpBase, "texts");
            registry[OutputLocation.r_TEXTURES_ENVIRONMENT] = Path.Combine(rpBase, "textures", "environment");
            registry[OutputLocation.r_TEXTURES_BLOCKS] = Path.Combine(rpBase, "textures", "blocks");
            registry[OutputLocation.r_TEXTURES_ENTITY] = Path.Combine(rpBase, "textures", "entity");
            registry[OutputLocation.r_TEXTURES_ITEMS] = Path.Combine(rpBase, "textures", "items");
            registry[OutputLocation.r_TEXTURES_PARTICLE] = Path.Combine(rpBase, "textures", "particle");
            registry[OutputLocation.r_UI] = Path.Combine(rpBase, "ui");
        }
        internal string this[OutputLocation location] =>
            registry[location];
    }
}
