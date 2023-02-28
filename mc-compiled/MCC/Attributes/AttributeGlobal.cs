using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Attributes
{
    internal class AttributeGlobal : IAttribute
    {
        public string GetDebugString() => "[Attribute: global]";

        internal AttributeGlobal() { }
        public void OnAddedValue(ScoreboardValue value, Statement causingStatement)
        {
            value.clarifier.SetGlobal(true);
        }

        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement) =>
            throw new StatementException(causingStatement, "Cannot apply attribute 'global' to a function.");
        public void OnCalledFunction(RuntimeFunction function, List<string> commandBuffer, Executor executor, Statement statement) { }
    }
}
