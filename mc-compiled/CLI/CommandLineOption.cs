using System;
using System.Linq;
using JetBrains.Annotations;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.CLI;

public abstract class CommandLineOption(string[] inputArgs)
{
    /// <summary>
    ///     The arguments inputted into this command.
    /// </summary>
    protected readonly string[] inputArgs = inputArgs;

    /// <summary>
    ///     If this option hidden from the help menu?
    /// </summary>
    public virtual bool IsHiddenFromHelp => false;
    /// <summary>
    ///     The long name of this option, specified with two hyphens. <c>--{LongName}</c>.
    /// </summary>
    /// <remarks>NOTE: This string itself should not contain any hyphens! They'll be automatically inserted as needed.</remarks>
    [NotNull]
    public abstract string LongName { get; }
    /// <summary>
    ///     The optional short name of this option, specified with one hyphens. <c>-{ShortName}</c>
    /// </summary>
    /// <remarks>NOTE: This string itself should not contain any hyphens! They'll be automatically inserted as needed.</remarks>
    [CanBeNull]
    public abstract string ShortName { get; }

    /// <summary>
    ///     Gets the command line usage string for this option.
    /// </summary>
    public string CommandLineUsage
    {
        get
        {
            string optionString = this.ShortName == null
                ? $"--{this.LongName}"
                : $"[-{this.ShortName} | --{this.LongName}]";

            string[] argNames = this.ArgNames;
            if (argNames == null || argNames.Length == 0)
                return optionString;
            string argNamesString = string.Join(" ", argNames.Select(a => '[' + a + ']'));
            return $"{optionString} {argNamesString}";
        }
    }
    /// <summary>
    ///     A description of what this command does.
    /// </summary>
    [NotNull]
    public abstract string Description { get; }

    /// <summary>
    ///     If this command is runnable (true) or simply provides information <b>to</b> a runnable command. (false)
    /// </summary>
    public abstract bool IsRunnable { get; }
    /// <summary>
    ///     The number of args allowed by this command.
    /// </summary>
    public abstract Range ArgCount { get; }
    /// <summary>
    ///     An array of argument names required by this command.
    /// </summary>
    [CanBeNull]
    public abstract string[] ArgNames { get; }

    /// <summary>
    ///     Determines whether the given <paramref name="arg" /> matches the option's short or long name.
    /// </summary>
    /// <param name="arg">
    ///     The input string to evaluate as a potential match. It may begin with one or more hyphens, followed by the name to
    ///     check.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the given <paramref name="arg" /> matches either the <see cref="ShortName" /> or
    ///     <see cref="LongName" />
    ///     of this command-line option, based on the number of leading hyphens; otherwise, <c>false</c>.
    /// </returns>
    public bool DoesArgMatch(string arg)
    {
        int numOfHyphens = arg.TakeWhile(c => c == '-').Count();
        if (numOfHyphens == 0)
            return false;
        if (numOfHyphens == 1)
            return arg[1..].Equals(this.ShortName, StringComparison.OrdinalIgnoreCase);
        return numOfHyphens == 2 && arg[2..].Equals(this.LongName, StringComparison.OrdinalIgnoreCase);
    }
    /// <summary>
    ///     Create a new instance of this CommandLineOption with the given <see cref="inputArgs" />
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract CommandLineOption CreateNewWithArgs(string[] args);

    /// <summary>
    ///     Depending on <see cref="IsRunnable" />:
    ///     <ul>
    ///         <li>If <c>true</c>: Run.</li>
    ///         <li>If <c>false</c>: Parse the input args into this option's fields, if any.</li>
    ///     </ul>
    /// </summary>
    /// <param name="workspaceManager">The workspace manager to modify, if needed.</param>
    /// <param name="context">The context to modify, if needed.</param>
    /// <param name="allNonRunnableOptions">
    ///     If this command <see cref="IsRunnable" />, this is a list of all non-runnable
    ///     options which were passed in alongside it.
    /// </param>
    /// <param name="files">The files which will be compiled. This can be modified.</param>
    public abstract void Run(WorkspaceManager workspaceManager,
        Context context,
        [CanBeNull] CommandLineOption[] allNonRunnableOptions,
        ref string[] files);
}