using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Attributes
{
    public class AttributeFolder : IAttribute
    {
        public string path;

        internal AttributeFolder(string path)
        {
            this.path = path;
        }

        public string GetDebugString() => "folder";


        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement)
        {
            function.file.folder = path;
        }
        public void OnCalledFunction(RuntimeFunction function, List<string> commandBuffer, Executor executor, Statement statement) { }


        public void OnAddedValue(ScoreboardValue value, Statement causingStatement) =>
            throw new StatementException(causingStatement, "Cannot apply attribute 'folder' to a value.");
    }
}
