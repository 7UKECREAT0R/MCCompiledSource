using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
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
        internal readonly List<FunctionParameter> parameters;
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
            parameters = new List<FunctionParameter>();
        }
        public Function AddParameter(FunctionParameter parameter)
        {
            parameters.Add(parameter);
            return this;
        }
        public Function AddParameters(IEnumerable<FunctionParameter> parameters)
        {
            this.parameters.AddRange(parameters);
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
                    throw new StatementException(caller, $"All return statements in this function must return the same type. Return type: {type.Name}");
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

            AddCommands(variable.CommandsSetLiteral(variable.Name, selector, value));
            returnValue = variable;
        }

        public CommandFile File
        {
            get => file;
        }
        public int ParameterCount
        {
            get => parameters.Count;
        }
        public List<string> CallFunction(string selector, Statement caller, ScoreboardManager sb, params Token[] inputs)
        {
            List<string> commands = new List<string>();

            sb.PushTempState();

            int count = this.parameters.Count;
            int inputsLength = inputs.Length;
            int parameterIndex = 0;
            for(int inputIndex = 0; inputIndex < count; inputIndex++)
            {
                FunctionParameter parameter = this.parameters[parameterIndex++];
                Token input;
                if (inputIndex >= inputsLength)
                {
                    if (parameter.HasDefault)
                        throw new StatementException(caller, $"Missing parameter '{parameter}' in function call.");
                    else
                        input = parameter.defaultValue;
                    inputIndex--;

                } else
                    input = inputs[inputIndex];

                // Scoreboard parameter implementation
                if (parameter.IsScoreboard)
                {
                    string outputAccessor = parameter.scoreboard.Name;
                    commands.AddRange(parameter.scoreboard.CommandsDefine());

                    if (input is TokenLiteral)
                    {
                        TokenLiteral literal = input as TokenLiteral;
                        commands.AddRange(parameter.scoreboard.CommandsSetLiteral(outputAccessor, selector, literal));
                    }
                    else if (input is TokenIdentifierValue)
                    {
                        ScoreboardValue src = (input as TokenIdentifierValue).value;
                        string thisAccessor = (input as TokenIdentifierValue).word;
                        commands.AddRange(parameter.scoreboard.CommandsSet(selector, src, thisAccessor, outputAccessor));
                    }
                    else
                        throw new StatementException(caller, $"Unexpected parameter type for input {parameter.scoreboard.AliasName}. Got: {input.GetType().Name}");
                }
                // PPV parameter implementation (unused)
                /*else if(parameter.IsPPV)
                {
                    if (!(input is IPreprocessor))
                        throw new StatementException(caller, $"Input {input} for PPV \"{parameter.ppvName}\" cannot be held in a preprocessor variable.");

                    object single = (input as IPreprocessor).GetValue();

                    string ppv = parameter.ppvName;
                    sb.executor.SetPPV(ppv, new[] { single });
                }*/
                // rest of implementations go here
                else
                    throw new StatementException(caller, $"Unimplemented parameter type: {parameter.type}");
            }

            sb.PopTempState();
            commands.Add(Command.Function(this.file));
            return commands;
        }

        // these methods forward calls to the file this function references
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
            return $"{name}({string.Join(" ", parameters)})";
        }
    }
}