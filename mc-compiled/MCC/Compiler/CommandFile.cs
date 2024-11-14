using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.Modding;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A file which holds minecraft commands.
    /// </summary>
    public class CommandFile : IAddonFile
    {
        /// <summary>
        /// A list of all of the other CommandFiles that this file makes reference to.
        /// </summary>
        private readonly List<CommandFile> calls = [];
        /// <summary>
        /// A list of all of the commands in this file.
        /// </summary>
        internal readonly List<string> commands = [];

        private bool isAsync;
        private bool isInUse;
        private bool isTest;
        private bool hasAssertions;
        
        internal bool IsInUse
        {
            get => this.isInUse;
            private set
            {
                // update all calls to also be in use.
                if(value)
                {
                    foreach (CommandFile call in this.calls)
                        call.AsInUse();
                }

                this.isInUse = value;
            }
        }
        /// <summary>
        /// Add a reference to the given CommandFile to this CommandFile. Indicates that this file calls the other file.
        /// <code>
        /// a -> [ b() ]
        /// </code>
        /// </summary>
        internal void RegisterCall(CommandFile file)
        {
            if (this.isInUse)
                file.AsInUse();

            this.calls.Add(file);
        }
        
        public readonly RuntimeFunction runtimeFunction;
        public bool IsUserFunction => this.runtimeFunction != null;
        public int Length => this.commands.Count;
        public bool IsRootFile
        {
            get; private set;
        }

        public string CommandReference
        {
            get
            {
                if (this.folder == null)
                    return this.name;

                return this.folder + '/' + this.name;
            }
        }
        public string CommandReferenceHash
        {
            get
            {
                int hash = GetHashCode();
                
                if (hash >= 0)
                    return hash.ToString();
                
                hash *= -1;
                return "0" + hash;

            }
        }
        
        public string[] Folders
        {
            set
            {
                if (this.isTest)
                    this.folder = Executor.MCC_TESTS_FOLDER + '/' + string.Join("/", value);
                else
                    this.folder = string.Join("/", value);
            }
        }

        internal string folder;
        internal string name;
        private bool _doNotWrite;

        /// <summary>
        /// Do NOT write this <see cref="CommandFile"/> to the output.
        /// </summary>
        internal bool DoNotWrite
        {
            get => this._doNotWrite || (!this.isInUse && !Program.EXPORT_ALL);
            set => this._doNotWrite = value;
        }

        /// <summary>
        /// Create a new command file with an optional runtime function linked to it.
        /// </summary>
        /// <param name="isInUse">Is the file currently in use? (the file will be omitted if false and EXPORT_ALL is disabled.)</param>
        /// <param name="name">The name of the command file, not including folder.</param>
        /// <param name="folder">The folder path this command file is contained in; can be separated using either slash type. Default is null.</param>
        /// <param name="runtimeFunction">The <see cref="RuntimeFunction"/> that is linked to this command file, if any. Default is null.</param>
        public CommandFile(bool isInUse, string name, string folder = null, RuntimeFunction runtimeFunction = null)
        {
            this.isInUse = isInUse;
            this.name = name;
            this.folder = folder;
            this.runtimeFunction = runtimeFunction;
        }
        /// <summary>
        /// Returns this CommandFile with IsRootFile set to true. Never use this on anything but the head file.
        /// </summary>
        /// <returns></returns>
        internal CommandFile AsRoot()
        {
            this.IsRootFile = true;
            return this;
        }
        /// <summary>
        /// Returns this CommandFile with isInUse set to true.
        /// </summary>
        /// <returns></returns>
        public void AsInUse()
        {
            this.IsInUse = true;
        }
        /// <summary>
        /// Returns this CommandFile with isTest set to true.
        /// </summary>
        /// <returns></returns>
        internal void AsTest()
        {
            if(!this.isTest)
            {
                if (this.folder == null)
                    this.folder = Executor.MCC_TESTS_FOLDER;
                else
                    this.folder = Executor.MCC_TESTS_FOLDER + '/' + this.folder;
            }

            this.isTest = true;
        }
        /// <summary>
        /// Mark this file as containing a test assertion.
        /// </summary>
        internal void MarkAssertion()
        {
            this.hasAssertions = true;
        }
        /// <summary>
        /// Mark this file as an async stage; used in static analysis for async control flow.
        /// </summary>
        internal void MarkAsync()
        {
            this.isAsync = true;
        }
        
        internal bool IsAsync
        {
            get => this.isAsync;
        }
        internal bool IsTest
        {
            get => this.isTest;
        }
        
        /// <summary>
        /// Returns if this file is a valid test, if it is. Otherwise, true.
        /// </summary>
        internal bool IsValidTest
        {
            get
            {
                if (!this.isTest)
                    return true;
                return this.hasAssertions;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is CommandFile file && this.folder == file.folder && this.name == file.name;
        }
        
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            int hashCode = -172474549;
            hashCode = hashCode * -1521134295 + (this.folder?.GetHashCode()).GetValueOrDefault(); // folder could be null
            hashCode = hashCode * -1521134295 + this.name.GetHashCode();
            return hashCode;
        }

        public void Add(string command) => this.commands.Add(command);
        public void Add(IEnumerable<string> newCommands) =>
            this.commands.AddRange(newCommands);
        public void CopyFrom(CommandFile sustainLoop)
        {
            this.commands.AddRange(sustainLoop.commands);
        }

        public void AddTop(string command) => this.commands.Insert(0, command);
        public void AddTop(IEnumerable<string> newCommands) =>
            this.commands.InsertRange(0, newCommands);

        /// <summary>
        /// Adds the following standard comment to the file: "Located at (callingFile) line (callingFile.Length)"
        /// </summary>
        /// <param name="callingFile"></param>
        internal void AddTrace(CommandFile callingFile)
        {
            this.commands.Add($"# Located at {callingFile.CommandReference} line {callingFile.Length + 1}");
            this.commands.Add("");
        }

        /// <summary>
        /// Adds the following standard comment to the file: "Located at (callingFile) line (line)"
        /// </summary>
        /// <param name="callingFile">The command file to make reference to.</param>
        /// <param name="line">The line of the file to reference.</param>
        internal void AddTrace(CommandFile callingFile, int line)
        {
            if (callingFile.IsRootFile)
                this.commands.Add($"# Located in {callingFile.CommandReference}");
            else
                this.commands.Add($"# Located at {callingFile.CommandReference} line {line}");

            this.commands.Add("");
        }

        public string GetExtendedDirectory()
        {
            if(this.DoNotWrite)
                return null;

            // correct path separators
            return this.folder?.Replace('/', Path.DirectorySeparatorChar);
        }
        public string GetExtendedDirectoryIDoNotCareIfYouDontWantToWriteIt()
        {
            // correct path separators
            return this.folder?.Replace('/', Path.DirectorySeparatorChar);
        }
        public string GetOutputFile()
        {
            if (this.DoNotWrite)
                return null;

            return $"{this.name}.mcfunction";
        }
        public string GetOutputFileIDoNotCareIfYouDontWantToWriteIt()
        {
            return $"{this.name}.mcfunction";
        }
        public byte[] GetOutputData()
        {
            string text = string.Join("\n", this.commands);
            return Encoding.UTF8.GetBytes(text);
        }
        public OutputLocation GetOutputLocation() =>
            OutputLocation.b_FUNCTIONS;
    }
}
