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
        
        private bool isLast;
        private readonly bool isFirst;
        
        private CommandFile stageCommands;
        private readonly AsyncFunction parent;
        
        private int? ticksUntilNextStage;

        /// <summary>
        /// Tell this stage that it's the stage in its parent async function.
        /// </summary>
        public void SignalIsLast()
        {
            this.isLast = true;
        }
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
            this.stageCommands = new CommandFile(true, stageName);

            this.ticksUntilNextStage = null;
        }
        
        /// <summary>
        /// Begins this stage and applies it to the given <see cref="Executor"/>.
        /// </summary>
        public void Begin(Executor executor)
        {
            executor.PushFile(this.stageCommands);
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
                if (this.stageCommands.Length > 0)
                    this.stageCommands.Add("");

                this.stageCommands.Add($"# End of async stage {this.stageIndex}.");
                this.stageCommands.Add($"# Waits {this.ticksUntilNextStage} ticks until next stage.");
            }
                
            if (this.isLast)
            {
                this.stageCommands.Add(Command.ScoreboardSet(this.parent.runningValue, 0));
                return;
            }
            
            if(this.isFirst)
                this.stageCommands.Add(Command.ScoreboardSet(this.parent.runningValue, 1));
            
            if (this.ticksUntilNextStage.HasValue)
            {
                this.stageCommands.Add(Command.ScoreboardSet(this.parent.timerValue, this.ticksUntilNextStage.Value));
                this.stageCommands.Add(Command.ScoreboardSet(this.parent.stageValue, this.stageIndex));
                return;
            }
            
            // immediately jump to the next stage
            this.stageCommands.Add(Command.ScoreboardSet(this.parent.stageValue, this.stageIndex + 1));
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
                if (this.stageCommands.Length > 0)
                    this.stageCommands.Add("");

                this.stageCommands.Add($"# End of async stage {this.stageIndex}.");
                this.stageCommands.Add($"# Continues if the following condition evaluates to true:");
                this.stageCommands.Add($"#     " + conditions.GetDescription());

            }

            string command = this.isLast ?
                Command.ScoreboardSet(this.parent.runningValue, 0) :
                this.ticksUntilNextStage.HasValue ?
                    Command.ScoreboardSet(this.parent.timerValue, this.ticksUntilNextStage.Value) :
                    Command.ScoreboardSet(this.parent.stageValue, this.stageIndex + 1);
            
            conditions.RunCommand(command, executor, callingStatement);
            executor.PopFile();
        }
    }
}