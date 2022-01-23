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
    public class CommandFile : IBehaviorOutput
    {
        List<string> commands = new List<string>();

        public readonly string folder;
        public readonly string name;
        public CommandFile(string name, string folder = null)
        {
            this.name = name;
            this.folder = folder;
        }

        public void Add(string command) =>
            commands.Add(command);
        public void Add(IEnumerable<string> commands) =>
            this.commands.AddRange(commands);

        public void AddTop(string command) =>
            commands.Insert(0, command);
        public void AddTop(IEnumerable<string> commands) =>
            this.commands.InsertRange(0, commands);

        public string GetOutputDirectory()
        {
            if(folder == null)
                return "functions\\";

            return Path.Combine("functions", folder);
        }
        public string GetOutputFile() =>
            $"{name}.mcfunction";
        public byte[] GetOutputData()
        {
            string text = string.Join("\n", commands);
            return Encoding.UTF8.GetBytes(text);
        }
    }
}
