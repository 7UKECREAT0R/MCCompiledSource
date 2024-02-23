using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.MCC.Functions;
using mc_compiled.Modding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using mc_compiled.Commands.Execute;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Scheduling;
using mc_compiled.Modding.Resources.Localization;
using JetBrains.Annotations;
using mc_compiled.Compiler;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.Modding.Behaviors.Dialogue;
using mc_compiled.Modding.Resources;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// The final stage of the compilation process. Runs statements and holds state.
    /// </summary>
    public class Executor
    {
        private const string _FSTRING_SELECTOR = @"@(?:[spaeri]|initiator)(?:\[.+\])?";
        private const string _FSTRING_VARIABLE = @"[\w\d_\-:]+";
        private static readonly Regex PPV_FMT = new Regex(@"\\*\$[\w\d]+");

        public static readonly Regex FSTRING_SELECTOR = new Regex(_FSTRING_SELECTOR);
        public static readonly Regex FSTRING_VARIABLE = new Regex(_FSTRING_VARIABLE);
        public const decimal MCC_VERSION = 1.17M;                 // _compiler
        public static string MINECRAFT_VERSION = "0.00.000";    // _minecraft
        public const string MCC_GENERATED_FOLDER = "compiler";  // folder that generated functions go into
        public const string MCC_TESTS_FOLDER = "tests";         // folder that generated tests go into
        public const string MCC_TRANSLATE_PREFIX = "mcc.generated.";
        public const string UNDOCUMENTED_TEXT = "This symbol has no documentation.";
        public const string FAKEPLAYER_NAME = "_";
        public static int MAXIMUM_DEPTH = 100;

        internal const string LANGUAGE_FILE = "language.json";
        internal const string BINDINGS_FILE = "bindings.json";

        /// <summary>
        /// Display a success message regardless of debug setting.
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
        /// Display a warning regardless of debug setting.
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

        private int iterationsUntilDeferProcess = 0;
        /// <summary>
        /// Defer an action to happen after the next valid statement has run.
        /// </summary>
        /// <param name="action"></param>
        public void DeferAction(Action<Executor> action)
        {
            this.iterationsUntilDeferProcess += 2;
            this.deferredActions.Push(action);
        }
        /// <summary>
        /// Process all deferred actions currently in the queue.
        /// </summary>
        private void ProcessDeferredActions()
        {
            while(this.deferredActions.Any())
            {
                Action<Executor> action = this.deferredActions.Pop();
                action.Invoke(this);
            }
        }

        internal readonly EntityManager entities;
        internal readonly ProjectManager project;

        private Statement[] statements;
        private int readIndex = 0;
        private int unreachableCode = -1;
        private readonly Stack<Action<Executor>> deferredActions;
        private readonly Dictionary<int, object> loadedFiles;
        private readonly List<int> definedStdFiles;
        private readonly bool[] lastPreprocessorCompare;
        private readonly PreviousComparisonStructure[] lastCompare;
        private readonly StringBuilder prependBuffer;
        private readonly Stack<CommandFile> currentFiles;
        private int testCount;
        
        internal int depth;
        internal bool linting;
        internal readonly ScoreboardManager scoreboard;
        internal readonly List<Macro> macros;
        internal readonly FunctionManager functions;
        internal readonly HashSet<string> definedTags;
        internal readonly HashSet<Typedef> definedReturnedTypes;
        internal readonly Dictionary<string, PreprocessorVariable> ppv;

        internal Executor(Statement[] statements, IReadOnlyCollection<Program.InputPPV> inputPPVs,
            string projectName, string bpBase, string rpBase)
        {
            this.statements = statements;
            this.project = new ProjectManager(projectName, bpBase, rpBase, this);
            this.entities = new EntityManager(this);

            this.definedStdFiles = new List<int>();
            this.ppv = new Dictionary<string, PreprocessorVariable>(StringComparer.OrdinalIgnoreCase);
            this.macros = new List<Macro>();
            this.definedTags = new HashSet<string>();
            this.definedReturnedTypes = new HashSet<Typedef>();

            if (inputPPVs != null && inputPPVs.Count > 0)
                foreach (Program.InputPPV ppv in inputPPVs)
                    SetPPV(ppv.name, ppv.value);

            // support up to MAXIMUM_SCOPE levels of scope before blowing up
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
        /// Returns this executor after setting it to lint mode, lowering memory usage
        /// </summary>
        /// <returns></returns>
        internal void Linter()
        {
            this.linting = true;
            this.project.Linter();
        }

        /// <summary>
        /// Pushes to the prepend buffer the proper execute command needed to align to the given selector.
        /// Sets the selector given by the reference parameter to @s.
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
        /// Resolve an FString into rawtext terms. Also adds all setup commands for variables.
        /// </summary>
        /// <param name="fstring">The string to resolve.</param>
        /// <param name="forExceptions">The statement to use for exceptions.</param>
        /// <param name="advanced">If this FString is 'advanced' as in, requires per-user execution.</param>
        /// <returns></returns>
        public List<JSONRawTerm> FString(string fstring, Statement forExceptions, out bool advanced)
        {
            advanced = false;
            var terms = new List<JSONRawTerm>();
            var buffer = new StringBuilder();

            int scoreIndex = 0;
            for(int i = 0; i < fstring.Length; i++)
            {
                char character = fstring[i];

                if(character == '{')
                {
                    // scan for closing bracket and return length
                    int segmentLength = ScanForCloser(fstring, i);
                    if (segmentLength == -1)
                    {
                        // append the rest of the string and cancel.
                        buffer.Append(fstring.Substring(i));
                        break;
                    }

                    // get segment inside brackets
                    string segment = fstring.Substring(i + 1, segmentLength - 1);

                    // skip this if the segment is empty.
                    if(string.IsNullOrWhiteSpace(segment))
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
                    Token[] tokens = tokenizer.Tokenize();
                    Statement subStatement = new StatementHusk(tokens);
                    subStatement.SetSource(forExceptions.Lines, "Inline FString Operation");

                    // squash & resolve the tokens as best it can
                    subStatement.PrepareThis(this);

                    // dump text buffer before we get started.
                    i += segmentLength;
                    DumpTextBuffer();

                    // get the rawtext terms
                    Token[] remaining = subStatement.GetRemainingTokens();
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

                                if(!identifierValue.value.clarifier.IsGlobal)
                                    advanced = true;

                                ScoreboardValue value = identifierValue.value;
                                int indexCopy = scoreIndex;

                                // type implementation called here
                                (string[] rtCommands, JSONRawTerm[] rtTerms) = value.ToRawText(ref indexCopy);
                            
                                AddCommandsClean(rtCommands, "string" + value.InternalName,
                                    $"Prepares the variable '{value.Name}' to be displayed in a rawtext. Invoked at {file.CommandReference} line {this.NextLineNumber}");

                                // localize and flatten the array.
                                terms.AddRange(rtTerms.SelectMany(term => term.Localize(this, forExceptions)));

                                scoreIndex++;
                                continue;
                            }
                        }

                        // default representation
                        string stringRepresentation = ResolveString(token.ToString());
                        if(!string.IsNullOrEmpty(stringRepresentation))
                            terms.AddRange(new JSONText(stringRepresentation).Localize(this, forExceptions));
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
                string bufferContents = ResolveString(buffer.ToString());
                
                if (string.IsNullOrEmpty(bufferContents))
                    return;
                
                terms.AddRange(new JSONText(bufferContents).Localize(this, forExceptions));
                buffer.Clear();
            }

            // stupid complex search for closing bracket: '}'
            int ScanForCloser(string str, int start)
            {
                int len = str.Length;
                int depth = -1;
                bool escape = false;

                for(int i = start + 1; i < len; i++)
                {
                    char c = str[i];

                    switch (c)
                    {
                        case '\\':
                            escape = !escape;
                            continue;
                        // actual bracket stuff
                        case '{':
                            depth++;
                            continue;
                        case '}':
                        {
                            depth--;
                            if (depth < 0)
                                return i - start;
                            break;
                        }
                    }
                }

                return -1;
            }
        }
        /// <summary>
        /// Append these terms to the end of this command. Will resolve <see cref="JSONVariant"/>s and construct the command combinations.
        /// </summary>
        /// <param name="terms">The terms constructed by FString.</param>
        /// <param name="command">The command to append the terms to.</param>
        /// <param name="root">If this is the root call.</param>
        /// <param name="builder">The ExecuteBuilder that will hold all score checks in it.</param>
        /// <param name="commands">Used for recursion, set to null.</param>
        /// <param name="copy">The existing terms to copy from.</param>
        /// <returns></returns>
        public static string[] ResolveRawText(List<JSONRawTerm> terms, string command, bool root = true,
            ExecuteBuilder builder = null, List<string> commands = null, RawTextJsonBuilder copy = null)
        {
            var jb = new RawTextJsonBuilder(copy);

            if (builder == null)
                builder = Command.Execute();
            if(commands == null)
                commands = new List<string>();

            for(int i = 0; i < terms.Count; i++)
            {
                JSONRawTerm term = terms[i];
                if (term is JSONVariant variant)
                {
                    // calculate all variants
                    foreach(ConditionalTerm possibleVariant in variant.terms)
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
                else
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

            if (root)
                return commands.ToArray();

            return null; // return value isn't used in this case
        }

        private LanguageManager languageManager;
        private SoundDefinitions soundDefinitions;
        private DialogueManager dialogueDefinitions;
        
        /// <summary>
        /// Returns the active locale, if any. Set using <see cref="SetLocale(string)"/>.
        /// </summary>
        private LocaleDefinition ActiveLocale { get; set; }

        /// <summary>
        /// Returns if a locale has been set via <see cref="SetLocale(string)"/>.
        /// </summary>
        public bool HasLocale => this.ActiveLocale != null;

        /// <summary>
        /// Sets the active locale that FString data will be sent to.
        /// </summary>
        /// <param name="locale"></param>
        public void SetLocale(string locale)
        {
            if (this.languageManager == null)
            {
                this.languageManager = new LanguageManager(this);
                AddExtraFile(this.languageManager);
            }

            this.ActiveLocale = this.languageManager.DefineLocale(locale);
        }
        /// <summary>
        /// Sets a locale entry in the associated .lang file. Throws a <see cref="StatementException"/> if no locale has been set yet via <see cref="SetLocale(string)"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="forExceptions"></param>
        /// <param name="overwrite"></param>
        /// <returns>The entry that *should* be used, in the case that a merge occurred. If null is returned, then translation should not be used for this input string at all.</returns>
        /// <exception cref="StatementException"></exception>
        public LangEntry? SetLocaleEntry(string key, string value, Statement forExceptions, bool overwrite)
        {
            // null cases
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (!value.Any(char.IsLetter))
                return null;
            
            if (!this.HasLocale)
                throw new StatementException(forExceptions, "No language has been set to write to. See the 'lang' command.");

            bool merge;

            if(this.ppv.TryGetValue(LanguageManager.MERGE_PPV, out PreprocessorVariable val))
                merge = (bool)val[0];
            else
                merge = false;

            var entry = LangEntry.Create(key, value);
            return this.ActiveLocale.file.Add(entry, overwrite, merge);
        }
        /// <summary>
        /// Gets the sound definitions file, reading it from the existing RP if it exists.
        /// </summary>
        public SoundDefinitions GetSoundDefinitions(Statement callingStatement)
        {
            if (this.soundDefinitions != null)
                return this.soundDefinitions;

            string file = this.project.GetOutputFileLocationFull(OutputLocation.r_SOUNDS, SoundDefinitions.FILE);

            if (File.Exists(file))
            {
                if (!(LoadJSONFile(file, callingStatement) is JObject jObject))
                    throw new StatementException(callingStatement, $"File RP/sounds/{SoundDefinitions.FILE} was not a JSON Object.");

                this.soundDefinitions = SoundDefinitions.Parse(jObject, callingStatement);
                AddExtraFile(this.soundDefinitions);
                return this.soundDefinitions;
            }

            this.soundDefinitions = new SoundDefinitions(FormatVersion.r_SOUNDS.ToString());
            AddExtraFile(this.soundDefinitions);
            return this.soundDefinitions;
        }

        /// <summary>
        /// Get the path of a file relative to the working directory.
        /// </summary>
        /// <param name="file">The file path.</param>
        /// <param name="_workingDirectory">The working directory path.</param>
        /// <returns>The relative path of the file, or null if the file is not contained within the working directory.</returns>
        [CanBeNull, Pure]
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
                relative = relative.Substring(indexOfSlash + 1);

            if (!relative.Contains(Path.DirectorySeparatorChar))
                return null;
            
            return relative;
        }

        /// <summary>
        /// Adds a new sound definition to the project, or returns an existing one if it already exists.
        /// </summary>
        /// <param name="soundFile">The path to the sound file to be added.</param>
        /// <param name="category">The category of the sound.</param>
        /// <param name="callingStatement">The calling statement that is adding the sound definition.</param>
        /// <returns>A new <see cref="SoundDefinition"/> object representing the added sound definition.</returns>
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
            string soundName = this.project.Identifier + '.' + Path.GetFileNameWithoutExtension(soundFile);
            
            // create CopyFile so that the sound file can be copied during file writing
            var copyFile = new CopyFile(soundFile, OutputLocation.r_SOUNDS, relativePath ?? fileName);
            AddExtraFile(copyFile);

            SoundDefinitions soundDefinitions = GetSoundDefinitions(callingStatement);
            var soundDefinition = new SoundDefinition(soundName, fileName, category, soundFolder.ToString());
            
            if (soundDefinitions.TryGetSoundDefinition(soundDefinition.CommandReference, out SoundDefinition existing))
                return existing;
            
            soundDefinitions.AddSoundDefinition(soundDefinition);
            return soundDefinition;
        }
        /// <summary>
        /// Gets the instance of Dialogue registry.
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
        /// Marks all files on the file stack as containing an assertion.
        /// </summary>
        public void MarkAssertionOnFileStack()
        {
            foreach (CommandFile file in this.currentFiles)
                file.MarkAssertion();
        }
        /// <summary>
        /// Tells the executor that the next line in the current block is be unreachable.
        /// </summary>
        public void UnreachableCode() => this.unreachableCode = 1;
        /// <summary>
        /// Throw a StatementException if a feature is not enabled.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="feature"></param>
        internal void RequireFeature(Statement source, Feature feature)
        {
            if (this.project.HasFeature(feature))
                return;

            string name = feature.ToString();
            throw new StatementException(source, $"Feature not enabled: {name}. Enable using the command 'feature {name.ToLower()}' at the top of the file.");
        }
        /// <summary>
        /// Checks if the execution context is currently in an unreachable area, and throw an exception if true.
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
        /// Load JSON file with caching for next use.
        /// </summary>
        /// <param name="path">The path to the JSON file.</param>
        /// <param name="callingStatement">The calling statement.</param>
        /// <returns></returns>
        public JToken LoadJSONFile(string path, Statement callingStatement)
        {
            int hash = path.GetHashCode();

            // cached file, dont read again
            if (this.loadedFiles.TryGetValue(hash, out object value))
            {
                if (value is JToken token)
                    return token;
            }

            if(string.IsNullOrWhiteSpace(path))
                throw new StatementException(callingStatement, "Empty JSON file path.");

            if(!File.Exists(path))
                throw new StatementException(callingStatement, "File \'" + path + "\' could not be found. Make sure you are in the right working directory.");

            string contents = File.ReadAllText(path);
            JToken json = JToken.Parse(contents);
            this.loadedFiles[hash] = json;
            return json;
        }

        /// <summary>
        /// Load JSON file with caching for next use, under a different hashcode.
        /// </summary>
        /// <param name="path">The path to the JSON file.</param>
        /// <param name="hash">The hash to use for remembering if the path is loaded once yet.</param>
        /// <param name="callingStatement">The statement to use for exceptions.</param>
        /// <returns></returns>
        public JToken LoadJSONFile(string path, int hash, Statement callingStatement)
        {
            // cached file, dont read again
            if (this.loadedFiles.TryGetValue(hash, out object value))
            {
                if (value is JToken token)
                    return token;
            }

            if (string.IsNullOrWhiteSpace(path))
                throw new StatementException(callingStatement, "Empty JSON file path.");

            if (!File.Exists(path))
                throw new StatementException(callingStatement, "File \'" + path + "\' could not be found. Make sure you are in the right working directory.");

            string contents = File.ReadAllText(path);
            JToken json = JToken.Parse(contents);
            this.loadedFiles[hash] = json;
            return json;
        }
        /// <summary>
        /// Load file with caching for next use.
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
        /// Load file with caching for next use.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns></returns>
        public byte[] LoadFileBytes(string path)
        {
            int hash = path.GetHashCode();

            // cached file, dont read again
            if (this.loadedFiles.TryGetValue(hash, out object value))
            {
                switch (value)
                {
                    case string s:
                        return Encoding.UTF8.GetBytes(s);
                    case byte[] bytes:
                        return bytes;
                }
            }

            byte[] contents = File.ReadAllBytes(path);
            this.loadedFiles[hash] = contents;
            return contents;
        }

        /// <summary>
        /// Attempts to fetch an addon file from the current BP, or downloads it from the default pack if not present. Uses caching.
        /// </summary>
        public JToken Fetch(IAddonFile toLocate, Statement callingStatement)
        {
            string outputFile = this.project.GetOutputFileLocationFull(toLocate, true);

            if (outputFile == null)
                throw new StatementException(callingStatement, "Attempted to fetch an unwritable file. Report this to the developers.");

            int outputFileHash = outputFile.GetHashCode();

            // the JSON root of the file
            JToken root;

            // find the user-defined entity file, or default to the vanilla pack provided by Microsoft
            if (this.loadedFiles.TryGetValue(outputFileHash, out object jValue))
                root = jValue as JToken;
            else
            {
                if (File.Exists(outputFile))
                    root = LoadJSONFile(outputFile, callingStatement);
                else
                {
                    string pathString = toLocate.GetOutputLocation().ToString();
                    var packType = TemporaryFilesManager.PackType.BehaviorPack;

                    if(pathString.StartsWith("r_"))
                    {
                        packType = TemporaryFilesManager.PackType.ResourcePack;
                        pathString = pathString.Substring(2);
                    }
                    if (pathString.StartsWith("b_"))
                    {
                        pathString = pathString.Substring(2);
                    }

                    string[] paths = pathString.Split(new[] { "__" }, StringSplitOptions.None);
                    string[] filePath = new string[paths.Length + 1];
                    for (int i = 0; i < paths.Length; i++)
                    {
                        string current = paths[i];
                        filePath[i] = current.ToLower();
                    }
                    filePath[paths.Length] = toLocate.GetOutputFile(); // last index

                    string downloadedFile = TemporaryFilesManager.Get(packType, filePath);
                    root = LoadJSONFile(downloadedFile, outputFileHash, null);
                }
            }

            return root;
        }

        /// <summary>
        /// Define a file that sort-of equates to a "standard library." Will only be added once.
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
        /// Determines if the given CommandFile exists in the definedStdFiles list.
        /// </summary>
        /// <param name="file">The CommandFile object to check for.</param>
        /// <returns>True if the CommandFile exists in the definedStdFiles list; otherwise, false.</returns>
        public bool HasSTDFile(CommandFile file)
        {
            return this.definedStdFiles.Contains(file.GetHashCode());
        }

        /// <summary>
        /// Returns if this executor has another statement available to run.
        /// </summary>
        public bool HasNext => this.readIndex < this.statements.Length;

        /// <summary>
        /// Tries to fetch a documentation string based whether the last statement was a comment or not. Returns <see cref="Executor.UNDOCUMENTED_TEXT"/> if no documentation was supplied.
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
                return ResolveString(comment.comment);
            }

            hadDocumentation = false;
            return UNDOCUMENTED_TEXT;
        }

        /// <summary>
        /// Peek at the next statement.
        /// </summary>
        /// <returns></returns>
        public Statement Peek() => this.statements[this.readIndex];
        /// <summary>
        /// Peek at the statement N statements in front of the read index. 0: current, 1: next, etc...
        /// Does perform bounds checking, and returns null if outside bounds.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Statement PeekSkip(int amount)
        {
            int index = this.readIndex + amount;

            if (index < 0)
                return null;
            else if (index < this.statements.Length)
                return this.statements[index];
            else
                return null;
        }
        /// <summary>
        /// Peek at the last statement that was gotten, if any.
        /// </summary>
        /// <returns><b>null</b> if no statements have been gotten yet.</returns>
        public Statement PeekLast()
        {
            if (this.readIndex > 1)
                return this.statements[this.readIndex - 2];

            return null;
        }

        /// <summary>
        /// Seek for the next statement that has valid executable data. Returns null if outside of bounds.
        /// </summary>
        /// <returns></returns>
        public Statement Seek() => SeekSkip(0);
        /// <summary>
        /// Seek forward from the statement N statements in front of the read index until it finds one with valid executable data. 0: current, 1: next, etc...
        /// Does perform bounds checking, and returns null if outside bounds.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Statement SeekSkip(int amount)
        {
            Statement statement = null;

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
        /// Seek backwards from the current statement until it finds one with valid executable data. Returns null if outside of bounds.
        /// </summary>
        /// <returns><b>null</b> if no statements have been gotten yet.</returns>
        public Statement SeekLast()
        {
            Statement statement = null;

            int i = this.readIndex - 1;

            if(i < 0)
                return null; // no statements??

            do
            {
                statement = this.statements[i--];
            } while ((statement == null || statement.Skip) && i >= 0);

            return statement;
        }

        /// <summary>
        /// Get the next statement to be read and then increment the read index.
        /// </summary>
        /// <returns></returns>
        public Statement Next() => this.statements[this.readIndex++];
        /// <summary>
        /// Returns the next statement to be read as a certain type. Increments the read index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Next<T>() where T : Statement => this.statements[this.readIndex++] as T;
        /// <summary>
        /// Peek at the next statement as a certain type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Peek<T>() where T : Statement => this.statements[this.readIndex] as T;
        /// <summary>
        /// Peek a certain number of statements into the future as a certain type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skip"></param>
        /// <returns></returns>
        public T Peek<T>(int skip) where T : Statement => this.statements[this.readIndex + skip] as T;
        /// <summary>
        /// Returns if there's another statement available and it's of a certain type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool NextIs<T>() where T : Statement => this.HasNext && this.statements[this.readIndex] is T;
        /// <summary>
        /// Returns if the next statement is an unknown statement with builder field(s) in it.
        /// </summary>
        /// <returns></returns>
        public bool NextIsBuilder()
        {
            if (!this.HasNext)
                return false;

            Statement _tokens = this.statements[this.readIndex];

            if (!(_tokens is StatementUnknown tokens))
                return false;

            return tokens.NextIs<TokenBuilderIdentifier>();
        }
        public bool NextBuilderField(ref Statement tokens, out TokenBuilderIdentifier builderField)
        {
            // next in statement?
            if (tokens.NextIs<TokenBuilderIdentifier>())
            {
                builderField = tokens.Next<TokenBuilderIdentifier>(null);
                return true;
            }

            // not in the current statement ... look ahead in code
            if(NextIsBuilder())
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
        /// Return an array of the next x statements.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Statement[] Peek(int amount)
        {
            if (amount == 0)
                return Array.Empty<Statement>();

            var ret = new Statement[amount];

            int write = 0;
            for (int i = this.readIndex; i < this.statements.Length && i < this.readIndex + amount; i++)
                ret[write++] = this.statements[i];

            return ret;
        }
        /// <summary>
        /// Reads the next statement, or set of statements if it is a block. Similar to Next() in that it updates the readIndex.
        /// </summary>
        /// <returns></returns>
        public Statement[] NextExecutionSet()
        {
            Statement current = this.statements[this.readIndex - 1];

            if (!this.HasNext)
                throw new StatementException(current, "Unexpected end-of-file while expecting statement/block.");

            if(NextIs<StatementOpenBlock>())
            {
                var block = Next<StatementOpenBlock>();
                int statements = block.statementsInside;
                Statement[] code = Peek(statements);
                this.readIndex += statements;
                this.readIndex++; // block closer
                return code;
            }

            Statement next = Next();

            if (!(next is IExecutionSetPart))
                throw new StatementException(current, "Following statement '" + next.Source + "' cannot be explicitly run.");

            return new[] { next };
        }

        /// <summary>
        /// Pop the prepend buffer's contents and return it.
        /// </summary>
        /// <returns></returns>
        private string PopPrepend()
        {
            string ret = this.prependBuffer.ToString();
            this.prependBuffer.Clear();
            return ret;
        }

        /// <summary>
        /// Setup the default preprocessor variables.
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
        /// Run this executor start to finish.
        /// </summary>
        public void Execute()
        {
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
                
                // check for unreachable code due to halt directive
                CheckUnreachable(statement);

                // run deferred processes
                if(this.iterationsUntilDeferProcess > 0)
                {
                    this.iterationsUntilDeferProcess--;
                    if (this.iterationsUntilDeferProcess == 0)
                        ProcessDeferredActions();
                }

                if(Program.DEBUG)
                    Console.WriteLine("EXECUTE LN{0}: {1}", statement.Lines[0], statement.ToString());
            }

            while (this.currentFiles.Any())
                PopFile();

            FinalizeInitFile();
        }
        /// <summary>
        /// Temporarily run another subsection of statements then resume this executor.
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

                // run deferred processes
                if (this.iterationsUntilDeferProcess > 0)
                {
                    this.iterationsUntilDeferProcess--;
                    if (this.iterationsUntilDeferProcess == 0)
                        ProcessDeferredActions();
                }

                if (Program.DEBUG)
                    Console.WriteLine("EXECUTE SUBSECTION LN{0}: {1}", statement.Lines[0], statement.ToString());
            }

            // now its done, so restore state
            this.statements = restore0;
            this.readIndex = restore1;
        }

        /// <summary>
        /// Set the result of the last preprocessor-if comparison in this scope.
        /// </summary>
        /// <param name="value"></param>
        public void SetLastIfResult(bool value) => this.lastPreprocessorCompare[this.ScopeLevel] = value;
        /// <summary>
        /// Get the result of the last preprocessor-if comparison in this scope.
        /// </summary>
        /// <returns></returns>
        public bool GetLastIfResult() => this.lastPreprocessorCompare[this.ScopeLevel];
        
        /// <summary>
        /// Set the last comparison data used at the given/current scope level.
        /// </summary>
        /// <param name="set">The previous comparison that ran.</param>
        /// <param name="scope">The scope of this comparison. Leave null and it will be replaced with <see cref="ScopeLevel"/>.</param>
        internal void SetLastCompare(PreviousComparisonStructure set, int? scope = null)
        {
            if (scope == null)
                scope = this.ScopeLevel;

            this.lastCompare[scope.Value] = set;
        }
        /// <summary>
        /// Get the last comparison data used at the given/current scope level.
        /// </summary>
        /// <returns></returns>
        internal PreviousComparisonStructure GetLastCompare(int? scope = null)
        {
            if (scope == null)
                scope = this.ScopeLevel;

            return this.lastCompare[scope.Value];
        }

        /// <summary>
        /// Register a macro to be looked up later.
        /// </summary>
        /// <param name="macro">The macro to register.</param>
        public void RegisterMacro(Macro macro) => this.macros.Add(macro);
        /// <summary>
        /// Look for a macro present in this project.
        /// </summary>
        /// <param name="name">The name used to look up a macro.</param>
        /// <returns>A nullable <see cref="Macro"/> which contains the found macro, if any.</returns>
        public Macro? LookupMacro(string name)
        {
            foreach (Macro macro in this.macros.Where(macro => macro.Matches(name)))
                return macro;
            return null;
        }
        /// <summary>
        /// Tries to look for a macro present in this project under a certain name.
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
        /// Get the current file that should be written to.
        /// </summary>
        public CommandFile CurrentFile => this.currentFiles.Peek();
        internal CommandFile HeadFile { get; private set; }
        internal CommandFile InitFile { get; private set; }
        private CommandFile TestsFile { get; set; }

        /// <summary>
        /// Get the current scope level.
        /// </summary>
        public int ScopeLevel => this.currentFiles.Count - 1;
        /// <summary>
        /// Get if the base file (projectName.mcfunction) is the active file.
        /// </summary>
        public bool IsScopeBase => this.currentFiles.Count <= 1;
        /// <summary>
        /// The number of the next line that will be added.
        /// </summary>
        public int NextLineNumber => this.CurrentFile.Length + 1;

        public void CreateTestsFile()
        {
            this.TestsFile = new CommandFile(true, "test");
            AddExtraFile(this.TestsFile);
            this.TestsFile.Add("");
        }
        /// <summary>
        /// Add a command to the current file, with prepend buffer.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(string command)
        {
            if (this.linting)
            {
                PopPrepend();
                return;
            }

            this.CurrentFile.Add(PopPrepend() + command);
        }
        /// <summary>
        /// Add a set of commands into a new branching file unless inline is set.
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="friendlyName">The friendly name to give the generated file, if any.</param>
        /// <param name="friendlyDescription">The description to be placed at the top of the generated file, if any.</param>
        /// <param name="inline">Force the commands to be inlined rather than sent to a generated file.</param>
        public void AddCommands(IEnumerable<string> commands, string friendlyName, string friendlyDescription, bool inline = false)
        {
            if (this.linting)
                return;
            string[] commandsAsArray = commands as string[] ?? commands.ToArray();
            int count = commandsAsArray.Count();
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

            CommandFile file = GetNextGeneratedFile(friendlyName);

            if (friendlyDescription != null)
                file.Add("# " + friendlyDescription);

            file.Add(commandsAsArray);

            AddExtraFile(file);
            AddCommand(Command.Function(file));
        }
        /// <summary>
        /// Add a command to the current file, not modifying the prepend buffer.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommandClean(string command)
        {
            if (this.linting)
                return;
            string prepend = this.prependBuffer.ToString();
            this.CurrentFile.Add(prepend + command);
        }
        /// <summary>
        /// Add a set of commands into a new branching file, not modifying the prepend buffer.
        /// If inline is set, no branching file will be made.
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="friendlyName">The friendly name to give the generated file, if any.</param>
        /// <param name="friendlyDescription">The description to be placed at the top of the generated file, if any.</param>
        /// <param name="inline">Force the commands to be inlined rather than sent to a generated file.</param>
        public void AddCommandsClean(IEnumerable<string> commands, string friendlyName, string friendlyDescription, bool inline = false)
        {
            if (this.linting || commands == null)
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

            CommandFile file = GetNextGeneratedFile(friendlyName);

            if(friendlyDescription != null)
                file.Add("# " + friendlyDescription);

            file.Add(commandsAsArray);

            AddExtraFile(file);
            this.CurrentFile.Add(buffer + Command.Function(file));
        }
        /// <summary>
        /// Add a file on its own to the list.
        /// </summary>
        /// <param name="file"></param>
        public void AddExtraFile(IAddonFile file) => this.project.AddFile(file);
        /// <summary>
        /// Add a file to the list, removing any other file that has a matching name/directory.
        /// </summary>
        /// <param name="file"></param>
        public void OverwriteExtraFile(IAddonFile file)
        {
            this.project.RemoveDuplicatesOf(file);
            this.project.AddFile(file);
        }
        /// <summary>
        /// Add a set of files on their own to the list.
        /// </summary>
        /// <param name="files">The files to add.</param>
        public void AddExtraFiles(IEnumerable<IAddonFile> files) => this.project.AddFiles(files);
        /// <summary>
        /// Add a set of files on their own to the list, removing any other files that have a matching name/directory.
        /// </summary>
        /// <param name="files">The files to overwrite.</param>
        public void OverwriteExtraFiles(IEnumerable<IAddonFile> files)
        {
            foreach (IAddonFile file in files)
                OverwriteExtraFile(file);
        }
        /// <summary>
        /// Returns if this executor has a file containing a specific string.
        /// </summary>
        /// <param name="text">The text to check for.</param>
        /// <returns></returns>
        public bool HasExtraFileContaining(string text) =>
            this.project.HasFileContaining(text);

        private readonly List<string> initCommands = new List<string>();
        /// <summary>
        /// Add a command to the 'init' file. Does not affect the prepend buffer.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommandInit(string command)
        {
            if (this.linting || command == null)
                return;
            this.initCommands.Add(command);
        }
        /// <summary>
        /// Adds a set of commands to the 'init' file. Does not affect the prepend buffer.
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommandsInit(IEnumerable<string> commands)
        {
            if (this.linting || commands == null)
                return;
            
            string[] commandsAsArray = commands as string[] ?? commands.ToArray();
            
            if (!commandsAsArray.Any())
                return;

            this.initCommands.AddRange(commandsAsArray);
        }

        /// <summary>
        /// Gets the tick scheduler in this Executor, or creates a new one if it doesn't already exist.
        /// </summary>
        /// <returns></returns>
        public TickScheduler GetScheduler()
        {
            if (this.scheduler != null)
                return this.scheduler;
            
            // create new one
            this.scheduler = new TickScheduler(this);
            this.project.AddFile(this.scheduler);
            return this.scheduler;
        }

        private TickScheduler scheduler;
        
        /// <summary>
        /// Set the content that will prepend the next added command.
        /// </summary>
        /// <param name="content"></param>
        /// <returns>The old buffer's contents.</returns>
        public string SetCommandPrepend(string content)
        {
            string oldContent = this.prependBuffer.ToString();
            this.prependBuffer.Clear().Append(content);

            if (string.IsNullOrEmpty(oldContent))
                return "";

            return oldContent;
        }
        /// <summary>
        /// Append to the content to the prepend buffer.
        /// </summary>
        /// <param name="content"></param>
        public void AppendCommandPrepend(string content) => this.prependBuffer.Append(content);
        /// <summary>
        /// Prepend content to the prepend buffer.
        /// </summary>
        /// <param name="content"></param>
        public void PrependCommandPrepend(string content) => this.prependBuffer.Insert(0, content);

        /// <summary>
        /// Try to get a preprocessor variable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetPPV(string name, out PreprocessorVariable value)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("Tried to get PPV with empty name.");
            if (name[0] == '$')
                name = name.Substring(1);
            
            return this.ppv.TryGetValue(name, out value);
        }

        /// <summary>
        /// Set or create a preprocessor variable copied from an existing one.
        /// </summary>
        /// <param name="name">The name of the preprocessor variable to set or create.</param>
        /// <param name="values">The values to set for the preprocessor variable.</param>
        public void SetPPVCopy(string name, PreprocessorVariable values)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("Tried to set PPV with empty name.");
            if (name[0] == '$')
                name = name.Substring(1);
            this.ppv[name] = values.Clone();
        }
        /// <summary>
        /// Set or create a preprocessor variable.
        /// </summary>
        /// <param name="name">The name of the preprocessor variable.</param>
        /// <param name="values">The values to set for the preprocessor variable.</param>
        public void SetPPV(string name, params dynamic[] values)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("Tried to set PPV with empty name.");
            if (name[0] == '$')
                name = name.Substring(1);
            this.ppv[name] = new PreprocessorVariable(values);
        }

        /// <summary>
        /// Get the names of all registered preprocessor variables.
        /// </summary>
        public IEnumerable<string> PPVNames => this.ppv.Select(p => p.Key).ToArray();

        /// <summary>
        /// Resolve all unescaped preprocessor variables in a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
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

                // If there are an odd number of preceding backslashes, this is escaped.
                if (backslashes % 2 == 1)
                {
                    sb.Remove(match.Index + lastIndex, 1);
                    continue;
                }

                string ppvName = text.Substring(lastIndex + 1);
                
                if (!TryGetPPV(ppvName, out PreprocessorVariable values))
                    continue; // no ppv named that

                string insertText = values.Length > 1 ? string.Join(" ", values) : (string) values[0].ToString();

                sb.Remove(insertIndex, match.Length - offset);
                sb.Insert(insertIndex, insertText);
            }

            return sb.ToString();
        }
        /// <summary>
        /// Resolve an unresolved PPV's literals. Returns an array of all the tokens contained inside.
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

                if (wrapped == null)
                    throw new StatementException(thrower, $"Found unexpected value in PPV '{unresolved.word}': {value.ToString()}");

                literals[i] = wrapped;
            }
            return literals;

        }

        public void PushFile(CommandFile file) => this.currentFiles.Push(file);
        public void PopFileDiscard()
        {
            this.unreachableCode = -1;
            _ = this.currentFiles.Pop();
        }
        public void PopFile()
        {
            this.unreachableCode = -1;

            CommandFile file = this.currentFiles.Pop();

            if(!file.IsValidTest)
            {
                // test doesnt have any assertions
                RuntimeFunction func = file.runtimeFunction;
                Statement creationStatement = func.creationStatement;
                throw new StatementException(creationStatement, $"Test '{func.name}' does not contain any assert statements, and thus will always pass.");
            }

            if(file.IsTest) // test is valid
            {
                // test related stuff
                int testId = ++this.testCount;
                RuntimeFunction func = file.runtimeFunction;
                Statement creationStatement = func.creationStatement;

                CommandFile tests = this.TestsFile;
                tests.Add($"# Test {testId}: {func.name}, located at line {creationStatement.Lines[0]}");
                tests.Add(Command.Function(file));
                tests.Add(Command.Tellraw("@s", new RawTextJsonBuilder().AddTerms(new JSONText($"§aTest {testId} ({func.name}) passed.")).BuildString()));
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

            this.project.AddFile(file);
        }
        private void FinalizeInitFile()
        {
            CommandFile file = this.InitFile;

            if (this.TestsFile != null)
            {
                this.TestsFile.AddTop("");
                this.TestsFile.AddTop(Command.Function(file));
                this.TestsFile.AddTop("# Run the initialization file to make sure any needed data is there.");
            }

            if (this.initCommands.Count <= 0)
                return;
            
            file.AddTop("");
            file.AddTop(this.initCommands);

            if (Program.DECORATE)
            {
                file.AddTop("# The purpose of this file is to prevent constantly re-calling `objective add` commands when it's not needed. If you're having strange issues, re-running this may fix it.");
                file.AddTop("# Runtime setup is placed here in the 'init file'. Re-run this ingame to ensure new scoreboard objectives are properly created.");
            }

            this.project.AddFile(file);
        }

        private static readonly Dictionary<int, int> generatedNames = new Dictionary<int, int>();
        /// <summary>
        /// Construct the next available name using a sequential number to distinguish it, like input0, input1, input2, etc...
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <returns>friendlyName[next available index]</returns>
        public static string GetNextGeneratedName(string friendlyName)
        {
            int hash = friendlyName.GetHashCode();
            if (!generatedNames.TryGetValue(hash, out int index))
                index = 0;
            generatedNames[hash] = index + 1;
            return friendlyName + index;
        }
        /// <summary>
        /// Construct the next available command file (in the generated folder) using a sequential number to distinguish it, like input0, input1, input2, etc...
        /// </summary>
        /// <param name="friendlyName">A user-friendly name to mark the file by.</param>
        /// <returns>A command file titled "friendlyName[next available index]" </returns>
        public static CommandFile GetNextGeneratedFile(string friendlyName)
        {
            string name = GetNextGeneratedName(friendlyName);
            return new CommandFile(true, name, MCC_GENERATED_FOLDER);
        }
        public static void ResetGeneratedNames()
        {
            generatedNames.Clear();
        }

        /// <summary>
        /// Do a cleanup of the massive amount of resources this thing takes up as soon as possible.
        /// </summary>
        public void Cleanup()
        {
            this.currentFiles.Clear();
            this.loadedFiles.Clear();
            this.ppv.Clear();
            this.definedTags.Clear();
            this.scoreboard.values.Clear();
            this.scoreboard.temps.Clear();
            GC.Collect();
        }
    }
}