using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    /// <summary>
    /// Represents a data-driven component attached to an entity.
    /// </summary>
    public struct AttributeNBT
    {
        public float baseValue;     // default value
        public float currentValue;  // current value
        public float max, min;      // maximum and minimum
        public float defaultMax;    // default maximum
        public float defaultMin;    // default minimum
        public string identifier;   // component identifier

        public AttributeNBT(float baseValue, float currentValue, float min, float max, float defaultMin, float defaultMax, string identifier)
        {
            this.baseValue = baseValue;
            this.currentValue = currentValue;
            this.max = max;
            this.min = min;
            this.defaultMax = defaultMax;
            this.defaultMin = defaultMin;
            this.identifier = identifier;
        }
        public AttributeNBT(float value, float min, float max, string identifier)
        {
            this.baseValue = value;
            this.currentValue = value;
            this.max = max;
            this.min = min;
            this.defaultMax = max;
            this.defaultMin = min;
            this.identifier = identifier;
        }

        public enum MovementType
        {
            LAND,
            UNDERWATER,
            UNDERLAVA
        }

        public static AttributeNBT CreateLuck() =>
            new AttributeNBT(0, -1024, 1024, "minecraft:luck");
        public static AttributeNBT CreateHealth(int health) =>
            new AttributeNBT(health, 0, health, "minecraft:health");
        public static AttributeNBT CreateAbsorption() =>
            new AttributeNBT(0, 0, 16, "minecraft:absorption");
        public static AttributeNBT CreateKnockbackResistance() =>
            new AttributeNBT(0, 0, 1, "minecraft:knockback_resistance");
        public static AttributeNBT CreateMovement(float speed, MovementType type = MovementType.LAND)
        {
            switch (type)
            {
                case MovementType.UNDERWATER:
                    return new AttributeNBT(speed, 0, float.MaxValue, "minecraft:underwater_movement");
                case MovementType.UNDERLAVA:
                    return new AttributeNBT(speed, 0, float.MaxValue, "minecraft:lava_movement");
                case MovementType.LAND:
                default:
                    return new AttributeNBT(speed, 0, float.MaxValue, "minecraft:movement");
            }
        }
        public static AttributeNBT CreateFollowRange(float range)
        {
            return new AttributeNBT(range, 0, 2048, "minecraft:follow_range");
        }

        public NBTCompound ToNBT()
        {
            return new NBTCompound()
            {
                name = "",
                values = new NBTNode[]
                {
                    new NBTFloat() { name = "Base", value = baseValue },
                    new NBTFloat() { name = "Current", value = currentValue },
                    new NBTFloat() { name = "DefaultMax", value = defaultMax },
                    new NBTFloat() { name = "DefaultMin", value = defaultMin },
                    new NBTFloat() { name = "Max", value = max },
                    new NBTFloat() { name = "Min", value = min },
                    new NBTString() { name = "Name", value = identifier },
                    new NBTEnd()
                }
            };
        }
    }
}
