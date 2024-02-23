namespace mc_compiled.NBT
{
    public struct PaletteEntryNBT
    {
        public string name;
        public NBTNode[] states;
        public int version;

        public PaletteEntryNBT(string name, NBTNode[] states = null, int version = 17879555)
        {
            this.name = name;
            this.states = states;
            this.version = version;
        }

        public NBTCompound ToNBT(string name)
        {
            if (this.states == null) this.states = new NBTNode[] { new NBTEnd() };

            NBTCompound state = new NBTCompound()
            {
                name = "states",
                values = this.states
            };

            return new NBTCompound()
            {
                name = name,
                values = new NBTNode[]
                {
                    new NBTString() { name = "name", value = this.name },
                    state,
                    new NBTInt() { name = "version", value = this.version },
                    new NBTEnd()
                }
            };
        }
    }
}
