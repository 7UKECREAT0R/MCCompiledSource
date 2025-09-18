using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.MCC.Compiler.Async;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.MCC.Scheduling;
using mc_compiled.Modding;
using mc_compiled.Modding.Behaviors.Dialogue;
using mc_compiled.Modding.Resources;
using mc_compiled.Modding.Resources.Localization;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     The final stage of the compilation process. Runs statements and holds state.
/// </summary>
public partial class Executor
{
    private const string _FSTRING_SELECTOR = @"@(?:[spaeri]|initiator)(?:\[.+\])?";
    private const string _FSTRING_VARIABLE = @"[\w\d_\-:]+";

    /// <summary>
    ///     The current major version of MCCompiled.
    /// </summary>
    public const int MCC_VERSION = 20; // 1.XX, _compiler
    /// <summary>
    ///     Folder name that compiler-generated functions fall into.
    /// </summary>
    public const string MCC_GENERATED_FOLDER = "compiler";
    /// <summary>
    ///     Folder name that tests (made with the <c>test</c> command) fall into.
    /// </summary>
    public const string MCC_TESTS_FOLDER = "tests";
    /// <summary>
    ///     Prefix to be prepended to any translation keys made by the compiler.
    /// </summary>
    public const string MCC_TRANSLATE_PREFIX = "mcc.";
    /// <summary>
    ///     Text used to describe a symbol without any documentation.
    /// </summary>
    public const string UNDOCUMENTED_TEXT = "This symbol has no documentation.";
    /// <summary>
    ///     The name of the fake-player used for global values.
    /// </summary>
    public const string FAKE_PLAYER_NAME = "_";

    private static readonly Regex PPV_FMT = new(@"\\*\$[\w\d]+");

    public static readonly Regex FSTRING_SELECTOR = FStringSelectorRegex();
    public static readonly Regex FSTRING_VARIABLE = FStringVariableRegex();
    public static string MINECRAFT_VERSION = "0.00.000"; // _minecraft
    /// <summary>
    ///     Defines the maximum branch depth of the compiler will accept (and pre-allocate arrays for, etc...)
    /// </summary>
    public static int MAXIMUM_DEPTH = 100;

    private static readonly ConcurrentDictionary<string, int> generatedNames = new(StringComparer.OrdinalIgnoreCase);
    internal readonly AsyncManager async;

    internal readonly Stack<string> currentLocaleEntryPath = new();
    private readonly Stack<Action<Executor>> deferredActions;
    internal readonly HashSet<Typedef> definedReturnedTypes;
    private readonly List<int> definedStdFiles;
    internal readonly HashSet<string> definedTags;

    internal readonly Emission emission;
    internal readonly EntityManager entities;
    internal readonly FunctionManager functions;

    private readonly List<string> initCommands = [];
    private readonly PreviousComparisonStructure[] lastCompare;
    private readonly bool[] lastPreprocessorCompare;
    private readonly Dictionary<int, object> loadedFiles;
    internal readonly List<Macro> macros;
    internal readonly Dictionary<string, PreprocessorVariable> ppv;
    private readonly StringBuilder prependBuffer;
    internal readonly ScoreboardManager scoreboard;

    public readonly WorkspaceManager workspace;
    private Stack<CommandFile> currentFiles;

    internal int depth;
    private DialogueManager dialogueDefinitions;
    /// <summary>
    ///     Set to <c>true</c> when the executor is actively executing a file that was imported through the <c>$include</c>
    ///     command.
    /// </summary>
    internal bool isLibrary;

    /// <summary>
    ///     The number of iterations until the <see cref="deferredActions" /> are processed in order of newest-to-oldest.
    /// </summary>
    private int iterationsUntilDeferProcess;
    private LanguageManager languageManager;
    private int readIndex;

    internal TickScheduler scheduler;
    private SoundDefinitions soundDefinitions;

    private Statement[] statements;
    private int testCount;
    private int unreachableCode = -1;

    internal Executor(Statement[] statements, WorkspaceManager workspace)
    {
        this.workspace = workspace;
        string projectName = GlobalContext.Current.projectName ?? "unknown";
        string bpPath = GlobalContext.Current.behaviorPackOutputPath.Replace(Context.PROJECT_REPLACER, projectName);
        string rpPath = GlobalContext.Current.resourcePackOutputPath.Replace(Context.PROJECT_REPLACER, projectName);

        this.statements = statements;
        this.emission = new Emission(projectName, bpPath, rpPath, this);
        this.entities = new EntityManager(this);
        this.async = new AsyncManager(this);

        this.definedStdFiles = [];
        this.ppv = new Dictionary<string, PreprocessorVariable>(StringComparer.OrdinalIgnoreCase);
        this.macros = [];
        this.definedTags = [];
        this.definedReturnedTypes = [];

        // support up to MAXIMUM_DEPTH levels of scope before blowing up
        this.lastPreprocessorCompare = new bool[MAXIMUM_DEPTH];
        this.lastCompare = new PreviousComparisonStructure[MAXIMUM_DEPTH];

        this.deferredActions = new Stack<Action<Executor>>();
        this.loadedFiles = new Dictionary<int, object>();
        this.currentFiles = new Stack<CommandFile>();
        this.prependBuffer = new StringBuilder();
        this.scoreboard = new ScoreboardManager(this);

        this.functions = new FunctionManager(this.scoreboard);
        this.functions.RegisterDefaultProviders();

        SetCompilerPPVs();

        this.InitFile = new CommandFile(true, "init"); // don't need to push it, special case
        this.HeadFile = new CommandFile(true, projectName).AsRoot();
        this.currentFiles.Push(this.HeadFile);
    }

    /// <summary>
    ///     The prefix to use for locale entries at the current location of the executor.
    /// </summary>
    internal string LocaleEntryPrefix =>
        this.currentLocaleEntryPath.Count != 0
            ? $"{MCC_TRANSLATE_PREFIX}{string.Join(".", this.currentLocaleEntryPath)}."
            : MCC_TRANSLATE_PREFIX;
    /// <summary>
    ///     Returns the active locale, if any. Set using <see cref="SetLocale(string)" />.
    /// </summary>
    internal LocaleDefinition ActiveLocale { get; private set; }
    /// <summary>
    ///     Returns if a locale has been set via <see cref="SetLocale(string)" />.
    /// </summary>
    public bool HasLocale => this.ActiveLocale != null;

    /// <summary>
    ///     Returns if this executor has another statement available to run.
    /// </summary>
    public bool HasNext => this.readIndex < this.statements.Length;

    /// <summary>
    ///     Get the current file that should be written to.
    /// </summary>
    public CommandFile CurrentFile => this.currentFiles.Peek();
    internal CommandFile HeadFile { get; }
    internal CommandFile InitFile { get; }
    private CommandFile TestsFile { get; set; }

    /// <summary>
    ///     Get the current scope level.
    /// </summary>
    public int ScopeLevel => this.currentFiles.Count - 1;
    /// <summary>
    ///     The number of the next line that will be added.
    /// </summary>
    public int NextLineNumber => this.CurrentFile.Length + 1;

    /// <summary>
    ///     Get the names of all registered preprocessor variables.
    /// </summary>
    public IEnumerable<string> PPVNames => this.ppv.Select(p => p.Key).ToArray();
    internal Executor SetPPVsFromInput(IEnumerable<Context.InputPPV> inputPPVs)
    {
        if (inputPPVs != null)
            foreach (Context.InputPPV inputPPV in inputPPVs)
                SetPPV(inputPPV.name, null, inputPPV.value);
        return this;
    }

    /// <summary>
    ///     Display a success message regardless of debug setting.
    /// </summary>
    /// <param name="message">The warning to display.</param>
    public static void Good(string message)
    {
        ConsoleColor old = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ForegroundColor = old;
    }
    /// <summary>
    ///     Display a warning regardless of debug setting.
    /// </summary>
    /// <param name="warning">The warning to display.</param>
    /// <param name="source">The statement that is causing this warning, if any.</param>
    public static void Warn(string warning, Statement source = null)
    {
        ConsoleColor old;

        if (source == null)
        {
            old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("<!> {0}", warning);
            Console.ForegroundColor = old;
            return;
        }

        old = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("<L{0}> {1}", source.Lines[0], warning);
        Console.ForegroundColor = old;
    }
    /// <summary>
    ///     Defer an action to happen after the next valid statement has run.
    /// </summary>
    /// <param name="action"></param>
    public void DeferAction(Action<Executor> action)
    {
        this.iterationsUntilDeferProcess = 2;
        this.deferredActions.Push(action);
    }
    /// <summary>
    ///     Tick the deferred actions and run all of them if the timer is in the right state.
    /// </summary>
    private void TickDeferredActions()
    {
        if (this.iterationsUntilDeferProcess <= 0)
            return;

        this.iterationsUntilDeferProcess--;
        if (this.iterationsUntilDeferProcess != 0)
            return;

        AcceptDeferredActions();
    }
    /// <summary>
    ///     Process all deferred actions in the queue.
    /// </summary>
    private void AcceptDeferredActions()
    {
        while (this.deferredActions.Count != 0)
        {
            Action<Executor> action = this.deferredActions.Pop();
            action.Invoke(this);
        }
    }
    /// <summary>
    ///     Returns this executor after setting it to lint mode, lowering memory usage
    /// </summary>
    /// <returns></returns>
    internal void Linter() { this.emission.isLinting = true; }

