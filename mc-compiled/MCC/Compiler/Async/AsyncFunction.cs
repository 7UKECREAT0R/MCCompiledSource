﻿using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.Commands;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Functions.Types;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Compiler.Async;

public class AsyncFunction : RuntimeFunction
{
    internal static readonly string FOLDER = $"{Executor.MCC_GENERATED_FOLDER}/async";
    internal readonly string actualInternalName;

    /// <summary>
    ///     The escaped string representing the function name that will be used in all related identifiers.
    /// </summary>
    internal readonly string escapedFunctionName;
    private readonly List<List<AsyncStage>> groups;

    private readonly AsyncManager parent;
    /// <summary>
    ///     The entity that should be targeted by this async function.
    ///     If global, use the global fake-player to hold the state.
    ///     If local, use the running entity to hold the state.
    /// </summary>
    internal readonly AsyncTarget target;
    /// <summary>
    ///     List of async functions this function waits on. Used to prevent deadlocks at compile time.
    /// </summary>
    private readonly HashSet<AsyncFunction> waitsOn;

    internal ScoreboardValue runningValue;
    internal ScoreboardValue stageValue;
    private CommandFile tick;
    internal CommandFile tickPrerequisite;
    internal ScoreboardValue timerValue;
    public AsyncFunction(Statement statement, string name, string internalName, string documentation,
        IAttribute[] attributes, AsyncManager parent, AsyncTarget target) :
        base(statement, name, internalName, documentation, attributes)
    {
        this.parent = parent;
        this.actualInternalName = internalName;
        this.escapedFunctionName = EscapeFunctionName(internalName);
        this.target = target;
        this.waitsOn = [];

        // initialize the groups
        this.groups = [];
        StartNewGroup();

        InitializeScoreboardValues(this.parent.parent.scoreboard);
        InitializeCommandFiles();
    }

    private Executor Executor => this.parent.parent;
    internal int NextStageIndex => this.groups.Sum(group => group.Count);

    /// <summary>
    ///     Returns the index of the active stage.
    /// </summary>
    internal AsyncStage ActiveStage { get; private set; }

    /// <summary>
    ///     Gets the current group of stages in the async function.
    /// </summary>
    /// <remarks>
    ///     This property returns the current group of stages in the async function.
    ///     A group of stages represents a logical unit of execution within the async function.
    /// </remarks>
    private List<AsyncStage> CurrentGroup => this.groups[^1];

    public override Action<Executor> BlockOpenAction => _ =>
    {
        // starts the first async stage
        StartNewStage();
    };
    public override Action<Executor> BlockCloseAction => _ =>
    {
        // stop this async function and remove it from the parent
        this.parent.StopAsyncFunction();
        PopulateTickFunction();
        InitializeCallCommands();
    };

    public override string Returns => "awaitable";
    private static string EscapeFunctionName(string functionName)
    {
        return functionName.Replace('/', '_').Replace('.', '_');
    }
    private static string NameTickPrerequisiteFunction(string functionName)
    {
        return $"preTick_{functionName}";
    }
    private static string NameTickFunction(string functionName)
    {
        return $"tick_{functionName}";
    }

    private static string NameRunningObjective(string functionName)
    {
        return $"_asyncRunning_{functionName}";
    }
    private static string NameStageObjective(string functionName)
    {
        return $"_asyncStage_{functionName}";
    }
    private static string NameTimerObjective(string functionName)
    {
        return $"_asyncTimer_{functionName}";
    }

    /// <summary>
    ///     Determines whether this <see cref="AsyncFunction" /> waits on the specified async function.
    /// </summary>
    /// <param name="other">The <see cref="AsyncFunction" /> to check if it is waited upon.</param>
    /// <returns>
    ///     True if this <see cref="AsyncFunction" /> waits on the specified <see cref="AsyncFunction" />, otherwise
    ///     false.
    /// </returns>
    private bool WaitsOn(AsyncFunction other)
    {
        return this.waitsOn.Contains(other);
    }
    /// <summary>
    ///     Adds a specified <see cref="AsyncFunction" /> to the list of async functions that this function waits on.
    /// </summary>
    /// <param name="other">The <see cref="AsyncFunction" /> to add to the list of waits on.</param>
    /// <param name="callingStatement">The <see cref="Statement" /> to use for exceptions.</param>
    /// <exception cref="StatementException">If a deadlock is detected.</exception>
    public void AddWaitsOn(AsyncFunction other, Statement callingStatement)
    {
        this.waitsOn.Add(other);

        if (other.WaitsOn(this))
            throw new StatementException(callingStatement,
                $"Function '{other}' also awaits this function, potentially causing a deadlock.");
    }
    /// <summary>
    ///     Pushes a new stage group onto the stack of stages.
    /// </summary>
    internal void StartNewGroup()
    {
        var stageList = new List<AsyncStage>();
        this.groups.Add(stageList);
    }

