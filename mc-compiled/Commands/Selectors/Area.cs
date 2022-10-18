using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents a selector option which selects based off of location and area.
    /// </summary>
    public struct Area
    {
        public Coord? x, y, z;
        public float? radiusMin, radiusMax;
        public int? volumeX, volumeY, volumeZ;

        public Area(Coord? x, Coord? y, Coord? z, float? radiusMin = null,
            float? radiusMax = null, int? volumeX = null, int? volumeY = null, int? volumeZ = null)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.radiusMin = radiusMin;
            this.radiusMax = radiusMax;
            this.volumeX = volumeX;
            this.volumeY = volumeY;
            this.volumeZ = volumeZ;
        }
        public Area Clone() => (Area)MemberwiseClone();

        /// <summary>
        /// Parse an area from traditional minecraft input.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Area Parse(string[] chunks)
        {
            Coord?
                x = null,
                y = null,
                z = null;
            float?
                radiusMin = null,
                radiusMax = null;
            int?
                volumeX = null,
                volumeY = null,
                volumeZ = null;

            foreach(string chunk in chunks)
            {
                int index = chunk.IndexOf('=');
                if (index == -1)
                    continue;
                string a = chunk.Substring(0, index).Trim().ToUpper();
                string b = chunk.Substring(index + 1).Trim();

                switch(a)
                {
                    case "X":
                        x = Coord.Parse(b);
                        break;
                    case "Y":
                        y = Coord.Parse(b);
                        break;
                    case "Z":
                        z = Coord.Parse(b);
                        break;
                    case "RM":
                        if (float.TryParse(b, out float rm))
                            radiusMin = rm;
                        break;
                    case "R":
                        if (float.TryParse(b, out float r))
                            radiusMax = r;
                        break;
                    case "DX":
                        if (int.TryParse(b, out int dx))
                            volumeX = dx;
                        break;
                    case "DY":
                        if (int.TryParse(b, out int dy))
                            volumeY = dy;
                        break;
                    case "DZ":
                        if (int.TryParse(b, out int dz))
                            volumeZ = dz;
                        break;
                }
            }

            /*if (!x.HasValue)
                x = Coord.here;
            if (!y.HasValue)
                y = Coord.here;
            if (!z.HasValue)
                z = Coord.here;*/

            return new Area(x, y, z, radiusMin, radiusMax, volumeX, volumeY, volumeZ);
        }

        /// <summary>
        /// Get the sections necessary to build a full and valid selector.
        /// </summary>
        /// <returns></returns>
        public string[] GetSections()
        {
            List<string> parts = new List<string>();
            if (x.HasValue)
            {
                parts.Add("x=" + x.Value);
                if (volumeX.HasValue)
                    parts.Add("dx=" + volumeX.Value);
            }
            if (y.HasValue)
            {
                parts.Add("y=" + y.Value);
                if (volumeY.HasValue)
                    parts.Add("dy=" + volumeY.Value);
            }
            if (z.HasValue)
            {
                parts.Add("z=" + z.Value);
                if (volumeZ.HasValue)
                    parts.Add("dz=" + volumeZ.Value);
            }

            if (radiusMin.HasValue)
                parts.Add("rm=" + radiusMin.Value);
            if (radiusMax.HasValue)
                parts.Add("r=" + radiusMax.Value);

            return parts.ToArray();
        }

        public override bool Equals(object obj)
        {
            return obj is Area area &&
                   EqualityComparer<Coord?>.Default.Equals(x, area.x) &&
                   EqualityComparer<Coord?>.Default.Equals(y, area.y) &&
                   EqualityComparer<Coord?>.Default.Equals(z, area.z) &&
                   radiusMin == area.radiusMin &&
                   radiusMax == area.radiusMax &&
                   volumeX == area.volumeX &&
                   volumeY == area.volumeY &&
                   volumeZ == area.volumeZ;
        }
        public override int GetHashCode()
        {
            int hashCode = -114996346;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            hashCode = hashCode * -1521134295 + radiusMin.GetHashCode();
            hashCode = hashCode * -1521134295 + radiusMax.GetHashCode();
            hashCode = hashCode * -1521134295 + volumeX.GetHashCode();
            hashCode = hashCode * -1521134295 + volumeY.GetHashCode();
            hashCode = hashCode * -1521134295 + volumeZ.GetHashCode();
            return hashCode;
        }

        public static Area operator +(Area a, Area other)
        {
            if (a.x == null)
                a.x = other.x;
            if (a.y == null)
                a.y = other.y;
            if (a.z == null)
                a.z = other.z;
            if (a.radiusMin == null)
                a.radiusMin = other.radiusMin;
            if (a.radiusMax == null)
                a.radiusMax = other.radiusMax;
            if (a.volumeX == null)
                a.volumeX = other.volumeX;
            if (a.volumeY == null)
                a.volumeY = other.volumeY;
            if (a.volumeZ == null)
                a.volumeZ = other.volumeZ;
            return a;
        }


    }
}
