using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Functions.Types
{
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

        public override bool CheckInput(Token token, Statement callingStatement)
        {
            if(token is TokenLiteral literal)
            {
                // throws if it can't fit in a scoreboard value.
                literal.GetScoreboardValueType(callingStatement);
                return true;
            }

            if (token is TokenIdentifierValue _value)
                return true;

            return false;
        }

        public override void SetParameter(Token token, Executor executor, Statement callingStatement)
        {
            if (token is TokenLiteral literal)
            {
                // throws if it can't fit in a scoreboard value.
                literal.GetScoreboardValueType(callingStatement);
                return true;
            }

            if (token is TokenIdentifierValue _value)
            {
                ScoreboardValue value = _value.value;
            }

            throw new StatementException
        }

        public override string ToString()
        {
            string type = this.runtimeDestination.GetTypeKeyword();
            return '[' + type + ' ' + this.name + ']';
        }
    }
}
