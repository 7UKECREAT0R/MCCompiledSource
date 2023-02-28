using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Shorthand for <see cref="ScoreboardValue.Name"/> on <see cref="runtimeDestination"/>
        /// </summary>
        public readonly string objectiveName;

        public RuntimeFunctionParameter(ScoreboardValue value, Token defaultValue = null) : base(value.AliasName, defaultValue)
        {
            this.runtimeDestination = value;
            this.objectiveName = value.Name;
        }

        public override ParameterFit CheckInput(Token token)
        {
            if(token is TokenLiteral literal)
            {
                var sbType = literal.GetScoreboardValueType();
                var sbDestType = this.runtimeDestination.valueType;

                if (sbType == ScoreboardManager.ValueType.INVALID)
                    return ParameterFit.No;
                if (sbType == sbDestType)
                {
                    // return WithSubConversion if the decimal precision doesn't match.
                    if(this.runtimeDestination is ScoreboardValueDecimal @decimal)
                    {
                        int sbPrecision = (literal as TokenDecimalLiteral).number.GetPrecision();
                        if (@decimal.precision == sbPrecision)
                            return ParameterFit.Yes;

                        return ParameterFit.WithSubConversion;
                    }

                    return ParameterFit.Yes;
                }

                return ParameterFit.WithConversion;
            }

            if (token is TokenIdentifierValue _value)
            {
                var valueType = _value.value.valueType;
                var valueDestType = this.runtimeDestination.valueType;

                if (valueType == ScoreboardManager.ValueType.INVALID)
                    return ParameterFit.No;
                if (valueType == valueDestType)
                {
                    // return WithSubConversion if the decimal precision doesn't match. 
                    if (this.runtimeDestination is ScoreboardValueDecimal @decimal)
                    {
                        int sourcePrecision = (_value.value as ScoreboardValueDecimal).precision;
                        if (@decimal.precision == sourcePrecision)
                            return ParameterFit.Yes;

                        return ParameterFit.WithSubConversion;
                    }

                    return ParameterFit.Yes;
                }

                return ParameterFit.WithConversion;
            }

            return ParameterFit.No;
        }

        public override void SetParameter(Token token, List<string> commandBuffer, Executor executor, Statement callingStatement)
        {
            if (token is TokenLiteral literal)
            {
                var type = literal.GetScoreboardValueType();
                if (type != ScoreboardManager.ValueType.INVALID)
                {
                    string accessor = this.runtimeDestination.AliasName;
                    string selector = this.runtimeDestination.clarifier.CurrentString;
                    string[] commands = this.runtimeDestination
                        .CommandsSetLiteral(selector, literal);
                    commandBuffer.AddRange(commands);
                    return;
                }
            }
            else if (token is TokenIdentifierValue _value)
            {
                ScoreboardValue value = _value.value;
                string selector = this.runtimeDestination.clarifier.CurrentString;
                string thisAccessor = this.runtimeDestination.AliasName;
                string thatAccessor = value.AliasName;
                string[] commands = this.runtimeDestination
                    .CommandsSet(selector, value);
                commandBuffer.AddRange(commands);
                return;
            }

            throw new StatementException(callingStatement, "Invalid parameter input. Developers: please use CheckInput(...)");
        }

        public override string ToString()
        {
            string type = this.runtimeDestination.GetTypeKeyword();
            return $"[{type} {name}]";
        }
    }
}
