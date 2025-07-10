using System;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class VersionCommand() : CommandLineOption([])
{
    public override string LongName => "version";
    public override string ShortName => null;
    public override string Description =>
        "Access this executable's MCCompiled version. Accessed through code with $compilerversion";
    public override bool IsRunnable => true;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new VersionCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        files = null; // prevent further execution
        Console.WriteLine("MCCompiled Version 1." + Executor.MCC_VERSION);
        Console.WriteLine("Andrew Criswell 2025");
    }
}