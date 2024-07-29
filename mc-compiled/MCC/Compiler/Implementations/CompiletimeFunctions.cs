using mc_compiled.MCC.Compiler.Implementations.Functions;
using mc_compiled.MCC.Functions;
using System.Collections.Generic;

namespace mc_compiled.MCC.Compiler.Implementations
{
    public static class CompiletimeFunctions
    {
        internal static readonly IFunctionProvider PROVIDER = new CompileTimeProvider();

        private class CompileTimeProvider : IFunctionProvider
        {
            IEnumerable<Function> IFunctionProvider.ProvideFunctions(ScoreboardManager manager)
            {
                return new Function[]
                {
                    // special
                    new FunctionGlyphE0(),
                    new FunctionGlyphE1(),
                    new FunctionGetValueByName(),
                    new FunctionCountEntities(),
                    
                    // compile-time functions
                    new FunctionRandomCompiletime(),
                    new FunctionMinCompiletime(),
                    new FunctionMaxCompiletime(),
                    new FunctionSqrtCompiletime(),
                    new FunctionSineCompiletime(),
                    new FunctionCosineCompiletime(),
                    new FunctionTangentCompiletime(),
                    new FunctionArctangentCompiletime(),
                    new FunctionRoundCompiletime(),
                    new FunctionFloorCompiletime(),
                    new FunctionCeilingCompiletime(),

                    // generative functions (generates runtime code as needed)
                    new FunctionMinRuntime(),
                    new FunctionMaxRuntime(),
                    new FunctionRoundRuntime(),
                    new FunctionFloorRuntime(),
                    new FunctionCeilingRuntime(),
                    new FunctionRandomRuntime()
                };
            }
        }
    }
}
