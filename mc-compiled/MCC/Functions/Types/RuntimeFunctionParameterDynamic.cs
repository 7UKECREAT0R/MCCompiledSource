using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Compiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// A runtime function parameter that can accept any type that falls under a filter. The parameter's current value will be held in the `RuntimeDestination` property.
    /// </summary>
    internal class RuntimeFunctionParameterDynamic : RuntimeFunctionParameter
    {
        private readonly GenerativeFunction parent;
        private readonly string originalName;
        public readonly string aliasName;   // The name shown to the user

        private Typedef[] acceptedTypes;

        /// <summary>
        /// A runtime function parameter that can accept any type. The parameter's current value will be held in the `RuntimeDestination` property.
        /// </summary>
        public RuntimeFunctionParameterDynamic(GenerativeFunction parent, string aliasName, string name, Token defaultValue = null) : base(name, defaultValue)
        {
            this.parent = parent;
            this.originalName = name;
            this.aliasName = aliasName;
            this.acceptedTypes = null;
        }
        public RuntimeFunctionParameterDynamic WithAcceptedTypes(params Typedef[] types)
        {
            this.acceptedTypes = types;
            return this;
        }

        public override ParameterFit CheckInput(Token token)
        {
            switch (token)
            {
                case TokenLiteral literal:
                    Typedef sbType = literal.GetTypedef();
                    if (sbType == null)
                        return ParameterFit.No;
                    if(acceptedTypes != null && !acceptedTypes.Contains(sbType))
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
            
            switch (token)
            {
                case TokenLiteral literal:
                    {
                        Typedef type = literal.GetTypedef();
                        ITypeStructure data = null;

                        if (type == null)
                            throw new StatementException(callingStatement, "Invalid parameter input (literal). Developers: please use CheckInput(...)");
                        if (type.CanAcceptLiteralForData(literal))
                            data = type.AcceptLiteral(literal);

                        value = new ScoreboardValue(name, true, type, data, executor.scoreboard);
                        string suffix = (parent.name.GetHashCode() ^ value.GetHashCode()).ToString().Replace('-', '_');
                        value.InternalName = value.InternalName + suffix;

                        commandBuffer.AddRange(value.AssignLiteral(literal, callingStatement));
                        break;
                    }
                case TokenIdentifierValue _value:
                    {
                        value = _value.value.Clone(callingStatement,
                            newClarifier: Clarifier.Global(),
                            newInternalName: name,
                            newName: name);
                        string suffix = (parent.name.GetHashCode() ^ value.GetHashCode()).ToString().Replace('-', '_');
                        value.InternalName = value.InternalName + suffix;

                        commandBuffer.AddRange(value.Assign(_value.value, callingStatement));
                        break;
                    }
                default:
                    throw new StatementException(callingStatement, "Invalid parameter input. Developers: please use CheckInput(...)");
            }

            // value now holds the destination variable
            this.RuntimeDestination = value;

            // register the value in the init function
            executor.AddCommandsInit(value.CommandsDefine());
        }

        public override string ToString()
        {
            return $"[T {aliasName}]";
        }
        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }
    }
}
