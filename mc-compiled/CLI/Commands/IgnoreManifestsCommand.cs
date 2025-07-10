using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class IgnoreManifestsCommand() : CommandLineOption([])
{
    public override string LongName => "ignoremanifests";
    public override string ShortName => "im";
    public override string Description => "Have the compiler completely ignore existing BP/RP manifests.";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new IgnoreManifestsCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.ignoreManifests = true;
    }
}