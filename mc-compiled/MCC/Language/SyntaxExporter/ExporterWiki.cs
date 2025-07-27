using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.MCC.Language.SyntaxExporter;

public class ExporterWiki() : SyntaxExporter("mcc-cheatsheet.md", "wiki", "Wiki Cheatsheet in Markdown")
{
    private readonly StringBuilder sb = new();

    public override string Export()
    {
        this.sb.Clear();
        AddHeader(1, "Cheat Sheet");
        AddLine("This file is generated automatically with `mc-compiled --syntax wiki`");

        // some preliminary documentation stuff
        AddHeader(2, "Comments", "comments");
        AddBulletPoint("`// <comment>` - Line comment. Lets you make a note for yourself on the current line.");
        AddBulletPoint("`/* <comment> */` - Multi-line comment. Lets you make a note for yourself on MULTIPLE LINES!");

        AddHeader(2, "Code Block", "code-block");
        AddBulletPoint("Starts and ends with `{` brackets `}`, holding only code inside.");

        AddMCCCodeBlock("""
                        ...
                        {
                            // inside the code block
                        }
                        """);
        AddSeparator();

        AddHeader(2, "Types", "types");
        AddLine("Descriptions of the upcoming types that will be present in the various command arguments.");
        AddDefinitionList(new List<KeyValuePair<string, string>>
        {
            new("identifier",
                "An identifier that either has meaning or doesn't. An identifier can be the name of anything defined in the language. It's usually self-explanatory when it's required."),
            new("integer",
                "Any integral number, like 5, 10, 5291, or -40. Use time suffixes to scale the integer accordingly, like with 4s -> 80."),
            new("string",
                "A block of text on a single line, surrounded with either 'single quotes' or \"double quotes.\""),
            new("true/false", "A value that can be either 'true' or 'false.'"),
            new("selector",
                "A Minecraft selector that targets a specific entity or set of entities. Example: `@e[type=cow]`"),
            new("value", "The name of a runtime value that was defined using the `define` command."),
            new("preprocessor variable",
                "The name of a preprocessor variable that was defined using the `$var` command or similar, **without** the `$` symbol."),
            new("coordinate",
                "A Minecraft coordinate value that can optionally be both relative and facing offset, like ~10, 40, or ^5."),
            new("range",
                "A Minecraft number that specifies a range of integers (inclusive). Omitting a number from one side makes the number unbounded. `4..` means four and up. `1..5` means one through five."),
            new("JSON", "A JSON object achieved by $dereferencing a preprocessor variable holding one.")
        });

        AddHeader(2, "Commands", "commands-root");
        AddLine(
            $"All the commands in the language (version 1.{Executor.MCC_VERSION}). The command ID is the first word of the line, followed by the arguments it gives. " +
            "Each command parameter includes the type it's allowed to be and its name. " +
            "A required parameter is surrounded in `<angle brackets>`, and an optional parameter is surrounded in `[square brackets]`.");

        foreach ((string categoryName, string categoryDescription) in Language.categories)
        {
            AddHeader(3, $"Category: {categoryName}", $"commands-{categoryName}");
            AddLine(categoryDescription);
            AddLine();

            Directive[] directives = Language.DirectivesByCategory(categoryName).ToArray();
            Array.Sort(directives, (a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

            foreach (Directive directive in directives)
            {
                (string usageString, int indentLevel)[] usages = directive.BuildUsageGuide();

                AddLine(directive.wikiLink != null
                    ? $"[{directive.description}]({directive.wikiLink})"
                    : directive.description);
                AddLine(": " + directive.details);

                if (directive.HasAttribute(DirectiveAttribute.DOCUMENTABLE))
                    AddLine(
                        "<format color=\"MediumSeaGreen\">Can be documented by writing a comment right before running this command.</format>");
                if (directive.HasAttribute(DirectiveAttribute.USES_FSTRING))
                    AddLine(
                        "<format color=\"CadetBlue\">Supports [<format color=\"CadetBlue\">format-strings.</format>](Text-Commands.md#format-strings)</format>");

                foreach ((string usageString, int indentLevel) in usages)
                    AddBulletPoint(usageString, indentLevel);

                if (!string.IsNullOrEmpty(directive.exampleCode))
                {
                    AddLine("```%lang%");
                    string[] lines = directive.exampleCode.Split('\n');
                    foreach (string line in lines)
                        if (string.IsNullOrWhiteSpace(line))
                            AddLine("%empty%");
                        else
                            AddLine(line);
                    AddLine("```");
                }

                AddLine();
            }
        }

        AddLine();
        return this.sb.ToString();
    }

    #region MarkdownStuff

    /// <summary>
    ///     Adds a horizontal separator to the internal <see cref="StringBuilder" />.
    ///     The separator consists of three dashes ("---") surrounded by blank lines before and after for clarity.
    /// </summary>
    /// <remarks>
    ///     This method is useful for visually separating sections in the generated content.
    ///     The separator is formatted to improve readability in Markdown documents.
    /// </remarks>
    public void AddSeparator() { this.sb.AppendLine().AppendLine("---").AppendLine(); }
    /// <summary>
    ///     Adds a Markdown header to the internal <see cref="StringBuilder" />.
    ///     The header is prefixed with a number of hash symbols ('#') corresponding to the specified <paramref name="level" />
    ///     ,
    ///     followed by the <paramref name="content" /> and a blank line for separation.
    /// </summary>
    /// <param name="level">
    ///     The level of the header, where 1 represents the top level (e.g., "# Header") and 6 represents the lowest level
    ///     (e.g., "###### Header"). Must be between 1 and 6, inclusive.
    /// </param>
    /// <param name="content">
    ///     The text content of the header.
    /// </param>
    /// <param name="linkId">If not null, the ID to apply to the header for linking support.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if <paramref name="level" /> is less than 1 or greater than 6.
    /// </exception>
    public void AddHeader(int level, string content, string linkId = null)
    {
        if (level is < 1 or > 6)
            throw new ArgumentOutOfRangeException(nameof(level));
        if (linkId != null)
            content = $"{content} {{id=\"{linkId}\"}}";
        this.sb.AppendLine().Append('#', level).Append(' ').AppendLine(content).AppendLine();
    }
    /// <summary>
    ///     Adds a single blank line to the internal <see cref="StringBuilder" />. Alias for <c>this.sb.AppendLine();</c>
    /// </summary>
    public void AddLine() { this.sb.AppendLine(); }
    /// <summary>
    ///     Adds a single line of text to the internal <see cref="StringBuilder" />.
    ///     Alias for <c>this.sb.AppendLine(text);</c>
    /// </summary>
    /// <param name="text">The text to place on the line.</param>
    public void AddLine(string text) { this.sb.AppendLine(text); }
    /// <summary>
    ///     Adds a bullet point to the internal <see cref="StringBuilder" />.
    ///     The bullet point is prefixed with a hyphen ("-") and followed by the specified text.
    /// </summary>
    /// <param name="text">
    ///     The content of the bullet point to be added. This is the text that will follow the hyphen.
    /// </param>
    /// <param name="indentLevel">The indent level to place the bullet point at. 0 is no intent.</param>
    /// <remarks>
    ///     This method is used to format content as a bullet point in Markdown, enabling structured lists
    ///     to be represented effectively in the generated document.
    /// </remarks>
    public void AddBulletPoint(string text, int indentLevel = 0)
    {
        if (indentLevel == 0)
        {
            this.sb.Append("- ").AppendLine(text);
            return;
        }

        this.sb.Append('\t', indentLevel).Append("- ").AppendLine(text);
    }
    /// <summary>
    ///     Adds a code block to the internal <see cref="StringBuilder" />. The block is formatted in Markdown using
    ///     syntax highlighting with an optional language.
    /// </summary>
    /// <param name="code">
    ///     The code content to be included within the code block. This string represents the block of code that will
    ///     be encapsulated within Markdown triple backticks.
    /// </param>
    /// <param name="language">
    ///     The optional language specifier used for syntax highlighting in Markdown. If specified, the language
    ///     identifier will follow the opening triple backticks (e.g., "```csharp"). If <c>null</c>, no language specifier
    ///     will be included.
    /// </param>
    /// <remarks>
    ///     This method generates a valid Markdown code block and appends it to the internal <see cref="StringBuilder" />.
    ///     Ensure that the provided code is formatted correctly for the intended language to produce meaningful syntax
    ///     highlighting in compatible Markdown renderers.
    /// </remarks>
    public void AddCodeBlock(string code, string language = null)
    {
        this.sb.AppendLine()
            .Append("```")
            .Append(language)
            .AppendLine()
            .AppendLine(code)
            .AppendLine("```")
            .AppendLine();
    }
    public void AddMCCCodeBlock(string code)
    {
        AddCodeBlock(code, "%lang%"); // the %lang% automatically gets replaced by writerside
    }

    /// <summary>
    ///     Adds a definition list to the internal <see cref="StringBuilder" /> in the Writerside definition list format.
    /// </summary>
    /// <param name="entries">
    ///     An <see cref="IEnumerable{T}" /> of keys/values where the key represents the title of the definition, and the value
    ///     represents the
    ///     explanatory text.
    ///     The keys and values in <paramref name="entries" /> are added in order of enumeration.
    /// </param>
    public void AddDefinitionList(IEnumerable<KeyValuePair<string, string>> entries)
    {
        this.sb.AppendLine();
        foreach ((string title, string text) in entries)
            this.sb.AppendLine(title)
                .Append(": ")
                .AppendLine(text)
                .AppendLine();
    }

    #endregion
}