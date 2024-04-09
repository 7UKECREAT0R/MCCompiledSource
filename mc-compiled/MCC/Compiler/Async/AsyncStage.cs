using mc_compiled.Commands;

namespace mc_compiled.MCC.Compiler.Async
{
    /// <summary>
    /// Represents a stage in an async function.
    /// </summary>
    public class AsyncStage
    {
        private static readonly string FOLDER = $"{Executor.MCC_GENERATED_FOLDER}/async/stages/";
        private static string NameStageFunction(string functionName, int stage) =>
            $"{functionName}_{stage}";

        public readonly int index;
        private readonly bool isFirst;
        
        internal readonly CommandFile file;
        private readonly AsyncFunction parent;
        
        private int? ticksUntilNextStage;
        
        /// <summary>
        /// Sets the tick delay for the current stage.
        /// </summary>
        /// <param name="delay">The number of ticks to delay before moving to the next stage.</param>
        public void SetTickDelay(int delay)
        {
            this.ticksUntilNextStage = delay;
        }
        public void RemoveTickDelay()
        {
            this.ticksUntilNextStage = null;
        }
        public AsyncStage(AsyncFunction parent, int index)
        {
            this.parent = parent;
            this.index = index;
            this.isFirst = index == 0;

            string stageName = FOLDER + NameStageFunction(parent.escapedFunctionName, index);
            this.file = new CommandFile(false, stageName);
            this.file.MarkAsync();
            
            parent.file.RegisterCall(this.file);

            this.ticksUntilNextStage = null;
        }
        
        /// <summary>
        /// Begins this stage and applies it to the given <see cref="Executor"/>.
        /// </summary>
        public void Begin(Executor executor)
        {
            executor.PushFile(this.file);
        }
        /// <summary>
        /// Finishes this stage by terminating the async state machine.
        /// </summary>
        /// <param name="executor"></param>
        public void FinishTerminate(Executor executor)
        {
            executor.PopFirstFile(file => file.IsAsync);
            
            if (Program.DECORATE)
            {
                if (this.file.Length > 0)
                    this.file.Add("");
                this.file.Add($"# End of the last async stage: {this.index}.");
            }
            
            this.file.Add(Command.ScoreboardSet(this.parent.runningValue, 0));
        }
        /// <summary>
        /// Finishes this stage by adding the closing commands and popping it from the executor. Make sure there's no
        /// other files in the way that would get popped instead.
        /// </summary>
        public void Finish(Executor executor)
        {
            if (Program.DECORATE)
            {
                if (this.file.Length > 0)
                    this.file.Add("");

                this.file.Add($"# End of async stage {this.index}.");
                this.file.Add($"# Waits {this.ticksUntilNextStage} ticks until next stage.");
            }

            if(this.isFirst)
                executor.AddCommandClean(Command.ScoreboardSet(this.parent.runningValue, 1));

            if (this.ticksUntilNextStage.HasValue)
            {
                executor.AddCommandsClean(new[]
                {
                    Command.ScoreboardSet(this.parent.timerValue, this.ticksUntilNextStage.Value),
                    Command.ScoreboardSet(this.parent.stageValue, this.index)
                }, null, null, true);
                return;
            }

            // immediately jump to the next stage
            executor.AddCommandsClean(new[]
            {
                Command.ScoreboardSet(this.parent.timerValue, -1),
                Command.ScoreboardSet(this.parent.stageValue, this.index + 1)
            }, null, null, true);


            executor.PopFile();
        }
        /// <summary>
        /// Finishes this stage if the given condition is met.
        /// </summary>
        /// <param name="executor">The executor</param>
        /// <param name="conditions">The conditions to check</param>
        /// <param name="callingStatement">The calling statement</param>
        public void FinishUnderCondition(Executor executor, ComparisonSet conditions, Statement callingStatement)
        {
            if (Program.DECORATE)
            {
                if (this.file.Length > 0)
                    this.file.Add("");

                this.file.Add($"# End of async stage {this.index}.");
                this.file.Add($"# Continues if the following condition evaluates to true:");
                this.file.Add($"#     " + conditions.GetDescription());
            }

            string command = this.ticksUntilNextStage.HasValue ?
                Command.ScoreboardSet(this.parent.timerValue, this.ticksUntilNextStage.Value) :
                Command.ScoreboardSet(this.parent.stageValue, this.index + 1);
            
            conditions.RunCommand(command, executor, callingStatement);
            executor.PopFile();
        }

        public override string ToString() => $"stage {this.index}: {this.file.commands.Count} commands";
    }
}