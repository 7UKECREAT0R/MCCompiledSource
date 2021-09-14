using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public struct VectorIntNBT
    {
        public int x, y, z;

        public VectorIntNBT(int x, int y, int z)
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
                listType = TAG.Int,
                values = new NBTInt[]
                {
                    new NBTInt() { name = "", value = x },
                    new NBTInt() { name = "", value = y },
                    new NBTInt() { name = "", value = z }
                }
            };
        }
    }
}
