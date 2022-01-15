using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// The compiler which processes the main MCCompiled code.
    /// </summary>
    class DefaultCompiler : Compiler
    {
        public override LegacyToken[] Compile(CodeReader code)
        {
            // Initialize buffer
            List<LegacyToken> tokens = new List<LegacyToken>();

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
                    tokens.Add(new LegacyTokenComment(line.Substring(2).Trim()));
                    continue;
                }
                if (line.StartsWith("{"))
                {
                    string block = code.GetBlock('{', '}');
                    LegacyToken[] sub = Compile(new CodeReader(block));
                    tokens.Add(new LegacyTokenBlock(sub));
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
                    LegacyToken shallow = new LegacyTokenVALUE(line);
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
                    LegacyToken shallow = new LegacyTokenCALL(functionName, functionArgs.Split(' '));
                    if (Program.DEBUG)
                        Console.WriteLine("Compile:\t{0}", shallow);
                    tokens.Add(shallow);
                    continue;
                }

                try
                {
                    LegacyToken set = null;
                    switch (keyword.ToLower())
                    {
                        case "ppv":
                            set = new LegacyTokenPPV(content);
                            guessedPPValues.Add((set as LegacyTokenPPV).name);
                            break;
                        case "ppinc":
                            set = new LegacyTokenPPINC(content);
                            break;
                        case "ppdec":
                            set = new LegacyTokenPPDEC(content);
                            break;
                        case "ppadd":
                            set = new LegacyTokenPPADD(content);
                            break;
                        case "ppsub":
                            set = new LegacyTokenPPSUB(content);
                            break;
                        case "ppmul":
                            set = new LegacyTokenPPMUL(content);
                            break;
                        case "ppdiv":
                            set = new LegacyTokenPPDIV(content);
                            break;
                        case "ppmod":
                            set = new LegacyTokenPPMOD(content);
                            break;
                        case "ppif":
                            set = new LegacyTokenPPIF(content);
                            break;
                        case "ppelse":
                            set = new LegacyTokenPPELSE();
                            break;
                        case "pprep":
                            set = new LegacyTokenPPREP(content);
                            break;
                        case "pplog":
                            set = new LegacyTokenPPLOG(content);
                            break;
                        case "_ppfile": // no longer used but people can if they want
                            set = new LegacyTokenPPFILE(content);
                            break;
                        case "function":
                            set = new LegacyTokenFUNCTION(content);
                            break;
                        case "call":
                            set = new LegacyTokenCALL(content);
                            break;
                        case "ppmacro":
                            set = new LegacyTokenPPMACRO(content);
                            break;
                        case "ppfriendly":
                            set = new LegacyTokenPPFRIENDLY(content);
                            break;
                        case "ppupper":
                            set = new LegacyTokenPPUPPER(content);
                            break;
                        case "pplower":
                            set = new LegacyTokenPPLOWER(content);
                            break;


                        case "mc":
                            set = new LegacyTokenMC(content);
                            break;
                        case "select":
                            set = new LegacyTokenSELECT(content);
                            break;
                        case "print":
                            set = new LegacyTokenPRINT(content);
                            break;
                        case "printp":
                            set = new LegacyTokenPRINTP(content);
                            break;
                        case "limit":
                            set = new LegacyTokenLIMIT(content);
                            break;
                        case "def":
                        case "define":
                            set = new LegacyTokenDEFINE(content);
                            guessedValues.Add((set as LegacyTokenDEFINE).ValueName);
                            break;
                        case "init":
                        case "initialize":
                            set = new LegacyTokenINITIALIZE(content);
                            break;
                        case "value":
                        case "val":
                            set = new LegacyTokenVALUE(content);
                            break;
                        case "if":
                            set = new LegacyTokenIF(content);
                            break;
                        case "else":
                        case "el":
                            set = new LegacyTokenELSE();
                            break;
                        case "give":
                            set = new LegacyTokenGIVE(content);
                            break;
                        case "tp":
                            set = new LegacyTokenTP(content);
                            break;
                        case "title":
                            set = new LegacyTokenTITLE(content);
                            break;
                        case "move":
                            set = new LegacyTokenMOVE(content);
                            break;
                        case "face":
                            set = new LegacyTokenFACE(content);
                            break;
                        case "place":
                            set = new LegacyTokenPLACE(content);
                            break;
                        case "fill":
                            set = new LegacyTokenFILL(content);
                            break;
                        case "kick":
                            set = new LegacyTokenKICK(content);
                            break;
                        case "halt":
                        case "stop":
                            set = new LegacyTokenHALT();
                            break;
                        case "gm":
                        case "gamemode":
                            set = new LegacyTokenGAMEMODE(content);
                            break;
                        case "diff":
                        case "difficulty":
                            set = new LegacyTokenDIFFICULTY(content);
                            break;
                        case "weather":
                            set = new LegacyTokenWEATHER(content);
                            break;
                        case "time":
                            set = new LegacyTokenTIME(content);
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
