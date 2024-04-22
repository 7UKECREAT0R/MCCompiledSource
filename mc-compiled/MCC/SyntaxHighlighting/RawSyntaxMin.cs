using System.IO;
using Newtonsoft.Json;

namespace mc_compiled.MCC.SyntaxHighlighting
{
    internal class RawSyntaxMin : RawSyntax
    {
        public override void Write(TextWriter writer)
        {
            writer.WriteLine(Build().ToString(Formatting.None));
        }

        public override string Describe() => "Raw syntax output in JSON (minified).";
        public override string GetFile() => "mcc-raw-min.json";
    }
}