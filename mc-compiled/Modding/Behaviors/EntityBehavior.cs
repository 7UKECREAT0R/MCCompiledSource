using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors
{
    /// <summary>
    /// An entity definition.
    /// </summary>
    public class EntityBehavior : IAddonFile
    {
        public FormatVersion formatVersion = FormatVersion.b_ENTITY;
        public EntityDescription description;

        public string GetExtendedDirectory() =>
            null;
        public byte[] GetOutputData()
        {
            
        }
        public string GetOutputFile() =>
            description.GetEntityName() + ".json";
        public OutputLocation GetOutputLocation() =>
            OutputLocation.b_ENTITIES;
    }
    public struct EntityDescription
    {
        public readonly string identifier;
        public readonly bool isSummonable;
        public readonly bool isSpawnable;
        public readonly bool isExperimental;

        public EntityDescription(string identifier, bool isSummonable = true,
            bool isSpawnable = false, bool isExperimental = false)
        {
            this.identifier = identifier;
            this.isSummonable = isSummonable;
            this.isSpawnable = isSpawnable;
            this.isExperimental = isExperimental;
        }
        /// <summary>
        /// Get the entity name without namespace.
        /// </summary>
        /// <returns></returns>
        public string GetEntityName()
        {
            int i = identifier.IndexOf(':');
            if (i == -1) return identifier;
            return identifier.Substring(i + 1);
        }
    }

}
