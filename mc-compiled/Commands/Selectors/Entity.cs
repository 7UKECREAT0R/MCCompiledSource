using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents selection option that limits based off of entity properties.
    /// </summary>
    public struct Entity
    {
        public string name;     // The name of this entity/player.
        public string type;     // The type of this entity.
        public string family;   // The family this entity is in.
        public int?
            rotXMin,
            rotXMax;
        public int?
            rotYMin,
            rotYMax;

        public Entity(string name, string type, string family, int? rotXMin = null,
            int? rotXMax = null, int? rotYMin = null, int? rotYMax = null)
        {
            this.name = name;
            this.type = type;
            this.family = family;
            this.rotXMin = rotXMin;
            this.rotXMax = rotXMax;
            this.rotYMin = rotYMin;
            this.rotYMax = rotYMax;
        }

        public string[] GetSections()
        {
            List<string> parts = new List<string>();

            if (name != null)
                if(name.StartsWith("!"))
                    parts.Add("name=!\"" + name.Substring(1) + "\"");
                else
                    parts.Add("name=\"" + name + "\"");

            if (type != null)
                parts.Add("type=" + type + "");
            if (family != null)
                parts.Add("family=" + family + "");
            if (rotXMin.HasValue)
                parts.Add("rxm=" + rotXMin.Value);
            if (rotXMax.HasValue)
                parts.Add("rx=" + rotXMax.Value);
            if (rotYMin.HasValue)
                parts.Add("rym=" + rotYMin.Value);
            if (rotYMax.HasValue)
                parts.Add("ry=" + rotYMax.Value);
            return parts.ToArray();
        }
    }
}
