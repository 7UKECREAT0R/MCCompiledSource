using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// A function that runs as a test only.
    /// </summary>
    internal class TestFunction : RuntimeFunction
    {
        public TestFunction(Statement creationStatement, string aliasedName, string name, string documentation) : base(creationStatement, aliasedName, name, documentation, Array.Empty<IAttribute>(), false)
        {
            this.file.AsInUse();
            this.file.AsTest();
        }

        public override int Importance => 0; // please dont try to call this
        public override bool AdvertiseOverLSP => false;

        public override bool MatchParameters(Token[] inputs, out string error, out int score)
        {
            error = null;
            score = -999;
            return true;
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            throw new StatementException(statement, "Attempted to call test function manually. Use `/function test` ingame.");
        }
    }
}
