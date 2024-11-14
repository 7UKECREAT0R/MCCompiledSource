using System;
using System.Collections.Generic;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents a selector option which selects based off of location and area.
    /// </summary>
    public struct Area(
        Coordinate? x,
        Coordinate? y,
        Coordinate? z,
        float? radiusMin = null,
        float? radiusMax = null,
        int? volumeX = null,
        int? volumeY = null,
        int? volumeZ = null)
    {
        public Coordinate? x = x, y = y, z = z;
        public float? radiusMin = radiusMin, radiusMax = radiusMax;
        public int? volumeX = volumeX, volumeY = volumeY, volumeZ = volumeZ;

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
                string a = chunk[..index].Trim().ToUpper();
                string b = chunk[(index + 1)..].Trim();

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
            List<string> parts = [];
            if (this.x.HasValue)
            {
                parts.Add("x=" + this.x.Value);
                if (this.volumeX.HasValue)
                    parts.Add("dx=" + this.volumeX.Value);
            }
            if (this.y.HasValue)
            {
                parts.Add("y=" + this.y.Value);
                if (this.volumeY.HasValue)
                    parts.Add("dy=" + this.volumeY.Value);
            }
            if (this.z.HasValue)
            {
                parts.Add("z=" + this.z.Value);
                if (this.volumeZ.HasValue)
                    parts.Add("dz=" + this.volumeZ.Value);
            }

            if (this.radiusMin.HasValue)
                parts.Add("rm=" + this.radiusMin.Value);
            if (this.radiusMax.HasValue)
                parts.Add("r=" + this.radiusMax.Value);

            return parts.ToArray();
        }

        public bool Equals(Area other)
        {
            return Nullable.Equals(this.x, other.x) && Nullable.Equals(this.y, other.y) && Nullable.Equals(this.z, other.z) &&
                   Nullable.Equals(this.radiusMin, other.radiusMin) && Nullable.Equals(this.radiusMax, other.radiusMax) && this.volumeX == other.volumeX && this.volumeY == other.volumeY && this.volumeZ == other.volumeZ;
        }
        public override bool Equals(object obj)
        {
            return obj is Area other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.x.GetHashCode();
                hashCode = (hashCode * 397) ^ this.y.GetHashCode();
                hashCode = (hashCode * 397) ^ this.z.GetHashCode();
                hashCode = (hashCode * 397) ^ this.radiusMin.GetHashCode();
                hashCode = (hashCode * 397) ^ this.radiusMax.GetHashCode();
                hashCode = (hashCode * 397) ^ this.volumeX.GetHashCode();
                hashCode = (hashCode * 397) ^ this.volumeY.GetHashCode();
                hashCode = (hashCode * 397) ^ this.volumeZ.GetHashCode();
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

        public static bool operator ==(Area left, Area right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Area left, Area right)
        {
            return !(left == right);
        }
    }
}
