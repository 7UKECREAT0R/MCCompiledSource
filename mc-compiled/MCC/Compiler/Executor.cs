using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.Modding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public static readonly Regex PPV_FMT = new Regex("\\\\*\\$[\\w\\d]+");
        public const float MCC_VERSION = 1.1f;                  // compilerversion
        public static string MINECRAFT_VERSION = "x.xx.xxx";    // mcversion
        public const string MCC_GENERATED_FOLDER = "compiler";  // folder that generated functions go into
        public const string FAKEPLAYER_NAME = "_";
        public static int MAXIMUM_DEPTH = 100;

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
            Console.WriteLine("<L{0}> {1}", source.Line, warning);
            Console.ForegroundColor = old;
        }

        internal readonly EntityManager entities;
        internal readonly ProjectManager project;
        public string lastStatementSource;

        Statement[] statements;
        int readIndex = 0;
        int unreachableCode = -1;

        internal int depth;
        internal bool linting;
        internal readonly Dictionary<int, object> loadedFiles;
        internal readonly List<int> definedStdFiles;
        internal readonly List<Macro> macros;
        internal readonly List<Function> functions;
        internal readonly HashSet<string> definedTags;
        internal readonly bool[] lastPreprocessorCompare;
        internal readonly ComparisonSet[] lastActualCompare;
        internal readonly Dictionary<string, dynamic[]> ppv;
        readonly StringBuilder prependBuffer;
        readonly Stack<CommandFile> currentFiles;
        readonly Stack<Selector> selections;
        readonly Stack<StructDefinition> definingStructs;
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
            functions = new List<Function>();
            definedTags = new HashSet<string>();
            selections = new Stack<Selector>();

            if (inputPPVs != null && inputPPVs.Length > 0)
                foreach (Program.InputPPV ppv in inputPPVs)
                    SetPPV(ppv.name, new object[] { ppv.value });

            // support up to MAXIMUM_SCOPE levels of scope before blowing up
            lastPreprocessorCompare = new bool[MAXIMUM_DEPTH];
            lastActualCompare = new ComparisonSet[MAXIMUM_DEPTH];

            loadedFiles = new Dictionary<int, object>();
            definingStructs = new Stack<StructDefinition>();
            currentFiles = new Stack<CommandFile>();
            prependBuffer = new StringBuilder();
            scoreboard = new ScoreboardManager(this);

            PushSelector(true);
            SetCompilerPPVs();
            currentFiles.Push(new CommandFile(projectName));
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
        /// Resolve an FString into rawtext terms. Also adds all setup commands for variables.
        /// </summary>
        /// <param name="fstring"></param>
        /// <returns></returns>
        public List<JSONRawTerm> FString(string fstring, out bool advanced)
        {
            // stupid complex search for closing bracket: '}'
            int ScanForCloser(string str, int start)
            {
                int len = str.Length;
                int depth = -1;
                bool skipping = false;
                bool escape = false;

                for(int i = start; i < len; i++)
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

            void DumpTextBuffer()
            {
                string bufferContents = buffer.ToString();
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

                    // check for selector match
                    if (FSTRING_SELECTOR.IsMatch(segment))
                    {
                        i += segmentLength;
                        DumpTextBuffer();
                        advanced = true;
                        terms.Add(new JSONSelector(segment));
                        continue;
                    }
                    else if(FSTRING_VARIABLE.IsMatch(segment))
                    {
                        if(scoreboard.TryGetByAccessor(segment, out ScoreboardValue value, true))
                        {
                            i += segmentLength;
                            DumpTextBuffer();
                            advanced = true;

                            int indexCopy = scoreIndex;
                            AddCommandsClean(value.CommandsRawTextSetup(segment, "@s", ref indexCopy), "string" + value.AliasName);
                            terms.AddRange(value.ToRawText(segment, "@s", ref scoreIndex));
                            scoreIndex++;
                            continue;
                        }
                    }
                }

                buffer.Append(character);
            }

            DumpTextBuffer();
            return terms;
        }
        /// <summary>
        /// Append these terms to the end of this command. Will resolve JSONVariant's and construct the command combinations.
        /// </summary>
        /// <param name="terms">The terms constructed by FString.</param>
        /// <param name="command">The command to append the terms to.</param>
        /// <param name="root">If this is the root call.</param>
        /// <param name="currentSelector">The current selector that holds all the scores checks. Set to null for default behavior.</param>
        /// <param name="commands">Used for recursion, set to null.</param>
        /// <param name="copy">The existing terms to copy from.</param>
        /// <returns></returns>
        public string[] ResolveRawText(List<JSONRawTerm> terms, string command, bool root = true,
            Selector currentSelector = null, List<string> commands = null, RawTextJsonBuilder copy = null)
        {
            RawTextJsonBuilder jb = new RawTextJsonBuilder(copy);

            if (currentSelector == null)
                currentSelector = new Selector(Selector.Core.s);
            if(commands == null)
                commands = new List<string>();

            for(int i = 0; i < terms.Count; i++)
            {
                JSONRawTerm term = terms[i];
                if (term is JSONVariant)
                {
                    // calculate both variants
                    JSONVariant variant = term as JSONVariant;
                    Selector checkA = variant.ConstructSelectorA(currentSelector);
                    Selector checkB = variant.ConstructSelectorB(currentSelector);
                    List<JSONRawTerm> restA = terms.Skip(i + 1).ToList();
                    List<JSONRawTerm> restB = terms.Skip(i + 1).ToList();
                    restA.InsertRange(0, variant.a);
                    restB.InsertRange(0, variant.b);
                    ResolveRawText(restA, command, false, checkA, commands, jb);
                    ResolveRawText(restB, command, false, checkB, commands, jb);
                    break;
                }
                else
                    jb.AddTerm(term);
            }

            bool hasVariant = terms.Any(t => t is JSONVariant);
            if (!root && !hasVariant)
                commands.Add(Command.Execute(currentSelector.ToString(),
                    Coord.here, Coord.here, Coord.here, command + jb.BuildString()));
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
        public JObject LoadJSONFile(string path)
        {
            int hash = path.GetHashCode();

            // cached file, dont read again
            if(loadedFiles.TryGetValue(hash, out object value))
                if (value is JObject)
                    return value as JObject;

            string contents = File.ReadAllText(path);
            JObject json = JObject.Parse(contents);
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
        /// Get or set the active selector.
        /// Get peeks the stack.
        /// Set pushes then pops a new selector to the stack.
        /// </summary>
        public Selector ActiveSelector
        {
            get => selections.Peek();
            set
            {
                selections.Pop();
                selections.Push(value);
            }
        }
        /// <summary>
        /// The currently active selector represented as a string.
        /// </summary>
        public string ActiveSelectorStr
        {
            get => selections.Peek().ToString();
        }
        /// <summary>
        /// The currently active selector core represented as a string.
        /// </summary>
        public string ActiveSelectorCore
        {
            get => '@' + selections.Peek().core.ToString();
        }
        /// <summary>
        /// Push a copy of the current selector to the stack. If doesAlign is set, then the selector is reset to '@s'.
        /// </summary>
        /// <param name="doesAlign"></param>
        public void PushSelector(bool doesAlign)
        {
            if (doesAlign)
                selections.Push(new Selector(Selector.Core.s));
            else
                selections.Push(ActiveSelector);
        }
        /// <summary>
        /// Push a selector to the stack.
        /// </summary>
        /// <param name="now"></param>
        public void PushSelector(Selector now)
        {
            selections.Push(now);
        }
        /// <summary>
        /// Alias for PushSelector(true). Pushes a new selector representing '@s' to the stack and prepends the
        /// necessary execute command so that the command run through it will be aligned to the selected entity(s).
        /// </summary>
        /// <returns>The previous value of the prepend buffer.</returns>
        public string PushSelectorExecute(Selector now)
        {
            if (now.NeedsAlign)
            {
                string prev = prependBuffer.ToString();
                AppendCommandPrepend(Command.Execute(now.ToString(), now.offsetX, now.offsetY, now.offsetZ, ""));
                PushSelector(true);
                return prev;
            }

            PushSelector(false);
            return "";
        }
        /// <summary>
        /// Alias for PushSelector(true). Pushes a new selector representing '@s' to the stack and prepends the
        /// necessary execute command so that the command run through it will be aligned to the selected entity(s).
        /// </summary>
        /// <returns>The previous value of the prepend buffer.</returns>
        public string PushSelectorExecute(Selector now, Coord offsetX, Coord offsetY, Coord offsetZ)
        {
            if (now.NeedsAlign)
            {
                string prev = prependBuffer.ToString();
                AppendCommandPrepend(Command.Execute(now.ToString(), offsetX, offsetY, offsetZ, ""));
                PushSelector(true);
                return prev;
            }

            PushSelector(false);
            return "";
        }
        /// <summary>
        /// Pushes a new selector representing '@s' to the stack and prepends the
        /// necessary execute command so that the command run through it will be aligned to the selected entity(s).
        /// </summary>
        /// <returns>The previous value of the prepend buffer.</returns>
        public string PushSelectorExecute()
        {
            Selector active = ActiveSelector;
            if (active.NeedsAlign)
            {
                string prev = prependBuffer.ToString();
                AppendCommandPrepend(Command.Execute(active.ToString(), active.offsetX, active.offsetY, active.offsetZ, ""));
                PushSelector(true);
                return prev;
            }

            PushSelector(false);
            return "";
        }
        /// <summary>
        /// Pushes a new selector representing '@s' to the stack and prepends the
        /// necessary execute command so that the command run through it will be aligned to the selected entity(s).
        /// 
        /// This variant offsets the position of the execution relative to each entity.
        /// </summary>
        /// <returns>The previous value of the prepend buffer.</returns>
        public string PushSelectorExecute(Coord offsetX, Coord offsetY, Coord offsetZ)
        {
            Selector active = ActiveSelector;
            if (active.NeedsAlign)
            {
                string prev = prependBuffer.ToString();
                AppendCommandPrepend(Command.Execute(active.ToString(), offsetX, offsetY, offsetZ, ""));
                PushSelector(true);
                return prev;
            }

            PushSelector(false);
            return "";
        }
        /// <summary>
        /// Pop a selector off the stack and return to the previous.
        /// </summary>
        public void PopSelector()
        {
            unreachableCode = -1;
            selections.Pop();
        }

        /// <summary>
        /// The number of statements which will run before popSelectorsCount number of selectors are automatically popped.
        /// </summary>
        int popSelectorsAfterNext = 0;
        /// <summary>
        /// Number of times popSelectorsAfterNext will be reset after popping a selector.
        /// </summary>
        int popSelectorsCount = 0;

        /// <summary>
        /// Schedules a selector pop after the next statement is run.
        /// </summary>
        public void PopSelectorAfterNext()
        {
            popSelectorsAfterNext = 2;
            popSelectorsCount++;
        }

        /// <summary>
        /// Returns if this executor has another statement available to run.
        /// </summary>
        public bool HasNext
        {
            get => readIndex < statements.Length;
        }
        /// <summary>
        /// Peek at the next statement.
        /// </summary>
        /// <returns></returns>
        public Statement Peek() => statements[readIndex];
        /// <summary>
        /// Peek at the last statement that was gotten, if any.
        /// </summary>
        /// <returns><b>null</b> if no statements have been gotten yet.</returns>
        public Statement PeekLast()
        {
            if (readIndex > 0)
                return statements[readIndex - 1];

            return null;
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
                scoreboard.PopTempState();

                if (statement is StatementComment)
                    continue; // ignore this statement

                // check for unreachable code due to halt directive
                CheckUnreachable(statement);

                if(Program.DEBUG)
                    Console.WriteLine("EXECUTE LN{0}: {1}", statement.Line, statement.ToString());

                // do the pop-selector stuff.
                if(popSelectorsCount > 0)
                {
                    popSelectorsAfterNext--;

                    if(popSelectorsAfterNext <= 0)
                    {
                        // pop all selectors based on count
                        while (popSelectorsCount-- > 0)
                            PopSelector();
                    }
                }
            }

            while (currentFiles.Count > 0)
                PopFile();
        }
        /// <summary>
        /// Temporarily run another subsection of statements then resume this executor.
        /// </summary>
        public void ExecuteSubsection(Statement[] section)
        {
            scoreboard.PushTempState();
            Statement[] restore0 = statements;
            int restore1 = readIndex;

            statements = section;
            readIndex = 0;
            while (HasNext)
            {
                Statement unresolved = Next();
                Statement statement = unresolved.ClonePrepare(this);
                statement.SetExecutor(this);
                statement.Run0(this);
                scoreboard.PopTempState();

                // check for unreachable code due to halt directive
                CheckUnreachable(statement);

                if (popSelectorsAfterNext >= 0)
                {
                    popSelectorsAfterNext--;
                    if (popSelectorsAfterNext == 0)
                        PopSelector();
                }
            }

            // now its done, so restore state
            scoreboard.PopTempState();
            statements = restore0;
            readIndex = restore1;
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
        /// Set the last if-statement tokens used at this scope.
        /// </summary>
        /// <param name="selector"></param>
        public void SetLastCompare(ComparisonSet set) =>
            lastActualCompare[ScopeLevel] = set;
        /// <summary>
        /// Get the last comparison used at this scope.
        /// </summary>
        /// <returns></returns>
        public ComparisonSet GetLastCompare() =>
            lastActualCompare[ScopeLevel];

        /// <summary>
        /// Add a macro to be looked up later.
        /// </summary>
        /// <param name="macro"></param>
        public void RegisterMacro(Macro macro) =>
            macros.Add(macro);
        /// <summary>
        /// Add a function to be looked up later. Its commands can be written to by simply PushFile()ing to this executor.
        /// </summary>
        /// <param name="function"></param>
        public void RegisterFunction(Function function) =>
            functions.Add(function);
        public Macro? LookupMacro(string name)
        {
            foreach (Macro macro in macros)
                if (macro.Matches(name))
                    return macro;
            return null;
        }
        public Function LookupFunction(string name)
        {
            foreach (Function function in functions)
                if (function.Matches(name))
                    return function;
            return null;
        }
        public bool TryLookupMacro(string name, out Macro? macro)
        {
            macro = LookupMacro(name);
            return macro.HasValue;
        }
        public bool TryLookupFunction(string name, out Function function)
        {
            function = LookupFunction(name);
            return function != null;
        }

        /// <summary>
        /// Get the current file that should be written to.
        /// </summary>
        public CommandFile CurrentFile { get => currentFiles.Peek(); }
        /// <summary>
        /// Get the main .mcfunction file for this project.
        /// </summary>
        public CommandFile HeadFile { get => currentFiles.Last(); }

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
        /// Add a set of files on their own to the list.
        /// </summary>
        /// <param name="file"></param>
        public void AddExtraFiles(IAddonFile[] files) =>
            project.AddFiles(files);
        /// <summary>
        /// Returns if this executor has a file containing a specific string.
        /// </summary>
        /// <param name="text">The text to check for.</param>
        /// <returns></returns>
        public bool HasExtraFileContaining(string text) =>
            this.project.HasFileContaining(text);
        /// <summary>
        /// Add a command to the top of the 'head' file, being the main project function. Does not affect the prepend buffer.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommandHead(string command)
        {
            if (linting)
                return;
            HeadFile.AddTop(command);
        }
        /// <summary>
        /// Adds a set of commands to the top of the 'head' file, being the main project function. Does not affect the prepend buffer.
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommandsHead(IEnumerable<string> commands)
        {
            if (linting)
                return;
            if (commands.Count() < 1)
                return;
            HeadFile.AddTop(commands);
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
        public TokenLiteral[] ResolvePPV(TokenUnresolvedPPV unresolved)
        {
            int line = unresolved.lineNumber;
            string word = unresolved.word;

            if (TryGetPPV(word, out dynamic[] values))
            {
                TokenLiteral[] literals = new TokenLiteral[values.Length];
                for(int i = 0; i < values.Length; i++)
                {
                    dynamic value = values[i];
                    if (value is int)
                        literals[i] = new TokenIntegerLiteral(value, IntMultiplier.none, line);
                    if (value is float)
                        literals[i] = new TokenDecimalLiteral(value, line);
                    if (value is bool)
                        literals[i] = new TokenBooleanLiteral(value, line);
                    if (value is string)
                        literals[i] = new TokenStringLiteral(value, line);
                    if (value is Coord)
                        literals[i] = new TokenCoordinateLiteral(value, line);
                    if (value is Selector)
                        literals[i] = new TokenSelectorLiteral(value, line);
                }
                return literals;
            }

            return null;
        }

        public void PushFile(CommandFile file) =>
            currentFiles.Push(file);
        public void PopFile()
        {
            unreachableCode = -1;

            CommandFile file = currentFiles.Pop();

            // root file is empty so it causes MC compile errors
            // solution: print project info!
            if(currentFiles.Count == 0 && file.Length == 0)
            {
                RawTextJsonBuilder jb = new RawTextJsonBuilder();
                jb.AddTerm(new JSONText(project.name + " for Minecraft " + MINECRAFT_VERSION));
                file.Add(Command.Tellraw("@s", jb.BuildString()));
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
            scoreboard.definedTempVars.Clear();
            scoreboard.values.Clear();
            scoreboard.structs.Clear();
            GC.Collect();
        }
    }
}