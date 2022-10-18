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

        public List<string> families;   // The family(s) this entity is in.

        public int?
            rotXMin,
            rotXMax;
        public int?
            rotYMin,
            rotYMax;

        public Entity(string name, string type, string[] families,
            int? rotXMin = null, int? rotXMax = null, int? rotYMin = null, int? rotYMax = null)
        {
            this.name = name;
            this.type = type;
            this.families = new List<string>(families);
            this.rotXMin = rotXMin;
            this.rotXMax = rotXMax;
            this.rotYMin = rotYMin;
            this.rotYMax = rotYMax;
        }
        public Entity(string name, string type, List<string> families,
            int? rotXMin = null, int? rotXMax = null, int? rotYMin = null, int? rotYMax = null)
        {
            this.name = name;
            this.type = type;
            this.families = families;
            this.rotXMin = rotXMin;
            this.rotXMax = rotXMax;
            this.rotYMin = rotYMin;
            this.rotYMax = rotYMax;
        }
        public static Entity Parse(string[] chunks)
        {
            string name = null;
            string type = null;

            List<string> families = new List<string>();

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
                        families.Add(b.Trim('\"'));
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

            return new Entity(name, type, families,
                rotXMin, rotXMax, rotYMin, rotYMax);
        }

        public override bool Equals(object obj)
        {
            return obj is Entity entity &&
                   name == entity.name &&
                   type == entity.type &&
                   EqualityComparer<List<string>>.Default.Equals(families, entity.families) &&
                   rotXMin == entity.rotXMin &&
                   rotXMax == entity.rotXMax &&
                   rotYMin == entity.rotYMin &&
                   rotYMax == entity.rotYMax;
        }
        public override int GetHashCode()
        {
            int hashCode = 996224562;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(type);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<string>>.Default.GetHashCode(families);
            hashCode = hashCode * -1521134295 + rotXMin.GetHashCode();
            hashCode = hashCode * -1521134295 + rotXMax.GetHashCode();
            hashCode = hashCode * -1521134295 + rotYMin.GetHashCode();
            hashCode = hashCode * -1521134295 + rotYMax.GetHashCode();
            return hashCode;
        }

        public string[] GetSections()
        {
            List<string> parts = new List<string>();

            if (name != null)
                parts.Add("name=\"" + name + "\"");
            if (type != null)
                parts.Add("type=" + type);
            if (families != null)
                foreach (string family in families)
                    parts.Add("family=" + family);
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
            a.families.AddRange(other.families);

            if (a.name == null)
                a.name = other.name;
            if (a.type == null)
                a.type = other.type;
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
