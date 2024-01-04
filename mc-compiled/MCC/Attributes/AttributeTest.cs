using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Attributes
{
    internal class AttributeTest : IAttribute
    {
        public string GetDebugString() => "test";
        public string GetCodeRepresentation() => "test";

        internal AttributeTest() { }

        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement)
        {
            causingStatement.executor.RequireFeature(causingStatement, Feature.TESTS);

            function.file.AsInUse();
            causingStatement.executor.functions.RegisterTest(function);
            function.AsTest();
        }

        public void OnAddedValue(ScoreboardValue value, Statement causingStatement) =>
            throw new StatementException(causingStatement, "Cannot apply attribute 'test' to a value.");

        public void OnCalledFunction(RuntimeFunction function,
            List<string> commands, Executor executor, Statement statement)
        {
            throw new StatementException(statement, "Should not call a test function manually, use `/function test` to run all tests.");
        }
    }
}
