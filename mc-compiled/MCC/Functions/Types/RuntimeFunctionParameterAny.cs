using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Compiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// A runtime function parameter that can accept any type. The parameter's current value will be held in the `RuntimeDestination` property.
    /// </summary>
    internal class RuntimeFunctionParameterAny : RuntimeFunctionParameter
    {
        private readonly GenerativeFunction parent;
        private readonly string originalName;
        public readonly string aliasName;   // The name shown to the user
        

        /// <summary>
        /// A runtime function parameter that can accept any type. The parameter's current value will be held in the `RuntimeDestination` property.
        /// </summary>
        public RuntimeFunctionParameterAny(GenerativeFunction parent, string aliasName, string name, Token defaultValue = null) : base(name, defaultValue)
        {
            this.parent = parent;
            this.originalName = name;
            this.aliasName = aliasName;
        }
        private void SetNameSuffix(ScoreboardValue hash)
        {
            string suffix = (parent.name.GetHashCode() ^ hash.GetHashCode()).ToString().Replace('-', '_');
            this.name = this.originalName + suffix;
        }

        public override ParameterFit CheckInput(Token token)
        {
            switch (token)
            {
                case TokenLiteral literal:
                    Typedef sbType = literal.GetTypedef();
                    if (sbType == null)
                        return ParameterFit.No;

                    return ParameterFit.Yes;
                case TokenIdentifierValue _value:
                    return ParameterFit.Yes;
                default:
                    return ParameterFit.No;
            }
        }
        public override void SetParameter(Token token, List<string> commandBuffer, Executor executor, Statement callingStatement)
        {
            ScoreboardValue value;

            switch(token)
            {
                case TokenLiteral literal:
                    Typedef type = literal.GetTypedef();

                    if (type == null)
                        throw new StatementException(callingStatement, "Invalid parameter input (literal). Developers: please use CheckInput(...)");

                    value = new ScoreboardValue(name, true, type, executor.scoreboard);
                    commandBuffer.AddRange(value.AssignLiteral(literal, callingStatement));
                    break;
                case TokenIdentifierValue _value:
                    value = _value.value.Clone(callingStatement,
                        newInternalName: name,
                        newName: name);
                    commandBuffer.AddRange(value.Assign(_value.value, callingStatement));
                    break;
                default:
                    throw new StatementException(callingStatement, "Invalid parameter input. Developers: please use CheckInput(...)");
            }

            // value now holds the destination variable
            this.RuntimeDestination = value;

            // set name to unique name for this input type
            SetNameSuffix(value);

            // register the value in the init function
            executor.AddCommandsInit(value.CommandsInit());
        }

        public override string ToString()
        {
            return $"[any {aliasName}]";
        }
        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }
    }
}
