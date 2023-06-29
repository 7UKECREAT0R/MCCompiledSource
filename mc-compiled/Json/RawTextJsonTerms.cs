using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
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

        /// <summary>
        /// Returns an array of terms representing this term, but localized (if enabled).
        /// </summary>
        /// <returns></returns>
        public abstract JSONRawTerm[] Localize(Executor executor, Statement forExceptions);
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

        public override JSONRawTerm[] Localize(Executor executor, Statement forExceptions)
        {
            if (!executor.HasLocale)
                return new[] { new JSONText(text) };

            // find leading/trailing whitespace
            int leadingWhitespace = text.TakeWhile(c => char.IsWhiteSpace(c)).Count();
            int trailingWhitespace = text.Reverse().TakeWhile(c => char.IsWhiteSpace(c)).Count();

            bool hasLeadingWhitespace = leadingWhitespace > 0;
            bool hasTrailingWhitespace = trailingWhitespace > 0;

            string safeHashCode = text.GetHashCode().ToString().Replace('-', '_');

            // basic string, nothing fancy here
            if (!hasLeadingWhitespace && !hasTrailingWhitespace)
            {
                string _key = Executor.GetNextGeneratedName(Executor.MCC_TRANSLATE_PREFIX + "rawtext" + safeHashCode);
                _key = executor.SetLocaleEntry(_key, text, forExceptions, true).key;
                return new[] { new JSONTranslate(_key) };
            }

            int indices = 1 + (hasLeadingWhitespace ? 1 : 0) + (hasTrailingWhitespace ? 1 : 0);
            int index = 0;
            string textCopy = this.text;
            JSONRawTerm[] output = new JSONRawTerm[indices];

            // extract the leading whitespace, if any
            if (hasLeadingWhitespace)
            {
                string whitespace = textCopy.Substring(0, leadingWhitespace);
                textCopy = textCopy.Substring(leadingWhitespace);
                output[index++] = new JSONText(whitespace);
            }

            // extract the trailing whitespace, if any
            if (hasTrailingWhitespace)
            {
                int len = textCopy.Length;
                string whitespace = textCopy.Substring(len - trailingWhitespace);
                textCopy = textCopy.Substring(0, len - trailingWhitespace);
                output[index + 1] = new JSONText(whitespace);
            }

            // finally, the translated part.
            string key = Executor.GetNextGeneratedName(Executor.MCC_TRANSLATE_PREFIX + "rawtext" + safeHashCode);
            key = executor.SetLocaleEntry(key, textCopy, forExceptions, true).key;
            output[index] = new JSONTranslate(key);

            // done
            return output;
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

        public override JSONRawTerm[] Localize(Executor executor, Statement forExceptions)
        {
            return new[] { this };
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

        public override JSONRawTerm[] Localize(Executor executor, Statement forExceptions)
        {
            return new[] { this };
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

        public override JSONRawTerm[] Localize(Executor executor, Statement forExceptions)
        {
            return new[] { this };
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

        public override JSONRawTerm[] Localize(Executor executor, Statement forExceptions)
        {
            var newConditionals = terms.Select(term => term.Localize(executor, forExceptions));
            return new[] { new JSONVariant(newConditionals.ToArray()) };
        }
    }
    public class ConditionalTerm
    {
        internal readonly JSONRawTerm[] terms;
        internal readonly ConditionalSubcommand condition;
        internal readonly bool invert;

        internal ConditionalTerm(JSONRawTerm[] terms, ConditionalSubcommand condition, bool invert)
        {
            this.terms = terms;
            this.condition = condition;
            this.invert = invert;
        }


        /// <summary>
        /// Returns a shallow copy of this ConditionalTerm, but localized (if enabled).
        /// </summary>
        /// <returns></returns>
        internal ConditionalTerm Localize(Executor executor, Statement forExceptions)
        {
            var localizedTerms = terms.SelectMany(term => term.Localize(executor, forExceptions));
            return new ConditionalTerm(localizedTerms.ToArray(), condition, invert);
        }
    }
}
