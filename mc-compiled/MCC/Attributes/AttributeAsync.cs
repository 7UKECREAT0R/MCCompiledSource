using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Compiler.Async;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Attributes
{
    public class AttributeAsync : IAttribute
    {
        public string GetDebugString()
        {
            return $"async: {this.target}";
        }
        public string GetCodeRepresentation()
        {
            return $"async({this.target.ToString().ToLower()})";
        }

        public readonly AsyncTarget target;
        
        internal AttributeAsync(AsyncTarget target)
        {
            this.target = target;
        }

        public void OnAddedValue(ScoreboardValue value, Statement causingStatement) =>
            throw new StatementException(causingStatement, "Cannot apply attribute 'async' to a value.");

        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement) { }
        public void OnCalledFunction(RuntimeFunction function, List<string> commands, Executor executor, Statement statement) { }
    }
}