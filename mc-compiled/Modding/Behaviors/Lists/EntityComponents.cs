using mc_compiled.Commands;
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
        public EntityEventHandler growUpEvent;
        
        public override string GetIdentifier() =>
            "minecraft:ageable";
        public override JObject _GetValue()
        {
            var json = new JObject()
            {
                ["duration"] = duration
            };
            if (dropItems != null)
                json["drop_items"] = new JArray(dropItems);
            if (feedItems != null)
                json["feed_items"] = new JArray(feedItems);
            if (growUpEvent != null)
                json["grow_up"] = growUpEvent.ToComponentJSON();

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
        public EntityEventHandler onCooldownComplete;
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

            if (onCooldownComplete != null)
                json["attack_cooldown_complete_event"] = onCooldownComplete.ToComponentJSON();

            return json;
        }
    }
    public class ComponentBarter : EntityComponent
    {
        public LootTable barterTable;
        public int cooldownAfterBeingAttacked;
        public override string GetIdentifier() =>
            "minecraft:barter";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["barter_table"] = barterTable.ResourcePath,
                ["cooldown_after_being_attacked"] = cooldownAfterBeingAttacked
            };
        }
    }
    public class ComponentBlockClimber : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:block_climber";
        public override JObject _GetValue()
        {
            return new JObject() { };
        }
    }
    public class ComponentBlockSensor : EntityComponent
    {
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
                return new JObject()
                {
                    ["block_list"] = new JArray(blockList),
                    ["on_block_broken"] = onBlockBroken
                };
            }
        }

        public SensorPool[] pools;
        public float sensorRadius;

        public override string GetIdentifier() =>
            "minecraft:block_sensor";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["sensor_radius"] = sensorRadius,
                ["on_break"] = new JArray(pools.Select(sp => sp.ToJSON()))
            };
        }
    }
    public class ComponentBoostable : EntityComponent
    {
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
                return new JObject()
                {
                    ["item"] = identifier,
                    ["damage"] = damage,
                    ["replace_item"] = replaceItem
                };
            }
        }

        public BoostItem[] boostItems;
        public float boostDuration;
        public float speedMultiplier;

        public override string GetIdentifier() =>
            "minecraft:boostable";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["duration"] = boostDuration,
                ["speed_multiplier"] = speedMultiplier,
                ["boost_items"] = new JArray(boostItems.Select(bi => bi.ToJSON()))
            };
        }
    }
    public class ComponentBoss : EntityComponent
    {
        public int hudRange;
        public string bossbarText;
        public bool shouldDarkenSky;

        public override string GetIdentifier() =>
            "minecraft:boss";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["hud_range"] = hudRange,
                ["name"] = bossbarText,
                ["should_darken_sky"] = shouldDarkenSky
            };
        }
    }
    public class ComponentBreakBlocks : EntityComponent
    {
        public string[] breakableBlocks;
        public override string GetIdentifier() =>
            "minecraft:break_blocks";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["breakable_blocks"] = new JArray(breakableBlocks)
            };
        }
    }
    public class ComponentBreathable : EntityComponent
    {
        public string[] breatheBlocks;
        public string[] nonBreatheBlocks;

        public bool breathesAir;
        public bool breathesLava;
        public bool breathesSolids;
        public bool breathesWater;
        public bool generatesBubbles;
        public float inhaleTime;
        public int suffocateTime;
        public int totalSupply;

        public override string GetIdentifier() =>
            "minecraft:breathable";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["breathes_air"] = breathesAir,
                ["breathes_lava"] = breathesLava,
                ["breathes_solids"] = breathesSolids,
                ["breathes_water"] = breathesWater,
                ["generates_bubbles"] = generatesBubbles,
                ["inhale_time"] = inhaleTime,
                ["suffocate_time"] = suffocateTime,
                ["total_supply"] = totalSupply
            };
            if (breatheBlocks != null)
                json["breathe_blocks"] = new JArray(breatheBlocks);
            if (nonBreatheBlocks != null)
                json["non_breathe_blocks"] = new JArray(nonBreatheBlocks);

            return json;
        }
    }
    public class ComponentBreedable : EntityComponent
    {
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
                JObject json = new JObject()
                {
                    ["baby_type"] = babyType,
                    ["mate_type"] = mateType
                };

                if (breedEventToCall != null)
                    json["breed_event"] = breedEventToCall.ToComponentJSON();

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
                return new JObject()
                {
                    ["chance"] = chance,
                    ["max_variant"] = maxVariant,
                    ["min_variant"] = minVariant
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
                return new JObject()
                {
                    ["blocks"] = new JArray(nearbyBlocks),
                    ["count"] = count,
                    ["radius"] = radius
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
                return new JObject()
                {
                    ["color"] = colorChance,
                    ["extra_variant"] = extraVariantChance,
                    ["variant"] = variantChance
                };
            }
        }

        public bool canBreedWhileSitting;
        public bool blendAttributes;
        public float breedCooldown;
        public bool causesPregnancy;
        public float extraBabyChance; // 0.0 - 1.0
        public bool inheritTamed;
        public bool requireFullHealth;
        public bool requireTamed;

        public string[] breedItems;
        public BreedEntity[] breedsWith;
        public DenyParentsVariant? denyParents;
        public EnvironmentRequirement[] environmentRequirements;
        public FilterCollection loveFilters;
        public MutationFactor? mutationFactor;

        public override string GetIdentifier() =>
            "minecraft:breedable";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["allow_sitting"] = canBreedWhileSitting,
                ["blend_attributes"] = blendAttributes,
                ["breed_cooldown"] = breedCooldown,
                ["causes_pregnancy"] = causesPregnancy,
                ["extra_baby_chance"] = extraBabyChance,
                ["inherit_tamed"] = inheritTamed,
                ["require_full_health"] = requireFullHealth,
                ["require_tame"] = requireTamed
            };

            if (breedItems != null)
                json["breed_items"] = new JArray(breedItems);
            if (breedsWith != null)
                json["breeds_with"] = new JArray(breedsWith.Select(be => be.ToJSON()));
            if (denyParents.HasValue)
                json["deny_parents_variant"] = denyParents.Value.ToJSON();
            if (environmentRequirements != null)
                json["environment_requirements"] = new JArray(environmentRequirements.Select(er => er.ToJSON()));
            if (loveFilters != null)
                json["love_filters"] = loveFilters.ToJSON();
            if (mutationFactor.HasValue)
                json["mutation_factor"] = mutationFactor.Value.ToJSON();

            return json;
        }
    }
    public class ComponentBribeable : EntityComponent
    {
        public float bribeCooldown;
        public string[] bribeItems;

        public override string GetIdentifier() =>
            "minecraft:bribeable";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["bribe_cooldown"] = bribeCooldown,
                ["bribe_items"] = new JArray(bribeItems)
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

        public override string GetIdentifier() =>
            "minecraft:buoyant";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["apply_gravity"] = applyGravity,
                ["base_buoyancy"] = baseBuoyancy,
                ["big_wave_probability"] = bigWaveProbability,
                ["big_wave_speed"] = bigWaveSpeed,
                ["drag_down_on_buoyancy_removed"] = dragDownOnBuoyancyRemoved,
                ["liquid_blocks"] = new JArray(liquidBlocks),
                ["simulate_waves"] = simulateWaves
            };
        }
    }
    public class ComponentBurnsInDaylight : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:burns_in_daylight";
        public override JObject _GetValue()
        {
            return new JObject() { };
        }
    }
    public class ComponentCelebrateHunt : EntityComponent
    {
        // for luke tmrw:
        // https://docs.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/entitycomponents/minecraftcomponent_celebrate_hunt
        // yo thanks luke from yesterday
        public bool broadcast;
        public FilterCollection celebrationTargets;
        public string celebrationSound;
        public int duration;
        public float radius;
        public float minSoundInterval;
        public float maxSoundInterval;

        public override string GetIdentifier() =>
            "minecraft:celebrate_hunt";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["broadcast"] = broadcast,
                ["duration"] = duration,
                ["radius"] = radius,
                ["sound_interval"] = new JObject()
                {
                    ["range_min"] = minSoundInterval,
                    ["range_max"] = maxSoundInterval
                }
            };

            if(celebrationTargets != null)
                json["celebration_targets"] = celebrationTargets.ToJSON();
            if (celebrationSound != null)
                json["celebrate_sound"] = celebrationSound;

            return json;
        }
    }
    public class ComponentCombatRegeneration : EntityComponent
    {
        public int regenDuration;
        public bool applyToSelf;
        public bool applyToFamily;

        public override string GetIdentifier() =>
            "minecraft:combat_regeneration";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["regeneration_duration"] = regenDuration,
                ["apply_to_self"] = applyToSelf,
                ["apply_to_family"] = applyToFamily
            };
        }
    }
    public class ComponentConditionalBandwidthOptimization : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:conditional_bandwidth_optimization";
        public override JObject _GetValue()
        {
            return new JObject() { };
        }
    }
    public class ComponentCustomHitTest : EntityComponent
    {
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
                return new JObject()
                {
                    ["pivot"] = pivot.ToArray(),
                    ["width"] = width,
                    ["height"] = height
                };
            }
        }
        public Hitbox[] hitboxes;

        public override string GetIdentifier() =>
            "minecraft:custom_hit_test";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["hitboxes"] = new JArray(hitboxes.Select(hb => hb.ToJSON()))
            };
        }
    }
    public class ComponentDamageOverTime : EntityComponent
    {
        public int damagePerHurt;
        public float timeBetweenHurt;

        public override string GetIdentifier() =>
            "minecraft:damage_over_time";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["damage_per_hurt"] = damagePerHurt,
                ["time_between_hurt"] = timeBetweenHurt
            };
        }
    }
    public class ComponentDamageSensor : EntityComponent
    {
        public struct Trigger
        {
            public Commands.DamageCause cause;
            public float damageModifier;
            public float damageMultiplier;
            public bool dealsDamage;
            public string onDamageSoundEvent;
            public FilterCollection onDamageFilters;
            public EntityEventHandler onDamage;

            public JObject ToJSON()
            {
                JObject json = new JObject()
                {
                    ["cause"] = cause.ToString(),
                    ["damage_modifier"] = damageModifier,
                    ["damage_multiplier"] = damageMultiplier,
                    ["deals_damage"] = dealsDamage
                };

                if (onDamageSoundEvent != null)
                    json["on_damage_sound_event"] = onDamageSoundEvent;

                bool hasFilter = onDamageFilters != null;
                bool hasEvent = onDamage != null;

                if (hasFilter | hasEvent)
                {
                    JObject child = new JObject();
                    if (hasFilter)
                        child["filters"] = onDamageFilters.ToJSON();
                    if (hasEvent)
                    {
                        child["event"] = onDamage.eventID;
                        child["target"] = onDamage.target.ToString();
                    }
                }

                return json;
            }
        }

        public Trigger[] triggerPool;
        public override string GetIdentifier() =>
            "minecraft:damage_sensor";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["triggers"] = new JArray(triggerPool.Select(t => t.ToJSON()))
            };
        }
    }
    public class ComponentDespawn : EntityComponent
    {
        public bool despawnFromDistance;
        public int minDistance, maxDistance;

        public bool despawnFromChance;
        public int randomChance;

        public bool despawnFromInactivity;
        public int inactivityTimer;

        public FilterCollection despawnRequirements;
        public bool removeChildEntities;
        public bool despawnFromSimulationEdge;

        public override string GetIdentifier() =>
            "minecraft:despawn";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["despawn_from_chance"] = despawnFromChance,
                ["despawn_from_inactivity"] = despawnFromInactivity,
                ["despawn_from_simulation_edge"] = despawnFromSimulationEdge,
                ["min_range_inactivity_timer"] = inactivityTimer,
                ["min_range_random_chance"] = randomChance,
                ["remove_child_entities"] = removeChildEntities
            };
            if (despawnFromDistance)
                json["despawn_from_distance"] = new JObject()
                {
                    ["max_distance"] = maxDistance,
                    ["min_distance"] = minDistance
                };
            if (despawnRequirements != null)
                json["filters"] = despawnRequirements.ToJSON();
            return json;
        }
    }
    public class ComponentDryingOutTimer : EntityComponent
    {
        public float totalTime;
        public EntityEventHandler driedOutEvent;
        public EntityEventHandler recoverEvent;
        public EntityEventHandler stoppedDryingEvent;
        public float waterBottleRefillTime;

        public override string GetIdentifier() =>
            "minecraft:drying_out_timer";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["total_time"] = totalTime,
                ["water_bottle_refill_time"] = waterBottleRefillTime
            };

            if (driedOutEvent != null)
                json["dried_out_event"] = driedOutEvent.ToComponentJSON();
            if (recoverEvent != null)
                json["recover_after_dried_out_event"] = recoverEvent.ToComponentJSON();
            if (stoppedDryingEvent != null)
                json["stopped_drying_out_event"] = stoppedDryingEvent.ToComponentJSON();

            return json;
        }
    }
    public class ComponentEconomyTradeTable : EntityComponent
    {
        public string displayName; // optional

        public bool convertTradesEconomy;
        public int heroDemandDiscount;
        public bool newScreen;
        public bool persistTrades;
        public bool showTradeScreen;
        public LootTable tradeTable;
        public bool legacyPriceFormula;

        public int curedDiscountMin;
        public int curedDiscountMax;

        public int maxCuredDiscountLow;
        public int maxCuredDiscountHigh;

        public int minNearbyCuredDiscount;
        public int maxNearbyCuredDiscount;

        public override string GetIdentifier() =>
            "minecraft:economy_trade_table";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["convert_trades_economy"] = convertTradesEconomy,
                ["cured_discount"] = new JArray(new[] { curedDiscountMin, curedDiscountMax }),
                ["hero_demand_discount"] = heroDemandDiscount,
                ["max_cured_discount"] = new JArray(new[] { maxCuredDiscountLow, maxCuredDiscountHigh }),
                ["max_nearby_cured_discount"] = maxNearbyCuredDiscount,
                ["nearby_cured_discount"] = minNearbyCuredDiscount,
                ["new_screen"] = newScreen,
                ["persistTrades"] = persistTrades,
                ["show_trade_screen"] = showTradeScreen,
                ["table"] = tradeTable.ResourcePath,
                ["use_legacy_price_formula"] = legacyPriceFormula
            };

            if (displayName != null)
                json["display_name"] = displayName;

            return json;
        }
    }
    public class ComponentEntitySensor : EntityComponent
    {
        public FilterCollection filters;
        public bool requireAllToPassFilter;
        public EntityEventHandler eventToFire;
        public bool relativeRange;

        public float sensorRange;
        public int minEntities;
        public int maxEntities;

        public override string GetIdentifier() =>
            "minecraft:entity_sensor";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["event"] = eventToFire.ToComponentJSON(),
                ["maximum_count"] = maxEntities,
                ["minimum_count"] = minEntities,
                ["relative_range"] = relativeRange,
                ["require_all"] = requireAllToPassFilter,
                ["sensor_range"] = sensorRange
            };

            if (filters != null)
                json["event_filters"] = filters.ToJSON();

            return json;
        }
    }
    public class ComponentEnvironmentSensor : EntityComponent
    {
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
                JObject json = new JObject()
                {
                    ["event"] = eventToFire.eventID,
                    ["target"] = eventToFire.target.ToString()
                };

                if (filters != null)
                    json["filters"] = filters.ToJSON();

                return json;
            }
        }

        public Trigger[] triggers;
        public override string GetIdentifier() =>
            "minecraft:environment_sensor";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["triggers"] = new JArray(triggers.Select(t => t.ToJSON()))
            };
        }
    }
    public class ComponentEquipItem : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:equip_item";
        public override JObject _GetValue()
        {
            return new JObject();
        }
    }
    public class ComponentEquippable : EntityComponent
    {
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
                JObject json = new JObject()
                {
                    ["slot"] = slotNumber
                };

                if (acceptedItems != null)
                    json["accepted_items"] = new JArray(acceptedItems);
                if (interactText != null)
                    json["interact_text"] = interactText;
                if (item != null)
                    json["item"] = item;
                if (onEquipEvent != null)
                    json["on_equip"] = onEquipEvent.ToComponentJSON();
                if (onEquipEvent != null)
                    json["on_unequip"] = onUnequipEvent.ToComponentJSON();

                return json;
            }
        }

        public EquippableSlot[] slots;
        public override string GetIdentifier() =>
            "minecraft:equippable";
        public override JObject _GetValue()
        {
            JObject json = new JObject();
            if (slots != null)
                json["slots"] = new JArray(slots.Select(slot => slot.ToJSON()));
            return json;
        }
    }
    public class ComponentExperienceReward : EntityComponent
    {
        public MolangValue experienceOnBred;
        public MolangValue experienceOnDeath;

        public override string GetIdentifier() =>
            "minecraft:experience_reward";
        public override JObject _GetValue()
        {
            JObject json = new JObject();
            if (experienceOnBred != null)
                json["on_bred"] = experienceOnBred.ToJSON();
            if (experienceOnDeath != null)
                json["on_death"] = experienceOnDeath.ToJSON();
            return json;
        }
    }
    public class ComponentExplode : EntityComponent
    {
        public bool breaksBlocks;
        public bool causesFire;
        public float maxBlockResistance;
        public int power;

        public bool followMobGriefingRule;

        /// <summary>
        /// Set a constant fuse rather than a random range.
        /// </summary>
        public float ConstantFuse
        {
            set
            {
                fuseMin = value;
                fuseMax = value;
            }
        }
        public float fuseMin;
        public float fuseMax;
        public bool fuseLit;

        public override string GetIdentifier() =>
            "minecraft:explode";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["breaks_blocks"] = breaksBlocks,
                ["causes_fire"] = causesFire,
                ["destroy_affected_by_griefing"] = followMobGriefingRule,
                ["fire_affected_by_griefing"] = followMobGriefingRule,
                ["fuse_lit"] = fuseLit,
                ["max_resistance"] = maxBlockResistance,
                ["power"] = power
            };

            if (fuseMin == fuseMax)
                json["fuse_length"] = fuseMin;
            else
                json["fuse_length"] = new JArray(new[] { fuseMin, fuseMax });

            return json;
        }
    }
    public class ComponentFlocking : EntityComponent
    {
        public float blockDistance;
        public float blockWeight;
        public float breachInfluence;
        public float goalWeight;
        public bool useCenterOfMass;

        public float cohesionThreshold;
        public float innerCohesionThreshold;
        public float cohesionWeight;

        public int lowFlockLimit;
        public int highFlockLimit;

        public bool inWater;
        public float influenceRadius;
        public float lonerChance; // 0.0 - 1.0
        public bool matchVariants; // racist bool

        public float minHeight;
        public float maxHeight;

        public float separationThreshold;
        public float separationWeight;

        public override string GetIdentifier() =>
            "minecraft:flocking";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["block_distance"] = blockDistance,
                ["block_weight"] = blockWeight,
                ["breach_influence"] = breachInfluence,
                ["cohesion_threshold"] = cohesionThreshold,
                ["goal_weight"] = goalWeight,
                ["high_flock_limit"] = highFlockLimit,
                ["in_water"] = inWater,
                ["influence_radius"] = influenceRadius,
                ["inner_cohesion_threshold"] = innerCohesionThreshold,
                ["loner_chance"] = lonerChance,
                ["low_flock_limit"] = lowFlockLimit,
                ["match_variants"] = matchVariants,
                ["max_height"] = maxHeight,
                ["min_height"] = minHeight,
                ["separation_threshold"] = separationThreshold,
                ["separation_weight"] = separationWeight,
                ["use_center_of_mass"] = useCenterOfMass
            };
        }
    }
    public class ComponentGenetics : EntityComponent
    {
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
                return new JObject()
                {
                    ["range_max"] = max,
                    ["range_min"] = min
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
                JObject json = new JObject()
                {
                    ["mutation_rate"] = mutationRate,
                    ["both_allele"] = bothAllele,
                    ["either_allele"] = eitherAllele,
                    ["hidden_allele"] = hiddenAllele,
                    ["main_allele"] = mainAllele
                };

                if (geneBirthEvent != null)
                    json["birth_event"] = geneBirthEvent.ToComponentJSON();

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
                JObject json = new JObject()
                {
                    ["allele_range"] = alleleRange
                };

                if (name != null)
                    json["name"] = name;
                if (variants != null)
                    json["genetic_variants"] = new JArray(variants.Select(variant => variant.ToJSON()));

                return json;
            }
        }

        public Gene[] genes;
        public float mutationRate; // 0.0 - 1.0; default is 0.03125f
        public override string GetIdentifier() =>
            "minecraft:genetics";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["genes"] = new JArray(genes.Select(gene => gene.ToJSON())),
                ["mutation_rate"] = mutationRate
            };
        }
    }
    public class ComponentGiveable : EntityComponent
    {
        public float? cooldown;
        public string[] acceptedItems;
        public EntityEventHandler onGiveEvent;

        public override string GetIdentifier() =>
            "minecraft:giveable";
        public override JObject _GetValue()
        {
            JObject json = new JObject();

            if (cooldown.HasValue)
                json["cooldown"] = cooldown.Value;
            if (acceptedItems != null)
                json["items"] = new JArray(acceptedItems);
            if (onGiveEvent != null)
                json["on_give"] = onGiveEvent.ToComponentJSON();

            return json;
        }
    }
    public class ComponentGroupSize : EntityComponent
    {
        public FilterCollection groupTest;
        public float radius;

        public override string GetIdentifier() =>
            "minecraft:group_size";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["radius"] = radius,
                ["filters"] = groupTest.ToJSON()
            };
        }
    }
    public class ComponentGrowsCrop : EntityComponent
    {
        public float chancePerTick; // 0.0 - 1.0
        public int charges;

        public override string GetIdentifier() =>
            "minecraft:grows_crop";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["chance"] = chancePerTick,
                ["charges"] = charges
            };
        }
    }
    public class ComponentHealable : EntityComponent
    {
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
                return new JObject()
                {
                    ["heal_amount"] = healAmount,
                    ["item"] = item
                };
            }
        }

        public FilterCollection filters;
        public bool forceUse;
        public HealItem[] items;

        public override string GetIdentifier() =>
            "minecraft:healable";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["force_use"] = forceUse
            };

            if (filters != null)
                json["filters"] = filters.ToJSON();
            if (items != null)
                json["items"] = new JArray(items.Select(item => item.ToJSON()));

            return json;
        }
    }
    public class ComponentHome : EntityComponent
    {
        // used by bees to mark their beehives

        public string[] homeBlocks;
        public int restrictionRadius;

        public override string GetIdentifier() =>
            "minecraft:home";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["restriction_radius"] = restrictionRadius
            };

            if (homeBlocks != null)
                json["home_block_list"] = new JArray(homeBlocks);

            return json;
        }
    }
    public class ComponentHurtOnCondition : EntityComponent
    {
        public struct DamageCondition
        {
            public FilterCollection filters;
            public Commands.DamageCause cause;
            public int damagePerTick;

            public JObject ToJSON()
            {
                JObject json = new JObject()
                {
                    ["cause"] = cause.ToString(),
                    ["damage_per_tick"] = damagePerTick
                };

                if (filters != null)
                    json["filters"] = filters.ToJSON();

                return json;
            }
            public static DamageCondition BurnInLava()
            {
                return new DamageCondition()
                {
                    filters = new FilterCollection()
                    {
                        new FilterInLava() { checkValue = true, subject = EventSubject.self }
                    },
                    cause = DamageCause.drowning,
                    damagePerTick = 4
                };
            }
        }

        public DamageCondition[] conditions;
        public override string GetIdentifier() =>
            "minecraft:hurt_on_condition";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["damage_condition"] = new JArray(conditions.Select(cond => cond.ToJSON()))
            };
        }
    }
    public class ComponentInsideBlockNotifier : EntityComponent
    {
        public struct InsideBlock
        {
            public string blockName;
            public JProperty[] blockStates;

            public EntityEventHandler enteredBlockEvent;
            public EntityEventHandler exitedBlockEvent;

            public JObject BlockStatesAsJSON()
            {
                if (blockStates == null)
                    return null;
                JObject obj = new JObject();
                foreach (JProperty state in blockStates)
                    obj[state.Name] = state.Value;
                return obj;
            }
            public JObject ToJSON()
            {
                JObject json = new JObject();

                JObject block = new JObject();
                block["name"] = blockName;
                if (blockStates != null)
                    block["states"] = BlockStatesAsJSON();
                json["block"] = block;

                if (enteredBlockEvent != null)
                    json["entered_block_event"] = enteredBlockEvent.ToComponentJSON();
                if (exitedBlockEvent != null)
                    json["exited_block_event"] = exitedBlockEvent.ToComponentJSON();

                return json;
            }
        }

        public InsideBlock[] blocks;
        public override string GetIdentifier() =>
            "minecraft:inside_block_notifier";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["block_list"] = new JArray(blocks.Select(block => block.ToJSON()))
            };
        }
    }
    public class ComponentInsomnia : EntityComponent
    {
        public float daysUntilExperiencesInsomnia;

        public override string GetIdentifier() =>
            "minecraft:insomnia";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["days_until_insomnia"] = daysUntilExperiencesInsomnia
            };
        }
    }
    public class ComponentInstantDespawn : EntityComponent
    {
        public bool removeChildEntities;

        public override string GetIdentifier() =>
            "minecraft:instant_despawn";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["remove_child_entities"] = removeChildEntities
            };
        }
    }
    public class ComponentInteract : EntityComponent
    {
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
                return new JObject()
                {
                    ["particle_offset_towards_interactor"] = offsetTowardsInteractor,
                    ["particle_type"] = particleType,
                    ["particle_y_offset"] = yOffset
                };
            }
        }

        public LootTable addItems;
        public LootTable spawnItems;
        public string interactText;
        public EntityEventHandler onInteractEvent;
        public ParticleOnStart? particles;
        public string[] soundsToPlay;
        public string[] entitiesToSpawn;
        public string transformToItem;

        public float cooldown;
        public float cooldownAfterBeingAttacked;
        public int healthAmount;
        public int dealItemDamage;
        public bool swingAnimation;
        public bool consumeItem;

        public override string GetIdentifier() =>
            "minecraft:interact";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["cooldown"] = cooldown,
                ["cooldown_after_being_attacked"] = cooldownAfterBeingAttacked,
                ["health_amount"] = healthAmount,
                ["hurt_item"] = dealItemDamage,
                ["swing"] = swingAnimation,
                ["use_item"] = consumeItem
            };

            if (addItems != null)
                json["add_items"] = new JObject() { ["table"] = addItems.ResourcePath };
            if (interactText != null)
                json["interact_text"] = interactText;
            if (onInteractEvent != null)
                json["on_interact"] = onInteractEvent.ToComponentJSON();
            if (particles.HasValue)
                json["particle_on_start"] = particles.Value.ToJSON();
            if (soundsToPlay != null)
                json["play_sounds"] = new JArray(soundsToPlay);
            if (entitiesToSpawn != null)
                json["spawn_entities"] = new JArray(entitiesToSpawn);
            if (spawnItems != null)
                json["spawn_items"] = new JArray(spawnItems);
            if (transformToItem != null)
                json["transform_to_item"] = transformToItem;

            return json;
        }
    }
    public class ComponentInventory : EntityComponent
    {
        public enum ContainerType
        {
            horse, minecart_chest, minecart_hopper, inventory, container, hopper
        }

        public int additionalSlotsPerStrength;
        public bool canBeSiphonedFrom;
        public ContainerType containerType;
        public int inventorySize;
        public bool dropOnDeath;
        public bool restrictToOwner;

        public override string GetIdentifier() =>
            "minecraft:inventory";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["additional_slots_per_strength"] = additionalSlotsPerStrength,
                ["can_be_siphoned_from"] = canBeSiphonedFrom,
                ["container_type"] = containerType.ToString(),
                ["inventory_size"] = inventorySize,
                ["private"] = !dropOnDeath,
                ["restrict_to_owner"] = restrictToOwner
            };
        }
    }
    public class ComponentItemHopper : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:item_hopper";
        public override JObject _GetValue()
        {
            return new JObject();
        }
    }
    public class ComponentJumpDynamic : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:jump.dynamic";
        public override JObject _GetValue()
        {
            return new JObject();
        }
    }
    public class ComponentJumpStatic : EntityComponent
    {
        public float jumpPower;

        public override string GetIdentifier() =>
            "minecraft:jump.static";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["jump_power"] = jumpPower
            };
        }
    }
    public class ComponentLeashable : EntityComponent
    {
        public bool canBeStolen;
        public float softDistance; // springs back
        public float hardDistance; // siffens
        public float maxDistance; // breaks

        public EntityEventHandler
            onLeashEvent,
            onUnleashEnvent;

        public override string GetIdentifier() =>
            "minecraft:leashable";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["can_be_stolen"] = canBeStolen,
                ["soft_distance"] = softDistance,
                ["hard_distance"] = hardDistance,
                ["max_distance"] = maxDistance
            };

            if (onLeashEvent != null)
                json["on_leash"] = onLeashEvent.ToComponentJSON();
            if (onUnleashEnvent != null)
                json["on_unleash"] = onUnleashEnvent.ToComponentJSON();

            return json;
        }
    }
    public class ComponentLookAt : EntityComponent
    {
        public bool allowInvulnerable;
        public FilterCollection filters;
        public EntityEventHandler lookedAtEvent;
        public float searchRadius;
        public bool attackTargets;

        public float lookCooldownMin;
        public float lookCooldownMax;

        public override string GetIdentifier() =>
            "minecraft:lookat";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["allow_invulnerable"] = allowInvulnerable,
                ["look_cooldown"] = new JArray(new[] { lookCooldownMin, lookCooldownMax }),
                ["search_radius"] = searchRadius,
                ["set_target"] = attackTargets
            };

            if (filters != null)
                json["filters"] = filters.ToJSON();
            if (lookedAtEvent != null)
                json["look_event"] = lookedAtEvent.eventID; // only accepts string :(

            return json;
        }
    }
    public class ComponentManagedWanderingTrader : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:managed_wandering_trader";
        public override JObject _GetValue()
        {
            return new JObject();
        }
    }
    public class ComponentMobEffect : EntityComponent
    {
        public FilterCollection entityFilter;
        public float effectRange;
        public int effectTime;
        public Commands.PotionEffect effect;

        public override string GetIdentifier() =>
            "minecraft:mob_effect";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["effect_range"] = effectRange,
                ["effect_time"] = effectTime,
                ["mob_effect"] = effect.ToString()
            };

            if (entityFilter != null)
                json["entity_filter"] = entityFilter.ToJSON();

            return json;
        }
    }
    public class ComponentMovementAmphibious : EntityComponent
    {
        public float maxTurn = 30; // degrees

        public override string GetIdentifier() =>
            "minecraft:movement.amphibious";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["max_turn"] = maxTurn
            };
        }
    }
    public class ComponentMovementBasic : EntityComponent
    {
        public float maxTurn = 30; // degrees

        public override string GetIdentifier() =>
            "minecraft:movement.basic";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["max_turn"] = maxTurn
            };
        }
    }
    public class ComponentMovementFly : EntityComponent
    {
        public float maxTurn = 30; // degrees

        public override string GetIdentifier() =>
            "minecraft:movement.fly";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["max_turn"] = maxTurn
            };
        }
    }
    public class ComponentMovementGeneric : EntityComponent
    {
        public float maxTurn = 30; // degrees

        public override string GetIdentifier() =>
            "minecraft:movement.generic";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["max_turn"] = maxTurn
            };
        }
    }
    public class ComponentMovementHover : EntityComponent
    {
        public float maxTurn = 30; // degrees

        public override string GetIdentifier() =>
            "minecraft:movement.hover";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["max_turn"] = maxTurn
            };
        }
    }
    public class ComponentMovementJump : EntityComponent
    {
        public float jumpDelayMin;
        public float jumpDelayMax;
        public float maxTurn = 30; // degrees

        public override string GetIdentifier() =>
            "minecraft:movement.jump";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["jump_delay"] = new JArray(new[] { jumpDelayMin, jumpDelayMax }),
                ["max_turn"] = maxTurn
            };
        }
    }
    public class ComponentMovementSkip : EntityComponent
    {
        public float maxTurn = 30; // degrees

        public override string GetIdentifier() =>
            "minecraft:movement.skip";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["max_turn"] = maxTurn
            };
        }
    }
    public class ComponentMovementSway : EntityComponent
    {
        public float maxTurn = 30; // degrees
        public float swayAmplitude = 0.05f;
        public float swayFrequency = 0.5f;

        public override string GetIdentifier() =>
            "minecraft:movement.sway";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["max_turn"] = maxTurn,
                ["sway_amplitude"] = swayAmplitude,
                ["sway_frequency"] = swayFrequency
            };
        }
    }
    public class ComponentNameable : EntityComponent
    {
        /// <summary>
        /// Fires an event when this entity is named something 
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
                if (specialNames.Length == 1)
                {
                    return new JObject()
                    {
                        ["name_filter"] = specialNames[0],
                        ["on_named"] = onNamedEvent.ToComponentJSON()
                    };
                }

                return new JObject()
                {
                    ["name_filter"] = new JArray(specialNames),
                    ["on_named"] = onNamedEvent.ToComponentJSON()
                };
            }
        }

        public bool allowNametags;
        public bool alwaysShowName;
        public EntityEventHandler onNamed;
        public SpecialNameAction[] specialNames;

        public override string GetIdentifier() =>
            "minecraft:nameable";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["allow_name_tag_renaming"] = allowNametags,
                ["always_show"] = alwaysShowName
            };

            if (onNamed != null)
                json["default_trigger"] = onNamed.ToComponentJSON();
            if (specialNames != null)
                json["name_actions"] = new JArray(specialNames.Select(name => name.ToJSON()));

            return json;
        }
    }
    public class ComponentNavigationClimb : EntityComponent
    {
        public string[] blocksToAvoid;
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

        public override string GetIdentifier() =>
            "minecraft:navigation.climb";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["avoid_damage_blocks"] = avoidDamageBlocks,
                ["avoid_portals"] = avoidPortals,
                ["avoid_sun"] = avoidSun,
                ["avoid_water"] = avoidWater,
                ["can_breach"] = canBreachWater,
                ["can_break_doors"] = canBreakDoors,
                ["can_jump"] = canJump,
                ["can_open_doors"] = canOpenDoors,
                ["can_open_iron_doors"] = canOpenIronDoors,
                ["can_pass_doors"] = canPassDoors,
                ["can_path_from_air"] = canPathFromAir,
                ["can_path_over_lava"] = canPathOverLava,
                ["can_path_over_water"] = canPathOverWater,
                ["can_sink"] = canSink,
                ["can_swim"] = canSwim,
                ["can_walk"] = canWalk,
                ["can_walk_in_lava"] = canWalkInLava,
                ["is_amphibious"] = isAmphibious
            };

            if (blocksToAvoid != null)
                json["blocks_to_avoid"] = new JArray(blocksToAvoid);

            return json;
        }
    }
    public class ComponentNavigationFloat : EntityComponent
    {
        public string[] blocksToAvoid;
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

        public override string GetIdentifier() =>
            "minecraft:navigation.float";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["avoid_damage_blocks"] = avoidDamageBlocks,
                ["avoid_portals"] = avoidPortals,
                ["avoid_sun"] = avoidSun,
                ["avoid_water"] = avoidWater,
                ["can_breach"] = canBreachWater,
                ["can_break_doors"] = canBreakDoors,
                ["can_jump"] = canJump,
                ["can_open_doors"] = canOpenDoors,
                ["can_open_iron_doors"] = canOpenIronDoors,
                ["can_pass_doors"] = canPassDoors,
                ["can_path_from_air"] = canPathFromAir,
                ["can_path_over_lava"] = canPathOverLava,
                ["can_path_over_water"] = canPathOverWater,
                ["can_sink"] = canSink,
                ["can_swim"] = canSwim,
                ["can_walk"] = canWalk,
                ["can_walk_in_lava"] = canWalkInLava,
                ["is_amphibious"] = isAmphibious
            };

            if (blocksToAvoid != null)
                json["blocks_to_avoid"] = new JArray(blocksToAvoid);

            return json;
        }
    }
    public class ComponentNavigationFly : EntityComponent
    {
        public string[] blocksToAvoid;
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

        public override string GetIdentifier() =>
            "minecraft:navigation.fly";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["avoid_damage_blocks"] = avoidDamageBlocks,
                ["avoid_portals"] = avoidPortals,
                ["avoid_sun"] = avoidSun,
                ["avoid_water"] = avoidWater,
                ["can_breach"] = canBreachWater,
                ["can_break_doors"] = canBreakDoors,
                ["can_jump"] = canJump,
                ["can_open_doors"] = canOpenDoors,
                ["can_open_iron_doors"] = canOpenIronDoors,
                ["can_pass_doors"] = canPassDoors,
                ["can_path_from_air"] = canPathFromAir,
                ["can_path_over_lava"] = canPathOverLava,
                ["can_path_over_water"] = canPathOverWater,
                ["can_sink"] = canSink,
                ["can_swim"] = canSwim,
                ["can_walk"] = canWalk,
                ["can_walk_in_lava"] = canWalkInLava,
                ["is_amphibious"] = isAmphibious
            };

            if (blocksToAvoid != null)
                json["blocks_to_avoid"] = new JArray(blocksToAvoid);

            return json;
        }
    }
    public class ComponentNavigationGeneric : EntityComponent
    {
        public string[] blocksToAvoid;
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

        public override string GetIdentifier() =>
            "minecraft:navigation.generic";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["avoid_damage_blocks"] = avoidDamageBlocks,
                ["avoid_portals"] = avoidPortals,
                ["avoid_sun"] = avoidSun,
                ["avoid_water"] = avoidWater,
                ["can_breach"] = canBreachWater,
                ["can_break_doors"] = canBreakDoors,
                ["can_jump"] = canJump,
                ["can_open_doors"] = canOpenDoors,
                ["can_open_iron_doors"] = canOpenIronDoors,
                ["can_pass_doors"] = canPassDoors,
                ["can_path_from_air"] = canPathFromAir,
                ["can_path_over_lava"] = canPathOverLava,
                ["can_path_over_water"] = canPathOverWater,
                ["can_sink"] = canSink,
                ["can_swim"] = canSwim,
                ["can_walk"] = canWalk,
                ["can_walk_in_lava"] = canWalkInLava,
                ["is_amphibious"] = isAmphibious
            };

            if (blocksToAvoid != null)
                json["blocks_to_avoid"] = new JArray(blocksToAvoid);

            return json;
        }
    }
    public class ComponentNavigationHover : EntityComponent
    {
        public string[] blocksToAvoid;
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

        public override string GetIdentifier() =>
            "minecraft:navigation.hover";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["avoid_damage_blocks"] = avoidDamageBlocks,
                ["avoid_portals"] = avoidPortals,
                ["avoid_sun"] = avoidSun,
                ["avoid_water"] = avoidWater,
                ["can_breach"] = canBreachWater,
                ["can_break_doors"] = canBreakDoors,
                ["can_jump"] = canJump,
                ["can_open_doors"] = canOpenDoors,
                ["can_open_iron_doors"] = canOpenIronDoors,
                ["can_pass_doors"] = canPassDoors,
                ["can_path_from_air"] = canPathFromAir,
                ["can_path_over_lava"] = canPathOverLava,
                ["can_path_over_water"] = canPathOverWater,
                ["can_sink"] = canSink,
                ["can_swim"] = canSwim,
                ["can_walk"] = canWalk,
                ["can_walk_in_lava"] = canWalkInLava,
                ["is_amphibious"] = isAmphibious
            };

            if (blocksToAvoid != null)
                json["blocks_to_avoid"] = new JArray(blocksToAvoid);

            return json;
        }
    }
    public class ComponentNavigationSwim : EntityComponent
    {
        public string[] blocksToAvoid;
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

        public override string GetIdentifier() =>
            "minecraft:navigation.swim";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["avoid_damage_blocks"] = avoidDamageBlocks,
                ["avoid_portals"] = avoidPortals,
                ["avoid_sun"] = avoidSun,
                ["avoid_water"] = avoidWater,
                ["can_breach"] = canBreachWater,
                ["can_break_doors"] = canBreakDoors,
                ["can_jump"] = canJump,
                ["can_open_doors"] = canOpenDoors,
                ["can_open_iron_doors"] = canOpenIronDoors,
                ["can_pass_doors"] = canPassDoors,
                ["can_path_from_air"] = canPathFromAir,
                ["can_path_over_lava"] = canPathOverLava,
                ["can_path_over_water"] = canPathOverWater,
                ["can_sink"] = canSink,
                ["can_swim"] = canSwim,
                ["can_walk"] = canWalk,
                ["can_walk_in_lava"] = canWalkInLava,
                ["is_amphibious"] = isAmphibious
            };

            if (blocksToAvoid != null)
                json["blocks_to_avoid"] = new JArray(blocksToAvoid);

            return json;
        }
    }
    public class ComponentNavigationWalk : EntityComponent
    {
        public string[] blocksToAvoid;
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

        public override string GetIdentifier() =>
            "minecraft:navigation.walk";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["avoid_damage_blocks"] = avoidDamageBlocks,
                ["avoid_portals"] = avoidPortals,
                ["avoid_sun"] = avoidSun,
                ["avoid_water"] = avoidWater,
                ["can_breach"] = canBreachWater,
                ["can_break_doors"] = canBreakDoors,
                ["can_jump"] = canJump,
                ["can_open_doors"] = canOpenDoors,
                ["can_open_iron_doors"] = canOpenIronDoors,
                ["can_pass_doors"] = canPassDoors,
                ["can_path_from_air"] = canPathFromAir,
                ["can_path_over_lava"] = canPathOverLava,
                ["can_path_over_water"] = canPathOverWater,
                ["can_sink"] = canSink,
                ["can_swim"] = canSwim,
                ["can_walk"] = canWalk,
                ["can_walk_in_lava"] = canWalkInLava,
                ["is_amphibious"] = isAmphibious
            };

            if (blocksToAvoid != null)
                json["blocks_to_avoid"] = new JArray(blocksToAvoid);

            return json;
        }
    }
    public class ComponentOutOfControl : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:out_of_control";
        public override JObject _GetValue()
        {
            return new JObject();
        }
    }
    public class ComponentPeek : EntityComponent
    {
        // shulker peeking

        public EntityEventHandler stopPeekingEvent;
        public EntityEventHandler startPeekingEvent;
        public EntityEventHandler spottedEvent;

        public override string GetIdentifier() =>
            "minecraft:peek";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["on_close"] = stopPeekingEvent.ToComponentJSON(),
                ["on_open"] = startPeekingEvent.ToComponentJSON(),
                ["on_target_open"] = spottedEvent.ToComponentJSON()
            };
        }
    }
    public class ComponentPersistent : EntityComponent
    {
        public override string GetIdentifier() =>
            "minecraft:persistent";
        public override JObject _GetValue()
        {
            return new JObject();
        }
    }
    public class ComponentPhysics : EntityComponent
    {
        public bool collision;
        public bool gravity;

        public override string GetIdentifier() =>
            "minecraft:physics";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["has_collision"] = collision,
                ["has_gravity"] = gravity
            };
        }
    }
    public class ComponentPreferredPath : EntityComponent
    {
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
                return new JObject()
                {
                    ["cost"] = cost,
                    ["blocks"] = new JArray(blocks)
                };
            }
        }

        // used by villagers to walk on paths
        public float defaultBlockCost;
        public int jumpCost;
        public int maxSafeFall;
        public PriorityBlocks[] blocks;

        public override string GetIdentifier() =>
            "minecraft:prefered_path";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["default_block_cost"] = defaultBlockCost,
                ["jump_cost"] = jumpCost,
                ["max_fall_blocks"] = maxSafeFall
            };

            if (blocks != null)
                json["preferred_path_blocks"] = new JArray(blocks.Select(b => b.ToJSON()));

            return json;
        }
    }
    public class ComponentProjectile : EntityComponent
    {
        public FilterCollection immuneEntities;
        public string hitSound;
        public string shootSound;
        public string particle = "iconcrack";
        public Commands.PotionEffect? dealPotionEffect;

        public float angleOffset;
        public float gravity;
        public float inertia;
        public float liquidInertia;
        public float offsetX = 0;
        public float offsetY = 0;
        public float offsetZ = 0;
        public float dealFireTime;
        public float power;
        public float splashRange;
        public float uncertaintyBase;
        public float uncertaintyMultiplier;

        public bool followMobGriefing;
        public bool catchFire;
        public bool critParticle;
        public bool destroyOnHurt;
        public bool homing;
        public bool isDangerous;
        public bool doesKnockback;
        public bool strikesLightning;
        public bool pierces;
        public bool reflectOffHit;
        public bool randomizeDamage;
        public bool gotoTargetOfShooter;
        public bool shouldBounce;
        public bool isSplashPotion;

        public override string GetIdentifier() =>
            "minecraft:projectile";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["angle_offset"] = angleOffset,
                ["catch_fire"] = catchFire,
                ["crit_particle_on_hurt"] = critParticle,
                ["destroy_on_hurt"] = destroyOnHurt,
                ["fire_affected_by_griefing"] = followMobGriefing,
                ["gravity"] = gravity,
                ["homing"] = homing,
                ["inertia"] = inertia,
                ["is_dangerous"] = isDangerous,
                ["knockback"] = doesKnockback,
                ["lightning"] = strikesLightning,
                ["liquid_inertia"] = liquidInertia,
                ["multiple_targets"] = pierces,
                ["offset"] = new JArray(new[] { offsetX, offsetY, offsetZ }),
                ["on_fire_time"] = dealFireTime,
                ["particle"] = particle,
                ["potion_effect"] = dealPotionEffect.HasValue ? (int)dealPotionEffect.Value : -1,
                ["power"] = power,
                ["reflect_on_hurt"] = reflectOffHit,
                ["semi_random_diff_damage"] = randomizeDamage,
                ["shoot_target"] = gotoTargetOfShooter,
                ["should_bounce"] = shouldBounce,
                ["splash_potion"] = isSplashPotion,
                ["splash_range"] = splashRange,
                ["uncertainty_base"] = uncertaintyBase,
                ["uncertainty_multiplier"] = uncertaintyMultiplier
            };

            if (immuneEntities != null)
                json["filter"] = immuneEntities.ToJSON();
            if (hitSound != null)
                json["hit_sound"] = hitSound;
            if (shootSound != null)
                json["shoot_sound"] = shootSound;

            return json;
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
        // used only by the player to trigger a raid.

        public EntityEventHandler raidEvent;
        public override string GetIdentifier() =>
            "minecraft:raid_trigger";
        public override JObject _GetValue()
        {
            JObject json = new JObject();
            if (raidEvent != null)
                json["triggered_event"] = raidEvent.ToComponentJSON();
            return json;
        }
    }
    public class ComponentRailMovement : EntityComponent
    {
        public float maxSpeed;

        public override string GetIdentifier() =>
            "minecraft:rail_movement";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["max_speed"] = maxSpeed
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

        public override string GetIdentifier() =>
            "minecraft:rail_sensor";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["check_block_types"] = checkBlockTypes,
                ["eject_on_activate"] = ejectOnActivate,
                ["eject_on_deactivate"] = ejectOnDeactivate,
                ["tick_command_block_on_activate"] = tickCommandBlockOnActivate,
                ["tick_command_block_on_deactivate"] = tickCommandBlockOnDeactivate
            };

            if (onActivateEvent != null)
                json["on_activate"] = onActivateEvent.ToComponentJSON();
            if (onDeactivateEvent != null)
                json["on_deactivate"] = onDeactivateEvent.ToComponentJSON();

            return json;
        }
    }
    public class ComponentRavagerBlocked : EntityComponent
    {
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
                JObject json = new JObject()
                {
                    ["weight"] = weight
                };

                if (value != null)
                    json["value"] = value.ToComponentJSON();

                return json;
            }
        }

        public float knockbackStrength;
        public ReactionChoice[] choices;
        public override string GetIdentifier() =>
            "minecraft:ravager_blocked";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["knockback_strength"] = knockbackStrength
            };

            if (choices != null)
                json["reaction_choices"] = new JArray(choices.Select(choice => choice.ToJSON()));

            return json;
        }
    }
    public class ComponentRideable : EntityComponent
    {
        public struct Seat
        {
            public float? lockRiderRotation;
            public int? maxRiderCount; // omit for "seat_count"
            public int minRiderCount;
            public float posX, posY, posZ;
            public MolangValue rotateRiderBy;

            public JObject ToJSON()
            {
                JObject json = new JObject()
                {
                    ["min_rider_count"] = minRiderCount,
                    ["position"] = new JArray(new[] { posX, posY, posZ }),
                    ["rotate_rider_by"] = rotateRiderBy == null ? (JToken)0 : rotateRiderBy.ToJSON()
                };

                if (lockRiderRotation.HasValue)
                    json["lock_rider_rotation"] = lockRiderRotation.Value;

                if (maxRiderCount.HasValue)
                    json["max_rider_count"] = maxRiderCount.Value;
                else
                    json["max_rider_count"] = "seat_count";

                return json;
            }
        }

        public string[] allowedFamilyTypes;
        public string interactText;

        public int controllingSeat;
        public bool crouchingSkipInteract;
        public bool pullInEntities;
        public bool riderCanInteract;

        public int seatCount;
        public Seat[] seats;

        public override string GetIdentifier() =>
            "minecraft:rideable";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["controlling_seat"] = controllingSeat,
                ["crouching_skip_interact"] = crouchingSkipInteract,
                ["priority"] = 0,
                ["pull_in_entities"] = pullInEntities,
                ["rider_can_interact"] = riderCanInteract,
                ["seat_count"] = seatCount
            };

            if (allowedFamilyTypes != null)
                json["family_types"] = new JArray(allowedFamilyTypes);
            if (interactText != null)
                json["interact_text"] = interactText;
            if (seats != null)
                json["seats"] = new JArray(seats.Select(seat => seat.ToJSON()));

            return json;
        }
    }
    public class ComponentScaffoldingClimber : EntityComponent
    {
        // deprecated
        public override string GetIdentifier() =>
            "minecraft:scaffolding_climber";
        public override JObject _GetValue()
        {
            return new JObject();
        }
    }
    public class ComponentScaleByAge : EntityComponent
    {
        public float startScale;
        public float endScale;
        public override string GetIdentifier() =>
            "minecraft:scale_by_age";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["start_scale"] = startScale,
                ["end_scale"] = endScale
            };
        }
    }
    public class ComponentScheduler : EntityComponent
    {
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
                return new JObject()
                {
                    ["event"] = call.ToComponentJSON(),
                    ["filters"] = new JArray(tests.Select(test => test.ToJSON()))
                };
            }
        }

        public ScheduledEvent[] events;

        // undocumented??
        public int? minDelaySeconds;
        public int? maxDelaySeconds;

        public override string GetIdentifier() =>
            "minecraft:scheduler";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["scheduled_events"] = new JArray(events.Select(e => e.ToJSON()))
            };

            if (minDelaySeconds.HasValue)
                json["min_delay_secs"] = minDelaySeconds.Value;
            if (maxDelaySeconds.HasValue)
                json["max_delay_secs"] = maxDelaySeconds.Value;

            return json;
        }
    }
    public class ComponentShareables : EntityComponent
    {
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
                JObject json = new JObject()
                {
                    ["admire"] = admire,
                    ["barter"] = barter,
                    ["consume_item"] = consumeItem,
                    ["max_amount"] = maxAmount,
                    ["pickup_limit"] = pickupLimit,
                    ["priority"] = priority,
                    ["stored_in_inventory"] = storeInInventory,
                    ["surplus_amount"] = surplusAmount,
                    ["want_amount"] = wantAmount
                };

                if (craftInfo != null)
                    json["craft_into"] = craftInfo;
                if (item != null)
                    json["item"] = item;

                return json;
            }
        }

        public bool allItems = false;
        public int allItemsMaxAmount = -1;
        public int allItemsSurplusAmount = -1;
        public int allItemsWantAmount = -1;

        public Shareable[] items;

        public override string GetIdentifier() =>
            "minecraft:shareables";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["all_items"] = allItems,
                ["all_items_max_amount"] = allItemsMaxAmount,
                ["all_items_surplus_amount"] = allItemsSurplusAmount,
                ["all_items_want_amount"] = allItemsWantAmount
            };

            if (items != null)
                json["items"] = new JArray(items.Select(item => item.ToJSON()));

            return json;
        }
    }
    public class ComponentShooter : EntityComponent
    {
        public Commands.PotionEffect? passEffectToProjectile;
        public string projectileEntity;

        public override string GetIdentifier() =>
            "minecraft:shooter";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["aux_val"] = passEffectToProjectile.HasValue ? (int)passEffectToProjectile.Value : -1,
                ["def"] = projectileEntity
            };
        }
    }
    public class ComponentSittable : EntityComponent
    {
        // sittable like a cat or dog

        public EntityEventHandler sitEvent;
        public EntityEventHandler standEvent;

        public override string GetIdentifier() =>
            "minecraft:sittable";
        public override JObject _GetValue()
        {
            JObject json = new JObject();

            if (sitEvent != null)
                json["sit_event"] = sitEvent.ToComponentJSON();
            if (standEvent != null)
                json["stand_event"] = standEvent.ToComponentJSON();

            return json;
        }
    }
    public class ComponentSpawnEntity : EntityComponent
    {
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
                JObject json = new JObject()
                {
                    ["min_wait_time"] = minWaitTime,
                    ["max_wait_time"] = maxWaitTime,
                    ["num_to_spawn"] = spawnCount,
                    ["should_leash"] = shouldLeash,
                    ["single_use"] = singleUse
                };

                if(spawnEntity != null)
                {
                    json["spawn_entity"] = spawnEntity;
                    if (spawnMethod != null)
                        json["spawn_method"] = spawnMethod;
                }
                if (spawnItem != null)
                    json["spawn_item"] = spawnItem;
                if (spawnSound != null)
                    json["spawn_sound"] = spawnSound;
                if (requirements != null)
                    json["filters"] = requirements.ToJSON();
                if (spawnEvent != null)
                    json["spawn_event"] = spawnEvent.ToComponentJSON();

                return json;
            }
        }

        public SpawnEntity[] entities;
        public override string GetIdentifier() =>
            "minecraft:spawn_entity";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["entities"] = new JArray(entities.Select(entity => entity.ToJSON()))
            };
        }
    }
    public class ComponentTameable : EntityComponent
    {
        public float probability; // 0.0 - 1.0
        public string[] tameItems;
        public EntityEventHandler tameEvent;

        public override string GetIdentifier() =>
            "minecraft:tameable";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["probability"] = probability
            };

            if (tameEvent != null)
                json["tame_event"] = tameEvent.ToComponentJSON();
            if (tameItems != null)
                json["tame_items"] = new JArray(tameItems);

            return json;
        }
    }
    public class ComponentTameMount : EntityComponent
    {
        public struct AutoRejectItem
        {
            public string item;

            public AutoRejectItem(string item)
            {
                this.item = item;
            }
            public JObject ToJSON()
            {
                return new JObject()
                {
                    ["item"] = item
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
                return new JObject()
                {
                    ["item"] = item,
                    ["tempter_mod"] = temperMod
                };
            }
        }

        public int attemptTemperMod;
        public int minTemper, maxTemper;

        public AutoRejectItem[] rejectItems;
        public FeedItem[] feedItems;
        public EntityEventHandler tameEvent;
        public string feedText;
        public string rideText;

        public override string GetIdentifier() =>
            "minecraft:tamemount";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["attempt_temper_mod"] = attemptTemperMod,
                ["min_temper"] = minTemper,
                ["max_temper"] = maxTemper
            };

            if (rejectItems != null)
                json["auto_reject_items"] = new JArray(rejectItems.Select(item => item.ToJSON()));
            if (feedItems != null)
                json["feed_items"] = new JArray(feedItems.Select(item => item.ToJSON()));
            if (tameEvent != null)
                json["tame_event"] = tameEvent.ToComponentJSON();
            if (feedText != null)
                json["feed_text"] = feedText;
            if (rideText != null)
                json["ride_text"] = rideText;

            return json;
        }
    }
    public class ComponentTargetNearbySensor : EntityComponent
    {
        public float insideRange;
        public float outsideRange;
        public bool mustSee;

        public EntityEventHandler onInsideRangeEvent;
        public EntityEventHandler onOutsideRangeEvent;
        public EntityEventHandler onVisionLostInsideRangeEvent;

        public override string GetIdentifier() =>
            "minecraft:target_nearby_sensor";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["inside_range"] = insideRange,
                ["outside_range"] = outsideRange,
                ["must_see"] = mustSee
            };

            if (onInsideRangeEvent != null)
                json["on_inside_range"] = onInsideRangeEvent.ToComponentJSON();
            if (onOutsideRangeEvent != null)
                json["on_outside_range"] = onOutsideRangeEvent.ToComponentJSON();
            if (onVisionLostInsideRangeEvent != null)
                json["on_vision_lost_inside_range"] = onVisionLostInsideRangeEvent.ToComponentJSON();

            return json;
        }
    }
    public class ComponentTeleport : EntityComponent
    {
        public float darkTeleportChance;
        public float lightTeleportChance;
        public float minRandomTeleportTime;
        public float maxRandomTeleportTime;
        public int cubeX, cubeY, cubeZ;
        public bool randomTeleports;

        public float targetDistance;
        public float targetTeleportChance;

        public override string GetIdentifier() =>
            "minecraft:teleport";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["dark_teleport_chance"] = darkTeleportChance,
                ["light_teleport_chance"] = lightTeleportChance,
                ["min_random_teleport_time"] = minRandomTeleportTime,
                ["max_random_teleport_time"] = maxRandomTeleportTime,
                ["random_teleport_cube"] = new JArray(new[] { cubeX, cubeY, cubeZ }),
                ["random_teleports"] = randomTeleports,
                ["target_distance"] = targetDistance,
                ["target_teleport_chance"] = targetTeleportChance
            };
        }
    }
    public class ComponentTickWorld : EntityComponent
    {
        public bool neverDespawn;
        public int tickRadius;
        public float distanceToPlayers; // only if neverDespawn == false

        public override string GetIdentifier() =>
            "minecraft:tick_world";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["distance_to_players"] = distanceToPlayers,
                ["never_Despawn"] = neverDespawn,
                ["radius"] = tickRadius
            };
        }
    }
    public class ComponentTimer : EntityComponent
    {
        public bool looping;
        public bool randomInterval;
        public EntityEventHandler call;

        /// <summary>
        /// Set time to a single value.
        /// </summary>
        public float TimeSingle
        {
            set {
                timeMin = value;
                timeMax = value;
            }
        }
        public float timeMin;
        public float timeMax;

        public override string GetIdentifier() =>
            "minecraft:timer";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["looping"] = looping,
                ["randomInterval"] = randomInterval,
                ["time_down_event"] = call.ToComponentJSON()
            };

            if (timeMin == timeMax)
                json["time"] = timeMin;
            else
                json["time"] = new JArray(new[] { timeMin, timeMax });

            return json;
        }
    }
    public class ComponentTradeTable : EntityComponent
    {
        public string displayName;
        public LootTable table;

        public bool convertEconomy;
        public bool newScreen;
        public bool persistTrades;

        public override string GetIdentifier() =>
            "minecraft:trade_table";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["table"] = table.ResourcePath,
                ["convert_trades_economy"] = convertEconomy,
                ["new_screen"] = newScreen,
                ["persist_trades"] = persistTrades
            };

            if (displayName != null)
                json["display_name"] = displayName;

            return json;
        }
    }
    public class ComponentTrail : EntityComponent
    {
        public FilterCollection filters;
        public int offsetX, offsetY, offsetZ;
        public string block;

        public override string GetIdentifier() =>
            "minecraft:trail";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["block_type"] = block,
                ["spawn_offset"] = new JArray(new[] { offsetX, offsetY, offsetZ })
            };

            if (filters != null)
                json["spawn_filter"] = filters.ToJSON();

            return json;
        }
    }
    public class ComponentTransformation : EntityComponent
    {
        public struct Add
        {
            public EntityComponentGroup[] groups;
            public Add(params EntityComponentGroup[] groups)
            {
                this.groups = groups;
            }
            public JObject ToJSON()
            {
                return new JObject()
                {
                    ["component_groups"] = new JArray(groups.Select(group => group.name))
                };
            }
        }
        public struct Delay
        {
            public float blockAssistChance; // 0.0 - 1.0
            public float blockChance;       // 0.0 - 1.0
            public int blockMax;
            public int blockRadius;
            public string[] speedUpBlocks;
            public float value;

            public JObject ToJSON()
            {
                JObject json = new JObject()
                {
                    ["block_assist_chance"] = blockAssistChance,
                    ["block_chance"] = blockChance,
                    ["block_max"] = blockMax,
                    ["block_radius"] = blockRadius,
                    ["value"] = value
                };

                if (speedUpBlocks != null)
                    json["block_type"] = new JArray(speedUpBlocks);

                return json;
            }
        }

        public Add[] groupsToAdd;
        public Delay? delay;

        public string beginTransformationSound;
        public string endTransformationSound;
        public string transformInto;

        public bool dropEquipment;
        public bool dropInventory;
        public bool keepLevel;
        public bool keepOwner;
        public bool preserveEquipment;

        public override string GetIdentifier() =>
            "minecraft:transformation";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["drop_equipment"] = dropEquipment,
                ["drop_inventory"] = dropInventory,
                ["keep_level"] = keepLevel,
                ["keep_owner"] = keepOwner,
                ["preserve_equipment"] = preserveEquipment
            };

            if (groupsToAdd != null)
                json["add"] = new JArray(groupsToAdd.Select(add => add.ToJSON()));
            if (beginTransformationSound != null)
                json["begin_transform_sound"] = beginTransformationSound;
            if (delay.HasValue)
                json["delay"] = delay.Value.ToJSON();
            if (beginTransformationSound != null)
                json["transformation_sound"] = endTransformationSound;
            if (beginTransformationSound != null)
                json["into"] = transformInto;

            return json;
        }
    }
    public class ComponentTrusting : EntityComponent
    {
        public float probability; // 0.0 - 1.0
        public string[] trustItems;
        public EntityEventHandler trustEvent;

        public override string GetIdentifier() =>
            "minecraft:trusting";
        public override JObject _GetValue()
        {
            JObject json = new JObject()
            {
                ["probability"] = probability,
                ["trust_event"] = trustEvent.ToComponentJSON()
            };

            if (trustItems != null)
                json["trust_items"] = new JArray(trustItems);

            return json;
        }
    }
    public class ComponentWaterMovement : EntityComponent
    {
        public float dragFactor;

        public override string GetIdentifier() =>
            "minecraft:water_movement";
        public override JObject _GetValue()
        {
            return new JObject()
            {
                ["drag_factor"] = dragFactor
            };
        }
    }
}