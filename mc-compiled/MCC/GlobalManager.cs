using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Manages the global entity and the commands used to access it.
    /// </summary>
    public class GlobalManager
    {
        public const string GLOBAL_ENTITY = "_global";
        public const string RESET_FUNCTION = "_mcc/reset_global";
        public const string RESET_FILE = "reset_global";

        readonly Executor executor;
        internal GlobalManager(Executor executor)
        {
            this.executor = executor;
        }

        /// <summary>
        /// Create a file which will reset the global entity.
        /// </summary>
        /// <returns></returns>
        public CommandFile CreateResetFile(string entityID)
        {
            CommandFile file = new CommandFile(RESET_FILE, Executor.MCC_GENERATED_FOLDER);
            file.Add(new[]
            {
                Command.Kill($"@e[c=1,type={entityID},name=\"_global\"]"),
                Command.Summon(entityID, 0, 0, 0, GLOBAL_ENTITY)
            });
            return file;
        }
    }
}
