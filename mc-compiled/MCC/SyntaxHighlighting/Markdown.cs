using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mc_compiled.MCC.SyntaxHighlighting
{
    internal class Markdown : SyntaxTarget
    {
        private const string PRELUDE_MARKDOWN = @"# Cheat Sheet

## Comments
`// <text>` - Line comment. Must be at the start of the line and extends to the very end.<br />
`/* <text> */` - Multiline comment. Only ends when specified, not at the end of the line.

## Code Block
Starts and ends with brackets, holding code inside:
```%lang%
...
{
    // inside code block
}
```

---
";

        /// <summary>
        /// Returns a markdown string describing a directive.
        /// </summary>
        /// <param name="directive"></param>
        /// <returns></returns>
        private static string DescribeDirective(Directive directive)
        {
            var sb = new StringBuilder();
            sb.AppendLine(directive.wikiLink == null
                ? directive.description
                : $"[{directive.description}]({directive.wikiLink})");
            sb.Append(": ");
            sb.AppendLine(directive.documentation);

            foreach(TypePattern pattern in directive.patterns)
            {
                sb.Append($"- `{directive.identifier}");

                if (pattern.Count < 1)
                {
                    sb.AppendLine();
                    continue;
                }

                sb.Append(' ');
                sb.Append(pattern.ToMarkdownDocumentation());
                sb.AppendLine("`");
            }

            sb.AppendLine();
            return sb.ToString();
        }

        public string Describe() => "Markdown exporter for the Wiki Cheatsheet.";
        public string GetFile() => "mcc-cheatsheet.md";
        public void Write(TextWriter writer)
        {
            writer.WriteLine(PRELUDE_MARKDOWN);
            writer.WriteLine("## Types");
            writer.WriteLine("Descriptions of the upcoming types that will be present in the various command arguments.");
            writer.WriteLine();
            writer.WriteLine("id");
            writer.WriteLine(": An identifier that either has meaning or doesn't. An identifier can be the name of anything defined in the language, and is usually context dependent.");
            
            foreach (NamedType typeKV in Syntax.mappings.Values)
            {
                string typeID = typeKV.name;
                Type type = typeKV.type;

                if (type.IsAbstract)
                    continue;
                if (!typeof(IDocumented).IsAssignableFrom(type))
                    continue;

                try
                {
                    var docs = Activator.CreateInstance(type, true) as IDocumented;
                    string documentString = docs?.GetDocumentation() ?? throw new Exception($"Could not create instance of type {type.Name}. as IDocumented.");
                    writer.WriteLine(typeID);
                    writer.WriteLine($": {documentString}");
                    writer.WriteLine();
                }
                catch(Exception)
                {
                    Console.WriteLine("FOR TYPE: " + type.Name);
                    throw;
                }
            }

            writer.WriteLine("## Commands");
            writer.WriteLine($"All the commands in the language (version {Executor.MCC_VERSION}). The command ID is the first word of the line, followed by the arguments it gives. Each command parameter includes the type it's allowed to be and its name. A required parameter is surrounded in `<angle brackets>`, and an optional parameter is surrounded in `[square brackets]`.");
            writer.WriteLine();

            var sortedDirectives = new Dictionary<string, List<string>>();

            foreach (string category in Syntax.categories.Keys)
            {
                var list = new List<string>();
                sortedDirectives[category] = list;
            }

            foreach (Directive directive in Directives.REGISTRY)
            {
                try
                {
                    sortedDirectives[directive.category].Add(DescribeDirective(directive));
                } catch(KeyNotFoundException)
                {
                    Console.WriteLine($"Category \"{directive.category}\" has not been defined in language.json.");
                    return;
                }
            }

            foreach (KeyValuePair<string, List<string>> strings in sortedDirectives)
                strings.Value.Sort();

            IEnumerable<string> directivesAndCategories = sortedDirectives.Select(kv =>
                $@"
### Category: {kv.Key}
{Syntax.categories[kv.Key]}

{string.Join("", kv.Value)}"
            );

            string finalString = string.Join("", directivesAndCategories);
            writer.WriteLine(finalString);
        }
    }
}
