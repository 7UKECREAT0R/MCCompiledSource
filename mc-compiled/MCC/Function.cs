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
        readonly List<Token> inputDefaults;
        readonly bool isCompilerGenerated;
        readonly CommandFile file;

        public ScoreboardValue returnValue;
        public readonly Selector defaultSelector;
        public readonly string name;

        public Function(string name, Selector defaultSelector, bool fromCompiler = false)
        {
            this.name = name;
            this.defaultSelector = defaultSelector;
            file = new CommandFile(name, null, this);
            isCompilerGenerated = fromCompiler;
            inputs = new List<ScoreboardValue>();
            inputDefaults = new List<Token>();
        }
        public Function AddParameter(ScoreboardValue parameter, TokenLiteral @default = null)
        {
            inputs.Add(parameter);
            inputDefaults.Add(@default);
            return this;
        }
        public Function AddParameters(IEnumerable<ScoreboardValue> parameters, IEnumerable<Token> defaults)
        {
            inputs.AddRange(parameters);
            inputDefaults.AddRange(defaults);
            return this;
        }
        /// <summary>
        /// Add commands and setup scoreboard to return a value.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="value"></param>
        /// <returns>A new scoreboard value that holds the returned value</returns>
        public void TryReturnValue(Statement caller, ScoreboardValue value, Executor executor, string selector)
        {
            if (returnValue != null)
            {
                Type type = returnValue.GetType();

                // check if types match
                if (!type.Equals(value.GetType()))
                    throw new StatementException(caller, $"All return statements in this function must return the same type. Return type: {GetType().Name}");
            }

            //   only run this code once for this function, that
            // will sort of 'define' what the return type should be
            ScoreboardValue clone = ScoreboardValue.AsReturnValue(value);
            foreach(string name in clone.GetAccessibleNames())
            {
                ScoreboardManager sb = executor.scoreboard;
                if(!sb.definedTempVars.Contains(name))
                {
                    sb.definedTempVars.Add(name);
                    executor.AddCommandsHead(clone.CommandsDefine());
                }
            }

            AddCommands(clone.CommandsSet(selector, value, null, null));
            returnValue = clone;
        }
        /// <summary>
        /// Add commands and setup scoreboard to return a value.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="value"></param>
        /// <returns>A new scoreboard value that holds the returned value</returns>
        public void TryReturnValue(Statement caller, Executor executor, TokenLiteral value, string selector)
        {
            if (returnValue != null)
            {
                Type type = returnValue.GetType();

                // check if types match
                if (!type.Equals(value))
                    throw new StatementException(caller, $"All return statements in this function must return the same type. Required: {GetType()}");

                return;
            }

            ScoreboardManager sb = executor.scoreboard;
            ScoreboardValue variable = ScoreboardValue.AsReturnValue(value, sb, caller);
            foreach (string name in variable.GetAccessibleNames())
            {
                if (!sb.definedTempVars.Contains(name))
                {
                    sb.definedTempVars.Add(name);
                    executor.AddCommandsHead(variable.CommandsDefine());
                }
            }

            AddCommands(variable.CommandsSetLiteral(variable.baseName, selector, value));
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
            int inputsLength = inputs.Length;
            for(int i = 0; i < count; i++)
            {
                ScoreboardValue output = this.inputs[i];
                Token input;
                if (i >= inputsLength)
                {
                    input = inputDefaults[i];
                    if (input == null)
                        throw new StatementException(caller, $"Missing parameter '{output.baseName}' in function call.");
                } else
                    input = inputs[i];

                string outputAccessor = output.baseName; // accessor is base name in integer case

                commands.AddRange(output.CommandsDefine());

                if (input is TokenLiteral)
                {
                    TokenLiteral literal = input as TokenLiteral;
                    commands.AddRange(output.CommandsSetLiteral(outputAccessor, selector, literal));
                }
                else if (input is TokenIdentifierValue)
                {
                    ScoreboardValue src = (input as TokenIdentifierValue).value;
                    string thisAccessor = (input as TokenIdentifierValue).word;
                    commands.AddRange(output.CommandsSet(selector, src, thisAccessor, outputAccessor));
                }
                else
                    throw new StatementException(caller, $"Unexpected parameter type for input {output.baseName}. Got: {input.GetType().Name}");
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
        public void AddCommands(IEnumerable<string> commands) =>
            file.Add(commands);
        public void AddCommandsTop(IEnumerable<string> commands) =>
            file.AddTop(commands);

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