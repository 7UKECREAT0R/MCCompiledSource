using System;
using System.Collections.Generic;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents a selector option which selects based off of location and area.
    /// </summary>
    public struct Area
    {
        public Coordinate? x, y, z;
        public float? radiusMin, radiusMax;
        public int? volumeX, volumeY, volumeZ;

        public Area(Coordinate? x, Coordinate? y, Coordinate? z, float? radiusMin = null,
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
        /// <param name="chunks"></param>
        /// <returns></returns>
        public static Area Parse(string[] chunks)
        {
            Coordinate?
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
                        x = Coordinate.Parse(b);
                        break;
                    case "Y":
                        y = Coordinate.Parse(b);
                        break;
                    case "Z":
                        z = Coordinate.Parse(b);
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

        public bool Equals(Area other)
        {
            return Nullable.Equals(x, other.x) && Nullable.Equals(y, other.y) && Nullable.Equals(z, other.z) &&
                   Nullable.Equals(radiusMin, other.radiusMin) && Nullable.Equals(radiusMax, other.radiusMax) &&
                   volumeX == other.volumeX && volumeY == other.volumeY && volumeZ == other.volumeZ;
        }
        public override bool Equals(object obj)
        {
            return obj is Area other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                hashCode = (hashCode * 397) ^ z.GetHashCode();
                hashCode = (hashCode * 397) ^ radiusMin.GetHashCode();
                hashCode = (hashCode * 397) ^ radiusMax.GetHashCode();
                hashCode = (hashCode * 397) ^ volumeX.GetHashCode();
                hashCode = (hashCode * 397) ^ volumeY.GetHashCode();
                hashCode = (hashCode * 397) ^ volumeZ.GetHashCode();
                return hashCode;
            }
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
