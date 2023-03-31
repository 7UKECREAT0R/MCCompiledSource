using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Selectors;
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

        public abstract string GetString();
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
        public override string GetString()
        {
            return $@"{{""text"": ""{text}""}}";
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
        public override string GetString()
        {
            return $@"{{""score"": {{""name"":""{selector}"", ""objective"": ""{objective}""}}}}";
        }
        public override string PreviewString()
        {
            return "[SCORE " + objective + " OF " + selector + ']';
        }

        /// <summary>
        /// Create a JSONVariant that will evaluate to two possible outcomes based on comparing this objective.
        /// </summary>
        /// <param name="a">The value which will be chosen if the condition evaluates to True.</param>
        /// <param name="condition">The condition to check on this objective.</param>
        /// <param name="b">The value which will be chosen if the condition evaluates to False.</param>
        /// <returns></returns>
        public JSONVariant CreateVariant(JSONRawTerm[] a, Range condition, JSONRawTerm[] b) =>
            new JSONVariant(a, selector, objective, condition, b);
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
        public override string GetString()
        {
            return $@"{{""selector"": ""{selector}""}}";
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

        public override string GetString()
        {
            if(with.Any())
            {
                string[] subtexts = with.Select(x => x.BuildString()).ToArray();
                string withComponent = string.Join(",", subtexts);
                return $@"{{""translate"": ""{translationKey}"",""with"": [{withComponent}]}}";
            }

            return $@"{{""translate"": ""{translationKey}""}}";
        }
        public override string PreviewString()
        {
            return '[' + translationKey + ']';
        }
    }
    /// <summary>
    /// A term which can convert to two possible outcomes depending on the evaluation of a range.
    /// Use JSONScore::CreateVariant to create one properly.
    /// </summary>
    public class JSONVariant : JSONRawTerm
    {
        public readonly string selector;
        public readonly string objective;
        public readonly Range condition;
        public readonly JSONRawTerm[] a, b;
        
        public JSONVariant(JSONRawTerm[] a, string selector, string objective, Range condition, JSONRawTerm[] b)
        {
            this.selector = EscapeString(selector);
            this.objective = EscapeString(objective);
            this.condition = condition;
            this.a = a;
            this.b = b;
        }
        public override string GetString()
        {
            // not supposed to get string'd
            return $@"{{""text"": ""{{variant: {objective}?}}""}}";
        }
        public override string PreviewString()
        {
            return $"{{variant: {objective}?}}";
        }

        /// <summary>
        /// Add a check to an execute chain that's needed to check for the A terms being chosen.
        /// </summary>
        /// <returns></returns>
        public Subcommand ConstructSubcommandA()
        {
            return new SubcommandIf(ConditionalSubcommandScore.New(selector, objective, condition));
        }
        /// <summary>
        /// Add a check to an execute chain that's needed to check for the B terms being chosen.
        /// </summary>
        /// <returns></returns>
        public Subcommand ConstructSubcommandB()
        {
            return new SubcommandUnless(ConditionalSubcommandScore.New(selector, objective, condition));
        }
    }
}
