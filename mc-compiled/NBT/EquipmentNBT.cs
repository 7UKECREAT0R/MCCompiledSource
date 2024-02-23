namespace mc_compiled.NBT
{
    /// <summary>
    /// Represents an item of equipment on an entity.
    /// </summary>
    public struct EquipmentNBT
    {
        public byte count;
        public short damage;
        public string name;
        public bool wasPickedUp;

        public EquipmentNBT(byte count, short damage = 0, string name = "", bool wasPickedUp = false)
        {
            this.count = count;
            this.damage = damage;
            this.name = name;
            this.wasPickedUp = wasPickedUp;
        }
        public NBTCompound ToNBT()
        {
            return new NBTCompound()
            {
                name = "",
                values = new NBTNode[]
                {
                    new NBTByte() { name = "Count", value = this.count },
                    new NBTShort() { name = "Damage", value = this.damage },
                    new NBTString() { name = "Name", value = this.name },
                    new NBTByte() { name = "WasPickedUp", value = (byte)(this.wasPickedUp ? 1 : 0) },
                    new NBTEnd()
                }
            };
        }
    }
}