    /// <summary>
    ///     Pushes to the prepend buffer the proper execute command needed to align to the given selector.
    ///     Sets the selector given by the reference parameter to @s.
    /// </summary>
    /// <param name="selector"></param>
    public void PushAlignSelector(ref Selector selector)
    {
        if (!selector.NonSelf)
            return;

        ExecuteBuilder builder = new ExecuteBuilder()
            .WithSubcommand(new SubcommandAs(selector))
            .WithSubcommand(new SubcommandAt(Selector.SELF))
            .WithSubcommand(new SubcommandRun());

        AppendCommandPrepend(builder.Build(out _));
        selector = Selector.SELF;
    }

    /// <summary>
    ///     Resolve an FString into rawtext terms. Also adds all setup commands for variables.
    /// </summary>
    /// <param name="fstring">The string to resolve.</param>
    /// <param name="langIdentifier">A unique identifier that makes language entries (if any) distinct.</param>
    /// <param name="forExceptions">The statement to use for exceptions.</param>
    /// <param name="advanced">If this FString is 'advanced' as in, requires per-user execution.</param>
    /// <returns></returns>
    public List<JSONRawTerm> FString(string fstring, string langIdentifier, Statement forExceptions, out bool advanced)
    {
        advanced = false;
        var terms = new List<JSONRawTerm>();
        var buffer = new StringBuilder();

        int scoreIndex = 0;
        for (int i = 0; i < fstring.Length; i++)
        {
            char character = fstring[i];

            if (character == '{')
            {
                // scan for closing bracket and return length
                int segmentLength = ScanForCloser(fstring, i);
                if (segmentLength == -1)
                {
                    // append the rest of the string and cancel.
                    buffer.Append(fstring[i..]);
                    break;
                }

                // get segment inside brackets
                string segment = fstring.Substring(i + 1, segmentLength - 1);

                // skip this if the segment is empty.
                if (string.IsNullOrWhiteSpace(segment))
                {
                    i += segmentLength;
                    buffer.Append('{');
                    buffer.Append(segment);
                    buffer.Append('}');
                    DumpTextBuffer();
                    continue;
                }

                // tokenize and assemble the inputs (without character stripping or definitions.def)
                var tokenizer = new Tokenizer(segment, false, false);
                Token[] tokens = tokenizer.Tokenize(forExceptions.SourceFile);
                Statement subStatement = new StatementHusk(tokens);
                subStatement.SetSource(forExceptions.Lines, "Inline FString Operation", forExceptions.SourceFile);

                // squash & resolve the tokens as best it can
                subStatement.PrepareThis(this);

                // dump text buffer before we get started.
                i += segmentLength;
                DumpTextBuffer();

                // get the rawtext terms
                IEnumerable<Token> remaining = subStatement.GetRemainingTokens();
                foreach (Token token in remaining)
                {
                    switch (token)
                    {
                        // try to find the best rawtext representation
                        case TokenSelectorLiteral selectorLiteral:
                        {
                            advanced = true;
                            Selector selector = selectorLiteral.selector;
                            terms.Add(new JSONSelector(selector.ToString()));
                            continue;
                        }
                        case TokenIdentifierValue identifierValue:
                        {
                            CommandFile file = this.CurrentFile;

                            if (!identifierValue.value.clarifier.IsGlobal)
                                advanced = true;

                            ScoreboardValue value = identifierValue.value;
                            int indexCopy = scoreIndex;

                            // type implementation called here
                            (string[] rtCommands, JSONRawTerm[] rtTerms) = value.ToRawText(ref indexCopy);

                            AddCommandsClean(rtCommands, "string" + value.InternalName,
                                $"Prepares the variable '{value.Name}' to be displayed in a rawtext. Invoked at {file.CommandReference} line {this.NextLineNumber}");

                            // localize and flatten the array.
                            terms.AddRange(
                                rtTerms.SelectMany(term => term.Localize(this, langIdentifier, forExceptions)));

                            scoreIndex++;
                            continue;
                        }
                    }

                    // default representation
                    string stringRepresentation = ResolveStringV2(token.ToString());
                    if (!string.IsNullOrEmpty(stringRepresentation))
                        terms.AddRange(
                            new JSONText(stringRepresentation).Localize(this, langIdentifier, forExceptions));
                }

                continue;
            }

            buffer.Append(character);
        }

        DumpTextBuffer();
        return terms;

        // dumps the contents of the text buffer into a string and adds it to the terms as text.
        void DumpTextBuffer()
        {
            string bufferContents = ResolveStringV2(buffer.ToString());

            if (string.IsNullOrEmpty(bufferContents))
                return;

            terms.AddRange(new JSONText(bufferContents).Localize(this, langIdentifier, forExceptions));
            buffer.Clear();
        }

        // stupid complex search for closing bracket: '}'
        int ScanForCloser(string str, int start)
        {
            int len = str.Length;
            int bracketDepth = -1;
            bool escape = false;

            for (int i = start + 1; i < len; i++)
            {
                char c = str[i];

                switch (c)
                {
                    case '\\':
                        escape = !escape;
                        continue;
                    // actual bracket stuff
                    case '{':
                        bracketDepth++;
                        continue;
                    case '}':
                    {
                        bracketDepth--;
                        if (bracketDepth < 0)
                            return i - start;
                        break;
                    }
                }
            }

            return -1;
        }
    }
    /// <summary>
    ///     Append these terms to the end of this command. Will resolve <see cref="JSONVariant" />s and construct the command
    ///     combinations.
    /// </summary>
    /// <param name="terms">The terms constructed by FString.</param>
    /// <param name="command">The command to append the terms to.</param>
    /// <param name="root">If this is the root call.</param>
    /// <param name="builder">The ExecuteBuilder that will hold all score checks in it.</param>
    /// <param name="commands">Used for recursion, set to null.</param>
    /// <param name="copy">The existing terms to copy from.</param>
    /// <returns></returns>
    public static string[] ResolveRawText(List<JSONRawTerm> terms,
        string command,
        bool root = true,
        ExecuteBuilder builder = null,
        List<string> commands = null,
        RawTextJsonBuilder copy = null)
    {
        var jb = new RawTextJsonBuilder(copy);

        builder ??= Command.Execute();
        commands ??= [];

        for (int i = 0; i < terms.Count; i++)
        {
            JSONRawTerm term = terms[i];
            if (term is JSONVariant variant)
            {
                // calculate all variants
                foreach (ConditionalTerm possibleVariant in variant.terms)
                {
                    Subcommand subcommand;
                    if (possibleVariant.invert)
                        subcommand = new SubcommandUnless(possibleVariant.condition);
                    else
                        subcommand = new SubcommandIf(possibleVariant.condition);

                    ExecuteBuilder branch = builder.Clone().WithSubcommand(subcommand);
                    List<JSONRawTerm> rest = terms.Skip(i + 1).ToList();
                    rest.InsertRange(0, possibleVariant.terms);
                    ResolveRawText(rest, command, false, branch, commands, jb);
                }

                break;
            }

            jb.AddTerm(term);
        }

        bool hasVariant = terms.Any(t => t is JSONVariant);

        switch (root)
        {
            case false when !hasVariant:
                commands.Add(builder.Run(command + jb.BuildString()));
                break;
            case true when !hasVariant:
                commands.Add(command + jb.BuildString());
                break;
        }

        return root ? commands.ToArray() : null; // return value isn't used in this case
    }
    /// <summary>
    ///     Sets the active locale that FString data will be sent to.
    /// </summary>
    /// <param name="locale"></param>
    public void SetLocale(string locale)
    {
        if (this.languageManager == null)
        {
            this.languageManager = new LanguageManager(this, false);
            AddExtraFile(this.languageManager);
        }

        this.ActiveLocale = this.languageManager.DefineLocale(locale);
    }
    /// <summary>
    ///     Sets a locale entry in the associated .lang file. Throws a <see cref="StatementException" /> if no locale has been
    ///     set yet via <see cref="SetLocale(string)" />.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="forExceptions"></param>
    /// <param name="overwrite"></param>
    /// <returns>
    ///     The entry that *should* be used in the case that a merge occurred. If null is returned, then translation
    ///     should not be used for this input string at all.
    /// </returns>
    /// <exception cref="StatementException"></exception>
    public LangEntry? SetLocaleEntry(string key, string value, Statement forExceptions, bool overwrite)
    {
        // null cases
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (!value.Any(char.IsLetter))
            return null;

        if (!this.HasLocale)
            throw new StatementException(forExceptions, "No language has been set. See the 'lang' command.");

        bool merge;

        if (this.ppv.TryGetValue(LanguageManager.MERGE_PPV, out PreprocessorVariable val))
            merge = (bool) val[0];
        else
            merge = false;

        var entry = LangEntry.Create(key, value);
        return this.ActiveLocale.file.Add(entry, overwrite, merge);
    }
    /// <summary>
    ///     Gets the sound definitions file, reading it from the existing RP if it exists.
    /// </summary>
    private SoundDefinitions GetSoundDefinitions(Statement callingStatement)
    {
        if (this.soundDefinitions != null)
            return this.soundDefinitions;

        string file = this.emission.GetOutputFileLocationFull(OutputLocation.r_SOUNDS, SoundDefinitions.FILE);

        if (File.Exists(file))
        {
            if (LoadJSONFile(file, callingStatement) is not JObject jObject)
                throw new StatementException(callingStatement,
                    $"File RP/sounds/{SoundDefinitions.FILE} was not a JSON Object.");

            this.soundDefinitions = SoundDefinitions.Parse(jObject, callingStatement);
            AddExtraFile(this.soundDefinitions);
            return this.soundDefinitions;
        }

        this.soundDefinitions = new SoundDefinitions(FormatVersion.r_SOUNDS.ToString());
        AddExtraFile(this.soundDefinitions);
        return this.soundDefinitions;
    }

