using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC.Scheduling.Implementations
{
    /// <summary>
    /// Makes a function repeat every tick.
    /// </summary>
    public class ScheduledRepeatEveryTick : ScheduledTask
    {
        private readonly CommandFile function;

        public ScheduledRepeatEveryTick(CommandFile function) : base(null)
        {
            this.function = function;
        }

        public override void Setup(TickScheduler scheduler, Executor executor)
        {
            scheduler.tickJSONEntries.Add(function.CommandReference);
        }
        public override string[] PerTickCommands() => null;
    }
}