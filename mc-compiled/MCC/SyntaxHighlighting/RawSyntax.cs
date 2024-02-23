using System.IO;
using System.Linq;

namespace mc_compiled.MCC.SyntaxHighlighting
{
    /// <summary>
    /// Raw syntax exporter.
    /// </summary>
    internal class RawSyntax : SyntaxTarget
    {
        public string Stringify(Keywords keywords) => string.Join(" ", keywords.keywords.Select(w => w.name.Contains(' ') ? ("<<" + w.name + ">>") : w.name));
        public void Write(TextWriter writer)
        {
            writer.WriteLine($"# MCC Raw Syntax (version {Compiler.Executor.MCC_VERSION})");
            writer.WriteLine("# This is a comment. Ignore empty lines.");
            writer.WriteLine("# Multi-word keywords are surrounded with <<double angle brackets>>.");
            writer.WriteLine();
            writer.WriteLine("extension=" + Syntax.EXTENSION);
            writer.WriteLine("ignoreCase=" + Syntax.IGNORE_CASE);
            writer.WriteLine("commentFolding=" + Syntax.COMMENT_FOLDING);
            writer.WriteLine("compactFolding=" + Syntax.COMPACT_FOLDING);
            writer.WriteLine();
            writer.WriteLine("blockOpen=" + Syntax.blockOpen);
            writer.WriteLine("blockClose=" + Syntax.blockClose);
            writer.WriteLine("selectorOpen=" + Syntax.bracketOpen);
            writer.WriteLine("selectorClose=" + Syntax.bracketClose);
            writer.WriteLine("string=" + Syntax.stringDelimiter);
            writer.WriteLine("escape=" + Syntax.escape);
            writer.WriteLine();
            writer.WriteLine("numberPrefixes=" + string.Join(" ", Syntax.numberPrefixes));
            writer.WriteLine("numberSuffixes=" + string.Join(" ", Syntax.numberSuffixes));
            writer.WriteLine("rangeOperator=" + Syntax.NUMBER_RANGE);
            writer.WriteLine();
            writer.WriteLine("lineComment=" + Syntax.lineComment);
            writer.WriteLine("openComment=" + Syntax.multilineOpen);
            writer.WriteLine("closeComment=" + Syntax.multilineClose);
            writer.WriteLine();
            writer.WriteLine("operators=" + Stringify(Syntax.operators));
            writer.WriteLine("selectors=" + Stringify(Syntax.selectors));
            writer.WriteLine("preprocessor=" + Stringify(Syntax.preprocessor));
            writer.WriteLine("commands=" + Stringify(Syntax.commands));
            writer.WriteLine("literals=" + Stringify(Syntax.literals));
            writer.WriteLine("types=" + Stringify(Syntax.types));
            writer.WriteLine("comparisons=" + Stringify(Syntax.comparisons));
            writer.WriteLine("options=" + Stringify(Syntax.options));
            writer.WriteLine();
        }
        public string Describe() => "Raw syntax output in a simple format for parsing into a data format.";
        public string GetFile() => "mcc-raw.txt";
    }
}
