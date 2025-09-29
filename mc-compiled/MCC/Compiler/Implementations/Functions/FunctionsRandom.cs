using System;
using System.Collections.Generic;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Functions.Types;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Compiler.Implementations.Functions;

internal class FunctionRandomCompiletimeRange : CompiletimeFunction
{
    public FunctionRandomCompiletimeRange() : base("random", "compiletimeRandomRange", "int",
        "Returns a random number in the given range. If a single number is given, it returns (0..n-1).")
    {
        AddParameter(
            new CompiletimeFunctionParameter<TokenRangeLiteral>("range")
        );
    }

    public override Token CallFunction(List<string> commandBuffer,
        Token[] allParameters,
        Executor executor,
        Statement statement)
    {
        Range randomRange = ((TokenRangeLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue)
            .range;
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
    public override Token CallFunction(List<string> commandBuffer,
        Token[] allParameters,
        Executor executor,
        Statement statement)
    {
        int maxExclusive = ((TokenNumberLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue)
            .GetNumberInt();

        if (maxExclusive == 1)
            throw new StatementException(statement, "Return value will always be the same. (0..0)");

        ScoreboardValue temp = executor.scoreboard.temps.RequestGlobal(Typedef.INTEGER);
        commandBuffer.Add(Command.ScoreboardRandom(temp, 0, maxExclusive - 1));
        return new TokenIdentifierValue(temp.ToString(), temp, statement.Lines[0]);
    }
}

internal class FunctionRandomBool : GenerativeFunction
{
    public FunctionRandomBool() : base("randomBool", "runtimeRandomBool", "bool",
        "Returns a random true/false value.") { }

    public override void GenerateCode(CommandFile output,
        int uniqueIdentifier,
        Executor executor,
        Statement statement,
        out ScoreboardValue resultValue)
    {
        ScoreboardValue temp = executor.scoreboard.temps.RequestGlobal(Typedef.BOOLEAN);
        output.Add(Command.ScoreboardRandom(temp, 0, 1));
        resultValue = temp;
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

    public override void GenerateCode(CommandFile output,
        int uniqueIdentifier,
        Executor executor,
        Statement statement,
        out ScoreboardValue resultValue)
    {
        ScoreboardValue bound = ((RuntimeFunctionParameterDynamic) this.parameters[0]).RuntimeDestination;
        ScoreboardValue temp = executor.scoreboard.temps.RequestGlobal(Typedef.INTEGER);

        output.Add(Command.ScoreboardRandom(temp, 0, int.MaxValue - 1));
        output.Add(Command.ScoreboardOpMod(temp, bound));
        resultValue = temp;
    }
}

internal class FunctionBakedRandom : CompiletimeFunction
{
    private static readonly Random random = new();
    public FunctionBakedRandom() : base("ctRandom", "bakedRandom", "int",
        "Returns a random number in the given range, once at compile-time. The result of this function will always be the same at runtime.")
    {
        AddParameter(new CompiletimeFunctionParameter<TokenNumberLiteral>("minInclusive"));
        AddParameter(new CompiletimeFunctionParameter<TokenNumberLiteral>("maxExclusive"));
    }

    public override Token CallFunction(List<string> commandBuffer,
        Token[] allParameters,
        Executor executor,
        Statement statement)
    {
        int minInclusive = ((TokenNumberLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue)
            .GetNumberInt();
        int maxExclusive = ((TokenNumberLiteral) ((CompiletimeFunctionParameter) this.Parameters[1]).CurrentValue)
            .GetNumberInt();

        int result = random.Next(minInclusive, maxExclusive);
        return new TokenIntegerLiteral(result, IntMultiplier.none, statement.Lines[0]);
    }
}

internal class FunctionBakedRandomRange : CompiletimeFunction
{
    private static readonly Random random = new();
    public FunctionBakedRandomRange() : base("ctRandom", "bakedRandomRange", "int",
        "Returns a random number in the given range, once at compile-time. The result of this function will always be the same at runtime.")
    {
        AddParameter(new CompiletimeFunctionParameter<TokenRangeLiteral>("range"));
    }

    public override Token CallFunction(List<string> commandBuffer,
        Token[] allParameters,
        Executor executor,
        Statement statement)
    {
        Range range = ((TokenRangeLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue).range;

        int min, max;

        if (range.single)
        {
            min = max = range.min.GetValueOrDefault();
        }
        else if (range.min == null)
        {
            min = int.MinValue;
            max = range.max.GetValueOrDefault();
        }
        else if (range.max == null)
        {
            min = range.min.GetValueOrDefault();
            max = int.MaxValue - 1;
        }
        else
        {
            min = range.min.GetValueOrDefault();
            max = range.max.GetValueOrDefault();
        }

        if (min == max)
            throw new StatementException(statement, $"Return value will always be the same. ({min}..{max})");

        int result = random.Next(min, max + 1);
        return new TokenIntegerLiteral(result, IntMultiplier.none, statement.Lines[0]);
    }
}

internal class FunctionBakedRandomBool : CompiletimeFunction
{
    private static readonly Random random = new();
    public FunctionBakedRandomBool() : base("ctRandomBool", "bakedRandomBool", "bool",
        "Returns a random true/false value, once at compile-time. The result of this function will always be the same at runtime.") { }

    public override Token CallFunction(List<string> commandBuffer,
        Token[] allParameters,
        Executor executor,
        Statement statement)
    {
        bool result = random.Next(0, 2) == 0;
        return new TokenBooleanLiteral(result, statement.Lines[0]);
    }
}