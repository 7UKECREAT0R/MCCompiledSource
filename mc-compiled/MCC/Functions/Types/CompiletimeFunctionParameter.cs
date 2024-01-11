using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// A compile-time parameter that can be just about anything.
    /// Setting this parameter's value will set <see cref="CurrentValue"/>.
    /// </summary>
    public abstract class CompiletimeFunctionParameter : FunctionParameter
    {
        /// <summary>
        /// The current value held in this parameter.
        /// </summary>
        public abstract Token CurrentValue { get; protected set; }
        protected CompiletimeFunctionParameter(string name, Token defaultValue) : base(name, defaultValue) { }

        /// <summary>
        /// Returns the name of the type that is required by this compile-time parameter.
        /// </summary>
        /// <returns></returns>
        public abstract string GetRequiredTypeName();
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
        public override Token CurrentValue { get; protected set; }

        public CompiletimeFunctionParameter(string name, T defaultValue = null) : base(name, defaultValue)
        {
            this.CurrentValue = null;
        }

        public override ParameterFit CheckInput(Token token)
        {
            if (token is T)
                return ParameterFit.Yes;

            if(token is IImplicitToken conversion)
            {
                Type sourceType = token.GetType();
                Type[] types = conversion.GetImplicitTypes();

                if (types.Any(type => sourceType.IsAssignableFrom(type)))
                    return ParameterFit.WithConversion;
            }

            return ParameterFit.No;
        }

        public override void SetParameter(Token token, List<string> commandBuffer, Executor executor, Statement callingStatement)
        {
            if (!(token is T))
            {
                if (!(token is IImplicitToken conversion))
                    throw new StatementException(callingStatement, "Invalid parameter input. Developers: please use CheckInput(...)");
                
                Type[] types = conversion.GetImplicitTypes();

                for(int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];

                    if (!typeof(T).IsAssignableFrom(type))
                        continue;

                    var convertedCasted = conversion.Convert(executor, i) as T;
                    CurrentValue = convertedCasted;
                }

                throw new StatementException(callingStatement, "Invalid parameter input. Developers: please use CheckInput(...)");
            }

            T casted = token as T;
            CurrentValue = casted;
            return;
        }

        public override string GetRequiredTypeName()
        {
            return nameof(T);
        }
        public override string ToString()
        {
            string type = nameof(T);
            return '[' + type + ' ' + this.name + ']';
        }
    }
}
