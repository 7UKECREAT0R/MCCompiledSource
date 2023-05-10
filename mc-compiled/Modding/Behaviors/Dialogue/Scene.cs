using mc_compiled.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace mc_compiled.Modding.Behaviors.Dialogue
{
    /// <summary>
    /// A scene, or page, in an NPC dialogue. See <see cref="Dialogue.scenes"/>
    /// </summary>
    public class Scene
    {
        public string sceneTag;
        private RawTextJsonBuilder _npcName;
        private RawTextJsonBuilder _text;
        private List<Button> buttons;

        public Scene(string sceneTag)
        {
            this.sceneTag = sceneTag;
            this.buttons = new List<Button>();
        }

        public Scene AddButton(Button button)
        {
            this.buttons.Add(button);
            return this;
        }
        public Scene AddButtons(params Button[] buttons)
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
                _npcName.AddTerm(new JSONText(value));
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
                _text.AddTerm(new JSONText(value));
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

        public JObject Build()
        {
            return new JObject()
            {
                ["scene_tag"] = sceneTag,
                ["npc_name"] = _npcName?.Build(),
                ["text"] = _text?.Build(),
                ["buttons"] = new JArray(from btn in buttons select btn.Build())
            };
        } 
    }
}
