using mc_compiled.Commands;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal class FunctionMinCompiletime : CompiletimeFunction
    {
        public FunctionMinCompiletime() : base("min", "compile_time_min", "T", "Returns the smaller of the two input values, favoring `a` if both values are equal.")
        {
            this.AddParameters(
                new CompiletimeFunctionParameter<TokenLiteral>("a"),
                new CompiletimeFunctionParameter<TokenLiteral>("b")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            TokenLiteral a = (this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenLiteral;
            TokenLiteral b = (this.Parameters[1] as CompiletimeFunctionParameter).CurrentValue as TokenLiteral;

            if(a.CompareWithOther(TokenCompare.Type.LESS_OR_EQUAL, b))
                return a;

            return b;
        }
    }
    internal class FunctionMinRuntime : GenerativeFunction
    {
        public FunctionMinRuntime() : base("min", "run_time_min", "T", "Returns the smaller of the two input values, favoring `a` if both values are equal.")
        {
            this.AddParameters(
                new RuntimeFunctionParameterAny(this, "a", "runtime_min_a"),
                new RuntimeFunctionParameterAny(this, "b", "runtime_min_b")
            );
        }
        public override void GenerateCode(CommandFile output, int uniqueIdentifier, Executor executor, Statement statement, out ScoreboardValue resultValue)
        {
            RuntimeFunctionParameterAny _a = parameters[0] as RuntimeFunctionParameterAny;
            RuntimeFunctionParameterAny _b = parameters[1] as RuntimeFunctionParameterAny;
            ScoreboardValue a = _a.RuntimeDestination;
            ScoreboardValue b = _b.RuntimeDestination;

            ScoreboardValue converted;

            if(b.NeedsToBeConvertedFor(a))
            {
                string convertedName = CreateUniqueTempValueName(uniqueIdentifier);
                converted = a.Clone(statement, newInternalName: convertedName, newName: convertedName);
                executor.scoreboard.Add(converted);
                executor.AddCommandsInit(converted.CommandsInit());
                output.Add(b.CommandsConvert(converted, statement));
            }
            else
                converted = b;

            string commandToReturnA = this.TryReturnValue(a, executor, statement, out resultValue);
            string commandToReturnB = this.TryReturnValue(b, executor, statement, out _);

            output.Add(Command.Execute().IfScore(a, TokenCompare.Type.LESS_OR_EQUAL, converted).Run(commandToReturnA));
            output.Add(Command.Execute().IfScore(a, TokenCompare.Type.GREATER, converted).Run(commandToReturnB));
        }
    }

    internal class FunctionMaxCompiletime : CompiletimeFunction
    {
        public FunctionMaxCompiletime() : base("max", "compile_time_max", "T", "Returns the larger of the two input values, favoring `a` if both values are equal.")
        {
            this.AddParameters(
                new CompiletimeFunctionParameter<TokenLiteral>("a"),
                new CompiletimeFunctionParameter<TokenLiteral>("b")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            TokenLiteral a = (this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenLiteral;
            TokenLiteral b = (this.Parameters[1] as CompiletimeFunctionParameter).CurrentValue as TokenLiteral;

            if (a.CompareWithOther(TokenCompare.Type.GREATER_OR_EQUAL, b))
                return a;

            return b;
        }
    }
}
