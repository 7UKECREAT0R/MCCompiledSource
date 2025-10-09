using System;
using System.Linq;
using mc_compiled.Commands;
using mc_compiled.MCC.CustomEntities;
using mc_compiled.Modding.Behaviors.Loot;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Lists;
// https://docs.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/componentlist

public class ComponentAddRider : EntityComponent
{
    public string entityType;
    public string spawnEvent;

    public override string GetIdentifier() { return "minecraft:addrider"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["entity_type"] = this.entityType
        };

        if (this.spawnEvent != null)
            json["spawn_event"] = this.spawnEvent;

        return json;
    }
}

public class ComponentAdmireItem : EntityComponent
{
    public int cooldownAfterBeingAttacked; // seconds
    public int duration;

    public override string GetIdentifier() { return "minecraft:admire_item"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["cooldown_after_being_attacked"] = this.cooldownAfterBeingAttacked,
            ["duration"] = this.duration
        };
    }
}

public class ComponentAgeable : EntityComponent
{
    public string[] dropItems; // items it drops when it grows up
    public float duration;
    public string[] feedItems;
    public EntityEventHandler growUpEvent;

    public override string GetIdentifier() { return "minecraft:ageable"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["duration"] = this.duration
        };
        if (this.dropItems != null)
            json["drop_items"] = new JArray(this.dropItems.Cast<object>().ToArray());
        if (this.feedItems != null)
            json["feed_items"] = new JArray(this.feedItems.Cast<object>().ToArray());
        if (this.growUpEvent != null)
            json["grow_up"] = this.growUpEvent.ToComponentJSON();

        return json;
    }
}

public class ComponentAngry : EntityComponent
{
    public int angerDuration;
    public int angerDurationDelta; // [+/-]random(delta)
    public string angrySound;
    public bool broadcastAnger;
    public bool broadcastAngerOnAttack;
    public bool broadcastAngerOnBeingAttacked;
    public string[] broadcastFamilies;
    public FilterCollection broadcastFilters;
    public int broadcastRange;
    public FilterCollection ignoreEntities;

    public override string GetIdentifier() { return "minecraft:angry"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["broadcast_anger"] = this.broadcastAnger,
            ["broadcast_anger_on_attack"] = this.broadcastAngerOnAttack,
            ["broadcast_anger_on_being_attacked"] = this.broadcastAngerOnBeingAttacked,
            ["broadcast_range"] = this.broadcastRange,
            ["duration"] = this.angerDuration,
            ["duration_delta"] = this.angerDurationDelta
        };
        if (this.angrySound != null)
            json["angry_sound"] = this.angrySound;
        if (this.broadcastFilters != null)
            json["broadcast_filters"] = this.broadcastFilters.ToJSON();
        if (this.broadcastFamilies != null)
            json["broadcast_targets"] = new JArray(this.broadcastFamilies.Cast<object>().ToArray());
        if (this.ignoreEntities != null)
            json["filters"] = this.ignoreEntities.ToJSON();

        return json;
    }
}

public class ComponentAnnotationBreakDoor : EntityComponent
{
    public float breakTime;
    public DifficultyMode difficulty;

    public override string GetIdentifier() { return "minecraft:annotation.break_door"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["break_time"] = this.breakTime,
            ["min_difficulty"] = this.difficulty.ToString()
        };
    }
}

public class ComponentAnnotationOpenDoor : EntityComponent
{
    public override string GetIdentifier() { return "minecraft:annotation.open_door"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentAreaAttack : EntityComponent
{
    public DamageCause? cause;
    public int damagePerTick;
    public float damageRange;
    public FilterCollection entityFilter;

    public override string GetIdentifier() { return "minecraft:area_attack"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["damage_per_tick"] = this.damagePerTick,
            ["damage_range"] = this.damageRange
        };
        if (this.entityFilter != null)
            json["entity_filter"] = this.entityFilter.ToJSON();
        if (this.cause.HasValue)
            json["cause"] = this.cause.Value.ToString();

        return json;
    }
}

public class ComponentAttackCooldown : EntityComponent
{
    public float? minCooldownTime;
    public EntityEventHandler onCooldownComplete;
    public float primaryCooldownTime;

    public override string GetIdentifier() { return "minecraft:attack_cooldown"; }
    public override JObject _GetValue()
    {
        var json = new JObject();

        if (this.minCooldownTime.HasValue)
            json["attack_cooldown_time"] = new JArray(new[]
            {
                this.minCooldownTime.Value, this.primaryCooldownTime
            });
        else
            json["attack_cooldown_time"] = this.primaryCooldownTime;

        if (this.onCooldownComplete != null)
            json["attack_cooldown_complete_event"] = this.onCooldownComplete.ToComponentJSON();

        return json;
    }
}

public class ComponentBarter : EntityComponent
{
    public LootTable barterTable;
    public int cooldownAfterBeingAttacked;
    public override string GetIdentifier() { return "minecraft:barter"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["barter_table"] = this.barterTable.ResourcePath,
            ["cooldown_after_being_attacked"] = this.cooldownAfterBeingAttacked
        };
    }
}

public class ComponentBlockClimber : EntityComponent
{
    public override string GetIdentifier() { return "minecraft:block_climber"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentBlockSensor : EntityComponent
{
    public SensorPool[] pools;
    public float sensorRadius;

    public override string GetIdentifier() { return "minecraft:block_sensor"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["sensor_radius"] = this.sensorRadius,
            ["on_break"] = new JArray(this.pools.Select(sp => sp.ToJSON()))
        };
    }

    public struct SensorPool
    {
        public string[] blockList;
        public string onBlockBroken;

        public SensorPool(string onBlockBroken, string[] blockList)
        {
            this.blockList = blockList;
            this.onBlockBroken = onBlockBroken;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["block_list"] = new JArray(this.blockList.Cast<object>().ToArray()),
                ["on_block_broken"] = this.onBlockBroken
            };
        }
    }
}

public class ComponentBoostable : EntityComponent
{
    public float boostDuration;

    public BoostItem[] boostItems;
    public float speedMultiplier;

    public override string GetIdentifier() { return "minecraft:boostable"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["duration"] = this.boostDuration,
            ["speed_multiplier"] = this.speedMultiplier,
            ["boost_items"] = new JArray(this.boostItems.Select(bi => bi.ToJSON()))
        };
    }

    public struct BoostItem
    {
        public string identifier;
        public int damage;
        public string replaceItem; // item that it's transformed to once broken

        public BoostItem(string identifier, int damage, string replaceItem)
        {
            this.identifier = identifier;
            this.damage = damage;
            this.replaceItem = replaceItem;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["item"] = this.identifier,
                ["damage"] = this.damage,
                ["replace_item"] = this.replaceItem
            };
        }
    }
}

public class ComponentBoss : EntityComponent
{
    public string bossbarText;
    public int hudRange;
    public bool shouldDarkenSky;

    public override string GetIdentifier() { return "minecraft:boss"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["hud_range"] = this.hudRange,
            ["name"] = this.bossbarText,
            ["should_darken_sky"] = this.shouldDarkenSky
        };
    }
}

public class ComponentBreakBlocks : EntityComponent
{
    public string[] breakableBlocks;
    public override string GetIdentifier() { return "minecraft:break_blocks"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["breakable_blocks"] = new JArray(this.breakableBlocks.Cast<object>().ToArray())
        };
    }
}

public class ComponentBreathable : EntityComponent
{
    public string[] breatheBlocks;

    public bool breathesAir;
    public bool breathesLava;
    public bool breathesSolids;
    public bool breathesWater;
    public bool generatesBubbles;
    public float inhaleTime;
    public string[] nonBreatheBlocks;
    public int suffocateTime;
    public int totalSupply;

    public override string GetIdentifier() { return "minecraft:breathable"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["breathes_air"] = this.breathesAir,
            ["breathes_lava"] = this.breathesLava,
            ["breathes_solids"] = this.breathesSolids,
            ["breathes_water"] = this.breathesWater,
            ["generates_bubbles"] = this.generatesBubbles,
            ["inhale_time"] = this.inhaleTime,
            ["suffocate_time"] = this.suffocateTime,
            ["total_supply"] = this.totalSupply
        };
        if (this.breatheBlocks != null)
            json["breathe_blocks"] = new JArray(this.breatheBlocks.Cast<object>().ToArray());
        if (this.nonBreatheBlocks != null)
            json["non_breathe_blocks"] = new JArray(this.nonBreatheBlocks.Cast<object>().ToArray());

        return json;
    }
}

public class ComponentBreedable : EntityComponent
{
    public bool blendAttributes;
    public float breedCooldown;

    public string[] breedItems;
    public BreedEntity[] breedsWith;

    public bool canBreedWhileSitting;
    public bool causesPregnancy;
    public DenyParentsVariant? denyParents;
    public EnvironmentRequirement[] environmentRequirements;
    public float extraBabyChance; // 0.0 - 1.0
    public bool inheritTamed;
    public FilterCollection loveFilters;
    public MutationFactor? mutationFactor;
    public bool requireFullHealth;
    public bool requireTamed;

    public override string GetIdentifier() { return "minecraft:breedable"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["allow_sitting"] = this.canBreedWhileSitting,
            ["blend_attributes"] = this.blendAttributes,
            ["breed_cooldown"] = this.breedCooldown,
            ["causes_pregnancy"] = this.causesPregnancy,
            ["extra_baby_chance"] = this.extraBabyChance,
            ["inherit_tamed"] = this.inheritTamed,
            ["require_full_health"] = this.requireFullHealth,
            ["require_tame"] = this.requireTamed
        };