    /// <summary>
    ///     Get the path of a file relative to the working directory.
    /// </summary>
    /// <param name="file">The file path.</param>
    /// <param name="_workingDirectory">The working directory path.</param>
    /// <returns>The relative path of the file, or null if the file is not contained within the working directory.</returns>
    [CanBeNull]
    [Pure]
    private static string GetPathRelativeToWorkingDirectory(string file, string _workingDirectory)
    {
        string _fullFile = Path.GetFullPath(file);
        var fullFile = new Uri(_fullFile);
        var workingDirectory = new Uri(_workingDirectory);

        if (!workingDirectory.IsBaseOf(fullFile))
            return null;

        string relative = workingDirectory.MakeRelativeUri(fullFile).ToString();
        relative = relative.Replace('/', Path.DirectorySeparatorChar);

        int indexOfSlash = relative.IndexOf(Path.DirectorySeparatorChar);
        if (indexOfSlash != -1)
            relative = relative[(indexOfSlash + 1)..];

        if (!relative.Contains(Path.DirectorySeparatorChar))
            return null;

        return relative;
    }

    /// <summary>
    ///     Adds a new sound definition to the project or returns an existing one if it already exists.
    /// </summary>
    /// <param name="soundFile">The path to the sound file to be added.</param>
    /// <param name="category">The category of the sound.</param>
    /// <param name="callingStatement">The calling statement that is adding the sound definition.</param>
    /// <returns>A new <see cref="SoundDefinition" /> object representing the added sound definition.</returns>
    public SoundDefinition AddNewSoundDefinition(string soundFile, SoundCategory category, Statement callingStatement)
    {
        // check if soundFile is contained somewhere within the working directory
        string workingDirectory = Environment.CurrentDirectory;
        string relativePath = GetPathRelativeToWorkingDirectory(soundFile, workingDirectory);

        var soundFolder = new StringBuilder("sounds" + Path.DirectorySeparatorChar);
        if (relativePath != null)
        {
            soundFolder.Append(Path.GetDirectoryName(relativePath));
            soundFolder.Append(Path.DirectorySeparatorChar);
        }

        soundFolder.Append(Path.GetFileNameWithoutExtension(soundFile));

        string fileName = Path.GetFileName(soundFile);
        string soundName = this.emission.Identifier + '.' + Path.GetFileNameWithoutExtension(soundFile);

        // create CopyFile so that the sound file can be copied during file writing
        var copyFile = new CopyFile(soundFile, OutputLocation.r_SOUNDS, relativePath ?? fileName);
        AddExtraFile(copyFile);

        SoundDefinitions soundDef = GetSoundDefinitions(callingStatement);
        var soundDefinition = new SoundDefinition(soundName, category, soundFolder.ToString());

        if (soundDef.TryGetSoundDefinition(soundDefinition.CommandReference, out SoundDefinition existing))
            return existing;

        soundDef.AddSoundDefinition(soundDefinition);
        return soundDefinition;
    }
    /// <summary>
    ///     Gets the instance of Dialogue registry.
    /// </summary>
    /// <returns>The Dialogue registry instance.</returns>
    public DialogueManager GetDialogueRegistry()
    {
        if (this.dialogueDefinitions != null)
            return this.dialogueDefinitions;

        this.dialogueDefinitions = new DialogueManager(MCC_GENERATED_FOLDER);
        AddExtraFile(this.dialogueDefinitions);
        return this.dialogueDefinitions;
    }

    /// <summary>
    ///     Marks all files on the file stack as containing an assertion.
    /// </summary>
    public void MarkAssertionOnFileStack()
    {
        foreach (CommandFile file in this.currentFiles)
            file.MarkAssertion();
    }
    /// <summary>
    ///     Tells the executor that the next line in the current block is be unreachable.
    /// </summary>
    public void UnreachableCode() { this.unreachableCode = 1; }
    /// <summary>
    ///     Throw a StatementException if a feature is not enabled.
    /// </summary>
    /// <param name="source">The statement to source as the cause of the exception.</param>
    /// <param name="feature">The feature to check for.</param>
    /// <param name="customMessage">
    ///     A custom message to display alongside the exception. If unspecified, the following is
    ///     shown: <c>Requires feature '[name]'. Enable using 'feature [name]' at the top of the file.</c>
    /// </param>
    /// <exception cref="StatementException">If <paramref name="feature" /> is not currently enabled in this executor.</exception>
    internal void RequireFeature(Statement source, Feature feature, string customMessage = null)
    {
        if (this.emission.HasFeature(feature))
            return;

        string name = feature.ToString();
        throw new StatementException(source,
            customMessage ??
            $"Requires feature '{name}'. Enable using 'feature {name.ToLower()}' at the top of the file.");
    }
    /// <summary>
    ///     Checks if the execution context is currently in an unreachable area and throw an exception if true.
    /// </summary>
    /// <param name="current">The statement that is currently being run.</param>
    /// <exception cref="StatementException">If the execution context is currently in an unreachable area.</exception>
    private void CheckUnreachable(Statement current)
    {
        if (this.unreachableCode > 0)
            this.unreachableCode--;
        else if (this.unreachableCode == 0)
            throw new StatementException(current, "Unreachable code detected.");
        else
            this.unreachableCode = -1;
    }

    /// <summary>
    ///     Load JSON file with caching for next use.
    /// </summary>
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="callingStatement">The calling statement.</param>
    /// <returns></returns>
    public JToken LoadJSONFile(string path, Statement callingStatement)
    {
        int hash = path.GetHashCode();

        // cached file, dont read again
        if (this.loadedFiles.TryGetValue(hash, out object value))
            if (value is JToken token)
                return token;

        if (string.IsNullOrWhiteSpace(path))
            throw new StatementException(callingStatement, "Empty JSON file path.");

        if (!File.Exists(path))
            throw new StatementException(callingStatement,
                "File \'" + path + "\' could not be found. Make sure you are in the right working directory.");

        string contents = File.ReadAllText(path);
        JToken json = JToken.Parse(contents);
        this.loadedFiles[hash] = json;
        return json;
    }

    /// <summary>
    ///     Load JSON file with caching for next use, under a different hashcode.
    /// </summary>
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="hash">The hash to use for remembering if the path is loaded once yet.</param>
    /// <param name="callingStatement">The statement to use for exceptions.</param>
    /// <returns></returns>
    public JToken LoadJSONFile(string path, int hash, Statement callingStatement)
    {
        // cached file, don't read again
        if (this.loadedFiles.TryGetValue(hash, out object value))
            if (value is JToken token)
                return token;

        if (string.IsNullOrWhiteSpace(path))
            throw new StatementException(callingStatement, "Empty JSON file path.");

        if (!File.Exists(path))
            throw new StatementException(callingStatement,
                "File \'" + path + "\' could not be found. Make sure you are in the right working directory.");

        string contents = File.ReadAllText(path);
        JToken json = JToken.Parse(contents);
        this.loadedFiles[hash] = json;
        return json;
    }
    /// <summary>
    ///     Load file with caching for next use.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns></returns>
    public string LoadFileString(string path)
    {
        int hash = path.GetHashCode();

        // cached file, dont read again
        if (this.loadedFiles.TryGetValue(hash, out object value))
            if (value is string s)
                return s;

        string contents = File.ReadAllText(path);
        this.loadedFiles[hash] = contents;
        return contents;
    }
    /// <summary>
    ///     Load file with caching for next use.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns></returns>
    public byte[] LoadFileBytes(string path)
    {
        int hash = path.GetHashCode();

        // cached file, dont read again
        if (this.loadedFiles.TryGetValue(hash, out object value))
            switch (value)
            {
                case string s:
                    return Encoding.UTF8.GetBytes(s);
                case byte[] bytes:
                    return bytes;
            }

        byte[] contents = File.ReadAllBytes(path);
        this.loadedFiles[hash] = contents;
        return contents;
    }

