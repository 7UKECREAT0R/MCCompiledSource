using System.Collections.Generic;

namespace mc_compiled.Commands.Selectors;

/// <summary>
///     Represents a selection option that limits based off of entity properties.
/// </summary>
public struct Entity(
    string name,
    string type,
    IEnumerable<string> families,
    int? rotXMin = null,
    int? rotXMax = null,
    int? rotYMin = null,
    int? rotYMax = null)
{
    public string name = name; // The name of this entity/player.
    public string type = type; // The type of this entity.

    public List<string> families = [..families]; // The family(s) this entity is in.

    private int? rotXMin = rotXMin;
    private int? rotXMax = rotXMax;
    private int? rotYMin = rotYMin;
    private int? rotYMax = rotYMax;

    public static Entity Parse(string[] chunks)
    {
        string name = null;
        string type = null;

        List<string> families = [];

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
            string a = chunk[..index].Trim().ToUpper();
            string b = chunk[(index + 1)..].Trim();

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
        return this.name == other.name && this.type == other.type && Equals(this.families, other.families) &&
               this.rotXMin == other.rotXMin && this.rotXMax == other.rotXMax && this.rotYMin == other.rotYMin &&
               this.rotYMax == other.rotYMax;
    }
    public override bool Equals(object obj)
    {
        return obj is Entity other && Equals(other);
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = this.name != null ? this.name.GetHashCode() : 0;
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
        List<string> parts = [];

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

    public static bool operator ==(Entity left, Entity right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }
}