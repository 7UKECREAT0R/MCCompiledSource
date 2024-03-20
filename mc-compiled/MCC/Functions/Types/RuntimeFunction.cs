using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler.Async;

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
        private readonly List<RuntimeFunctionParameter> parameters;
        protected readonly List<IAttribute> attributes;

        private readonly string name;   // user-facing name
        public bool isExtern;           // created outside of MCCompiled, parameter names are verbatim
        public string internalName;     // name used internally if the normal name won't work
        public string documentation;    // the documentation that should show under this function
        private bool _hasSignaled;

        /// <summary>
        /// The action to be run when this function's block opens.
        /// Overridden by <see cref="AsyncFunction"/> to change this behavior.
        /// </summary>
        public virtual Action<Executor> BlockOpenAction => e =>
        {
            e.PushFile(this.file);
        };
        /// <summary>
        /// The action to be run when this function's block closes.
        /// Overridden by <see cref="AsyncFunction"/> to change this behavior.
        /// </summary>
        public virtual Action<Executor> BlockCloseAction => e =>
        {
            e.PopFile();
        };
        
        public RuntimeFunction(Statement creationStatement, string name, string internalName, string documentation, IAttribute[] attributes, bool isCompilerGenerated = false)
        {
            this.creationStatement = creationStatement;
            this.isAddedToExecutor = false;

            this.name = name;
            this.internalName = internalName;
            this.documentation = documentation;
            this.isCompilerGenerated = isCompilerGenerated;

            this.file = new CommandFile(false, internalName, null, this);
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
            if (this._hasSignaled)
                throw new Exception($"Attempt to call SignalToAttributes again on function '{this.internalName}'");

            this._hasSignaled = true;

            foreach (IAttribute attribute in this.attributes)
                attribute.OnAddedFunction(this, callingStatement);
        }
        /// <summary>
        /// Checks if the <see cref="RuntimeFunction"/> has an attribute of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <returns>True if the <see cref="RuntimeFunction"/> has an attribute of type <typeparamref name="T"/>, otherwise false.</returns>
        public bool HasAttribute<T>() where T : IAttribute
        {
            return this.attributes.Any(a => a is T);
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
            this.internalName = ScoreboardValue.StandardizedHash(this.internalName);
            return this;
        }

        public override string Keyword => this.name;
        public override string Returns => this.returnValue?.GetExtendedTypeKeyword();
        public override string Documentation => this.documentation;
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
        public virtual void TryReturnValue(ScoreboardValue value, Executor executor, Statement caller)
        {
            IEnumerable<string> commands;
            if (this.returnValue != null && value.NeedsToBeConvertedFor(this.returnValue))
            {
                commands = value.CommandsConvert(this.returnValue, caller);
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
                this.returnValue = clone;
            }
            
            executor.AddCommands(commands, "returnValue", $"Returns the objective '{this.returnValue.InternalName}'. Called in a return command located in {this.file.CommandReference} line {executor.NextLineNumber}");
        }
        /// <summary>
        /// Add commands and setup scoreboard to return a value.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="executor"></param>
        /// <param name="value"></param>
        /// <returns>A new scoreboard value that holds the returned value</returns>
        public virtual void TryReturnValue(TokenLiteral value, Statement caller, Executor executor)
        {
            IEnumerable<string> commands;
            
            if (this.returnValue != null)
            {
                commands = this.returnValue.AssignLiteral(value, caller);
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
                this.returnValue = variable;
            }
            
            executor.AddCommands(commands, "returnValue", $"Returns the literal '{value}'. Called in a return command located in {this.file.CommandReference} line {executor.NextLineNumber}");
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            // add the file to the executor if it hasn't been yet.
            if(!this.isAddedToExecutor && !this.isExtern)
            {
                executor.AddExtraFile(this.file);
                this.isAddedToExecutor = true;
            }

            commandBuffer.Add(Command.Function(this.file));

            // apply attributes
            foreach (IAttribute attribute in this.attributes)
                attribute.OnCalledFunction(this, commandBuffer, executor, statement);
            
            // if there's nothing to 'return', send back a null literal.
            if (this.returnValue == null) 
                return new TokenNullLiteral(statement.Lines[0]);

            // sets a temp to the return value.
            ScoreboardValue returnHolder = executor.scoreboard.temps.RequestCopy(this.returnValue, true);
            commandBuffer.AddRange(returnHolder.Assign(this.returnValue, statement));
            return new TokenIdentifierValue(returnHolder.Name, returnHolder, statement.Lines[0]);
        }

        public void AddCommand(string command) => this.file.Add(command);
        public void AddCommandTop(string command) => this.file.AddTop(command);
        public void AddCommands(IEnumerable<string> commands) => this.file.Add(commands);
        public void AddCommandsTop(IEnumerable<string> commands) => this.file.AddTop(commands);

        public override int GetHashCode()
        {
            return (this.internalName ?? this.name).GetHashCode();
        }
    }
}
