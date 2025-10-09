using System.Collections.Generic;
using System.Linq;
using mc_compiled.Json;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors.Dialogue;

/// <summary>
///     A scene, or page, in an NPC dialogue. See <see cref="DialogueManager.scenes" />
/// </summary>
public class Scene
{
    private readonly List<Button> buttons;
    public readonly string sceneTag;
    private RawText _npcName;
    private RawText _text;
    public string[] closeCommands;

    public string[] openCommands;

    public Scene(string sceneTag)
    {
        this.sceneTag = sceneTag;
        this.buttons = [];
    }
    public string CommandReference => this.sceneTag;

    /// <summary>
    ///     Sets the NPC name for this scene as a raw string.
    /// </summary>
    public string NPCNameString
    {
        set
        {
            this._npcName = new RawText();
            this._npcName.AddTerm(new Text(value));
        }
    }
    /// <summary>
    ///     Sets the NPC name for this scene as a translation string.
    /// </summary>
    public string NPCNameTranslate
    {
        set
        {
            this._npcName = new RawText();
            this._npcName.AddTerm(new Translate(value).WithNewlineSupport());
        }
    }
    /// <summary>
    ///     Sets the NPC name for this scene using a set of raw-text terms.
    /// </summary>
    public RawTextEntry[] NPCName
    {
        set
        {
            this._npcName = new RawText();
            this._npcName.AddTerms(value);
        }
    }
    /// <summary>
    ///     Get the underlying RawTextJsonBuilder for the NPC name, with the terms empty.
    /// </summary>
    public RawText NPCNameRaw
    {
        get
        {
            if (this._npcName == null)
            {
                this._npcName = new RawText();
                return this._npcName;
            }

            this._npcName.ClearTerms();
            return this._npcName;
        }
    }

    /// <summary>
    ///     Sets the text of this scene as a raw string.
    /// </summary>
    public string TextString
    {
        set
        {
            this._text = new RawText();
            this._text.AddTerm(new Text(value));
        }
    }
    /// <summary>
    ///     Sets the text of this scene as a translation string.
    /// </summary>
    public string TextTranslate
    {
        set
        {
            this._text = new RawText();
            this._text.AddTerm(new Translate(value).WithNewlineSupport());
        }
    }
    /// <summary>
    ///     Sets the text of this scene using a set of raw-text terms.
    /// </summary>
    public RawTextEntry[] Text
    {
        set
        {
            this._text = new RawText();
            this._text.AddTerms(value);
        }
    }
    /// <summary>
    ///     Get the underlying RawTextJsonBuilder for the text, with the terms cleared.
    /// </summary>
    public RawText TextRaw
    {
        get
        {
            if (this._text == null)
            {
                this._text = new RawText();
                return this._text;
            }

            this._text.ClearTerms();
            return this._text;
        }
    }

    public Scene AddButton(Button button)
    {
        this.buttons.Add(button);
        return this;
    }
    public Scene AddButtons(params Button[] newButtons) { return AddButtons((IEnumerable<Button>) newButtons); }
    public Scene AddButtons(IEnumerable<Button> newButtons)
    {
        this.buttons.AddRange(newButtons);
        return this;
    }

    public JObject ToJSON()
    {
        var json = new JObject
        {
            ["scene_tag"] = this.sceneTag,
            ["npc_name"] = this._npcName?.Build(),
            ["text"] = this._text?.Build(),
            ["buttons"] = new JArray(from btn in this.buttons select btn.Build())
        };

        if (this.openCommands != null && this.openCommands.Length > 0)
            json["on_open_commands"] =
                new JArray(this.openCommands.Select(cmd => cmd.StartsWith("/") ? cmd : '/' + cmd));

        if (this.closeCommands != null && this.closeCommands.Length > 0)
            json["on_close_commands"] =
                new JArray(this.closeCommands.Select(cmd => cmd.StartsWith("/") ? cmd : '/' + cmd));

        return json;
    }
}