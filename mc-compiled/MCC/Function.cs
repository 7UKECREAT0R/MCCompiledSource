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
        readonly List<string> inputs;
        readonly bool isCompilerGenerated;
        readonly CommandFile file;

        public ScoreboardValue returnValue;
        public readonly string name;

        public Function(string name, bool fromCompiler = false)
        {
            this.name = name;
            file = new CommandFile(name, null, this);
            isCompilerGenerated = fromCompiler;
            inputs = new List<string>();
        }
        public Function AddParameter(string parameter)
        {
            inputs.Add(parameter);
            return this;
        }
        public Function AddParameters(IEnumerable<string> parameters)
        {
            inputs.AddRange(parameters);
            return this;
        }
        /// <summary>
        /// Add commands and setup scoreboard to return a value.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="value"></param>
        /// <returns>A new scoreboard value that holds the returned value</returns>
        public void TryReturnValue(Statement caller, ScoreboardValue value)
        {
            if (returnValue != null)
            {
                Type type = returnValue.GetType();

                // check if types match
                if (!type.Equals(value))
                    throw new StatementException(caller, $"All return statements in this function must return the same type. Required: {GetType()}");

                return;
            }

            //   only run this code once for this function, that
            // will sort of 'define' what the return type should be
            ScoreboardValue clone = ScoreboardValue.GetReturnValue(value);
            foreach(string name in clone.GetAccessibleNames())
            {
                ScoreboardManager sb = value.manager;
                if(!sb.definedTempVars.Contains(name))
                {
                    sb.definedTempVars.Add(name);
                    AddCommandsTop(clone.CommandsInit());
                    AddCommandsTop(clone.CommandsDefine());
                }
            }

            returnValue = clone;
        }
        /// <summary>
        /// Add commands and setup scoreboard to return a value.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="value"></param>
        /// <returns>A new scoreboard value that holds the returned value</returns>
        public void TryReturnValue(Statement caller, ScoreboardManager sb, TokenLiteral value)
        {
            
            if (returnValue != null)
            {
                Type type = returnValue.GetType();

                // check if types match
                if (!type.Equals(value))
                    throw new StatementException(caller, $"All return statements in this function must return the same type. Required: {GetType()}");

                return;
            }

            ScoreboardValue variable = ScoreboardValue.GetReturnValue(value, sb, caller);
            foreach (string name in variable.GetAccessibleNames())
            {
                if (!sb.definedTempVars.Contains(name))
                {
                    sb.definedTempVars.Add(name);
                    AddCommandsTop(variable.CommandsInit());
                    AddCommandsTop(variable.CommandsDefine());
                }
            }

            returnValue = variable;
        }

        public CommandFile File
        {
            get => file;
        }
        public int ParameterCount
        {
            get => inputs.Count;
        }
        public string[] CallFunction(string selector, Statement caller, ScoreboardManager sb, params Token[] inputs)
        {
            List<string> commands = new List<string>();

            sb.PushTempState();

            int count = this.inputs.Count;
            for(int i = 0; i < count; i++)
            {
                Token input = inputs[i];
                string accessor = this.inputs[i];

                ScoreboardValue output = new ScoreboardValueInteger(accessor, sb);
                commands.AddRange(output.CommandsInit());

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
                else
                    throw new StatementException(caller, $"Unexcpected parameter type for input {output.baseName}. Got: {input.GetType()}");
            }

            sb.PopTempState();

            commands.Add(Command.Function(this.file));
            return commands.ToArray();
        }

        // forward calls to the file it references
        public void AddCommand(string command) =>
            file.Add(command);
        public void AddCommandTop(string command) =>
            file.AddTop(command);
        public void AddCommands(IEnumerable<string> command) =>
            file.Add(command);
        public void AddCommandsTop(IEnumerable<string> command) =>
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

        public override string ToString()
        {
            return $"{name}({string.Join(" ", inputs)})";
        }
    }
}
