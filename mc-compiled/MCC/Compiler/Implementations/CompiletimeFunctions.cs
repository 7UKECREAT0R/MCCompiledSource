using mc_compiled.MCC.Compiler.Implementations.Functions;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                    // compile-time functions
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
                };
            }
        }
    }
}