    private void InitializeScoreboardValues(ScoreboardManager manager)
    {
        bool global = this.target == AsyncTarget.Global;
        this.runningValue = new ScoreboardValue(NameRunningObjective(this.escapedFunctionName), global, Typedef.BOOLEAN,
            manager);
        this.stageValue =
            new ScoreboardValue(NameStageObjective(this.escapedFunctionName), global, Typedef.INTEGER, manager);
        this.timerValue =
            new ScoreboardValue(NameTimerObjective(this.escapedFunctionName), global, Typedef.TIME, manager);

        this.parent.parent.AddCommandsInit(this.runningValue.CommandsDefine());
        this.parent.parent.AddCommandsInit(this.stageValue.CommandsDefine());
        this.parent.parent.AddCommandsInit(this.timerValue.CommandsDefine());
    }
    private void InitializeCommandFiles()
    {
        this.tickPrerequisite = new CommandFile(false,
            NameTickPrerequisiteFunction(this.escapedFunctionName), FOLDER);
        this.tick = new CommandFile(false,
            NameTickFunction(this.escapedFunctionName), FOLDER);

        this.parent.parent.AddExtraFile(this.tickPrerequisite);
        this.parent.parent.AddExtraFile(this.tick);
        this.file.RegisterCall(this.tickPrerequisite);
        this.file.RegisterCall(this.tick);

        this.tickPrerequisite.Add([
            // if timer == 0: stage++
            Command.Execute().IfScore(this.timerValue, Range.Of(0)).Run(Command.ScoreboardAdd(this.stageValue, 1)),
            // if timer >= 0: timer--
            Command.Execute().IfScore(this.timerValue, new Range(0, null))
                .Run(Command.ScoreboardSubtract(this.timerValue, 1)),
            // if timer == -1: tick()
            Command.Execute().IfScore(this.timerValue, Range.Of(-1)).Run(Command.Function(this.tick))
        ]);
    }
    private void InitializeCallCommands()
    {
        // these commands are NOT what get run when you call the function in MCCompiled; this is only here to
        // facilitate if the function gets run in-game.

        if (this.groups.Count == 0)
        {
            AddCommand("# No async code was added to this function.");
            return;
        }

        List<AsyncStage> validGroup = this.groups.FirstOrDefault(group => group.Count > 0);

        if (validGroup == null)
        {
            AddCommand("# No async code was added to this function.");
            return;
        }

        AddCommand(Command.Function(validGroup[0].file));
    }
    private void PopulateTickFunction()
    {
        foreach (List<AsyncStage> group in this.groups)
        foreach (AsyncStage stage in group)
        {
            bool stageSetsTimer = stage.HasTickDelay;
            string command;

            if (stageSetsTimer)
                // this stage may have been run by the previous stage internally, so to prevent it
                // from being called twice, make sure it didn't set a timer for incrementation.
                command = Command.Execute()
                    .IfScore(this.stageValue, Range.Of(stage.index))
                    .IfScore(this.timerValue, Range.Of(-1))
                    .Run(Command.Function(stage.file));
            else
                command = Command.Execute()
                    .IfScore(this.stageValue, Range.Of(stage.index))
                    .Run(Command.Function(stage.file));

            this.tick.Add(command);
        }
    }
    public override void TryReturnValue(ScoreboardValue value, Executor executor, Statement caller)
    {
        throw new StatementException(caller, "Async functions cannot return values.");
    }
    public override void TryReturnValue(TokenLiteral value, Statement caller, Executor executor)
    {
        throw new StatementException(caller, "Async functions cannot return values.");
    }
    public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor,
        Statement statement)
    {
        // add the file to the executor if it hasn't been yet.
        if (!this.isAddedToExecutor && !this.isExtern)
        {
            executor.AddExtraFile(this.file);
            this.isAddedToExecutor = true;
        }

        if (this.groups.Count == 0)
        {
            commandBuffer.Add(Command.Function(this.file));
            return new TokenAwaitable(this, statement.Lines[0]);
        }

        commandBuffer.Add(Command.Function(this.groups[0][0].file));

        // apply attributes
        foreach (IAttribute attribute in this.attributes)
            attribute.OnCalledFunction(this, commandBuffer, executor, statement);

        return new TokenAwaitable(this, statement.Lines[0]);
    }

    /// <summary>
    ///     Starts a new stage in this async function.
    /// </summary>
    public void StartNewStage()
    {
        int index = this.NextStageIndex;
        if (this.ActiveStage != null)
            throw new Exception("Error while processing async: Started new stage without ending previous one.");
        this.ActiveStage = new AsyncStage(this, index);
        this.ActiveStage.Begin(this.Executor);
        this.CurrentGroup.Add(this.ActiveStage);
        this.file.RegisterCall(this.ActiveStage.file);
    }
    /// <summary>
    ///     Finish the last stage by terminating the async state machine.
    /// </summary>
    public void TerminateStage()
    {
        this.ActiveStage.FinishTerminate(this.Executor);
        this.ActiveStage = null;
    }
    /// <summary>
    ///     Finish the current stage of the async function.
    /// </summary>
    /// <param name="tickDelayUntilNext">The number of ticks to delay before moving to the next stage.</param>
    public void FinishStage(int tickDelayUntilNext)
    {
        this.ActiveStage.SetTickDelay(tickDelayUntilNext);
        this.ActiveStage.Finish(this.Executor);
        this.ActiveStage = null;
    }
    /// <summary>
    ///     Finish the current stage of the async function immediately in the same tick.
    /// </summary>
    public void FinishStageImmediate()
    {
        this.ActiveStage.RemoveTickDelay();
        this.ActiveStage.Finish(this.Executor);
        this.ActiveStage = null;
    }
    /// <summary>
    ///     Finish the currently active stage of the async function.
    /// </summary>
    /// <param name="comparisonToGoToNext">The comparison that must be met to transition to the next stage.</param>
    /// <param name="callingStatement">The statement to blame when everything blows up.</param>
    public void FinishStage(ComparisonSet comparisonToGoToNext, Statement callingStatement)
    {
        this.ActiveStage.FinishUnderCondition(this.Executor, comparisonToGoToNext, callingStatement);
        this.ActiveStage = null;
    }

    /// <summary>
    ///     Returns the commands needed to halt the execution of this async function.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> CommandsHalt()
    {
        return
        [
            Command.ScoreboardSet(this.runningValue, 0),
            Command.ScoreboardSet(this.timerValue, -1),
            Command.ScoreboardSet(this.stageValue, -1)
        ];
    }
    /// <summary>
    ///     Sends information about this function's groups and stages to stdout.
    /// </summary>
    /// <returns></returns>
    internal void PrintDebugGroupInfo()
    {
        Console.WriteLine($"Group information for async function '{this.internalName}' ({this.groups.Count} groups):");

        int groupNumber = 1;
        int stageNumber = 1;

        foreach (List<AsyncStage> group in this.groups)
        {
            Console.WriteLine($"\tGroup {groupNumber++} ({group.Count} stages):");

            foreach (AsyncStage stage in group)
            {
                string[] commands = stage.file.commands.ToArray();
                Console.WriteLine($"\t\tStage {stageNumber++} ({commands.Length} commands):");
                foreach (string command in commands)
                    Console.WriteLine($"\t\t\t/{command}");
            }
        }
    }

    private bool Equals(AsyncFunction other)
    {
        return this.actualInternalName == other.actualInternalName && this.target == other.target;
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((AsyncFunction) obj);
    }
    public override int GetHashCode()
    {
        unchecked
        {
            return ((this.actualInternalName != null ? this.actualInternalName.GetHashCode() : 0) * 397) ^
                   (int) this.target;
        }
    }
}

public enum AsyncTarget
{
    Local,
    Global
}