        if (this.breedItems != null)
            json["breed_items"] = new JArray(this.breedItems.Cast<object>().ToArray());
        if (this.breedsWith != null)
            json["breeds_with"] = new JArray(this.breedsWith.Select(be => be.ToJSON()));
        if (this.denyParents.HasValue)
            json["deny_parents_variant"] = this.denyParents.Value.ToJSON();
        if (this.environmentRequirements != null)
            json["environment_requirements"] = new JArray(this.environmentRequirements.Select(er => er.ToJSON()));
        if (this.loveFilters != null)
            json["love_filters"] = this.loveFilters.ToJSON();
        if (this.mutationFactor.HasValue)
            json["mutation_factor"] = this.mutationFactor.Value.ToJSON();

        return json;
    }

    public struct BreedEntity
    {
        public string babyType;
        public string mateType;
        public EntityEventHandler breedEventToCall;

        public BreedEntity(string babyType, string mateType, EntityEventHandler breedEventToCall)
        {
            this.babyType = babyType;
            this.mateType = mateType;
            this.breedEventToCall = breedEventToCall;
        }
        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["baby_type"] = this.babyType,
                ["mate_type"] = this.mateType
            };

            if (this.breedEventToCall != null)
                json["breed_event"] = this.breedEventToCall.ToComponentJSON();

            return json;
        }
    }

    public struct DenyParentsVariant
    {
        public float chance; // 0.0 - 1.0
        public int maxVariant;
        public int minVariant;

        public DenyParentsVariant(float chance, int maxVariant, int minVariant)
        {
            this.chance = chance;
            this.maxVariant = maxVariant;
            this.minVariant = minVariant;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["chance"] = this.chance,
                ["max_variant"] = this.maxVariant,
                ["min_variant"] = this.minVariant
            };
        }
    }

    public struct EnvironmentRequirement
    {
        public string[] nearbyBlocks;
        public int count;
        public float radius;

        public EnvironmentRequirement(string[] nearbyBlocks, int count, float radius)
        {
            this.nearbyBlocks = nearbyBlocks;
            this.count = count;
            this.radius = radius;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["blocks"] = new JArray(this.nearbyBlocks.Cast<object>().ToArray()),
                ["count"] = this.count,
                ["radius"] = this.radius
            };
        }
    }

    public struct MutationFactor
    {
        // 0.0 - 1.0
        public float colorChance;
        public float extraVariantChance;
        public float variantChance;

        public MutationFactor(float colorChance, float extraVariantChance, float variantChance)
        {
            this.colorChance = colorChance;
            this.extraVariantChance = extraVariantChance;
            this.variantChance = variantChance;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["color"] = this.colorChance,
                ["extra_variant"] = this.extraVariantChance,
                ["variant"] = this.variantChance
            };
        }
    }
}

public class ComponentBribeable : EntityComponent
{
    public float bribeCooldown;
    public string[] bribeItems;

    public override string GetIdentifier() { return "minecraft:bribeable"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["bribe_cooldown"] = this.bribeCooldown,
            ["bribe_items"] = new JArray(this.bribeItems.Cast<object>().ToArray())
        };
    }
}

public class ComponentBuoyant : EntityComponent
{
    public bool applyGravity;
    public float baseBuoyancy;
    public float bigWaveProbability;
    public float bigWaveSpeed;
    public float dragDownOnBuoyancyRemoved;
    public string[] liquidBlocks;
    public bool simulateWaves;

    public override string GetIdentifier() { return "minecraft:buoyant"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["apply_gravity"] = this.applyGravity,
            ["base_buoyancy"] = this.baseBuoyancy,
            ["big_wave_probability"] = this.bigWaveProbability,
            ["big_wave_speed"] = this.bigWaveSpeed,
            ["drag_down_on_buoyancy_removed"] = this.dragDownOnBuoyancyRemoved,
            ["liquid_blocks"] = new JArray(this.liquidBlocks.Cast<object>().ToArray()),
            ["simulate_waves"] = this.simulateWaves
        };
    }
}

public class ComponentBurnsInDaylight : EntityComponent
{
    public override string GetIdentifier() { return "minecraft:burns_in_daylight"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentCelebrateHunt : EntityComponent
{
    // for luke tmrw:
    // https://docs.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitycomponents/minecraftcomponent_celebrate_hunt
    // yo thanks luke from yesterday
    public bool broadcast;
    public string celebrationSound;
    public FilterCollection celebrationTargets;
    public int duration;
    public float maxSoundInterval;
    public float minSoundInterval;
    public float radius;

    public override string GetIdentifier() { return "minecraft:celebrate_hunt"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["broadcast"] = this.broadcast,
            ["duration"] = this.duration,
            ["radius"] = this.radius,
            ["sound_interval"] = new JObject
            {
                ["range_min"] = this.minSoundInterval,
                ["range_max"] = this.maxSoundInterval
            }
        };

        if (this.celebrationTargets != null)
            json["celebration_targets"] = this.celebrationTargets.ToJSON();
        if (this.celebrationSound != null)
            json["celebrate_sound"] = this.celebrationSound;

        return json;
    }
}

public class ComponentCombatRegeneration : EntityComponent
{
    public bool applyToFamily;
    public bool applyToSelf;
    public int regenDuration;

    public override string GetIdentifier() { return "minecraft:combat_regeneration"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["regeneration_duration"] = this.regenDuration,
            ["apply_to_self"] = this.applyToSelf,
            ["apply_to_family"] = this.applyToFamily
        };
    }
}

public class ComponentConditionalBandwidthOptimization : EntityComponent
{
    public override string GetIdentifier() { return "minecraft:conditional_bandwidth_optimization"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentCustomHitTest : EntityComponent
{
    public Hitbox[] hitboxes;

    public override string GetIdentifier() { return "minecraft:custom_hit_test"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["hitboxes"] = new JArray(this.hitboxes.Select(hb => hb.ToJSON()))
        };
    }

    public struct Hitbox
    {
        public Offset3 pivot;
        public int width;
        public int height;

        public Hitbox(Offset3 pivot, int width, int height)
        {
            this.pivot = pivot;
            this.width = width;
            this.height = height;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["pivot"] = this.pivot.ToArray(),
                ["width"] = this.width,
                ["height"] = this.height
            };
        }
    }
}

public class ComponentDamageOverTime : EntityComponent
{
    public int damagePerHurt;
    public float timeBetweenHurt;

    public override string GetIdentifier() { return "minecraft:damage_over_time"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["damage_per_hurt"] = this.damagePerHurt,
            ["time_between_hurt"] = this.timeBetweenHurt
        };
    }
}

public class ComponentDamageSensor : EntityComponent
{
    public Trigger[] triggerPool;
    public override string GetIdentifier() { return "minecraft:damage_sensor"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["triggers"] = new JArray(this.triggerPool.Select(t => t.ToJSON()))
        };
    }

    public struct Trigger
    {
        public DamageCause cause;
        public float damageModifier;
        public float damageMultiplier;
        public bool dealsDamage;
        public string onDamageSoundEvent;
        public FilterCollection onDamageFilters;
        public EntityEventHandler onDamage;

        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["cause"] = this.cause.ToString(),
                ["damage_modifier"] = this.damageModifier,
                ["damage_multiplier"] = this.damageMultiplier,
                ["deals_damage"] = this.dealsDamage
            };

            if (this.onDamageSoundEvent != null)
                json["on_damage_sound_event"] = this.onDamageSoundEvent;

            bool hasFilter = this.onDamageFilters != null;
            bool hasEvent = this.onDamage != null;

            if (hasFilter | hasEvent)
            {
                var child = new JObject();
                if (hasFilter)
                    child["filters"] = this.onDamageFilters.ToJSON();
                if (hasEvent)
                {
                    child["event"] = this.onDamage.eventID;
                    child["target"] = this.onDamage.target.ToString();
                }
            }

            return json;
        }
    }
}

public class ComponentDespawn : EntityComponent
{
    public bool despawnFromChance;
    public bool despawnFromDistance;

    public bool despawnFromInactivity;
    public bool despawnFromSimulationEdge;

    public FilterCollection despawnRequirements;
    public int inactivityTimer;
    public int minDistance, maxDistance;
    public int randomChance;
    public bool removeChildEntities;

    public override string GetIdentifier() { return "minecraft:despawn"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["despawn_from_chance"] = this.despawnFromChance,
            ["despawn_from_inactivity"] = this.despawnFromInactivity,
            ["despawn_from_simulation_edge"] = this.despawnFromSimulationEdge,
            ["min_range_inactivity_timer"] = this.inactivityTimer,
            ["min_range_random_chance"] = this.randomChance,
            ["remove_child_entities"] = this.removeChildEntities
        };
        if (this.despawnFromDistance)
            json["despawn_from_distance"] = new JObject
            {
                ["max_distance"] = this.maxDistance,
                ["min_distance"] = this.minDistance
            };
        if (this.despawnRequirements != null)
            json["filters"] = this.despawnRequirements.ToJSON();
        return json;
    }
}

public class ComponentDryingOutTimer : EntityComponent
{
    public EntityEventHandler driedOutEvent;
    public EntityEventHandler recoverEvent;
    public EntityEventHandler stoppedDryingEvent;
    public float totalTime;
    public float waterBottleRefillTime;

    public override string GetIdentifier() { return "minecraft:drying_out_timer"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["total_time"] = this.totalTime,
            ["water_bottle_refill_time"] = this.waterBottleRefillTime
        };

        if (this.driedOutEvent != null)
            json["dried_out_event"] = this.driedOutEvent.ToComponentJSON();
        if (this.recoverEvent != null)
            json["recover_after_dried_out_event"] = this.recoverEvent.ToComponentJSON();
        if (this.stoppedDryingEvent != null)
            json["stopped_drying_out_event"] = this.stoppedDryingEvent.ToComponentJSON();

        return json;
    }
}

public class ComponentEconomyTradeTable : EntityComponent
{
    public bool convertTradesEconomy;
    public int curedDiscountMax;

    public int curedDiscountMin;
    public string displayName; // optional
    public int heroDemandDiscount;
    public bool legacyPriceFormula;
    public int maxCuredDiscountHigh;

    public int maxCuredDiscountLow;
    public int maxNearbyCuredDiscount;

