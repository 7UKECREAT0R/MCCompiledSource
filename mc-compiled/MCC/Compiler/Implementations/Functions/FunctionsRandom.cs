using System.Collections.Generic;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal class FunctionRandomCompiletimeRange : CompiletimeFunction
    {
        public FunctionRandomCompiletimeRange() : base("random", "compiletimeRandomRange", "int",
            "Returns a random number in the given range. If a single number is given, it returns (0..n-1).")
        {
            AddParameter(
                new CompiletimeFunctionParameter<TokenRangeLiteral>("range")
            );
        }

        public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor, Statement statement)
        {
            Range randomRange = ((TokenRangeLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue).range;
            int min, max;

            if (randomRange.single)
            {
                min = 0;
                max = (randomRange.min ?? 1) - 1;
            }
            else
            {
                min = randomRange.min.GetValueOrDefault();
                max = randomRange.max.GetValueOrDefault();
            }

            if (min == max)
                throw new StatementException(statement, $"Return value will always be the same. ({min}..{max})");

            ScoreboardValue temp = executor.scoreboard.temps.RequestGlobal(Typedef.INTEGER);
            commandBuffer.Add(Command.ScoreboardRandom(temp, min, max));
            return new TokenIdentifierValue(temp.ToString(), temp, statement.Lines[0]);
        }
    }

    internal class FunctionRandomCompiletime : CompiletimeFunction
    {
        public FunctionRandomCompiletime() : base("random", "compiletimeRandom", "int",
            "Returns a random number in the given range. If a single number is given, it returns (0..n-1).")
        {
            AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("range")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor, Statement statement)
        {
            int maxExclusive = ((TokenNumberLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue).GetNumberInt();

            if (maxExclusive == 1)
                throw new StatementException(statement, $"Return value will always be the same. (0..0)");

            ScoreboardValue temp = executor.scoreboard.temps.RequestGlobal(Typedef.INTEGER);
            commandBuffer.Add(Command.ScoreboardRandom(temp, 0, maxExclusive - 1));
            return new TokenIdentifierValue(temp.ToString(), temp, statement.Lines[0]);
        }
    }

    internal class FunctionRandomRuntime : GenerativeFunction
    {
        public FunctionRandomRuntime() : base("random", "runtimeRandomModulo", "int",
            "Returns a random number in the given range. If a single number is given, it returns (0..n-1).")
        {
            AddParameter(
                new RuntimeFunctionParameterDynamic(this, "maxExclusive", "max_exclusive")
                    .WithAcceptedTypes(Typedef.INTEGER)
            );
        }
        
        public override void GenerateCode(CommandFile output, int uniqueIdentifier, Executor executor, Statement statement,
            out ScoreboardValue resultValue)
        {
            ScoreboardValue bound = ((RuntimeFunctionParameterDynamic) this.parameters[0]).RuntimeDestination;
            ScoreboardValue temp = executor.scoreboard.temps.RequestGlobal(Typedef.INTEGER);

            output.Add(Command.ScoreboardRandom(temp, 0, int.MaxValue - 1));
            output.Add(Command.ScoreboardOpMod(temp, bound));
            resultValue = temp;
        }
    }
}