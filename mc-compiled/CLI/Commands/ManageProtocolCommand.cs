using System;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.ServerWebSocket;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class ManageProtocolCommand(string[] args) : CommandLineOption(args)
{
    public override string LongName => "protocol";
    public override string ShortName => null;
    public override string Description =>
        "Enables/disables MCCompiled's protocol URL so the web editor can open it. Pass 'true' to install, pass 'false' to uninstall.";
    public override bool IsRunnable => true;
    public override Range ArgCount => Range.Of(1);
    public override string[] ArgNames => ["true/false"];
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new ManageProtocolCommand(args); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        string arg1 = this.inputArgs[0];
        var config = new RegistryConfiguration();

        if (arg1.Equals("true", StringComparison.OrdinalIgnoreCase))
            config.Install();
        else if (arg1.Equals("false", StringComparison.OrdinalIgnoreCase))
            config.Uninstall();
        else
            Console.Error.WriteLine("Expected argument to be 'true' or 'false'; got '{0}'.", arg1);

        files = null; // prevent further execution
    }
}