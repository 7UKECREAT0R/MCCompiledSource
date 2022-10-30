using mc_compiled.Modding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Scheduling
{
    /// <summary>
    /// Manages tick.json and the scheduling of functions.
    /// </summary>
    public class TickScheduler : IAddonFile
    {
        List<ScheduledTask> tasks;
        List<string> files;
        Compiler.Executor executor;

        public string CommandReference => throw new NotImplementedException();

        /// <summary>
        /// Schedules a task, assigns it an ID, and calls ScheduledTask::Setup
        /// </summary>
        /// <param name="task"></param>
        /// <returns>The ID of the scheduled task.</returns>
        public int ScheduleTask(ScheduledTask task)
        {
            // use index as id
            int id = tasks.Count;
            task.id = id;

            // setup and add
            task.Setup(executor);

            // do commands
            string[] cmds = task.PerTickCommands();
            if(cmds != null && cmds.Length > 0)
            {
                Compiler.CommandFile func = new Compiler.CommandFile
                    (task.functionName, "scheduler");
                func.Add(cmds);

                executor.AddExtraFile(func);
                files.Add(func.CommandReference);
            }

            tasks.Add(task);
            return id;
        }
        internal TickScheduler(Compiler.Executor executor)
        {
            this.tasks = new List<ScheduledTask>();
            this.files = new List<string>();
            this.executor = executor;
        }
        public byte[] GetOutputData()
        {
            JObject json = new JObject()
            {
                ["values"] = new JArray(files.ToArray())
            };

            string text = json.ToString();
            return Encoding.UTF8.GetBytes(text);
        }
        public string GetExtendedDirectory() => null;
        public string GetOutputFile() => "tick.json";
        public OutputLocation GetOutputLocation() =>
            OutputLocation.b_FUNCTIONS;
    }
}
