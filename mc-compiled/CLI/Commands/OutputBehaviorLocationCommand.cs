using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class OutputBehaviorLocationCommand(string[] inputArgs) : CommandLineOption(inputArgs)
{
    public override string LongName => "outputbp";
    public override string ShortName => "obp";
    public override string Description =>
        $"Set the location where the behavior pack will be written to. Use {Context.PROJECT_REPLACER} to denote the project name.";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.Of(1);
    public override string[] ArgNames => ["location"];
    public override CommandLineOption CreateNewWithArgs(string[] args)
    {
        return new OutputBehaviorLocationCommand(args);
    }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.behaviorPackOutputPath = this.inputArgs[0];
    }
}