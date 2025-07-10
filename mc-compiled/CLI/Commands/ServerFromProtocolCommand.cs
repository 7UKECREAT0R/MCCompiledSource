using System.Diagnostics;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Language;
using mc_compiled.MCC.ServerWebSocket;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class ServerFromProtocolCommand() : CommandLineOption([])
{
    public override string LongName => "fromprotocol";
    public override string ShortName => null;
    public override string Description =>
        "Starts the language server for use with the web-based editor. This is the argument passed when the application is launched by the protocol URL.";
    public override bool IsHiddenFromHelp => true;
    public override bool IsRunnable => true;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new ServerFromProtocolCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        files = null;

        // check for existing MCCompiled processes
        Process[] mccProcesses = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
        if (mccProcesses.Length > 1)
            return;

        // step up to the job
        Definitions.TryInitialize(context.debug);
        Language.TryLoad();

        context.SetOutputToDevelopmentFolders();
        using var server = new MCCServer(context.behaviorPackOutputPath, context.resourcePackOutputPath);
        server.StartServer(); // never returns
    }
}