using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding;
using mc_compiled.Modding.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.CustomEntities
{
    /// <summary>
    /// Manages dummy entities in a project.
    /// </summary>
    internal class DummyManager : CustomEntityManager
    {
        public const string DESTROY_COMPONENT_GROUP = "instant_despawn";

        public const string DESTROY_EVENT_NAME = "destroy";
        public const string CLEAN_EVENT_NAME = "clean";

        internal HashSet<string> existingDummies;
        internal HashSet<string> existingClasses;
        public readonly string dummyType;
        DummyFiles dummyFiles;

        /// <summary>
        /// Get the name of the family for a specific dummy-class name.
        /// </summary>
        /// <param name="clazz"></param>
        /// <returns></returns>
        public static string FamilyName(string clazz)
        {
            return "class_" + Tokenizer.StripForPack(clazz);
        }
        /// <summary>
        /// Get the name of the family for a specific dummy-class name.
        /// </summary>
        /// <param name="clazz"></param>
        /// <returns></returns>
        public static string EventName(string clazz)
        {
            return "with_" + Tokenizer.StripForPack(clazz);
        }

        internal DummyManager(Executor parent) : base(parent)
        {
            createdEntityFiles = false;
            existingDummies = new HashSet<string>();
            existingClasses = new HashSet<string>();
            dummyType = parent.project.Namespace("dummy");
        }
        /// <summary>
        /// Create a selector for a dummy entity.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Selector GetSelector(string name)
        {
            return new Selector(Selector.Core.e)
            {
                count = new Commands.Selectors.Count(1),
                entity = new Commands.Selectors.Entity()
                {
                    type = dummyType,
                    name = name
                }
            };
        }
        /// <summary>
        /// Create a string-ed selector for a dummy entity.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetStringSelector(string name) =>
            $"@e[type={dummyType},name={name}]";
        /// <summary>
        /// Create a string-ed selector for a dummy entity with a class.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetStringSelector(string name, string clazz) =>
            $"@e[name={name},family={FamilyName(clazz)}]";
        /// <summary>
        /// Get a selector to select all dummy entities.
        /// </summary>
        /// <returns></returns>
        public string GetAllStringSelector() =>
            $"@e[type={dummyType}]";
        public string GetAllClassSelector(string clazz) =>
            $"@e[family={FamilyName(clazz)}]";
        
        /// <summary>
        /// Create a new dummy entity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="yRot"></param>
        /// <param name="xRot"></param>
        /// <returns>The commands to create this dummy entity.</returns>
        public string Create(string name, string @class, Coord x, Coord y, Coord z)
        {
            List<string> commands = new List<string>();
            existingDummies.Add(name);

            if(@class != null)
            {
                string eventName = DefineClass(@class);
                return Command.Summon(dummyType, x, y, z, name, eventName);
            }

            return Command.Summon(dummyType, x, y, z, name);
        }
        /// <summary>
        /// Destroy a dummy entity.
        /// </summary>
        /// <param name="name">The name of the dummy entity to destroy.</param>
        /// <param name="clazz">If specified, will only remove if in this class.</param>
        /// <returns></returns>
        public string Destroy(string name, string clazz = null)
        {
            if(clazz == null)
                return Command.Event(GetStringSelector(name), DESTROY_EVENT_NAME);
            else
                return Command.Event(GetStringSelector(name, clazz), DESTROY_EVENT_NAME);
        }

        /// <summary>
        /// Defines and sets up behaviors to handle this class, if not already. If class is defined, no action will happen.
        /// </summary>
        /// <param name="class">The name of the class to define.</param>
        /// <returns>The name of the event to call.</returns>
        public string DefineClass(string @class)
        {
            // Use a spawn event to control the family 
            @class = Tokenizer.StripForPack(@class);
            string eventName = "with_" + @class;

            if (!existingClasses.Contains(@class))
            {
                string className = "class_" + @class;
                string groupName = "group_" + @class;

                // Create the family component to add.
                ComponentFamily family = new ComponentFamily()
                {
                    families = new[] { className }
                };

                // Create the component groups 
                EntityComponentGroup group = new EntityComponentGroup(groupName, family);
                EntityEventHandler trigger = new EntityEventHandler(eventName,
                    action: new EventActionAddGroup(groupName));

                if (dummyFiles.cleanEvent.action is EventActionRemoveGroup)
                {
                    EventActionRemoveGroup removeGroup = dummyFiles.cleanEvent.action as EventActionRemoveGroup;
                    removeGroup.groups.Add(groupName);
                }

                dummyFiles.behavior.componentGroups.Add(group);
                dummyFiles.behavior.events.Add(trigger);
                existingClasses.Add(@class);

                if (Program.DEBUG)
                {
                    ConsoleColor old = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Creating dummy class: " + @class);
                    Console.ForegroundColor = old;
                }
            }

            return eventName;
        }

        internal override IAddonFile[] CreateEntityFiles()
        {
            dummyFiles = EntityBehavior.CreateDummy(dummyType);
            return dummyFiles.AddonFiles;
        }
        public override bool HasEntity(string entity) =>
            existingDummies.Contains(entity);
        public override bool Search(string name, out Commands.Selectors.Selector selector)
        {
            if (existingDummies.Contains(name))
            {
                selector = GetSelector(name);
                return true;
            }

            selector = default;
            return false;
        }
    }
    public struct DummyFiles
    {
        public Modding.Behaviors.EntityEventHandler cleanEvent;
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
}
