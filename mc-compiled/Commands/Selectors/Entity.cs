using System.Collections.Generic;

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

        private int? rotXMin;
        private int? rotXMax;
        private int? rotYMin;
        private int? rotYMax;

        public Entity(string name, string type, IEnumerable<string> families,
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

        public bool Equals(Entity other)
        {
            return this.name == other.name && this.type == other.type && Equals(this.families, other.families) && this.rotXMin == other.rotXMin && this.rotXMax == other.rotXMax && this.rotYMin == other.rotYMin && this.rotYMax == other.rotYMax;
        }
        public override bool Equals(object obj)
        {
            return obj is Entity other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (this.name != null ? this.name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.type != null ? this.type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.families != null ? this.families.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.rotXMin.GetHashCode();
                hashCode = (hashCode * 397) ^ this.rotXMax.GetHashCode();
                hashCode = (hashCode * 397) ^ this.rotYMin.GetHashCode();
                hashCode = (hashCode * 397) ^ this.rotYMax.GetHashCode();
                return hashCode;
            }
        }

        public string[] GetSections()
        {
            List<string> parts = new List<string>();

            if (this.name != null)
                parts.Add("name=\"" + this.name + "\"");
            if (this.type != null)
                parts.Add("type=" + this.type);
            if (this.families != null)
                foreach (string family in this.families)
                    parts.Add("family=" + family);
            if (this.rotXMin.HasValue)
                parts.Add("rxm=" + this.rotXMin.Value);
            if (this.rotXMax.HasValue)
                parts.Add("rx=" + this.rotXMax.Value);
            if (this.rotYMin.HasValue)
                parts.Add("rym=" + this.rotYMin.Value);
            if (this.rotYMax.HasValue)
                parts.Add("ry=" + this.rotYMax.Value);
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
