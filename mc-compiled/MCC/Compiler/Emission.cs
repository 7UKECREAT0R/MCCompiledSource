using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Scheduling;
using mc_compiled.MCC.Scheduling.Implementations;
using mc_compiled.Modding;
using mc_compiled.Modding.Manifest;
using mc_compiled.Modding.Manifest.Modules;
using mc_compiled.Modding.Resources.Localization;
using Newtonsoft.Json.Linq;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     The full results of a file compilation performed by an <see cref="Executor" />.
/// </summary>
/// <remarks>
///     This class is constructed over the course of the runtime of an <see cref="Executor.Execute()" /> call.
///     It wouldn't really make sense to create one yourself.
/// </remarks>
public class Emission
{
    private readonly HashSet<CopyFile> filesToCopy;
    private readonly List<IAddonFile> filesToWrite;
    private readonly OutputRegistry outputRegistry;

    private readonly string projectName;

    internal Manifest behaviorManifest;
    private Function[] definedFunctions;
    private Macro[] definedMacros;
    private string[] definedPPVs;
    private ScoreboardValue[] definedValues;
    private Feature enabledFeatures;
    /// <summary>
    ///     A reference to the root file that was created alongside the <see cref="parentExecutor" />.
    ///     Will continue to live after <see cref="Complete" />tion.
    /// </summary>
    private CommandFile headFile;

    private bool isCompleted;
    /// <summary>
    ///     Is this Emission part of a cut-down compilation as a result of linting?
    /// </summary>
    public bool isLinting;
    private Executor parentExecutor;
    internal Manifest resourceManifest;
    /// <summary>
    ///     The tick scheduler that was created alongside the <see cref="parentExecutor" />.
    ///     Will continue to live after <see cref="Complete" />tion.
    /// </summary>
    /// <returns></returns>
    private TickScheduler scheduler;
    /// <summary>
    ///     Create a new <see cref="Emission" /> with the base configuration.
    /// </summary>
    /// <param name="projectName">The name of the project.</param>
    /// <param name="bpBase">The behavior pack root folder for the contained project.</param>
    /// <param name="rpBase">The resource pack root folder for the contained project.</param>
    /// <param name="parent">The parent executor.</param>
    /// <remarks>
    ///     The <paramref name="bpBase" /> and <paramref name="rpBase" /> parameters are required to route the output
    ///     files to their correct locations.
    /// </remarks>
    internal Emission(string projectName, string bpBase, string rpBase, Executor parent)
    {
        this.filesToCopy = [];
        this.filesToWrite = [];
        this.projectName = projectName;
        this.parentExecutor = parent;
        this.enabledFeatures = 0;
        this.outputRegistry = new OutputRegistry(bpBase, rpBase);
    }

    public IReadOnlyCollection<Function> DefinedFunctions => this.definedFunctions;
    public IReadOnlyCollection<Macro> DefinedMacros => this.definedMacros;
    public IReadOnlyCollection<string> DefinedPPVs => this.definedPPVs;
    public IReadOnlyCollection<ScoreboardValue> DefinedValues => this.definedValues;

    /// <summary>
    ///     An identifier to use in this emission's entities/assets.
    /// </summary>
    public string Identifier => this.projectName.ToLower().Replace(' ', '_').Trim();
    /// <summary>
    ///     Namespace an identifier with this project's identifier.
    ///     <code>this.Identifier + ':' + name</code>
    /// </summary>
    /// <param name="rawName"></param>
    /// <returns></returns>
    public string Namespace(string rawName) { return this.Identifier + ':' + rawName; }

