using mc_compiled.Commands.Selectors;
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
    /// Represents a function that is compressed to a <see cref="IAttribute"/>
    /// </summary>
    public class AttributeFunction : Function
    {
        readonly List<CompiletimeFunctionParameter> parameters;

        //          ARG1                            ARG2      ARG3       RETURN
        public Func<CompiletimeFunctionParameter[], Executor, Statement, IAttribute> callAction;

        public readonly string aliasedName; // user-facing name (keyword)
        public readonly string name;        // name used internally.

        public AttributeFunction(string aliasedName, string name)
        {
            this.aliasedName = aliasedName;
            this.name = name;

            this.parameters = new List<CompiletimeFunctionParameter>();
        }
        /// <summary>
        /// Returns this function with a given call action.
        /// The input delegate takes four parameters and returns a <see cref="IAttribute"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="CompiletimeFunctionParameter"/>[] - The parameters passed into this function. <br />
        /// <see cref="Executor"/> - The executor running this action. <br />
        /// <see cref="Statement"/> - The statement running this action.
        /// </remarks>
        /// <param name="callAction">The call action to set. See above for its parameters.</param>
        /// <returns>This object for chaining.</returns>
        public AttributeFunction WithCallAction(Func<
            CompiletimeFunctionParameter[],
            Executor,
            Statement,
            IAttribute> callAction)
        {
            this.callAction = callAction;
            return this;
        }
        /// <summary>
        /// Adds a compile-time parameter to this attribute function.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>This object for chaining.</returns>
        public AttributeFunction AddParameter(CompiletimeFunctionParameter parameter)
        {
            this.parameters.Add(parameter);
            return this;
        }
        /// <summary>
        /// Adds multiple compile-time parameters to this attribute function.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>This object for chaining.</returns>
        public AttributeFunction AddParameters(IEnumerable<CompiletimeFunctionParameter> parameters)
        {
            this.parameters.AddRange(parameters);
            return this;
        }

        public override string Keyword => aliasedName;
        public override string Returns => "attribute";
        public override FunctionParameter[] Parameters => this.parameters.ToArray();
        public override int ParameterCount => this.parameters.Count;
        public override string[] Aliases => null;
        public override int Importance => 2; // most important.
        public override bool ImplicitCall => this.parameters.Count == 0;

        public override bool MatchParameters(Token[] inputs, out string error, out int score)
        {
            if (this.callAction == null)
            {
                error = $"Function \"{name}\" has no call action bound. This is a bug with the compiler.";
                score = 0;
                return false;
            }

            return base.MatchParameters(inputs, out error, out score);
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            if (this.callAction == null)
                throw new StatementException(statement, $"Function \"{name}\" has no call action bound. This is a bug with the compiler.");

            IAttribute constructed = callAction(parameters.ToArray(), executor, statement);
            return new TokenAttribute(constructed, statement.Line);
        }
    }
}
