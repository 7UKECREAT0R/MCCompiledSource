using mc_compiled.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors.Dialogue
{
    /// <summary>
    /// A button on a dialogue <see cref="Scene"/>.
    /// </summary>
    public class Button
    {
        private RawTextJsonBuilder _name;
        public string[] commands;

        public string NameString
        {
            set
            {
                _name = new RawTextJsonBuilder();
                _name.AddTerm(new JSONText(value));
            }
        }
        public string NameTranslate
        {
            set
            {
                _name = new RawTextJsonBuilder();
                _name.AddTerm(new JSONText(value));
            }
        }
        public JSONRawTerm[] Name
        {
            set
            {
                _name = new RawTextJsonBuilder();
                _name.AddTerms(value);
            }
        }
        public RawTextJsonBuilder NameRaw
        {
            get
            {
                if (_name == null)
                {
                    _name = new RawTextJsonBuilder();
                    return _name;
                }

                _name.ClearTerms();
                return _name;
            }
        }

        public Button(string[] commands)
        {
            this.commands = commands;
        }

        public JObject Build()
        {
            return new JObject()
            {
                ["name"] = _name.Build(),
                ["commands"] = new JArray(commands ?? new string[0])
            };
        }
    }
}
