using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using mc_compiled.Commands.Native;

namespace mc_compiled.MCC
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


        bool obfuscate, debug;
        public Compiler(bool debug, bool obfuscate)
        {
            this.debug = debug;
            this.obfuscate = obfuscate;
            defs = Definitions.GLOBAL_DEFS;
        }

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
            // Initialize buffer
            List<Token> tokens = new List<Token>();

            // Actually compile
            string line;
            while ((line = code.ReadLine()) != null)
            {
                CURRENT_LINE++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();

                if (line.StartsWith("//"))
                {
                    tokens.Add(new TokenComment(line.Substring(2).Trim()));
                    continue;
                }
                if (line.StartsWith("{"))
                {
                    string block = code.GetBlock('{', '}');
                    Token[] sub = Compile(new CodeReader(block));
                    tokens.Add(new TokenBlock(sub));
                    continue;
                }

                int index = line.IndexOf(' '); // Find first space.

                string keyword, content;
                if (index == -1)
                {
                    keyword = line;
                    content = "";
                }
                else
                {
                    keyword = line.Substring(0, index);
                    content = line.Substring(index + 1);
                }

                // Test if this might be a shallow value operation.
                if(guessedValues.Contains(keyword))
                {
                    Token shallow = new TokenVALUE(line);
                    if (debug)
                        Console.WriteLine("Compile:\t{0}", shallow);
                    tokens.Add(shallow);
                    continue;
                }
                // Then test if this might be a shallow function call.
                // Can't use guessed values since they may use preprocessor variables.
                Match functionCallMatch = CALL_REGEX.Match(line);
                if(functionCallMatch.Success)
                {
                    string functionName = functionCallMatch.Groups[1].Value;
                    string functionArgs = functionCallMatch.Groups[2].Value;
                    Token shallow = new TokenCALL(functionName, functionArgs.Split(' '));
                    if (debug)
                        Console.WriteLine("Compile:\t{0}", shallow);
                    tokens.Add(shallow);
                    continue;
                }

                try
                {
                    Token set = null;
                    switch (keyword.ToLower())
                    {
                        case "ppv":
                            set = new TokenPPV(content);
                            guessedPPValues.Add((set as TokenPPV).name);
                            break;
                        case "ppinc":
                            set = new TokenPPINC(content);
                            break;
                        case "ppdec":
                            set = new TokenPPDEC(content);
                            break;
                        case "ppadd":
                            set = new TokenPPADD(content);
                            break;
                        case "ppsub":
                            set = new TokenPPSUB(content);
                            break;
                        case "ppmul":
                            set = new TokenPPMUL(content);
                            break;
                        case "ppdiv":
                            set = new TokenPPDIV(content);
                            break;
                        case "ppmod":
                            set = new TokenPPMOD(content);
                            break;
                        case "ppif":
                            set = new TokenPPIF(content);
                            break;
                        case "ppelse":
                            set = new TokenPPELSE();
                            break;
                        case "pprep":
                            set = new TokenPPREP(content);
                            break;
                        case "pplog":
                            set = new TokenPPLOG(content);
                            break;
                        case "_ppfile": // no longer used but people can if they want
                            set = new TokenPPFILE(content);
                            break;
                        case "function":
                            set = new TokenFUNCTION(content);
                            break;
                        case "call":
                            set = new TokenCALL(content);
                            break;
                        case "ppmacro":
                            set = new TokenPPMACRO(content);
                            break;
                        case "ppfriendly":
                            set = new TokenPPFRIENDLY(content);
                            break;
                        case "ppupper":
                            set = new TokenPPUPPER(content);
                            break;
                        case "pplower":
                            set = new TokenPPLOWER(content);
                            break;


                        case "mc":
                            set = new TokenMC(content);
                            break;
                        case "select":
                            set = new TokenSELECT(content);
                            break;
                        case "print":
                            set = new TokenPRINT(content);
                            break;
                        case "printp":
                            set = new TokenPRINTP(content);
                            break;
                        case "limit":
                            set = new TokenLIMIT(content);
                            break;
                        case "define":
                            set = new TokenDEFINE(content);
                            guessedValues.Add((set as TokenDEFINE).ValueName);
                            break;
                        case "init":
                        case "initialize":
                            set = new TokenINITIALIZE(content);
                            break;
                        case "value":
                            set = new TokenVALUE(content);
                            break;
                        case "if":
                            set = new TokenIF(content);
                            break;
                        case "else":
                            set = new TokenELSE();
                            break;
                        case "give":
                            set = new TokenGIVE(content);
                            break;
                        case "tp":
                            set = new TokenTP(content);
                            break;
                        case "move":
                            set = new TokenMOVE(content);
                            break;
                    }
                    if (set == null)
                        continue;

                    if (debug)
                        Console.WriteLine("Compile:\t{0}", set);
                    tokens.Add(set);

                } catch(NullReferenceException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Compiler Error: Missing argument(s) on line {0}", CURRENT_LINE);
                    Environment.Exit(-1);
                    return null;
                }
            }
            // Return result
            return tokens.ToArray();
        }
    }
}
