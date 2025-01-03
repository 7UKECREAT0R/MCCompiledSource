using System;
using System.Collections.Generic;
using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding;
using mc_compiled.Modding.Behaviors;
using mc_compiled.Modding.Behaviors.Lists;
using mc_compiled.Modding.Resources;

namespace mc_compiled.MCC.CustomEntities;

internal class ExploderManager : CustomEntityManager
{
    /// <summary>
    ///     The explosion presets that have been defined in the entity file. (component groups/events).
    /// </summary>
    private readonly HashSet<ExploderPreset> definedPresets;
    /// <summary>
    ///     The identifier for the exploder entity.
    /// </summary>
    private readonly string exploderType;
    /// <summary>
    ///     The files used to modify the explosion entity.
    /// </summary>
    private ExploderFiles files;

    internal ExploderManager(Executor executor) : base(executor)
    {
        this.definedPresets = [];
        this.files = new ExploderFiles();
        this.exploderType = executor.project.Namespace("exploder");
    }
    /// <summary>
    ///     Get the name of the component group of an explosion happening in <b>delay</b> ticks.
    /// </summary>
    /// <returns></returns>
    public static string GroupExplode(int power, int delayTicks, bool causesFire, bool breaksBlocks)
    {
        return
            $"properties_{power}_{delayTicks}_{(causesFire ? "fire" : "nofire")}_{(breaksBlocks ? "break" : "nobreak")}";
    }

    /// <summary>
    ///     Get the name of the event for an explosion happening in <b>delay</b> ticks.
    /// </summary>
    /// <returns></returns>
    public static string EventExplode(int power, int delayTicks, bool causesFire, bool breaksBlocks)
    {
        return
            $"explode_{power}_{delayTicks}_{(causesFire ? "fire" : "nofire")}_{(breaksBlocks ? "break" : "nobreak")}";
    }

    /// <summary>
    ///     Get the name of the component group of an explosion happening in <b>delay</b> ticks.
    /// </summary>
    /// <returns></returns>
    private static string GroupExplode(ExploderPreset preset)
    {
        return
            $"properties_{preset.power}_{preset.delay}_{(preset.fire ? "fire" : "nofire")}_{(preset.breaks ? "break" : "nobreak")}";
    }

    /// <summary>
    ///     Get the name of the event for an explosion happening in <b>delay</b> ticks.
    /// </summary>
    /// <returns></returns>
    private static string EventExplode(ExploderPreset preset)
    {
        return
            $"explode_{preset.power}_{preset.delay}_{(preset.fire ? "fire" : "nofire")}_{(preset.breaks ? "break" : "nobreak")}";
    }

    protected override IEnumerable<IAddonFile> CreateEntityFiles()
    {
        this.files = EntityBehavior.CreateExploder(this.exploderType);
        return this.files.AddonFiles;
    }
    public override bool HasEntity(string name) { return false; }
    public override bool Search(string name, out Selector selector)
    {
        selector = null;
        return false;
    }

    /// <summary>
    ///     Get the event needed for an explosion preset, registering it if needed.
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    private string GetPreset(ExploderPreset entry)
    {
        if (this.definedPresets.Contains(entry))
            return EventExplode(entry);

        string groupName = GroupExplode(entry);
        string eventName = EventExplode(entry);

        if (GlobalContext.Debug)
            Console.WriteLine("Registered exploder preset: " + groupName);

        var group = new EntityComponentGroup(groupName, new ComponentExplode(entry, true));
        var entityEvent = new EntityEventHandler(eventName, action: new EventActionAddGroup(group));
        this.files.groups.Add(group);
        this.files.events.Add(entityEvent);
        this.definedPresets.Add(entry);

        return eventName;
    }
    /// <summary>
    ///     Get the event needed for an explosion preset, registering it if needed.
    /// </summary>
    /// <param name="power"></param>
    /// <param name="delay"></param>
    /// <param name="fire"></param>
    /// <param name="breaksBlocks"></param>
    /// <returns></returns>
    public string GetPreset(int power, int delay, bool fire, bool breaksBlocks)
    {
        return GetPreset(new ExploderPreset(power, delay, fire, breaksBlocks));
    }
    /// <summary>
    ///     Register a preset in the entity file if needed. Returns the command to summon an explosion with a set of
    ///     properties.
    /// </summary>
    /// <returns></returns>
    public string CreateExplosion(Coordinate x,
        Coordinate y,
        Coordinate z,
        int power = 3,
        int delay = 0,
        bool fire = false,
        bool breaksBlocks = true)
    {
        // Register the preset if needed and get the event name to trigger it.
        string eventName = GetPreset(power, delay, fire, breaksBlocks);

        // Simple summon command!
        return Command.SummonWithEvent(this.exploderType, x, y, z, Coordinate.here, Coordinate.here, eventName);
    }
}

public struct ExploderFiles
{
    internal List<EntityComponentGroup> groups;
    internal List<EntityEventHandler> events;

    public EntityBehavior behavior;
    public EntityResource resources;
    public EntityGeometry geometry;

    public IAddonFile[] AddonFiles =>
    [
        this.behavior, this.resources, this.geometry
    ];
}

public readonly struct ExploderPreset
{
    public ExploderPreset(int power, int delay, bool fire, bool breaksBlocks)
    {
        this.power = power;
        this.delay = delay;
        this.fire = fire;
        this.breaks = breaksBlocks;
    }

    public readonly int power;
    public readonly int delay;
    public readonly bool fire;
    public readonly bool breaks;

    private bool Equals(ExploderPreset other)
    {
        return this.power == other.power && this.delay == other.delay && this.fire == other.fire &&
               this.breaks == other.breaks;
    }
    public override bool Equals(object obj) { return obj is ExploderPreset other && Equals(other); }
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = this.power;
            hashCode = (hashCode * 397) ^ this.delay;
            hashCode = (hashCode * 397) ^ this.fire.GetHashCode();
            hashCode = (hashCode * 397) ^ this.breaks.GetHashCode();
            return hashCode;
        }
    }
}