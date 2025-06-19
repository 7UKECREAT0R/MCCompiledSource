using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mc_compiled.MCC.Language.SyntaxExporter;

public class ExporterUDL2() : SyntaxExporter("mcc-udl2.xml", "udl2", "Notepad++ Language File")
{
    private readonly StringBuilder buffer = new();
    private int indentLevel;

    /// <summary>
    ///     Appends spaces to the internal buffer based on the current indentation level.
    /// </summary>
    private void AppendIndent() { this.buffer.Append(new string(' ', this.indentLevel * 4)); }
    /// <summary>
    ///     Escapes quotation marks and apostrophes with their associated XML escape sequences.
    /// </summary>
    /// <param name="str">The string to escape.</param>
    /// <returns>The escaped string.</returns>
    private static string EscapeQuotes(string str)
    {
        return str.Replace("\"", "&quot;")
            .Replace("'", "&apos;")
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
    /// <summary>
    ///     Converts a dictionary of attributes into a single string representation where each key-value pair
    ///     is formatted as an attribute assignment suitable for XML or HTML elements.
    /// </summary>
    /// <param name="attributes">
    ///     A dictionary containing the attributes to stringify. The dictionary keys represent attribute names,
    ///     and the dictionary values represent the corresponding attribute values to be escaped and included in the output.
    ///     Each attribute name and value pair is mapped to a string formatted as `key="escapedValue"`.
    /// </param>
    /// <returns>
    ///     A single string where all attributes are concatenated using a space delimiter. Each attribute is formatted
    ///     as key-value pairs, with special characters in attribute values escaped for XML compliance using
    ///     <see cref="EscapeQuotes(string)" />.
    /// </returns>
    private static string StringifyAttributes(Dictionary<string, string> attributes)
    {
        IEnumerable<string> entries = attributes.Select(entry =>
            $"{entry.Key}=\"{EscapeQuotes(entry.Value)}\"");
        return string.Join(" ", entries);
    }
    /// <summary>
    ///     Add a full XML tag to the buffer.
    /// </summary>
    /// <param name="tag">The tag to open.</param>
    /// <param name="attributes">The attributes to include with the tag, if any.</param>
    /// <param name="content">The content inside the tag, if any.</param>
    private void AddTag(string tag, Dictionary<string, string> attributes = null, string content = null)
    {
        string attributesString = attributes == null ? null : StringifyAttributes(attributes);

        AppendIndent();

        // open the tag
        this.buffer.Append("<")
            .Append(tag)
            .Append(' ');

        if (attributesString != null)
            this.buffer.Append(attributesString);

        // end the tag early if there's no content
        if (string.IsNullOrWhiteSpace(content))
        {
            this.buffer.AppendLine(" />");
            return;
        }

        this.buffer.Append(">")
            .Append(EscapeQuotes(content))
            .Append("</")
            .Append(tag)
            .AppendLine(">");
    }
    /// <summary>
    ///     Adds a word style tag. This is just a convenience method for <see cref="AddTag" />.
    /// </summary>
    /// <param name="name">The name of the keywords list to associate this style with.</param>
    /// <param name="fgColor">The foreground color; hexadecimal format without a '#'</param>
    /// <param name="bgColor">The background color; hexadecimal format without a '#'</param>
    /// <param name="fontName">The name of the font to use.</param>
    /// <param name="fontStyle">The style of the font to use, generally 0</param>
    /// <param name="nesting">The nesting value for the style, generally 0.</param>
    private void AddWordStyle(string name,
        string fgColor,
        string bgColor = "FFFFFF",
        string fontName = MCCPrismTheme.DEFAULT_FONT,
        int fontStyle = 0,
        int nesting = 0)
    {
        AddTag("WordsStyle", new Dictionary<string, string>
        {
            ["name"] = name,
            ["fgColor"] = fgColor,
            ["bgColor"] = bgColor,
            ["fontName"] = fontName,
            ["fontStyle"] = fontStyle.ToString(),
            ["nesting"] = nesting.ToString()
        });
    }

    /// <summary>
    ///     Opens an XML tag.
    /// </summary>
    /// <param name="tag">The tag to open.</param>
    /// <param name="attributes">The attributes to include with the tag, if any.</param>
    /// <returns>A <see cref="TagContract" /> which can be disposed to close the tag.</returns>
    private TagContract OpenTag(string tag, Dictionary<string, string> attributes = null)
    {
        AppendIndent();
        this.indentLevel++;

        this.buffer.Append("<")
            .Append(tag);

        if (attributes == null)
        {
            this.buffer.AppendLine(">");
            return new TagContract(this, tag);
        }

        this.buffer.Append(' ')
            .Append(StringifyAttributes(attributes))
            .AppendLine(">");
        return new TagContract(this, tag);
    }
    /// <summary>
    ///     Called by <see cref="TagContract.Dispose" />, you should <i>probably</i> use that instead.
    /// </summary>
    /// <param name="tag"></param>
    private void _CloseTag(string tag)
    {
        this.indentLevel--;
        AppendIndent();

        this.buffer.Append("</")
            .Append(tag)
            .AppendLine(">");
    }

    public override string Export()
    {
        this.buffer.Clear();
        this.indentLevel = 0;

        TagContract rootTag = OpenTag("NotepadPlus");
        TagContract userLangTag = OpenTag("UserLang", new Dictionary<string, string>
        {
            ["name"] = "MCCompiled",
            ["ext"] = "mcc",
            ["udlVersion"] = "2.1"
        });
        using (OpenTag("Settings"))
        {
            AddTag("Global", new Dictionary<string, string>
            {
                ["caseIgnored"] = "no",
                ["allowFoldOfComments"] = "yes",
                ["foldCompact"] = "no",
                ["forcePureLC"] = "0",
                ["decimalSeparator"] = "0"
            });
            AddTag("Prefix", new Dictionary<string, string>
            {
                ["Keywords1"] = "no",
                ["Keywords2"] = "no",
                ["Keywords3"] = "no",
                ["Keywords4"] = "no",
                ["Keywords5"] = "no",
                ["Keywords6"] = "no",
                ["Keywords7"] = "no",
                ["Keywords8"] = "no"
            });
        }

        using (OpenTag("KeywordLists"))
        {
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Comments"},
                "00// 01 02 03/* 04*/");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Numbers, prefix1"},
                "~ ^ ! ..");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Numbers, prefix2"},
                "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Numbers, extras1"},
                "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Numbers, extras2"},
                "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Numbers, suffix1"},
                "h m s t");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Numbers, suffix2"},
                "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Numbers, range"},
                "..");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Operators1"},
                "< > { } = ( ) + - * / % !");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Operators2"},
                "");

            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Folders in code1, open"}, "{");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Folders in code1, middle"}, "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Folders in code1, close"}, "}");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Folders in code2, open"}, "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Folders in code2, middle"}, "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Folders in code2, close"}, "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Folders in comment, open"}, "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Folders in comment, middle"}, "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Folders in comment, close"}, "");

            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Keywords1"},
                "@e @a @s @p @r");

            string preprocessorDirectiveNames =
                string.Join(" ", Language.AllPreprocessorDirectives.Select(d => d.name));
            string regularDirectiveNames =
                string.Join(" ", Language.AllRuntimeDirectives.Select(d => d.name));

            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Keywords2"},
                preprocessorDirectiveNames);
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Keywords3"},
                regularDirectiveNames);

            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Keywords4"},
                "true false not and null ~ ^");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Keywords5"},
                "int decimal bool time ppv extern export bind auto partial async global local");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Keywords6"},
                "count any block blocks until align anchored as at facing if unless in positioned rotated");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Keywords7"},
                "dummies autoinit exploders uninstall tests audiofiles up down left right forward backward ascending " +
                "descending survival creative adventure spectator removeall times subtitle destroy replace hollow outline keep " +
                "new open change lockinventory lockslot canplaceon: candestroy: enchant: name: lore: author: title: page: dye: " +
                "text: button: onOpen: onClose:");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Keywords8"}, "");
            AddTag("Keywords", new Dictionary<string, string> {["name"] = "Delimiters"},
                "00\" 01\\ 02((\" EOL)) 03[ 04 05((] EOL)) 06 07 08 09 10 11 12 13 14 15 16 17 18 19 20 21 22 23");
        }

        using (OpenTag("Styles"))
        {
            AddWordStyle("DEFAULT", MCCPrismTheme.DEFAULT_TEXT.Hex);
            AddWordStyle("COMMENTS", MCCPrismTheme.COMMENTS.Hex);
            AddWordStyle("LINE COMMENTS", MCCPrismTheme.COMMENTS.Hex);
            AddWordStyle("NUMBERS", MCCPrismTheme.NUMBERS.Hex);
            AddWordStyle("KEYWORDS1", MCCPrismTheme.SELECTORS.Hex);
            AddWordStyle("KEYWORDS2", MCCPrismTheme.PREPROCESSOR_DIRECTIVES.Hex);
            AddWordStyle("KEYWORDS3", MCCPrismTheme.REGULAR_DIRECTIVES.Hex);
            AddWordStyle("KEYWORDS4", MCCPrismTheme.LANGUAGE_KEYWORDS.Hex);
            AddWordStyle("KEYWORDS5", MCCPrismTheme.TYPE_KEYWORDS.Hex);
            AddWordStyle("KEYWORDS6", MCCPrismTheme.CONDITIONAL_KEYWORDS.Hex);
            AddWordStyle("KEYWORDS7", MCCPrismTheme.SUBCOMMAND_KEYWORDS.Hex);
            AddWordStyle("KEYWORDS8", "000000");
            AddWordStyle("OPERATORS", MCCPrismTheme.OPERATORS.Hex);
            AddWordStyle("FOLDER IN CODE1", MCCPrismTheme.BLOCK_BRACKETS.Hex);
            AddWordStyle("FOLDER IN CODE2", "000000");
            AddWordStyle("FOLDER IN COMMENT", "000000");
            AddWordStyle("DELIMITERS1", MCCPrismTheme.DELIMITERS.Hex);
            AddWordStyle("DELIMITERS2", MCCPrismTheme.DELIMITERS_ALTERNATE.Hex);
            AddWordStyle("DELIMITERS3", "000000");
            AddWordStyle("DELIMITERS4", "000000");
            AddWordStyle("DELIMITERS5", "000000");
            AddWordStyle("DELIMITERS6", "000000");
            AddWordStyle("DELIMITERS7", "000000");
            AddWordStyle("DELIMITERS8", "000000");
        }

        userLangTag.Dispose();
        rootTag.Dispose();
        return this.buffer.ToString();
    }

    /// <summary>
    ///     Represents a contract for managing an open XML tag in the <see cref="ExporterUDL2" />.
    ///     Instances of this class are used to ensure that tags are properly
    ///     closed by implementing the <see cref="IDisposable" /> pattern.
    /// </summary>
    private class TagContract(ExporterUDL2 parent, string tag) : IDisposable
    {
        private bool isClosed;

        public void Dispose()
        {
            if (this.isClosed)
                return;
            this.isClosed = true;
            parent._CloseTag(tag);
        }
    }
}