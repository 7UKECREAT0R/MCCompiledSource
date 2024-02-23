namespace mc_compiled.NBT
{
    /// <summary>
    /// A local 2-axis rotation.
    /// </summary>
    public struct RotationNBT
    {
        public float x, z;

        public RotationNBT(float x, float z)
        {
            this.x = x;
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
                    new NBTFloat() { name = "", value = this.x },
                    new NBTFloat() { name = "", value = this.z }
                }
            };
        }
    }
}
