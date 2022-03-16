using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors.Lists
{
    // https://docs.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/componentlist

    public class ComponentAddRider : EntityComponent
    {
        public string entityType;
        public string spawnEvent;

        public override string GetIdentifier() =>
            "minecraft:addrider";
        public override JObject _GetValue()
        {
            var json = new JObject()
            {
                ["entity_type"] = entityType
            };

            if (spawnEvent != null)
                json["spawn_event"] = spawnEvent;

            return json;
        }
    }
    public class ComponentAdmireItem : EntityComponent
    {
        public int cooldownAfterBeingAttacked; // seconds
        public int duration;

        public override string GetIdentifier() =>
            "minecraft:admire_item";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["cooldown_after_being_attacked"] = cooldownAfterBeingAttacked,
                ["duration"] = duration
            };
        }
    }
    public class ComponentAgeable : EntityComponent
    {
        public string[] dropItems; // items it drops when it grows up
        public float duration;
        public string[] feedItems;
        public EntityEventHandler? growUpEvent;
        
        public override string GetIdentifier() =>
            "minecraft:ageable";
        public override JObject _GetValue()
        {
            var json = new JObject()
            {
                ["duration"] = duration
            };
            if (dropItems != null && dropItems.Length > 0)
                json["drop_items"] = new JArray(dropItems);
            if (feedItems != null && feedItems.Length > 0)
                json["feed_items"] = new JArray(feedItems);
            if (growUpEvent.HasValue)
                json["grow_up"] = growUpEvent.Value.ToJSON();

            return json;
        }
    }
    public class ComponentAngry : EntityComponent
    {
        public string angrySound;
        public bool broadcastAnger;
        public bool broadcastAngerOnAttack;
        public bool broadcastAngerOnBeingAttacked;
        public FilterCollection broadcastFilters;
        public int broadcastRange;
        public string[] broadcastFamilies;
        public int angerDuration;
        public int angerDurationDelta; // [+/-]random(delta)
        public FilterCollection ignoreEntities;

        public override string GetIdentifier() =>
            "minecraft:angry";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["broadcast_anger"] = broadcastAnger,
                ["broadcast_anger_on_attack"] = broadcastAngerOnAttack,
                ["broadcast_anger_on_being_attacked"] = broadcastAngerOnBeingAttacked,
                ["broadcast_range"] = broadcastRange,
                ["duration"] = angerDuration,
                ["duration_delta"] = angerDurationDelta
            };
            if (angrySound != null)
                json["angry_sound"] = angrySound;
            if (broadcastFilters != null)
                json["broadcast_filters"] = broadcastFilters.ToJSON();
            if (broadcastFamilies != null)
                json["broadcast_targets"] = new JArray(broadcastFamilies);
            if (ignoreEntities != null)
                json["filters"] = ignoreEntities.ToJSON();

            return json;
        }
    }
    public class ComponentAnnotationBreakDoor : EntityComponent
    {
        public float breakTime;
        public Commands.DifficultyMode difficulty;

        public override string GetIdentifier() =>
            "minecraft:annotation.break_door";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["break_time"] = breakTime,
                ["min_difficulty"] = difficulty.ToString()
            };
        }
    }
    public class ComponentAnnotationOpenDoor : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:annotation.open_door";
        public override JObject _GetValue()
        {
            return new JObject() { };
        }
    }
    public class ComponentAreaAttack : EntityComponent
    {
        public Commands.DamageCause? cause;
        public FilterCollection entityFilter;
        public int damagePerTick;
        public float damageRange;

        public override string GetIdentifier() =>
            "minecraft:area_attack";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["damage_per_tick"] = damagePerTick,
                ["damage_range"] = damageRange
            };
            if (entityFilter != null)
                json["entity_filter"] = entityFilter.ToJSON();
            if (cause.HasValue)
                json["cause"] = cause.Value.ToString();

            return json;
        }
    }
    public class ComponentAttackCooldown : EntityComponent
    {
        public EntityEventHandler? onCooldownComplete;
        public float primaryCooldownTime;
        public float? minCooldownTime;

        public override string GetIdentifier() =>
            "minecraft:attack_cooldown";
        public override JObject _GetValue()
        {
            JObject json = new JObject();

            if (minCooldownTime.HasValue)
                json["attack_cooldown_time"] = new JArray(new float[]
                { minCooldownTime.Value, primaryCooldownTime });
            else
                json["attack_cooldown_time"] = primaryCooldownTime;

            if (onCooldownComplete.HasValue)
                json["attack_cooldown_complete_event"] = onCooldownComplete.Value.ToJSON();

            return json;
        }
    }
    public class ComponentBarter : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentBlockClimber : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentBlockSensor : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentBoostable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentBoss : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentBreakBlocks : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentBreathable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentBreedable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentBribeable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentBuoyant : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentBurnsInDaylight : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentCelebrateHunt : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentCombatRegeneration : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentConditionalBandwidthOptimization : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentCustomHitTest : EntityComponent
    {
        public Offset3 pivot;
        public int width;
        public int height;

        public override string GetIdentifier() =>
            "minecraft:custom_hit_test";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["hitboxes"] = new JArray(new JObject()
                {
                    ["pivot"] = pivot.ToArray(),
                    ["width"] = width,
                    ["height"] = height
                })
            };
        }
    }
    public class ComponentDamageOverTime : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentDamageSensor : EntityComponent
    {
        bool dealsDamage = false;

        public ComponentDamageSensor(bool dealsDamage)
        {
            this.dealsDamage = dealsDamage;
        }

        public override string GetIdentifier() =>
            "minecraft:damage_sensor";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["triggers"] = new JObject()
                {
                    ["deals_damage"] = dealsDamage
                }
            };
        }
    }
    public class ComponentDespawn : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentDryingOutTimer : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentEconomyTradeTable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentEntitySensor : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentEnvironmentSensor : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentEquipItem : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentEquippable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentExperienceReward : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentExplode : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentFlocking : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentGenetics : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentGiveable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentGroupSize : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentGrowsCrop : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentHealable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentHome : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentHurtOnCondition : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentInsideBlockNotifier : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentInsomnia : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentInstantDespawn : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentInteract : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentInventory : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentItemHopper : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentJumpDynamic : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:jump.dynamic";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentJumpStatic : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:jump.static";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentLeashable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentLookAt : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentManagedWanderingTrader : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentMobEffect : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentMovementAmphibious : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:movement.amphibious";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentMovementBasic : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:movement.basic";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentMovementFly : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:movement.fly";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentMovementGeneric : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:movement.generic";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentMovementHover : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:movement.hover";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentMovementJump : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:movement.jump";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentMovementSkip : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:movement.skip";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentMovementSway : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:movement.sway";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentNameable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentNavigationClimb : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:navigation.climb";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentNavigationFloat : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:navigation.float";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentNavigationFly : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:navigation.fly";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentNavigationGeneric : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:navigation.generic";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentNavigationHover : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:navigation.hover";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentNavigationSwim : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:navigation.swim";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentNavigationWalk : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:navigation.walk";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentOutOfControl : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentPeek : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentPersistent : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentPhysics : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentPreferredPath : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentProjectile : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentPushable : EntityComponent
    {
        public bool isPushableByEntity;
        public bool isPushableByPiston;

        public override string GetIdentifier() =>
            "minecraft:pushable";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["is_pushable"] = isPushableByEntity,
                ["is_pushable_by_piston"] = isPushableByPiston
            };
        }
    }
    public class ComponentRaidTrigger : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentRailMovement : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentRailSensor : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentRavagerBlocked : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentRideable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentScaffoldingClimber : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentScaleByAge : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentScheduler : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentShareables : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentShooter : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentSittable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentSpawnEntity : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentTameable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentTameMount : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentTargetNearbySensor : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentTeleport : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentTickWorld : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentTimer : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentTradeTable : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentTrail : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentTransformation : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentTrusting : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
    public class ComponentWaterMovement : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:";
        public override JObject _GetValue()
        {
            return new JObject()
            {

            };
        }
    }
}