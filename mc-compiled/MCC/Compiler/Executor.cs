using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Attributes;
using mc_compiled.Modding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using mc_compiled.Commands.Execute;
using System.Runtime.CompilerServices;
using System.CodeDom.Compiler;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// The final stage of the compilation process. Runs statements and holds state on 
    /// </summary>
    public class Executor
    {
        public const string _FSTRING_SELECTOR = @"@(?:[spaeri]|initiator)(?:\[.+\])?";
        public const string _FSTRING_VARIABLE = @"[\w\d_\-:]+";
        public static readonly Regex FSTRING_SELECTOR = new Regex(_FSTRING_SELECTOR);
        public static readonly Regex FSTRING_VARIABLE = new Regex(_FSTRING_VARIABLE);
        //public static readonly Regex PPV_FMT = new Regex("\\\\*\\$[\\w\\d]+(\\[\"?.+\"?\\])*");
        public static readonly Regex PPV_FMT = new Regex("\\\\*\\$[\\w\\d]+");
        public const double MCC_VERSION = 1.13;                 // compilerversion
        public static string MINECRAFT_VERSION = "x.xx.xxx";    // mcversion
        public const string MCC_GENERATED_FOLDER = "compiler";  // folder that generated functions go into
        public const string UNDOCUMENTED_TEXT = "undocumented";
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

        int iterationsUntilDeferProcess = 0;
        /// <summary>
        /// Defer an action to happen after the next valid statement has run.
        /// </summary>
        /// <param name="action"></param>
        public void DeferAction(Action<Executor> action)
        {
            iterationsUntilDeferProcess += 2;
            this.deferredActions.Push(action);
        }
        /// <summary>
        /// Process all deferred actions currently in the queue.
        /// </summary>
        private void ProcessDeferredActions()
        {
            while(this.deferredActions.Any())
            {
                var action = this.deferredActions.Pop();
                action.Invoke(this);
            }
        }

        internal readonly EntityManager entities;
        internal readonly ProjectManager project;
        public string lastStatementSource;

        Statement[] statements;
        int readIndex = 0;
        int unreachableCode = -1;

        internal int depth;
        internal bool linting;
        internal readonly Stack<Action<Executor>> deferredActions;
        internal readonly Dictionary<int, object> loadedFiles;
        internal readonly List<int> definedStdFiles;
        internal readonly List<Macro> macros;
        internal readonly FunctionManager functions;
        internal readonly HashSet<string> definedTags;
        internal readonly bool[] lastPreprocessorCompare;
        internal readonly PreviousComparisonStructure[] lastCompare;
        internal readonly Dictionary<string, dynamic[]> ppv;
        readonly StringBuilder prependBuffer;
        readonly Stack<CommandFile> currentFiles;
        public readonly ScoreboardManager scoreboard;

        /// <summary>
        /// Get the bracket depth of the executor currently.
        /// </summary>
        public int Depth
        {
            get => depth;
        }

        internal Executor(Statement[] statements, Program.InputPPV[] inputPPVs,
            string projectName, string bpBase, string rpBase)
        {
            this.statements = statements;
            this.project = new ProjectManager(projectName, bpBase, rpBase, this);
            this.entities = new EntityManager(this);

            definedStdFiles = new List<int>();
            ppv = new Dictionary<string, dynamic[]>();
            macros = new List<Macro>();
            definedTags = new HashSet<string>();

            if (inputPPVs != null && inputPPVs.Length > 0)
                foreach (Program.InputPPV ppv in inputPPVs)
                    SetPPV(ppv.name, new object[] { ppv.value });

            // support up to MAXIMUM_SCOPE levels of scope before blowing up
            lastPreprocessorCompare = new bool[MAXIMUM_DEPTH];
            lastCompare = new PreviousComparisonStructure[MAXIMUM_DEPTH];

            deferredActions = new Stack<Action<Executor>>();
            loadedFiles = new Dictionary<int, object>();
            currentFiles = new Stack<CommandFile>();
            prependBuffer = new StringBuilder();
            scoreboard = new ScoreboardManager(this);

            functions = new FunctionManager(scoreboard);
            functions.RegisterDefaultProviders(scoreboard);

            SetCompilerPPVs();

            HeadFile = new CommandFile(projectName).AsRoot();
            currentFiles.Push(HeadFile);
        }
        /// <summary>
        /// Returns this executor after setting it to lint mode, lowering memory usage
        /// </summary>
        /// <returns></returns>
        internal Executor Linter()
        {
            this.linting = true;
            this.project.Linter();
            return this;
        }

        /// <summary>
        /// Pushes to the prepend buffer the proper execute command needed to align to the given selector.
        /// Sets the selector given by the reference parameter to @s.
        /// </summary>
        /// <param name="selector"></param>
        public void PushAlignSelector(ref Selector selector)
        {
            if(selector.NonSelf)
            {
                ExecuteBuilder builder = new ExecuteBuilder()
                    .WithSubcommand(new SubcommandAs(selector))
                    .WithSubcommand(new SubcommandAt(Selector.SELF))
                    .WithSubcommand(new SubcommandRun());

                AppendCommandPrepend(builder.Build(out _));
                selector = Selector.SELF;
            }
        }

        /// <summary>
        /// Resolve an FString into rawtext terms. Also adds all setup commands for variables.
        /// </summary>
        /// <param name="fstring"></param>
        /// <returns></returns>
        public List<JSONRawTerm> FString(string fstring, int[] lineNumbers, out bool advanced)
        {
            // stupid complex search for closing bracket: '}'
            int ScanForCloser(string str, int start)
            {
                int len = str.Length;
                int depth = -1;
                bool skipping = false;
                bool escape = false;

                for(int i = start + 1; i < len; i++)
                {
                    char c = str[i];

                    // skipping due to string
                    if(skipping)
                    {
                        // escaping chars
                        if (escape)
                        {
                            escape = false;
                            continue;
                        }
                        if (c == '\\')
                        {
                            escape = true;
                            continue;
                        }

                        if (c == '"')
                            skipping = false;
                        continue;
                    }

                    // actual bracket stuff
                    if (c == '{')
                    {
                        depth++;
                        continue;
                    }
                    if(c == '}')
                    {
                        depth--;
                        if (depth < 0)
                            return i - start;
                    }
                }

                return -1;
            }

            advanced = false;
            List<JSONRawTerm> terms = new List<JSONRawTerm>();
            StringBuilder buffer = new StringBuilder();

            // dumps the contents of the text buffer into a string and adds it to the terms as text.
            void DumpTextBuffer()
            {
                string bufferContents = ResolveString(buffer.ToString());
                if (!string.IsNullOrEmpty(bufferContents))
                {
                    terms.Add(new JSONText(bufferContents));
                    buffer.Clear();
                }
            }

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
                    Tokenizer tokenizer = new Tokenizer(segment, false, false);
                    Token[] tokens = tokenizer.Tokenize();
                    Statement subStatement = new StatementHusk(tokens);
                    subStatement.SetSource(lineNumbers, "Inline FString Operation");

                    // squash & resolve the tokens as best it can
                    subStatement.PrepareThis(this);

                    // dump text buffer before we get started.
                    i += segmentLength;
                    DumpTextBuffer();

                    // get the rawtext terms
                    Token[] remaining = subStatement.GetRemainingTokens();
                    foreach (Token token in remaining)
                    {
                        // try to find the best rawtext representation
                        if(token is TokenSelectorLiteral selectorLiteral)
                        {
                            advanced = true;
                            Selector selector = selectorLiteral.selector;
                            terms.Add(new JSONSelector(selector.ToString()));
                            continue;
                        } else if(token is TokenIdentifierValue identifierValue)
                        {
                            advanced = true;
                            ScoreboardValue value = identifierValue.value;
                            int indexCopy = scoreIndex;
                            AddCommandsClean(value.CommandsRawTextSetup(ref indexCopy), "string" + value.Name);
                            terms.AddRange(value.ToRawText(ref indexCopy));
                            scoreIndex++;
                            continue;
                        }

                        // default representation
                        string stringRepresentation = ResolveString(token.ToString());
                        terms.Add(new JSONText(stringRepresentation));
                        continue;
                    }

                    continue;
                }

                buffer.Append(character);
            }

            DumpTextBuffer();
            return terms;
        }
        /// <summary>
        /// Append these terms to the end of this command. Will resolve <see cref="JSONVariant"/>s and construct the command combinations.
        /// </summary>
        /// <param name="terms">The terms constructed by FString.</param>
        /// <param name="command">The command to append the terms to.</param>
        /// <param name="root">If this is the root call.</param>
        /// <param name="currentSelector">The current selector that holds all the scores checks. Set to null for default behavior.</param>
        /// <param name="commands">Used for recursion, set to null.</param>
        /// <param name="copy">The existing terms to copy from.</param>
        /// <returns></returns>
        public string[] ResolveRawText(List<JSONRawTerm> terms, string command, bool root = true,
            ExecuteBuilder builder = null, List<string> commands = null, RawTextJsonBuilder copy = null)
        {
            RawTextJsonBuilder jb = new RawTextJsonBuilder(copy);

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
                        rest.Insert(0, possibleVariant.term);
                        ResolveRawText(rest, command, false, branch, commands, jb);
                    }
                    break;
                }
                else
                    jb.AddTerm(term);
            }

            bool hasVariant = terms.Any(t => t is JSONVariant);
            if (!root && !hasVariant)
                commands.Add(builder.Run(command + jb.BuildString()));
            else if(root && !hasVariant)
                commands.Add(command + jb.BuildString());

            if (root)
                return commands.ToArray();

            return null; // return value isn't used in this case
        }
        public void UnreachableCode() =>
            unreachableCode = 1;
        /// <summary>
        /// Throw a StatementException if a feature is not enabled.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="feature"></param>
        internal void RequireFeature(Statement source, Feature feature)
        {
            if (project.HasFeature(feature))
                return;

            string name = feature.ToString();
            throw new StatementException(source, $"Feature not enabled: {name}. Enable using the command 'feature {name.ToLower()}' at the top of the file.");
        }
        /// <summary>
        /// Checks if the execution context is currently in an unreachable area, and throw an exception if true.
        /// </summary>
        /// <param name="current">The statement that is currently being run.</param>
        /// <exception cref="StatementException">If the execution context is currently in an unreachable area.</exception>
        void CheckUnreachable(Statement current)
        {
            if (unreachableCode > 0)
                unreachableCode--;
            else if (unreachableCode == 0)
                throw new StatementException(current, "Unreachable code detected.");
            else
                unreachableCode = -1;
        }

        /// <summary>
        /// Load JSON file with caching for next use.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public JToken LoadJSONFile(string path, Statement callingStatement)
        {
            int hash = path.GetHashCode();

            // cached file, dont read again
            if (loadedFiles.TryGetValue(hash, out object value))
            {
                if (value is JToken)
                    return value as JToken;
            }

            if(string.IsNullOrWhiteSpace(path))
                throw new StatementException(callingStatement, "Empty JSON file path.");

            if(!File.Exists(path))
                throw new StatementException(callingStatement, "File \'" + path + "\' could not be found. Make sure you are in the right working directory.");

            string contents = File.ReadAllText(path);
            JToken json = JToken.Parse(contents);
            loadedFiles[hash] = json;
            return json;
        }
        /// <summary>
        /// Load JSON file with caching for next use, under a different hashcode.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="underName"></param>
        /// <returns></returns>
        public JToken LoadJSONFile(string path, int hash, Statement callingStatement)
        {
            // cached file, dont read again
            if (loadedFiles.TryGetValue(hash, out object value))
            {
                if (value is JToken)
                    return value as JToken;
            }

            if (string.IsNullOrWhiteSpace(path))
                throw new StatementException(callingStatement, "Empty JSON file path.");

            if (!File.Exists(path))
                throw new StatementException(callingStatement, "File \'" + path + "\' could not be found. Make sure you are in the right working directory.");

            string contents = File.ReadAllText(path);
            JToken json = JToken.Parse(contents);
            loadedFiles[hash] = json;
            return json;
        }
        /// <summary>
        /// Load file with caching for next use.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string LoadFileString(string path)
        {
            int hash = path.GetHashCode();

            // cached file, dont read again
            if (loadedFiles.TryGetValue(hash, out object value))
                if (value is string)
                    return value as string;

            string contents = File.ReadAllText(path);
            loadedFiles[hash] = contents;
            return contents;
        }
        /// <summary>
        /// Load file with caching for next use.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public byte[] LoadFileBytes(string path)
        {
            int hash = path.GetHashCode();

            // cached file, dont read again
            if (loadedFiles.TryGetValue(hash, out object value))
            {
                if (value is string)
                    return Encoding.UTF8.GetBytes(value as string);
                else if (value is byte[])
                    return value as byte[];
            }

            byte[] contents = File.ReadAllBytes(path);
            loadedFiles[hash] = contents;
            return contents;
        }

        /// <summary>
        /// Attempts to fetch an addon file from the current BP, or downloads it from the default pack if not present. Uses caching.
        /// </summary>
        public JToken Fetch(IAddonFile toLocate, Statement callingStatement)
        {
            string outputFile = this.project.GetOutputFileLocationFull(toLocate, true);

            int outputFileHash = outputFile.GetHashCode();

            // the JSON root of the file
            JToken root;

            // find the user-defined entity file, or default to the vanilla pack provided by Microsoft
            if (this.loadedFiles.TryGetValue(outputFileHash, out object jValue))
                root = jValue as JToken;
            else
            {
                if (System.IO.File.Exists(outputFile))
                    root = this.LoadJSONFile(outputFile, callingStatement);
                else
                {
                    string pathString = toLocate.GetOutputLocation().ToString();
                    DefaultPackManager.PackType packType = DefaultPackManager.PackType.BehaviorPack;

                    if(pathString.StartsWith("r_"))
                    {
                        packType = DefaultPackManager.PackType.ResourcePack;
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

                    string downloadedFile = DefaultPackManager.Get(packType, filePath);
                    root = this.LoadJSONFile(downloadedFile, outputFileHash, null);
                }
            }

            return root;
        }

        /// <summary>
        /// Define a file that sort-of equates to a "standard library." Will only be added once.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        public void DefineSTDFile(CommandFile file)
        {
            if (definedStdFiles.Contains(file.GetHashCode()))
                return;
            definedStdFiles.Add(file.GetHashCode());
            AddExtraFile(file);
        }
        public bool HasSTDFile(CommandFile file)
        {
            return definedStdFiles.Contains(file.GetHashCode());
        }

        /// <summary>
        /// Returns if this executor has another statement available to run.
        /// </summary>
        public bool HasNext
        {
            get => readIndex < statements.Length;
        }

        /// <summary>
        /// Tries to fetch a documentation string based whether the last statement was a comment or not. Returns <see cref="Executor.UNDOCUMENTED_TEXT"/> if no documentation was supplied.
        /// </summary>
        /// <returns></returns>
        public string GetDocumentationString()
        {
            if (readIndex < 1)
                return UNDOCUMENTED_TEXT;

            Statement last = PeekLast();
            if(last is StatementComment comment)
                return comment.comment;

            return UNDOCUMENTED_TEXT;
        }
        /// <summary>
        /// Peek at the next statement.
        /// </summary>
        /// <returns></returns>
        public Statement Peek() => statements[readIndex];
        /// <summary>
        /// Peek at the statement N statements in front of the read index. 0: current, 1: next, etc...
        /// Does perform bounds checking, and returns null if outside bounds.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Statement PeekSkip(int amount)
        {
            int index = readIndex + amount;

            if (index < 0)
                return null;
            else if (index < statements.Length)
                return statements[index];
            else
                return null;
        }
        /// <summary>
        /// Peek at the last statement that was gotten, if any.
        /// </summary>
        /// <returns><b>null</b> if no statements have been gotten yet.</returns>
        public Statement PeekLast()
        {
            if (readIndex > 1)
                return statements[readIndex - 2];

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

            int i = readIndex + amount;

            if (i < 0 || i >= statements.Length)
                return null;

            do
            {
                statement = statements[i++];
            } while ((statement == null || statement.Skip) && i < statements.Length);

            return statement;
        }
        /// <summary>
        /// Seek backwards from the current statement until it finds one with valid executable data. Returns null if outside of bounds.
        /// </summary>
        /// <returns><b>null</b> if no statements have been gotten yet.</returns>
        public Statement SeekLast()
        {
            Statement statement = null;

            int i = readIndex - 1;

            if(i < 0)
                return null; // no statements??

            do
            {
                statement = statements[i--];
            } while ((statement == null || statement.Skip) && i >= 0);

            return statement;
        }

        /// <summary>
        /// Get the next statement to be read and then increment the read index.
        /// </summary>
        /// <returns></returns>
        public Statement Next() => statements[readIndex++];
        /// <summary>
        /// Returns the next statement to be read as a certain type. Increments the read index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Next<T>() where T : Statement => statements[readIndex++] as T;
        /// <summary>
        /// Peek at the next statement as a certain type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Peek<T>() where T : Statement => statements[readIndex] as T;
        /// <summary>
        /// Peek a certain number of statements into the future as a certain type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skip"></param>
        /// <returns></returns>
        public T Peek<T>(int skip) where T : Statement => statements[readIndex + skip] as T;
        /// <summary>
        /// Returns if there's another statement available and it's of a certain type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool NextIs<T>() where T : Statement => HasNext && statements[readIndex] is T;
        /// <summary>
        /// Returns if the next statement is an unknown statement with builder field(s) in it.
        /// </summary>
        /// <returns></returns>
        public bool NextIsBuilder()
        {
            if (!HasNext)
                return false;

            Statement _tokens = statements[readIndex];

            if (!(_tokens is StatementUnknown))
                return false;

            StatementUnknown tokens = _tokens as StatementUnknown;
            return tokens.NextIs<TokenBuilderIdentifier>();
        }
        public bool NextBuilderField(ref Statement tokens, out TokenBuilderIdentifier builderField)
        {
            // next in statement?
            if (tokens.NextIs<TokenBuilderIdentifier>())
            {
                builderField = tokens.Next<TokenBuilderIdentifier>();
                return true;
            }

            // not in the current statement ... look ahead in code
            if(NextIsBuilder())
            {
                // reassigns the field in the caller's code
                tokens = Next<StatementUnknown>();
                builderField = tokens.Next<TokenBuilderIdentifier>();
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
                return new Statement[0];

            Statement[] ret = new Statement[amount];

            int write = 0;
            for (int i = readIndex; i < statements.Length && i < readIndex + amount; i++)
                ret[write++] = statements[i];

            return ret;
        }
        /// <summary>
        /// Reads the next statement, or set of statements if it is a block. Similar to Next() in that it updates the readIndex.
        /// </summary>
        /// <returns></returns>
        public Statement[] NextExecutionSet()
        {
            Statement current = statements[readIndex - 1];

            if (!HasNext)
                throw new StatementException(current, "Unexpected end-of-file while expecting statement/block.");

            if(NextIs<StatementOpenBlock>())
            {
                StatementOpenBlock block = Next<StatementOpenBlock>();
                int statements = block.statementsInside;
                Statement[] code = Peek(statements);
                readIndex += statements;
                readIndex++; // block closer
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
            string ret = prependBuffer.ToString();
            prependBuffer.Clear();
            return ret;
        }

        /// <summary>
        /// Setup the default preprocessor variables.
        /// </summary>
        void SetCompilerPPVs()
        {
            ppv["minecraftversion"] = new dynamic[] { MINECRAFT_VERSION };
            ppv["compilerversion"] = new dynamic[] { MCC_VERSION };
            ppv["_realtime"] = new dynamic[] { DateTime.Now.ToShortTimeString() };
            ppv["_realdate"] = new dynamic[] { DateTime.Now.ToShortDateString() };
            ppv["_timeformat"] = new dynamic[] { TimeFormat.Default.ToString() };
            ppv["_true"] = new dynamic[] { "true" };
            ppv["_false"] = new dynamic[] { "false" };
        }
        /// <summary>
        /// Run this executor start to finish.
        /// </summary>
        public void Execute()
        {
            readIndex = 0;

            while (HasNext)
            {
                Statement unresolved = Next();
                Statement statement = unresolved.ClonePrepare(this);
                statement.SetExecutor(this);
                statement.Run0(this);

                if (statement.Skip)
                    continue; // ignore this statement
                
                // check for unreachable code due to halt directive
                CheckUnreachable(statement);

                // run deferred processes
                if(iterationsUntilDeferProcess > 0)
                {
                    iterationsUntilDeferProcess--;
                    if (iterationsUntilDeferProcess == 0)
                        ProcessDeferredActions();
                }

                if(Program.DEBUG)
                    Console.WriteLine("EXECUTE LN{0}: {1}", statement.Lines[0], statement.ToString());
            }

            while (currentFiles.Any())
                PopFile();
        }
        /// <summary>
        /// Temporarily run another subsection of statements then resume this executor.
        /// </summary>
        public void ExecuteSubsection(Statement[] section)
        {
            using (scoreboard.temps.PushTempState())
            {
                Statement[] restore0 = statements;
                int restore1 = readIndex;

                statements = section;
                readIndex = 0;
                while (HasNext)
                {
                    Statement unresolved = Next();
                    Statement statement = unresolved.ClonePrepare(this);
                    statement.Run0(this);

                    if (statement.Skip)
                        continue; // ignore this statement

                    // check for unreachable code due to halt directive
                    CheckUnreachable(statement);

                    // run deferred processes
                    if (iterationsUntilDeferProcess > 0)
                    {
                        iterationsUntilDeferProcess--;
                        if (iterationsUntilDeferProcess == 0)
                            ProcessDeferredActions();
                    }

                    if (Program.DEBUG)
                        Console.WriteLine("EXECUTE SUBSECTION LN{0}: {1}", statement.Lines[0], statement.ToString());
                }

                // now its done, so restore state
                statements = restore0;
                readIndex = restore1;
            }
        }

        /// <summary>
        /// Set the result of the last preprocessor-if comparison in this scope.
        /// </summary>
        /// <param name="value"></param>
        public void SetLastIfResult(bool value) => lastPreprocessorCompare[ScopeLevel] = value;
        /// <summary>
        /// Get the result of the last preprocessor-if comparison in this scope.
        /// </summary>
        /// <returns></returns>
        public bool GetLastIfResult() => lastPreprocessorCompare[ScopeLevel];

        /// <summary>
        /// Set the last comparison data used at the given/current scope level.
        /// </summary>
        /// <param name="selector"></param>
        internal void SetLastCompare(PreviousComparisonStructure set, int? scope = null)
        {
            if (scope == null)
                scope = ScopeLevel;

            lastCompare[scope.Value] = set;
        }
        /// <summary>
        /// Get the last comparison data used at the given/current scope level.
        /// </summary>
        /// <returns></returns>
        internal PreviousComparisonStructure GetLastCompare(int? scope = null)
        {
            if (scope == null)
                scope = ScopeLevel;

            return lastCompare[scope.Value];
        }

        /// <summary>
        /// Register a macro to be looked up later.
        /// </summary>
        /// <param name="macro">The macro to register.</param>
        public void RegisterMacro(Macro macro) =>
            macros.Add(macro);
        /// <summary>
        /// Look for a macro present in this project.
        /// </summary>
        /// <param name="name">The name used to look up a macro.</param>
        /// <returns>A nullable <see cref="Macro"/> which contains the found macro, if any.</returns>
        public Macro? LookupMacro(string name)
        {
            foreach (Macro macro in macros)
                if (macro.Matches(name))
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
        public CommandFile CurrentFile { get => currentFiles.Peek(); }
        /// <summary>
        /// Get the main .mcfunction file for this project.
        /// </summary>
        public CommandFile HeadFile { get; private set; }

        /// <summary>
        /// Get the current scope level.
        /// </summary>
        public int ScopeLevel { get => currentFiles.Count - 1; }
        /// <summary>
        /// Get if the base file (projectName.mcfunction) is the active file.
        /// </summary>
        public bool IsScopeBase { get => currentFiles.Count <= 1; }

        /// <summary>
        /// Add a command to the current file, with prepend buffer.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(string command)
        {
            if (linting)
            {
                PopPrepend();
                return;
            }
            CurrentFile.Add(PopPrepend() + command);
        }
        /// <summary>
        /// Add a set of commands into a new branching file unless inline is set.
        /// </summary>
        /// <param name="friendlyName">The friendly name to give the generated file, if any.</param>
        /// <param name="inline">Force the commands to be inlined rather than sent to a generated file.</param>
        /// <param name="commands"></param>
        public void AddCommands(IEnumerable<string> commands, string friendlyName, bool inline = false)
        {
            if (linting)
                return;
            int count = commands.Count();
            if (count < 1)
                return;

            if (inline)
            {
                string buffer = PopPrepend();
                CurrentFile.Add(from c in commands select buffer + c);
                return;
            }

            if (count == 1)
            {
                AddCommand(commands.First());
                return;
            }

            CommandFile file = Executor.GetNextGeneratedFile(friendlyName);
            file.Add(commands);

            AddExtraFile(file);
            AddCommand(Command.Function(file));
        }
        /// <summary>
        /// Add a command to the current file, not modifying the prepend buffer.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommandClean(string command)
        {
            if (linting)
                return;
            string prepend = prependBuffer.ToString();
            CurrentFile.Add(prepend + command);
        }
        /// <summary>
        /// Add a set of commands into a new branching file, not modifying the prepend buffer.
        /// If inline is set, no branching file will be made.
        /// </summary>
        /// <param name="friendlyName">The friendly name to give the generated file, if any.</param>
        /// <param name="inline">Force the commands to be inlined rather than sent to a generated file.</param>
        /// <param name="commands"></param>
        public void AddCommandsClean(IEnumerable<string> commands, string friendlyName, bool inline = false)
        {
            if (linting)
                return;
            string buffer = prependBuffer.ToString();

            if (inline)
            {
                CurrentFile.Add(commands.Select(c => buffer + c));
                return;
            }

            int count = commands.Count();
            if (count < 1)
                return;
            if (count == 1)
            {
                AddCommandClean(commands.First());
                return;
            }

            CommandFile file = Executor.GetNextGeneratedFile(friendlyName);
            file.Add(commands);

            AddExtraFile(file);
            CurrentFile.Add(buffer + Command.Function(file));
        }
        /// <summary>
        /// Add a file on its own to the list.
        /// </summary>
        /// <param name="file"></param>
        public void AddExtraFile(IAddonFile file) =>
            project.AddFile(file);
        /// <summary>
        /// Add a file to the list, removing any other file that has a matching name/directory.
        /// </summary>
        /// <param name="file"></param>
        public void OverwriteExtraFile(IAddonFile file)
        {
            project.RemoveDuplicatesOf(file);
            project.AddFile(file);
        }
        /// <summary>
        /// Add a set of files on their own to the list.
        /// </summary>
        /// <param name="file"></param>
        public void AddExtraFiles(IAddonFile[] files) =>
            project.AddFiles(files);
        /// <summary>
        /// Add a set of files on their own to the list, removing any other files that have a matching name/directory.
        /// </summary>
        /// <param name="file"></param>
        public void OverwriteExtraFiles(IAddonFile[] files)
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

        private List<string> initCommands = new List<string>();
        /// <summary>
        /// Add a command to the 'init' file. Does not affect the prepend buffer.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommandInit(string command)
        {
            if (linting)
                return;
            initCommands.Add(command);
        }
        /// <summary>
        /// Adds a set of commands to the 'init' file. Does not affect the prepend buffer.
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommandsInit(IEnumerable<string> commands)
        {
            if (linting)
                return;
            if (commands.Count() < 1)
                return;
            initCommands.AddRange(commands);
        }

        /// <summary>
        /// Set the content that will prepend the next added command.
        /// </summary>
        /// <param name="content"></param>
        /// <returns>The old buffer's contents.</returns>
        public string SetCommandPrepend(string content)
        {
            string oldContent = prependBuffer.ToString();
            prependBuffer.Clear().Append(content);

            if (string.IsNullOrEmpty(oldContent))
                return "";

            return oldContent;
        }
        /// <summary>
        /// Append to the content to the prepend buffer.
        /// </summary>
        /// <param name="content"></param>
        public void AppendCommandPrepend(string content) =>
            prependBuffer.Append(content);
        /// <summary>
        /// Prepend content to the prepend buffer.
        /// </summary>
        /// <param name="content"></param>
        public void PrependCommandPrepend(string content) =>
            prependBuffer.Insert(0, content);

        /// <summary>
        /// Try to get a preprocessor variable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetPPV(string name, out dynamic[] value)
        {
            if (name.StartsWith("$"))
                name = name.Substring(1);
            return ppv.TryGetValue(name, out value);
        }
        /// <summary>
        /// Set or create a preprocessor variable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        public void SetPPV(string name, object[] values) =>
            ppv[name] = values;
        /// <summary>
        /// Get the names of all registered preprocessor variables.
        /// </summary>
        public string[] PPVNames
        {
            get => ppv.Select(p => p.Key).ToArray();
        }

        /// <summary>
        /// Resolve all unescaped preprocessor variables in a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string ResolveString(string str)
        {
            if (ppv.Count < 1)
                return str;

            StringBuilder sb = new StringBuilder(str);
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

                // If there are an odd number of preceeding backslashes, this is escaped.
                if (backslashes % 2 == 1)
                {
                    sb.Remove(match.Index + lastIndex, 1);
                    continue;
                }

                string ppvName = text.Substring(lastIndex + 1);
                if (!TryGetPPV(ppvName, out dynamic[] values))
                    continue; // no ppv named that

                string insertText;
                if (values.Length > 1)
                    insertText = string.Join(" ", values);
                else
                    insertText = values[0].ToString();

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
        public TokenLiteral[] ResolvePPV(TokenUnresolvedPPV unresolved, Statement thrower)
        {
            int line = unresolved.lineNumber;
            string word = unresolved.word;

            if (TryGetPPV(word, out dynamic[] values))
            {
                TokenLiteral[] literals = new TokenLiteral[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    dynamic value = values[i];
                    TokenLiteral wrapped = PreprocessorUtils.DynamicToLiteral(value, line);

                    if (wrapped == null)
                        throw new StatementException(thrower, $"Found unexpected value in PPV '{word}': {value.ToString()}");

                    literals[i] = wrapped;
                }
                return literals;
            }

            throw new StatementException(thrower, $"Unknown preprocessor variable '{word}'.");
        }
        /// <summary>
        /// Resolves a PPV using an indexer rather than a general expansion.
        /// </summary>
        /// <param name="unresolved">The unresolved PPV to index from.</param>
        /// <param name="indexer">The indexer to index with.</param>
        /// <param name="thrower">The statement that would be the cause of the error, if any.</param>
        /// <returns>The resolved token from the indexer.</returns>
        /// <exception cref="StatementException"></exception>
        public Token ResolvePPVIndex(TokenUnresolvedPPV unresolved, TokenIndexer indexer, Statement thrower)
        {
            int line = unresolved.lineNumber;
            string word = unresolved.word;

            if(TryGetPPV(word, out dynamic[] values))
            {
                int length = values.Length;
                if(length > 1)
                {
                    // simply pull from the index.
                    if (indexer is TokenIndexerInteger integer)
                    {
                        int index = integer.token.number;
                        if (index >= length || index < 0)
                            throw integer.GetIndexOutOfBounds(0, length - 1, thrower);

                        dynamic indexedDynamic = values[index];
                        TokenLiteral indexedLiteral = PreprocessorUtils.DynamicToLiteral(indexedDynamic, line);

                        if (indexedLiteral == null)
                            throw new StatementException(thrower, "Preprocessor variable's indexed data couldn't be wrapped: " + indexedDynamic.ToString());

                        return indexedLiteral;
                    }
                    else
                        throw indexer.GetException(unresolved, thrower);
                }

                // pull first (single) value.
                dynamic singleDynamic = values[0];
                TokenLiteral singleLiteral = PreprocessorUtils.DynamicToLiteral(singleDynamic, line);

                if(singleLiteral == null)
                    throw new StatementException(thrower, "Preprocessor variable's indexed data couldn't be wrapped: " + singleDynamic.ToString());

                // check that it's actually indexable.
                if(!(singleLiteral is IIndexable singleIndexable))
                    throw new StatementException(thrower, "Couldn't index token: " + singleLiteral.ToString());

                // index it!
                Token final = singleIndexable.Index(indexer, thrower);
                return final;
            }

            throw new StatementException(thrower, $"Unknown preprocessor variable '{word}'.");
        }

        public void PushFile(CommandFile file) =>
            currentFiles.Push(file);
        public void PopFile()
        {
            unreachableCode = -1;

            CommandFile file = currentFiles.Pop();

            // file is empty so it causes MC compile errors
            // solution: print project info!
            if(currentFiles.Count == 0 && file.Length == 0)
            {
                RawTextJsonBuilder jb = new RawTextJsonBuilder();
                jb.AddTerm(new JSONText(project.name + " for Minecraft " + MINECRAFT_VERSION));
                file.Add(Command.Tellraw("@s", jb.BuildString()));
            }

            if(file.IsRootFile && initCommands.Any())
            {
                // add initialization commands now.
                file.AddTop(initCommands);

                if (Program.DECORATE)
                    file.AddTop("# Initialize the project.");
            }

            project.AddFile(file);
        }

        private static Dictionary<int, int> branchFileIndexes = new Dictionary<int, int>();
        /// <summary>
        /// Construct the next available command file with this name, like input0, input1, input2, etc...
        /// </summary>
        /// <param name="friendlyName">A user-friendly name to mark the file by.</param>
        /// <returns></returns>
        public static CommandFile GetNextGeneratedFile(string friendlyName)
        {
            int hash = friendlyName.GetHashCode();
            if (!branchFileIndexes.TryGetValue(hash, out int index))
                index = 0;

            branchFileIndexes[hash] = index + 1;
            return new CommandFile(friendlyName + index, MCC_GENERATED_FOLDER);
        }
        public static void ResetGeneratedFiles()
        {
            branchFileIndexes.Clear();
        }

        /// <summary>
        /// Do a cleanup of the massive amount of resources this thing takes up as soon as possible.
        /// </summary>
        public void Cleanup()
        {
            currentFiles.Clear();
            loadedFiles.Clear();
            ppv.Clear();
            definedTags.Clear();
            scoreboard.values.Clear();
            scoreboard.temps.Clear();
            GC.Collect();
        }
    }
}