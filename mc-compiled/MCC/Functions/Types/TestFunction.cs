using System.Collections.Generic;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC.Functions.Types;

/// <summary>
///     A function that runs as a test only.
/// </summary>
internal class TestFunction : RuntimeFunction
{
    public TestFunction(Statement creationStatement, string name, string internalName, string documentation) : base(
        creationStatement, name, internalName, documentation, [])
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
    public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor,
        Statement statement)
    {
        throw new StatementException(statement,
            "Attempted to call test function manually. Use `/function test` ingame.");
    }
}