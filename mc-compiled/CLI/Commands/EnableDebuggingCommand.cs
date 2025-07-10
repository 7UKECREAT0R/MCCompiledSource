using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class EnableDebuggingCommand() : CommandLineOption([])
{
    public override string LongName => "debug";
    public override string ShortName => "db";
    public override string Description =>
        "Get debug information during compilation. Might hit compilation time for larger projects.";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new EnableDebuggingCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.debug = true;
    }
}