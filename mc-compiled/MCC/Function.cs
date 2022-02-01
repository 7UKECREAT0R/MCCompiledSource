using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Represents a function that can be called.
    /// </summary>
    public class Function
    {
        readonly List<ScoreboardValue> inputs;
        readonly bool isCompilerGenerated;
        readonly CommandFile file;

        public readonly string name;

        public Function(string name, bool fromCompiler = false)
        {
            this.name = name;
            file = new CommandFile(name);
            isCompilerGenerated = fromCompiler;
            inputs = new List<ScoreboardValue>();
        }
        public Function AddParameter(ScoreboardValue parameter)
        {
            inputs.Add(parameter);
            return this;
        }
        public Function AddParameters(IEnumerable<ScoreboardValue> parameters)
        {
            inputs.AddRange(parameters);
            return this;
        }

        public CommandFile File
        {
            get => file;
        }
        public string[] CallFunction(string selector, params Token[] inputs)
        {
            List<string> commands = new List<string>();

            int count = this.inputs.Count;
            for(int i = 0; i < count; i++)
            {
                Token input = inputs[i];
                ScoreboardValue output = this.inputs[i];
                string accessor = output.baseName;

                if (input is TokenLiteral)
                {
                    TokenLiteral literal = input as TokenLiteral;
                    commands.AddRange(output.CommandsSetLiteral(accessor, selector, literal));
                }
                else if (input is TokenIdentifierValue)
                {
                    ScoreboardValue src = (input as TokenIdentifierValue).value;
                    string thisAccessor = (input as TokenIdentifierValue).word;
                    commands.AddRange(output.CommandsSet(selector, src, thisAccessor, accessor));
                }
            }

            commands.Add(Command.Function(this.file));
            return commands.ToArray();
        }

        // forward calls to the file it references
        public void AddCommand(string command) =>
            file.Add(command);
        public void AddCommandTop(string command) =>
            file.AddTop(command);
        public void AddCommand(IEnumerable<string> command) =>
            file.Add(command);
        public void AddCommandTop(IEnumerable<string> command) =>
            file.AddTop(command);

        /// <summary>
        /// Case-insensitive-match this function's name.
        /// </summary>
        /// <param name="otherName"></param>
        /// <returns></returns>
        public bool Matches(string otherName)
        {
            return name.ToUpper().Trim().Equals
                (otherName.ToUpper().Trim());
        }
    }
}
