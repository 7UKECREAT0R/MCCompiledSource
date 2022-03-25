using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors
{
    public class AnimationController : IAddonFile
    {
        public readonly string name;
        public List<ControllerState> states;

        public string Identifier
        {
            get => "controller.animation." + name;
        }

        public AnimationController(string name)
        {
            this.name = name;
            states = new List<ControllerState>();
        }
        public JObject ToJSON()
        {
            JObject baseJson = new JObject();
            baseJson["format_version"] = FormatVersion.b_ANIMATION_CONTROLLER.ToString();

            JObject statesJson = new JObject();
            foreach (ControllerState state in states)
                statesJson[state.name] = state.ToJSON();

            baseJson["animation_controllers"] = new JObject()
            {
                [Identifier] = new JObject()
                {
                    ["states"] = statesJson,
                    ["initial_state"] = (states.Count > 0) ? states[0].name : null as string
                }
            };

            return baseJson;
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
            name + ".json";
        public OutputLocation GetOutputLocation() =>
            OutputLocation.b_ANIMATION_CONTROLLERS;
    }
    public struct ControllerState
    {
        public struct Transition
        {
            public readonly string other;
            public readonly MolangValue condition;

            public Transition(string other, MolangValue condition)
            {
                this.other = other;
                this.condition = condition;
            }
            public Transition(ControllerState other, MolangValue condition)
            {
                this.other = other.name;
                this.condition = condition;
            }
            public JObject ToJSON()
            {
                return new JObject()
                {
                    [other] = condition.ToString()
                };
            }
        }

        public readonly string name;
        public Transition[] transitions;
        public string[] onEntry, onExit;

        public JObject ToJSON()
        {
            JObject json = new JObject();

            if (transitions != null)
                json["transitions"] = new JArray(transitions.Select(t => t.ToJSON()));
            if (onEntry != null)
                json["on_entry"] = new JArray(onEntry);
            if (onEntry != null)
                json["on_exit"] = new JArray(onExit);

            return json;
        }
    }
}
