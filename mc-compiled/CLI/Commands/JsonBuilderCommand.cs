using mc_compiled.Json;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class JsonBuilderCommand() : CommandLineOption([])
{
    public override string LongName => "jsonbuilder";
    public override string ShortName => null;
    public override string Description => "Opens a command-line based UI for building JSON rawtext.";
    public override bool IsHiddenFromHelp => true; // not super useful, but have it still be an option that exists.
    public override bool IsRunnable => true;
    public override Range ArgCount => Range.zero;
    public override string[] ArgNames => null;
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new JsonBuilderCommand(); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        Definitions.TryInitialize(context.debug);
        var builder = new RawTextJsonBuilder();
        builder.ConsoleInterface();
        files = null; // prevent further execution
    }
}