﻿using mc_compiled.Commands;
using mc_compiled.Commands.Native;
using mc_compiled.Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Runs a set of compiled tokens.
    /// </summary>
    public class Executor
    {
        public const double MCC_VERSION = 1.0;
        public static string MINECRAFT_VERSION = "1.17.10";

        // Command Related
        public const string NAME_ARITHMETIC = "_mcc_math";      // Used for multistep scoreboard operations 
        public const string NAME_GHOSTTAG = "_ghost";           // Used for ghost armor stands.
        public const string NAME_INVERTER = "_mcc_block_inv";   // Used for inverting block check results.

        public int currentMacroHash = 0;

        public readonly bool debug;
        public readonly Dictionary<string, Dynamic> ppv;
        public readonly Dictionary<string, Macro> macros;
        public readonly Token[] tokens;

        public Stack<Selector> selection;
        public List<string> valueCache;
        public bool hasCreatedMath = false;         // NAME_ARITHMETIC
        public bool[] hasCreatedInv = new bool[99]; // NAME_INVERTER

        /// <summary>
        /// Clone and return a new selector which can be written to, preserving the lower scope's selection stack.
        /// </summary>
        /// <returns>A new selector which can be freely written to and retains the exact properties of the lower scope's selector.</returns>
        public Selector PushSelectionStack()
        {
            Selector old = selection.Peek();
            Selector clone = new Selector(old);
            selection.Push(clone);
            return clone;
        }
        /// <summary>
        /// Return the selection stack to one scope level lower.
        /// </summary>
        public void PopSelectionStack()
        {
            selection.Pop();
        }

        int _reader;
        public int ReaderLocation
        {
            get { return _reader; }
            private set { _reader = value; }
        }
        public bool TargetPositionAligned
        {
            get
            {
                return selection.Count > 1;
            }
        }
        public Selector.Core SelectionReference
        {
            get
            {
                Selector.Core core = selection.Peek().core;
                if(TargetPositionAligned)
                {
                    if (core == Selector.Core.e || core == Selector.Core.a)
                        return Selector.Core.s;
                }
                return core;
            }
        }

        internal List<MCFunction> functionsToBeWritten = new List<MCFunction>();
        internal List<Tuple<string, ItemStack>> itemsToBeWritten = new List<Tuple<string, ItemStack>>();

        public string projectName = "DefaultProject";
        public string projectDesc = "Change with 'SETPROJECT DESC Your Text'";

        string baseFileName;        // The base file name for all the functions.
        string fileOffset;          // The offset appended after baseFileName if not null.
        string folder = null;       // (obsolete) The folder which output will be sent into.
        List<string> currentFile;   // The current file being written to.

        StringBuilder addLineBuffer = new StringBuilder();
        /// <summary>
        /// Finish the current line and append it to the file.
        /// </summary>
        /// <param name="line"></param>
        public void FinishRaw(string line, bool useBuffer = true)
        {
            if (useBuffer)
            {
                addLineBuffer.Append(line);
                currentFile?.Add(addLineBuffer.ToString());
                addLineBuffer.Clear();
            } else
            {
                currentFile?.Add(addLineBuffer.ToString() + line);
            }
        }
        /// <summary>
        /// Sets the text in the current line but doesn't finish it.
        /// </summary>
        /// <param name="text"></param>
        public void SetRaw(string text)
        {
            if (addLineBuffer == null)
                addLineBuffer = new StringBuilder(text);
            else
            {
                addLineBuffer.Clear();
                addLineBuffer.Append(text);
            }
        }
        /// <summary>
        /// Add a line to the top of the file.
        /// </summary>
        /// <param name="line"></param>
        public void AddLineTop(string line)
        {
            currentFile?.Insert(0, line);
        }

        /// <summary>
        /// Apply the current file code and set the file offset to something new.
        /// </summary>
        /// <param name="fileOffset"></param>
        public void NewFileOffset(string fileOffset)
        {
            if(currentFile.Count > 0)
            {
                functionsToBeWritten.Add(new MCFunction(baseFileName, this.fileOffset, currentFile));
                currentFile.Clear();
            }

            if (string.IsNullOrWhiteSpace(fileOffset))
                fileOffset = null;

            if (functionsToBeWritten.Any(mcf => mcf.fileOffset == fileOffset ||
                (mcf.fileOffset != null && mcf.fileOffset.Equals(fileOffset))))
            {
                MCFunction first = functionsToBeWritten.First(mcf => mcf.fileOffset == fileOffset ||
                    (mcf.fileOffset != null && mcf.fileOffset.Equals(fileOffset)));
                currentFile.AddRange(first.content);
            } else
                this.fileOffset = fileOffset;
        }
        /// <summary>
        /// Set the folder which the file will be output to (unused)
        /// </summary>
        /// <param name="folder"></param>
        public void SetFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                this.folder = null;
            else
                this.folder = folder;
        }
        /// <summary>
        /// Replace preprocessor variables in a piece code with their respective values.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ReplacePPV(string input)
        {
            foreach (var entry in ppv.AsEnumerable())
                input = input.Replace(entry.Key, entry.Value.data.s);
            return input;
        }
        public Executor(Token[] tokens, bool debug, string baseFileName)
        {
            this.debug = debug; 
            this.baseFileName = baseFileName;
            projectName = baseFileName;
            currentFile = new List<string>();
            macros = new Dictionary<string, Macro>();
            fileOffset = null;

            ppv = new Dictionary<string, Dynamic>();
            selection = new Stack<Selector>();
            valueCache = new List<string>();

            selection.Push(new Selector()
            {
                core = Selector.Core.s
            });

            this.tokens = tokens;

            ppv["_compilerversion"] = new Dynamic(MCC_VERSION);
            ppv["_mcversion"] = new Dynamic(MCC_VERSION);
            ppv["_lines"] = new Dynamic(tokens.Length);
        }

        /// <summary>
        /// Run the entire file from start to finish.
        /// </summary>
        public void Run()
        {
            RunSection(tokens);
            functionsToBeWritten.Add(new MCFunction(baseFileName, fileOffset, currentFile));
            currentFile.Clear();
        }
        /// <summary>
        /// Run a set of tokens.
        /// </summary>
        /// <param name="tokens"></param>
        public void RunSection(Token[] tokens)
        {
            try
            {
                for(int i = 0; i < tokens.Length; i++)
                {
                    Token token = tokens[i];

                    if (token is TokenPPMACRO && i + 1 < tokens.Length)
                    {
                        Token next = tokens[i + 1];
                        if (next is TokenBlock)
                        {
                            i++;
                            // This is a macro definition
                            TokenPPMACRO ppm = token as TokenPPMACRO;
                            TokenBlock block = next as TokenBlock;
                            Macro macro = new Macro(ppm.name, ppm.args, block.contents);

                            if (debug)
                                Console.WriteLine("Defined macro '{0}' with {1} argument(s) and {2} statements inside.",
                                    ppm.name, ppm.args.Length, block.contents.Length);

                            macros.Add(ppm.name.ToUpper(), macro);
                            continue;
                        }
                    }

                    // Set up prefix for this statement.
                    if(selection.Count > 1)
                        SetRaw(selection.Peek().GetAsPrefix());

                    // The big man execute. For block requiring statements this will setup and resolve their contents.
                    token.Execute(this);

                    if(token is TokenIF && i + 1 < tokens.Length)
                    {
                        Token next = tokens[++i];
                        if (!(next is TokenBlock))
                            throw new TokenException(token, "No block after IF statement.");
                        (next as TokenBlock).Execute(this);
                        PopSelectionStack();

                        if (i + 1 < tokens.Length && (next = tokens[i + 1]) is TokenELSE)
                        {
                            i++;
                            next = tokens[++i];
                            if (!(next is TokenBlock))
                                throw new TokenException(token, "No block after PPELSE statement.");
                            TokenIF tokenIf = token as TokenIF;
                            tokenIf.forceInvert = true;
                            tokenIf.Execute(this);
                            tokenIf.forceInvert = false;
                            (next as TokenBlock).Execute(this);
                            PopSelectionStack();
                            continue;
                        }  
                    } else if (token is TokenPPIF && i + 1 < tokens.Length)
                    {
                        bool result = (token as TokenPPIF).output;
                        Token next = tokens[++i];
                        if (!(next is TokenBlock))
                            throw new TokenException(token, "No block after PPIF statement.");

                        if (result)
                            (next as TokenBlock).Execute(this);

                        if(i + 1 < tokens.Length)
                        {
                            next = tokens[i + 1];
                            if (next is TokenPPELSE)
                            {
                                i++; next = tokens[++i];
                                if (!(next is TokenBlock))
                                    throw new TokenException(token, "No block after PPELSE statement.");
                                if(!result)
                                    (next as TokenBlock).Execute(this);
                                continue;
                            }
                        }
                        continue;
                    } else if (token is TokenPPREP && i + 1 < tokens.Length)
                    {
                        int repetitions = (token as TokenPPREP).output;
                        Token next = tokens[++i];
                        if (!(next is TokenBlock))
                            throw new TokenException(token, "No block after PPREP statement.");

                        for (int r = 0; r < repetitions; r++)
                            (next as TokenBlock).Execute(this);
                    }
                }
            } catch(TokenException texc)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("EXECUTION ERROR:\n" +
                    $"\tLINE {texc.token.line}\n" +
                    $"\tDESCRIPTION: {texc.desc}\n");
                return;
            } catch(Exception exc)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("GENERIC EXECUTION ERROR:\n" +
                    $"\tMESSAGE: {exc.Message}\n" +
                    $"\tDESCRIPTION: {exc}\n");
                return;
            }

        }
        /// <summary>
        /// Get the compiled files after execution.
        /// </summary>
        /// <returns></returns>
        public MCFunction[] GetFiles()
        {
            if (functionsToBeWritten == null || functionsToBeWritten.Count < 1)
                return new MCFunction[0];

            return functionsToBeWritten.ToArray();
        }
        /// <summary>
        /// Get this executor's results as a proper BehaviorPack.
        /// </summary>
        /// <returns></returns>
        public BehaviorPack GetAsPack()
        {
            BehaviorPack pack = new BehaviorPack()
            {
                packName = projectName,
                manifest = new Manifest(projectName, projectDesc),
                functions = GetFiles(),
                structures = null // Support not implemented yet
            };

            return pack;
        }
        /// <summary>
        /// Get the custom item drops defined during execution.
        /// </summary>
        /// <returns></returns>
        public Tuple<string, ItemStack>[] GetItemDefinitions()
        {
            if (itemsToBeWritten == null || itemsToBeWritten.Count < 1)
                return new Tuple<string, ItemStack>[0];

            return itemsToBeWritten.ToArray();
        }
    }
}
