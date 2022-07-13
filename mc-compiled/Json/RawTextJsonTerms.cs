using mc_compiled.Commands;
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
        /// Add a check to a selector that's needed to check for the A terms being chosen.
        /// </summary>
        /// <returns></returns>
        public Commands.Selector ConstructSelectorA(Commands.Selector existing)
        {
            Commands.Selector copy = new Selector(existing);
            copy.scores.checks.Add(new ScoresEntry(objective, condition));
            return copy;
        }
        /// <summary>
        /// Add a check to a selector that's needed to check for the B terms being chosen.
        /// </summary>
        /// <returns></returns>
        public Commands.Selector ConstructSelectorB(Commands.Selector existing)
        {
            Commands.Selector copy = new Selector(existing);
            Range inverse = condition;
            inverse.invert = !inverse.invert;
            copy.scores.checks.Add(new ScoresEntry(objective, inverse));
            return copy;
        }
    }
}
