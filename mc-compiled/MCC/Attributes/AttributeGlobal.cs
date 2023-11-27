﻿using System;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;
using System.Collections.Generic;

namespace mc_compiled.MCC.Attributes
{
    internal class AttributeGlobal : IAttribute
    {
        public string GetDebugString() => "global";

        internal AttributeGlobal() { }
        public void OnAddedValue(ScoreboardValue value, Statement causingStatement)
        {
            value.clarifier.SetGlobal(true);
        }

        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement) =>
            throw new StatementException(causingStatement, "Cannot apply attribute 'global' to a function.");

        public void OnCalledFunction(RuntimeFunction function,
            List<string> commands, Executor executor, Statement statement) {}
    }
}
