using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// The final stage of the compilation process. Runs statements and holds state on 
    /// </summary>
    public class Executor
    {
        public readonly string projectName;
        public readonly Dictionary<string, dynamic> ppv;

        readonly Statement[] statements;
        readonly Stack<CommandFile> currentFiles;
        readonly List<CommandFile> filesToWrite;
        readonly StringBuilder prependBuffer;

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

            currentFiles = new Stack<CommandFile>();
            filesToWrite = new List<CommandFile>();
            prependBuffer = new StringBuilder();

            currentFiles.Push(new CommandFile(projectName));
        }
        public void Execute()
        {
            for(int i = 0; i < statements.Length; i++)
            {
                Statement statement = statements[i];
                statement.Run0(this);
            }
        }

        /// <summary>
        /// Get the current file that should be written to.
        /// </summary>
        public CommandFile CurrentFile { get => currentFiles.Peek(); }
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
        public void AddCommandDirty(string command) =>
            CurrentFile.Add(PopPrepend() + command);
        /// <summary>
        /// Add a set of commands to the current file, all with the prepend buffer.
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommandsDirty(IEnumerable<string> commands)
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
        /// Add a command to the top of the current file. Does not affect the prepend buffer.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommandTop(string command) =>
            CurrentFile.AddTop(command);
        /// <summary>
        /// Add a set of commands to the top of the current file. Does not affect the prepend buffer.
        /// </summary>
        /// <param name="commands"></param>
        public void AddCommandsTop(IEnumerable<string> commands) =>
            CurrentFile.AddTop(commands);

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


        public void PushFile(CommandFile file) =>
            currentFiles.Push(file);
        public void PopFile() =>
            filesToWrite.Add(currentFiles.Pop());
    }
}