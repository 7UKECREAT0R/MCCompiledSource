using mc_compiled.MCC.CustomEntities;
using mc_compiled.Modding.Behaviors.Lists;
using mc_compiled.Modding.Resources;
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
        public List<AnimationController> controllers;
        public List<EntityComponent> components;
        public List<EntityComponentGroup> componentGroups;
        public List<EntityEventHandler> events;

        public string CommandReference
        {
            get => description.identifier;
        }

        public EntityBehavior(string identifier)
        {
            description = new EntityDescription(identifier);
            events = new List<EntityEventHandler>();
        }
        public EntityBehavior()
        {
            controllers = new List<AnimationController>();
            components = new List<EntityComponent>();
            componentGroups = new List<EntityComponentGroup>();
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
        /// Create a dummy entity that serves to act as points, raycasters, etc...
        /// </summary>
        /// <returns></returns>
        internal static DummyFiles CreateDummy(string entityID)
        {
            string geometryID = "geometry." + entityID.Replace(':', '.');
            EntityGeometry geometry = new EntityGeometry("dummy", geometryID);
            EntityEventHandler cleanEvent = new EntityEventHandler(DummyManager.CLEAN_EVENT_NAME,
                EventSubject.self, new EventActionRemoveGroup(new string[] { }));

            return new DummyFiles() {
                cleanEvent = cleanEvent,
                behavior = new EntityBehavior()
                {
                    description = new EntityDescription(entityID),
                    components = new List<EntityComponent>(new EntityComponent[]
                    {
                        new ComponentNameable()
                        {
                            allowNametags = true,
                            alwaysShowName = Program.DEBUG
                        },
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
                    }),
                    componentGroups = new List<EntityComponentGroup>(new EntityComponentGroup[]
                    {
                        new EntityComponentGroup(DummyManager.DESTROY_COMPONENT_GROUP, new ComponentInstantDespawn())
                    }),
                    events = new List<EntityEventHandler>(new EntityEventHandler[]
                    {
                        new EntityEventHandler(DummyManager.DESTROY_EVENT_NAME, action:
                            new EventActionAddGroup(DummyManager.DESTROY_COMPONENT_GROUP)),
                        cleanEvent
                    })
                },
                resources = new EntityResource()
                {
                    name = "dummy",
                    description = new ClientEntityDescription()
                    {
                        identifier = entityID,
                        geometry = geometry,
                        material = "entity_alphatest"
                    },
                },
                geometry = geometry
            };
        }
        /// <summary>
        /// https://wiki.bedrock.dev/entities/dummy-entities.html
        /// Create a null entity that is able to explode at either delayed time periods or instantly.
        /// </summary>
        /// <returns></returns>
        internal static ExploderFiles CreateExploder(string entityID)
        {
            string geometryID = "geometry." + entityID.Replace(':', '.');
            EntityGeometry geometry = new EntityGeometry("exploder", geometryID);

            List<EntityComponentGroup> groups = new List<EntityComponentGroup>();
            List<EntityEventHandler> events = new List<EntityEventHandler>();


            return new ExploderFiles()
            {
                groups = groups,
                events = events,

                behavior = new EntityBehavior()
                {
                    description = new EntityDescription(entityID),
                    components = new List<EntityComponent>(new EntityComponent[]
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
                        }
                    }),
                    componentGroups = groups,
                    events = events
                },
                resources = new EntityResource()
                {
                    name = "exploder",
                    description = new ClientEntityDescription()
                    {
                        identifier = entityID,
                        geometry = geometry,
                        material = "entity_alphatest"
                    },
                },
                geometry = geometry
            };
        }
    }
}
