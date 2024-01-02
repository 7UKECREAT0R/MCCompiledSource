using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal class FunctionRoundCompiletime : CompiletimeFunction
    {
        public FunctionRoundCompiletime() : base("round", "compile_time_round", "int", "Rounds the given value to the nearest integer, or does nothing if it is already an integer.")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float number = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            int result = (int)Math.Round(number);

            return new TokenIntegerLiteral(result, IntMultiplier.none, statement.Lines[0]);
        }
    }
    internal class FunctionFloorCompiletime : CompiletimeFunction
    {
        public FunctionFloorCompiletime() : base("floor", "compile_time_floor", "int", "Rounds down the given value to the nearest integer.")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float number = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            int result = (int)Math.Floor(number);

            return new TokenIntegerLiteral(result, IntMultiplier.none, statement.Lines[0]);
        }
    }
    internal class FunctionCeilingCompiletime : CompiletimeFunction
    {
        public FunctionCeilingCompiletime() : base("ceiling", "compile_time_ceiling", "int", "Rounds up the given value to the nearest integer.")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float number = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            int result = (int)Math.Ceiling(number);

            return new TokenIntegerLiteral(result, IntMultiplier.none, statement.Lines[0]);
        }
    }
}
