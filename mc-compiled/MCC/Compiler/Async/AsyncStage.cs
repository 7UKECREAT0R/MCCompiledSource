using mc_compiled.Commands;
using mc_compiled.Commands.Execute;

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

        private readonly int stageIndex;
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
        public AsyncStage(AsyncFunction parent, int stageIndex)
        {
            this.parent = parent;
            this.stageIndex = stageIndex;
            this.isFirst = stageIndex == 0;

            string stageName = FOLDER + NameStageFunction(parent.escapedFunctionName, stageIndex);
            this.file = new CommandFile(true, stageName);

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
            executor.PopFile();
            
            if (Program.DECORATE)
            {
                if (this.file.Length > 0)
                    this.file.Add("");
                this.file.Add($"# End of the last async stage: {this.stageIndex}.");
            }
            
            this.file.Add(Command.ScoreboardSet(this.parent.runningValue, 0));
        }
        /// <summary>
        /// Finishes this stage by adding the closing commands and popping it from the executor. Make sure there's no
        /// other files in the way that would get popped instead.
        /// </summary>
        public void Finish(Executor executor)
        {
            executor.PopFile();

            if (Program.DECORATE)
            {
                if (this.file.Length > 0)
                    this.file.Add("");

                this.file.Add($"# End of async stage {this.stageIndex}.");
                this.file.Add($"# Waits {this.ticksUntilNextStage} ticks until next stage.");
            }
            
            if(this.isFirst)
                this.file.Add(Command.ScoreboardSet(this.parent.runningValue, 1));
            
            if (this.ticksUntilNextStage.HasValue)
            {
                this.file.Add(Command.ScoreboardSet(this.parent.timerValue, this.ticksUntilNextStage.Value));
                this.file.Add(Command.ScoreboardSet(this.parent.stageValue, this.stageIndex));
                return;
            }
            
            // immediately jump to the next stage
            this.file.Add(Command.ScoreboardSet(this.parent.stageValue, this.stageIndex + 1));
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

                this.file.Add($"# End of async stage {this.stageIndex}.");
                this.file.Add($"# Continues if the following condition evaluates to true:");
                this.file.Add($"#     " + conditions.GetDescription());

            }

            string command = this.ticksUntilNextStage.HasValue ?
                Command.ScoreboardSet(this.parent.timerValue, this.ticksUntilNextStage.Value) :
                Command.ScoreboardSet(this.parent.stageValue, this.stageIndex + 1);
            
            conditions.RunCommand(command, executor, callingStatement);
            executor.PopFile();
        }
    }
}