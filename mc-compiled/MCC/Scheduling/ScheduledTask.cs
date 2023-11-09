namespace mc_compiled.MCC.Scheduling
{
    /// <summary>
    /// Represents an abstract implementation of a task which is held in tick.json.
    /// </summary>
    public abstract class ScheduledTask
    {
        internal string functionName;
        internal int id; // Set upon being passed to the scheduler.

        public ScheduledTask(string functionName)
        {
            this.functionName = functionName;
        }

        /// <summary>
        /// Setup whatever is needed for this task in the head file. 
        /// </summary>
        /// <param name="executor">The target executor to modify.</param>
        public abstract void Setup(Compiler.Executor executor);

        /// <summary>
        /// The function to be run in tick.json that will presumably check/perform an action.
        /// </summary>
        /// <returns></returns>
        public abstract string[] PerTickCommands();
    }
}
