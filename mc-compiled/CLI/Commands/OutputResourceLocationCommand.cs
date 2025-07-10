using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class OutputResourceLocationCommand(string[] inputArgs) : CommandLineOption(inputArgs)
{
    public override string LongName => "outputrp";
    public override string ShortName => "orp";
    public override string Description =>
        $"Set the location where the resource pack will be written to. Use {Context.PROJECT_REPLACER} to denote the project name.";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.Of(1);
    public override string[] ArgNames => ["location"];
    public override CommandLineOption CreateNewWithArgs(string[] args)
    {
        return new OutputResourceLocationCommand(args);
    }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.resourcePackOutputPath = this.inputArgs[0];
    }
}