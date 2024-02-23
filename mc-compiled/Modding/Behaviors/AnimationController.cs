using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mc_compiled.Modding.Behaviors
{
    public class AnimationController : IAddonFile
    {
        public readonly string name;
        public string defaultState;
        public List<ControllerState> states;

        public string CommandReference => throw new NotImplementedException();
        /// <summary>
        /// Retrieves the default state, or the name of the first state if unavailable.
        /// </summary>
        public string DefaultState
        {
            get
            {
                if(this.defaultState != null)
                    return this.defaultState;

                return (this.states.Count > 0) ? this.states[0].name : null as string;
            }
        }

        public string Identifier
        {
            get => "controller.animation." + this.name;
        }

        public AnimationController(string name)
        {
            this.name = name;
            this.states = new List<ControllerState>();
        }
        public JObject ToJSON()
        {
            JObject baseJson = new JObject();
            baseJson["format_version"] = FormatVersion.b_ANIMATION_CONTROLLER.ToString();

            JObject statesJson = new JObject();
            foreach (ControllerState state in this.states)
                statesJson[state.name] = state.ToJSON();

            baseJson["animation_controllers"] = new JObject()
            {
                [this.Identifier] = new JObject()
                {
                    ["states"] = statesJson,
                    ["initial_state"] = this.DefaultState
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
        public string GetOutputFile() => this.name + ".json";
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
                    [this.other] = this.condition.ToString()
                };
            }
        }

        public string name;
        public Transition[] transitions;
        public string[] onEntryCommands,
                        onExitCommands;

        public JObject ToJSON()
        {
            JObject json = new JObject();

            if (this.transitions != null)
                json["transitions"] = new JArray(this.transitions.Select(t => t.ToJSON()));
            if (this.onEntryCommands != null)
                json["on_entry"] = new JArray(this.onEntryCommands);
            if (this.onExitCommands != null)
                json["on_exit"] = new JArray(this.onExitCommands);

            return json;
        }
    }
}
