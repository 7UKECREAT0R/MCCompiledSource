using System;
using System.IO;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class EnableSearchOption() : CommandLineOption([])
{
    public override string LongName => "search";
    public override string ShortName => null;
    public override string Description => "Search for and compile MCC files in all subdirectories.";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new EnableSearchOption(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.regolith = false;
        context.search = true;
        files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mcc", SearchOption.AllDirectories);

        if (files.Length == 0)
            Console.Error.WriteLine("No .MCC files found in the current directory.");
    }
}