using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors;

public class AnimationController : IAddonFile
{
    public readonly string name;
    public string defaultState;
    public List<ControllerState> states;

    public AnimationController(string name)
    {
        this.name = name;
        this.states = [];
    }
    /// <summary>
    ///     Retrieves the default state, or the name of the first state if unavailable.
    /// </summary>
    public string DefaultState
    {
        get
        {
            if (this.defaultState != null)
                return this.defaultState;

            return this.states.Count > 0 ? this.states[0].name : null;
        }
    }

    public string Identifier => "controller.animation." + this.name;

    public string CommandReference => throw new NotImplementedException();

    public string GetExtendedDirectory()
    {
        return null;
    }
    public byte[] GetOutputData()
    {
        JObject full = ToJSON();
        string str = full.ToString();
        return Encoding.UTF8.GetBytes(str);
    }
    public string GetOutputFile()
    {
        return this.name + ".json";
    }
    public OutputLocation GetOutputLocation()
    {
        return OutputLocation.b_ANIMATION_CONTROLLERS;
    }
    public JObject ToJSON()
    {
        var baseJson = new JObject();
        baseJson["format_version"] = FormatVersion.b_ANIMATION_CONTROLLER.ToString();

        var statesJson = new JObject();
        foreach (ControllerState state in this.states)
            statesJson[state.name] = state.ToJSON();

        baseJson["animation_controllers"] = new JObject
        {
            [this.Identifier] = new JObject
            {
                ["states"] = statesJson,
                ["initial_state"] = this.DefaultState
            }
        };

        return baseJson;
    }
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
            return new JObject
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
        var json = new JObject();

        if (this.transitions != null)
            json["transitions"] = new JArray(this.transitions.Select(t => t.ToJSON()));
        if (this.onEntryCommands != null)
            json["on_entry"] = new JArray(this.onEntryCommands.Cast<object>().ToArray());
        if (this.onExitCommands != null)
            json["on_exit"] = new JArray(this.onExitCommands.Cast<object>().ToArray());

        return json;
    }
}