using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;
using System.Collections.Generic;

namespace mc_compiled.MCC.Attributes
{
    internal class AttributeExtern : IAttribute
    {
        public string GetDebugString() => "extern";
        public string GetCodeRepresentation() => "extern";
        
        internal AttributeExtern() { }

        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement)
        {
            function.isExtern = true;           // mark extern
            function.file.DoNotWrite = true;    // we don't want the extern file to be overwritten, but we want it registered.
        }

        public void OnAddedValue(ScoreboardValue value, Statement causingStatement) =>
            throw new StatementException(causingStatement, "Cannot apply attribute 'extern' to a value.");

        public void OnCalledFunction(RuntimeFunction function,
            List<string> commands, Executor executor, Statement statement) {}
    }
}
