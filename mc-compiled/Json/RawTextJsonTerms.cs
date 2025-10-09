using System.Collections.Generic;
using System.Linq;
using mc_compiled.Commands.Execute;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;

namespace mc_compiled.Json;

/// <summary>
///     Term in a JSON rawtext sequence.
/// </summary>
public abstract class RawTextEntry
{
    public static string EscapeString(string text) { return text.Replace(@"\", @"\\").Replace("\"", "\\\""); }

    /// <summary>
    ///     Builds the JObject that represents this JSON term.
    /// </summary>
    /// <returns></returns>
    public abstract JObject Build();

    /// <summary>
    ///     Returns a preview string of this JSON term, for the rawtext builder.
    /// </summary>
    /// <returns></returns>
    public abstract string PreviewString();

    /// <summary>
    ///     Returns an array of terms representing this term, but localized (if enabled).
    /// </summary>
    /// <returns></returns>
    public abstract RawTextEntry[] Localize(Executor executor, string identifier, Statement forExceptions);
}

/// <summary>
///     Represents a token of plain text.
/// </summary>
public class Text : RawTextEntry
{
    private string text;
    public Text(string text) { this.text = EscapeString(text); }

    public override JObject Build()
    {
        return new JObject
        {
            ["text"] = this.text
        };
    }
    public override string PreviewString() { return '[' + this.text + ']'; }

    public override RawTextEntry[] Localize(Executor executor, string identifier, Statement forExceptions)
    {
        bool hasNewlines = this.text.Contains(@"\\n");

        if (!executor.HasLocale)
        {
            if (hasNewlines)
                this.text = this.text.Replace(@"\\n", "\n");
            return [new Text(this.text)];
        }

        // find leading/trailing whitespace
        int leadingWhitespace = this.text.TakeWhile(char.IsWhiteSpace).Count();
        int trailingWhitespace = this.text.Reverse().TakeWhile(char.IsWhiteSpace).Count();

        // find unescaped newlines in this.text
        if (hasNewlines)
            this.text = this.text.Replace(@"\\n", "%1");
        this.text = this.text.Replace("\\", ""); // this is really stupid and prone to break

        bool hasLeadingWhitespace = leadingWhitespace > 0;
        bool hasTrailingWhitespace = trailingWhitespace > 0;

        string key = Executor.GetNextGeneratedName(executor.LocaleEntryPrefix + identifier, false, true);

        // basic string, nothing fancy here
        if (!hasLeadingWhitespace && !hasTrailingWhitespace)
        {
            key = executor.SetLocaleEntry(key, this.text, forExceptions, true)?.key;
            return
            [
                key == null ? new Text(this.text) :
                hasNewlines ? new Translate(key).WithNewlineSupport() : new Translate(key)
            ];
        }

        int indices = 1 + (hasLeadingWhitespace ? 1 : 0) + (hasTrailingWhitespace ? 1 : 0);
        int index = 0;
        string textCopy = this.text;
        var output = new RawTextEntry[indices];

        // extract the leading whitespace, if any
        if (hasLeadingWhitespace)
        {
            string whitespace = textCopy[..leadingWhitespace];
            textCopy = textCopy[leadingWhitespace..];
            output[index++] = new Text(whitespace);
        }

        // extract the trailing whitespace, if any
        if (hasTrailingWhitespace)
        {
            int len = textCopy.Length;
            string whitespace = textCopy[(len - trailingWhitespace)..];
            textCopy = textCopy[..(len - trailingWhitespace)];
            output[index + 1] = new Text(whitespace);
        }

        // finally, the translated part.
        key = executor.SetLocaleEntry(key, textCopy, forExceptions, true)?.key;

        if (key == null)
            output[index] = new Text(textCopy);
        else
            output[index] = hasNewlines ? new Translate(key).WithNewlineSupport() : new Translate(key);

        // done
        return output;
    }
}

/// <summary>
///     Represents the value of a scoreboard objective under a certain entity.
/// </summary>
public class Score : RawTextEntry
{
    private readonly string objective;
    private readonly string selector;
    public Score(string selector, string objective)
    {
        this.selector = EscapeString(selector);
        this.objective = EscapeString(objective);
    }
    public Score(ScoreboardValue objective)
    {
        this.selector = EscapeString(objective.clarifier.CurrentString);
        this.objective = EscapeString(objective.InternalName);
    }
    public override JObject Build()
    {
        return new JObject
        {
            ["score"] = new JObject
            {
                ["name"] = this.selector,
                ["objective"] = this.objective
            }
        };
    }
    public override string PreviewString() { return "[SCORE " + this.objective + " OF " + this.selector + ']'; }

