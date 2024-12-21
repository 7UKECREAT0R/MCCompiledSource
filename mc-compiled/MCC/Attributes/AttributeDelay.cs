namespace mc_compiled.MCC.Attributes;
/*public class AttributeDelay : IAttribute
{
    private readonly ScheduledOneShot task;

    internal AttributeDelay(string[] commands, int tickDelay)
    {
        task = new ScheduledOneShot(commands, tickDelay, false);
    }

    public string GetDebugString() => "delay";

    public void OnAddedValue(ScoreboardValue value, Statement causingStatement)
        => throw new StatementException(causingStatement, "Cannot apply attribute 'delay' to a value.");
    public void OnAddedFunction(RuntimeFunction function, Statement causingStatement)
    {
        function.file.AsInUse();
        Executor executor = causingStatement.executor;
        TickScheduler scheduler = executor.GetScheduler();
        scheduler.ScheduleTask(task);
    }

    public void OnCalledFunction(RuntimeFunction function,
        List<string> commands, Executor executor, Statement statement)
    {
        commands.Insert(0, task.Run());
    }
}*/