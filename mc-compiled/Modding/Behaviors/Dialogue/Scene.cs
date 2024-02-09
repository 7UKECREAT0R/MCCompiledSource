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
        public string CommandReference => sceneTag;
        
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
                _npcName = new RawTextJsonBuilder();
                _npcName.AddTerm(new JSONText(value));
            }
        }
        /// <summary>
        /// Sets the NPC name for this scene as a translation string.
        /// </summary>
        public string NPCNameTranslate
        {
            set
            {
                _npcName = new RawTextJsonBuilder();
                _npcName.AddTerm(new JSONTranslate(value).With("\n"));
            }
        }
        /// <summary>
        /// Sets the NPC name for this scene using a set of raw-text terms.
        /// </summary>
        public JSONRawTerm[] NPCName
        {
            set
            {
                _npcName = new RawTextJsonBuilder();
                _npcName.AddTerms(value);
            }
        }
        /// <summary>
        /// Get the underlying RawTextJsonBuilder for the NPC name, with the terms empty.
        /// </summary>
        public RawTextJsonBuilder NPCNameRaw
        {
            get
            {
                if(_npcName == null)
                {
                    _npcName = new RawTextJsonBuilder();
                    return _npcName;
                }

                _npcName.ClearTerms();
                return _npcName;
            }
        }

        /// <summary>
        /// Sets the text of this scene as a raw string.
        /// </summary>
        public string TextString
        {
            set
            {
                _text = new RawTextJsonBuilder();
                _text.AddTerm(new JSONText(value));
            }
        }
        /// <summary>
        /// Sets the text of this scene as a translation string.
        /// </summary>
        public string TextTranslate
        {
            set
            {
                _text = new RawTextJsonBuilder();
                _text.AddTerm(new JSONTranslate(value).With("\n"));
            }
        }
        /// <summary>
        /// Sets the text of this scene using a set of raw-text terms.
        /// </summary>
        public JSONRawTerm[] Text
        {
            set
            {
                _text = new RawTextJsonBuilder();
                _text.AddTerms(value);
            }
        }
        /// <summary>
        /// Get the underlying RawTextJsonBuilder for the text, with the terms cleared.
        /// </summary>
        public RawTextJsonBuilder TextRaw
        {
            get
            {
                if (_text == null)
                {
                    _text = new RawTextJsonBuilder();
                    return _text;
                }

                _text.ClearTerms();
                return _text;
            }
        }

        public JObject ToJSON()
        {
            var json = new JObject()
            {
                ["scene_tag"] = sceneTag,
                ["npc_name"] = _npcName?.Build(),
                ["text"] = _text?.Build(),
                ["buttons"] = new JArray(from btn in buttons select btn.Build())
            };

            if (openCommands != null && openCommands.Length > 0)
                json["on_open_commands"] = new JArray(openCommands.Select(cmd => cmd.StartsWith("/") ? cmd : '/' + cmd));

            if (closeCommands != null && closeCommands.Length > 0)
                json["on_close_commands"] = new JArray(closeCommands.Select(cmd => cmd.StartsWith("/") ? cmd : '/' + cmd));

            return json;
        }
    }
}
