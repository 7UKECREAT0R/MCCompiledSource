using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using mc_compiled.Commands.Native;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Compile and tokenize a script.
    /// </summary>
    public class Compiler
    {
        public static bool DISABLE_MACRO_GUARD = false;
        
        public static readonly Regex INCLUDE_REGEX = new Regex("ppinclude (.+)", RegexOptions.IgnoreCase);
        public static readonly Regex CALL_REGEX = new Regex(@"(\w+)\(((\w+,?\s*)*)\)");
        public const string DEFS_FILE = "definitions.def";
        public readonly Definitions defs;
        public static int CURRENT_LINE = 0;

        public static List<string> guessedValues = new List<string>();
        public static List<string> guessedPPValues = new List<string>();

        /// <summary>
        /// Splits by space but preserves arguments encapsulated with quotation marks.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string[] GetArguments(string input)
        {
            return Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>().Select(match => match.Value).ToArray();
        }

        public Compiler()
        {
            defs = Definitions.GLOBAL_DEFS;
        }

        public Token[] CompileFile(string file)
        {
            // Load file
            if (!File.Exists(file))
                throw new FileNotFoundException("File specified could not be found.");
            string content = defs.ReplaceDefinitions(File.ReadAllText(file));

            foreach (Match fileReplace in INCLUDE_REGEX.Matches(content))
            {
                string fileName = fileReplace.Groups[1].Value;
                fileName = Path.GetFileNameWithoutExtension(fileName) + ".mcc";
                if(!File.Exists(fileName))
                {
                    Console.WriteLine("Linking error: Cannot locate file {0}.", fileName);
                    continue;
                }
                string read = File.ReadAllText(fileName) + '\n';
                content = content.Replace(fileReplace.Value, read);
            }

            return Compile(new CodeReader(content));
        }

        public Token[] Compile(CodeReader code)
        {

        }
    }
}
