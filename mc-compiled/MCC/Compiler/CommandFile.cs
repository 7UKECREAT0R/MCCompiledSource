using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A file which holds minecraft commands.
    /// </summary>
    public class CommandFile : IAddonFile
    {
        List<string> commands = new List<string>();

        public readonly RuntimeFunction userFunction;
        public bool IsUserFunction
        {
            get => userFunction != null;
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

        public string folder;
        public string name;

        public CommandFile(string name, string folder = null, RuntimeFunction userFunction = null)
        {
            this.name = name;
            this.folder = folder;
            this.userFunction = userFunction;
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
            hashCode = hashCode * -1521134295 + folder.GetHashCode();
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

        public string GetExtendedDirectory() =>
            folder;
        public string GetOutputFile() =>
            $"{name}.mcfunction";
        public byte[] GetOutputData()
        {
            string text = string.Join("\n", commands);
            return Encoding.UTF8.GetBytes(text);
        }
        public OutputLocation GetOutputLocation() =>
            OutputLocation.b_FUNCTIONS;
    }
}
