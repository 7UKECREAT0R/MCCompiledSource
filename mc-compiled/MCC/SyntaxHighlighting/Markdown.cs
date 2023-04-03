using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.SyntaxHighlighting
{
    internal class Markdown : SyntaxTarget
    {
        private static readonly string PRELUDE_MARKDOWN = @"# Concepts
#### Comments
`// <text>` - Line comment. Must be at the start of the line and extends to the very end.<br />
`/* <text> */` - Multiline comment. Only ends when specified, not at the end of the line.

#### Code Block
Starts and ends with brackets, holding code inside:
```js
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
        public string DescribeDirective(Directive directive)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"#### {directive.description} `{directive.identifier}`");
            sb.AppendLine(directive.documentation);

            foreach(TypePattern pattern in directive.patterns)
            {
                sb.Append($"- `{directive.identifier}`");

                if (pattern.Count < 1)
                {
                    sb.AppendLine();
                    continue;
                }

                sb.Append(' ');
                sb.AppendLine(pattern.ToMarkdownDocumentation());
            }

            return sb.ToString();
        }

        public string Describe() => "Markdown exporter for the Wiki Cheatsheet.";
        public string GetFile() => "mcc-cheatsheet.md";
        public void Write(TextWriter writer)
        {
            writer.WriteLine(PRELUDE_MARKDOWN);
            writer.WriteLine("# Types");
            writer.WriteLine("Descriptions of the upcoming types that will be present in the different command arguments.");
            writer.WriteLine("- `id` An identifier that either has meaning or doesn't. An identifier can be the name of anything defined in the language, and is usually context dependent.");
            foreach (var typeKV in Syntax.mappings.Values)
            {
                string typeID = typeKV.name;
                Type type = typeKV.type;

                if (type.IsAbstract)
                    continue;
                if (!typeof(IDocumented).IsAssignableFrom(type))
                    continue;

                try
                {
                    IDocumented docs = Activator.CreateInstance(type, true) as IDocumented;
                    string documentString = docs.GetDocumentation();
                    writer.WriteLine($"- `{typeID}` {documentString}");
                } catch(Exception)
                {
                    Console.WriteLine("FOR TYPE: " + type.Name);
                    throw;
                }
            }

            writer.WriteLine();
            writer.WriteLine("---");
            writer.WriteLine();

            writer.WriteLine("# Commands");
            writer.WriteLine($"All of the commands in the language (version {Executor.MCC_VERSION}). The command ID is the first word of the line, followed by the arguments it gives. Each command parameter includes the type it's allowed to be and its name. A required parameter is surrounded in \\<angle brackets\\>, and an optional parameter is surrounded in [square brackets].");
            writer.WriteLine();

            Dictionary<string, List<string>> sortedDirectives = new Dictionary<string, List<string>>();

            foreach (string category in Syntax.categories.Keys)
            {
                List<string> list = new List<string>();
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
                $@"---
## Category: {kv.Key}
{Syntax.categories[kv.Key]}


{string.Join("\n", kv.Value)}"
            );

            string finalString = string.Join("\n", directivesAndCategories);
            writer.WriteLine(finalString);

            writer.WriteLine("---");
            writer.WriteLine("# Features");
            writer.WriteLine("Extended language features can be enabled through the `feature` `<id: feature name>` command and enable functionality into your project. The reason these need to be manually enabled is because they produce extra files which may be unexpected for users not intending to use these features.");
            writer.WriteLine();
            foreach (Feature feature in FeatureManager.FEATURE_LIST)
            {
                string name = feature.ToString().ToLower();
                string docs = Syntax.options.keywords.First(key => key.name == name).documentation;
                writer.WriteLine($"#### `feature` `{name}`");
                writer.WriteLine($"{docs}\n");

            }
        }
    }
}
