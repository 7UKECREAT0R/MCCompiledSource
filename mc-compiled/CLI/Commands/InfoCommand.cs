using System;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class InfoCommand() : CommandLineOption([])
{
    public override string LongName => "info";
    public override string ShortName => null;
    public override string Description => "Prints out info about the compiler.";
    public override bool IsHiddenFromHelp => true; // used by the installer to query the currently installed version
    public override bool IsRunnable => true;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new InfoCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        Console.WriteLine("V1.{0}", Executor.MCC_VERSION);
        Console.WriteLine("L{0}", AppContext.BaseDirectory);
        files = null; // prevent further execution
    }
}