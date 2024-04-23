using System.Collections.Generic;
using System.Linq;
using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Scheduling.Implementations;

namespace mc_compiled.MCC.Compiler.Async
{
    public class AsyncManager
    {
        public static StatementException UnsupportedException(Statement statement) =>
            throw new StatementException(statement, "Command is currently unsupported in async contexts, but being worked on.");
        public AsyncFunction CurrentFunction { get; private set; }
        public bool IsInAsync => this.CurrentFunction != null;

        private CommandFile tickFile;
        public readonly Executor parent;
        
        public AsyncManager(Executor parent)
        {
            this.CurrentFunction = null;
            this.parent = parent;
        }

        /// <summary>
        /// Attempts to build the tick file for all active async functions. Run after everything else is done.
        /// </summary>
        internal void TryBuildTickFile()
        {
            AsyncFunction[] asyncFunctions = this.parent.functions.FetchAll()
                .Where(f => f is AsyncFunction)
                .Cast<AsyncFunction>()
                .ToArray();

            foreach (AsyncFunction asyncFunction in asyncFunctions)
                asyncFunction.PrintDebugGroupInfo();
            
            if (!Program.EXPORT_ALL && !asyncFunctions.Any(f => f.file.IsInUse))
                return;
            
            if (this.tickFile == null)
            {
                this.tickFile = new CommandFile(true, "tick", $"{Executor.MCC_GENERATED_FOLDER}/async");
                this.parent.AddExtraFile(this.tickFile);
                
                // put in tick.json
                this.parent.GetScheduler()
                    .ScheduleTask(new ScheduledRepeatEveryTick(this.tickFile));
                
                if (Program.DECORATE)
                {
                    this.tickFile.Add("# Runs via tick.json; ticks the state of every running async function.");
                    this.tickFile.Add(string.Empty);
                }
            }

            foreach (AsyncFunction asyncFunction in asyncFunctions)
            {
                if (!Program.EXPORT_ALL && !asyncFunction.file.IsInUse)
                    return;
                
                string command;
                if (asyncFunction.target == AsyncTarget.Local)
                {
                    Selector selector = new Selector(Selector.Core.e);
                    selector.scores.checks.Add(new ScoresEntry(asyncFunction.runningValue.InternalName, Range.Of(1)));
                    command = Command.Execute().As(selector).AtSelf().Run(Command.Function(asyncFunction.tickPrerequisite));
                }
                else
                {
                    command = Command.Execute().IfScore(asyncFunction.runningValue, Range.Of(1))
                        .Run(Command.Function(asyncFunction.tickPrerequisite));
                }
            
                this.tickFile.Add(command);
            }
        }
        public AsyncFunction StartNewAsyncFunction(Statement callingStatement, string name, string internalName,
            string documentation, IAttribute[] attributes, AsyncTarget target)
        {
            this.CurrentFunction = new AsyncFunction(callingStatement, name, internalName, documentation, attributes, this, target);
            return this.CurrentFunction;
        }
        public void StopAsyncFunction()
        {
            if (this.CurrentFunction == null)
                return;
            
            this.CurrentFunction.TerminateStage();
            this.CurrentFunction = null;
        }
    }
}