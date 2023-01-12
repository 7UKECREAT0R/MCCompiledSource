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

        public RuntimeFunctionParameter(ScoreboardValue value, Token defaultValue) : base(value.AliasName, defaultValue)
        {
            this.runtimeDestination = value;
            this.objectiveName = value.Name;
        }

        public override bool CheckInput(Token token)
        {
            if(token is TokenLiteral literal)
            {
                var type = literal.GetScoreboardValueType();
                return type != ScoreboardManager.ValueType.INVALID;
            }

            if (token is TokenIdentifierValue _value)
                return true; // always can fit somehow

            return false;
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
                        .CommandsSetLiteral(accessor, selector, literal);
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
                    .CommandsSet(selector, value, thisAccessor, thatAccessor);
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
