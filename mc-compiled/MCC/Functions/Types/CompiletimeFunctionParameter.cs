using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// A compile-time parameter that can be just about anything.
    /// Setting this parameter's value will set <see cref="CurrentValue"/>.
    /// </summary>
    public abstract class CompiletimeFunctionParameter : FunctionParameter
    {
        protected CompiletimeFunctionParameter(string name, Token defaultValue) : base(name, defaultValue) { }
    }

    /// <summary>
    /// A compile-time parameter that can be just about anything.
    /// Setting this parameter's value will set <see cref="CurrentValue"/>, but can only be done if the given literal falls under T.
    /// </summary>
    /// <typeparam name="T">The type the given literal must be to fit into this parameter.</typeparam>
    public class CompiletimeFunctionParameter<T> : CompiletimeFunctionParameter where T : Token
    {
        /// <summary>
        /// The current value held in this parameter.
        /// </summary>
        public T CurrentValue { get; private set; }

        public CompiletimeFunctionParameter(string name, T defaultValue) : base(name, defaultValue)
        {
            this.CurrentValue = null;
        }

        public override bool CheckInput(Token token)
        {
            if (token is T)
                return true;

            return false;
        }

        public override void SetParameter(Token token, List<string> commandBuffer, Executor executor, Statement callingStatement)
        {
            if (!(token is T))
                throw new StatementException(callingStatement, "Invalid parameter input. Developers: please use CheckInput(...)");

            T casted = token as T;
            CurrentValue = casted;
            return;
        }
        /// <summary>
        /// Same functionality as <see cref="SetParameter(Token, Executor, Statement)"/>, but skips type checking. This method doesn't throw exceptions.
        /// </summary>
        /// <param name="token"></param>
        public void SetParameterUnchecked(Token token)
        {
            T casted = token as T;
            CurrentValue = casted;
            return;
        }

        public override string ToString()
        {
            string type = nameof(T);
            return '[' + type + ' ' + this.name + ']';
        }
    }
}
