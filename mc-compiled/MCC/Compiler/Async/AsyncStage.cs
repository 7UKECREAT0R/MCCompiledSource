namespace mc_compiled.MCC.Compiler.Async
{
    /// <summary>
    /// Represents a stage in an async function.
    /// </summary>
    public class AsyncStage
    {
        private static readonly string FOLDER = $"{Executor.MCC_GENERATED_FOLDER}/async/stages";
        private static string NameStageFunction(string functionName, int stage) =>
            $"{functionName}_{stage}";
        
        public readonly int stageIndex;
        public readonly bool isLast;

        internal CommandFile stageCommands;
        private readonly AsyncFunction parent;
    }
}