    /// <summary>
    ///     Attempts to fetch an addon file from the current BP, or downloads it from the default pack if not present. Uses
    ///     caching.
    /// </summary>
    public JToken Fetch(IAddonFile toLocate, Statement callingStatement)
    {
        string outputFile = this.emission.GetOutputFileLocationFull(toLocate, true);

        if (outputFile == null)
            throw new StatementException(callingStatement,
                "Attempted to fetch an unwritable file. Report this to the developers.");

        int outputFileHash = outputFile.GetHashCode();

        // the JSON root of the file
        JToken root;

        // find the user-defined entity file, or default to the vanilla pack provided by Microsoft
        if (this.loadedFiles.TryGetValue(outputFileHash, out object jValue))
        {
            root = jValue as JToken;
        }
        else
        {
            if (File.Exists(outputFile))
            {
                root = LoadJSONFile(outputFile, callingStatement);
            }
            else
            {
                string pathString = toLocate.GetOutputLocation().ToString();
                var packType = TemporaryFilesManager.PackType.BehaviorPack;

                if (pathString.StartsWith("r_"))
                {
                    packType = TemporaryFilesManager.PackType.ResourcePack;
                    pathString = pathString[2..];
                }

                if (pathString.StartsWith("b_"))
                    pathString = pathString[2..];

                string[] paths = pathString.Split(["__"], StringSplitOptions.None);
                string[] filePath = new string[paths.Length + 1];
                for (int i = 0; i < paths.Length; i++)
                {
                    string current = paths[i];
                    filePath[i] = current.ToLower();
                }

                filePath[paths.Length] = toLocate.GetOutputFile(); // last index

                string downloadedFile = TemporaryFilesManager.Get(packType, callingStatement, filePath);
                root = LoadJSONFile(downloadedFile, outputFileHash, callingStatement);
            }
        }

        return root;
    }

    /// <summary>
    ///     Define a file that sort-of equates to a "standard library." Will only be added once.
    /// </summary>
    /// <param name="file">The command file to define as a standard library file.</param>
    public void DefineSTDFile(CommandFile file)
    {
        if (this.definedStdFiles.Contains(file.GetHashCode()))
            return;
        this.definedStdFiles.Add(file.GetHashCode());
        AddExtraFile(file);
    }

    /// <summary>
    ///     Determines if the given CommandFile exists in the definedStdFiles list.
    /// </summary>
    /// <param name="file">The CommandFile object to check for.</param>
    /// <returns>True if the CommandFile exists in the definedStdFiles list; otherwise, false.</returns>
    public bool HasSTDFile(CommandFile file) { return this.definedStdFiles.Contains(file.GetHashCode()); }

    /// <summary>
    ///     Tries to fetch a documentation string based on whether the last statement was a comment or not. Returns
    ///     <see cref="Executor.UNDOCUMENTED_TEXT" /> if no documentation was supplied.
    /// </summary>
    /// <returns></returns>
    public string GetDocumentationString(out bool hadDocumentation)
    {
        if (this.readIndex < 1)
        {
            hadDocumentation = false;
            return UNDOCUMENTED_TEXT;
        }

        Statement last = PeekLast();
        if (last is StatementComment comment)
        {
            hadDocumentation = true;
            return ResolveStringV2(comment.comment);
        }

        hadDocumentation = false;
        return UNDOCUMENTED_TEXT;
    }

    /// <summary>
    ///     Peek at the next statement.
    /// </summary>
    /// <returns></returns>
    public Statement Peek() { return this.statements[this.readIndex]; }
    /// <summary>
    ///     Peek at the statement N statements in front of the read index. 0: current, 1: next, etc...
    ///     Does perform bounds checking, and returns null if outside bounds.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public Statement PeekSkip(int amount)
    {
        int index = this.readIndex + amount;

        if (index < 0)
            return null;
        if (index < this.statements.Length)
            return this.statements[index];
        return null;
    }
    /// <summary>
    ///     Peek at the last statement that was gotten, if any.
    /// </summary>
    /// <returns><b>null</b> if no statements have been gotten yet.</returns>
    public Statement PeekLast() { return this.readIndex > 1 ? this.statements[this.readIndex - 2] : null; }

    /// <summary>
    ///     Seek for the next statement that has valid executable data. Returns null if outside of bounds.
    /// </summary>
    /// <returns></returns>
    public Statement Seek() { return SeekSkip(0); }
    /// <summary>
    ///     Seek forward from the statement N statements in front of the read index until it finds one with valid executable
    ///     data. 0: current, 1: next, etc...
    ///     Does perform bounds checking, and returns null if outside bounds.
    /// </summary>
    /// <param name="amount">The number of valid-data statements to skip.</param>
    /// <returns></returns>
    public Statement SeekSkip(int amount)
    {
        Statement statement;

        int i = this.readIndex + amount;

        if (i < 0 || i >= this.statements.Length)
            return null;

        do
        {
            statement = this.statements[i++];
        } while ((statement == null || statement.Skip) && i < this.statements.Length);

        return statement;
    }
    /// <summary>
    ///     Seek backwards from the current statement until it finds one with valid executable data. Returns null if outside of
    ///     bounds.
    /// </summary>
    /// <returns><b>null</b> if no statements have been gotten yet.</returns>
    public Statement SeekLast()
    {
        Statement statement;

        int i = this.readIndex - 1;

        if (i < 0)
            return null; // no statements??

        do
        {
            statement = this.statements[i--];
        } while ((statement == null || statement.Skip) && i >= 0);

        return statement;
    }

    /// <summary>
    ///     Get the next statement to be read and then increment the read index.
    /// </summary>
    /// <returns></returns>
    public Statement Next() { return this.statements[this.readIndex++]; }
    /// <summary>
    ///     Returns the next statement to be read as a certain type. Increments the read index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Next<T>() where T : Statement { return this.statements[this.readIndex++] as T; }

    /// <summary>
    ///     Peek at the next statement as a certain type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Peek<T>() where T : Statement { return this.statements[this.readIndex] as T; }
    /// <summary>
    ///     Peek a certain number of statements into the future as a certain type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="skip"></param>
    /// <returns></returns>
    public T Peek<T>(int skip) where T : Statement { return this.statements[this.readIndex + skip] as T; }

    /// <summary>
    ///     Returns if there's another statement available and it's of a certain type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool NextIs<T>() where T : Statement { return this.HasNext && this.statements[this.readIndex] is T; }
    /// <summary>
    ///     Returns if the next statement is an unknown statement with builder field(s) in it.
    /// </summary>
    /// <returns></returns>
    private bool NextIsBuilder()
    {
        if (!this.HasNext)
            return false;

        Statement _tokens = this.statements[this.readIndex];

        if (_tokens is not StatementUnknown tokens)
            return false;

        return tokens.NextIs<TokenBuilderIdentifier>(false);
    }

    public bool NextBuilderField(ref Statement tokens, out TokenBuilderIdentifier builderField)
    {
        // next in statement?
        if (tokens.NextIs<TokenBuilderIdentifier>(false))
        {
            builderField = tokens.Next<TokenBuilderIdentifier>(null);
            return true;
        }

        // not in the current statement ... look ahead in code
        if (NextIsBuilder())
        {
            // reassigns the field in the caller's code
            tokens = Next<StatementUnknown>().ClonePrepare(this);
            builderField = tokens.Next<TokenBuilderIdentifier>(null);
            return true;
        }

        // end of builder cycle
        builderField = null;
        return false;
    }
    /// <summary>
    ///     Return an array of the next N statements.
    /// </summary>
    /// <param name="amount">The number of statements to grab.</param>
    /// <returns></returns>
    public Statement[] Peek(int amount)
    {
        if (amount == 0)
            return [];

        var ret = new Statement[amount];

        int write = 0;
        for (int i = this.readIndex; i < this.statements.Length && i < this.readIndex + amount; i++)
        {
            Statement statement = this.statements[i];
            ret[write++] = statement;
        }

        return ret;
    }
    /// <summary>
    ///     Return an array of the next N statements.
    /// </summary>
    /// <param name="skip">The number of statements to skip before grabbing the next N statements.</param>
    /// <param name="amount">The number of statements to grab.</param>
    /// <returns></returns>
    public Statement[] Peek(int skip, int amount)
    {
        if (amount == 0)
            return [];

        var ret = new Statement[amount];

        int write = 0;
        int lastIndex = this.readIndex + skip + amount;
        for (int i = this.readIndex + skip; i < this.statements.Length && i < lastIndex; i++)
        {
            Statement statement = this.statements[i];
            ret[write++] = statement;
        }

        return ret;
    }
    /// <summary>
    ///     Reads the next statement, or set of statements if it is a block. Similar to Next() in that it updates the
    ///     readIndex.
    /// </summary>
    /// <param name="shouldBlockAsyncBeDisabled">Should a block be fetched, should its effect on async statements be disabled?</param>
    /// <returns></returns>
    public Statement[] NextExecutionSet(bool shouldBlockAsyncBeDisabled)
    {
        Statement current = this.statements[this.readIndex - 1];

        if (!this.HasNext)
            throw new StatementException(current, "Unexpected end-of-file while expecting statement/block.");

        if (NextIs<StatementOpenBlock>())
        {
            var block = Next<StatementOpenBlock>();
            if (shouldBlockAsyncBeDisabled)
                block.ignoreAsync = true;
            int statementCount = block.statementsInside;
            Statement[] code = Peek(statementCount);
            this.readIndex += statementCount;
            this.readIndex++; // block closer
            return code;
        }

        Statement next = Next();

        if (next is not IExecutionSetPart)
            throw new StatementException(current,
                "Following statement '" + next.Source + "' cannot be explicitly run.");

        return [next];
    }

