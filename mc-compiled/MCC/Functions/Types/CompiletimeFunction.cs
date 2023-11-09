using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// Represents a function that can be called and fully evaluated at compile-time.
    /// </summary>
    public class CompiletimeFunction : Function
    {
        readonly List<CompiletimeFunctionParameter> parameters;

        //          ARG1                            ARG2      ARG3       RETURN
        public Func<CompiletimeFunctionParameter[], Executor, Statement, Token> callAction;

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
        /// Returns this function with a given call action.
        /// The input delegate takes four parameters and returns a <see cref="Token"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="CompiletimeFunctionParameter"/>[] - The parameters passed into this function. <br />
        /// <see cref="Executor"/> - The executor running this action. <br />
        /// <see cref="Statement"/> - The statement running this action.
        /// </remarks>
        /// <param name="callAction">The call action to set. See above for its parameters.</param>
        /// <returns>This object for chaining.</returns>
        public CompiletimeFunction WithCallAction(Func<
            CompiletimeFunctionParameter[],
            Executor,
            Statement,
            Token> callAction)
        {
            this.callAction = callAction;
            return this;
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
        public override string Returns => "Compiler-Defined: " + returnType;
        public override string Documentation => documentation;
        public override FunctionParameter[] Parameters => this.parameters.ToArray();
        public override int ParameterCount => this.parameters.Count;
        public override string[] Aliases => null;
        public override int Importance => 1; // more important. tries to run compile-time stuff over runtime stuff.
        public override bool ImplicitCall => false;

        public override bool MatchParameters(Token[] inputs, out string error, out int score)
        {
            if (this.callAction == null)
            {
                error = $"Function \"{name}\" has no call action bound. This is a bug with the compiler.";
                score = -999;
                return false;
            }

            return base.MatchParameters(inputs, out error, out score);
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            if (this.callAction == null)
                throw new StatementException(statement, $"Function \"{name}\" has no call action bound. This is a bug with the compiler.");

            // call the delegate
            return callAction(parameters.ToArray(), executor, statement);
        }
    }
}
