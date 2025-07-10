using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class EnableExportAllCommand() : CommandLineOption([])
{
    public override string LongName => "exportall";
    public override string ShortName => "ea";
    public override string Description =>
        "Forces MCCompiled to export all functions to disk, even if they went unused.";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new EnableExportAllCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.exportAll = true;
    }
}