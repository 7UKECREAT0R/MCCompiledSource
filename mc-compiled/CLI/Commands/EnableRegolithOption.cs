using System.IO;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class EnableRegolithOption() : CommandLineOption([])
{
    public override string LongName => "regolith";
    public override string ShortName => null;
    public override string Description => "Launch in a compatibility mode intended for running as a Regolith filter.";
    public override bool IsHiddenFromHelp => true; // already used by the filter implementation itself
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new EnableRegolithOption(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.regolith = true;
        context.search = true;
        files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mcc", SearchOption.AllDirectories);
    }
}