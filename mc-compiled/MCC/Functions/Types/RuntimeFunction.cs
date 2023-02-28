﻿using mc_compiled.Commands.Selectors;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mc_compiled.MCC.Attributes;
using System.Security.Cryptography;

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

        public ScoreboardManager.ValueType returnValueType;
        public ScoreboardValue returnValue;

        public readonly bool isCompilerGenerated;
        readonly List<IAttribute> attributes;
        readonly List<RuntimeFunctionParameter> parameters;
        
        public string name;                 // name used internally if the normal name won't work.
        public readonly string aliasedName; // user-facing name (keyword)
        bool _hasSignaled = false;

        public RuntimeFunction(string name, IAttribute[] attributes, bool isCompilerGenerated = false)
        {
            this.isAddedToExecutor = false;

            this.aliasedName = name;
            this.name = name;
            this.isCompilerGenerated = isCompilerGenerated;

            this.file = new CommandFile(name, null, this);
            this.returnValue = null;
            this.returnValueType = ScoreboardManager.ValueType.INVALID;

            if (attributes == null)
                attributes = new IAttribute[0];

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

            foreach (var attribute in attributes)
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
        /// Require this functions's internal name to be hashed and hidden behind an alias.
        /// </summary>
        /// <returns>This object for chaining.</returns>
        public RuntimeFunction ForceHash()
        {
            this.name = ScoreboardValue.StandardizedHash(this.name);
            return this;
        }

        public override string Keyword => aliasedName;
        public override string Returns => returnValue?.GetTypeKeyword();
        public override FunctionParameter[] Parameters => this.parameters.ToArray();
        public override int ParameterCount => this.parameters.Count;
        public override string[] Aliases => null;
        public override int Importance => 0; // least important. always call runtime functions at last resort.
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
                // check if types match
                if (value.valueType != returnValueType)
                    throw new StatementException(caller, $"All return statements in this function must return the same type. Return type: {returnValueType}");
            }

            //   only run this code once for this function, that
            // will sort of 'define' what the return type should be
            ScoreboardManager sb = executor.scoreboard;
            ScoreboardValue clone = ScoreboardValue.AsReturnValue(value);
            if (sb.definedTempVars.Add(clone))
                executor.AddCommandsHead(clone.CommandsDefine());

            AddCommands(clone.CommandsSet(selector, value));
            returnValue = clone;
            returnValueType = clone.valueType;
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
                if(value.GetScoreboardValueType() != returnValueType)
                    throw new StatementException(caller, $"All return statements in this function must return the same type. Required: {returnValueType}");

                return;
            }

            ScoreboardManager sb = executor.scoreboard;
            ScoreboardValue variable = ScoreboardValue.AsReturnValue(value, sb, caller);

            if (sb.definedTempVars.Add(variable))
                executor.AddCommandsHead(variable.CommandsDefine());

            AddCommands(variable.CommandsSetLiteral(selector, value));
            returnValue = variable;
            returnValueType = variable.valueType;
        }

        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            // add the file to the executor if it hasn't been yet.
            if(!isAddedToExecutor)
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
                return new TokenNullLiteral(statement.Line);

            // sets a temp to the return value.
            ScoreboardValue returnHolder = executor.scoreboard.RequestTemp(returnValue);
            commandBuffer.AddRange(returnHolder.CommandsSet(executor.ActiveSelectorStr, returnValue));
            return new TokenIdentifierValue(returnHolder.AliasName, returnHolder, statement.Line);
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
