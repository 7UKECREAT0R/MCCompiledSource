using mc_compiled.MCC.Functions.Types;
using mc_compiled.Modding;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        private readonly List<CommandFile> calls = new List<CommandFile>();
        /// <summary>
        /// A list of all of the commands in this file.
        /// </summary>
        internal readonly List<string> commands = new List<string>();
        
        private bool isInUse = false;
        private bool isTest = false;
        private bool hasAssertions = false;

        internal bool IsInUse
        {
            get => isInUse;
            set
            {
                // update all calls to also be in use.
                if(value)
                {
                    foreach (CommandFile call in calls)
                        call.AsInUse();
                }

                isInUse = value;
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
            if (isInUse)
                file.AsInUse();

            calls.Add(file);
        }
        
        public readonly RuntimeFunction runtimeFunction;
        public bool IsUserFunction
        {
            get => runtimeFunction != null;
        }
        public bool IsRootFile
        {
            get; private set;
        }
        public int Length
        {
            get => commands.Count;
        }

        public string CommandReference
        {
            get
            {
                if (folder == null)
                    return name;

                return folder + '/' + name;
            }
        }
        public string[] Folders
        {
            set
            {
                if (isTest)
                    folder = Executor.MCC_TESTS_FOLDER + '/' + string.Join("/", value);
                else
                    folder = string.Join("/", value);
            }
        }

        internal string folder;
        internal readonly string name;
        private bool _doNotWrite;

        /// <summary>
        /// Do NOT write this commandfile to the output.
        /// </summary>
        internal bool DoNotWrite
        {
            get => _doNotWrite || (!isInUse && !Program.EXPORT_ALL);
            set => _doNotWrite = value;
        }

        /// <summary>
        /// Create a new command file with an optional runtime function linked to it.
        /// </summary>
        /// <param name="isInUse"></param>
        /// <param name="name"></param>
        /// <param name="folder"></param>
        /// <param name="runtimeFunction"></param>
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
        public CommandFile AsInUse()
        {
            this.IsInUse = true;
            return this;
        }
        /// <summary>
        /// Returns this CommandFile with isInUse set to false.
        /// </summary>
        /// <returns></returns>
        public CommandFile AsNotInUse()
        {
            this.IsInUse = false;
            return this;
        }
        /// <summary>
        /// Returns this CommandFile with isTest set to true.
        /// </summary>
        /// <returns></returns>
        internal CommandFile AsTest()
        {
            if(!this.isTest)
            {
                if (this.folder == null)
                    this.folder = Executor.MCC_TESTS_FOLDER;
                else
                    this.folder = Executor.MCC_TESTS_FOLDER + '/' + this.folder;
            }

            this.isTest = true;
            return this;
        }
        /// <summary>
        /// Mark this file as containing a test assertion.
        /// </summary>
        internal void MarkAssertion()
        {
            this.hasAssertions = true;
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
            return obj is CommandFile file &&
                folder == file.folder &&
                name == file.name;
        }
        public override int GetHashCode()
        {
            int hashCode = -172474549;
            hashCode = hashCode * -1521134295 + (folder?.GetHashCode()).GetValueOrDefault(); // folder could be null
            hashCode = hashCode * -1521134295 + name.GetHashCode();
            return hashCode;
        }

        public void Add(string command) =>
            commands.Add(command);
        public void Add(IEnumerable<string> commands) =>
            this.commands.AddRange(commands);

        public void AddTop(string command) =>
            commands.Insert(0, command);
        public void AddTop(IEnumerable<string> commands) =>
            this.commands.InsertRange(0, commands);

        /// <summary>
        /// Adds the following standard comment to the file: "Located at (callingFile) line (callingFile.Length)"
        /// </summary>
        /// <param name="callingFile"></param>
        internal void AddTrace(CommandFile callingFile)
        {
            commands.Add($"# Located at {callingFile.CommandReference} line {callingFile.Length + 1}");
            commands.Add("");
        }

        /// <summary>
        /// Adds the following standard comment to the file: "Located at (callingFile) line (line)"
        /// </summary>
        /// <param name="callingFile">The command file to make reference to.</param>
        /// <param name="line">The line of the file to reference.</param>
        internal void AddTrace(CommandFile callingFile, int line)
        {
            if (callingFile.IsRootFile)
                commands.Add($"# Located in {callingFile.CommandReference}");
            else
                commands.Add($"# Located at {callingFile.CommandReference} line {line}");

            commands.Add("");
        }

        public string GetExtendedDirectory()
        {
            if(DoNotWrite)
                return null;

            // correct path separators
            return folder?.Replace('/', Path.DirectorySeparatorChar);
        }
        public string GetExtendedDirectoryIDoNotCareIfYouDontWantToWriteIt()
        {
            // correct path separators
            return folder?.Replace('/', Path.DirectorySeparatorChar);
        }
        public string GetOutputFile()
        {
            if (DoNotWrite)
                return null;

            return $"{name}.mcfunction";
        }
        public string GetOutputFileIDoNotCareIfYouDontWantToWriteIt()
        {
            return $"{name}.mcfunction";
        }
        public byte[] GetOutputData()
        {
            string text = string.Join("\n", commands);
            return Encoding.UTF8.GetBytes(text);
        }
        public OutputLocation GetOutputLocation() =>
            OutputLocation.b_FUNCTIONS;

    }
}