    /// <summary>
    ///     Pop the prepend buffer's contents and return it.
    /// </summary>
    /// <returns></returns>
    private string PopPrepend()
    {
        string ret = this.prependBuffer.ToString();
        this.prependBuffer.Clear();
        return ret;
    }

    /// <summary>
    ///     Set up the default preprocessor variables.
    /// </summary>
    private void SetCompilerPPVs()
    {
        this.ppv["_minecraft"] = new PreprocessorVariable(MINECRAFT_VERSION);
        this.ppv["_compiler"] = new PreprocessorVariable(MCC_VERSION);
        this.ppv["_realtime"] = new PreprocessorVariable(DateTime.Now.ToShortTimeString());
        this.ppv["_realdate"] = new PreprocessorVariable(DateTime.Now.ToShortDateString());
        this.ppv["_timeformat"] = new PreprocessorVariable(TimeFormat.Default.ToString());
        this.ppv["_true"] = new PreprocessorVariable("true");
        this.ppv["_false"] = new PreprocessorVariable("false");
    }
    /// <summary>
    ///     Run this executor start to finish.
    /// </summary>
    /// <param name="lint">
    ///     If the compilation should be cut down to improve speed.
    ///     Used for linting or other situations where no <i>actual</i> files are going to be written.
    /// </param>
    /// <param name="resultEmission">
    ///     The emission of the compilation; may or may not be <see cref="Emission.Complete()" />ed
    ///     yet.
    /// </param>
    public void Execute(bool lint, out Emission resultEmission)
    {
        resultEmission = this.emission;
        this.emission.isLinting = lint;

        this.readIndex = 0;

        while (this.HasNext)
        {
            Statement unresolved = Next();
            unresolved.Decorate(this);
            Statement statement = unresolved.ClonePrepare(this);

            using (this.scoreboard.temps.PushTempState())
            {
                statement.SetExecutor(this);
                statement.Run0(this);
            }

            if (statement.Skip)
                continue; // ignore this statement

            // check for unreachable code due to a halt directive
            CheckUnreachable(statement);

            // tick deferred processes
            TickDeferredActions();

            if (GlobalContext.Debug)
                Console.WriteLine("EXECUTE LN{0}: {1}", statement.Lines[0], statement);
        }

        while (this.currentFiles.Count != 0)
            PopFile();

        FinalizeInitFile();

        this.emission.Complete();
    }
    /// <summary>
    ///     Temporarily run another subsection of statements, then resume the executor where it left off.
    /// </summary>
    public void ExecuteSubsection(Statement[] section)
    {
        Statement[] restore0 = this.statements;
        int restore1 = this.readIndex;

        this.statements = section;
        this.readIndex = 0;
        while (this.HasNext)
        {
            Statement unresolved = Next();
            unresolved.Decorate(this);
            Statement statement = unresolved.ClonePrepare(this);

            using (this.scoreboard.temps.PushTempState())
            {
                statement.Run0(this);
            }

            if (statement.Skip)
                continue; // ignore this statement

            // check for unreachable code due to halt directive
            CheckUnreachable(statement);

            // tick deferred processes
            TickDeferredActions();

            if (GlobalContext.Debug)
                Console.WriteLine("EXECUTE SUBSECTION LN{0}: {1}", statement.Lines[0], statement);
        }

        // now it's done, so restore state
        this.statements = restore0;
        this.readIndex = restore1;
    }
    /// <summary>
    ///     Runs a subsection of statements, then resume the executor where it left off. During the execution,
    ///     <see cref="isLibrary" /> will be set to true.
    /// </summary>
    /// <param name="section"></param>
    internal void ExecuteLibrary(Statement[] section)
    {
        bool previouslyInLibrary = this.isLibrary;
        this.isLibrary = true;
        ExecuteSubsection(section);
        this.isLibrary = previouslyInLibrary;
    }

    /// <summary>
    ///     Set the result of the last preprocessor-if comparison in this scope.
    /// </summary>
    /// <param name="value"></param>
    public void SetLastIfResult(bool value) { this.lastPreprocessorCompare[this.ScopeLevel] = value; }
    /// <summary>
    ///     Get the result of the last preprocessor-if comparison in this scope.
    /// </summary>
    /// <returns></returns>
    public bool GetLastIfResult() { return this.lastPreprocessorCompare[this.ScopeLevel]; }

    /// <summary>
    ///     Set the last comparison data used at the given/current scope level.
    /// </summary>
    /// <param name="set">The previous comparison that ran.</param>
    /// <param name="scope">The scope of this comparison. Leave null and it will be replaced with <see cref="ScopeLevel" />.</param>
    internal void SetLastCompare(PreviousComparisonStructure set, int? scope = null)
    {
        if (scope == null)
            scope = this.ScopeLevel;

        this.lastCompare[scope.Value] = set;
    }
    /// <summary>
    ///     Get the last comparison data used at the given/current scope level.
    /// </summary>
    /// <returns></returns>
    internal PreviousComparisonStructure GetLastCompare(int? scope = null)
    {
        if (scope == null)
            scope = this.ScopeLevel;

        return this.lastCompare[scope.Value];
    }

    /// <summary>
    ///     Register a macro to be looked up later.
    /// </summary>
    /// <param name="macro">The macro to register.</param>
    public void RegisterMacro(Macro macro) { this.macros.Add(macro); }
    /// <summary>
    ///     Look for a macro present in this project.
    /// </summary>
    /// <param name="name">The name used to look up a macro.</param>
    /// <returns>A nullable <see cref="Macro" /> which contains the found macro, if any.</returns>
    public Macro? LookupMacro(string name)
    {
        foreach (Macro macro in this.macros.Where(macro => macro.Matches(name)))
            return macro;
        return null;
    }
    /// <summary>
    ///     Tries to look for a macro present in this project under a certain name.
    /// </summary>
    /// <param name="name">The name used to look up a macro.</param>
    /// <param name="macro">The output of this method if it returns true.</param>
    /// <returns>If a macro was successfully found and set.</returns>
    public bool TryLookupMacro(string name, out Macro? macro)
    {
        macro = LookupMacro(name);
        return macro.HasValue;
    }

