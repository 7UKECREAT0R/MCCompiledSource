using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class EnableDecorationCommand() : CommandLineOption([])
{
    public override string LongName => "decorate";
    public override string ShortName => "dc";
    public override string Description =>
        "Decorate the compiled files with comments containing original source code and extra helpful information for pathing, debugging, etc...";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new EnableDecorationCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.decorate = true;
    }
}