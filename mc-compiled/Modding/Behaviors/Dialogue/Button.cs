using System;
using System.Linq;
using mc_compiled.Json;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Dialogue;

/// <summary>
///     A button on a dialogue <see cref="Scene" />.
/// </summary>
public class Button
{
    private RawTextJsonBuilder _name;
    public string[] commands;

    public Button(string[] commands)
    {
        this.commands = commands;
    }

    public string NameString
    {
        set
        {
            this._name = new RawTextJsonBuilder();
            this._name.AddTerm(new JSONText(value));
        }
    }
    public string NameTranslate
    {
        set
        {
            this._name = new RawTextJsonBuilder();
            this._name.AddTerm(new JSONTranslate(value));
        }
    }
    public JSONRawTerm[] Name
    {
        set
        {
            this._name = new RawTextJsonBuilder();
            this._name.AddTerms(value);
        }
    }
    public RawTextJsonBuilder NameRaw
    {
        get
        {
            if (this._name == null)
            {
                this._name = new RawTextJsonBuilder();
                return this._name;
            }

            this._name.ClearTerms();
            return this._name;
        }
    }

    public JObject Build()
    {
        return new JObject
        {
            ["name"] = this._name.Build(),
            ["commands"] = new JArray(this.commands?.Select(cmd => cmd.StartsWith("/") ? cmd : '/' + cmd) ??
                                      [])
        };
    }
}