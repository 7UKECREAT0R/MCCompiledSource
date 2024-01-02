using mc_compiled.MCC.Compiler;
using System.Collections.Generic;

namespace mc_compiled.MCC.Functions
{
    /// <summary>
    /// A single parameter definition in a function. Used more as a model than actual storage of the tokens.
    /// </summary>
    public abstract class FunctionParameter
    {
        public string name;                 // the name of this parameter
        public readonly bool optional;      // if this parameter can be skipped.
        public readonly Token defaultValue;

        public FunctionParameter(string name, Token defaultValue)
        {
            this.name = name;
            this.optional = defaultValue != null;
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// Checks how the input token would fit into this parameter. This method should not ever throw.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>A <see cref="ParameterFit"/> identifying how the token fits.</returns>
        public abstract ParameterFit CheckInput(Token token);
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

            if (CheckInput(defaultValue) != ParameterFit.No)
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
    public enum ParameterFit
    {
        /// <summary>
        /// Object cannot fit into the parameter.
        /// </summary>
        No,
        /// <summary>
        /// Object can fit into the parameter with a conversion.
        /// </summary>
        WithConversion,
        /// <summary>
        /// Object fits into the parameter natively, but will require a sub-conversion of that type.
        /// </summary>
        WithSubConversion,
        /// <summary>
        /// Object can fit into the parameter natively.
        /// </summary>
        Yes
    }
}
