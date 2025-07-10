using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class OutputDevelopmentFoldersCommand() : CommandLineOption([])
{
    public override string LongName => "outputdevelopment";
    public override string ShortName => "od";
    public override string Description =>
        "Set the BP/RP output location to the local machine's Minecraft development folders.";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new OutputDevelopmentFoldersCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.SetOutputToDevelopmentFolders();
    }
}