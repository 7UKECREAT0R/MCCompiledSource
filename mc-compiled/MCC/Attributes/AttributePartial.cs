using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Attributes
{
    internal class AttributePartial : IAttribute
    {
        public string GetDebugString() => "partial";
        public string GetCodeRepresentation() => "partial";
        
        internal AttributePartial() { }

        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement)
        {
            if (function.HasAttribute<AttributeAsync>())
                throw new StatementException(causingStatement, "Cannot make an async function 'partial'.");
        }
        public void OnAddedValue(ScoreboardValue value, Statement causingStatement) =>
            throw new StatementException(causingStatement, "Cannot apply attribute 'partial' to a value.");
        public void OnCalledFunction(RuntimeFunction function,
            List<string> commands, Executor executor, Statement statement) {}
    }
}