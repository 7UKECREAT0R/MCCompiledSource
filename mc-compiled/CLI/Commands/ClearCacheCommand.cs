using System;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class ClearCacheCommand() : CommandLineOption([])
{
    public override string LongName => "clearcache";
    public override string ShortName => "cc";
    public override string Description => "Clear's MCCompiled's temporary file cache.";
    public override bool IsRunnable => true;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new ClearCacheCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        Console.WriteLine("Clearing temporary cache...");
        TemporaryFilesManager.ClearCache();
        Console.WriteLine("Successfully cleared the temporary cache.");
        files = null; // prevent further execution
    }
}