    public int minNearbyCuredDiscount;
    public bool newScreen;
    public bool persistTrades;
    public bool showTradeScreen;
    public LootTable tradeTable;

    public override string GetIdentifier() { return "minecraft:economy_trade_table"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["convert_trades_economy"] = this.convertTradesEconomy,
            ["cured_discount"] = new JArray(new[] {this.curedDiscountMin, this.curedDiscountMax}),
            ["hero_demand_discount"] = this.heroDemandDiscount,
            ["max_cured_discount"] = new JArray(new[] {this.maxCuredDiscountLow, this.maxCuredDiscountHigh}),
            ["max_nearby_cured_discount"] = this.maxNearbyCuredDiscount,
            ["nearby_cured_discount"] = this.minNearbyCuredDiscount,
            ["new_screen"] = this.newScreen,
            ["persistTrades"] = this.persistTrades,
            ["show_trade_screen"] = this.showTradeScreen,
            ["table"] = this.tradeTable.ResourcePath,
            ["use_legacy_price_formula"] = this.legacyPriceFormula
        };

        if (this.displayName != null)
            json["display_name"] = this.displayName;

        return json;
    }
}

public class ComponentEntitySensor : EntityComponent
{
    public EntityEventHandler eventToFire;
    public FilterCollection filters;
    public int maxEntities;
    public int minEntities;
    public bool relativeRange;
    public bool requireAllToPassFilter;

    public float sensorRange;

    public override string GetIdentifier() { return "minecraft:entity_sensor"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["event"] = this.eventToFire.ToComponentJSON(),
            ["maximum_count"] = this.maxEntities,
            ["minimum_count"] = this.minEntities,
            ["relative_range"] = this.relativeRange,
            ["require_all"] = this.requireAllToPassFilter,
            ["sensor_range"] = this.sensorRange
        };

        if (this.filters != null)
            json["event_filters"] = this.filters.ToJSON();

        return json;
    }
}

public class ComponentEnvironmentSensor : EntityComponent
{
    public Trigger[] triggers;
    public override string GetIdentifier() { return "minecraft:environment_sensor"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["triggers"] = new JArray(this.triggers.Select(t => t.ToJSON()))
        };
    }

    public struct Trigger
    {
        public EntityEventHandler eventToFire;
        public FilterCollection filters;

        public Trigger(EntityEventHandler eventToFire, FilterCollection filters)
        {
            this.eventToFire = eventToFire;
            this.filters = filters;
        }

        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["event"] = this.eventToFire.eventID,
                ["target"] = this.eventToFire.target.ToString()
            };

            if (this.filters != null)
                json["filters"] = this.filters.ToJSON();

            return json;
        }
    }
}

public class ComponentEquipItem : EntityComponent
{
    public override string GetIdentifier() { return "minecraft:equip_item"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentEquippable : EntityComponent
{
    public EquippableSlot[] slots;
    public override string GetIdentifier() { return "minecraft:equippable"; }
    public override JObject _GetValue()
    {
        var json = new JObject();
        if (this.slots != null)
            json["slots"] = new JArray(this.slots.Select(slot => slot.ToJSON()));
        return json;
    }

    public struct EquippableSlot
    {
        public string[] acceptedItems;
        public string interactText;
        public string item;
        public EntityEventHandler onEquipEvent;
        public EntityEventHandler onUnequipEvent;
        public int slotNumber;

        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["slot"] = this.slotNumber
            };

            if (this.acceptedItems != null)
                json["accepted_items"] = new JArray(this.acceptedItems);
            if (this.interactText != null)
                json["interact_text"] = this.interactText;
            if (this.item != null)
                json["item"] = this.item;
            if (this.onEquipEvent != null)
                json["on_equip"] = this.onEquipEvent.ToComponentJSON();
            if (this.onEquipEvent != null)
                json["on_unequip"] = this.onUnequipEvent.ToComponentJSON();

            return json;
        }
    }
}

public class ComponentExperienceReward : EntityComponent
{
    public MolangValue experienceOnBred;
    public MolangValue experienceOnDeath;

    public override string GetIdentifier() { return "minecraft:experience_reward"; }
    public override JObject _GetValue()
    {
        var json = new JObject();
        if (this.experienceOnBred != null)
            json["on_bred"] = this.experienceOnBred.ToJSON();
        if (this.experienceOnDeath != null)
            json["on_death"] = this.experienceOnDeath.ToJSON();
        return json;
    }
}

public class ComponentExplode : EntityComponent
{
    public bool breaksBlocks;
    public bool causesFire;
    public bool followMobGriefingRule;
    public bool fuseLit;
    public float fuseMax;
    public float fuseMin;
    public float? maxBlockResistance;
    public int power;

    public ComponentExplode(ExploderPreset preset, bool fuseLit)
    {
        this.breaksBlocks = preset.breaks;
        this.causesFire = preset.fire;
        this.power = preset.power;
        this.followMobGriefingRule = false;
        this.maxBlockResistance = null;
        this.ConstantFuse = preset.delay / 20f;
        this.fuseLit = fuseLit;
    }

    /// <summary>
    ///     Set a constant fuse rather than a random range.
    /// </summary>
    public float ConstantFuse
    {
        set
        {
            this.fuseMin = value;
            this.fuseMax = value;
        }
    }

    public override string GetIdentifier() { return "minecraft:explode"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["breaks_blocks"] = this.breaksBlocks,
            ["causes_fire"] = this.causesFire,
            ["destroy_affected_by_griefing"] = this.followMobGriefingRule,
            ["fire_affected_by_griefing"] = this.followMobGriefingRule,
            ["fuse_lit"] = this.fuseLit,
            ["power"] = this.power
        };

        if (this.maxBlockResistance.HasValue)
            json["max_resistance"] = this.maxBlockResistance.Value;

        if (Math.Abs(this.fuseMin - this.fuseMax) < 0.0001F)
            json["fuse_length"] = this.fuseMin;
        else
            json["fuse_length"] = new JArray(new[] {this.fuseMin, this.fuseMax});

        return json;
    }
}

public class ComponentFlocking : EntityComponent
{
    public float blockDistance;
    public float blockWeight;
    public float breachInfluence;

    public float cohesionThreshold;
    public float cohesionWeight;
    public float goalWeight;
    public int highFlockLimit;
    public float influenceRadius;
    public float innerCohesionThreshold;

    public bool inWater;
    public float lonerChance; // 0.0 - 1.0

    public int lowFlockLimit;
    public bool matchVariants; // racist bool
    public float maxHeight;

    public float minHeight;

    public float separationThreshold;
    public float separationWeight;
    public bool useCenterOfMass;

    public override string GetIdentifier() { return "minecraft:flocking"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["block_distance"] = this.blockDistance,
            ["block_weight"] = this.blockWeight,
            ["breach_influence"] = this.breachInfluence,
            ["cohesion_threshold"] = this.cohesionThreshold,
            ["goal_weight"] = this.goalWeight,
            ["high_flock_limit"] = this.highFlockLimit,
            ["in_water"] = this.inWater,
            ["influence_radius"] = this.influenceRadius,
            ["inner_cohesion_threshold"] = this.innerCohesionThreshold,
            ["loner_chance"] = this.lonerChance,
            ["low_flock_limit"] = this.lowFlockLimit,
            ["match_variants"] = this.matchVariants,
            ["max_height"] = this.maxHeight,
            ["min_height"] = this.minHeight,
            ["separation_threshold"] = this.separationThreshold,
            ["separation_weight"] = this.separationWeight,
            ["use_center_of_mass"] = this.useCenterOfMass
        };
    }
}

public class ComponentGenetics : EntityComponent
{
    public Gene[] genes;
    public float mutationRate; // 0.0 - 1.0; default is 0.03125f
    public override string GetIdentifier() { return "minecraft:genetics"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["genes"] = new JArray(this.genes.Select(gene => gene.ToJSON())),
            ["mutation_rate"] = this.mutationRate
        };
    }

    public struct AlleleRange
    {
        public int max, min;

        public AlleleRange(int max, int min)
        {
            this.max = max;
            this.min = min;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["range_max"] = this.max,
                ["range_min"] = this.min
            };
        }
    }

    public struct GeneticVariant
    {
        public EntityEventHandler geneBirthEvent;
        public float mutationRate;
        public int bothAllele;
        public int eitherAllele;
        public int hiddenAllele;
        public int mainAllele;

        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["mutation_rate"] = this.mutationRate,
                ["both_allele"] = this.bothAllele,
                ["either_allele"] = this.eitherAllele,
                ["hidden_allele"] = this.hiddenAllele,
                ["main_allele"] = this.mainAllele
            };

            if (this.geneBirthEvent != null)
                json["birth_event"] = this.geneBirthEvent.ToComponentJSON();

            return json;
        }
    }

    public struct Gene
    {
        public GeneticVariant[] variants;
        public int alleleRange;
        public string name;

        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["allele_range"] = this.alleleRange
            };

            if (this.name != null)
                json["name"] = this.name;
            if (this.variants != null)
                json["genetic_variants"] = new JArray(this.variants.Select(variant => variant.ToJSON()));

            return json;
        }
    }
}

public class ComponentGiveable : EntityComponent
{
    public string[] acceptedItems;
    public float? cooldown;
    public EntityEventHandler onGiveEvent;

    public override string GetIdentifier() { return "minecraft:giveable"; }
    public override JObject _GetValue()
    {
        var json = new JObject();

        if (this.cooldown.HasValue)
            json["cooldown"] = this.cooldown.Value;
        if (this.acceptedItems != null)
            json["items"] = new JArray(this.acceptedItems.Cast<object>().ToArray());
        if (this.onGiveEvent != null)
            json["on_give"] = this.onGiveEvent.ToComponentJSON();

        return json;
    }
}

public class ComponentGroupSize : EntityComponent
{
    public FilterCollection groupTest;
    public float radius;

    public override string GetIdentifier() { return "minecraft:group_size"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["radius"] = this.radius,
            ["filters"] = this.groupTest.ToJSON()
        };
    }
}

