using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

/// <summary>
///     Run the compilation with a preprocessor variable already set.
/// </summary>
/// <param name="input">The input arguments.</param>
public class SetInputPPVCommand(string[] input) : CommandLineOption(input)
{
    public override string LongName => "variable";
    public override string ShortName => "ppv";
    public override string Description => "Run the compilation with a preprocessor variable already set.";
    public override bool IsRunnable => false;
    public override Range ArgCount => Range.Of(2);
    public override string[] ArgNames => ["name", "value"];
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new SetInputPPVCommand(args); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        context.inputPPVs.Add(Parse());
    }

    /// <summary>
    ///     Parse the inputs to this command and return them as an <see cref="Context.InputPPV" />
    /// </summary>
    /// <returns></returns>
    public Context.InputPPV Parse() { return new Context.InputPPV(this.inputArgs[0], this.inputArgs[1]); }
}