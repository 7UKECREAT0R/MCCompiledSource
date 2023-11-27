using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.CustomEntities;
using mc_compiled.Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            CommandFile file = new CommandFile(true, "uninstall", Executor.MCC_GENERATED_FOLDER);
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
                file.Add(Commands.Command.ScoreboardRemoveObjective(sb.InternalName));
            }

            foreach (string tag in parentExecutor.definedTags)
            {
                file.Add(Commands.Command.TagRemove($"@e[tag={tag}]", tag));

            }
        }

        /// <summary>
        /// Removes any files that duplicate/would overwrite this file.
        /// </summary>
        /// <param name="file"></param>
        internal void RemoveDuplicatesOf(IAddonFile file)
        {
            string outputFile = file.GetOutputFile();

            if (string.IsNullOrEmpty(outputFile))
                return;

            // compress to just the file names, we don't care
            // about directory included with the file name.
            string fileA = Path.GetFileName(file.GetOutputFile());
            string directoryA = file.GetExtendedDirectory();

            OutputLocation fileLocation = file.GetOutputLocation();

            for(int i = files.Count -1; i >= 0; i--)
            {
                IAddonFile test = files[i];
                string fileB = Path.GetFileName(test.GetOutputFile());
                string directoryB = test.GetExtendedDirectory();

                bool match = true;
                match &= fileA.Equals(fileB);
                match &= (directoryA == null) == (directoryB == null);

                if (match && directoryA != null && directoryB != null)
                    match &= directoryA.Equals(directoryB);

                if (test.GetOutputFile().Equals(file) && test.GetOutputLocation() == fileLocation)
                    files.RemoveAt(i);
            }
        }

        /// <summary>
        /// Attempts to add a file to this project, skipping the operation entirely if it's null.
        /// </summary>
        /// <param name="file"></param>
        internal void TryAddFile(IAddonFile file)
        {
            if (file == null)
                return;
            files.Add(file);
        }
        /// <summary>
        /// Adds a file to this project.
        /// </summary>
        /// <param name="file"></param>
        internal void AddFile(IAddonFile file) =>
            files.Add(file);
        /// <summary>
        /// Adds a collection of files to this project.
        /// </summary>
        /// <param name="files"></param>
        internal void AddFiles(IEnumerable<IAddonFile> files) =>
            this.files.AddRange(files);
        /// <summary>
        /// Returns if this project has any file name containing a set of text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal bool HasFileContaining(string text)
        {
            return files.Any(file => (file.GetOutputFile() ?? "").Contains(text));
        }
        /// <summary>
        /// Writes all files to the disk and clears the list.
        /// </summary>
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
            ApplyManifests();

            // actual writing
            foreach (IAddonFile file in files)
            {
                // log the write to console
                if(Program.DEBUG && file.GetOutputFile() != null)
                {
                    string partialPath = GetOutputFileLocationFull(file, true);
                    string fullPath = Path.GetFullPath(partialPath);
                    Console.WriteLine($"\t- File: {fullPath}");
                }

                // write it
                WriteSingleFile(file);
            }

            files.Clear();
        }

        /// <summary>
        /// Ensures the manifests are present, linked, and are correctly set up. Adds them to the <see cref="files"/> list.
        /// </summary>
        private void ApplyManifests()
        {
            string behaviorManifestLocation = Path.Combine(registry.bpBase, "manifest.json");
            string resourceManifestLocation = Path.Combine(registry.rpBase, "manifest.json");
            bool hasBehaviorManifest = File.Exists(behaviorManifestLocation);
            bool hasResourceManifest = File.Exists(resourceManifestLocation);
            bool needsBehaviorManifest = !hasBehaviorManifest & files.Any(file => file.GetOutputLocation().IsBehavior());
            bool needsResourceManifest = !hasResourceManifest & files.Any(file => !file.GetOutputLocation().IsBehavior());

            Manifest behaviorManifest = null;
            Manifest resourceManifest = null;

            // create/load behavior manifest
            if (hasBehaviorManifest)
            {
                string data = File.ReadAllText(behaviorManifestLocation);
                behaviorManifest = new Manifest(data, OutputLocation.b_ROOT);
            }
            else
            {
                if (needsBehaviorManifest)
                {
                    string projectDescription = "MCCompiled " + Executor.MCC_VERSION + " Project";
                    behaviorManifest = new Manifest(OutputLocation.b_ROOT, Guid.NewGuid(), name, projectDescription)
                        .WithModule(Manifest.Module.BehaviorData(name));
                }
            }

            // create/load resource manifest
            if (hasResourceManifest)
            {
                string data = File.ReadAllText(resourceManifestLocation);
                resourceManifest = new Manifest(data, OutputLocation.r_ROOT);
            }
            else
            {
                if (needsResourceManifest)
                {
                    string projectDescription = "MCCompiled " + Executor.MCC_VERSION + " Project - Resources";
                    resourceManifest = new Manifest(OutputLocation.r_ROOT, Guid.NewGuid(), name, projectDescription)
                        .WithModule(Manifest.Module.ResourceData(name));
                }
            }

            // link dependencies
            if (resourceManifest != null && behaviorManifest != null)
            {
                behaviorManifest.dependsOn = resourceManifest.uuid;
                resourceManifest.dependsOn = behaviorManifest.uuid;
            }

            TryAddFile(behaviorManifest);
            TryAddFile(resourceManifest);
        }

        /// <summary>
        /// Returns the full, exact output location of this IAddonFile in relation to this project.
        /// </summary>
        /// <param name="file">The file to get the full path of.</param>
        /// <param name="includeFileName">Whether to include the file name in the output, or just the directory.</param>
        /// <returns>null if the file should not be outputted.</returns>
        public string GetOutputFileLocationFull(IAddonFile file, bool includeFileName)
        {
            string outputFile = file.GetOutputFile();

            if (string.IsNullOrEmpty(outputFile))
                return null;

            OutputLocation baseLocation = file.GetOutputLocation();
            string folder = registry[baseLocation];
            string extend = file.GetExtendedDirectory();

            if (extend != null)
                folder = Path.Combine(folder, extend);

            if (includeFileName)
                folder = Path.Combine(folder, outputFile);

            return folder;
        }
        public string GetOutputFileLocationFullIDoNotCareIfYouDontWantToWriteIt(CommandFile file, bool includeFileName)
        {
            string outputFile = file.GetOutputFileIDoNotCareIfYouDontWantToWriteIt();

            if (string.IsNullOrEmpty(outputFile))
                return null;

            OutputLocation baseLocation = file.GetOutputLocation();
            string folder = registry[baseLocation];
            string extend = file.GetExtendedDirectoryIDoNotCareIfYouDontWantToWriteIt();

            if (extend != null)
                folder = Path.Combine(folder, extend);

            if (includeFileName)
                folder = Path.Combine(folder, outputFile);

            return folder;
        }
        /// <summary>
        /// Returns the full, exact output location of a file that is located under a specific <see cref="OutputLocation"/> in relation to this project.
        /// </summary>
        /// <param name="outputLocation">The OutputLocation of the theoretical file.</param>
        /// <param name="file">The file name and extension. If left null, only the directory will be returned.</param>
        /// <returns></returns>
        public string GetOutputFileLocationFull(OutputLocation outputLocation, string file = null)
        {
            string folder = registry[outputLocation];

            if (file == null)
                return folder;

            return Path.Combine(folder, file);
        }
        /// <summary>
        /// Returns the full, exact output location of a file that is located under a specific <see cref="OutputLocation"/> in relation to this project.
        /// </summary>
        /// <param name="outputLocation"></param>
        /// <param name="file">The file name and extension. If left null, only the directory will be returned.</param>
        /// <param name="extendedFolder">The extended directory. If left null, only the directory will be returned.</param>
        /// <returns></returns>
        public string GetOutputFileLocationFull(OutputLocation outputLocation, params string[] paths)
        {
            string folder = registry[outputLocation];

            // concat each path sequentially.
            foreach (string path in paths)
                folder = Path.Combine(folder, path);

            return folder;
        }
        /// <summary>
        /// Writes an addon file to disk right now, without waiting for the end of the compilation.
        /// </summary>
        /// <param name="file"></param>
        internal void WriteSingleFile(IAddonFile file)
        {
            if (linting)
                return;

            string output = GetOutputFileLocationFull(file, true);

            if (output == null)
                return; // file should not be written.

            // create folder if it doesn't exist
            string directory = Path.GetDirectoryName(output);
            if(!string.IsNullOrWhiteSpace(directory)) // folder directly at the root (e.g., manifest)
                Directory.CreateDirectory(directory);

            // write it
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

            // None
            registry[OutputLocation.NONE] = "";

            // BP Folders
            registry[OutputLocation.b_ROOT] = bpBase;
            registry[OutputLocation.b_ANIMATIONS] = Path.Combine(bpBase, "animations");
            registry[OutputLocation.b_ANIMATION_CONTROLLERS] = Path.Combine(bpBase, "animation_controllers");
            registry[OutputLocation.b_BLOCKS] = Path.Combine(bpBase, "blocks");
            registry[OutputLocation.b_BIOMES] = Path.Combine(bpBase, "biomes");
            registry[OutputLocation.b_DIALOGUE] = Path.Combine(bpBase, "dialogue");
            registry[OutputLocation.b_ENTITIES] = Path.Combine(bpBase, "entities");
            registry[OutputLocation.b_FEATURES] = Path.Combine(bpBase, "features");
            registry[OutputLocation.b_FEATURE_RULES] = Path.Combine(bpBase, "feature_rules");
            registry[OutputLocation.b_FUNCTIONS] = Path.Combine(bpBase, "functions");
            registry[OutputLocation.b_ITEMS] = Path.Combine(bpBase, "items");
            registry[OutputLocation.b_LOOT_TABLES] = Path.Combine(bpBase, "loot_tables");
            registry[OutputLocation.b_RECIPES] = Path.Combine(bpBase, "recipes");
            registry[OutputLocation.b_SCRIPTS__CLIENT] = Path.Combine(bpBase, "scripts", "client");
            registry[OutputLocation.b_SCRIPTS__SERVER] = Path.Combine(bpBase, "scripts", "server");
            registry[OutputLocation.b_SCRIPTS__GAMETESTS] = Path.Combine(bpBase, "scripts", "gametests");
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
            registry[OutputLocation.r_MODELS__ENTITY] = Path.Combine(rpBase, "models", "entity");
            registry[OutputLocation.r_MODELS__BLOCKS] = Path.Combine(rpBase, "models", "blocks");
            registry[OutputLocation.r_PARTICLES] = Path.Combine(rpBase, "particles");
            registry[OutputLocation.r_ITEMS] = Path.Combine(rpBase, "items");
            registry[OutputLocation.r_RENDER_CONTROLLERS] = Path.Combine(rpBase, "render_controllers");
            registry[OutputLocation.r_SOUNDS] = Path.Combine(rpBase, "sounds");
            registry[OutputLocation.r_TEXTS] = Path.Combine(rpBase, "texts");
            registry[OutputLocation.r_TEXTURES__ENVIRONMENT] = Path.Combine(rpBase, "textures", "environment");
            registry[OutputLocation.r_TEXTURES__BLOCKS] = Path.Combine(rpBase, "textures", "blocks");
            registry[OutputLocation.r_TEXTURES__ENTITY] = Path.Combine(rpBase, "textures", "entity");
            registry[OutputLocation.r_TEXTURES__ITEMS] = Path.Combine(rpBase, "textures", "items");
            registry[OutputLocation.r_TEXTURES__PARTICLE] = Path.Combine(rpBase, "textures", "particle");
            registry[OutputLocation.r_UI] = Path.Combine(rpBase, "ui");
        }
        internal string this[OutputLocation location] =>
            registry[location];
    }
}