    /// <summary>
    ///     Ensures the manifests are present, linked, and are correctly set up. Adds them to the <see cref="filesToWrite" />
    ///     list.
    /// </summary>
    private void ProcessManifests()
    {
        string behaviorManifestLocation = Path.Combine(this.outputRegistry.bpBase, "manifest.json");
        string resourceManifestLocation = Path.Combine(this.outputRegistry.rpBase, "manifest.json");
        bool hasBehaviorManifest = File.Exists(behaviorManifestLocation);
        bool hasResourceManifest = File.Exists(resourceManifestLocation);
        bool needsBehaviorManifest =
            !hasBehaviorManifest & this.filesToWrite.Any(file => file.GetOutputLocation().IsBehavior());
        bool needsResourceManifest =
            !hasResourceManifest & this.filesToWrite.Any(file => !file.GetOutputLocation().IsBehavior());

        // create/load behavior manifest
        if (hasBehaviorManifest)
        {
            string data = File.ReadAllText(behaviorManifestLocation);
            JObject json = JObject.Parse(data);
            this.behaviorManifest = Manifest.Parse(json, ManifestType.BP, this.projectName);
        }
        else
        {
            if (needsBehaviorManifest)
                this.behaviorManifest = new Manifest(ManifestType.BP, this.projectName)
                    .WithModule(new BasicModule(ModuleType.data));
        }

        // create/load resource manifest
        if (hasResourceManifest)
        {
            string data = File.ReadAllText(resourceManifestLocation);
            JObject json = JObject.Parse(data);
            this.resourceManifest = Manifest.Parse(json, ManifestType.RP, this.projectName);
        }
        else
        {
            if (needsResourceManifest)
                this.resourceManifest = new Manifest(ManifestType.RP, this.projectName)
                    .WithModule(new BasicModule(ModuleType.resources));
        }

        // link dependencies if both packs exist
        if (this.resourceManifest != null && this.behaviorManifest != null)
        {
            this.behaviorManifest.DependOn(this.resourceManifest);
            this.resourceManifest.DependOn(this.behaviorManifest);
        }

        // add to output.
        AddFile(this.behaviorManifest);
        AddFile(this.resourceManifest);

        // localize them if necessary
        if (this.parentExecutor.HasLocale)
            LocalizeManifests(this.parentExecutor.ActiveLocale.file);
    }
    /// <summary>
    ///     Localizes the manifests which are loaded and valid.
    /// </summary>
    /// <param name="_langFile">The lang file to store the <c>pack.name</c> and <c>pack.description</c> keys in.</param>
    private void LocalizeManifests(Lang _langFile)
    {
        const string localizedName = "pack.name";
        const string localizedDescription = "pack.description";

        if (this.behaviorManifest != null)
        {
            // the BP is a special case because it needs its own languages.json and Lang file generated.
            var bpLangManager = new LanguageManager(this.parentExecutor, true);
            AddFile(bpLangManager);
            Lang bpLang = bpLangManager.DefineLocale(this.parentExecutor.ActiveLocale.locale).file;
            TryLocalizeManifestFile(this.behaviorManifest.header, bpLang);
        }

        if (this.resourceManifest != null)
            TryLocalizeManifestFile(this.resourceManifest.header, _langFile);

        return;

        void TryLocalizeManifestFile(ManifestHeader header, Lang langFile)
        {
            if (!header.description.Equals(localizedDescription))
            {
                string oldDescription = header.description;
                header.description = localizedDescription;

                var entry = LangEntry.Create(localizedDescription, oldDescription);

                int indexOfExisting = langFile.IndexOf(localizedDescription);
                if (indexOfExisting == -1)
                    langFile.InsertAtIndex(0, entry);
                else
                    langFile.SetAtIndex(indexOfExisting, entry);
            }

            // ReSharper disable once InvertIf
            if (!header.name.Equals(localizedName))
            {
                string oldName = header.name;
                header.name = localizedName;

                var entry = LangEntry.Create(localizedName, oldName);

                int indexOfExisting = langFile.IndexOf(localizedName);
                if (indexOfExisting == -1)
                    langFile.InsertAtIndex(0, entry);
                else
                    langFile.SetAtIndex(indexOfExisting, entry);
            }
        }
    }

