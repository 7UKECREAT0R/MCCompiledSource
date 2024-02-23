using System.Linq;

namespace mc_compiled.NBT
{
    public struct BlockIndicesNBT
    {
        public VectorIntNBT size;

        NBTList primaryLayer;
        NBTList secondaryLayer;

        public BlockIndicesNBT(VectorIntNBT size, NBTList primaryLayer, NBTList secondaryLayer)
        {
            this.size = size;
            this.primaryLayer = primaryLayer;
            this.secondaryLayer = secondaryLayer;
        }
        /// <summary>
        /// Parses an array of integers.
        /// </summary>
        /// <param name="blocks"></param>
        public BlockIndicesNBT(int[,,] blocks)
        {
            this.size = new VectorIntNBT()
            {
                x = blocks.GetLength(0),
                y = blocks.GetLength(1),
                z = blocks.GetLength(2)
            };

            int length = blocks.Length;
            int[] unwrap = new int[length];
            int[] empty = new int[length];
            int write = 0;

            // Unwrap 3D array.
            for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++)
            for (int z = 0; z < this.size.z; z++)
                unwrap[write++] = blocks[x, y, z];

            for (int i = 0; i < length; i++)
                empty[i] = -1;

            this.primaryLayer = new NBTList()
            {
                name = "",
                listType = TAG.Int,
                values = (from convert in unwrap select new NBTInt() { name = "", value = convert }).ToArray()
            };
            this.secondaryLayer = new NBTList()
            {
                name = "",
                listType = TAG.Int,
                values = (from convert in empty select new NBTInt() { name = "", value = convert }).ToArray()
            };
        }

        public NBTList ToNBT()
        {
            return new NBTList()
            {
                name = "block_indices",
                listType = TAG.List,
                values = new NBTList[]
                {
                    this.primaryLayer, this.secondaryLayer
                }
            };
        }
    }
}