public class ComponentGrowsCrop : EntityComponent
{
    public float chancePerTick; // 0.0 - 1.0
    public int charges;

    public override string GetIdentifier() { return "minecraft:grows_crop"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["chance"] = this.chancePerTick,
            ["charges"] = this.charges
        };
    }
}

public class ComponentHealable : EntityComponent
{
    public FilterCollection filters;
    public bool forceUse;
    public HealItem[] items;

    public override string GetIdentifier() { return "minecraft:healable"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["force_use"] = this.forceUse
        };

        if (this.filters != null)
            json["filters"] = this.filters.ToJSON();
        if (this.items != null)
            json["items"] = new JArray(this.items.Select(item => item.ToJSON()));

        return json;
    }

    public struct HealItem
    {
        public string item;
        public int healAmount;

        public HealItem(string item, int healAmount)
        {
            this.item = item;
            this.healAmount = healAmount;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["heal_amount"] = this.healAmount,
                ["item"] = this.item
            };
        }
    }
}

public class ComponentHome : EntityComponent
{
    // used by bees to mark their beehives

    public string[] homeBlocks;
    public int restrictionRadius;

    public override string GetIdentifier() { return "minecraft:home"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["restriction_radius"] = this.restrictionRadius
        };

        if (this.homeBlocks != null)
            json["home_block_list"] = new JArray(this.homeBlocks.Cast<object>());

        return json;
    }
}

public class ComponentHurtOnCondition : EntityComponent
{
    public DamageCondition[] conditions;
    public override string GetIdentifier() { return "minecraft:hurt_on_condition"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["damage_condition"] = new JArray(this.conditions.Select(cond => cond.ToJSON()))
        };
    }

    public struct DamageCondition
    {
        public FilterCollection filters;
        public DamageCause cause;
        public int damagePerTick;

        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["cause"] = this.cause.ToString(),
                ["damage_per_tick"] = this.damagePerTick
            };

            if (this.filters != null)
                json["filters"] = this.filters.ToJSON();

            return json;
        }
        public static DamageCondition BurnInLava()
        {
            return new DamageCondition
            {
                filters = [new FilterInLava {checkValue = true, subject = EventSubject.self}],
                cause = DamageCause.drowning,
                damagePerTick = 4
            };
        }
    }
}

public class ComponentInsideBlockNotifier : EntityComponent
{
    public InsideBlock[] blocks;
    public override string GetIdentifier() { return "minecraft:inside_block_notifier"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["block_list"] = new JArray(this.blocks.Select(block => block.ToJSON()))
        };
    }

    public struct InsideBlock
    {
        public string blockName;
        public JProperty[] blockStates;

        public EntityEventHandler enteredBlockEvent;
        public EntityEventHandler exitedBlockEvent;

        public JObject BlockStatesAsJSON()
        {
            if (this.blockStates == null)
                return null;
            var obj = new JObject();
            foreach (JProperty state in this.blockStates)
                obj[state.Name] = state.Value;
            return obj;
        }
        public JObject ToJSON()
        {
            var json = new JObject();

            var block = new JObject();
            block["name"] = this.blockName;
            if (this.blockStates != null)
                block["states"] = BlockStatesAsJSON();
            json["block"] = block;

            if (this.enteredBlockEvent != null)
                json["entered_block_event"] = this.enteredBlockEvent.ToComponentJSON();
            if (this.exitedBlockEvent != null)
                json["exited_block_event"] = this.exitedBlockEvent.ToComponentJSON();

            return json;
        }
    }
}

public class ComponentInsomnia : EntityComponent
{
    public float daysUntilExperiencesInsomnia;

    public override string GetIdentifier() { return "minecraft:insomnia"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["days_until_insomnia"] = this.daysUntilExperiencesInsomnia
        };
    }
}

public class ComponentInstantDespawn : EntityComponent
{
    public bool removeChildEntities;

    public override string GetIdentifier() { return "minecraft:instant_despawn"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["remove_child_entities"] = this.removeChildEntities
        };
    }
}

public class ComponentInteract : EntityComponent
{
    public LootTable addItems;
    public bool consumeItem;

    public float cooldown;
    public float cooldownAfterBeingAttacked;
    public int dealItemDamage;
    public string[] entitiesToSpawn;
    public int healthAmount;
    public string interactText;
    public EntityEventHandler onInteractEvent;
    public ParticleOnStart? particles;
    public string[] soundsToPlay;
    public LootTable spawnItems;
    public bool swingAnimation;
    public string transformToItem;

    public override string GetIdentifier() { return "minecraft:interact"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["cooldown"] = this.cooldown,
            ["cooldown_after_being_attacked"] = this.cooldownAfterBeingAttacked,
            ["health_amount"] = this.healthAmount,
            ["hurt_item"] = this.dealItemDamage,
            ["swing"] = this.swingAnimation,
            ["use_item"] = this.consumeItem
        };

        if (this.addItems != null)
            json["add_items"] = new JObject {["table"] = this.addItems.ResourcePath};
        if (this.interactText != null)
            json["interact_text"] = this.interactText;
        if (this.onInteractEvent != null)
            json["on_interact"] = this.onInteractEvent.ToComponentJSON();
        if (this.particles.HasValue)
            json["particle_on_start"] = this.particles.Value.ToJSON();
        if (this.soundsToPlay != null)
            json["play_sounds"] = new JArray(this.soundsToPlay);
        if (this.entitiesToSpawn != null)
            json["spawn_entities"] = new JArray(this.entitiesToSpawn);
        if (this.spawnItems != null)
            json["spawn_items"] = new JArray(this.spawnItems);
        if (this.transformToItem != null)
            json["transform_to_item"] = this.transformToItem;

        return json;
    }

    public struct ParticleOnStart
    {
        public bool offsetTowardsInteractor;
        public string particleType;
        public float yOffset;

        public ParticleOnStart(string particleType, bool offsetTowardsInteractor, float yOffset)
        {
            this.particleType = particleType;
            this.offsetTowardsInteractor = offsetTowardsInteractor;
            this.yOffset = yOffset;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["particle_offset_towards_interactor"] = this.offsetTowardsInteractor,
                ["particle_type"] = this.particleType,
                ["particle_y_offset"] = this.yOffset
            };
        }
    }
}

public class ComponentInventory : EntityComponent
{
    public enum ContainerType
    {
        horse,
        minecart_chest,
        minecart_hopper,
        inventory,
        container,
        hopper
    }

    public int additionalSlotsPerStrength;
    public bool canBeSiphonedFrom;
    public ContainerType containerType;
    public bool dropOnDeath;
    public int inventorySize;
    public bool restrictToOwner;

    public override string GetIdentifier() { return "minecraft:inventory"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["additional_slots_per_strength"] = this.additionalSlotsPerStrength,
            ["can_be_siphoned_from"] = this.canBeSiphonedFrom,
            ["container_type"] = this.containerType.ToString(),
            ["inventory_size"] = this.inventorySize,
            ["private"] = !this.dropOnDeath,
            ["restrict_to_owner"] = this.restrictToOwner
        };
    }
}

public class ComponentItemHopper : EntityComponent
{
    public override string GetIdentifier() { return "minecraft:item_hopper"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentJumpDynamic : EntityComponent
{
    public override string GetIdentifier() { return "minecraft:jump.dynamic"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentJumpStatic : EntityComponent
{
    public float jumpPower;

    public override string GetIdentifier() { return "minecraft:jump.static"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["jump_power"] = this.jumpPower
        };
    }
}

public class ComponentLeashable : EntityComponent
{
    public bool canBeStolen;
    public float hardDistance; // siffens
    public float maxDistance; // breaks

    public EntityEventHandler
        onLeashEvent,
        onUnleashEnvent;
    public float softDistance; // springs back

    public override string GetIdentifier() { return "minecraft:leashable"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["can_be_stolen"] = this.canBeStolen,
            ["soft_distance"] = this.softDistance,
            ["hard_distance"] = this.hardDistance,
            ["max_distance"] = this.maxDistance
        };

        if (this.onLeashEvent != null)
            json["on_leash"] = this.onLeashEvent.ToComponentJSON();
        if (this.onUnleashEnvent != null)
            json["on_unleash"] = this.onUnleashEnvent.ToComponentJSON();

        return json;
    }
}

public class ComponentLookAt : EntityComponent
{
    public bool allowInvulnerable;
    public bool attackTargets;
    public FilterCollection filters;
    public float lookCooldownMax;

    public float lookCooldownMin;
    public EntityEventHandler lookedAtEvent;
    public float searchRadius;

    public override string GetIdentifier() { return "minecraft:lookat"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["allow_invulnerable"] = this.allowInvulnerable,
            ["look_cooldown"] = new JArray(new[] {this.lookCooldownMin, this.lookCooldownMax}),
            ["search_radius"] = this.searchRadius,
            ["set_target"] = this.attackTargets
        };

        if (this.filters != null)
            json["filters"] = this.filters.ToJSON();
        if (this.lookedAtEvent != null)
            json["look_event"] = this.lookedAtEvent.eventID; // only accepts string :(

        return json;
    }
}

public class ComponentManagedWanderingTrader : EntityComponent
{
    public override string GetIdentifier() { return "minecraft:managed_wandering_trader"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentMobEffect : EntityComponent
{
    public PotionEffect effect;
    public float effectRange;
    public int effectTime;
    public FilterCollection entityFilter;

    public override string GetIdentifier() { return "minecraft:mob_effect"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["effect_range"] = this.effectRange,
            ["effect_time"] = this.effectTime,
            ["mob_effect"] = this.effect.ToString()
        };

        if (this.entityFilter != null)
            json["entity_filter"] = this.entityFilter.ToJSON();

        return json;
    }
}

public class ComponentMovementAmphibious : EntityComponent
{
    public float maxTurn = 30; // degrees

