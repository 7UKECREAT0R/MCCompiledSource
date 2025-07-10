using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class EnableTraceOutputCommand() : CommandLineOption([])
{
    public override string LongName => "trace";
    public override string ShortName => null;
    public override string Description =>
        "Include trace commands in the final result, which is useful for debugging async code and other complex behavior.";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new EnableTraceOutputCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.trace = true;
    }
}