using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    /// <summary>
    /// A 3d vector.
    /// </summary>
    public struct VectorNBT
    {
        public float x, y, z;

        public VectorNBT(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public NBTList ToNBT(string name)
        {
            return new NBTList()
            {
                name = name,
                listType = TAG.Float,
                values = new NBTFloat[]
                {
                    new NBTFloat() { name = "", value = x },
                    new NBTFloat() { name = "", value = y },
                    new NBTFloat() { name = "", value = z }
                }
            };
        }
    }
}
