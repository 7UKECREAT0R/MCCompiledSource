using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compilers
{
    /// <summary>
    /// The compiler which processes the main MCCompiled code.
    /// </summary>
    class DefaultCompiler : Compiler
    {
        public override Token[] Compile(CodeReader code)
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
                if (guessedValues.Contains(keyword))
                {
                    Token shallow = new TokenVALUE(line);
                    if (Program.DEBUG)
                        Console.WriteLine("Compile:\t{0}", shallow);
                    tokens.Add(shallow);
                    continue;
                }
                // Then test if this might be a shallow function call.
                // Can't use guessed values since they may use preprocessor variables.
                Match functionCallMatch = CALL_REGEX.Match(line);
                if (functionCallMatch.Success)
                {
                    string functionName = functionCallMatch.Groups[1].Value;
                    string functionArgs = functionCallMatch.Groups[2].Value;
                    Token shallow = new TokenCALL(functionName, functionArgs.Split(' '));
                    if (Program.DEBUG)
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
                        case "def":
                        case "define":
                            set = new TokenDEFINE(content);
                            guessedValues.Add((set as TokenDEFINE).ValueName);
                            break;
                        case "init":
                        case "initialize":
                            set = new TokenINITIALIZE(content);
                            break;
                        case "value":
                        case "val":
                            set = new TokenVALUE(content);
                            break;
                        case "if":
                            set = new TokenIF(content);
                            break;
                        case "else":
                        case "el":
                            set = new TokenELSE();
                            break;
                        case "give":
                            set = new TokenGIVE(content);
                            break;
                        case "tp":
                            set = new TokenTP(content);
                            break;
                        case "title":
                            set = new TokenTITLE(content);
                            break;
                        case "move":
                            set = new TokenMOVE(content);
                            break;
                        case "face":
                            set = new TokenFACE(content);
                            break;
                        case "place":
                            set = new TokenPLACE(content);
                            break;
                        case "fill":
                            set = new TokenFILL(content);
                            break;
                        case "kick":
                            set = new TokenKICK(content);
                            break;
                        case "halt":
                        case "stop":
                            set = new TokenHALT();
                            break;
                        case "gm":
                        case "gamemode":
                            set = new TokenGAMEMODE(content);
                            break;
                        case "diff":
                        case "difficulty":
                            set = new TokenDIFFICULTY(content);
                            break;
                        case "weather":
                            set = new TokenWEATHER(content);
                            break;
                        case "time":
                            set = new TokenTIME(content);
                            break;
                    }
                    if (set == null)
                        continue;

                    if (Program.DEBUG)
                        Console.WriteLine("Compile:\t{0}", set);
                    tokens.Add(set);

                }
                catch (NullReferenceException)
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
