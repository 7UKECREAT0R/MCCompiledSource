using System.Collections.Generic;
using System.IO;
using System.Linq;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC.SyntaxHighlighting;

/// <summary>
///     Monarch exporter for all Monarch-based editors.
/// </summary>
internal class Monarch : SyntaxTarget
{
    public void Write(TextWriter writer)
    {
        writer.WriteLine("const mccompiled = {");

        KeywordPattern(writer, "operators", Syntax.operators.keywords);
        KeywordPattern(writer, "selectors", Syntax.selectors.keywords);
        KeywordPattern(writer, "preprocessor", Syntax.preprocessor.keywords);
        KeywordPattern(writer, "commands", Syntax.commands.keywords);
        KeywordPattern(writer, "literals", Syntax.literals.keywords);
        KeywordPattern(writer, "types", Syntax.types.keywords);
        KeywordPattern(writer, "comparisons", Syntax.comparisons.keywords);
        KeywordPattern(writer, "options", Syntax.options.keywords);

        writer.WriteLine(
            @"    tokenizer: {
        root: [
            [ /@?[a-zA-Z$]\w*/, {
                cases: {
                    '@selectors': 'selectors',
                    '@preprocessor': 'preprocessor',
                    '@commands': 'commands',
                    '@literals': 'literals',
                    '@types': 'types',
                    '@comparisons': 'comparisons',
                    '@options': 'options'
                }
            }],
			
			{ include: '@handler' },
			
			[ /[<>{}=()+\-*/%!]+/, 'operators' ],

            // terminated strings
            [ /""(?:[^""\\]|\\.)*""/, 'string' ],
            [ /'(?:[^'\\]|\\.)*'/, 'string' ],

            // unterminated strings
			[ /""(?:[^""\\]|\\.)*$/, 'string' ],
			[ /'(?:[^'\\]|\\.)*$/, 'string' ],

            [ /\[.+]/, 'selectors.properties' ],
            [ /!?(?:\.\.)?\d+(?:\.\.)?\.?\d*[hms]?/, 'numbers' ]
        ],
		comment: [
            [/[^\/*]+/, 'comment' ],
			[/\/\*/, 'comment', '@push' ],
			[""\\*/"", 'comment', '@pop'  ],
			[/[\/*]/, 'comment' ]
		],
		handler: [
			[/[ \t\r\n]+/, 'white' ],
			[/\/\*/, 'comment', '@comment' ],
			[/\/\/.*$/, 'comment' ],
		]
    }");
        writer.WriteLine('}');

        KeywordField(writer, "mcc_operators", Syntax.operators.keywords);
        KeywordField(writer, "mcc_selectors", Syntax.selectors.keywords);
        KeywordField(writer, "mcc_preprocessor", Syntax.preprocessor.keywords);
        KeywordField(writer, "mcc_commands", Syntax.commands.keywords);
        KeywordField(writer, "mcc_literals", Syntax.literals.keywords);
        KeywordField(writer, "mcc_types", Syntax.types.keywords);
        KeywordField(writer, "mcc_comparisons", Syntax.comparisons.keywords);
        KeywordField(writer, "mcc_options", Syntax.options.keywords);
    }

    public string Describe()
    {
        return "Monarch exporter for monaco-based editors.";
    }
    public string GetFile()
    {
        return "mcc-monarch.js";
    }
    private static void KeywordPattern(TextWriter writer, string type, IEnumerable<Keyword> values)
    {
        writer.WriteLine($"\t{type}: [ `{string.Join("`, `", values.Select(v => v.name))}` ],");
    }
    private static void KeywordField(TextWriter writer, string type, Keyword[] values)
    {
        writer.WriteLine($@"const {type} = [");

        int len = values.Length;
        for (int i = 0; i < len; i++)
        {
            Keyword value = values[i];

            writer.WriteLine("\t{");
            writer.WriteLine($"\t\tword: `{value.name}`,");
            writer.WriteLine(value.documentation != null
                ? $"\t\tdocs: `{value.documentation.Replace("`", "\\`")}`"
                : $"\t\tdocs: 'No documentation available for v{Executor.MCC_VERSION}.'");
            writer.WriteLine("\t},");
        }

        writer.WriteLine("]");
    }
}