    /// <summary>
    ///     Generates a new tests file.
    /// </summary>
    public void CreateTestsFile()
    {
        this.TestsFile = new CommandFile(true, "test");
        AddExtraFile(this.TestsFile);
        this.TestsFile.Add("");
    }
    /// <summary>
    ///     Add a command to the current file, with prepend buffer.
    /// </summary>
    /// <param name="command"></param>
    public void AddCommand(string command)
    {
        if (this.emission.isLinting)
        {
            PopPrepend();
            return;
        }

        this.CurrentFile.Add(PopPrepend() + command);
    }
    /// <summary>
    ///     Add a set of commands into a new branching file unless inline is set.
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="friendlyName">The friendly name to give the generated file, if any.</param>
    /// <param name="friendlyDescription">The description to be placed at the top of the generated file, if any.</param>
    /// <param name="inline">Force the commands to be inlined rather than sent to a generated file.</param>
    public void AddCommands(IEnumerable<string> commands,
        string friendlyName,
        string friendlyDescription,
        bool inline = false)
    {
        if (this.emission.isLinting)
            return;
        string[] commandsAsArray = commands as string[] ?? commands.ToArray();
        int count = commandsAsArray.Length;
        if (count < 1)
            return;

        if (inline)
        {
            string buffer = PopPrepend();
            this.CurrentFile.Add(from c in commandsAsArray select buffer + c);
            return;
        }

        if (count == 1)
        {
            AddCommand(commandsAsArray.First());
            return;
        }

        CommandFile file = GetNextGeneratedFile(friendlyName, false);

        if (friendlyDescription != null)
            file.Add("# " + friendlyDescription);

        file.Add(commandsAsArray);

        AddExtraFile(file);
        AddCommand(Command.Function(file));
    }
    /// <summary>
    ///     Add a command to the current file, not modifying the prepend buffer.
    /// </summary>
    /// <param name="command"></param>
    public void AddCommandClean(string command)
    {
        if (this.emission.isLinting)
            return;
        string prepend = this.prependBuffer.ToString();
        this.CurrentFile.Add(prepend + command);
    }
    /// <summary>
    ///     Add a set of commands into a new branching file, not modifying the prepend buffer.
    ///     If inline is set, no branching file will be made.
    /// </summary>
    /// <param name="commands"></param>
    /// <param name="friendlyName">The friendly name to give the generated file, if any.</param>
    /// <param name="friendlyDescription">The description to be placed at the top of the generated file, if any.</param>
    /// <param name="inline">Force the commands to be inlined rather than sent to a generated file.</param>
    public void AddCommandsClean(IEnumerable<string> commands,
        string friendlyName,
        string friendlyDescription,
        bool inline = false)
    {
        if (this.emission.isLinting || commands == null)
            return;
        string buffer = this.prependBuffer.ToString();

        if (inline)
        {
            this.CurrentFile.Add(commands.Select(c => buffer + c));
            return;
        }

        string[] commandsAsArray = commands as string[] ?? commands.ToArray();
        int count = commandsAsArray.Count();
        if (count < 1)
            return;
        if (count == 1)
        {
            AddCommandClean(commandsAsArray.First());
            return;
        }

        CommandFile file = GetNextGeneratedFile(friendlyName, false);

        if (friendlyDescription != null)
            file.Add("# " + friendlyDescription);

        file.Add(commandsAsArray);

        AddExtraFile(file);
        this.CurrentFile.Add(buffer + Command.Function(file));
    }
    /// <summary>
    ///     Add a file on its own to the list.
    /// </summary>
    /// <param name="file"></param>
    public void AddExtraFile(IAddonFile file) { this.emission.AddFile(file); }
    /// <summary>
    ///     Add a file to the list, removing any other file that has a matching name/directory.
    /// </summary>
    /// <param name="file"></param>
    public void OverwriteExtraFile(IAddonFile file)
    {
        this.emission.RemoveDuplicatesOf(file);
        this.emission.AddFile(file);
    }
    /// <summary>
    ///     Add a set of files on their own to the list.
    /// </summary>
    /// <param name="files">The files to add.</param>
    public void AddExtraFiles(IEnumerable<IAddonFile> files) { this.emission.AddFiles(files); }
    /// <summary>
    ///     Add a set of files on their own to the list, removing any other files that have a matching name/directory.
    /// </summary>
    /// <param name="files">The files to overwrite.</param>
    public void OverwriteExtraFiles(IEnumerable<IAddonFile> files)
    {
        foreach (IAddonFile file in files)
            OverwriteExtraFile(file);
    }
    /// <summary>
    ///     Add a command to the 'init' file. Does not affect the prepend buffer.
    /// </summary>
    /// <param name="command"></param>
    public void AddCommandInit(string command)
    {
        if (this.emission.isLinting || command == null)
            return;
        this.initCommands.Add(command);
    }
    /// <summary>
    ///     Adds a set of commands to the 'init' file. Does not affect the prepend buffer.
    /// </summary>
    /// <param name="commands"></param>
    public void AddCommandsInit(IEnumerable<string> commands)
    {
        if (this.emission.isLinting || commands == null)
            return;

        string[] commandsAsArray = commands as string[] ?? commands.ToArray();

        if (commandsAsArray.Length == 0)
            return;

        this.initCommands.AddRange(commandsAsArray);
    }

    /// <summary>
    ///     Gets the tick scheduler in this Executor, or creates a new one if it doesn't already exist.
    /// </summary>
    /// <returns></returns>
    public TickScheduler GetScheduler()
    {
        if (this.scheduler != null)
            return this.scheduler;

        // create a new one
        this.scheduler = new TickScheduler(this);
        this.emission.AddFile(this.scheduler);
        return this.scheduler;
    }
    /// <summary>
    ///     Returns if this Executor has a tick scheduler yet.
    /// </summary>
    /// <returns></returns>
    public bool HasScheduler() { return this.scheduler != null; }

    /// <summary>
    ///     Set the content that will prepend the next added command.
    /// </summary>
    /// <param name="content"></param>
    /// <returns>The old buffer's contents.</returns>
    public string SetCommandPrepend(string content)
    {
        string oldContent = this.prependBuffer.ToString();
        this.prependBuffer.Clear().Append(content);
        return string.IsNullOrEmpty(oldContent) ? "" : oldContent;
    }
    /// <summary>
    ///     Append to the content to the prepend buffer.
    /// </summary>
    /// <param name="content"></param>
    public void AppendCommandPrepend(string content) { this.prependBuffer.Append(content); }
    /// <summary>
    ///     Prepend content to the prepend buffer.
    /// </summary>
    /// <param name="content"></param>
    public void PrependCommandPrepend(string content) { this.prependBuffer.Insert(0, content); }

    /// <summary>
    ///     Try to get a preprocessor variable.
    /// </summary>
    /// <param name="name">
    ///     The name of the preprocessor variable to try to get.
    ///     May contain a <c>$</c>, it will be stripped out internally.
    /// </param>
    /// <param name="value">If the method returns <c>true</c>, the value of the obtained preprocessor variable.</param>
    /// <returns></returns>
    public bool TryGetPPV(string name, out PreprocessorVariable value)
    {
        if (string.IsNullOrEmpty(name))
        {
            value = null;
            return false;
        }

        if (name[0] == '$')
            name = name[1..];

        return this.ppv.TryGetValue(name, out value);
    }

    /// <summary>
    ///     Set or create a preprocessor variable copied from an existing one.
    /// </summary>
    /// <param name="name">The name of the preprocessor variable to set or create.</param>
    /// <param name="callingStatement">The calling statement in case this method decides the input name is not acceptable.</param>
    /// <param name="values">The values to set for the preprocessor variable.</param>
    public void SetPPVCopy(string name, Statement callingStatement, PreprocessorVariable values)
    {
        if (string.IsNullOrEmpty(name))
            throw new Exception("Tried to set a PPV with an empty name.");
        if (name[0] == '$')
            name = name[1..];
        this.ppv[name] = values.Clone();
    }
    /// <summary>
    ///     Set or create a preprocessor variable.
    /// </summary>
    /// <param name="name">The name of the preprocessor variable.</param>
    /// <param name="callingStatement">The calling statement in case this method decides the input name is not acceptable.</param>
    /// <param name="values">The values to set for the preprocessor variable.</param>
    public void SetPPV(string name, [CanBeNull] Statement callingStatement, params dynamic[] values)
    {
        // we can only do error handling if there's a source location to report the error from
        if (callingStatement != null)
            if (string.IsNullOrEmpty(name))
                throw new StatementException(callingStatement, "Tried to set a PPV with an empty name.");

        if (name[0] == '$')
            name = name[1..];
        this.ppv[name] = new PreprocessorVariable(values);
    }

