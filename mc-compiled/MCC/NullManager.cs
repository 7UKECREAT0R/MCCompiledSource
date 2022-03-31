﻿using mc_compiled.Commands;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Manages null entities in a project.
    /// </summary>
    public class NullManager : ISelectorProvider
    {
        public const string destroyComponentGroup = "instant_despawn";
        public const string destroyEventName = "destroy";

        internal HashSet<int> existingNulls;
        public readonly string nullType;

        readonly Executor parent;
        bool createdEntityFile;

        internal NullManager(Executor parent)
        {
            this.parent = parent;
            createdEntityFile = false;
            existingNulls = new HashSet<int>();
            nullType = parent.project.Namespace("null");
        }
        /// <summary>
        /// Create a selector for a null entity.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Selector GetSelector(string name)
        {
            return new Selector()
            {
                core = Selector.Core.e,
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
            $"@e[c=1,type={nullType},name={name}]";
        /// <summary>
        /// Get a selector to select all null entities.
        /// </summary>
        /// <returns></returns>
        public string GetAllStringSelector() =>
            $"@e[type={nullType}]";
        /// <summary>
        /// Ensure that a null entity has been defined. If not, create and append to the project output.
        /// </summary>
        internal void EnsureEntity()
        {
            if (createdEntityFile)
                return;

            parent.AddExtraFile(EntityBehavior.CreateNull(nullType));
            createdEntityFile = true;
            return;
        }
        
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
        public string[] Create(string name, Coord x, Coord y, Coord z, Coord? yRot = null, Coord? xRot = null)
        {
            EnsureEntity();
            int hash = name.GetHashCode();
            List<string> commands = new List<string>();

            if (existingNulls.Contains(hash))
                commands.Add(Destroy(name));
            else
                existingNulls.Add(hash);

            commands.Add(Command.Summon(nullType, x, y, z, name));

            if (yRot.HasValue)
            {
                if (!xRot.HasValue)
                    xRot = Coord.here;

                commands.Add(Command.Teleport(GetStringSelector(name), x, y, z, yRot.Value, xRot.Value));
            }

            return commands.ToArray();
        }
        /// <summary>
        /// Destroy a null entity.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Destroy(string name)
        {
            EnsureEntity();
            existingNulls.Remove(name.GetHashCode());
            return Command.Event(GetStringSelector(name), destroyEventName);
        }

        public bool HasEntity(int hash) =>
            existingNulls.Contains(hash);
        public bool Search(string name, out Commands.Selector selector)
        {
            int hash = name.GetHashCode();
            if(existingNulls.Contains(hash))
            {
                selector = GetSelector(name);
                return true;
            }

            selector = default;
            return false;
        }
    }
}