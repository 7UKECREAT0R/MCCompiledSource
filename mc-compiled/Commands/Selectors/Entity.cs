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
        public static Entity Parse(string[] chunks)
        {
            string name = null;
            string type = null;
            string family = null;

            int?
                rotXMin = null,
                rotXMax = null,
                rotYMin = null,
                rotYMax = null;

            foreach (string chunk in chunks)
            {
                int index = chunk.IndexOf('=');
                if (index == -1)
                    continue;
                string a = chunk.Substring(0, index).Trim().ToUpper();
                string b = chunk.Substring(index + 1).Trim();

                switch (a)
                {
                    case "NAME":
                        name = b.Trim('\"');
                        break;
                    case "TYPE":
                        type = b.Trim('\"');
                        break;
                    case "FAMILY":
                        family = b.Trim('\"');
                        break;
                    case "RX":
                        rotXMax = int.Parse(b);
                        break;
                    case "RXM":
                        rotXMin = int.Parse(b);
                        break;
                    case "RY":
                        rotYMax = int.Parse(b);
                        break;
                    case "RYM":
                        rotYMin = int.Parse(b);
                        break;
                }
            }

            return new Entity(name, type, family,
                rotXMin, rotXMax, rotYMin, rotYMax);
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
        public static Entity operator +(Entity a, Entity other)
        {
            if (a.name == null)
                a.name = other.name;
            if (a.type == null)
                a.type = other.type;
            if (a.family == null)
                a.family = other.family;
            if (a.rotXMin == null)
                a.rotXMin = other.rotXMin;
            if (a.rotXMax == null)
                a.rotXMax = other.rotXMax;
            if (a.rotYMin == null)
                a.rotYMin = other.rotYMin;
            if (a.rotYMax == null)
                a.rotYMax = other.rotYMax;
            return a;
        }
    }
}
