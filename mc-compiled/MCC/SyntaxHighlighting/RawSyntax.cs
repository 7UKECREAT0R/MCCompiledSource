using System.IO;
using System.Linq;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.SyntaxHighlighting;

/// <summary>
///     Raw syntax exporter.
/// </summary>
internal class RawSyntax : SyntaxTarget
{
    public virtual void Write(TextWriter writer)
    {
        writer.WriteLine(Build().ToString(Formatting.Indented));
    }
    public virtual string Describe()
    {
        return "Raw syntax output in JSON (formatted).";
    }
    public virtual string GetFile()
    {
        return "mcc-raw.json";
    }
    private static JObject BuildKeywords(Keywords keywords)
    {
        var json = new JObject
        {
            ["formatting"] = keywords.style.ToJson()
        };

        foreach (Keyword keyword in keywords.keywords)
            json[keyword.name] = keyword.documentation;

        return json;
    }
    protected static JObject Build()
    {
        var json = new JObject
        {
            ["version"] = Executor.MCC_VERSION,
            ["extension"] = Syntax.EXTENSION,
            ["misc"] = new JObject
            {
                ["ignoreCase"] = Syntax.IGNORE_CASE,
                ["allowCommentFolding"] = Syntax.COMMENT_FOLDING,
                ["allowCompactFolding"] = Syntax.COMPACT_FOLDING
            },
            ["syntax"] = new JObject
            {
                ["rangeDelimiter"] = Syntax.rangeDelimiter,
                ["invertDelimiter"] = Syntax.invertDelimiter,
                ["stringDelimiters"] = new JArray(Syntax.stringDelimiter0, Syntax.stringDelimiter1),
                ["bracketOpen"] = Syntax.bracketOpen,
                ["bracketClose"] = Syntax.bracketClose,
                ["blockOpen"] = Syntax.blockOpen,
                ["blockClose"] = Syntax.blockClose,
                ["escape"] = Syntax.escape,
                ["comments"] = new JObject
                {
                    ["line"] = Syntax.lineComment,
                    ["open"] = Syntax.multilineOpen,
                    ["close"] = Syntax.multilineClose
                },
                ["numberPrefixes"] = new JArray(Syntax.numberPrefixes.Cast<object>()),
                ["numberSuffixes"] = new JArray(Syntax.numberSuffixes.Cast<object>())
            },
            ["keywords"] = new JObject
            {
                ["operators"] = BuildKeywords(Syntax.operators),
                ["selectors"] = BuildKeywords(Syntax.selectors),
                ["preprocessor"] = BuildKeywords(Syntax.preprocessor),
                ["commands"] = BuildKeywords(Syntax.commands),
                ["literals"] = BuildKeywords(Syntax.literals),
                ["types"] = BuildKeywords(Syntax.types),
                ["comparisons"] = BuildKeywords(Syntax.comparisons),
                ["options"] = BuildKeywords(Syntax.options)
            }
        };

        return json;
    }
}