    public override string GetIdentifier() { return "minecraft:movement.amphibious"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["max_turn"] = this.maxTurn
        };
    }
}

public class ComponentMovementBasic : EntityComponent
{
    public float maxTurn = 30; // degrees

    public override string GetIdentifier() { return "minecraft:movement.basic"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["max_turn"] = this.maxTurn
        };
    }
}

public class ComponentMovementFly : EntityComponent
{
    public float maxTurn = 30; // degrees

    public override string GetIdentifier() { return "minecraft:movement.fly"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["max_turn"] = this.maxTurn
        };
    }
}

public class ComponentMovementGeneric : EntityComponent
{
    public float maxTurn = 30; // degrees

    public override string GetIdentifier() { return "minecraft:movement.generic"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["max_turn"] = this.maxTurn
        };
    }
}

public class ComponentMovementHover : EntityComponent
{
    public float maxTurn = 30; // degrees

    public override string GetIdentifier() { return "minecraft:movement.hover"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["max_turn"] = this.maxTurn
        };
    }
}

public class ComponentMovementJump : EntityComponent
{
    public float jumpDelayMax;
    public float jumpDelayMin;
    public float maxTurn = 30; // degrees

    public override string GetIdentifier() { return "minecraft:movement.jump"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["jump_delay"] = new JArray(new[] {this.jumpDelayMin, this.jumpDelayMax}),
            ["max_turn"] = this.maxTurn
        };
    }
}

public class ComponentMovementSkip : EntityComponent
{
    public float maxTurn = 30; // degrees

    public override string GetIdentifier() { return "minecraft:movement.skip"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["max_turn"] = this.maxTurn
        };
    }
}

public class ComponentMovementSway : EntityComponent
{
    public float maxTurn = 30; // degrees
    public float swayAmplitude = 0.05f;
    public float swayFrequency = 0.5f;

    public override string GetIdentifier() { return "minecraft:movement.sway"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["max_turn"] = this.maxTurn,
            ["sway_amplitude"] = this.swayAmplitude,
            ["sway_frequency"] = this.swayFrequency
        };
    }
}

public class ComponentNameable : EntityComponent
{
    public bool allowNametags;
    public bool alwaysShowName;
    public EntityEventHandler onNamed;
    public SpecialNameAction[] specialNames;

    public override string GetIdentifier() { return "minecraft:nameable"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["allow_name_tag_renaming"] = this.allowNametags,
            ["always_show"] = this.alwaysShowName
        };

        if (this.onNamed != null)
            json["default_trigger"] = this.onNamed.ToComponentJSON();
        if (this.specialNames != null)
            json["name_actions"] = new JArray(this.specialNames.Select(name => name.ToJSON()));

        return json;
    }

    /// <summary>
    ///     Fires an event when this entity is named something
    /// </summary>
    public struct SpecialNameAction
    {
        public string[] specialNames;
        public EntityEventHandler onNamedEvent;

        public SpecialNameAction(EntityEventHandler onNamedEvent, params string[] specialNames)
        {
            this.specialNames = specialNames;
            this.onNamedEvent = onNamedEvent;
        }
        public JObject ToJSON()
        {
            if (this.specialNames.Length == 1)
                return new JObject
                {
                    ["name_filter"] = this.specialNames[0],
                    ["on_named"] = this.onNamedEvent.ToComponentJSON()
                };

            return new JObject
            {
                ["name_filter"] = new JArray(this.specialNames.Cast<object>().ToArray()),
                ["on_named"] = this.onNamedEvent.ToComponentJSON()
            };
        }
    }
}

public class ComponentNavigationClimb : EntityComponent
{
    public bool
        avoidDamageBlocks,
        avoidPortals,
        avoidSun,
        avoidWater,
        canBreachWater,
        canBreakDoors,
        canJump,
        canOpenDoors,
        canOpenIronDoors,
        canPassDoors,
        canPathFromAir,
        canPathOverLava,
        canPathOverWater,
        canSink,
        canSwim,
        canWalk,
        canWalkInLava,
        isAmphibious;
    public string[] blocksToAvoid;

    public override string GetIdentifier() { return "minecraft:navigation.climb"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["avoid_damage_blocks"] = this.avoidDamageBlocks,
            ["avoid_portals"] = this.avoidPortals,
            ["avoid_sun"] = this.avoidSun,
            ["avoid_water"] = this.avoidWater,
            ["can_breach"] = this.canBreachWater,
            ["can_break_doors"] = this.canBreakDoors,
            ["can_jump"] = this.canJump,
            ["can_open_doors"] = this.canOpenDoors,
            ["can_open_iron_doors"] = this.canOpenIronDoors,
            ["can_pass_doors"] = this.canPassDoors,
            ["can_path_from_air"] = this.canPathFromAir,
            ["can_path_over_lava"] = this.canPathOverLava,
            ["can_path_over_water"] = this.canPathOverWater,
            ["can_sink"] = this.canSink,
            ["can_swim"] = this.canSwim,
            ["can_walk"] = this.canWalk,
            ["can_walk_in_lava"] = this.canWalkInLava,
            ["is_amphibious"] = this.isAmphibious
        };

        if (this.blocksToAvoid != null)
            json["blocks_to_avoid"] = new JArray(this.blocksToAvoid);

        return json;
    }
}

public class ComponentNavigationFloat : EntityComponent
{
    public bool
        avoidDamageBlocks,
        avoidPortals,
        avoidSun,
        avoidWater,
        canBreachWater,
        canBreakDoors,
        canJump,
        canOpenDoors,
        canOpenIronDoors,
        canPassDoors,
        canPathFromAir,
        canPathOverLava,
        canPathOverWater,
        canSink,
        canSwim,
        canWalk,
        canWalkInLava,
        isAmphibious;
    public string[] blocksToAvoid;

    public override string GetIdentifier() { return "minecraft:navigation.float"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["avoid_damage_blocks"] = this.avoidDamageBlocks,
            ["avoid_portals"] = this.avoidPortals,
            ["avoid_sun"] = this.avoidSun,
            ["avoid_water"] = this.avoidWater,
            ["can_breach"] = this.canBreachWater,
            ["can_break_doors"] = this.canBreakDoors,
            ["can_jump"] = this.canJump,
            ["can_open_doors"] = this.canOpenDoors,
            ["can_open_iron_doors"] = this.canOpenIronDoors,
            ["can_pass_doors"] = this.canPassDoors,
            ["can_path_from_air"] = this.canPathFromAir,
            ["can_path_over_lava"] = this.canPathOverLava,
            ["can_path_over_water"] = this.canPathOverWater,
            ["can_sink"] = this.canSink,
            ["can_swim"] = this.canSwim,
            ["can_walk"] = this.canWalk,
            ["can_walk_in_lava"] = this.canWalkInLava,
            ["is_amphibious"] = this.isAmphibious
        };

        if (this.blocksToAvoid != null)
            json["blocks_to_avoid"] = new JArray(this.blocksToAvoid.Cast<object>().ToArray());

        return json;
    }
}

public class ComponentNavigationFly : EntityComponent
{
    public bool
        avoidDamageBlocks,
        avoidPortals,
        avoidSun,
        avoidWater,
        canBreachWater,
        canBreakDoors,
        canJump,
        canOpenDoors,
        canOpenIronDoors,
        canPassDoors,
        canPathFromAir,
        canPathOverLava,
        canPathOverWater,
        canSink,
        canSwim,
        canWalk,
        canWalkInLava,
        isAmphibious;
    public string[] blocksToAvoid;

    public override string GetIdentifier() { return "minecraft:navigation.fly"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["avoid_damage_blocks"] = this.avoidDamageBlocks,
            ["avoid_portals"] = this.avoidPortals,
            ["avoid_sun"] = this.avoidSun,
            ["avoid_water"] = this.avoidWater,
            ["can_breach"] = this.canBreachWater,
            ["can_break_doors"] = this.canBreakDoors,
            ["can_jump"] = this.canJump,
            ["can_open_doors"] = this.canOpenDoors,
            ["can_open_iron_doors"] = this.canOpenIronDoors,
            ["can_pass_doors"] = this.canPassDoors,
            ["can_path_from_air"] = this.canPathFromAir,
            ["can_path_over_lava"] = this.canPathOverLava,
            ["can_path_over_water"] = this.canPathOverWater,
            ["can_sink"] = this.canSink,
            ["can_swim"] = this.canSwim,
            ["can_walk"] = this.canWalk,
            ["can_walk_in_lava"] = this.canWalkInLava,
            ["is_amphibious"] = this.isAmphibious
        };

        if (this.blocksToAvoid != null)
            json["blocks_to_avoid"] = new JArray(this.blocksToAvoid.Cast<object>().ToArray());

        return json;
    }
}

public class ComponentNavigationGeneric : EntityComponent
{
    public bool
        avoidDamageBlocks,
        avoidPortals,
        avoidSun,
        avoidWater,
        canBreachWater,
        canBreakDoors,
        canJump,
        canOpenDoors,
        canOpenIronDoors,
        canPassDoors,
        canPathFromAir,
        canPathOverLava,
        canPathOverWater,
        canSink,
        canSwim,
        canWalk,
        canWalkInLava,
        isAmphibious;
    public string[] blocksToAvoid;

    public override string GetIdentifier() { return "minecraft:navigation.generic"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["avoid_damage_blocks"] = this.avoidDamageBlocks,
            ["avoid_portals"] = this.avoidPortals,
            ["avoid_sun"] = this.avoidSun,
            ["avoid_water"] = this.avoidWater,
            ["can_breach"] = this.canBreachWater,
            ["can_break_doors"] = this.canBreakDoors,
            ["can_jump"] = this.canJump,
            ["can_open_doors"] = this.canOpenDoors,
            ["can_open_iron_doors"] = this.canOpenIronDoors,
            ["can_pass_doors"] = this.canPassDoors,
            ["can_path_from_air"] = this.canPathFromAir,
            ["can_path_over_lava"] = this.canPathOverLava,
            ["can_path_over_water"] = this.canPathOverWater,
            ["can_sink"] = this.canSink,
            ["can_swim"] = this.canSwim,
            ["can_walk"] = this.canWalk,
            ["can_walk_in_lava"] = this.canWalkInLava,
            ["is_amphibious"] = this.isAmphibious
        };

