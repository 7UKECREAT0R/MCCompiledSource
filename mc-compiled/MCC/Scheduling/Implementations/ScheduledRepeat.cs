﻿using System;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Compiler.TypeSystem;

namespace mc_compiled.MCC.Scheduling.Implementations
{
    public class ScheduledRepeat : ScheduledTask
    {
        private const string MASTER_FUNCTION = "repeating_functions";
        private const string SCOREBOARD_PREFIX = "auto_counter_";
        private const string SUBFUNCTION_PREFIX = "auto_tick_";
        
        private readonly string counterName;
        private readonly CommandFile function;
        private readonly int delay;

        private CommandFile thisFunction;
        
        public ScheduledRepeat(CommandFile function, int delay) : base(MASTER_FUNCTION)
        {
            counterName = SCOREBOARD_PREFIX + function.CommandReference.GetHashCode();
            this.function = function;
            this.delay = delay;
        }

        public override void Setup(TickScheduler scheduler, Executor executor)
        {
            string subfunctionName = SUBFUNCTION_PREFIX + function.CommandReference.GetHashCode();
            thisFunction = new CommandFile(true, subfunctionName, TickScheduler.FOLDER);
            executor.AddExtraFile(thisFunction);

            var counter = new ScoreboardValue(counterName, true, Typedef.INTEGER, executor.scoreboard);
            executor.AddCommandsInit(counter.CommandsDefine());
            executor.AddCommandInit(Command.ScoreboardSet(counter, delay));
            
            thisFunction.Add(Command.Execute().IfScore(counter, new Range(0, null)).Run(Command.ScoreboardSubtract(counter, 1)));
            thisFunction.Add(Command.Execute().IfScore(counter, new Range(0, false)).Run(Command.Function(function.CommandReference)));
            thisFunction.Add(Command.Execute().IfScore(counter, new Range(0, false)).Run(Command.ScoreboardSet(counter, delay)));
        }
        public override string[] PerTickCommands()
        {
            return new[] {Command.Function(thisFunction)};
        }
    }
}