using mc_compiled.Commands;
using mc_compiled.MCC.Functions.Types;
using System.Collections.Generic;

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal class FunctionMinCompiletime : CompiletimeFunction
    {
        public FunctionMinCompiletime() : base("min", "compiletimeMin", "T", "Returns the smaller of the two input values, favoring `a` if both values are equal.")
        {
            AddParameters(
                new CompiletimeFunctionParameter<TokenLiteral>("a"),
                new CompiletimeFunctionParameter<TokenLiteral>("b")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor,
            Statement statement)
        {
            TokenLiteral a = ((CompiletimeFunctionParameter)this.Parameters[0]).CurrentValue as TokenLiteral;
            TokenLiteral b = ((CompiletimeFunctionParameter)this.Parameters[1]).CurrentValue as TokenLiteral;

            if(a.CompareWithOther(TokenCompare.Type.LESS_OR_EQUAL, b))
                return a;

            return b;
        }
    }
    internal class FunctionMinRuntime : GenerativeFunction
    {
        public FunctionMinRuntime() : base("min", "runtimeMin", "T", "Returns the smaller of the two input values, favoring `a` if both values are equal.")
        {
            AddParameters(
                new RuntimeFunctionParameterDynamic(this, "a", "runtime_min_a"),
                new RuntimeFunctionParameterDynamic(this, "b", "runtime_min_b")
            );
        }
        public override void GenerateCode(CommandFile output, int uniqueIdentifier, Executor executor, Statement statement, out ScoreboardValue resultValue)
        {
            RuntimeFunctionParameterDynamic _a = (RuntimeFunctionParameterDynamic)this.parameters[0];
            RuntimeFunctionParameterDynamic _b = (RuntimeFunctionParameterDynamic)this.parameters[1];
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

            string commandToReturnA = TryReturnValue(a, executor, statement, out resultValue);
            string commandToReturnB = TryReturnValue(b, executor, statement, out _);

            output.Add(Command.Execute().IfScore(a, TokenCompare.Type.LESS_OR_EQUAL, converted).Run(commandToReturnA));
            output.Add(Command.Execute().IfScore(a, TokenCompare.Type.GREATER, converted).Run(commandToReturnB));
        }
    }

    internal class FunctionMaxCompiletime : CompiletimeFunction
    {
        public FunctionMaxCompiletime() : base("max", "compiletimeMax", "T", "Returns the larger of the two input values, favoring `a` if both values are equal.")
        {
            AddParameters(
                new CompiletimeFunctionParameter<TokenLiteral>("a"),
                new CompiletimeFunctionParameter<TokenLiteral>("b")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor,
            Statement statement)
        {
            TokenLiteral a = ((CompiletimeFunctionParameter)this.Parameters[0]).CurrentValue as TokenLiteral;
            TokenLiteral b = ((CompiletimeFunctionParameter)this.Parameters[1]).CurrentValue as TokenLiteral;

            if (a.CompareWithOther(TokenCompare.Type.GREATER_OR_EQUAL, b))
                return a;

            return b;
        }
    }
    internal class FunctionMaxRuntime : GenerativeFunction
    {
        public FunctionMaxRuntime() : base("max", "runtimeMax", "T", "Returns the larger of the two input values, favoring `a` if both values are equal.")
        {
            AddParameters(
                new RuntimeFunctionParameterDynamic(this, "a", "runtime_min_a"),
                new RuntimeFunctionParameterDynamic(this, "b", "runtime_min_b")
            );
        }
        public override void GenerateCode(CommandFile output, int uniqueIdentifier, Executor executor, Statement statement, out ScoreboardValue resultValue)
        {
            RuntimeFunctionParameterDynamic _a = (RuntimeFunctionParameterDynamic)this.parameters[0];
            RuntimeFunctionParameterDynamic _b = (RuntimeFunctionParameterDynamic)this.parameters[1];
            ScoreboardValue a = _a.RuntimeDestination;
            ScoreboardValue b = _b.RuntimeDestination;

            ScoreboardValue converted;

            if (b.NeedsToBeConvertedFor(a))
            {
                string convertedName = CreateUniqueTempValueName(uniqueIdentifier);
                converted = a.Clone(statement, newInternalName: convertedName, newName: convertedName);
                executor.scoreboard.Add(converted);
                executor.AddCommandsInit(converted.CommandsInit());
                output.Add(b.CommandsConvert(converted, statement));
            }
            else
                converted = b;

            string commandToReturnA = TryReturnValue(a, executor, statement, out resultValue);
            string commandToReturnB = TryReturnValue(b, executor, statement, out _);

            output.Add(Command.Execute().IfScore(a, TokenCompare.Type.GREATER_OR_EQUAL, converted).Run(commandToReturnA));
            output.Add(Command.Execute().IfScore(a, TokenCompare.Type.LESS, converted).Run(commandToReturnB));
        }
    }
}
