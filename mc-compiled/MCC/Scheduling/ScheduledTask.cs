using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC.Scheduling;

/// <summary>
///     Represents an abstract implementation of a task which is held in tick.json.
/// </summary>
public abstract class ScheduledTask
{
    /// <summary>
    ///     The name of this function. Placed under compiler/scheduler/{functionName}.
    ///     Any functions with the same name are appended together into one file.
    /// </summary>
    internal string functionName;
    /// <summary>
    ///     Unique identifier for this task. Set when sent through <see cref="TickScheduler.ScheduleTask" />.
    /// </summary>
    internal int id;

    protected ScheduledTask(string functionName)
    {
        this.functionName = functionName;
    }

    /// <summary>
    ///     Setup whatever is needed for this task.
    /// </summary>
    /// <param name="scheduler">The calling scheduler.</param>
    /// <param name="executor">The target executor to modify.</param>
    public abstract void Setup(TickScheduler scheduler, Executor executor);

    /// <summary>
    ///     The function to be run in tick.json that will presumably check/perform an action.
    ///     These commands will either be appended to the function called 'functionName' or added to a new one if it doesn't
    ///     yet exist.
    /// </summary>
    /// <returns></returns>
    public abstract string[] PerTickCommands();
}