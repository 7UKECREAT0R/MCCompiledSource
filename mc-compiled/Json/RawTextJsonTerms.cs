using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Json
{
    /// <summary>
    /// Term in a JSON rawtext sequence.
    /// </summary>
    public abstract class JSONRawTerm
    {
        public static string EscapeString(string text) =>
            text.Replace(@"\", @"\\").Replace("\"", "\\\"");

        /// <summary>
        /// Builds the JObject that represents this JSON term.
        /// </summary>
        /// <returns></returns>
        public abstract JObject Build();

        /// <summary>
        /// Returns a preview string of this JSON term, for the rawtext builder.
        /// </summary>
        /// <returns></returns>
        public abstract string PreviewString();
    }

    /// <summary>
    /// Represents a token of plain text.
    /// </summary>
    public class JSONText : JSONRawTerm
    {
        string text;
        public JSONText(string text)
        {
            this.text = EscapeString(text);
        }

        public override JObject Build()
        {
            return new JObject()
            {
                ["text"] = text
            };
        }
        public override string PreviewString()
        {
            return '[' + text + ']';
        }
    }
    /// <summary>
    /// Represents the value of a scoreboard objective under a certain entity.
    /// </summary>
    public class JSONScore : JSONRawTerm
    {
        string selector, objective;
        public JSONScore(string selector, string objective)
        {
            this.selector = EscapeString(selector);
            this.objective = EscapeString(objective);
        }
        public JSONScore(ScoreboardValue objective)
        {
            this.selector = EscapeString(objective.clarifier.CurrentString);
            this.objective = EscapeString(objective.Name);
        }
        public override JObject Build()
        {
            return new JObject()
            {
                ["score"] = new JObject()
                {
                    ["name"] = selector,
                    ["objective"] = objective
                }
            };
        }
        public override string PreviewString()
        {
            return "[SCORE " + objective + " OF " + selector + ']';
        }
    }
    /// <summary>
    /// Represents an entity's name based off of a selector.
    /// </summary>
    public class JSONSelector : JSONRawTerm
    {
        string selector;
        public JSONSelector(string selector)
        {
            this.selector = EscapeString(selector);
        }

        public override JObject Build()
        {
            return new JObject()
            {
                ["selector"] = selector.ToString()
            };
        }
        public override string PreviewString()
        {
            return '[' + selector + ']';
        }
    }
    /// <summary>
    /// Represents a translation key with optional objects inserted.
    /// </summary>
    public class JSONTranslate : JSONRawTerm
    {
        string translationKey;
        List<RawTextJsonBuilder> with;

        public JSONTranslate(string translationKey)
        {
            this.translationKey = EscapeString(translationKey);
            this.with = new List<RawTextJsonBuilder>();
        }
        public JSONTranslate With(params RawTextJsonBuilder[] jsonTerms)
        {
            this.with.AddRange(jsonTerms);
            return this;
        }

        public override JObject Build()
        {
            if(with.Any())
            {
                return new JObject()
                {
                    ["translate"] = translationKey,
                    ["with"] = new JArray(from subtext in with select subtext.Build())
                };
            }

            return new JObject()
            {
                ["translate"] = translationKey
            };
        }
        public override string PreviewString()
        {
            return '[' + translationKey + ']';
        }
    }
    /// <summary>
    /// A term which can convert to multiple possible outcomes depending on the evaluation 
    /// </summary>
    public class JSONVariant : JSONRawTerm
    {
        public readonly List<ConditionalTerm> terms;

        public JSONVariant(params ConditionalTerm[] terms)
        {
            this.terms = new List<ConditionalTerm>(terms);
        }
        public JSONVariant(IEnumerable<ConditionalTerm> terms)
        {
            this.terms = new List<ConditionalTerm>(terms);
        }
        public override JObject Build()
        {
            // not supposed to get string'd
            return new JObject();
        }
        public override string PreviewString()
        {
            return "{variant}";
        }
    }
    public class ConditionalTerm
    {
        internal readonly JSONRawTerm term;
        internal readonly ConditionalSubcommand condition;
        internal readonly bool invert;

        internal ConditionalTerm(JSONRawTerm term, ConditionalSubcommand condition, bool invert)
        {
            this.term = term;
            this.condition = condition;
            this.invert = invert;
        }
    }
}
