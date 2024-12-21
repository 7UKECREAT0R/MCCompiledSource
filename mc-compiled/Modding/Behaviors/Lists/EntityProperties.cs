using System.Linq;
using mc_compiled.Commands;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors;
// https://docs.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/propertieslist

public class ComponentAmbientSoundInterval : EntityComponent
{
    public string eventName;
    public float range, value;

    public override string GetIdentifier()
    {
        return "minecraft:ambient_sound_interval";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["event_name"] = this.eventName,
            ["range"] = this.range,
            ["value"] = this.value
        };
    }
}

public class ComponentCanClimb : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:can_climb";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentCanFly : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:can_fly";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentCanPowerJump : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:can_power_jump";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentCollisionBox : EntityComponent
{
    public float height;
    public float width;

    public override string GetIdentifier()
    {
        return "minecraft:collision_box";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["width"] = this.width,
            ["height"] = this.height
        };
    }
}

public class ComponentColor : EntityComponent
{
    public int color;

    public override string GetIdentifier()
    {
        return "minecraft:color";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.color
        };
    }
}

public class ComponentColor2 : EntityComponent
{
    public int color;

    public override string GetIdentifier()
    {
        return "minecraft:color2";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.color
        };
    }
}

public class ComponentDefaultLookAngle : EntityComponent
{
    public float angle;

    public override string GetIdentifier()
    {
        return "minecraft:default_look_angle";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.angle
        };
    }
}

public class ComponentEquipment : EntityComponent
{
    public SlotDropChance[] dropChances;

    public LootTable lootTable;

    public override string GetIdentifier()
    {
        return "minecraft:equipment";
    }
    public override JObject _GetValue()
    {
        if (this.dropChances == null)
            return new JObject
            {
                ["table"] = this.lootTable.ResourcePath
            };

        return new JObject
        {
            ["table"] = this.lootTable.ResourcePath,
            ["slot_drop_chance"] = new JArray(this.dropChances.Select(dc => dc.ToJSON()))
        };
    }

    public struct SlotDropChance
    {
        public ItemSlot slot;
        public float dropChance; // 0.0 - 1.0

        public JObject ToJSON()
        {
            return new JObject
            {
                ["slot"] = this.slot.String(),
                ["drop_chance"] = this.dropChance
            };
        }
    }
}

public class ComponentFireImmune : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:file_immune";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentFloats : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:floats_in_liquid";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentFlyingSpeed : EntityComponent
{
    public float flySpeed;

    public override string GetIdentifier()
    {
        return "minecraft:flying_speed";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.flySpeed
        };
    }
}

public class ComponentFrictionModifier : EntityComponent
{
    public float friction; // 0.0 - 1.0
    public override string GetIdentifier()
    {
        return "minecraft:friction_modifier";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.friction
        };
    }
}

public class ComponentGroundOffset : EntityComponent
{
    public float groundOffset = 1f;

    public override string GetIdentifier()
    {
        return "minecraft:ground_offset";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.groundOffset
        };
    }
}

public class ComponentInputGroundControlled : EntityComponent
{
    // This component allows the use of the entity like a horse; full WASD movement.
    public override string GetIdentifier()
    {
        return "minecraft:input_ground_controlled";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsBaby : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_baby";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsCharged : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_charged";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsChested : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_chested";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsDyeable : EntityComponent
{
    // The text that will display when interacting with this
    // entity with a dye when playing with Touch-screen controls.
    // This can be a lang file entry.
    public string interactText;

    public override string GetIdentifier()
    {
        return "minecraft:is_dyeable";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["interact_text"] = this.interactText
        };
    }
}

public class ComponentIsHiddenWhenInvisible : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_hidden_when_invisible";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsIgnited : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_ignited";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsIllagerCaptain : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_illager_captain";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsSaddled : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_saddled";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsShaking : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_shaking";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsSheared : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_sheared";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsStackable : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_stackable";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsStunned : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_stunned";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentIsTamed : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:is_tamed";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}

public class ComponentItemControllable : EntityComponent
{
    // This component allows the entity to be controlled similarly to a pig with a carrot-on-a-stick.
    public string[] controlItems;
    public override string GetIdentifier()
    {
        return "minecraft:item_controllable";
    }
    public override JObject _GetValue()
    {
        if (this.controlItems.Length == 1)
            return new JObject
            {
                ["control_items"] = this.controlItems[0]
            };

        return new JObject
        {
            ["control_items"] = new JArray(this.controlItems.Cast<object>().ToArray())
        };
    }
}

public class ComponentLoot : EntityComponent
{
    // Loot to drop upon death.
    public LootTable lootTable;

    public override string GetIdentifier()
    {
        return "minecraft:loot";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["table"] = this.lootTable.ResourcePath
        };
    }
}

public class ComponentMarkVariant : EntityComponent
{
    public int markVariant;

    public override string GetIdentifier()
    {
        return "minecraft:mark_variant";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.markVariant
        };
    }
}

public class ComponentPushThrough : EntityComponent
{
    // ???
    public float value;

    public override string GetIdentifier()
    {
        return "minecraft:push_through";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.value
        };
    }
}

public class ComponentScale : EntityComponent
{
    public float scale = 1f;

    public override string GetIdentifier()
    {
        return "minecraft:scale";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.scale
        };
    }
}

public class ComponentSkinID : EntityComponent
{
    public int skinID;

    public override string GetIdentifier()
    {
        return "minecraft:skin_id";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.skinID
        };
    }
}

public class ComponentFamily : EntityComponent
{
    public string[] families;

    public override string GetIdentifier()
    {
        return "minecraft:type_family";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["family"] = new JArray(this.families.Cast<object>().ToArray())
        };
    }
}

public class ComponentVariant : EntityComponent
{
    public int variant;

    public override string GetIdentifier()
    {
        return "minecraft:variant";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.variant
        };
    }
}

public class ComponentWalkAnimationSpeed : EntityComponent
{
    public float speed = 1f;

    public override string GetIdentifier()
    {
        return "minecraft:walk_animation_speed";
    }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["value"] = this.speed
        };
    }
}

public class ComponentWantsJockey : EntityComponent
{
    public override string GetIdentifier()
    {
        return "minecraft:wants_jockey";
    }
    public override JObject _GetValue()
    {
        return new JObject();
    }
}