        if (this.blocksToAvoid != null)
            json["blocks_to_avoid"] = new JArray(this.blocksToAvoid.Cast<object>().ToArray());

        return json;
    }
}

public class ComponentNavigationHover : EntityComponent
{
    public bool
        avoidDamageBlocks,
        avoidPortals,
        avoidSun,
        avoidWater,
        canBreachWater,
        canBreakDoors,
        canJump,
        canOpenDoors,
        canOpenIronDoors,
        canPassDoors,
        canPathFromAir,
        canPathOverLava,
        canPathOverWater,
        canSink,
        canSwim,
        canWalk,
        canWalkInLava,
        isAmphibious;
    public string[] blocksToAvoid;

    public override string GetIdentifier() { return "minecraft:navigation.hover"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["avoid_damage_blocks"] = this.avoidDamageBlocks,
            ["avoid_portals"] = this.avoidPortals,
            ["avoid_sun"] = this.avoidSun,
            ["avoid_water"] = this.avoidWater,
            ["can_breach"] = this.canBreachWater,
            ["can_break_doors"] = this.canBreakDoors,
            ["can_jump"] = this.canJump,
            ["can_open_doors"] = this.canOpenDoors,
            ["can_open_iron_doors"] = this.canOpenIronDoors,
            ["can_pass_doors"] = this.canPassDoors,
            ["can_path_from_air"] = this.canPathFromAir,
            ["can_path_over_lava"] = this.canPathOverLava,
            ["can_path_over_water"] = this.canPathOverWater,
            ["can_sink"] = this.canSink,
            ["can_swim"] = this.canSwim,
            ["can_walk"] = this.canWalk,
            ["can_walk_in_lava"] = this.canWalkInLava,
            ["is_amphibious"] = this.isAmphibious
        };

        if (this.blocksToAvoid != null)
            json["blocks_to_avoid"] = new JArray(this.blocksToAvoid.Cast<object>().ToArray());

        return json;
    }
}

public class ComponentNavigationSwim : EntityComponent
{
    public bool
        avoidDamageBlocks,
        avoidPortals,
        avoidSun,
        avoidWater,
        canBreachWater,
        canBreakDoors,
        canJump,
        canOpenDoors,
        canOpenIronDoors,
        canPassDoors,
        canPathFromAir,
        canPathOverLava,
        canPathOverWater,
        canSink,
        canSwim,
        canWalk,
        canWalkInLava,
        isAmphibious;
    public string[] blocksToAvoid;

    public override string GetIdentifier() { return "minecraft:navigation.swim"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["avoid_damage_blocks"] = this.avoidDamageBlocks,
            ["avoid_portals"] = this.avoidPortals,
            ["avoid_sun"] = this.avoidSun,
            ["avoid_water"] = this.avoidWater,
            ["can_breach"] = this.canBreachWater,
            ["can_break_doors"] = this.canBreakDoors,
            ["can_jump"] = this.canJump,
            ["can_open_doors"] = this.canOpenDoors,
            ["can_open_iron_doors"] = this.canOpenIronDoors,
            ["can_pass_doors"] = this.canPassDoors,
            ["can_path_from_air"] = this.canPathFromAir,
            ["can_path_over_lava"] = this.canPathOverLava,
            ["can_path_over_water"] = this.canPathOverWater,
            ["can_sink"] = this.canSink,
            ["can_swim"] = this.canSwim,
            ["can_walk"] = this.canWalk,
            ["can_walk_in_lava"] = this.canWalkInLava,
            ["is_amphibious"] = this.isAmphibious
        };

        if (this.blocksToAvoid != null)
            json["blocks_to_avoid"] = new JArray(this.blocksToAvoid.Cast<object>().ToArray());

        return json;
    }
}

public class ComponentNavigationWalk : EntityComponent
{
    public bool
        avoidDamageBlocks,
        avoidPortals,
        avoidSun,
        avoidWater,
        canBreachWater,
        canBreakDoors,
        canJump,
        canOpenDoors,
        canOpenIronDoors,
        canPassDoors,
        canPathFromAir,
        canPathOverLava,
        canPathOverWater,
        canSink,
        canSwim,
        canWalk,
        canWalkInLava,
        isAmphibious;
    public string[] blocksToAvoid;

    public override string GetIdentifier() { return "minecraft:navigation.walk"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["avoid_damage_blocks"] = this.avoidDamageBlocks,
            ["avoid_portals"] = this.avoidPortals,
            ["avoid_sun"] = this.avoidSun,
            ["avoid_water"] = this.avoidWater,
            ["can_breach"] = this.canBreachWater,
            ["can_break_doors"] = this.canBreakDoors,
            ["can_jump"] = this.canJump,
            ["can_open_doors"] = this.canOpenDoors,
            ["can_open_iron_doors"] = this.canOpenIronDoors,
            ["can_pass_doors"] = this.canPassDoors,
            ["can_path_from_air"] = this.canPathFromAir,
            ["can_path_over_lava"] = this.canPathOverLava,
            ["can_path_over_water"] = this.canPathOverWater,
            ["can_sink"] = this.canSink,
            ["can_swim"] = this.canSwim,
            ["can_walk"] = this.canWalk,
            ["can_walk_in_lava"] = this.canWalkInLava,
            ["is_amphibious"] = this.isAmphibious
        };

        if (this.blocksToAvoid != null)
            json["blocks_to_avoid"] = new JArray(this.blocksToAvoid.Cast<object>().ToArray());

        return json;
    }
}

public class ComponentOutOfControl : EntityComponent
{
    public override string GetIdentifier() { return "minecraft:out_of_control"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentPeek : EntityComponent
{
    public EntityEventHandler spottedEvent;
    public EntityEventHandler startPeekingEvent;
    // shulker peeking

    public EntityEventHandler stopPeekingEvent;

    public override string GetIdentifier() { return "minecraft:peek"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["on_close"] = this.stopPeekingEvent.ToComponentJSON(),
            ["on_open"] = this.startPeekingEvent.ToComponentJSON(),
            ["on_target_open"] = this.spottedEvent.ToComponentJSON()
        };
    }
}

public class ComponentPersistent : EntityComponent
{
    public override string GetIdentifier() { return "minecraft:persistent"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentPhysics : EntityComponent
{
    public bool collision;
    public bool gravity;

    public override string GetIdentifier() { return "minecraft:physics"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["has_collision"] = this.collision,
            ["has_gravity"] = this.gravity
        };
    }
}

public class ComponentPreferredPath : EntityComponent
{
    public PriorityBlocks[] blocks;

    // used by villagers to walk on paths
    public float defaultBlockCost;
    public int jumpCost;
    public int maxSafeFall;

    public override string GetIdentifier() { return "minecraft:prefered_path"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["default_block_cost"] = this.defaultBlockCost,
            ["jump_cost"] = this.jumpCost,
            ["max_fall_blocks"] = this.maxSafeFall
        };

        if (this.blocks != null)
            json["preferred_path_blocks"] = new JArray(this.blocks.Select(b => b.ToJSON()));

        return json;
    }

    public struct PriorityBlocks
    {
        public int cost;
        public string[] blocks;

        public PriorityBlocks(int cost, params string[] blocks)
        {
            this.cost = cost;
            this.blocks = blocks;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["cost"] = this.cost,
                ["blocks"] = new JArray(this.blocks.Cast<object>().ToArray())
            };
        }
    }
}

public class ComponentProjectile : EntityComponent
{
    public float angleOffset;
    public bool catchFire;
    public bool critParticle;
    public float dealFireTime;
    public PotionEffect? dealPotionEffect;
    public bool destroyOnHurt;
    public bool doesKnockback;

    public bool followMobGriefing;
    public bool gotoTargetOfShooter;
    public float gravity;
    public string hitSound;
    public bool homing;
    public FilterCollection immuneEntities;
    public float inertia;
    public bool isDangerous;
    public bool isSplashPotion;
    public float liquidInertia;
    public float offsetX = 0;
    public float offsetY = 0;
    public float offsetZ = 0;
    public string particle = "iconcrack";
    public bool pierces;
    public float power;
    public bool randomizeDamage;
    public bool reflectOffHit;
    public string shootSound;
    public bool shouldBounce;
    public float splashRange;
    public bool strikesLightning;
    public float uncertaintyBase;
    public float uncertaintyMultiplier;

    public override string GetIdentifier() { return "minecraft:projectile"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["angle_offset"] = this.angleOffset,
            ["catch_fire"] = this.catchFire,
            ["crit_particle_on_hurt"] = this.critParticle,
            ["destroy_on_hurt"] = this.destroyOnHurt,
            ["fire_affected_by_griefing"] = this.followMobGriefing,
            ["gravity"] = this.gravity,
            ["homing"] = this.homing,
            ["inertia"] = this.inertia,
            ["is_dangerous"] = this.isDangerous,
            ["knockback"] = this.doesKnockback,
            ["lightning"] = this.strikesLightning,
            ["liquid_inertia"] = this.liquidInertia,
            ["multiple_targets"] = this.pierces,
            ["offset"] = new JArray(new[] {this.offsetX, this.offsetY, this.offsetZ}),
            ["on_fire_time"] = this.dealFireTime,
            ["particle"] = this.particle,
            ["potion_effect"] = this.dealPotionEffect.HasValue ? (int) this.dealPotionEffect.Value : -1,
            ["power"] = this.power,
            ["reflect_on_hurt"] = this.reflectOffHit,
            ["semi_random_diff_damage"] = this.randomizeDamage,
            ["shoot_target"] = this.gotoTargetOfShooter,
            ["should_bounce"] = this.shouldBounce,
            ["splash_potion"] = this.isSplashPotion,
            ["splash_range"] = this.splashRange,
            ["uncertainty_base"] = this.uncertaintyBase,
            ["uncertainty_multiplier"] = this.uncertaintyMultiplier
        };

