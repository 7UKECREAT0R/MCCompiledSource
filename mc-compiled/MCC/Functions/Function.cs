using mc_compiled.MCC.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.Functions
{
    /// <summary>
    /// Represents a function that can be called, squashing its result into a token.
    /// </summary>
    public abstract class Function
    {
        /// <summary>
        /// The keyword used to invoke this function.
        /// </summary>
        public abstract string Keyword { get; }
        /// <summary>
        /// Text describing what this function returns. Allowed to return null.
        /// </summary>
        public abstract string Returns { get; }
        /// <summary>
        /// Documentation about the function. Allowed to return null.
        /// </summary>
        public abstract string Documentation { get; }
        /// <summary>
        /// The additional aliases that can be used to invoke this function.
        /// This property may return null in the case that this function doesn't support aliases.
        /// </summary>
        public abstract string[] Aliases { get; }
        /// <summary>
        /// The pattern that must be matched by <see cref="MatchParameters(Token[])"/> in order to call this function.
        /// </summary>
        public abstract FunctionParameter[] Parameters { get; }
        /// <summary>
        /// Get the number of parameters in this function.
        /// </summary>
        public abstract int ParameterCount { get; }

        /// <summary>
        /// The importance of this function. Functions are sorted and checked from highest-importance-first.
        /// </summary>
        public abstract int Importance { get; }
        /// <summary>
        /// Returns if this function can be called without any parameters or parenthesis, just the keyword.
        /// </summary>
        public abstract bool ImplicitCall { get; }

        /// <summary>
        /// Attempts to match this set of inputs against this function's parameter pattern.
        /// Accounts for optional parameters.
        /// </summary>
        /// <param name="inputs">The inputs to check this function against.</param>
        /// <param name="error">If false is returned, this is the error string to give the user. Otherwise, null.</param>
        /// <param name="score">If true is returned, this is the score the match received. </param>
        /// <returns></returns>
        public virtual bool MatchParameters(Token[] inputs, out string error, out int score)
        {
            int inputLength = inputs.Length;

            FunctionParameter[] parameters = this.Parameters;
            int maximumParameters = parameters.Length;
            int minimumParameters = parameters.Count(p => !p.optional);

            if (inputLength == 0 && minimumParameters == 0)
            {
                // zero-parameter function, no processing is needed.
                error = null;
                score = -999;
                return true;
            }
            if (inputLength < minimumParameters)
            {
                // not enough parameters to satisfy even the minimum.
                FunctionParameter missingParameter = parameters[inputLength];
                error = $"Missing parameter \"{missingParameter.name}\"";
                score = -999;
                return false;
            }

            if (inputLength > maximumParameters)
                inputLength = maximumParameters;

            score = 0;

            for (int i = 0; i < inputLength; i++)
            {
                Token source = inputs[i];
                FunctionParameter parameter = parameters[i];

                ParameterFit fit = parameter.CheckInput(source);

                switch (fit)
                {
                    case ParameterFit.No:
                        // failed match.
                        error = $"Couldn't accept input \"{source.ToString()}\" for parameter \"{parameter.name}\"";
                        score = -999;
                        return false;
                    case ParameterFit.WithConversion:
                        score += 1;
                        break;
                    case ParameterFit.WithSubConversion:
                        score += 2;
                        break;
                    case ParameterFit.Yes:
                        score += 3;
                        break;
                    default:
                        break;
                }
            }

            error = null;
            return true; 
        }

        /// <summary>
        /// Load the given tokens into this function's parameters.
        /// </summary>
        /// <param name="inputs">The input parameters that have been checked by <see cref="MatchParameters"/></param>
        /// <param name="commandBuffer">The commands that will be added after this function call is completed in full.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="callingStatement">The statement calling this method.</param>
        public void ProcessParameters(Token[] inputs, List<string> commandBuffer, Executor executor, Statement callingStatement)
        {
            ScoreboardManager sb = executor.scoreboard;
            FunctionParameter[] parameters = this.Parameters;

            using (sb.temps.PushTempState())
            {
                int parameterCount = parameters.Length;
                int inputsCount = inputs.Length;

                for (int i = 0; i < parameterCount; i++)
                {
                    FunctionParameter parameter = parameters[i];
                    Token input; // this will either be a given token, or default if it ran out of those.

                    // out of inputs to pull, start using defaults
                    if (i >= inputsCount)
                    {   
                        if (parameter.optional)
                            input = parameter.defaultValue;
                        else
                            throw new StatementException(callingStatement, $"Missing parameter '{parameter}' in '{Keyword}' call.");
                    }
                    else
                        input = inputs[i];

                    if(parameter.CheckInput(input) == ParameterFit.No)
                        throw new StatementException(callingStatement, $"Couldn't accept input \"{input}\" for parameter '{parameter.name}'");

                    parameter.SetParameter(input, commandBuffer, executor, callingStatement);
                }
            }
        }

        /// <summary>
        /// Call this function after the inputs have been processed by <see cref="ProcessParameters"/>.
        /// </summary>
        /// <param name="commandBuffer">The commands that will be added after this function call is completed in full.</param>
        /// <param name="executor">The executing environment.</param>
        /// <param name="statement">The statement that is calling this function.</param>
        /// <returns>The token to replace the function during squashing. Return null to completely remove the token.</returns>
        public abstract Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement);
        
        /// <summary>
        /// Gets the hash of this function's Keyword.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Keyword.GetHashCode();
        }
    }
}
