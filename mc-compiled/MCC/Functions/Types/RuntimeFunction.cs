using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler.TypeSystem;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// Represents a function that can be called at runtime supporting both compile-time and run-time parameters.
    /// Supports customized return values and attributes. Treats every parameter as a run-time parameter in the end.
    /// </summary>
    public class RuntimeFunction : Function
    {
        internal bool isAddedToExecutor; // if the function's files have been added to the executor yet.

        public readonly CommandFile file;
        public readonly Statement creationStatement;
        private ScoreboardValue returnValue;

        public readonly bool isCompilerGenerated;
        protected readonly List<RuntimeFunctionParameter> parameters;
        protected readonly List<IAttribute> attributes;

        public bool isExtern;               // created outside of MCCompiled, assume parameter names are as-listed.
        public readonly string aliasedName; // user-facing name (keyword)
        public string name;                 // name used internally if the normal name won't work.
        public string documentation;        // docs
        bool _hasSignaled = false;

        public RuntimeFunction(Statement creationStatement, string aliasedName, string name, string documentation, IAttribute[] attributes, bool isCompilerGenerated = false)
        {
            this.creationStatement = creationStatement;
            this.isAddedToExecutor = false;

            this.aliasedName = aliasedName;
            this.name = name;
            this.documentation = documentation;
            this.isCompilerGenerated = isCompilerGenerated;

            this.file = new CommandFile(false, name, null, this);
            this.returnValue = null;

            if (attributes == null)
                attributes = Array.Empty<IAttribute>();

            this.attributes = new List<IAttribute>(attributes);
            this.parameters = new List<RuntimeFunctionParameter>();
        }
        /// <summary>
        /// Signal to this function's attributes that they have been added to a function.
        /// </summary>
        /// <param name="callingStatement"></param>
        /// <exception cref="Exception">If this method has already been called once.</exception>
        internal void SignalToAttributes(Statement callingStatement)
        {
            if (_hasSignaled)
                throw new Exception($"Attempt to call SignalToAttributes again on function '{name}'");

            _hasSignaled = true;

            foreach (IAttribute attribute in attributes)
                attribute.OnAddedFunction(this, callingStatement);
        }

        /// <summary>
        /// Adds a runtime parameter to this function.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>This object for chaining.</returns>
        public RuntimeFunction AddParameter(RuntimeFunctionParameter parameter)
        {
            this.parameters.Add(parameter);
            return this;
        }
        /// <summary>
        /// Adds multiple runtime parameters to this function.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>This object for chaining.</returns>
        public RuntimeFunction AddParameters(IEnumerable<RuntimeFunctionParameter> parameters)
        {
            this.parameters.AddRange(parameters);
            return this;
        }
        /// <summary>
        /// Adds multiple runtime parameters to this function.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>This object for chaining.</returns>
        public RuntimeFunction AddParameters(params RuntimeFunctionParameter[] parameters)
        {
            this.parameters.AddRange(parameters);
            return this;
        }
        /// <summary>
        /// Require this functions' internal name to be hashed and hidden behind an alias.
        /// </summary>
        /// <returns>This object for chaining.</returns>
        public RuntimeFunction ForceHash()
        {
            this.name = ScoreboardValue.StandardizedHash(this.name);
            return this;
        }

        public override string Keyword => aliasedName;
        public override string Returns => returnValue?.GetExtendedTypeKeyword();
        public override string Documentation => documentation;
        public override FunctionParameter[] Parameters => this.parameters.Cast<FunctionParameter>().ToArray();
        public override int ParameterCount => this.parameters.Count;
        public override string[] Aliases => null;
        public override int Importance => 0; // least important. always call runtime functions at last resort.
        public override bool ImplicitCall => false;
        public override bool AdvertiseOverLSP => true;

        /// <summary>
        /// Add commands and setup scoreboard to return a value.
        /// </summary>
        /// <param name="value">The value to try to return.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="caller">The statement to blame for when everything explodes.</param>
        /// <returns>A new scoreboard value that holds the returned value</returns>
        public void TryReturnValue(ScoreboardValue value, Executor executor, Statement caller)
        {
            IEnumerable<string> commands;
            if (returnValue != null && value.NeedsToBeConvertedFor(returnValue))
            {
                commands = value.CommandsConvert(returnValue, caller);
            }
            else
            {
                // only run this code once for this function, that
                // will sort of 'define' what the return type should be
                ScoreboardManager sb = executor.scoreboard;
                ScoreboardValue clone = ScoreboardValue.AsReturnValue(value, caller);
                
                if (sb.temps.DefinedTemps.Add(clone.InternalName))
                {
                    sb.temps.DefinedTempsRecord.Add(clone.InternalName);
                    executor.AddCommandsInit(clone.CommandsDefine());
                }
                
                // register return type with executor
                executor.definedReturnedTypes.Add(value.type);

                commands = clone.Assign(value, caller);
                returnValue = clone;
            }
            
            executor.AddCommands(commands, "returnValue", $"Returns the objective '{returnValue.InternalName}'. Called in a return command located in {file.CommandReference} line {executor.NextLineNumber}");
        }
        /// <summary>
        /// Add commands and setup scoreboard to return a value.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="executor"></param>
        /// <param name="value"></param>
        /// <returns>A new scoreboard value that holds the returned value</returns>
        public void TryReturnValue(TokenLiteral value, Statement caller, Executor executor)
        {
            IEnumerable<string> commands;
            
            if (returnValue != null)
            {
                commands = returnValue.AssignLiteral(value, caller);
            }
            else
            {
                ScoreboardManager sb = executor.scoreboard;
                ScoreboardValue variable = ScoreboardValue.AsReturnValue(value, sb, caller);

                if (sb.temps.DefinedTemps.Add(variable.InternalName))
                {
                    sb.temps.DefinedTempsRecord.Add(variable.InternalName);
                    executor.AddCommandsInit(variable.CommandsDefine());
                }

                commands = variable.AssignLiteral(value, caller);
                returnValue = variable;
            }
            
            executor.AddCommands(commands, "returnValue", $"Returns the literal '{value}'. Called in a return command located in {file.CommandReference} line {executor.NextLineNumber}");
        }

        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            // add the file to the executor if it hasn't been yet.
            if(!isAddedToExecutor && !isExtern)
            {
                executor.AddExtraFile(file);
                isAddedToExecutor = true;
            }

            commandBuffer.Add(Command.Function(file));

            // apply attributes
            foreach (IAttribute attribute in this.attributes)
                attribute.OnCalledFunction(this, commandBuffer, executor, statement);
            
            // if there's nothing to 'return', send back a null literal.
            if (returnValue == null) 
                return new TokenNullLiteral(statement.Lines[0]);

            // sets a temp to the return value.
            ScoreboardValue returnHolder = executor.scoreboard.temps.RequestCopy(returnValue, true);
            commandBuffer.AddRange(returnHolder.Assign(returnValue, statement));
            return new TokenIdentifierValue(returnHolder.Name, returnHolder, statement.Lines[0]);
        }

        public void AddCommand(string command) =>
            file.Add(command);
        public void AddCommandTop(string command) =>
            file.AddTop(command);
        public void AddCommands(IEnumerable<string> commands) =>
            file.Add(commands);
        public void AddCommandsTop(IEnumerable<string> commands) =>
            file.AddTop(commands);

        public override int GetHashCode()
        {
            return (this.name ?? this.aliasedName).GetHashCode();
        }
    }
}