        if (this.immuneEntities != null)
            json["filter"] = this.immuneEntities.ToJSON();
        if (this.hitSound != null)
            json["hit_sound"] = this.hitSound;
        if (this.shootSound != null)
            json["shoot_sound"] = this.shootSound;

        return json;
    }
}

public class ComponentPushable : EntityComponent
{
    public bool isPushableByEntity;
    public bool isPushableByPiston;

    public override string GetIdentifier() { return "minecraft:pushable"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["is_pushable"] = this.isPushableByEntity,
            ["is_pushable_by_piston"] = this.isPushableByPiston
        };
    }
}

public class ComponentRaidTrigger : EntityComponent
{
    // used only by the player to trigger a raid.

    public EntityEventHandler raidEvent;
    public override string GetIdentifier() { return "minecraft:raid_trigger"; }
    public override JObject _GetValue()
    {
        var json = new JObject();
        if (this.raidEvent != null)
            json["triggered_event"] = this.raidEvent.ToComponentJSON();
        return json;
    }
}

public class ComponentRailMovement : EntityComponent
{
    public float maxSpeed;

    public override string GetIdentifier() { return "minecraft:rail_movement"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["max_speed"] = this.maxSpeed
        };
    }
}

public class ComponentRailSensor : EntityComponent
{
    public bool checkBlockTypes; // allows onDeactivateEvent
    public bool ejectOnActivate;
    public bool ejectOnDeactivate;

    public EntityEventHandler onActivateEvent;
    public EntityEventHandler onDeactivateEvent;

    public bool tickCommandBlockOnActivate;
    public bool tickCommandBlockOnDeactivate;

    public override string GetIdentifier() { return "minecraft:rail_sensor"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["check_block_types"] = this.checkBlockTypes,
            ["eject_on_activate"] = this.ejectOnActivate,
            ["eject_on_deactivate"] = this.ejectOnDeactivate,
            ["tick_command_block_on_activate"] = this.tickCommandBlockOnActivate,
            ["tick_command_block_on_deactivate"] = this.tickCommandBlockOnDeactivate
        };

        if (this.onActivateEvent != null)
            json["on_activate"] = this.onActivateEvent.ToComponentJSON();
        if (this.onDeactivateEvent != null)
            json["on_deactivate"] = this.onDeactivateEvent.ToComponentJSON();

        return json;
    }
}

public class ComponentRavagerBlocked : EntityComponent
{
    public ReactionChoice[] choices;

    public float knockbackStrength;
    public override string GetIdentifier() { return "minecraft:ravager_blocked"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["knockback_strength"] = this.knockbackStrength
        };

        if (this.choices != null)
            json["reaction_choices"] = new JArray(this.choices.Select(choice => choice.ToJSON()));

        return json;
    }

    public struct ReactionChoice
    {
        public int weight;
        public EntityEventHandler value;

        public ReactionChoice(int weight, EntityEventHandler value)
        {
            this.weight = weight;
            this.value = value;
        }
        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["weight"] = this.weight
            };

            if (this.value != null)
                json["value"] = this.value.ToComponentJSON();

            return json;
        }
    }
}

public class ComponentRideable : EntityComponent
{
    public string[] allowedFamilyTypes;

    public int controllingSeat;
    public bool crouchingSkipInteract;
    public string interactText;
    public bool pullInEntities;
    public bool riderCanInteract;

    public int seatCount;
    public Seat[] seats;

    public override string GetIdentifier() { return "minecraft:rideable"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["controlling_seat"] = this.controllingSeat,
            ["crouching_skip_interact"] = this.crouchingSkipInteract,
            ["priority"] = 0,
            ["pull_in_entities"] = this.pullInEntities,
            ["rider_can_interact"] = this.riderCanInteract,
            ["seat_count"] = this.seatCount
        };

        if (this.allowedFamilyTypes != null)
            json["family_types"] = new JArray(this.allowedFamilyTypes.Cast<object>().ToArray());
        if (this.interactText != null)
            json["interact_text"] = this.interactText;
        if (this.seats != null)
            json["seats"] = new JArray(this.seats.Select(seat => seat.ToJSON()));

        return json;
    }

    public struct Seat
    {
        public float? lockRiderRotation;
        public int? maxRiderCount; // omit for "seat_count"
        public int minRiderCount;
        public float posX, posY, posZ;
        public MolangValue rotateRiderBy;

        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["min_rider_count"] = this.minRiderCount,
                ["position"] = new JArray(new[] {this.posX, this.posY, this.posZ}),
                ["rotate_rider_by"] = this.rotateRiderBy == null ? 0 : this.rotateRiderBy.ToJSON()
            };

            if (this.lockRiderRotation.HasValue)
                json["lock_rider_rotation"] = this.lockRiderRotation.Value;

            if (this.maxRiderCount.HasValue)
                json["max_rider_count"] = this.maxRiderCount.Value;
            else
                json["max_rider_count"] = "seat_count";

            return json;
        }
    }
}

public class ComponentScaffoldingClimber : EntityComponent
{
    // deprecated
    public override string GetIdentifier() { return "minecraft:scaffolding_climber"; }
    public override JObject _GetValue() { return new JObject(); }
}

public class ComponentScaleByAge : EntityComponent
{
    public float endScale;
    public float startScale;
    public override string GetIdentifier() { return "minecraft:scale_by_age"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["start_scale"] = this.startScale,
            ["end_scale"] = this.endScale
        };
    }
}

public class ComponentScheduler : EntityComponent
{
    public ScheduledEvent[] events;
    public int? maxDelaySeconds;

    // undocumented??
    public int? minDelaySeconds;

    public override string GetIdentifier() { return "minecraft:scheduler"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["scheduled_events"] = new JArray(this.events.Select(e => e.ToJSON()))
        };

        if (this.minDelaySeconds.HasValue)
            json["min_delay_secs"] = this.minDelaySeconds.Value;
        if (this.maxDelaySeconds.HasValue)
            json["max_delay_secs"] = this.maxDelaySeconds.Value;

        return json;
    }

    public struct ScheduledEvent
    {
        public FilterCollection[] tests;
        public EntityEventHandler call;

        public ScheduledEvent(EntityEventHandler call, params FilterCollection[] tests)
        {
            this.call = call;
            this.tests = tests;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["event"] = this.call.ToComponentJSON(),
                ["filters"] = new JArray(this.tests.Select(test => test.ToJSON()))
            };
        }
    }
}

public class ComponentShareables : EntityComponent
{
    public bool allItems = false;
    public int allItemsMaxAmount = -1;
    public int allItemsSurplusAmount = -1;
    public int allItemsWantAmount = -1;

    public Shareable[] items;

    public override string GetIdentifier() { return "minecraft:shareables"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["all_items"] = this.allItems,
            ["all_items_max_amount"] = this.allItemsMaxAmount,
            ["all_items_surplus_amount"] = this.allItemsSurplusAmount,
            ["all_items_want_amount"] = this.allItemsWantAmount
        };

        if (this.items != null)
            json["items"] = new JArray(this.items.Select(item => item.ToJSON()));

        return json;
    }

    public struct Shareable
    {
        public string craftInfo;
        public string item;

        public bool admire;
        public bool barter; // requires ComponentBarter
        public bool consumeItem;
        public int maxAmount;
        public int pickupLimit;
        public int priority;
        public bool storeInInventory; // requires ComponentInventory
        public int surplusAmount;
        public int wantAmount;

        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["admire"] = this.admire,
                ["barter"] = this.barter,
                ["consume_item"] = this.consumeItem,
                ["max_amount"] = this.maxAmount,
                ["pickup_limit"] = this.pickupLimit,
                ["priority"] = this.priority,
                ["stored_in_inventory"] = this.storeInInventory,
                ["surplus_amount"] = this.surplusAmount,
                ["want_amount"] = this.wantAmount
            };

            if (this.craftInfo != null)
                json["craft_into"] = this.craftInfo;
            if (this.item != null)
                json["item"] = this.item;

            return json;
        }
    }
}

public class ComponentShooter : EntityComponent
{
    public PotionEffect? passEffectToProjectile;
    public string projectileEntity;

    public override string GetIdentifier() { return "minecraft:shooter"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["aux_val"] = this.passEffectToProjectile.HasValue ? (int) this.passEffectToProjectile.Value : -1,
            ["def"] = this.projectileEntity
        };
    }
}

public class ComponentSittable : EntityComponent
{
    // sittable like a cat or dog

    public EntityEventHandler sitEvent;
    public EntityEventHandler standEvent;

    public override string GetIdentifier() { return "minecraft:sittable"; }
    public override JObject _GetValue()
    {
        var json = new JObject();

        if (this.sitEvent != null)
            json["sit_event"] = this.sitEvent.ToComponentJSON();
        if (this.standEvent != null)
            json["stand_event"] = this.standEvent.ToComponentJSON();

        return json;
    }
}

public class ComponentSpawnEntity : EntityComponent
{
    public SpawnEntity[] entities;
    public override string GetIdentifier() { return "minecraft:spawn_entity"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["entities"] = new JArray(this.entities.Select(entity => entity.ToJSON()))
        };
    }

    public struct SpawnEntity
    {
        public string spawnEntity;
        public string spawnMethod;

        public FilterCollection requirements;
        public EntityEventHandler spawnEvent;
        public string spawnSound;
        public string spawnItem;

        public int minWaitTime, maxWaitTime;
        public int spawnCount;
        public bool shouldLeash;
        public bool singleUse;

        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["min_wait_time"] = this.minWaitTime,
                ["max_wait_time"] = this.maxWaitTime,
                ["num_to_spawn"] = this.spawnCount,
                ["should_leash"] = this.shouldLeash,
                ["single_use"] = this.singleUse
            };

            if (this.spawnEntity != null)
            {
                json["spawn_entity"] = this.spawnEntity;
                if (this.spawnMethod != null)
                    json["spawn_method"] = this.spawnMethod;
            }

            if (this.spawnItem != null)
                json["spawn_item"] = this.spawnItem;
            if (this.spawnSound != null)
                json["spawn_sound"] = this.spawnSound;
            if (this.requirements != null)
                json["filters"] = this.requirements.ToJSON();
            if (this.spawnEvent != null)
                json["spawn_event"] = this.spawnEvent.ToComponentJSON();

            return json;
        }
    }
}

