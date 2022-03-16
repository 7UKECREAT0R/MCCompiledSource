using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors.Lists
{
    // https://docs.microsoft.com/en-us/minecraft/creator/reference/content/entityreference/examples/componentlist

    public class ComponentPushable : EntityComponent
    {
        public bool isPushableByEntity;
        public bool isPushableByPiston;

        public ComponentPushable(bool isPushableByEntity, bool isPushableByPiston)
        {
            this.isPushableByEntity = isPushableByEntity;
            this.isPushableByPiston = isPushableByPiston;
        }

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
    public class ComponentCustomHitTest : EntityComponent
    {
        public Offset3 pivot;
        public int width;
        public int height;

        public ComponentCustomHitTest(Offset3 pivot, int width, int height)
        {
            this.pivot = pivot;
            this.width = width;
            this.height = height;
        }

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
}
