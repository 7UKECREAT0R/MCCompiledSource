using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.NBT
{
    public struct EntityNBT
    {
        public short age;               // Ticks this entity has lived.
        public short air;               // Amount of air the entity has left.
        public EquipmentNBT[] armor;    // The armor equipped on the entity.
        public short attackTime;        // (?) instance hit delay?
        public List<AttributeNBT> attributes; // looks like interface for data-driven stuff. 
        public float bodyRot;           // Base part rotation Y
        public int breedCooldown;       // Time between breed ticks
        public bool chested;            // (?) Unknown
        public byte color;              // The color of this entity.
        public byte color2;             // The alternate color of this entity.
        public string customName;       // The nametag for this entity.
        public bool customNameVisible;  // If the nametag should be visible to the player.
        public bool dead;               // Is this entity is dead?
        public short deathTime;         // (?) probably for despawning
        public float fallDistance;      // The distance this entity has been falling for.
        public short hurtTime;          // (?) instance hurt delay?
        public int inLove;              // remaining ticks inlove for
        public short fire;              // The ticks left of fire on this entity
        public short health;            // The health of this entity.
        public bool invulnerable;       // Whether this entity is invulnerable
        public bool isAngry;            // Is this entity angry at a player?
        public bool isAutonomous;       // (?) [NoAI Maybe?]
        public bool isBaby;             // Is this entity a baby?
        public bool isEating;           // Is this entity in the middle of eating?
        public bool isGliding;          // Is this entity gliding on an elytra?
        public bool isGlobal;           // (?)
        public bool isIllagerCaptain;   // Is this entity an illager captain? (bad omen)
        public bool isOrphaned;         // Is this entity an orphaned animal?
        public bool isOutOfControl;     // Is this entity going wild?
        public bool isRoaring;          // Is this entity roaring?
        public bool isScared;           // Is this entity scared?
        public bool isStunned;          // Is this entity stunned?
        public bool isSwimming;         // Is this entity in the middle of swimming?
        public bool isTamed;            // Has this entity been tamed?
        public bool isTrusting;         // Does this entity trust players?
        public ItemNBT? item;           // If this entity is an item, specify.
        public int lastDimensionID;     // The dimension this entity was in last tick.
        public bool lootDropped;        // Whether this entity has dropped its loot yet.
        public int markVariant;         // (?)
        public VectorNBT motion;        // The current velocity of this entity.
        public bool onGround;           // If this entity is currently on the ground.
        public long ownerID;            // The owner of this pet, if any. -1 otherwise.
        public long ownerNew;           // (?)
        public int portalCooldown;      // Ticks until entity can travel a nether portal again.
        public VectorNBT pos;           // The position of this entity in the world.
        public RotationNBT rotation;    // The current rotation of this entity.
        public bool saddled;            // Whether entity has a saddle (if applicable.)
        public bool sheared;            // Whether entity is sheared (if sheep.)
        public bool showBottom;         // Should the bottom of this entity be rendered?
        public bool sitting;            // Is this entity sitting?
        public int skinID;              // (?)
        public int strength;            // (?)
        public int strengthMax;         // (?)
        public byte[] tags;             // Entity tags. Unknown type.
        public long uniqueID;           // The unique identifier for this entity.
        public int variant;             // (?)
        public string[] definitions;      // Entity definitions. Unknown type.
        public string identifier;       // The actual namespaced ID of this entity (minecraft:pig)

        public EntityNBT(short age = 200, short air = 300, short attackTime = 0, bool chested = false, float bodyRot = 0f, int breedCooldown = 0,
            string customName = null, bool customNameVisible = true, bool dead = false, short deathTime = 0, short hurtTime = 0, int inLove = 0,
            byte color = 0, byte color2 = 0, float fallDistance = 0, short fire = 0,
            short health = 20, bool invulnerable = false, bool isAngry = false, bool isAutonomous = false, bool isBaby = false, bool isEating = false,
            bool isGliding = false, bool isGlobal = false, bool isIllagerCaptain = false, bool isOrphaned = false, bool isOutOfControl = false,
            bool isRoaring = false, bool isScared = false, bool isStunned = false, bool isSwimming = false, bool isTamed = false, bool isTrusting = false,
            ItemNBT? item = null, int lastDimensionID = 0, bool lootDropped = false, int markVariant = 0, VectorNBT motion = default, bool onGround = true,
            long ownerID = -1, long ownerNew = -1, int portalCooldown = 0, VectorNBT pos = default, RotationNBT rotation = default, bool saddled = false,
            bool sheared = false, bool showBottom = false, bool sitting = false, int skinID = 0, int strength = 0, int strengthMax = 0, byte[] tags = null,
            long uniqueID = 123456, int variant = 0, string[] definitions = null, string identifier = "minecraft:armor_stand")
        {
            EquipmentNBT[] armor = new EquipmentNBT[4];
            armor[0] = new EquipmentNBT(0);
            armor[1] = new EquipmentNBT(0);
            armor[2] = new EquipmentNBT(0);
            armor[3] = new EquipmentNBT(0);

            this.age = age;
            this.air = air;
            this.armor = armor;
            this.attackTime = attackTime;
            this.attributes = new List<AttributeNBT>();
            this.bodyRot = bodyRot;
            this.breedCooldown = breedCooldown;
            this.chested = chested;
            this.color = color;
            this.color2 = color2;
            this.customName = customName;
            this.customNameVisible = customNameVisible;
            this.dead = dead;
            this.deathTime = deathTime;
            this.fallDistance = fallDistance;
            this.hurtTime = hurtTime;
            this.inLove = inLove;
            this.fire = fire;
            this.health = health;
            this.invulnerable = invulnerable;
            this.isAngry = isAngry;
            this.isAutonomous = isAutonomous;
            this.isBaby = isBaby;
            this.isEating = isEating;
            this.isGliding = isGliding;
            this.isGlobal = isGlobal;
            this.isIllagerCaptain = isIllagerCaptain;
            this.isOrphaned = isOrphaned;
            this.isOutOfControl = isOutOfControl;
            this.isRoaring = isRoaring;
            this.isScared = isScared;
            this.isStunned = isStunned;
            this.isSwimming = isSwimming;
            this.isTamed = isTamed;
            this.isTrusting = isTrusting;
            this.item = item;
            this.lastDimensionID = lastDimensionID;
            this.lootDropped = lootDropped;
            this.markVariant = markVariant;
            this.motion = motion;
            this.onGround = onGround;
            this.ownerID = ownerID;
            this.ownerNew = ownerNew;
            this.portalCooldown = portalCooldown;
            this.pos = pos;
            this.rotation = rotation;
            this.saddled = saddled;
            this.sheared = sheared;
            this.showBottom = showBottom;
            this.sitting = sitting;
            this.skinID = skinID;
            this.strength = strength;
            this.strengthMax = strengthMax;
            this.tags = tags;
            this.uniqueID = uniqueID;
            this.variant = variant;
            this.definitions = definitions;
            this.identifier = identifier;
        }

        // oh boy here we go...
        public NBTCompound ToNBT(string name)
        {
            List<NBTNode> nodes = new List<NBTNode>();
            nodes.Add(new NBTShort() { name = "Age", value = age });
            nodes.Add(new NBTShort() { name = "Air", value = air });
            nodes.Add(new NBTShort() { name = "AttackTime", value = attackTime });
            nodes.Add(new NBTFloat() { name = "BodyRot", value = bodyRot });
            nodes.Add(new NBTInt() { name = "BreedCooldown", value = breedCooldown });

            nodes.Add(new NBTList()
            {
                name = "Armor",
                listType = TAG.Compound,
                values = armor.Select(tag => tag.ToNBT()).ToArray()
            });
            nodes.Add(new NBTList()
            {
                name = "Attributes",
                listType = TAG.Compound,
                values = attributes.Select(attrib => attrib.ToNBT()).ToArray()
            });

            nodes.Add(new NBTByte() { name = "Chested", value = (byte)(chested ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "Color", value = color });
            nodes.Add(new NBTByte() { name = "Color2", value = color2 });

            if (customName != null)
            {
                nodes.Add(new NBTString() { name = "CustomName", value = customName });
                nodes.Add(new NBTByte() { name = "CustomNameVisible", value = (byte)(customNameVisible ? 1 : 0) });
            }

            nodes.Add(new NBTByte() { name = "Dead", value = (byte)(dead ? 1 : 0) });
            nodes.Add(new NBTShort() { name = "DeathTime", value = deathTime });
            nodes.Add(new NBTFloat() { name = "FallDistance", value = fallDistance });
            nodes.Add(new NBTShort() { name = "HurtTime", value = hurtTime });
            nodes.Add(new NBTInt() { name = "InLove", value = inLove });
            nodes.Add(new NBTShort() { name = "Fire", value = fire });
            nodes.Add(new NBTShort() { name = "Health", value = health });
            nodes.Add(new NBTByte() { name = "Invulnerable", value = (byte)(invulnerable ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsAngry", value = (byte)(isAngry ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsAutonomous", value = (byte)(isAutonomous ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsBaby", value = (byte)(isBaby ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsEating", value = (byte)(isEating ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsGliding", value = (byte)(isGliding ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsGlobal", value = (byte)(isGlobal ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsIllagerCaptain", value = (byte)(isIllagerCaptain ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsOrphaned", value = (byte)(isOrphaned ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsOutOfControl", value = (byte)(isOutOfControl ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsRoaring", value = (byte)(isRoaring ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsScared", value = (byte)(isScared ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsStunned", value = (byte)(isStunned ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsSwimming", value = (byte)(isSwimming ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsTamed", value = (byte)(isTamed ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "IsTrusting", value = (byte)(isTrusting ? 1 : 0) });
            
            if(item.HasValue)
                nodes.Add(item.Value.ToNBT());

            nodes.Add(new NBTInt() { name = "LastDimensionId", value = lastDimensionID });
            nodes.Add(new NBTByte() { name = "LootDropped", value = (byte)(lootDropped ? 1 : 0) });
            nodes.Add(new NBTInt() { name = "MarkVariant", value = markVariant });
            nodes.Add(motion.ToNBT("Motion"));
            nodes.Add(new NBTByte() { name = "OnGround", value = (byte)(onGround ? 1 : 0) });
            nodes.Add(new NBTLong() { name = "OwnerID", value = ownerID });
            nodes.Add(new NBTLong() { name = "OwnerNew", value = ownerNew });
            nodes.Add(new NBTInt() { name = "PortalCooldown", value = portalCooldown });
            nodes.Add(pos.ToNBT("Pos"));
            nodes.Add(rotation.ToNBT("Rotation"));
            nodes.Add(new NBTByte() { name = "Saddled", value = (byte)(saddled ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "Sheared", value = (byte)(sheared ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "ShowBottom", value = (byte)(showBottom ? 1 : 0) });
            nodes.Add(new NBTByte() { name = "Sitting", value = (byte)(sitting ? 1 : 0) });
            nodes.Add(new NBTInt() { name = "SkinID", value = skinID });
            nodes.Add(new NBTInt() { name = "Strength", value = strength });
            nodes.Add(new NBTInt() { name = "StrengthMax", value = strengthMax });

            if (tags == null)
                tags = new byte[0];
            if (definitions == null)
                definitions = new string[0];

            nodes.Add(new NBTList()
            {
                name = "Tags",
                listType = TAG.Byte,
                values = (from t in tags select new NBTByte() { name = "", value = t }).ToArray()
            });
            nodes.Add(new NBTLong() { name = "UniqueID", value = uniqueID });
            nodes.Add(new NBTInt() { name = "Variant", value = variant });
            nodes.Add(new NBTList()
            {
                name = "definitions",
                listType = TAG.String,
                values = (from t in definitions select new NBTString() { name = "", value = t }).ToArray()
            });
            nodes.Add(new NBTString() { name = "identifier", value = identifier });
            nodes.Add(new NBTEnd());

            return new NBTCompound()
            {
                name = name,
                values = nodes.ToArray()
            };
        }
    }
}
