using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Attributes;

internal class AttributeLocal : IAttribute
{
    public string GetDebugString()
    {
        return "local";
    }
    public string GetCodeRepresentation()
    {
        return "local";
    }

    public void OnAddedValue(ScoreboardValue value, Statement causingStatement)
    {
        value.clarifier.SetGlobal(false);
    }
    public void OnAddedFunction(RuntimeFunction function, Statement causingStatement)
    {
        throw new StatementException(causingStatement,
            "Cannot apply attribute 'local' to a function. Did you mean 'async(local)'?"
        );
    }
    public void OnCalledFunction(RuntimeFunction function,
        List<string> commands, Executor executor, Statement statement) { }
}