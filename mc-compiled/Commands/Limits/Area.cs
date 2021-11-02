using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Limits
{
    /// <summary>
    /// Represents a selector option which selects based off of location and area.
    /// </summary>
    public struct Area
    {
        public CoordinateValue? x, y, z;
        public float? radiusMin, radiusMax;
        public int? volumeX, volumeY, volumeZ;

        public Area(CoordinateValue x, CoordinateValue y, CoordinateValue z, float? radiusMin = null,
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

        /// <summary>
        /// Parse an area from traditional minecraft input.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Area Parse(string str)
        {
            str = str.Trim().TrimStart('[').TrimEnd(']');
            string[] specifiedParts = str.Split(',');

            CoordinateValue?
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

            IEnumerable<string> parts = from s in specifiedParts select s.Trim();
            foreach(string part in parts)
            {
                int index = part.IndexOf('=');
                if (index == -1)
                    continue;
                string a = part.Substring(0, index).Trim().ToUpper();
                string b = part.Substring(index + 1).Trim();

                switch(a)
                {
                    case "X":
                        x = CoordinateValue.Parse(b);
                        break;
                    case "Y":
                        y = CoordinateValue.Parse(b);
                        break;
                    case "Z":
                        z = CoordinateValue.Parse(b);
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

            if (!x.HasValue)
                x = CoordinateValue.here;
            if (!y.HasValue)
                y = CoordinateValue.here;
            if (!z.HasValue)
                z = CoordinateValue.here;

            return new Area(x.Value, y.Value, z.Value, radiusMin, radiusMax, volumeX, volumeY, volumeZ);
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
    }
}