    /// <summary>
    ///     Resolve all unescaped preprocessor variables in a string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [Obsolete($"{nameof(ResolveString)} is obsolete and not full-featured. Use {nameof(ResolveStringV2)} instead.")]
    public string ResolveString(string str)
    {
        if (this.ppv.Count < 1)
            return str;

        var sb = new StringBuilder(str);
        MatchCollection matches = PPV_FMT.Matches(str);
        int count = matches.Count;

        if (count == 0)
            return str;

        for (int i = count - 1; i >= 0; i--)
        {
            Match match = matches[i];
            string text = match.Value;
            int lastIndex = text.LastIndexOf('\\');
            int backslashes = lastIndex + 1;

            int offset = lastIndex < 0 ? 0 : lastIndex;
            int insertIndex = match.Index + offset;

            // if there are an odd number of preceding backslashes, this is escaped.
            if (backslashes % 2 == 1)
            {
                sb.Remove(match.Index + lastIndex, 1);
                continue;
            }

            string ppvName = FindLongestExistingPPV(text[(lastIndex + 1)..]);

            if (string.IsNullOrEmpty(ppvName))
                continue; // no ppv named that
            if (!TryGetPPV(ppvName, out PreprocessorVariable values))
                continue; // shouldn't happen since FindLongestValidPPV checks this

            string insertText = values.Length > 1
                ? string.Join(" ", values)
                : values.Length == 1
                    ? (string) values[0].ToString()
                    : "";

            sb.Remove(insertIndex, ppvName.Length - offset);
            sb.Insert(insertIndex, insertText);
        }

        return sb.ToString();

        string FindLongestExistingPPV(string candidateName)
        {
            if (TryGetPPV(candidateName, out _))
                return candidateName;

            for (int i = candidateName.Length - 1; i > 1; i--)
            {
                string substring = candidateName[..i];
                if (TryGetPPV(substring, out _))
                    return substring;
            }

            return null;
        }
    }
    /// <summary>
    ///     Resolve all unescaped preprocessor variables in a string.
    /// </summary>
    /// <param name="str">The string to resolve.</param>
    /// <returns>The resolved string. Will likely be completely unchanged.</returns>
    /// <remarks>
    ///     This is a rewrite of the original, regex-based ResolveString method.
    ///     This one is much faster, uses much less memory, supports indexers, and is more reliable overall.
    /// </remarks>
    public string ResolveStringV2(string str)
    {
        ReadOnlySpan<char> chars = str.AsSpan();
        if (!chars.Contains('$') && !chars.Contains('['))
            return str;
        var sb = new StringBuilder();
        var output = new StringBuilder();

        int i = 0;
        bool previousCharacterWasDeref = false;
        int previousNumberOfBackslashes = 0;

        // welcome to the world's most ridiculous string-interpolation parser
        // buckle up and enjoy the ride

        // step over every character and chunk-by-chunk append it to the output StringBuilder
        // based on whether it resolves to a ppv or not
        while (i < chars.Length)
        {
            char c = chars[i++];

            if (c == '\\')
            {
                previousNumberOfBackslashes++;
                previousCharacterWasDeref = false;
                continue;
            }

            if (c == '$')
            {
                if (previousCharacterWasDeref) // this would imply 2 deref characters in a row, which would make `numberOfBackslashes` invalid
                {
                    output.Append('$');
                    previousNumberOfBackslashes = 0;
                }
                else
                {
                    previousCharacterWasDeref = true;
                }

                continue;
            }

            // default behavior for non-word characters
            if (c != '_' && !char.IsAsciiLetterOrDigit(c))
            {
                output.Append('\\', previousNumberOfBackslashes);
                if (previousCharacterWasDeref)
                    output.Append('$');
                output.Append(c);
                previousNumberOfBackslashes = 0;
                previousCharacterWasDeref = false;
                continue;
            }

            // word character, let's fetch the longest chunk possible to match as a preprocessor variable
            sb.Clear();
            sb.Append(c);
            bool hasIndexerAtEnd = false;
            while (i < chars.Length)
            {
                char c2 = chars[i];
                if (c2 == '_' || char.IsAsciiLetterOrDigit(c2))
                {
                    sb.Append(c2);
                    i++;
                    continue;
                }

                if (c2 == '[')
                    hasIndexerAtEnd = true;
                break;
            }

            int amountTrimmedFromBeginning; // number of characters trimmed from the end of the area captured by `sb`
            int amountTrimmedFromEnd; // number of characters trimmed from the end of the area captured by `sb`
            string ppvName;

            // if the last character was a deref, we're not relying
            // on there being an indexer '[' to make this dereference valid
            if (previousCharacterWasDeref)
            {
                ppvName = FindLongestExistingPPV(sb.ToString(), true, out amountTrimmedFromEnd);
                amountTrimmedFromBeginning = 0;

                if (ppvName == null && hasIndexerAtEnd)
                {
                    ppvName = FindLongestExistingPPV(sb.ToString(), false, out amountTrimmedFromBeginning);
                    amountTrimmedFromEnd = 0;
                }
            }
            else if (hasIndexerAtEnd)
            {
                ppvName = FindLongestExistingPPV(sb.ToString(), false, out amountTrimmedFromBeginning);
                amountTrimmedFromEnd = 0;
            }
            else
            {
                ppvName = null;
                amountTrimmedFromBeginning = 0;
                amountTrimmedFromEnd = 0;
            }

            if (ppvName == null)
            {
                output.Append('\\', previousNumberOfBackslashes);
                if (previousCharacterWasDeref)
                    output.Append('$');
                output.Append(sb);
                previousNumberOfBackslashes = 0;
                previousCharacterWasDeref = false;
                continue;
            }

            if (amountTrimmedFromBeginning > 0)
            {
                // disregard the preceding dereference operator and backslashes
                // since the resolved PPV name doesn't extend back to it.
                output.Append('\\', previousNumberOfBackslashes);
                if (previousCharacterWasDeref)
                    output.Append('$');
                output.Append(sb, 0, amountTrimmedFromBeginning);
                previousNumberOfBackslashes = 0;
                previousCharacterWasDeref = false;
            }

            if (amountTrimmedFromEnd > 0)
            {
                // disregard the proceeding indexer operator
                // since the resolved PPV name doesn't extend up to it.
                i -= amountTrimmedFromEnd;
                hasIndexerAtEnd = false;
            }

            // handle the backslashes, if any.
            if (previousNumberOfBackslashes > 0)
            {
                output.Append('\\', previousNumberOfBackslashes / 2);
                if (previousNumberOfBackslashes % 2 == 1)
                {
                    if (previousCharacterWasDeref)
                        output.Append('$');
                    output.Append(sb, amountTrimmedFromBeginning,
                        sb.Length - amountTrimmedFromBeginning - amountTrimmedFromEnd);
                    previousNumberOfBackslashes = 0;
                    previousCharacterWasDeref = false;
                    continue;
                }
            }

            // great, we have a valid PPV name now; fetch the values
            if (!TryGetPPV(ppvName, out PreprocessorVariable values))
                throw new Exception("Impossible case"); // shouldn't happen because of FindLongestExistingPPV
            string[] valuesFinal;

            // indexer time, let's fetch all the raw inner values
            if (hasIndexerAtEnd && values.Length > 0)
            {
                List<string> indexerValuesRaw = [];
                sb.Clear();
                int i2 = i;
                int bracketDepth = -1;
                while (i2 < chars.Length)
                {
                    char c2 = chars[i2++];
                    if (c2 == '[')
                    {
                        bracketDepth++;
                        if (bracketDepth > 0)
                            sb.Append('[');
                    }
                    else if (c2 == ']')
                    {
                        bracketDepth--;
                        if (bracketDepth >= 0)
                        {
                            sb.Append(']');
                        }
                        else
                        {
                            if (sb.Length == 0)
                                // an empty indexer is not valid, so we can discard here
                                break;

                            indexerValuesRaw.Add(sb.ToString());
                            sb.Clear();
                            if (i2 < chars.Length && chars[i2] == '[')
                                continue; // another indexer immediately follows
                            break;
                        }
                    }
                    else
                    {
                        sb.Append(c2);
                    }
                }

                // now let's dereference as many times as we can

                // wrap the dynamics in tokens which contain the indexing implementations
                // only if a full dereference is actually going to occur, though.
                Token[] tokenizedValues = previousCharacterWasDeref ? new Token[values.Length] : null;
                if (previousCharacterWasDeref)
                    for (int j = 0; j < values.Length; j++)
                        tokenizedValues[j] = PreprocessorUtils.DynamicToLiteral(values[j], -1);

                bool anyValidIndexer = false;
                bool indexValuesDirectly = !previousCharacterWasDeref;
                int jumpIndexForward = 0; // number of indices `i` will move forward due to indexers being accepted
                foreach (string indexerValueRaw in indexerValuesRaw)
                {
                    string indexerValueResolved = ResolveStringV2(indexerValueRaw);
                    var indexer = TokenIndexer.CreateIndexerFromString(indexerValueResolved);
                    if (indexer == null)
                        break;
                    if (indexValuesDirectly)
                    {
                        indexValuesDirectly = false;
                        if (indexer is not TokenIndexerInteger _integerIndexer)
                            break;
                        int indexerInteger = _integerIndexer.token.number;
                        if (indexerInteger < 0 || indexerInteger >= values.Length)
                            break;
                        tokenizedValues = new Token[1];
                        dynamic dynamicValue = values[indexerInteger]; // evil dynamic
                        tokenizedValues[0] = PreprocessorUtils.DynamicToLiteral(dynamicValue, -1);
                    }
                    else
                    {
                        // indexers should affect the last token
                        Token lastToken = tokenizedValues[^1];
                        if (lastToken is not IIndexable indexable)
                            break;
                        Token resultToken = indexable.Index(indexer, null);
                        if (resultToken == null)
                            break; // indexing was unsuccessful
                        tokenizedValues[^1] = resultToken;
                    }

                    jumpIndexForward += indexerValueRaw.Length + 2;
                    anyValidIndexer = true;
                }

                // none of the indexers were valid, and there was no deref operator, so we have to abort
                if (!previousCharacterWasDeref && !anyValidIndexer)
                {
                    output.Append('\\', previousNumberOfBackslashes / 2);
                    output.Append(sb, amountTrimmedFromBeginning,
                        sb.Length - amountTrimmedFromBeginning - amountTrimmedFromEnd);
                    previousNumberOfBackslashes = 0;
                    previousCharacterWasDeref = false;
                    continue;
                }

                i += jumpIndexForward;
                valuesFinal = tokenizedValues.Select(v => v.ToString()).ToArray();
            }
            else if (values.Length == 0)
            {
                valuesFinal = [];
            }
            else
            {
                valuesFinal = values.Select(v => (string) v.ToString()).ToArray();
            }

            string insertText = valuesFinal.Length > 1
                ? string.Join(" ", valuesFinal)
                : valuesFinal.Length == 1
                    ? valuesFinal[0]
                    : "";
            output.Append(insertText);
            previousNumberOfBackslashes = 0;
            previousCharacterWasDeref = false;
        }

        return output.ToString();

        // local functions

        string FindLongestExistingPPV(string candidateName, bool shrinkFromEnd, out int shrunkAmount)
        {
            shrunkAmount = 0;
            if (TryGetPPV(candidateName, out _))
                return candidateName;

            if (shrinkFromEnd)
                for (int j = candidateName.Length - 1; j > 1; j--)
                {
                    shrunkAmount++;
                    string substring = candidateName[..j];
                    if (TryGetPPV(substring, out _))
                        return substring;
                }
            else
                for (int j = 1; j < candidateName.Length; j++)
                {
                    shrunkAmount++;
                    string substring = candidateName[j..];
                    if (TryGetPPV(substring, out _))
                        return substring;
                }

            return null;
        }
    }

