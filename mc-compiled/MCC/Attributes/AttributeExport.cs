using System;
using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Attributes
{
    internal class AttributeExport : IAttribute
    {
        public string GetDebugString() => "export";
        public string GetCodeRepresentation() => "export";

        internal AttributeExport() { }

        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement)
        {
            function.file.AsInUse(); // Marks file as 'in use' so it is included in the output, along with anything it calls.
        }

        public void OnAddedValue(ScoreboardValue value, Statement causingStatement) =>
            throw new StatementException(causingStatement, "Cannot apply attribute 'export' to a value.");

        public void OnCalledFunction(RuntimeFunction function,
            List<string> commands, Executor executor, Statement statement) {}
    }
}