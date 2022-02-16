using mc_compiled.Commands;
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
        public const string FSTRING_REGEX = "({([a-zA-Z0-9-:._]{1,16})})|({(@[psea](\\[.+\\])?)})";
        public static readonly Regex FSTRING_FMT = new Regex(FSTRING_REGEX);
        public static readonly Regex FSTRING_FMT_SPLIT = new Regex(FSTRING_REGEX, RegexOptions.ExplicitCapture);

        public readonly string projectName;
        public string lastStatementSource;

        Statement[] statements;
        int readIndex = 0;
        int unreachableCode = -1;

        readonly List<int> definedStdFiles;
        readonly List<Macro> macros;
        readonly List<Function> functions;
        readonly bool[] lastPreprocessorCompare;
        readonly Token[][] lastActualCompare;
        readonly Dictionary<string, dynamic> ppv;
        readonly List<IBehaviorFile> filesToWrite;
        readonly StringBuilder prependBuffer;
        readonly Stack<CommandFile> currentFiles;
        readonly Stack<Selector> selections;
        readonly Stack<StructDefinition> definingStructs;

        public readonly ScoreboardManager scoreboard;
        /// <summary>
        /// Resolve an FString into rawtext terms. Also adds all setup commands for variables.
        /// </summary>
        /// <param name="fstring"></param>
        /// <returns></returns>
        public List<JSONRawTerm> FString(string fstring)
        {
            MatchCollection matches = FSTRING_FMT.Matches(fstring);
            if (matches.Count < 1)
                return new List<JSONRawTerm>() { new JSONText(fstring) };

            List<JSONRawTerm> terms = new List<JSONRawTerm>();
            IEnumerable<string> piecesReversed = FSTRING_FMT_SPLIT.Split(fstring).Reverse();
            Stack<string> pieces = new Stack<string>(piecesReversed);

            int index = 0;
            string sel = ActiveSelectorStr;
            foreach (Match match in matches)
            {
                int mindex = match.Index;
                if (mindex != 0 && pieces.Count > 0)
                    terms.Add(new JSONText(pieces.Pop()));
                else
                    pieces.Pop();

                string src = match.Value;
                string varAccessor = match.Groups[2].Value;
                string selector = match.Groups[4].Value.Trim('{', '}');

                if (!string.IsNullOrEmpty(varAccessor))
                {
                    if (scoreboard.TryGetByAccessor(varAccessor, out ScoreboardValue value))
                    {
                        AddCommandsClean(value.CommandsRawTextSetup(varAccessor, sel, ref index));
                        terms.AddRange(value.ToRawText(varAccessor, sel, ref index));
                        index++;
                    }
                    else
                        terms.Add(new JSONText(src));
                }
                else if(!string.IsNullOrEmpty(selector))
                    terms.Add(new JSONSelector(selector));
                else
                    terms.Add(new JSONText(src));
            }

            while (pieces.Count > 0)
                terms.Add(new JSONText(pieces.Pop()));

            return terms;
        }
        public void UnreachableCode() =>
            unreachableCode = 1;
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

        public void BeginDefiningStruct(StructDefinition definition) =>
            definingStructs.Push(definition);
        public void EndDefiningStruct() =>
            scoreboard.DefineStruct(definingStructs.Pop());
        public bool IsDefiningStruct
        {
            get => definingStructs.Count > 0;
        }
        public StructDefinition DefiningStruct
        {
            get => definingStructs.Peek();
        }


        /// <summary>
        /// Get the active selector.
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
        /// Push a copy of the current selector to the stack. If doesAlign is set, then the selector is reset to '@s'.
        /// </summary>
        /// <param name="doesAlign"></param>
        public void PushSelector(bool doesAlign)
        {
            if (doesAlign)
                selections.Push(new Selector() { core = Selector.Core.s });
            else
                selections.Push(ActiveSelector);
        }
        /// <summary>
        /// Alias for PushSelector(true). Pushes a new selector representing '@s' to the stack and prepends the
        /// necessary execute command so that the command run through it will be aligned to the selected entity(s).
        /// </summary>
        public void PushSelectorExecute()
        {
            Selector active = ActiveSelector;
            if (active.NeedsAlign)
            {
                AppendCommandPrepend(Command.Execute(active.ToString(), Coord.here, Coord.here, Coord.here, ""));
                PushSelector(true);
                return;
            }

            PushSelector(false);
        }
        /// <summary>
        /// Alias for PushSelector(true). Pushes a new selector representing '@s' to the stack and prepends the
        /// necessary execute command so that the command run through it will be aligned to the selected entity(s).
        /// 
        /// This variant offsets the position of the execution relative to each entity.
        /// </summary>
        public void PushSelectorExecute(Coord offsetX, Coord offsetY, Coord offsetZ)
        {
            Selector active = ActiveSelector;
            if (active.NeedsAlign)
            {
                AppendCommandPrepend(Command.Execute(active.ToString(), offsetX, offsetY, offsetZ, ""));
                PushSelector(true);
                return;
            }

            PushSelector(false);
        }
        /// <summary>
        /// Pop a selector off the stack and return to the previous.
        /// </summary>
        public void PopSelector()
        {
            unreachableCode = -1;
            selections.Pop();
        }


        int popSelectorsAfterNext = 0;
        /// <summary>
        /// Schedules a selector pop after the next statement is run.
        /// </summary>
        public void PopSelectorAfterNext()
        {
            popSelectorsAfterNext = 2;
        }

        public bool HasNext
        {
            get => readIndex < statements.Length;
        }
        public Statement Peek() => statements[readIndex];
        public Statement Next() => statements[readIndex++];
        public T Next<T>() where T : Statement => statements[readIndex++] as T;
        public T Peek<T>() where T : Statement => statements[readIndex] as T;
        public T Peek<T>(int skip) where T : Statement => statements[readIndex + skip] as T;
        public bool NextIs<T>() where T : Statement => statements[readIndex] is T;
        /// <summary>
        /// Return an array of the next x statements.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Statement[] Peek(int amount)
        {
            Statement[] ret = new Statement[amount];

            int write = 0;
            for (int i = readIndex; i < statements.Length && i < readIndex + amount; i++)
                ret[write++] = statements[i];

            return ret;
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

        public Executor(Statement[] statements, string projectName)
        {
            this.statements = statements;
            this.projectName = projectName;

            definedStdFiles = new List<int>();
            ppv = new Dictionary<string, dynamic>();
            macros = new List<Macro>();
            functions = new List<Function>();
            selections = new Stack<Selector>();

            // support up to 100 levels of scope before blowing up
            lastPreprocessorCompare = new bool[100];
            lastActualCompare = new Token[100][];

            definingStructs = new Stack<StructDefinition>();
            currentFiles = new Stack<CommandFile>();
            filesToWrite = new List<IBehaviorFile>();
            prependBuffer = new StringBuilder();
            scoreboard = new ScoreboardManager(this);

            // extract guids from existing manifest
            if (!Program.BASIC_OUTPUT)
            {
                Guid uuid1, uuid2;
                string manifestPath = Path.Combine(projectName, "manifest.json");

                if (File.Exists(manifestPath))
                {
                    Console.WriteLine("Reading GUIDs from existing manifest file.");
                    string manifestData = File.ReadAllText(manifestPath);
                    JObject json = JObject.Parse(manifestData);
                    string strUUID1 = json["header"]["uuid"].ToString();
                    string strUUID2 = json["modules"][0]["uuid"].ToString();
                    uuid1 = new Guid(strUUID1);
                    uuid2 = new Guid(strUUID2);
                }
                else
                {
                    Console.WriteLine("Generating new manifest file.");
                    uuid1 = Guid.NewGuid();
                    uuid2 = Guid.NewGuid();
                }

                Manifest manifestFile = new Manifest(uuid1, uuid2,
                    projectName, "MCCompiled Project");
                filesToWrite.Add(manifestFile);
            }

            PushSelector(true);
            currentFiles.Push(new CommandFile(projectName));
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
        public void SetLastCompare(Token[] inputTokens) =>
            lastActualCompare[ScopeLevel] = inputTokens;
        /// <summary>
        /// Get the last if-statement tokens used at this scope.
        /// </summary>
        /// <returns></returns>
        public Token[] GetLastCompare() =>
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
        public void AddCommand(string command) =>
            CurrentFile.Add(PopPrepend() + command);
        /// <summary>
        /// Add a set of commands into a new branching file unless inline is set.
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommands(IEnumerable<string> commands, bool inline = false)
        {
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

            CommandFile file = StatementOpenBlock.GetNextBranchFile();
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
            string prepend = prependBuffer.ToString();
            CurrentFile.Add(prepend + command);
        }
        /// <summary>
        /// Add a set of commands into a new branching file, not modifying the prepend buffer.
        /// If inline is set, no branching file will be made.
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommandsClean(IEnumerable<string> commands, bool inline = false)
        {
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

            CommandFile file = StatementOpenBlock.GetNextBranchFile();
            file.Add(commands);

            AddExtraFile(file);
            CurrentFile.Add(buffer + Command.Function(file));
        }
        /// <summary>
        /// Add a file on its own to the list.
        /// </summary>
        /// <param name="file"></param>
        public void AddExtraFile(IBehaviorFile file) =>
            filesToWrite.Add(file);
        /// <summary>
        /// Add a command to the top of the 'head' file, being the main project function. Does not affect the prepend buffer.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommandHead(string command) =>
            HeadFile.AddTop(command);
        /// <summary>
        /// Adds a set of commands to the top of the 'head' file, being the main project function. Does not affect the prepend buffer.
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommandsHead(IEnumerable<string> commands)
        {
            if (commands.Count() < 1)
                return;
            HeadFile.AddTop(commands);
        }

        /// <summary>
        /// Set the content that will prepend the next added dirty command.
        /// </summary>
        /// <param name="content"></param>
        public void SetCommandPrepend(string content) =>
            prependBuffer.Clear().Append(content);
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
        public bool TryGetPPV(string name, out dynamic value)
        {
            if (name.StartsWith("$"))
                name = name.Substring(1);
            return ppv.TryGetValue(name, out value);
        }
        /// <summary>
        /// Set or create a preprocessor variable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetPPV(string name, object value) =>
            ppv[name] = value;
        /// <summary>
        /// Resolve all preprocessor variables in a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string ResolveString(string str)
        {
            foreach (var kv in ppv)
            {
                string name = '$' + kv.Key;
                string value = kv.Value.ToString();
                str = str.Replace(name, value);
            }

            return str;
        }
        public TokenLiteral ResolvePPV(TokenUnresolvedPPV unresolved)
        {
            int line = unresolved.lineNumber;
            string word = unresolved.word;

            if (TryGetPPV(word, out dynamic value))
            {
                if (value is int)
                    return new TokenIntegerLiteral(value, line);
                if (value is float)
                    return new TokenDecimalLiteral(value, line);
                if (value is bool)
                    return new TokenBooleanLiteral(value, line);
                if (value is string)
                    return new TokenStringLiteral(value, line);
                if (value is Coord)
                    return new TokenCoordinateLiteral(value, line);
                if (value is Selector)
                    return new TokenSelectorLiteral(value, line);
            }

            return null;
        }

        public void PushFile(CommandFile file) =>
            currentFiles.Push(file);
        public void PopFile()
        {
            unreachableCode = -1;
            filesToWrite.Add(currentFiles.Pop());
        }

        /// <summary>
        /// Write all files that have been generated.
        /// </summary>
        public void WriteAllFiles()
        {
            foreach(IBehaviorFile file in filesToWrite)
                WriteFileNow(file);
            filesToWrite.Clear();
        }
        /// <summary>
        /// Write an output file right now. Should be used when it might take up too much memory to hold.
        /// </summary>
        /// <param name="file"></param>
        public void WriteFileNow(IBehaviorFile file)
        {
            string dir = Path.Combine(projectName, file.GetOutputDirectory());
            Directory.CreateDirectory(dir);
            string outputFile = Path.Combine(dir, file.GetOutputFile());
            File.WriteAllBytes(outputFile, file.GetOutputData());
        }
    }
}