using mc_compiled.Commands;
using mc_compiled.Json;
using mc_compiled.Modding;
using System;
using System.Collections.Generic;
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
        public static readonly Regex FSTRING_VAR = new Regex("{([a-zA-Z-:._]{1,16})}");

        public readonly string projectName;

        Statement[] statements;
        int readIndex = 0;

        readonly List<Macro> macros;
        readonly bool[] lastPreprocessorCompare;
        readonly Token[][] lastActualCompare;
        readonly Dictionary<string, dynamic> ppv;
        readonly Stack<CommandFile> currentFiles;
        readonly Stack<Selector.Core> selections;
        readonly List<IBehaviorFile> filesToWrite;
        readonly StringBuilder prependBuffer;

        public readonly ScoreboardManager scoreboard;
        /// <summary>
        /// Resolve an FString into rawtext terms. Also adds all setup commands for variables.
        /// </summary>
        /// <param name="fstring"></param>
        /// <returns></returns>
        public List<JSONRawTerm> FString(string fstring)
        {
            MatchCollection matches = FSTRING_VAR.Matches(fstring);
            if (matches.Count < 1)
                return new List<JSONRawTerm>() { new JSONText(fstring) };

            List<JSONRawTerm> terms = new List<JSONRawTerm>();
            Stack<string> pieces = new Stack<string>(FSTRING_VAR.Split(fstring).Reverse());

            int index = 0;
            string sel = "@" + ActiveSelector;
            foreach (Match match in matches)
            {
                if (match.Index != 0 && pieces.Count > 0)
                    terms.Add(new JSONText(pieces.Pop()));
                pieces.Pop();

                string src = match.Value;
                string accessor = match.Groups[1].Value;

                if(scoreboard.TryGetByAccessor(accessor, out ScoreboardValue value))
                {
                    AddCommandsClean(value.CommandsRawTextSetup(accessor, sel, index));
                    terms.AddRange(value.ToRawText(accessor, sel, index++));
                } else
                    terms.Add(new JSONText(src));
            }

            while (pieces.Count > 0)
                terms.Add(new JSONText(pieces.Pop()));

            return terms;
        }

        public Selector.Core ActiveSelector
        {
            get => selections.Peek();
            set
            {
                selections.Pop();
                selections.Push(value);
            }
        }
        public string ActiveSelectorStr
        {
            get => $"@{ActiveSelector}";
        }
        public void PushSelector(bool doesAlign)
        {
            if (doesAlign)
                selections.Push(Selector.Core.s);
            else
                selections.Push(ActiveSelector);
        }
        public void PopSelector() =>
            selections.Pop();

        public bool HasNext
        {
            get => readIndex < statements.Length;
        }
        public Statement Peek() => statements[readIndex];
        public Statement Next() => statements[readIndex++];
        public T Next<T>() where T: Statement => statements[readIndex++] as T;
        public T Peek<T>() where T : Statement => statements[readIndex] as T;
        public T Peek<T>(int skip) where T : Statement => statements[readIndex + skip] as T;
        public bool NextIs<T>() where T: Statement => statements[readIndex] is T;
        /// <summary>
        /// Return an array of the next x statements.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Statement[] Peek(int amount)
        {
            Statement[] ret = new Statement[amount];

            int write = 0;
            for(int i = readIndex; i < statements.Length; i++)
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

            macros = new List<Macro>();
            selections = new Stack<Selector.Core>();
            lastPreprocessorCompare = new bool[100];
            lastActualCompare = new Token[100][];
            currentFiles = new Stack<CommandFile>();
            filesToWrite = new List<IBehaviorFile>();
            prependBuffer = new StringBuilder();

            selections.Push(Selector.Core.s);
            currentFiles.Push(new CommandFile(projectName));
        }
        /// <summary>
        /// Run this executor start to finish.
        /// </summary>
        public void Execute()
        {
            readIndex = 0;

            while(HasNext)
            {
                Statement statement = Next();
                statement.Run0(this);
            }
        }
        /// <summary>
        /// Temporarily run another subsection of statements then resume this executor.
        /// </summary>
        public void ExecuteSubsection(Statement[] section)
        {
            Statement[] restore0 = statements;
            int restore1 = readIndex;

            statements = section;
            readIndex = 0;
            while (HasNext)
            {
                Statement statement = Next();
                statement.Run0(this);
            }

            // now its done, so restore state
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
        public void AddMacro(Macro macro) => macros.Add(macro);
        public Macro? LookupMacro(string name)
        {
            foreach (Macro macro in macros)
                if (macro.Matches(name))
                    return macro;
            return null;
        }

        /// <summary>
        /// Get the current file that should be written to.
        /// </summary>
        public CommandFile CurrentFile { get => currentFiles.Peek(); }
        public CommandFile HeadFile { get => currentFiles.First(); }

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
        /// Add a set of commands to the current file, all with the prepend buffer.
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommands(IEnumerable<string> commands)
        {
            string prepend = PopPrepend();
            CurrentFile.Add(commands.Select(c => prepend + c));
        }
        /// <summary>
        /// Add a command to the current file, ignoring the prepend buffer.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommandClean(string command) =>
            CurrentFile.Add(command);
        /// <summary>
        /// Add a set of commands to the current file, ignoring the prepend buffer.
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommandsClean(IEnumerable<string> commands) =>
            CurrentFile.Add(commands);
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
        public void AddCommandsHead(IEnumerable<string> commands) =>
            HeadFile.AddTop(commands);

        /// <summary>
        /// Set the content that will prepend the next added dirty command.
        /// </summary>
        /// <param name="content"></param>
        public void SetCommandPrepend(string content) =>
            prependBuffer.Clear().Append(content);
        /// <summary>
        /// Append to the content that will prepend the next added dirty command.
        /// </summary>
        /// <param name="content"></param>
        public void AppendCommandPrepend(string content) =>
            prependBuffer.Append(content);

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
            foreach(var kv in ppv)
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

            if(ppv.TryGetValue(word, out dynamic value))
            {
                if (value is int)
                    return new TokenIntegerLiteral(value, line);
                if (value is float)
                    return new TokenDecimalLiteral(value, line);
                if (value is bool)
                    return new TokenBooleanLiteral(value, line);
                if (value is string)
                    return new TokenStringLiteral(value, line);
            }

            return null;
        }

        public void PushFile(CommandFile file) =>
            currentFiles.Push(file);
        public void PopFile() =>
            filesToWrite.Add(currentFiles.Pop());
    }
}