using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// Represents a function that can be called and fully evaluated at compile-time.
    /// </summary>
    public abstract class CompiletimeFunction : Function
    {
        readonly List<CompiletimeFunctionParameter> parameters;

        public readonly string aliasedName; // user-facing name (keyword)
        public readonly string name;        // name used internally.
        public readonly string returnType;  // the type-keyword of the value this returns, or null.
        public readonly string documentation;

        /// <summary>
        /// Creates a new compile-time function.
        /// </summary>
        /// <param name="aliasedName">The user-facing name of the function.</param>
        /// <param name="name">The internal name of the function.</param>
        /// <param name="returnType">The type-keyword of the value this returns, or null.</param>
        public CompiletimeFunction(string aliasedName, string name, string returnType, string documentation)
        {
            this.aliasedName = aliasedName;
            this.name = name;
            this.returnType = returnType;
            this.documentation = documentation;
            this.parameters = new List<CompiletimeFunctionParameter>();
        }
        /// <summary>
        /// Adds a compile-time parameter to this function.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>This object for chaining.</returns>
        public CompiletimeFunction AddParameter(CompiletimeFunctionParameter parameter)
        {
            this.parameters.Add(parameter);
            return this;
        }
        /// <summary>
        /// Adds multiple compile-time parameters to this function.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>This object for chaining.</returns>
        public CompiletimeFunction AddParameters(IEnumerable<CompiletimeFunctionParameter> parameters)
        {
            this.parameters.AddRange(parameters);
            return this;
        }
        /// <summary>
        /// Adds multiple compile-time parameters to this function.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>This object for chaining.</returns>
        public CompiletimeFunction AddParameters(params CompiletimeFunctionParameter[] parameters)
        {
            this.parameters.AddRange(parameters);
            return this;
        }

        public override string Keyword => aliasedName;
        public override string Returns => returnType;
        public override string Documentation => documentation;
        public override FunctionParameter[] Parameters => this.parameters.ToArray();
        public override int ParameterCount => this.parameters.Count;
        public override string[] Aliases => null;
        public override int Importance => 2; // most important. always prefer compile-time.
        public override bool ImplicitCall => false;
        public override bool AdvertiseOverLSP => true;

        public override bool MatchParameters(Token[] inputs, out string error, out int score)
        {
            return base.MatchParameters(inputs, out error, out score);
        }
        public override int GetHashCode()
        {
            return (this.name ?? this.aliasedName).GetHashCode();
        }
    }
}
