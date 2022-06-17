using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.SyntaxHighlighting
{
    /// <summary>
    /// UDL2.0 (notepad++) language exporter.
    /// </summary>
    internal class UDL2 : SyntaxTarget
    {
        int level;
        TextWriter writer;

        // super basic xml writer. performance isnt important since this will be run like... once
        public string EscapeXML(string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            str = str.Replace("'", "&apos;");
            str = str.Replace("\"", "&quot;");
            return str;
        }
        public void WriteTabs()
        {
            for (int i = 0; i < level; i++)
                writer.Write('\t');
        }
        public void Open(string str)
        {
            WriteTabs();
            writer.WriteLine($"<{str}>");
            level++;
        }
        public void Open(string str, string parts)
        {
            WriteTabs();
            writer.WriteLine($"<{str} {parts}>");
            level++;
        }
        public void Close(string str)
        {
            level--;
            WriteTabs();
            writer.WriteLine($"</{str}>");
        }
        public void State(string str, string parts)
        {
            WriteTabs();
            writer.WriteLine($"<{str} {parts} />");
        }
        public void State(Highlight highlight, string name, string fontName)
        {
            string fgColor = highlight.HexWithoutHash;
            string bgColor = "FFFFFF";
            string fontStyle = ((int)highlight.style).ToString();

            State("WordsStyle", $@"name=""{name}"" fgColor=""{fgColor}"" bgColor=""{bgColor}"" fontName=""{fontName}"" fontStyle=""{fontStyle}"" nesting=""0""");
        }
        public void Content(string str, string parts, string contents)
        {
            WriteTabs();
            writer.WriteLine($"<{str} {parts}>{EscapeXML(contents)}</{str}>");
        }
        public void Content(Keywords keywords, string name, bool numbered = false)
        {
            List<string> items = new List<string>();

            int index = 0;
            foreach (string keyword in keywords.keywords)
            {
                string keywordNormal;

                if (keyword.Contains(' '))
                    keywordNormal = $"'{keyword}'";
                else
                    keywordNormal = keyword;

                if (numbered)
                    items.Add(index++ + keywordNormal);
                else
                    items.Add(keywordNormal);
            }

            string kw = string.Join(" ", items);
            Content("Keywords", $@"name=""{name}""", kw);
        }
        public string YN(bool v) => v ? "\"yes\"" : "\"no\"";

        public void Write(TextWriter writer)
        {
            this.writer = writer;

            Open("NotepadPlus");
                Open("UserLang", $@"name=""MCCompiled"" ext=""{Syntax.EXTENSION}"" udlVersion=""2.1""");
                    Open("Settings");
                        State("Global", $@"caseIgnored={YN(Syntax.IGNORE_CASE)} allowFoldOfComments={YN(Syntax.COMMENT_FOLDING)} foldCompact={YN(Syntax.COMPACT_FOLDING)} forcePureLC=""0"" decimalSeparator=""0""");
                        State("Prefix", "Keywords1=\"no\" Keywords2=\"no\" Keywords3=\"no\" Keywords4=\"no\" Keywords5=\"no\" Keywords6=\"no\" Keywords7=\"no\" Keywords8=\"no\"");
                    Close("Settings");
                    Open("KeywordLists");
                        Content("Keywords", "name=\"Comments\"", $"00{Syntax.lineComment} 01 02 03{Syntax.multilineOpen} 04{Syntax.multilineClose}");
                        Content("Keywords", "name=\"Numbers, prefix1\"", string.Join(" ", Syntax.numberPrefixes));
                        Content("Keywords", "name=\"Numbers, prefix2\"", "");
                        Content("Keywords", "name=\"Numbers, extras1\"", "");
                        Content("Keywords", "name=\"Numbers, extras2\"", "");
                        Content("Keywords", "name=\"Numbers, suffix1\"", string.Join(" ", Syntax.numberSuffixes));
                        Content("Keywords", "name=\"Numbers, suffix2\"", "");
                        Content("Keywords", "name=\"Numbers, range\"", Syntax.NUMBER_RANGE);
                        Content(Syntax.operators, "Operators1");
                        Content("Keywords", "name=\"Operators2\"", "");
                        Content("Keywords", "name=\"Folders in code1, open\"", Syntax.blockOpen);
                        Content("Keywords", "name=\"Folders in code1, middle\"", "");
                        Content("Keywords", "name=\"Folders in code1, close\"", Syntax.blockClose);
                        Content("Keywords", "name=\"Folders in code2, open\"", "");
                        Content("Keywords", "name=\"Folders in code2, middle\"", "");
                        Content("Keywords", "name=\"Folders in code2, close\"", "");
                        Content("Keywords", "name=\"Folders in comment, open\"", "");
                        Content("Keywords", "name=\"Folders in comment, middle\"", "");
                        Content("Keywords", "name=\"Folders in comment, close\"", "");
                        Content(Syntax.selectors, "Keywords1");
                        Content(Syntax.preprocessor, "Keywords2");
                        Content(Syntax.commands, "Keywords3");
                        Content(Syntax.literals, "Keywords4");
                        Content(Syntax.types, "Keywords5");
                        Content(Syntax.comparisons, "Keywords6");
                        Content(Syntax.options, "Keywords7");
                        Content("Keywords", "name=\"Keywords8\"", "");
                        Content("Keywords", "name=\"Delimiters\"", $@"00{Syntax.stringDelimiter} 01\ 02(({Syntax.stringDelimiter} EOL)) 03{Syntax.bracketOpen} 04 05(({Syntax.bracketClose} EOL)) 06 07 08 09 10 11 12 13 14 15 16 17 18 19 20 21 22 23");
                    Close("KeywordLists");
                    Open("Styles");
                        const string font = "Consolas";
                        State("WordsStyle", $@"name=""DEFAULT"" fgColor=""FFFFFF"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                        State(Syntax.commentColor, "COMMENTS", font);
                        State(Syntax.commentColor, "LINE COMMENTS", font);
                        State(Syntax.numberColor, "NUMBERS", font);
                        State(Syntax.selectors.style, "KEYWORDS1", font);
                        State(Syntax.preprocessor.style, "KEYWORDS2", font);
                        State(Syntax.commands.style, "KEYWORDS3", font);
                        State(Syntax.literals.style, "KEYWORDS4", font);
                        State(Syntax.types.style, "KEYWORDS5", font);
                        State(Syntax.comparisons.style, "KEYWORDS6", font);
                        State(Syntax.options.style, "KEYWORDS7", font);
                        State("WordsStyle", $@"name=""KEYWORDS8"" fgColor=""000000"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                        State(Syntax.operators.style, "OPERATORS", font);
                        State("WordsStyle", $@"name=""FOLDER IN CODE1"" fgColor=""00FF40"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                        State("WordsStyle", $@"name=""FOLDER IN CODE2"" fgColor=""000000"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                        State("WordsStyle", $@"name=""FOLDER IN COMMENT"" fgColor=""000000"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                        State(Syntax.options.style, "KEYWORDS7", font);
                        State(Syntax.stringColor, "DELIMITERS1", font);
                        State(Syntax.selectorColor, "DELIMITERS2", font);
                        State("WordsStyle", $@"name=""DELIMITERS3"" fgColor=""000000"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                        State("WordsStyle", $@"name=""DELIMITERS4"" fgColor=""000000"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                        State("WordsStyle", $@"name=""DELIMITERS5"" fgColor=""000000"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                        State("WordsStyle", $@"name=""DELIMITERS6"" fgColor=""000000"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                        State("WordsStyle", $@"name=""DELIMITERS7"" fgColor=""000000"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                        State("WordsStyle", $@"name=""DELIMITERS8"" fgColor=""000000"" bgColor=""FFFFFF"" fontName=""{font}"" fontStyle=""0"" nesting=""0""");
                    Close("Styles");
                Close("UserLang");
            Close("NotepadPlus");
        }
        public string Describe() =>
            "UDL 2.1 Exporter for Notepad++";
        public string GetFile() =>
            "mcc.xml";
    }
}
