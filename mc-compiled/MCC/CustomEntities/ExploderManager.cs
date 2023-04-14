using mc_compiled.Commands;
using mc_compiled.Modding;
using mc_compiled.Modding.Behaviors;
using mc_compiled.Modding.Behaviors.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.CustomEntities
{
    internal class ExploderManager : CustomEntityManager
    {
        /// <summary>
        /// Get the name of the component group of an explosion happening in <b>delay</b> ticks.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static string GroupExplode(int power, int delayTicks, bool causesFire, bool breaksBlocks) =>
            $"properties_{power}_{delayTicks}_{(causesFire?"fire":"nofire")}_{(breaksBlocks?"break":"nobreak")}";
        /// <summary>
        /// Get the name of the event for an explosion happening in <b>delay</b> ticks.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static string EventExplode(int power, int delayTicks, bool causesFire, bool breaksBlocks) =>
            $"explode_{power}_{delayTicks}_{(causesFire ? "fire" : "nofire")}_{(breaksBlocks ? "break" : "nobreak")}";

        /// <summary>
        /// Get the name of the component group of an explosion happening in <b>delay</b> ticks.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static string GroupExplode(ExploderPreset preset) =>
            $"properties_{preset.power}_{preset.delay}_{(preset.fire ? "fire" : "nofire")}_{(preset.breaks ? "break" : "nobreak")}";
        /// <summary>
        /// Get the name of the event for an explosion happening in <b>delay</b> ticks.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static string EventExplode(ExploderPreset preset) =>
            $"explode_{preset.power}_{preset.delay}_{(preset.fire ? "fire" : "nofire")}_{(preset.breaks ? "break" : "nobreak")}";

        /// <summary>
        /// The explosion presets that have been defined in the entity file. (component groups/events).
        /// </summary>
        HashSet<ExploderPreset> definedPresets;
        /// <summary>
        /// The files used to modify the explosion entity.
        /// </summary>
        ExploderFiles files;
        /// <summary>
        /// The identifier for the exploder entity.
        /// </summary>
        public readonly string exploderType;

        internal ExploderManager(Compiler.Executor executor) : base(executor)
        {
            definedPresets = new HashSet<ExploderPreset>();
            files = new ExploderFiles();
            exploderType = executor.project.Namespace("exploder");
        }

        internal override IAddonFile[] CreateEntityFiles()
        {
            files = EntityBehavior.CreateExploder(exploderType);
            return files.AddonFiles;
        }
        public override bool HasEntity(string name) => false;
        public override bool Search(string name, out Commands.Selectors.Selector selector)
        {
            selector = null;
            return false;
        }

        /// <summary>
        /// Get the event needed for an explosion preset, registering it if needed.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public string GetPreset(ExploderPreset entry)
        {
            if (definedPresets.Contains(entry))
                return EventExplode(entry);

            string groupName = GroupExplode(entry);
            string eventName = EventExplode(entry);

            if (Program.DEBUG)
                Console.WriteLine("Registered exploder preset: " + groupName);

            EntityComponentGroup group = new EntityComponentGroup(groupName, new ComponentExplode(entry, true));
            EntityEventHandler entityEvent = new EntityEventHandler(eventName, action: new EventActionAddGroup(group));
            files.groups.Add(group);
            files.events.Add(entityEvent);
            definedPresets.Add(entry);

            return eventName;
        }
        /// <summary>
        /// Get the event needed for an explosion preset, registering it if needed.
        /// </summary>
        /// <param name="power"></param>
        /// <param name="delay"></param>
        /// <param name="fire"></param>
        /// <param name="breaksBlocks"></param>
        /// <returns></returns>
        public string GetPreset(int power, int delay, bool fire, bool breaksBlocks) =>
            GetPreset(new ExploderPreset(power, delay, fire, breaksBlocks));
        /// <summary>
        /// Register a preset in the entity file if needed. Returns the command to summon an explosion with a set of properties.
        /// </summary>
        /// <returns></returns>
        public string CreateExplosion(Coord x, Coord y, Coord z, int power = 3, int delay = 0, bool fire = false, bool breaksBlocks = true)
        {
            // Register the preset if needed and get the event name to trigger it.
            string eventName = GetPreset(power, delay, fire, breaksBlocks);

            // Simple summon command!
            return Command.SummonWithEvent(exploderType, x, y, z, eventName);
        }
    }
    public struct ExploderFiles
    {
        internal List<Modding.Behaviors.EntityComponentGroup> groups;
        internal List<Modding.Behaviors.EntityEventHandler> events;

        public Modding.Behaviors.EntityBehavior behavior;
        public Modding.Resources.EntityResource resources;
        public Modding.Resources.EntityGeometry geometry;

        public IAddonFile[] AddonFiles
        {
            get => new Modding.IAddonFile[]
            {
                behavior, resources, geometry,
            };
        }
    }
    public struct ExploderPreset
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

        public bool Equals(int power, int delay, bool fire, bool breaksBlocks)
        {
            return this.power == power &&
                this.delay == delay &&
                this.fire == fire &&
                this.breaks == breaksBlocks;
        }
        public override bool Equals(object obj)
        {
            return obj is ExploderPreset entry &&
                   power == entry.power &&
                   delay == entry.delay &&
                   fire == entry.fire &&
                   breaks == entry.breaks;
        }
        public override int GetHashCode()
        {
            int hashCode = 230165662;
            hashCode = hashCode * -1521134295 + power.GetHashCode();
            hashCode = hashCode * -1521134295 + delay.GetHashCode();
            hashCode = hashCode * -1521134295 + fire.GetHashCode();
            hashCode = hashCode * -1521134295 + breaks.GetHashCode();
            return hashCode;
        }
    }
}