    /// <summary>
    ///     Removes any files that duplicate/would overwrite this file.
    /// </summary>
    /// <param name="file"></param>
    internal void RemoveDuplicatesOf(IAddonFile file)
    {
        string outputFile = file.GetOutputFile();

        if (string.IsNullOrEmpty(outputFile))
            return;

        OutputLocation fileLocation = file.GetOutputLocation();

        for (int i = this.filesToWrite.Count - 1; i >= 0; i--)
        {
            IAddonFile test = this.filesToWrite[i];

            if (test.GetOutputFile().Equals(outputFile) && test.GetOutputLocation() == fileLocation)
                this.filesToWrite.RemoveAt(i);
        }
    }

    /// <summary>
    ///     Registers a file to be emitted.
    /// </summary>
    /// <param name="file">The file to add. If <c>null</c>, nothing will happen.</param>
    public void AddFile([CanBeNull] IAddonFile file)
    {
        if (file == null)
            return;

        if (file is CopyFile copyFile)
        {
            this.filesToCopy.Add(copyFile);
            return;
        }

        this.filesToWrite.Add(file);
    }
    /// <summary>
    ///     Registers the given files to be emitted.
    /// </summary>
    /// <param name="files">The files to be registered.</param>
    public void AddFiles(IEnumerable<IAddonFile> files)
    {
        foreach (IAddonFile file in files)
            AddFile(file);
    }

    /// <summary>
    ///     Returns the full, exact output location of this IAddonFile in relation to the project.
    /// </summary>
    /// <param name="file">The file to get the full path of.</param>
    /// <param name="includeFileName">Whether to include the file name in the output, or just the directory.</param>
    /// <returns>null if the file shouldn’t be outputted.</returns>
    public string GetOutputFileLocationFull(IAddonFile file, bool includeFileName)
    {
        string outputFile = file.GetOutputFile();

        if (string.IsNullOrEmpty(outputFile))
            return null;

        OutputLocation baseLocation = file.GetOutputLocation();
        string folder = this.outputRegistry[baseLocation];
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
        string folder = this.outputRegistry[baseLocation];
        string extend = file.GetExtendedDirectoryIDoNotCareIfYouDontWantToWriteIt();

        if (extend != null)
            folder = Path.Combine(folder, extend);

        if (includeFileName)
            folder = Path.Combine(folder, outputFile);

        return folder;
    }
    /// <summary>
    ///     Returns the full, exact output location of a file that is located under a specific <see cref="OutputLocation" /> in
    ///     relation to this project.
    /// </summary>
    /// <param name="outputLocation">The OutputLocation of the theoretical file.</param>
    /// <param name="file">The file name and extension. If left null, only the directory will be returned.</param>
    /// <returns></returns>
    public string GetOutputFileLocationFull(OutputLocation outputLocation, string file = null)
    {
        string folder = this.outputRegistry[outputLocation];
        return file == null ? folder : Path.Combine(folder, file);
    }

    /// <summary>
    ///     Returns the full, exact output location of a file that is located under a specific <see cref="OutputLocation" /> in
    ///     relation to this project.
    /// </summary>
    /// <param name="outputLocation"></param>
    /// <param name="paths">The path to follow after the given output location.</param>
    /// <returns></returns>
    public string GetOutputFileLocationFull(OutputLocation outputLocation, params string[] paths)
    {
        string folder = this.outputRegistry[outputLocation];
        return paths.Aggregate(folder, Path.Combine);
    }

    /// <summary>
    ///     Writes an addon file to disk right now, without waiting for the end of the compilation.
    /// </summary>
    /// <param name="file"></param>
    internal void WriteSingleFile(IAddonFile file)
    {
        if (this.isLinting)
            return;

        string output = GetOutputFileLocationFull(file, true);

        if (output == null)
            return; // file should not be written.

        // create folder if it doesn't exist
        string directory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(directory)) // folder directly at the root (e.g., manifest)
            Directory.CreateDirectory(directory);

