﻿using System.Collections.Generic;
using mc_compiled.MCC.Compiler.Implementations.Functions;
using mc_compiled.MCC.Functions;

namespace mc_compiled.MCC.Compiler.Implementations;

public static class CompiletimeFunctions
{
    internal static readonly IFunctionProvider PROVIDER = new CompileTimeProvider();

    private class CompileTimeProvider : IFunctionProvider
    {
        IEnumerable<Function> IFunctionProvider.ProvideFunctions(ScoreboardManager manager)
        {
            return
            [
                // special
                new FunctionGlyphE0(),
                new FunctionGlyphE1(),
                new FunctionGetValueByName(),
                new FunctionCountEntities(),
                new FunctionParseInteger(),
                new FunctionParseNumber(),
                new FunctionParseSelector(),
                new FunctionParseBoolean(),

                // compile-time functions
                new FunctionRandomCompiletime(),
                new FunctionRandomCompiletimeRange(),
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
            ];
        }
    }
}