public class ComponentTameable : EntityComponent
{
    public float probability; // 0.0 - 1.0
    public EntityEventHandler tameEvent;
    public string[] tameItems;

    public override string GetIdentifier() { return "minecraft:tameable"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["probability"] = this.probability
        };

        if (this.tameEvent != null)
            json["tame_event"] = this.tameEvent.ToComponentJSON();
        if (this.tameItems != null)
            json["tame_items"] = new JArray(this.tameItems.Cast<object>().ToArray());

        return json;
    }
}

public class ComponentTameMount : EntityComponent
{
    public int attemptTemperMod;
    public FeedItem[] feedItems;
    public string feedText;
    public int minTemper, maxTemper;

    public AutoRejectItem[] rejectItems;
    public string rideText;
    public EntityEventHandler tameEvent;

    public override string GetIdentifier() { return "minecraft:tamemount"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["attempt_temper_mod"] = this.attemptTemperMod,
            ["min_temper"] = this.minTemper,
            ["max_temper"] = this.maxTemper
        };

        if (this.rejectItems != null)
            json["auto_reject_items"] = new JArray(this.rejectItems.Select(item => item.ToJSON()));
        if (this.feedItems != null)
            json["feed_items"] = new JArray(this.feedItems.Select(item => item.ToJSON()));
        if (this.tameEvent != null)
            json["tame_event"] = this.tameEvent.ToComponentJSON();
        if (this.feedText != null)
            json["feed_text"] = this.feedText;
        if (this.rideText != null)
            json["ride_text"] = this.rideText;

        return json;
    }

    public struct AutoRejectItem
    {
        public string item;

        public AutoRejectItem(string item) { this.item = item; }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["item"] = this.item
            };
        }
    }

    public struct FeedItem
    {
        public string item;
        public int temperMod;

        public FeedItem(string item, int temperMod)
        {
            this.item = item;
            this.temperMod = temperMod;
        }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["item"] = this.item,
                ["tempter_mod"] = this.temperMod
            };
        }
    }
}

public class ComponentTargetNearbySensor : EntityComponent
{
    public float insideRange;
    public bool mustSee;

    public EntityEventHandler onInsideRangeEvent;
    public EntityEventHandler onOutsideRangeEvent;
    public EntityEventHandler onVisionLostInsideRangeEvent;
    public float outsideRange;

    public override string GetIdentifier() { return "minecraft:target_nearby_sensor"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["inside_range"] = this.insideRange,
            ["outside_range"] = this.outsideRange,
            ["must_see"] = this.mustSee
        };

        if (this.onInsideRangeEvent != null)
            json["on_inside_range"] = this.onInsideRangeEvent.ToComponentJSON();
        if (this.onOutsideRangeEvent != null)
            json["on_outside_range"] = this.onOutsideRangeEvent.ToComponentJSON();
        if (this.onVisionLostInsideRangeEvent != null)
            json["on_vision_lost_inside_range"] = this.onVisionLostInsideRangeEvent.ToComponentJSON();

        return json;
    }
}

public class ComponentTeleport : EntityComponent
{
    public int cubeX, cubeY, cubeZ;
    public float darkTeleportChance;
    public float lightTeleportChance;
    public float maxRandomTeleportTime;
    public float minRandomTeleportTime;
    public bool randomTeleports;

    public float targetDistance;
    public float targetTeleportChance;

    public override string GetIdentifier() { return "minecraft:teleport"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["dark_teleport_chance"] = this.darkTeleportChance,
            ["light_teleport_chance"] = this.lightTeleportChance,
            ["min_random_teleport_time"] = this.minRandomTeleportTime,
            ["max_random_teleport_time"] = this.maxRandomTeleportTime,
            ["random_teleport_cube"] = new JArray(new[] {this.cubeX, this.cubeY, this.cubeZ}),
            ["random_teleports"] = this.randomTeleports,
            ["target_distance"] = this.targetDistance,
            ["target_teleport_chance"] = this.targetTeleportChance
        };
    }
}

public class ComponentTickWorld : EntityComponent
{
    public float distanceToPlayers; // only assigned if neverDespawn == false
    public bool neverDespawn;
    public int tickRadius;

    public override string GetIdentifier() { return "minecraft:tick_world"; }
    public override JObject _GetValue()
    {
        if (this.neverDespawn)
            return new JObject
            {
                ["never_despawn"] = true,
                ["radius"] = this.tickRadius
            };
        return new JObject
        {
            ["distance_to_players"] = this.distanceToPlayers,
            ["never_despawn"] = false,
            ["radius"] = this.tickRadius
        };
    }
}

public class ComponentTimer : EntityComponent
{
    public EntityEventHandler call;
    public bool looping;
    public bool randomInterval;
    public float timeMax;
    public float timeMin;

    /// <summary>
    ///     Set time to a single value.
    /// </summary>
    public float TimeSingle
    {
        set
        {
            this.timeMin = value;
            this.timeMax = value;
        }
    }

    public override string GetIdentifier() { return "minecraft:timer"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["looping"] = this.looping,
            ["randomInterval"] = this.randomInterval,
            ["time_down_event"] = this.call.ToComponentJSON()
        };

        if (Math.Abs(this.timeMin - this.timeMax) < 0.0001F)
            json["time"] = this.timeMin;
        else
            json["time"] = new JArray(new[] {this.timeMin, this.timeMax});

        return json;
    }
}

public class ComponentTradeTable : EntityComponent
{
    public bool convertEconomy;
    public string displayName;
    public bool newScreen;
    public bool persistTrades;
    public LootTable table;

    public override string GetIdentifier() { return "minecraft:trade_table"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["table"] = this.table.ResourcePath,
            ["convert_trades_economy"] = this.convertEconomy,
            ["new_screen"] = this.newScreen,
            ["persist_trades"] = this.persistTrades
        };

        if (this.displayName != null)
            json["display_name"] = this.displayName;

        return json;
    }
}

public class ComponentTrail : EntityComponent
{
    public string block;
    public FilterCollection filters;
    public int offsetX, offsetY, offsetZ;

    public override string GetIdentifier() { return "minecraft:trail"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["block_type"] = this.block,
            ["spawn_offset"] = new JArray(new[] {this.offsetX, this.offsetY, this.offsetZ})
        };

        if (this.filters != null)
            json["spawn_filter"] = this.filters.ToJSON();

        return json;
    }
}

public class ComponentTransformation : EntityComponent
{
    public string beginTransformationSound;
    public Delay? delay;

    public bool dropEquipment;
    public bool dropInventory;
    public string endTransformationSound;

    public Add[] groupsToAdd;
    public bool keepLevel;
    public bool keepOwner;
    public bool preserveEquipment;
    public string transformInto;

    public override string GetIdentifier() { return "minecraft:transformation"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["drop_equipment"] = this.dropEquipment,
            ["drop_inventory"] = this.dropInventory,
            ["keep_level"] = this.keepLevel,
            ["keep_owner"] = this.keepOwner,
            ["preserve_equipment"] = this.preserveEquipment
        };

        if (this.groupsToAdd != null)
            json["add"] = new JArray(this.groupsToAdd.Select(add => add.ToJSON()));
        if (this.beginTransformationSound != null)
            json["begin_transform_sound"] = this.beginTransformationSound;
        if (this.delay.HasValue)
            json["delay"] = this.delay.Value.ToJSON();
        if (this.beginTransformationSound != null)
            json["transformation_sound"] = this.endTransformationSound;
        if (this.beginTransformationSound != null)
            json["into"] = this.transformInto;

        return json;
    }

    public struct Add
    {
        public EntityComponentGroup[] groups;
        public Add(params EntityComponentGroup[] groups) { this.groups = groups; }
        public JObject ToJSON()
        {
            return new JObject
            {
                ["component_groups"] = new JArray(this.groups.Select(group => group.name))
            };
        }
    }

    public struct Delay
    {
        public float blockAssistChance; // 0.0 - 1.0
        public float blockChance; // 0.0 - 1.0
        public int blockMax;
        public int blockRadius;
        public string[] speedUpBlocks;
        public float value;

        public JObject ToJSON()
        {
            var json = new JObject
            {
                ["block_assist_chance"] = this.blockAssistChance,
                ["block_chance"] = this.blockChance,
                ["block_max"] = this.blockMax,
                ["block_radius"] = this.blockRadius,
                ["value"] = this.value
            };

            if (this.speedUpBlocks != null)
                json["block_type"] = new JArray(this.speedUpBlocks.Cast<object>().ToArray());

            return json;
        }
    }
}

public class ComponentTrusting : EntityComponent
{
    public float probability; // 0.0 - 1.0
    public EntityEventHandler trustEvent;
    public string[] trustItems;

    public override string GetIdentifier() { return "minecraft:trusting"; }
    public override JObject _GetValue()
    {
        var json = new JObject
        {
            ["probability"] = this.probability,
            ["trust_event"] = this.trustEvent.ToComponentJSON()
        };

        if (this.trustItems != null)
            json["trust_items"] = new JArray(this.trustItems.Cast<object>().ToArray());

        return json;
    }
}

public class ComponentWaterMovement : EntityComponent
{
    public float dragFactor;

    public override string GetIdentifier() { return "minecraft:water_movement"; }
    public override JObject _GetValue()
    {
        return new JObject
        {
            ["drag_factor"] = this.dragFactor
        };
    }
}