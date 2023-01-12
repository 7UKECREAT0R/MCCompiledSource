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
        /// Checks if the input token would fit into this parameter. This method should not ever throw, only return false.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>If the token will fit into this parameter.</returns>
        public abstract bool CheckInput(Token token);
        /// <summary>
        /// Performs the actions necessary to set the parameter.
        /// </summary>
        /// <param name="token">The token to set this parameter to.</param>
        /// <param name="commandBuffer">The list of commands that will be automatically added as part of the function call.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="callingStatement">The statement calling this method.</param>
        public abstract void SetParameter(Token token, List<string> commandBuffer, Executor executor, Statement callingStatement);

        /// <summary>
        /// Performs the actions necessary to set the parameter to its default value. Implemented at the root.
        /// <param name="commandBuffer">The list of commands that will be automatically added as part of the function call.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="callingStatement">The statement calling this method.</param>
        /// </summary>
        public void SetParameterDefault(List<string> commandBuffer, Executor executor, Statement callingStatement)
        {
            if (defaultValue == null)
                throw new StatementException(callingStatement, $"Function parameter \"{name}\" does not have a value to default to.");

            if (CheckInput(defaultValue))
                SetParameter(defaultValue, commandBuffer, executor, callingStatement);
            else
                throw new StatementException(callingStatement, $"Default value for function parameter \"{name}\" didn't fit.");
        }

        /// <summary>
        /// Returns essentially the name and type of the parameter, enclosed in [brackets].
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{name}]";
        }
    }
}
