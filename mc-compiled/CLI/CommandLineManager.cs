using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using mc_compiled.CLI.Commands;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.CLI;

/// <summary>
///     Handles available command-line commands and dishes them out.
/// </summary>
public static class CommandLineManager
{
    private static readonly CommandLineOption[] OPTIONS =
    [
        new ClearCacheCommand(), // --clearcache
        new EnableDebuggingCommand(), // --debug
        new EnableDecorationCommand(), // --decorate
        new EnableExportAllCommand(), // --exportall
        new EnableRegolithOption(), // --regolith
        new EnableSearchOption(), // --search
        new EnableTraceOutputCommand(), // --trace
        new GenerateManifestWizardCommand(null), // --generatemanifest [project name]
        new IgnoreManifestsCommand(), // --ignoremanifests
        new InfoCommand(), // --info
        new JsonBuilderCommand(), // --jsonbuilder
        new ManageProtocolCommand(null), // --protocol <true/false>
        new OutputBehaviorLocationCommand(null), // --outputbp <location>
        new OutputDevelopmentFoldersCommand(), // --outputdevelopment
        new OutputResourceLocationCommand(null), // --outputrp <location>
        new ServerCommand(), // --server
        new ServerFromProtocolCommand(), // --fromprotocol
        new SetInputPPVCommand(null), // --variable
        new SetProjectNameCommand(null), // --project
        new SyntaxExporterCommand(null), // --syntax
        new TestLootCommand(), // --testloot
        new VersionCommand() // --version
    ];

    /// <summary>
    ///     Processes command-line arguments, parses options, and executes specific actions such as compiling files
    ///     based on the provided input.
    /// </summary>
    /// <param name="_args">
    ///     The array of command-line arguments passed to the application. This parameter is used to determine options,
    ///     files to compile, or whether to display help information.
    /// </param>
    public static void Run(string[] _args)
    {
        if (_args.Length < 1 || _args[0].Equals("--help", StringComparison.OrdinalIgnoreCase))
        {
            Help();
            return;
        }

        // give us a stack we can pop from to get fresh args
        Stack<string> args = new();
        for (int i = _args.Length - 1; i >= 0; i--)
            args.Push(_args[i]);

        string[] filesToCompile;
        string firstArg = args.Count > 0 ? args.Peek() : null;
        if (firstArg != null && !firstArg.StartsWith('-'))
            filesToCompile = [args.Pop()];
        else
            filesToCompile = [];

        // parse the whole command into options first.
        List<CommandLineOption> options = [];

        while (args.TryPeek(out string arg))
            if (arg.StartsWith('-'))
            {
                args.Pop();
                CommandLineOption matchingOption =
                    OPTIONS.FirstOrDefault(o => o.DoesArgMatch(arg));
                if (matchingOption == null)
                {
                    Executor.Warn($"Ignoring input argument: \"{arg}\" as it was not an available option.");
                    continue;
                }

                // pull args until the next one starts with a '-'
                List<string> optionArgs = [];
                while (args.TryPeek(out string argArg))
                {
                    if (argArg.StartsWith('-'))
                        break;
                    optionArgs.Add(args.Pop());
                }

                int numberOfOptions = optionArgs.Count;
                if (!matchingOption.ArgCount.Contains(numberOfOptions))
                {
                    Console.Error.WriteLine($"Invalid number of arguments for option \"{matchingOption.LongName}\". " +
                                            $"Expected {matchingOption.ArgCount.ToString()} but got {numberOfOptions}.");
                    return;
                }

                CommandLineOption clonedOption = matchingOption.CreateNewWithArgs(optionArgs.ToArray());
                options.Add(clonedOption);
            }
            else
            {
                Executor.Warn($"Ignoring input argument: \"{arg}\" as it was not part of a command.");
                args.Pop();
            }

        // setup stuff
        ContextContract context = GlobalContext.NewInherit();
        var workspaceManager = new WorkspaceManager();

        // run any non-runnable options
        CommandLineOption[] nonRunnableOptions = options.Where(o => !o.IsRunnable).ToArray();
        foreach (CommandLineOption nonRunnableOption in nonRunnableOptions)
            nonRunnableOption.Run(workspaceManager, context.heldContext, null, ref filesToCompile);

        // see if a runnable option was passed.
        CommandLineOption firstRunnableOption = options.FirstOrDefault(o => o.IsRunnable);
        if (firstRunnableOption != null)
        {
            firstRunnableOption.Run(workspaceManager, context.heldContext, nonRunnableOptions, ref filesToCompile);

            if (filesToCompile == null || filesToCompile.Length == 0)
                return; // no files to compile, so stop execution here.
        }

        // parse definitions.def
        Definitions.TryInitialize(context.heldContext.debug);

        if (filesToCompile == null || filesToCompile.Length == 0)
        {
            Console.Error.WriteLine("No file was given to compile.");
            return;
        }

        // generally will be only one file, but the --search or --regolith options could cause more.
        foreach (string fileToCompile in filesToCompile)
        {
            if (!File.Exists(fileToCompile))
            {
                Console.Error.WriteLine($"File \"{fileToCompile}\" does not exist.");
                return;
            }

            // reset static stuff because this is the ROOT of a compilation.
            WorkspaceManager.ResetStaticStates();

            // use the shiny new compilation API!
            workspaceManager.OpenFile(fileToCompile);
            bool success = workspaceManager.CompileFileWithSimpleErrorHandler(fileToCompile, false, false,
                out Emission emission);
            if (success)
                emission?.WriteAllFiles();
        }
    }
    private static void Help()
    {
        const string MAIN_COMMAND_NAME = "mccompiled";
        ConsoleColor originalColor = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("{0} --help", MAIN_COMMAND_NAME);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\tShow the help menu for this application.");
        Console.WriteLine();

        // print out all terminating options first.
        foreach (CommandLineOption commandLineOption in OPTIONS.Where(option => option.IsRunnable))
        {
            if (commandLineOption.IsHiddenFromHelp)
                continue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("{0} {1}", MAIN_COMMAND_NAME, commandLineOption.CommandLineUsage);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\t{0}", commandLineOption.Description);
            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("{0} <file> [options...]", MAIN_COMMAND_NAME);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\tCompile a .mcc file with the given options. Available options include:");

        // now print all non-terminating options, which are all modifiers built for the main command.
        foreach (CommandLineOption commandLineOption in OPTIONS.Where(option => !option.IsRunnable))
        {
            if (commandLineOption.IsHiddenFromHelp)
                continue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\t\t{0}", commandLineOption.CommandLineUsage);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\t\t\t{0}", commandLineOption.Description);
        }

        Console.ForegroundColor = originalColor;
    }
}