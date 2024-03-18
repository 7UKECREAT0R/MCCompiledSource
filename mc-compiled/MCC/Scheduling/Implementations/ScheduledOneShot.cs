using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Compiler.TypeSystem;

namespace mc_compiled.MCC.Scheduling.Implementations
{
    /// <summary>
    /// A scheduled task that occurs after a given period and terminates.
    /// </summary>
    public class ScheduledOneShot : ScheduledTask
    {
        private const string FUNCTION = "one_shot";
        private ScoreboardValue trigger;
        private readonly string[] commands;
        private readonly bool global;
        private readonly int tickDelay;

        /// <summary>
        /// The command to run to run `commands`. Set during setup, and is either a function call or the command itself.
        /// </summary>
        private string callCommand;

        /// <summary>
        /// Gets the command needed to start this one-shot for the executing entity (or globally, if global is set.)
        /// </summary>
        /// <returns></returns>
        public string Run()
        {
            return Command.ScoreboardSet(this.trigger, this.tickDelay);
        }
        
        public ScheduledOneShot(string[] commands, int tickDelay, bool global) : base(null)
        {
            Debug.Assert(commands != null, "commands was null");
            Debug.Assert(commands.Length > 0, "commands was empty");

            this.functionName = FUNCTION;
            this.commands = commands;
            this.tickDelay = tickDelay;
            this.global = global;
        }
        public override void Setup(TickScheduler scheduler, Executor executor)
        {
            string scoreboardName = FUNCTION + "_timer_" + GetHashCode().ToString().Replace('-', '0');
            this.trigger = new ScoreboardValue(scoreboardName, this.global, Typedef.INTEGER, executor.scoreboard);
            executor.AddCommandsInit(this.trigger.CommandsDefine());
            if(this.global)
                executor.AddCommandInit(Command.ScoreboardSet(this.trigger, -1));
            
            if (this.commands.Length == 1)
                this.callCommand = this.commands[0];
            else
            {
                string callFunctionName = FUNCTION + "_invoke_" + GetHashCode().ToString().Replace('-', '0');
                var file = new CommandFile(true, callFunctionName, TickScheduler.FOLDER);
                executor.AddExtraFile(file);
                this.callCommand = Command.Function(file);
            }
        }
        public override string[] PerTickCommands()
        {
            if (this.global)
            {
                return new[]
                {
                    Command.Execute().IfScore(this.trigger, new Range(0, null)).Run(Command.ScoreboardSubtract(this.trigger, 1)),
                    Command.Execute().IfScore(this.trigger, Range.Of(0)).Run(this.callCommand),
                };
            }

            return new[]
            {
                Command.Execute().As(Selector.ALL_ENTITIES).IfScore(this.trigger, new Range(0, null)).Run(Command.ScoreboardSubtract(this.trigger, 1)),
                Command.Execute().As(Selector.ALL_ENTITIES).IfScore(this.trigger, Range.Of(0)).Run(this.callCommand)
            };
        }

        public override int GetHashCode()
        {
            return FUNCTION.GetHashCode() ^ ((IStructuralEquatable) this.commands).GetHashCode(EqualityComparer<int>.Default);
        }
    }
}