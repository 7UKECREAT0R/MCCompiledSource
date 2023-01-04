using mc_compiled.Commands.Selectors;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// The additional aliases that can be used to invoke this function.
        /// This property may return null in the case that this function doesn't support aliases.
        /// </summary>
        public abstract string[] Aliases { get; }
        /// <summary>
        /// The pattern that must be matched by <see cref="MatchParameters(Token[])"/> in order to call this function.
        /// </summary>
        protected abstract FunctionParameter[] Parameters { get; }
        /// <summary>
        /// Get the number of parameters in this function.
        /// </summary>
        public abstract int ParameterCount { get; }

        /// <summary>
        /// Returns if this function can be called without any parameters or parenthesis, just the keyword.
        /// </summary>
        public abstract bool ImplicitCall { get; }

        /// <summary>
        /// Attempts to match this set of inputs against this function's parameter pattern.
        /// Accounts for optional parameters.
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public bool MatchParameters(Token[] inputs)
        {
            int maxLength = inputs.Length;
            int sourceIndex = 0;
            int parameterIndex = 0;

            FunctionParameter[] parameters = this.Parameters;
            int minimumLength = parameters.Count(p => !p.optional);

            if(inputs.Length < minimumLength)
                return false;

            for (; parameterIndex < maxLength; parameterIndex++)
            {
                Token source = inputs[sourceIndex];
                FunctionParameter parameter = parameters[parameterIndex];

                if(parameter.CheckInput(source))
                {
                    sourceIndex++;
                    if (sourceIndex >= maxLength)
                        return true; // matched
                    continue;
                } else
                {
                    if (!parameter.optional)
                        return false; // failed match

                    // continue to next loop since that parameter was optional.
                    continue;
                }
            }

            return false;
        }

        /// <summary>
        /// Call this function.
        /// </summary>
        /// <param name="inputs">The tokens to pass into the function.</param>
        /// <param name="executor">The executing environment.</param>
        /// <param name="statement">The statement that is calling this function.</param>
        /// <returns>The token to replace the function during squashing. Return null to completely remove the token.</returns>
        public abstract Token CallFunction(Token[] inputs, Executor executor, Statement statement);
        
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
