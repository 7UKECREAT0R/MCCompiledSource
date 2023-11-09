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
        private List<CommandFile> calls = new List<CommandFile>();

        private bool isInUse = false;
        internal bool IsInUse
        {
            get => isInUse;
            set
            {
                // update all calls to also be in use.
                if(value)
                {
                    foreach (var call in calls)
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


        /// <summary>
        /// A list of all of the commands in this file.
        /// </summary>
        internal List<string> commands = new List<string>();

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
                folder = string.Join("/", value);
            }
        }

        public string folder;
        public string name;
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
        internal CommandFile AsInUse()
        {
            this.IsInUse = true;
            return this;
        }
        /// <summary>
        /// Returns this CommandFile with isInUse set to false.
        /// </summary>
        /// <returns></returns>
        internal CommandFile AsNotInUse()
        {
            this.IsInUse = false;
            return this;
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
        /// <param name="callingFile"></param>
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
            if (folder == null)
                return null;

            // correct path separators
            string correctedFolder = folder.Replace('/', Path.DirectorySeparatorChar);

            // return the corrected directory
            return correctedFolder;
        }
        public string GetOutputFile()
        {
            if (DoNotWrite)
                return null;

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