    public override RawTextEntry[] Localize(Executor executor, string identifier, Statement forExceptions)
    {
        return [this];
    }
}

/// <summary>
///     Represents an entity's name based off of a selector.
/// </summary>
public class Selector : RawTextEntry
{
    private readonly string selector;
    public Selector(string selector) { this.selector = EscapeString(selector); }

    public override JObject Build()
    {
        return new JObject
        {
            ["selector"] = this.selector
        };
    }
    public override string PreviewString() { return '[' + this.selector + ']'; }

    public override RawTextEntry[] Localize(Executor executor, string identifier, Statement forExceptions)
    {
        return [this];
    }
}

/// <summary>
///     Represents a translation key with optional objects inserted.
/// </summary>
public class Translate : RawTextEntry
{
    private readonly string translationKey;
    private readonly List<RawText> with;
    private readonly List<string> withStr;

    private bool withUsesStr;

    public Translate(string translationKey)
    {
        this.translationKey = EscapeString(translationKey);
        this.withStr = [];
        this.with = [];
    }
    public Translate With(params RawText[] jsonTerms)
    {
        this.withUsesStr = false;
        this.with.AddRange(jsonTerms);
        return this;
    }
    public Translate With(params string[] strings)
    {
        this.withUsesStr = true;
        this.withStr.AddRange(strings);
        return this;
    }
    /// <summary>
    ///     Adds a "with" entry to this rawtext, which enables support for using newlines.
    /// </summary>
    /// <returns></returns>
    public Translate WithNewlineSupport() { return With("\n"); }

    public override JObject Build()
    {
        if (!this.with.Any() && !this.withStr.Any())
            return new JObject
            {
                ["translate"] = this.translationKey
            };

        if (this.withUsesStr)
            return new JObject
            {
                ["translate"] = this.translationKey,
                ["with"] = new JArray(this.withStr.ToArray().Cast<object>())
            };

        return new JObject
        {
            ["translate"] = this.translationKey,
            ["with"] = new JArray(from subtext in this.with select subtext.Build())
        };
    }
    public override string PreviewString() { return '[' + this.translationKey + ']'; }

    public override RawTextEntry[] Localize(Executor executor, string identifier, Statement forExceptions)
    {
        return [this];
    }
}

/// <summary>
///     A term which can convert to multiple possible outcomes depending on the evaluation
/// </summary>
public class Variant : RawTextEntry
{
    public readonly List<ConditionalTerm> terms;

    public Variant(params ConditionalTerm[] terms) { this.terms = [..terms]; }
    public Variant(IEnumerable<ConditionalTerm> terms) { this.terms = [..terms]; }
    public override JObject Build()
    {
        // not supposed to get string'd
        return new JObject();
    }
    public override string PreviewString() { return "{variant}"; }

    public override RawTextEntry[] Localize(Executor executor, string identifier, Statement forExceptions)
    {
        IEnumerable<ConditionalTerm> newConditionals =
            this.terms.Select(term => term.Localize(executor, identifier, forExceptions));
        return [new Variant(newConditionals.ToArray())];
    }
}

public class ConditionalTerm
{
    internal readonly ConditionalSubcommand condition;
    internal readonly bool invert;
    internal readonly RawTextEntry[] terms;

    internal ConditionalTerm(RawTextEntry[] terms, ConditionalSubcommand condition, bool invert)
    {
        this.terms = terms;
        this.condition = condition;
        this.invert = invert;
    }

    /// <summary>
    ///     Returns a shallow copy of this ConditionalTerm, but localized (if enabled).
    /// </summary>
    /// <returns></returns>
    internal ConditionalTerm Localize(Executor executor, string identifier, Statement forExceptions)
    {
        IEnumerable<RawTextEntry> localizedTerms =
            this.terms.SelectMany(term => term.Localize(executor, identifier, forExceptions));
        return new ConditionalTerm(localizedTerms.ToArray(), this.condition, this.invert);
    }
}