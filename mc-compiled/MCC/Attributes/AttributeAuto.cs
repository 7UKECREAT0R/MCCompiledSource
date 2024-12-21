using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.MCC.Scheduling;
using mc_compiled.MCC.Scheduling.Implementations;

namespace mc_compiled.MCC.Attributes;

/// <summary>
///     Automatically runs a function every tick/interval.
/// </summary>
public class AttributeAuto : IAttribute
{
    private readonly int tickDelay;
    private ScheduledTask task;
    internal AttributeAuto(int tickDelay)
    {
        this.tickDelay = tickDelay;
    }

    private string TickDelayOptimized
    {
        get
        {
            if (this.tickDelay % 1200 == 0)
                return $"{this.tickDelay / 1200}m";
            if (this.tickDelay % 20 == 0)
                return $"{this.tickDelay / 20}s";
            return this.tickDelay.ToString();
        }
    }

    public string GetDebugString()
    {
        return $"auto: {this.tickDelay} ticks";
    }
    public string GetCodeRepresentation()
    {
        return this.tickDelay < 2 ? "auto" : $"auto({this.TickDelayOptimized})";
    }

    public void OnAddedValue(ScoreboardValue value, Statement causingStatement)
    {
        throw new StatementException(causingStatement, "Cannot apply attribute 'auto' to a value.");
    }

    public void OnAddedFunction(RuntimeFunction function, Statement causingStatement)
    {
        // cannot add if function has parameters.
        if (function.ParameterCount > 0)
            throw new StatementException(causingStatement,
                "Cannot apply attribute 'auto' to a function with parameters.");

        if (this.tickDelay < 2)
            this.task = new ScheduledRepeatEveryTick(function.file);
        else
            this.task = new ScheduledRepeat(function.file, this.tickDelay);

        function.file.AsInUse();

        Executor executor = causingStatement.executor;
        TickScheduler scheduler = executor.GetScheduler();
        scheduler.ScheduleTask(this.task);
    }

    public void OnCalledFunction(RuntimeFunction function,
        List<string> commands, Executor executor, Statement statement) { }
}