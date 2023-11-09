using System;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC.Scheduling
{
    /// <summary>
    /// A scheduled task that occurs after a given preiod and terminates.
    /// </summary>
    public sealed class ScheduledOneShot : ScheduledTask
    {
        readonly string command;
        readonly int tickDelay;

        public ScheduledOneShot(string command, int tickDelay, string functionName) : base(functionName)
        {
            this.command = command;
            this.tickDelay = tickDelay;
        }
        public override void Setup(Executor executor)
        {
            
        }
        public override string[] PerTickCommands()
        {
            throw new NotImplementedException();
        }
    }
}
