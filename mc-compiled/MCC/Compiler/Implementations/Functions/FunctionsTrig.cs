using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal class FunctionSineCompiletime : CompiletimeFunction
    {
        public FunctionSineCompiletime() : base("sin", "compile_time_sin", "decimal ?", "Calculates the sine of the given number.")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float number = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            float result = (float)Math.Sin(number);

            return new TokenDecimalLiteral(result, statement.Lines[0]);
        }
    }
    internal class FunctionCosineCompiletime : CompiletimeFunction
    {
        public FunctionCosineCompiletime() : base("cos", "compile_time_cos", "decimal ?", "Calculates the co-sine of the given number.")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float number = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            float result = (float)Math.Cos(number);

            return new TokenDecimalLiteral(result, statement.Lines[0]);
        }
    }
    internal class FunctionTangentCompiletime : CompiletimeFunction
    {
        public FunctionTangentCompiletime() : base("tan", "compile_time_tan", "decimal ?", "Calculates the tangent of the given number.")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float number = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            float result = (float)Math.Tan(number);

            return new TokenDecimalLiteral(result, statement.Lines[0]);
        }
    }
    internal class FunctionArctangentCompiletime : CompiletimeFunction
    {
        public FunctionArctangentCompiletime() : base("arctan", "compile_time_arctan", "decimal ?", "Calculates the angle thats tangent is equal to the given number. To get an equivalent function to arctan2, use `arctan(a / b)`")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float numberA = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            float result = (float)Math.Atan(numberA);

            return new TokenDecimalLiteral(result, statement.Lines[0]);
        }
    }
}
