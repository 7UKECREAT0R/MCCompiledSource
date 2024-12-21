using System;
using System.Collections.Generic;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Compiler.Implementations.Functions;

internal class FunctionSineCompiletime : CompiletimeFunction
{
    public FunctionSineCompiletime() : base("sin", "compiletimeSin", "decimal ?",
        "Calculates the sine of the given number.")
    {
        AddParameter(
            new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
        );
    }
    public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor,
        Statement statement)
    {
        decimal number = ((TokenNumberLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue)
            .GetNumber();
        decimal result = (decimal) (float) Math.Sin((double) number);

        return new TokenDecimalLiteral(result, statement.Lines[0]);
    }
}

internal class FunctionCosineCompiletime : CompiletimeFunction
{
    public FunctionCosineCompiletime() : base("cos", "compiletimeCos", "decimal ?",
        "Calculates the co-sine of the given number.")
    {
        AddParameter(
            new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
        );
    }
    public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor,
        Statement statement)
    {
        decimal number = ((TokenNumberLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue)
            .GetNumber();
        decimal result = (decimal) (float) Math.Cos((double) number);

        return new TokenDecimalLiteral(result, statement.Lines[0]);
    }
}

internal class FunctionTangentCompiletime : CompiletimeFunction
{
    public FunctionTangentCompiletime() : base("tan", "compiletimeTan", "decimal ?",
        "Calculates the tangent of the given number.")
    {
        AddParameter(
            new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
        );
    }
    public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor,
        Statement statement)
    {
        decimal number = ((TokenNumberLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue)
            .GetNumber();
        decimal result = (decimal) (float) Math.Tan((double) number);

        return new TokenDecimalLiteral(result, statement.Lines[0]);
    }
}

internal class FunctionArctangentCompiletime : CompiletimeFunction
{
    public FunctionArctangentCompiletime() : base("arctan", "compiletimeArctan", "decimal ?",
        "Calculates the angle thats tangent is equal to the given number. To get an equivalent function to arctan2, use `arctan(a / b)`")
    {
        AddParameter(
            new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
        );
    }
    public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor,
        Statement statement)
    {
        decimal number = ((TokenNumberLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue)
            .GetNumber();
        decimal result = (decimal) (float) Math.Atan((double) number);

        return new TokenDecimalLiteral(result, statement.Lines[0]);
    }
}