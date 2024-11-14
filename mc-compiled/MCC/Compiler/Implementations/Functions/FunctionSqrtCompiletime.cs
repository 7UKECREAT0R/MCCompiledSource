using System;
using System.Collections.Generic;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal class FunctionSqrtCompiletime : CompiletimeFunction
    {
        public FunctionSqrtCompiletime() : base("sqrt", "compiletimeSqrt", "decimal ?", "Calculates the square root of the given number.")
        {
            AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor,
            Statement statement)
        {
            decimal number = ((TokenNumberLiteral)((CompiletimeFunctionParameter)this.Parameters[0]).CurrentValue).GetNumber();
            decimal result = (decimal)(float)Math.Sqrt((double)number);

            return new TokenDecimalLiteral(result, statement.Lines[0]);
        }
    }
}
