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
}
