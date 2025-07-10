using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class SetProjectNameCommand(string[] inputArgs) : CommandLineOption(inputArgs)
{
    public override string LongName => "project";
    public override string ShortName => "p";
    public override string Description =>
        "Set the name of the project to use. If unspecified, the file name will be used.";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.Of(1);
    public override string[] ArgNames => ["project name"];
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new SetProjectNameCommand(args); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.projectName = this.inputArgs[0];
    }
}