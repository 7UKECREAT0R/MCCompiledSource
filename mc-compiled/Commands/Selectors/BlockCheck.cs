using System.Collections.Generic;

namespace mc_compiled.Commands.Selectors
{
    public struct BlockCheck
    {
        public static readonly BlockCheck DISABLED = new BlockCheck() { present = false };
        /// <summary>
        /// only false if this blockcheck is disabled
        /// </summary>
        public bool present;

        public Coordinate x, y, z;
        public string block;
        public int? data;

        public BlockCheck(string x, string y, string z, string block = "air", string data = null)
        {
            this.present = true;
            this.x = Coordinate.Parse(x).GetValueOrDefault();
            this.y = Coordinate.Parse(y).GetValueOrDefault();
            this.z = Coordinate.Parse(z).GetValueOrDefault();
            this.block = block;
            if (data == null || data.Equals("0") || string.IsNullOrEmpty(data))
                this.data = null;
            else
                this.data = int.Parse(data);
        }
        public BlockCheck(Coordinate x, Coordinate y, Coordinate z, string block = "air", int? data = null)
        {
            this.present = true;
            this.x = x;
            this.y = y;
            this.z = z;
            this.block = block;
            this.data = data;
        }

        public override string ToString()
        {
            List<string> parts = new List<string>();

            parts.Add(this.x.ToString());
            parts.Add(this.y.ToString());
            parts.Add(this.z.ToString());
            parts.Add(this.block);
            int tempData = this.data ?? 0;
            parts.Add(tempData.ToString());

            return "detect " + string.Join(" ", parts);
        }
        /// <summary>
        /// Get this BlockCheck as a testfor statement.
        /// </summary>
        /// <returns></returns>
        public string AsStoreIn(string selector, string objective)
        {
            return $"execute {selector} ~~~ detect {this.x} {this.y} {this.z} {this.block} {this.data ?? 0} scoreboard players set @s {objective} 1";
        }

        public override bool Equals(object obj)
        {
            return obj is BlockCheck check && this.present == check.present &&
                   EqualityComparer<Coordinate>.Default.Equals(this.x, check.x) &&
                   EqualityComparer<Coordinate>.Default.Equals(this.y, check.y) &&
                   EqualityComparer<Coordinate>.Default.Equals(this.z, check.z) && this.block == check.block && this.data == check.data;
        }
        public override int GetHashCode()
        {
            if (!this.present)
                return 851597659;

            int hashCode = 851597659;
            hashCode = hashCode * -1521134295 + this.present.GetHashCode();
            hashCode = hashCode * -1521134295 + this.x.GetHashCode();
            hashCode = hashCode * -1521134295 + this.y.GetHashCode();
            hashCode = hashCode * -1521134295 + this.z.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.block);
            hashCode = hashCode * -1521134295 + this.data.GetHashCode();
            return hashCode;
        }
    }
}