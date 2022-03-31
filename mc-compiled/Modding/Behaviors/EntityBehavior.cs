﻿using mc_compiled.Modding.Behaviors.Lists;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors
{
    /// <summary>
    /// An entity definition.
    /// </summary>
    public class EntityBehavior : IAddonFile
    {
        public FormatVersion formatVersion = FormatVersion.b_ENTITY;
        public EntityDescription description;
        public AnimationController[] controllers;
        public EntityComponent[] components;
        public EntityComponentGroup[] componentGroups;
        public List<EntityEventHandler> events;

        public EntityBehavior(string identifier)
        {
            description = new EntityDescription(identifier);
            events = new List<EntityEventHandler>();
        }
        public EntityBehavior()
        {
            events = new List<EntityEventHandler>();
        }
        /// <summary>
        /// Create a new event and register it into this behavior.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public EntityEventHandler CreateEvent(string id, EntityEventAction action)
        {
            EntityEventHandler handler = new EntityEventHandler(id, action: action);
            events.Add(handler);
            return handler;
        }
        public JObject ToJSON()
        {
            JObject root = new JObject();
            
            // Description
            JObject desc = description.ToJSON();
            if (controllers != null)
            {
                JObject animations = new JObject();
                foreach (AnimationController anim in controllers)
                    animations[anim.name] = anim.Identifier;

                var animate = controllers.Select(anim => anim.name);

                desc["animations"] = animations;
                desc["scripts"] = new JObject()
                {
                    ["animate"] = new JArray(animate)
                };
            }
            root["description"] = desc;

            // Component Groups
            if (componentGroups != null)
            {
                JObject componentGroupsJson = new JObject();
                foreach (EntityComponentGroup group in componentGroups)
                    componentGroupsJson.Add(group.ToJSON());
                root["component_groups"] = componentGroupsJson;
            }

            // Components
            if (components != null)
            {
                JObject componentsJson = new JObject();
                foreach (EntityComponent component in components)
                    componentsJson[component.GetIdentifier()] = component.GetValue();
                root["components"] = componentsJson;
            }

            // Events
            if(events.Count > 0)
            {
                JObject eventsJson = new JObject();
                foreach (EntityEventHandler @event in events)
                    eventsJson.Add(@event.ToDefinitionJSON());
                root["events"] = eventsJson;
            }

            return new JObject()
            {
                ["format_version"] = FormatVersion.b_ENTITY.ToString(),
                ["minecraft:entity"] = root
            };
        }

        public string GetExtendedDirectory() =>
            null;
        public byte[] GetOutputData()
        {
            JObject full = ToJSON();
            string str = full.ToString();
            return Encoding.UTF8.GetBytes(str);
        }
        public string GetOutputFile() =>
            description.GetEntityName() + ".json";
        public OutputLocation GetOutputLocation() =>
            OutputLocation.b_ENTITIES;

        /// <summary>
        /// https://wiki.bedrock.dev/entities/dummy-entities.html
        /// Create a null entity that serves to act as points, raycasters, etc...
        /// </summary>
        /// <returns></returns>
        internal static EntityBehavior CreateNull(string entityID)
        {
            return new EntityBehavior()
            {
                description = new EntityDescription(entityID),
                components = new EntityComponent[]
                {
                    new ComponentCustomHitTest()
                    {
                        hitboxes = new ComponentCustomHitTest.Hitbox[]
                        {
                            new ComponentCustomHitTest.Hitbox(new Offset3(0, 100, 0), 0, 0)
                        }
                    },
                    new ComponentDamageSensor()
                    {
                        triggerPool = new ComponentDamageSensor.Trigger[]
                        {
                            new ComponentDamageSensor.Trigger()
                            {
                                dealsDamage = false,
                                cause = Commands.DamageCause.all,
                                damageModifier = 1,
                                damageMultiplier = 0
                            }
                        }
                    },
                    new ComponentPushable()
                    {
                        isPushableByEntity = false,
                        isPushableByPiston = false
                    },
                    new ComponentCollisionBox()
                    {
                        width = 0.0001f,
                        height = 0.0001f
                    },
                    new ComponentTickWorld()
                    {
                        neverDespawn = true,
                        tickRadius = 2
                    }
                },
                componentGroups = new EntityComponentGroup[]
                {
                    new EntityComponentGroup(MCC.NullManager.destroyComponentGroup, new ComponentInstantDespawn())
                },
                events = new List<EntityEventHandler>(new EntityEventHandler[]
                {
                    new EntityEventHandler(MCC.NullManager.destroyEventName, action:
                        new EventActionAddGroup(MCC.NullManager.destroyComponentGroup))
                })
            };
        }
    }
}