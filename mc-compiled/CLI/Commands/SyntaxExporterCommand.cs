using System;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Language.SyntaxExporter;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI.Commands;

public class SyntaxExporterCommand(string[] inputArgs) : CommandLineOption(inputArgs)
{
    public override string LongName => "syntax";
    public override string ShortName => null;
    public override string Description =>
        "Export language information into a file. Not specifying an exporter will list them off.";
    public override bool IsRunnable => true;
    public override Range ArgCount => new(0, 1);
    public override string[] ArgNames => ["exporter"];
    public override CommandLineOption CreateNewWithArgs(string[] args) { return new SyntaxExporterCommand(args); }
    public override void Run(WorkspaceManager workspaceManager,
        Context context,
        CommandLineOption[] allNonRunnableOptions,
        ref string[] files)
    {
        files = null; // prevent further execution
        string _target = this.inputArgs.Length == 1 ? null : this.inputArgs[1];
        ConsoleColor originalColor = Console.ForegroundColor;

        if (_target == "*")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Exporting all output targets...");
            Console.ForegroundColor = ConsoleColor.White;

            foreach (SyntaxExporter exporter in SyntaxExporters.AllExporters)
            {
                Console.WriteLine("\tExporting target '{0}'... ({1})", exporter.Identifier, exporter.FileName);
                exporter.ExportToFile();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Completed exporting for {0} output targets.",
                SyntaxExporters.AllExporters.Count);
            Console.ForegroundColor = originalColor;
            return;
        }

        SyntaxExporter target = null;
        if (_target != null)
        {
            Console.WriteLine("Looking up target '{0}'...", _target);
            target = SyntaxExporters.GetExporter(_target);
        }

        if (target == null)
        {
            Console.WriteLine("Available Syntax Targets");

            Console.Write("\t*: ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("All available output targets individually.");
            Console.ForegroundColor = originalColor;

            foreach (SyntaxExporter exporter in SyntaxExporters.AllExporters)
            {
                Console.Write("\t{0}: ", exporter.Identifier);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(exporter.Description);
                Console.ForegroundColor = originalColor;
            }

            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Exporting target '{0}'...", target.Identifier);

        target.ExportToFile();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Completed. Output file: {0}", target.FileName);
        Console.ForegroundColor = originalColor;
    }
}