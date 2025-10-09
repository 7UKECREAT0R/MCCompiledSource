using System.Linq;
using mc_compiled.Json;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Dialogue;

/// <summary>
///     A button on a dialogue <see cref="Scene" />.
/// </summary>
public class Button
{
    private RawText _name;
    public string[] commands;

    public Button(string[] commands) { this.commands = commands; }

    public string NameString
    {
        set
        {
            this._name = new RawText();
            this._name.AddTerm(new Text(value));
        }
    }
    public string NameTranslate
    {
        set
        {
            this._name = new RawText();
            this._name.AddTerm(new Translate(value));
        }
    }
    public RawTextEntry[] Name
    {
        set
        {
            this._name = new RawText();
            this._name.AddTerms(value);
        }
    }
    public RawText NameRaw
    {
        get
        {
            if (this._name == null)
            {
                this._name = new RawText();
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