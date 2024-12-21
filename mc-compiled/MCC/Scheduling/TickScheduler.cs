using System;
using System.Collections.Generic;
using System.Text;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Compiler.Async;
using mc_compiled.Modding;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.Scheduling;

/// <summary>
///     Manages tick.json and the scheduling of functions.
/// </summary>
public class TickScheduler : IAddonFile
{
    internal const string FOLDER = Executor.MCC_GENERATED_FOLDER + "/scheduler";
    private readonly Executor executor;
    private readonly Dictionary<string, CommandFile> existingFiles;
    private readonly List<ScheduledTask> tasks;

    internal readonly List<string> tickJSONEntries;
    internal TickScheduler(Executor executor)
    {
        this.tasks = [];
        this.tickJSONEntries = [];
        this.existingFiles = new Dictionary<string, CommandFile>();
        this.executor = executor;
    }

    public string CommandReference => throw new NotImplementedException();
    public byte[] GetOutputData()
    {
        var json = new JObject
        {
            // ReSharper disable once CoVariantArrayConversion
            // (library handles this already)
            ["values"] = new JArray(this.tickJSONEntries.ToArray())
        };

        string text = json.ToString();
        return Encoding.UTF8.GetBytes(text);
    }
    public string GetExtendedDirectory()
    {
        return null;
    }
    public string GetOutputFile()
    {
        return "tick.json";
    }
    public OutputLocation GetOutputLocation()
    {
        return OutputLocation.b_FUNCTIONS;
    }

    /// <summary>
    ///     Either creates or gets an existing command file in this scheduler.
    ///     If the file is created and 'addToTickJSON' is true, it will be added to <see cref="tickJSONEntries" /> as well.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="addToTickJSON"></param>
    /// <returns></returns>
    private CommandFile CreateOrGetCommandFile(string name, bool addToTickJSON = true)
    {
        if (this.existingFiles.TryGetValue(name, out CommandFile file))
            return file;

        // create new file
        file = new CommandFile(true, name, FOLDER);
        this.executor.AddExtraFile(file);
        this.existingFiles[name] = file;

        if (addToTickJSON)
            this.tickJSONEntries.Add(file.CommandReference);

        return file;
    }
    /// <summary>
    ///     Returns if the input file is related directly to the scheduler.
    /// </summary>
    /// <param name="file">The file to check.</param>
    /// <returns>True if the file is used in any scheduled tasks; otherwise false.</returns>
    internal bool IsFileAuto(CommandFile file)
    {
        // ReSharper disable once InvertIf
        if (file.folder != null && file.folder.Equals(AsyncFunction.FOLDER))
            if (file.name.Equals("tick"))
                return true;

        return file.folder?.StartsWith(FOLDER) ?? false;
    }

    /// <summary>
    ///     Schedules a task, assigns it an ID, and calls ScheduledTask::Setup
    /// </summary>
    /// <param name="task"></param>
    /// <returns>The ID of the scheduled task.</returns>
    public int ScheduleTask(ScheduledTask task)
    {
        // use index as id
        int id = this.tasks.Count;
        task.id = id;

        // setup and add
        task.Setup(this, this.executor);

        // do commands
        string[] commands = task.PerTickCommands();
        if ((commands?.Length ?? 0) > 0)
        {
            CommandFile file = CreateOrGetCommandFile(task.functionName);
            file.Add(commands);
        }

        this.tasks.Add(task);
        return id;
    }
}