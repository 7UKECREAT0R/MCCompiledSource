using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands
{
    public struct BlockCheck
    {
        public static readonly BlockCheck DISABLED = new BlockCheck() { present = false };
        public bool present;

        public CoordinateValue x, y, z;
        public string block;
        public int? data;

        public BlockCheck(string x, string y, string z, string block = "air", string data = null)
        {
            present = true;
            this.x = CoordinateValue.Parse(x).GetValueOrDefault();
            this.y = CoordinateValue.Parse(y).GetValueOrDefault();
            this.z = CoordinateValue.Parse(z).GetValueOrDefault();
            this.block = block;
            if (data == null)
                this.data = null;
            else this.data = int.Parse(data);
        }
        public BlockCheck(CoordinateValue x, CoordinateValue y, CoordinateValue z, string block = "air", int? data = null)
        {
            present = true;
            this.x = x;
            this.y = y;
            this.z = z;
            this.block = block;
            this.data = data;
        }

        public override string ToString()
        {
            List<string> parts = new List<string>();

            parts.Add(x.ToString());
            parts.Add(y.ToString());
            parts.Add(z.ToString());
            parts.Add(block);
            int tempData = data ?? 0;
            parts.Add(tempData.ToString());

            return "detect " + string.Join(" ", parts);
        }
        /// <summary>
        /// Get this BlockCheck as a testfor statement.
        /// </summary>
        /// <returns></returns>
        public string AsStoreIn(string objective)
        {
            return $"execute @s ~~~ detect {x} {y} {z} {block} {data ?? 0} scoreboard players set @s {objective} 1";
        }
    }
}