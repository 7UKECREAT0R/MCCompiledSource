using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors.Rewrite
{
    /// <summary>
    /// A volume, or zone, in the minecraft world to check for entities in.
    /// </summary>
    public class ZoneCheck
    {
        public Coord? x, y, z;
        public float? radiusMin, radiusMax;
        public int? dx, dy, dz;

        public ZoneCheck()
        {
            this.x = null;
            this.y = null;
            this.z = null;
            this.radiusMin = null;
            this.radiusMax = null;
            this.dx = null;
            this.dy = null;
            this.dz = null;
        }


    }
}
