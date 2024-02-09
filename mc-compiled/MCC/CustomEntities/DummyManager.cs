using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding;
using mc_compiled.Modding.Behaviors;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace mc_compiled.MCC.CustomEntities
{
    /// <summary>
    /// Manages dummy entities in a project.
    /// </summary>
    internal class DummyManager : CustomEntityManager
    {
        public const string DESTROY_COMPONENT_GROUP = "instant_despawn";
        public const string DESTROY_EVENT_NAME = "destroy";

        public const string TAGGABLE_COMPONENT_GROUP = "taggable";
        public const string TAGGABLE_EVENT_ADD_NAME = "as_taggable";
        public const string TAGGABLE_EVENT_REMOVE_NAME = "remove_taggable";
        public const string TAGGABLE_FAMILY_NAME = "__dummy_taggable";

        private readonly HashSet<string> existingDummies;
        public readonly string dummyType;
        private DummyFiles dummyFiles;

        internal DummyManager(Executor parent) : base(parent)
        {
            createdEntityFiles = false;
            existingDummies = new HashSet<string>();
            dummyType = parent.project.Namespace("dummy");
        }

        /// <summary>
        /// Create a selector for a dummy entity.
        /// </summary>
        /// <param name="name">The name of the dummy entity.</param>
        /// <param name="setForTagging">Flag to indicate if the selector should check for dummies waiting to be tagged.</param>
        /// <returns></returns>
        [PublicAPI]
        public Selector GetSelector(string name, bool setForTagging)
        {
            if (setForTagging)
            {
                return new Selector(Selector.Core.e)
                {
                    count = new Count(1),
                    entity = new Entity()
                    {
                        type = dummyType,
                        name = name,
                        families = new List<string> { TAGGABLE_FAMILY_NAME }
                    },
                };
            }

            return new Selector(Selector.Core.e)
            {
                count = new Count(1),
                entity = new Entity()
                {
                    type = dummyType,
                    name = name
                }
            };
        }

        /// <summary>
        /// Create a string selector for a dummy entity.
        /// </summary>
        /// <param name="name">The name of the dummy entity.</param>
        /// <param name="setForTagging">Flag to indicate if the selector should check for dummies waiting to be tagged.</param>
        /// <param name="tag">The tag of the dummy entity (optional).</param>
        /// <returns>The string-ed selector for the dummy entity.</returns>
        public string GetStringSelector(string name, bool setForTagging, string tag = null)
        {
            if (setForTagging)
            {
                if (tag == null)
                    return $"@e[type={dummyType},name={name},family={DummyManager.TAGGABLE_FAMILY_NAME}]";
                return $"@e[type={dummyType},name={name},family={DummyManager.TAGGABLE_FAMILY_NAME},tag={tag}]";
            }

            if(tag == null)
                return $"@e[type={dummyType},name={name}]";
            return $"@e[type={dummyType},name={name},tag={tag}]";
        }
        /// <summary>
        /// Get a selector to select all dummy entities.
        /// </summary>
        /// <returns></returns>
        public string GetAllStringSelector() =>
            $"@e[type={dummyType}]";
        /// <summary>
        /// Get a selector to select all dummy entities.
        /// </summary>
        /// <returns></returns>
        public string GetAllStringSelector(string tag) =>
            $"@e[type={dummyType},tag={tag}]";

        /// <summary>
        /// Create a new dummy entity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="forTagging"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>The commands to create this dummy entity.</returns>
        public string Create(string name, bool forTagging, Coordinate x, Coordinate y, Coordinate z)
        {
            var commands = new List<string>();
            existingDummies.Add(name);

            if (!forTagging)
                return Command.Summon(dummyType, x, y, z, Coordinate.here, Coordinate.here, name);
            
            return Command.Summon(dummyType, x, y, z, Coordinate.here,Coordinate.here, name, TAGGABLE_EVENT_ADD_NAME);
        }

        /// <summary>
        /// Destroy a dummy entity.
        /// </summary>
        /// <param name="name">The name of the dummy entity to destroy.</param>
        /// <param name="setForTagging"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string Destroy(string name, bool setForTagging, string tag = null)
        {
            return Command.Event(GetStringSelector(name, setForTagging, tag), DESTROY_EVENT_NAME);
        }

        /// <summary>
        /// Destroy all dummy entities.
        /// </summary>
        /// <param name="tag">The tag to limit this destroy operation to (optional).</param>
        /// <returns></returns>
        public string DestroyAll(string tag = null)
        {
            if (tag == null)
                return Command.Event(GetAllStringSelector(), DESTROY_EVENT_NAME);
            return Command.Event(GetAllStringSelector(tag), DESTROY_EVENT_NAME);
        }

        protected override IEnumerable<IAddonFile> CreateEntityFiles()
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
                selector = GetSelector(name, false);
                return true;
            }

            selector = default;
            return false;
        }
    }
    public struct DummyFiles
    {
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
