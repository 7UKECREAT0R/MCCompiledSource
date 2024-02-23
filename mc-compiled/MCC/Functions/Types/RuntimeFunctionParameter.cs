using mc_compiled.MCC.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using mc_compiled.MCC.Compiler.TypeSystem;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// A function parameter that points to a runtime value.
    /// Setting this parameter's value will result in commands being added to set <see cref="RuntimeDestination"/>.
    /// </summary>
    public class RuntimeFunctionParameter : FunctionParameter
    {
        /// <summary>
        /// The destination variable that the value given will arrive at.
        /// </summary>
        public ScoreboardValue RuntimeDestination { get; protected set; }

        public RuntimeFunctionParameter(ScoreboardValue value, Token defaultValue = null) : base(value.Name, defaultValue)
        {
            this.RuntimeDestination = value;
        }
        protected RuntimeFunctionParameter(string name, Token defaultValue = null) : base(name, defaultValue)
        {
            this.RuntimeDestination = null;
        }

        public override ParameterFit CheckInput(Token token)
        {
            switch (token)
            {
                case TokenLiteral literal:
                    {
                        Typedef sbType = literal.GetTypedef();
                        Typedef sbDestType = this.RuntimeDestination.type;

                        if (sbType == null)
                            return ParameterFit.No;
                        if (!Equals(sbType, sbDestType))
                            return ParameterFit.WithConversion;

                        return ParameterFit.Yes;
                    }
                case TokenIdentifierValue _value:
                    {
                        Typedef valueType = _value.value.type;
                        Typedef valueDestType = this.RuntimeDestination.type;

                        if (valueType == null)
                            return ParameterFit.No;

                        if (!valueType.NeedsToBeConvertedTo(_value.value, this.RuntimeDestination))
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
                            IEnumerable<string> commands = this.RuntimeDestination.AssignLiteral(literal, callingStatement);
                            commandBuffer.AddRange(commands);
                            return;
                        }
                        break;
                    }
                case TokenIdentifierValue _value:
                    {
                        ScoreboardValue value = _value.value;
                        IEnumerable<string> commands = this.RuntimeDestination.Assign(value, callingStatement);
                        commandBuffer.AddRange(commands);
                        return;
                    }
            }

            throw new StatementException(callingStatement, "Invalid parameter input. Developers: please use CheckInput(...)");
        }

        public override string ToString()
        {
            string type = this.RuntimeDestination.type.TypeKeyword;
            return $"[{type} {this.name}]";
        }

        protected bool Equals(RuntimeFunctionParameter other)
        {
            return Equals(this.RuntimeDestination, other.RuntimeDestination);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RuntimeFunctionParameter) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")] // DNC
        public override int GetHashCode()
        {
            return (this.RuntimeDestination != null ? this.RuntimeDestination.GetHashCode() : 0);
        }
    }
}
