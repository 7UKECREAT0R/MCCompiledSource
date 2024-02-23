﻿using mc_compiled.Commands;
using mc_compiled.Modding;
using mc_compiled.Modding.Behaviors;
using mc_compiled.Modding.Behaviors.Lists;
using System;
using System.Collections.Generic;

namespace mc_compiled.MCC.CustomEntities
{
    internal class ExploderManager : CustomEntityManager
    {
        /// <summary>
        /// Get the name of the component group of an explosion happening in <b>delay</b> ticks.
        /// </summary>
        /// <returns></returns>
        public static string GroupExplode(int power, int delayTicks, bool causesFire, bool breaksBlocks) =>
            $"properties_{power}_{delayTicks}_{(causesFire?"fire":"nofire")}_{(breaksBlocks?"break":"nobreak")}";

        /// <summary>
        /// Get the name of the event for an explosion happening in <b>delay</b> ticks.
        /// </summary>
        /// <returns></returns>
        public static string EventExplode(int power, int delayTicks, bool causesFire, bool breaksBlocks) =>
            $"explode_{power}_{delayTicks}_{(causesFire ? "fire" : "nofire")}_{(breaksBlocks ? "break" : "nobreak")}";

        /// <summary>
        /// Get the name of the component group of an explosion happening in <b>delay</b> ticks.
        /// </summary>
        /// <returns></returns>
        private static string GroupExplode(ExploderPreset preset) =>
            $"properties_{preset.power}_{preset.delay}_{(preset.fire ? "fire" : "nofire")}_{(preset.breaks ? "break" : "nobreak")}";

        /// <summary>
        /// Get the name of the event for an explosion happening in <b>delay</b> ticks.
        /// </summary>
        /// <returns></returns>
        private static string EventExplode(ExploderPreset preset) =>
            $"explode_{preset.power}_{preset.delay}_{(preset.fire ? "fire" : "nofire")}_{(preset.breaks ? "break" : "nobreak")}";

        /// <summary>
        /// The explosion presets that have been defined in the entity file. (component groups/events).
        /// </summary>
        private readonly HashSet<ExploderPreset> definedPresets;
        /// <summary>
        /// The files used to modify the explosion entity.
        /// </summary>
        private ExploderFiles files;
        /// <summary>
        /// The identifier for the exploder entity.
        /// </summary>
        private readonly string exploderType;

        internal ExploderManager(Compiler.Executor executor) : base(executor)
        {
            this.definedPresets = new HashSet<ExploderPreset>();
            this.files = new ExploderFiles();
            this.exploderType = executor.project.Namespace("exploder");
        }

        protected override IEnumerable<IAddonFile> CreateEntityFiles()
        {
            this.files = EntityBehavior.CreateExploder(this.exploderType);
            return this.files.AddonFiles;
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
        private string GetPreset(ExploderPreset entry)
        {
            if (this.definedPresets.Contains(entry))
                return EventExplode(entry);

            string groupName = GroupExplode(entry);
            string eventName = EventExplode(entry);

            if (Program.DEBUG)
                Console.WriteLine("Registered exploder preset: " + groupName);

            EntityComponentGroup group = new EntityComponentGroup(groupName, new ComponentExplode(entry, true));
            EntityEventHandler entityEvent = new EntityEventHandler(eventName, action: new EventActionAddGroup(group));
            this.files.groups.Add(group);
            this.files.events.Add(entityEvent);
            this.definedPresets.Add(entry);

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
        public string CreateExplosion(Coordinate x, Coordinate y, Coordinate z, int power = 3, int delay = 0, bool fire = false, bool breaksBlocks = true)
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
        public Modding.Resources.EntityResource resources;
        public Modding.Resources.EntityGeometry geometry;

        public IAddonFile[] AddonFiles
        {
            get => new IAddonFile[]
            {
                this.behavior, this.resources, this.geometry,
            };
        }
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
            return this.power == other.power && this.delay == other.delay && this.fire == other.fire && this.breaks == other.breaks;
        }
        public override bool Equals(object obj)
        {
            return obj is ExploderPreset other && Equals(other);
        }
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
}
