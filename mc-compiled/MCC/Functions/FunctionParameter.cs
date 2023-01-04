using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Functions
{
    /// <summary>
    /// A single parameter definition in a function. Used more as a model than actual storage of the tokens.
    /// </summary>
    public abstract class FunctionParameter
    {
        public readonly string name;        // the name of this parameter
        public readonly bool optional;      // if this parameter can be skipped.
        public readonly Token defaultValue;

        public FunctionParameter(string name, Token defaultValue)
        {
            this.name = name;
            this.optional = defaultValue != null;
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// Checks if the input token would fit into this parameter.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <param name="callingStatement">The statement calling this method.</param>
        /// <returns>If the token will fit into this parameter.</returns>
        public abstract bool CheckInput(Token token, Statement callingStatement);
        /// <summary>
        /// Performs the actions necessary to set the parameter.
        /// </summary>
        /// <param name="token">The token to set this parameter to.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="callingStatement">The statement calling this method.</param>
        public abstract void SetParameter(Token token, Executor executor, Statement callingStatement);

        /// <summary>
        /// Returns essentially the name and type of the parameter, enclosed in [brackets].
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return '[' + this.name + ']';
        }
    }
}
