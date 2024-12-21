using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Attributes;

internal class AttributeGlobal : IAttribute
{
    public string GetDebugString()
    {
        return "global";
    }
    public string GetCodeRepresentation()
    {
        return "global";
    }

    public void OnAddedValue(ScoreboardValue value, Statement causingStatement)
    {
        value.clarifier.SetGlobal(true);
    }

    public void OnAddedFunction(RuntimeFunction function, Statement causingStatement)
    {
        throw new StatementException(causingStatement,
            "Cannot apply attribute 'global' to a function. Did you mean 'async(global)'?"
        );
    }

    public void OnCalledFunction(RuntimeFunction function,
        List<string> commands, Executor executor, Statement statement) { }
}