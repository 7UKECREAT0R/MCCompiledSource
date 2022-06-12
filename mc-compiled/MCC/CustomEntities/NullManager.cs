using mc_compiled.Commands;
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
    /// Manages null entities in a project.
    /// </summary>
    internal class NullManager : CustomEntityManager
    {
        public const string DESTROY_COMPONENT_GROUP = "instant_despawn";

        public const string DESTROY_EVENT_NAME = "destroy";
        public const string CLEAN_EVENT_NAME = "clean";

        internal HashSet<string> existingNulls;
        internal HashSet<string> existingClasses;
        public readonly string nullType;
        NullFiles nullFiles;

        /// <summary>
        /// Get the name of the family for a specific null-class name.
        /// </summary>
        /// <param name="clazz"></param>
        /// <returns></returns>
        public static string FamilyName(string clazz)
        {
            return "class_" + Tokenizer.StripForPack(clazz);
        }
        /// <summary>
        /// Get the name of the family for a specific null-class name.
        /// </summary>
        /// <param name="clazz"></param>
        /// <returns></returns>
        public static string EventName(string clazz)
        {
            return "with_" + Tokenizer.StripForPack(clazz);
        }

        internal NullManager(Executor parent) : base(parent)
        {
            createdEntityFiles = false;
            existingNulls = new HashSet<string>();
            existingClasses = new HashSet<string>();
            nullType = parent.project.Namespace("null");
        }
        /// <summary>
        /// Create a selector for a null entity.
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
                    type = nullType,
                    name = name
                }
            };
        }
        /// <summary>
        /// Create a string-ed selector for a null entity.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetStringSelector(string name) =>
            $"@e[type={nullType},name={name}]";
        /// <summary>
        /// Create a string-ed selector for a null entity with a class.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetStringSelector(string name, string clazz) =>
            $"@e[name={name},family={FamilyName(clazz)}]";
        /// <summary>
        /// Get a selector to select all null entities.
        /// </summary>
        /// <returns></returns>
        public string GetAllStringSelector() =>
            $"@e[type={nullType}]";
        public string GetAllClassSelector(string clazz) =>
            $"@e[family={FamilyName(clazz)}]";
        
        /// <summary>
        /// Create a new null entity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="yRot"></param>
        /// <param name="xRot"></param>
        /// <returns>The commands to create this null entity.</returns>
        public string Create(string name, string @class, Coord x, Coord y, Coord z)
        {
            EnsureEntity();
            List<string> commands = new List<string>();
            existingNulls.Add(name);

            if(@class != null)
            {
                string eventName = DefineClass(@class);
                return Command.Summon(nullType, x, y, z, name, eventName);
            }

            return Command.Summon(nullType, x, y, z, name);
        }
        /// <summary>
        /// Destroy a null entity.
        /// </summary>
        /// <param name="name">The name of the null entity to destroy.</param>
        /// <param name="clazz">If specified, will only remove if in this class.</param>
        /// <returns></returns>
        public string Destroy(string name, string clazz = null)
        {
            EnsureEntity();

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

                if (nullFiles.cleanEvent.action is EventActionRemoveGroup)
                {
                    EventActionRemoveGroup removeGroup = nullFiles.cleanEvent.action as EventActionRemoveGroup;
                    removeGroup.groups.Add(groupName);
                }

                nullFiles.behavior.componentGroups.Add(group);
                nullFiles.behavior.events.Add(trigger);
                existingClasses.Add(@class);

                if (Program.DEBUG)
                {
                    ConsoleColor old = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Creating null class: " + @class);
                    Console.ForegroundColor = old;
                }
            }

            return eventName;
        }

        internal override IAddonFile[] CreateEntityFiles()
        {
            nullFiles = EntityBehavior.CreateNull(nullType);
            return nullFiles.AddonFiles;
        }
        public override bool HasEntity(string entity) =>
            existingNulls.Contains(entity);
        public override bool Search(string name, out Commands.Selector selector)
        {
            if (existingNulls.Contains(name))
            {
                selector = GetSelector(name);
                return true;
            }

            selector = default;
            return false;
        }
    }
    public struct NullFiles
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
