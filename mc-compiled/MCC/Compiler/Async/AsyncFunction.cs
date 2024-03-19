using System.Collections.Generic;
using System.Windows.Media.TextFormatting;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Compiler.Async
{
    public class AsyncFunction
    {
        private static readonly string FOLDER = $"{Executor.MCC_GENERATED_FOLDER}/async";
        private static string EscapeFunctionName(string functionName) =>
            functionName.Replace('/', '_').Replace('.', '_');
        private static string NameTickPrerequisiteFunction(string functionName) =>
            $"pretick_{functionName}";
        private static string NameTickFunction(string functionName) =>
           $"tick_{functionName}";

        private static string NameRunningObjective(string functionName) => $"_asyncRunning_{functionName}";
        private static string NameStageObjective(string functionName) => $"_asyncStage_{functionName}";
        private static string NameTimerObjective(string functionName) => $"_asyncTimer_{functionName}";

        /// <summary>
        /// The escaped string representing the function name that will be used in all related identifiers.
        /// </summary>
        internal readonly string escapedFunctionName;
        /// <summary>
        /// The entity that should be targeted by this async function.
        /// If global, use the global fake-player to hold the state.
        /// If local, use the running entity to hold the state.
        /// </summary>
        internal readonly AsyncTarget target;
        /// <summary>
        /// The function that's registered with the compiler and shows up in things like autocomplete results.
        /// Calling this function should route to the first stage of the code.
        /// </summary>
        public RuntimeFunction registeredFunction;

        private int NextStageIndex => this.stages.Count;
        private readonly AsyncManager parent;
        private readonly List<AsyncStage> stages;

        private AsyncStage activeStage;
        private CommandFile tick;
        internal CommandFile tickPrerequisite;
        
        internal ScoreboardValue runningValue;
        internal ScoreboardValue stageValue;
        internal ScoreboardValue timerValue;

        private void InitializeScoreboardValues(ScoreboardManager manager)
        {
            bool global = this.target == AsyncTarget.Global;
            this.runningValue = new ScoreboardValue(NameRunningObjective(this.escapedFunctionName), global, Typedef.BOOLEAN, manager);
            this.stageValue = new ScoreboardValue(NameStageObjective(this.escapedFunctionName), global, Typedef.INTEGER, manager);
            this.timerValue = new ScoreboardValue(NameTimerObjective(this.escapedFunctionName), global, Typedef.TIME, manager);
        }
        private void InitializeCommandFiles()
        {
            this.tickPrerequisite = new CommandFile(true,
                NameTickPrerequisiteFunction(this.escapedFunctionName), FOLDER);
            this.tick = new CommandFile(true,
                NameTickFunction(this.escapedFunctionName), FOLDER);
            
            this.parent.parent.AddExtraFile(this.tickPrerequisite);
            this.parent.parent.AddExtraFile(this.tick);

            this.tickPrerequisite.Add(new[] {
                // if timer == 0: stage++
                Command.Execute().IfScore(this.timerValue, Range.Of(0)).Run(Command.ScoreboardAdd(this.stageValue, 1)),
                // if timer >= 0: timer--
                Command.Execute().IfScore(this.timerValue, new Range(0, null)).Run(Command.ScoreboardSubtract(this.timerValue, 1)),
                // if timer == -1: tick()
                Command.Execute().IfScore(this.timerValue, Range.Of(-1)).Run(Command.Function(this.tick))
            });
        }
        public AsyncFunction(AsyncManager parent, string functionName, AsyncTarget target)
        {
            this.parent = parent;
            this.escapedFunctionName = EscapeFunctionName(functionName);
            this.target = target;
            this.stages = new List<AsyncStage>();

            InitializeScoreboardValues(this.parent.parent.scoreboard);
            InitializeCommandFiles();
        }
        
        public void StartNewStage()
        {
            int index = this.NextStageIndex;
            
        }
        public void FinishStage(int tickDelayUntilNext)
        {
            
        }
        public void FinishStage(ComparisonSet comparisonToGoToNext)
        {
            
        }
    }
    public enum AsyncTarget
    {
        Local,
        Global
    }
}