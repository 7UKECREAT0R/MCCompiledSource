using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.MCC.Scheduling;
using mc_compiled.MCC.Scheduling.Implementations;

namespace mc_compiled.MCC.Attributes
{
    /// <summary>
    /// Automatically runs a function every tick/interval.
    /// </summary>
    public class AttributeAuto : IAttribute
    {
        private readonly int tickDelay;
        private ScheduledTask task;

        internal AttributeAuto(int tickDelay)
        {
            this.tickDelay = tickDelay;
        }
        
        public string GetDebugString() => "auto";

        public void OnAddedValue(ScoreboardValue value, Statement causingStatement)
            => throw new StatementException(causingStatement, "Cannot apply attribute 'auto' to a value.");
        
        public void OnAddedFunction(RuntimeFunction function, Statement causingStatement)
        {
            if (tickDelay < 2)
                task = new ScheduledRepeatEveryTick(function.file);
            else
                task = new ScheduledRepeat(function.file, tickDelay);

            function.file.AsInUse();
            
            Executor executor = causingStatement.executor;
            TickScheduler scheduler = executor.GetScheduler();
            scheduler.ScheduleTask(task);
        }

        public void OnCalledFunction(RuntimeFunction function,
            List<string> commands, Executor executor, Statement statement) {}
    }
}