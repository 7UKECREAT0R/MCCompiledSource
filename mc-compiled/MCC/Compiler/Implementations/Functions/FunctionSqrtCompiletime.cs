using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal class FunctionSqrtCompiletime : CompiletimeFunction
    {
        public FunctionSqrtCompiletime() : base("sqrt", "compiletimeSqrt", "decimal ?", "Calculates the square root of the given number.")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float number = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            float result = (float)Math.Sqrt(number);

            return new TokenDecimalLiteral(result, statement.Lines[0]);
        }
    }
}