    /// <summary>
    ///     Resolve an unresolved PPV's literals. Returns an array of all the tokens contained inside.
    /// </summary>
    /// <param name="unresolved">The unresolved PPV.</param>
    /// <param name="thrower">The statement that would be the cause of the error, if any.</param>
    /// <returns></returns>
    /// <exception cref="StatementException"></exception>
    public TokenLiteral[] ResolvePPV(TokenIdentifierPreprocessor unresolved, Statement thrower)
    {
        int line = unresolved.lineNumber;
        PreprocessorVariable values = unresolved.variable;
        var literals = new TokenLiteral[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            dynamic value = values[i];

            if (value is TokenLiteral literal)
            {
                literals[i] = literal;
                continue;
            }

            TokenLiteral wrapped = PreprocessorUtils.DynamicToLiteral(value, line);
            literals[i] = wrapped ?? throw new StatementException(thrower,
                $"Found unexpected value in PPV '{unresolved.word}': {value.ToString()}");
        }

        return literals;
    }

    public void PushFile(CommandFile file) { this.currentFiles.Push(file); }

    public void PopFileDiscard()
    {
        this.unreachableCode = -1;
        _ = this.currentFiles.Pop();
    }

    public void PopFile()
    {
        this.unreachableCode = -1;

        CommandFile file = this.currentFiles.Pop();

        if (!file.IsValidTest)
        {
            // test doesnt have any assertions
            RuntimeFunction func = file.runtimeFunction;
            Statement creationStatement = func.creationStatement;
            throw new StatementException(creationStatement,
                $"Test '{func.internalName}' does not contain any assert statements, and thus will always pass.");
        }

        if (file.IsTest) // test is valid
        {
            // test related stuff
            int testId = ++this.testCount;
            RuntimeFunction func = file.runtimeFunction;
            Statement creationStatement = func.creationStatement;

            CommandFile tests = this.TestsFile;
            tests.Add($"# Test {testId}: {func.internalName}, located at line {creationStatement.Lines[0]}");
            tests.Add(Command.Function(file));
            tests.Add(Command.Tellraw("@s",
                new RawTextJsonBuilder().AddTerms(new JSONText($"§aTest {testId} ({func.internalName}) passed."))
                    .BuildString()));
            tests.Add("");
        }

        if (ReferenceEquals(file, this.InitFile))
            return; // do not write the init file until the whole program is finished.

        // file is empty, so it causes minecraft errors if we don't do this
        if (file.Length == 0)
        {
            if (file.IsRootFile)
                return; // no need to save down the root file if it doesn't exist.
            file.Add("# empty file");
        }

        this.emission.AddFile(file);
    }

    /// <summary>
    ///     Pops the first item off the stack that matches the given predicate, starting from the top.
    /// </summary>
    /// <param name="condition">The condition to match.</param>
    public void PopFirstFile(Predicate<CommandFile> condition)
    {
        List<CommandFile> files = this.currentFiles.ToList();
        int indexOfMatch = files.FindIndex(condition);

        if (indexOfMatch == -1)
        {
            PopFile();
            return;
        }

        CommandFile file = files[indexOfMatch];
        files.RemoveAt(indexOfMatch);
        this.currentFiles = new Stack<CommandFile>(files);

        this.unreachableCode = -1;

        if (!file.IsValidTest)
        {
            // test doesnt have any assertions
            RuntimeFunction func = file.runtimeFunction;
            Statement creationStatement = func.creationStatement;
            throw new StatementException(creationStatement,
                $"Test '{func.internalName}' does not contain any assert statements, and thus will always pass.");
        }

        if (file.IsTest) // test is valid
        {
            // test related stuff
            int testId = ++this.testCount;
            RuntimeFunction func = file.runtimeFunction;
            Statement creationStatement = func.creationStatement;

            CommandFile tests = this.TestsFile;
            tests.Add($"# Test {testId}: {func.internalName}, located at line {creationStatement.Lines[0]}");
            tests.Add(Command.Function(file));
            tests.Add(Command.Tellraw("@s",
                new RawTextJsonBuilder().AddTerms(new JSONText($"§aTest {testId} ({func.internalName}) passed."))
                    .BuildString()));
            tests.Add("");
        }

        if (ReferenceEquals(file, this.InitFile))
            return; // do not write the init file until the whole program is finished.

        // file is empty, so it causes minecraft errors if we don't do this
        if (file.Length == 0)
        {
            if (file.IsRootFile)
                return; // no need to save down the root file if it doesn't exist.
            file.Add("# empty file");
        }

        this.emission.AddFile(file);
    }

    private void FinalizeInitFile()
    {
        if (this.TestsFile != null)
        {
            this.TestsFile.AddTop("");
            this.TestsFile.AddTop(Command.Function(this.InitFile));
            this.TestsFile.AddTop("# Run the initialization file to make sure any needed data is there.");
        }

        if (this.initCommands.Count <= 0)
            return;

        this.InitFile.AddTop("");
        this.InitFile.AddTop(this.initCommands);

        if (GlobalContext.Decorate)
        {
            this.InitFile.AddTop(
                "# The purpose of this file is to prevent constantly re-calling `objective add` commands when it's not needed. If you're having strange issues, re-running this may fix it.");
            this.InitFile.AddTop(
                "# Runtime setup is placed here in the 'init file'. Re-run this ingame to ensure new scoreboard objectives are properly created.");
        }

        this.emission.AddFile(this.InitFile);
    }

    /// <summary>
    ///     Construct the next available name using a sequential number to distinguish it, like input0, input1, input2, etc...
    /// </summary>
    /// <param name="friendlyName">The name to get the next number for.</param>
    /// <param name="showNumberOnFirst">If a number should be appended to the first item with a generated name.</param>
    /// <param name="oneIndexed">If the file numbers should be indexed by 1 rather than 0.</param>
    /// <returns>friendlyName[next available index]</returns>
    public static string GetNextGeneratedName(string friendlyName, bool showNumberOnFirst, bool oneIndexed)
    {
        int index = generatedNames.GetValueOrDefault(friendlyName, 0);
        generatedNames[friendlyName] = index + 1;

        if (index == 0 && !showNumberOnFirst)
            return friendlyName;

        return friendlyName + (index + (oneIndexed ? 1 : 0));
    }

    /// <summary>
    ///     Construct the next available command file (in the generated folder) using a sequential number to distinguish it,
    ///     like input0, input1, input2, etc...
    /// </summary>
    /// <param name="friendlyName">A user-friendly name to mark the file by.</param>
    /// <param name="oneIndexed">If the file numbers should be indexed by 1 rather than 0.</param>
    /// <returns>A command file titled <c>friendlyName[next available index]</c></returns>
    public static CommandFile GetNextGeneratedFile(string friendlyName, bool oneIndexed)
    {
        string name = GetNextGeneratedName(friendlyName, true, oneIndexed);
        return new CommandFile(true, name, MCC_GENERATED_FOLDER);
    }

    public static void ResetGeneratedNames() { generatedNames.Clear(); }

    /// <summary>
    ///     Do a cleanup of the executor's resources and call <see cref="GC.Collect()" />
    /// </summary>
    internal void Cleanup()
    {
        this.currentFiles.Clear();
        this.loadedFiles.Clear();
        this.ppv.Clear();
        this.definedTags.Clear();
        this.scoreboard.values.Clear();
        this.scoreboard.temps.Clear();
        GC.Collect();
    }

    [GeneratedRegex(_FSTRING_SELECTOR)]
    private static partial Regex FStringSelectorRegex();
    [GeneratedRegex(_FSTRING_VARIABLE)]
    private static partial Regex FStringVariableRegex();

#pragma warning disable SYSLIB1045

#pragma warning restore SYSLIB1045
}