using mc_compiled.MCC.Compiler;
using System.Collections.Generic;
using mc_compiled.MCC.Compiler.TypeSystem;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// A function parameter that points to a runtime value.
    /// Setting this parameter's value will result in commands being added to set <see cref="runtimeDestination"/>.
    /// </summary>
    public class RuntimeFunctionParameter : FunctionParameter
    {
        /// <summary>
        /// The destination variable that the value given will arrive at.
        /// </summary>
        public readonly ScoreboardValue runtimeDestination;
        /// <summary>
        /// The actual name of the objective that will store the parameter.
        /// Shorthand for <see cref="ScoreboardValue.InternalName"/> on <see cref="runtimeDestination"/>
        /// </summary>
        public readonly string objectiveName;

        public RuntimeFunctionParameter(ScoreboardValue value, Token defaultValue = null) : base(value.Name, defaultValue)
        {
            this.runtimeDestination = value;
            this.objectiveName = value.InternalName;
        }

        public override ParameterFit CheckInput(Token token)
        {
            switch (token)
            {
                case TokenLiteral literal:
                {
                    Typedef sbType = literal.GetTypedef();
                    Typedef sbDestType = this.runtimeDestination.type;

                    if (sbType == null)
                        return ParameterFit.No;
                    if (sbType != sbDestType)
                        return ParameterFit.WithConversion;
                    
                    return ParameterFit.Yes;

                }
                case TokenIdentifierValue _value:
                {
                    Typedef valueType = _value.value.type;
                    Typedef valueDestType = this.runtimeDestination.type;

                    if (valueType == null)
                        return ParameterFit.No;

                    if (!valueType.NeedsToBeConvertedTo(_value.value, this.runtimeDestination))
                        return ParameterFit.Yes;
                    
                    if (valueType.TypeEnum == valueDestType.TypeEnum)
                        return ParameterFit.WithSubConversion;
                    
                    if (valueType.CanConvertTo(valueDestType))
                        return ParameterFit.WithConversion;
                    
                    return ParameterFit.No;

                }
                default:
                    return ParameterFit.No;
            }
        }

        public override void SetParameter(Token token, List<string> commandBuffer, Executor executor, Statement callingStatement)
        {
            switch (token)
            {
                case TokenLiteral literal:
                {
                    Typedef type = literal.GetTypedef();
                    if (type != null)
                    {
                        IEnumerable<string> commands = this.runtimeDestination.AssignLiteral(literal, callingStatement);
                        commandBuffer.AddRange(commands);
                        return;
                    }

                    break;
                }
                case TokenIdentifierValue _value:
                {
                    ScoreboardValue value = _value.value;
                    IEnumerable<string> commands = this.runtimeDestination.Assign(value, callingStatement);
                    commandBuffer.AddRange(commands);
                    return;
                }
            }

            throw new StatementException(callingStatement, "Invalid parameter input. Developers: please use CheckInput(...)");
        }

        public override string ToString()
        {
            string type = this.runtimeDestination.type.TypeKeyword;
            return $"[{type} {name}]";
        }
    }
}
