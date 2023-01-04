using mc_compiled.Commands.Selectors;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mc_compiled.MCC.Functions.Attributes;
using System.Security.Cryptography;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// Represents a function that can be called at runtime supporting both compile-time and run-time parameters.
    /// Supports customized return values and attributes. Treats every parameter as a run-time parameter in the end.
    /// </summary>
    public class RuntimeFunction : Function
    {
        public readonly CommandFile file;
        public readonly Selector defaultSelector;
        public ScoreboardValue returnValue;

        readonly bool isCompilerGenerated;
        readonly List<IFunctionAttribute> attributes;
        readonly List<FunctionParameter> parameters;
        
        public string name;                 // name used internally if the normal name won't work.
        public readonly string aliasedName; // user-facing name (keyword)

        public RuntimeFunction(string name, Selector defaultSelector, bool isCompilerGenerated = false)
        {
            this.aliasedName = name;
            this.name = name;
            this.defaultSelector = defaultSelector;
            this.isCompilerGenerated = isCompilerGenerated;

            this.file = new CommandFile(name, null, this);
            this.returnValue = null;

            this.attributes = new List<IFunctionAttribute>();
            this.parameters = new List <FunctionParameter>();
        }
        public RuntimeFunction AddParameter(FunctionParameter parameter)
        {
            this.parameters.Add(parameter);
            return this;
        }
        public RuntimeFunction AddParameters(IEnumerable<FunctionParameter> parameters)
        {
            this.parameters.AddRange(parameters);
            return this;
        }
        /// <summary>
        /// Require this functions's internal name to be hashed and hidden behind an alias.
        /// </summary>
        public RuntimeFunction ForceHash()
        {
            this.name = ScoreboardValue.StandardizedHash(this.name);
            return this;
        }


        public override string Keyword => aliasedName;
        protected override FunctionParameter[] Parameters => this.parameters.ToArray();
        public override int ParameterCount => this.parameters.Count;
        public override string[] Aliases => null;
        public override bool ImplicitCall => false;

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
            ScoreboardManager sb = executor.scoreboard;
            ScoreboardValue clone = ScoreboardValue.AsReturnValue(value);
            if (sb.definedTempVars.Add(clone))
                executor.AddCommandsHead(clone.CommandsDefine());

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

            if (sb.definedTempVars.Add(variable))
                executor.AddCommandsHead(variable.CommandsDefine());

            AddCommands(variable.CommandsSetLiteral(variable.Name, selector, value));
            returnValue = variable;
        }

        public override Token CallFunction(Token[] inputs, Executor executor, Statement statement)
        {
            ScoreboardManager sb = executor.scoreboard;
            List<string> commandsToCall = new List<string>();

            using (sb.PushTempState())
            {
                int parameterCount = ParameterCount;
                int inputsCount = inputs.Length;
                int parameterIndex = 0;

                for(int inputIndex = 0; inputIndex < parameterCount; inputIndex++)
                {
                    FunctionParameter currentParameter = this.parameters[parameterIndex++];

                    Token input;
                    if (inputIndex >= inputsCount)
                    {
                        if (currentParameter.optional)
                            input = currentParameter.defaultValue;
                        else
                            throw new StatementException(statement, $"Missing parameter '{currentParameter}' in function call.");

                        inputIndex--;

                    } else input = inputs[inputIndex];

                    if (currentParameter.resolution == FunctionParameterResolution.RUN_TIME)
                    {
                        string outputAccessor = currentParameter.scoreboard.Name;
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
                }
            }
        }

        public void AddCommand(string command) =>
            file.Add(command);
        public void AddCommandTop(string command) =>
            file.AddTop(command);
        public void AddCommands(IEnumerable<string> commands) =>
            file.Add(commands);
        public void AddCommandsTop(IEnumerable<string> commands) =>
            file.AddTop(commands);
    }
}
