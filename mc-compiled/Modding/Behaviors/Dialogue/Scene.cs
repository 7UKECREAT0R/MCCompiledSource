using mc_compiled.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.Modding.Behaviors.Dialogue
{
    /// <summary>
    /// A scene, or page, in an NPC dialogue. See <see cref="Dialogue.scenes"/>
    /// </summary>
    public class Scene
    {
        public readonly string sceneTag;
        private RawTextJsonBuilder _npcName;
        private RawTextJsonBuilder _text;
        private readonly List<Button> buttons;

        public string[] openCommands;
        public string[] closeCommands;

        public Scene(string sceneTag)
        {
            this.sceneTag = sceneTag;
            this.buttons = new List<Button>();
        }
        public string CommandReference => this.sceneTag;
        
        public Scene AddButton(Button button)
        {
            this.buttons.Add(button);
            return this;
        }
        public Scene AddButtons(params Button[] buttons)
        {
            return AddButtons((IEnumerable<Button>) buttons);
        }
        public Scene AddButtons(IEnumerable<Button> buttons)
        {
            this.buttons.AddRange(buttons);
            return this;
        }

        /// <summary>
        /// Sets the NPC name for this scene as a raw string.
        /// </summary>
        public string NPCNameString
        {
            set
            {
                this._npcName = new RawTextJsonBuilder();
                this._npcName.AddTerm(new JSONText(value));
            }
        }
        /// <summary>
        /// Sets the NPC name for this scene as a translation string.
        /// </summary>
        public string NPCNameTranslate
        {
            set
            {
                this._npcName = new RawTextJsonBuilder();
                this._npcName.AddTerm(new JSONTranslate(value).WithNewlineSupport());
            }
        }
        /// <summary>
        /// Sets the NPC name for this scene using a set of raw-text terms.
        /// </summary>
        public JSONRawTerm[] NPCName
        {
            set
            {
                this._npcName = new RawTextJsonBuilder();
                this._npcName.AddTerms(value);
            }
        }
        /// <summary>
        /// Get the underlying RawTextJsonBuilder for the NPC name, with the terms empty.
        /// </summary>
        public RawTextJsonBuilder NPCNameRaw
        {
            get
            {
                if(this._npcName == null)
                {
                    this._npcName = new RawTextJsonBuilder();
                    return this._npcName;
                }

                this._npcName.ClearTerms();
                return this._npcName;
            }
        }

        /// <summary>
        /// Sets the text of this scene as a raw string.
        /// </summary>
        public string TextString
        {
            set
            {
                this._text = new RawTextJsonBuilder();
                this._text.AddTerm(new JSONText(value));
            }
        }
        /// <summary>
        /// Sets the text of this scene as a translation string.
        /// </summary>
        public string TextTranslate
        {
            set
            {
                this._text = new RawTextJsonBuilder();
                this._text.AddTerm(new JSONTranslate(value).WithNewlineSupport());
            }
        }
        /// <summary>
        /// Sets the text of this scene using a set of raw-text terms.
        /// </summary>
        public JSONRawTerm[] Text
        {
            set
            {
                this._text = new RawTextJsonBuilder();
                this._text.AddTerms(value);
            }
        }
        /// <summary>
        /// Get the underlying RawTextJsonBuilder for the text, with the terms cleared.
        /// </summary>
        public RawTextJsonBuilder TextRaw
        {
            get
            {
                if (this._text == null)
                {
                    this._text = new RawTextJsonBuilder();
                    return this._text;
                }

                this._text.ClearTerms();
                return this._text;
            }
        }

        public JObject ToJSON()
        {
            var json = new JObject()
            {
                ["scene_tag"] = this.sceneTag,
                ["npc_name"] = this._npcName?.Build(),
                ["text"] = this._text?.Build(),
                ["buttons"] = new JArray(from btn in this.buttons select btn.Build())
            };

            if (this.openCommands != null && this.openCommands.Length > 0)
                json["on_open_commands"] = new JArray(this.openCommands.Select(cmd => cmd.StartsWith("/") ? cmd : '/' + cmd));

            if (this.closeCommands != null && this.closeCommands.Length > 0)
                json["on_close_commands"] = new JArray(this.closeCommands.Select(cmd => cmd.StartsWith("/") ? cmd : '/' + cmd));

            return json;
        }
    }
}