        // if CopyFile, just copy the file directly
        if (file is CopyFile copyFile)
        {
            string source = copyFile.sourceFile;
            File.Copy(source, output, true);
            return;
        }

        // write it
        File.WriteAllBytes(output, file.GetOutputData());
    }
    /// <summary>
    ///     Pulls all necessary emission information from <see cref="parentExecutor" /> and releases it for garbage collection,
    ///     essentially "completing" this emission instance.
    /// </summary>
    /// <remarks>
    ///     This method can only be called once per instance. If called a second time, nothing will happen.
    /// </remarks>
    public void Complete()
    {
        if (this.isCompleted)
            return;

        // we only want to build these files if they're actually going to be written in the end
        if (!this.isLinting)
        {
            // get manifests
            if (!GlobalContext.Current.ignoreManifests)
                ProcessManifests();

            // uninstall feature
            if (HasFeature(Feature.UNINSTALL))
                CreateUninstallFile();
            if (HasFeature(Feature.AUTOINIT))
                CreateAutoInitFile();

            // async
            this.parentExecutor.async.TryBuildTickFile();
        }

        this.definedFunctions = this.parentExecutor.functions.FetchAll().ToArray();
        this.definedMacros = this.parentExecutor.macros.ToArray();
        this.definedPPVs = this.parentExecutor.PPVNames.ToArray();
        this.definedValues = this.parentExecutor.scoreboard.values.ToArray();
        this.headFile = this.parentExecutor.HeadFile;
        this.scheduler = this.parentExecutor.scheduler;
        this.parentExecutor.Cleanup();
        this.parentExecutor = null;
        this.isCompleted = true;
    }
    /// <summary>
    ///     Writes all emitted files to disk.
    /// </summary>
    internal void WriteAllFiles()
    {
        if (this.isLinting)
            return;
        if (!this.isCompleted)
            throw new InvalidOperationException(
                "Attempted to write files before Emission was completed. Please call `.Complete()` first!");

        // actual writing
        foreach (IAddonFile rawFile in this.filesToWrite)
        {
            if (rawFile is CommandFile commandFile)
            {
                // file only contains comments
                bool fileNotInUse = !commandFile.IsInUse;
                bool fileIsHead = ReferenceEquals(commandFile, this.headFile);
                bool fileCanBeOmitted = fileNotInUse & fileIsHead;
                bool fileContainsOnlyComments = commandFile.commands.TrueForAll(c => c.StartsWith('#'));
                if (fileContainsOnlyComments && fileCanBeOmitted)
                    continue; // skip the file; no reason to write it, and nothing's depending on it existing.

                // traces
                bool isDirectlyFromTickJson = this.scheduler != null &&
                                              this.scheduler.IsFileAuto(commandFile);
                if (GlobalContext.Current.trace && !isDirectlyFromTickJson)
                {
                    commandFile.AddTop("");
                    commandFile.AddTop(Command.Tellraw(Selector.ALL_PLAYERS.ToString(),
                        new RawTextJsonBuilder().AddTerm(new JSONText($"[TRACE] > {commandFile.CommandReference}"))
                            .Build()));
                }
            }

            // log the write to console
            if (GlobalContext.Debug && rawFile.GetOutputFile() != null)
            {
                string partialPath = GetOutputFileLocationFull(rawFile, true);
                string fullPath = Path.GetFullPath(partialPath);
                Console.WriteLine($"\t- File: {fullPath}");
            }

            // write it
            WriteSingleFile(rawFile);
        }
    }

    /// <summary>
    ///     Creates the uninstall file.
    /// </summary>
    private void CreateUninstallFile()
    {
        var file = new CommandFile(true, "uninstall");
        AddFile(file);

        file.Add("# Created by the 'uninstall' feature. Uninstalls the addon from the world.");
        file.Add("");

        if (HasFeature(Feature.DUMMIES))
        {
            file.Add("# Removes dummy entities from the world.");
            file.Add(this.parentExecutor.entities.dummies.DestroyAll());
            file.Add("");
        }

        if (this.parentExecutor.scoreboard.temps.DefinedTempsRecord.Count != 0)
        {
            file.Add("# Remove temporary values used by the compiled code.");
            foreach (string temp in this.parentExecutor.scoreboard.temps.DefinedTempsRecord)
                file.Add(Command.ScoreboardRemoveObjective(temp));
            file.Add("");
        }

        // removes return related scoreboard objectives, if any.
        if (this.parentExecutor.definedReturnedTypes.Count != 0)
        {
            file.Add("# Removes return values used by the compiled code.");

            var objectives = new HashSet<string>();

            foreach (string objective in this.parentExecutor
                         .definedReturnedTypes
                         .Select(returnedType => new ScoreboardValue(ScoreboardValue.RETURN_NAME, false, returnedType,
                             this.parentExecutor.scoreboard))
                         .SelectMany(value => value.GetObjectives()))
                objectives.Add(objective);

            foreach (string objective in objectives)
                file.Add(Command.ScoreboardRemoveObjective(objective));

            file.Add("");
        }

        if (this.parentExecutor.scoreboard.values.Count != 0)
        {
            file.Add("# Removes user-defined values.");
            foreach (ScoreboardValue sb in this.parentExecutor.scoreboard.values)
                file.Add(Command.ScoreboardRemoveObjective(sb.InternalName));
            file.Add("");
        }

        // ReSharper disable once InvertIf
        if (this.parentExecutor.definedTags.Count != 0)
            foreach (string tag in this.parentExecutor.definedTags)
                file.Add(Command.TagRemove($"@e[tag={tag}]", tag));
    }

    private void CreateAutoInitFile()
    {
        const string TEMP_FILE = "projectVersion_";
        const string CURRENT_VERSION = "__currentVersion";
        string tempFile = TEMP_FILE + this.Identifier + ".bin";
        int version = 0;

        // file I/O; slowdown is probably pretty bad here
        {
            if (TemporaryFilesManager.HasFile(tempFile))
            {
                byte[] contents = TemporaryFilesManager.GetFileBytes(tempFile);

                if (contents.Length < 4)
                    Executor.Warn("Auto-Init project version file was corrupted or modified. Defaulting to 0.");
                else
                    version = BitConverter.ToInt32(contents, 0);
            }

            version += 1;
            TemporaryFilesManager.WriteFile(tempFile, BitConverter.GetBytes(version));
        }

        var file = new CommandFile(true, "_autoinit");
        AddFile(file);

        file.Add("# Created by the 'autoinit' feature. Checks for new versions and auto-initializes in new worlds.");
        var value = new ScoreboardValue(CURRENT_VERSION, true, Typedef.INTEGER, null, this.parentExecutor.scoreboard);

        file.Add(value.CommandsDefine());
        file.Add(value.CommandsInit());
        file.Add(Command.Execute().IfScore(value, new Range(version, true))
            .Run(Command.Function(this.parentExecutor.InitFile)));
        this.parentExecutor.InitFile.Add(Command.ScoreboardSet(value, version));

        // schedule file to run every tick.
        TickScheduler scheduler = this.parentExecutor.GetScheduler();
        scheduler.ScheduleTask(new ScheduledRepeatEveryTick(file));
    }

    /// <summary>
    ///     Enable a feature for this emission.
    /// </summary>
    /// <param name="feature"></param>
    internal void EnableFeature(Feature feature) { this.enabledFeatures |= feature; }
    /// <summary>
    ///     Check if this emission has a feature enabled.
    /// </summary>
    /// <param name="feature"></param>
    internal bool HasFeature(Feature feature) { return (this.enabledFeatures & feature) != Feature.NO_FEATURES; }
}