using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.Commands;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Compiler.Async
{
    public class AsyncFunction : RuntimeFunction
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
        internal readonly string actualInternalName;
        /// <summary>
        /// The entity that should be targeted by this async function.
        /// If global, use the global fake-player to hold the state.
        /// If local, use the running entity to hold the state.
        /// </summary>
        internal readonly AsyncTarget target;
        /// <summary>
        /// List of async functions this function waits on. Used to prevent deadlocks at compile time.
        /// </summary>
        private readonly HashSet<AsyncFunction> waitsOn;

        /// <summary>
        /// Determines whether this <see cref="AsyncFunction"/> waits on the specified async function.
        /// </summary>
        /// <param name="other">The <see cref="AsyncFunction"/> to check if it is waited upon.</param>
        /// <returns>True if this <see cref="AsyncFunction"/> waits on the specified <see cref="AsyncFunction"/>, otherwise false.</returns>
        public bool WaitsOn(AsyncFunction other)
        {
            return this.waitsOn.Contains(other);
        }
        /// <summary>
        /// Adds a specified <see cref="AsyncFunction"/> to the list of async functions that this function waits on.
        /// </summary>
        /// <param name="other">The <see cref="AsyncFunction"/> to add to the list of waits on.</param>
        /// <param name="callingStatement">The <see cref="Statement"/> to use for exceptions.</param>
        /// <exception cref="StatementException">If a deadlock is detected.</exception>
        public void AddWaitsOn(AsyncFunction other, Statement callingStatement)
        {
            this.waitsOn.Add(other);

            if(other.WaitsOn(this))
            {
                throw new StatementException(callingStatement, 
                    $"Function '{other}' also awaits this function, potentially causing a deadlock.");
            }
        }
        
        private Executor Executor => this.parent.parent;
        private int NextStageIndex => this.groups.Sum(group => group.Count);
        private readonly AsyncManager parent;
        private readonly List<List<AsyncStage>> groups;

        /// <summary>
        /// Gets the current group of stages in the async function.
        /// </summary>
        /// <remarks>
        /// This property returns the current group of stages in the async function.
        /// A group of stages represents a logical unit of execution within the async function.
        /// </remarks>
        private List<AsyncStage> CurrentGroup => this.groups[this.groups.Count - 1];
        /// <summary>
        /// Pushes a new stage group onto the stack of stages.
        /// </summary>
        internal void StartNewGroup()
        {
            var stageList = new List<AsyncStage>();
            this.groups.Add(stageList);
        }
        
        public override Action<Executor> BlockOpenAction => e =>
        {
            // starts the first async stage
            StartNewStage();
        };
        public override Action<Executor> BlockCloseAction => e =>
        {
            // stop this async function and remove it from the parent
            this.parent.StopAsyncFunction();
            PopulateTickFunction();
            InitializeCallCommands();
        };
        
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
            
            this.parent.parent.AddCommandsInit(this.runningValue.CommandsDefine());
            this.parent.parent.AddCommandsInit(this.stageValue.CommandsDefine());
            this.parent.parent.AddCommandsInit(this.timerValue.CommandsDefine());
        }
        private void InitializeCommandFiles()
        {
            this.tickPrerequisite = new CommandFile(false,
                NameTickPrerequisiteFunction(this.escapedFunctionName), FOLDER);
            this.tick = new CommandFile(false,
                NameTickFunction(this.escapedFunctionName), FOLDER);
            
            this.parent.parent.AddExtraFile(this.tickPrerequisite);
            this.parent.parent.AddExtraFile(this.tick);
            this.file.RegisterCall(this.tickPrerequisite);
            this.file.RegisterCall(this.tick);

            this.tickPrerequisite.Add(new[] {
                // if timer == 0: stage++
                Command.Execute().IfScore(this.timerValue, Range.Of(0)).Run(Command.ScoreboardAdd(this.stageValue, 1)),
                // if timer >= 0: timer--
                Command.Execute().IfScore(this.timerValue, new Range(0, null)).Run(Command.ScoreboardSubtract(this.timerValue, 1)),
                // if timer == -1: tick()
                Command.Execute().IfScore(this.timerValue, Range.Of(-1)).Run(Command.Function(this.tick))
            });
        }
        private void InitializeCallCommands()
        {
            // these commands are NOT what get run when you call the function in MCCompiled; this is only here to
            // facilitate if the function gets run in-game.
            
            if (this.groups.Count == 0)
            {
                AddCommand("# No async code was added to this function.");
                return;
            }

            List<AsyncStage> validGroup = this.groups.FirstOrDefault(group => group.Count > 0);

            if (validGroup == null)
            {
                AddCommand("# No async code was added to this function.");
                return;
            }
                
            AddCommand(Command.Function(validGroup[0].file));
        }
        private void PopulateTickFunction()
        {
            foreach (List<AsyncStage> group in this.groups)
            {
                foreach (AsyncStage stage in group)
                {
                    string command = Command.Execute().IfScore(this.stageValue, Range.Of(stage.index)).Run(Command.Function(stage.file));
                    this.tick.Add(command);
                }
            }
        }
        public AsyncFunction(Statement statement, string name, string internalName, string documentation,
            IAttribute[] attributes, AsyncManager parent, AsyncTarget target) :
            base(statement, name, internalName, documentation, attributes)
        {
            this.parent = parent;
            this.actualInternalName = internalName;
            this.escapedFunctionName = EscapeFunctionName(internalName);
            this.target = target;
            this.waitsOn = new HashSet<AsyncFunction>();
            
            // initialize the groups
            this.groups = new List<List<AsyncStage>>();
            StartNewGroup();
            
            InitializeScoreboardValues(this.parent.parent.scoreboard);
            InitializeCommandFiles();
        }
        

        public override string Returns => "async context";
        public override void TryReturnValue(ScoreboardValue value, Executor executor, Statement caller) =>
            throw new StatementException(caller, "Async functions cannot return values.");
        public override void TryReturnValue(TokenLiteral value, Statement caller, Executor executor) =>
            throw new StatementException(caller, "Async functions cannot return values.");
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            // add the file to the executor if it hasn't been yet.
            if(!this.isAddedToExecutor && !this.isExtern)
            {
                executor.AddExtraFile(this.file);
                this.isAddedToExecutor = true;
            }

            if (this.groups.Count == 0)
            {
                commandBuffer.Add(Command.Function(this.file));
                return new TokenAsyncResult(this, statement.Lines[0]);
            }

            commandBuffer.Add(Command.Function(this.groups[0][0].file));

            // apply attributes
            foreach (IAttribute attribute in this.attributes)
                attribute.OnCalledFunction(this, commandBuffer, executor, statement);

            return new TokenAsyncResult(this, statement.Lines[0]);
        }
        
        /// <summary>
        /// Starts a new stage in this async function.
        /// </summary>
        public void StartNewStage()
        {
            int index = this.NextStageIndex;
            this.activeStage = new AsyncStage(this, index);
            this.activeStage.Begin(this.Executor);
            this.CurrentGroup.Add(this.activeStage);
            this.file.RegisterCall(this.activeStage.file);
        }
        /// <summary>
        /// Finish the last stage by terminating the async state machine.
        /// </summary>
        public void TerminateStage()
        {
            this.activeStage.FinishTerminate(this.Executor);
            this.activeStage = null;
        }
        /// <summary>
        /// Finish the current stage of the async function.
        /// </summary>
        /// <param name="tickDelayUntilNext">The number of ticks to delay before moving to the next stage.</param>
        public void FinishStage(int tickDelayUntilNext)
        {
            this.activeStage.SetTickDelay(tickDelayUntilNext);
            this.activeStage.Finish(this.Executor);
            this.activeStage = null;
        }
        /// <summary>
        /// Finish the current stage of the async function immediately in the same tick.
        /// </summary>
        public void FinishStageImmediate()
        {
            this.activeStage.RemoveTickDelay();
            this.activeStage.Finish(this.Executor);
            this.activeStage = null;
        }
        /// <summary>
        /// Finish the currently active stage of the async function.
        /// </summary>
        /// <param name="comparisonToGoToNext">The comparison that must be met to transition to the next stage.</param>
        /// <param name="callingStatement">The statement to blame when everything blows up.</param>
        public void FinishStage(ComparisonSet comparisonToGoToNext, Statement callingStatement)
        {
            this.activeStage.FinishUnderCondition(this.Executor, comparisonToGoToNext, callingStatement);
            this.activeStage = null;
        }

        /// <summary>
        /// Sends information about this function's groups and stages to stdout.
        /// </summary>
        /// <returns></returns>
        internal void PrintDebugGroupInfo()
        {
            Console.WriteLine($"Group information for async function '{this.internalName}' ({this.groups.Count} groups):");

            int groupNumber = 1;
            int stageNumber = 1;

            foreach (List<AsyncStage> group in this.groups)
            {
                Console.WriteLine($"\tGroup {groupNumber++} ({group.Count} stages):");
                foreach (AsyncStage stage in group)
                {
                    string[] commands = stage.file.commands.ToArray();
                    Console.WriteLine($"\t\tStage {stageNumber++} ({commands.Length} commands):");
                    foreach (string command in commands)
                    {
                        Console.WriteLine($"\t\t\t/" + command);
                    }
                }
            }
        }
        
        private bool Equals(AsyncFunction other)
        {
            return this.actualInternalName == other.actualInternalName && this.target == other.target;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((AsyncFunction) obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.actualInternalName != null ? this.actualInternalName.GetHashCode() : 0) * 397) ^ (int) this.target;
            }
        }
    }
    public enum AsyncTarget
    {
        Local,
        Global
    }
}