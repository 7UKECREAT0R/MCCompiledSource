using System.Collections.Generic;
using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;

namespace mc_compiled.MCC.Compiler.Async
{
    public class AsyncManager
    {
        public AsyncFunction CurrentFunction { get; private set; }
        public bool IsInAsync => this.CurrentFunction != null;
        
        private CommandFile tickFile;
        public readonly Executor parent;
        
        public AsyncManager(Executor parent)
        {
            this.CurrentFunction = null;
            this.parent = parent;
        }

        private void RegisterNewFunction(AsyncFunction function)
        {
            if (this.tickFile == null)
            {
                this.tickFile = new CommandFile(true, "tickAll", $"{Executor.MCC_GENERATED_FOLDER}/async");
                this.parent.AddExtraFile(this.tickFile);

                if (Program.DECORATE)
                {
                    this.tickFile.Add("# Runs via tick.json; ticks the state of every running async function.");
                    this.tickFile.Add(string.Empty);
                }
            }

            string command;
            if (function.target == AsyncTarget.Local)
            {
                Selector selector = new Selector(Selector.Core.e);
                selector.scores.checks.Add(new ScoresEntry(function.runningValue.InternalName, Range.Of(1)));
                command = Command.Execute().As(selector).AtSelf().Run(Command.Function(function.tickPrerequisite));
            }
            else
            {
                command = Command.Execute().IfScore(function.runningValue, Range.Of(1))
                    .Run(Command.Function(function.tickPrerequisite));
            }
            
            this.tickFile.Add(command);
        }
        public AsyncFunction StartNewAsyncFunction(string name, AsyncTarget target)
        {
            this.CurrentFunction = new AsyncFunction(this, name, target);
            RegisterNewFunction(this.CurrentFunction);
            return this.CurrentFunction;
        }
    }
}