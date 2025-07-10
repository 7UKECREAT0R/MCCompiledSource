using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Language;
using mc_compiled.MCC.ServerWebSocket;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class ServerCommand() : CommandLineOption([])
{
    public override string LongName => "server";
    public override string ShortName => null;
    public override string Description => "Starts a language server for use with the web-based editor.";
    public override bool IsRunnable => true;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new ServerCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        files = null;
        Definitions.TryInitialize(context.debug);
        Language.TryLoad();

        context.SetOutputToDevelopmentFolders();
        using var server = new MCCServer(context.behaviorPackOutputPath, context.resourcePackOutputPath);
        server.StartServer(); // never returns
    }
}