using mc_compiled.Commands;
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
        public const string MATH_TEMP = "_mcc_math";        // Used for multistep scoreboard operations.
        public const string MATH_TEMP2 = "_mcc_math2";      // Used for multistep scoreboard operations.
        public const string MATH_INVERTER = "_mcc_invert";  // Used for inverting block check results.
        public const string DECIMAL_UNIT = "_mcc_dec_unit"; // Unit for fixed-point decimal operations.
        public const string DECIMAL_SUB_CARRY = "dec_carry_";   // Prefix used for decimal subtraction carry functions.
        public const string GHOST_TAG = "_gst";             // Used for ghost armor stands.

        private readonly List<string> createdTemplates = new List<string>();
        public bool HasCreatedTemplate(string templateName) => createdTemplates.Contains(templateName);
        public void CreateTemplate(string name, string[] code, bool file = false)
        {
            if (HasCreatedTemplate(name))
                return;

            createdTemplates.Add(name);

            if (!file)
                for (int i = code.Length - 1; i >= 0; i--)
                    AddLineTop(code[i]);
            else
            {
                List<string> nfile = new List<string>(code);
                functionsToBeWritten.Add(new MCFunction(name, null, nfile));
            }
        }

        public int currentMacroHash = 0;

        public readonly bool debug;
        public readonly Dictionary<string, Dynamic> ppv;
        public readonly Dictionary<string, Macro> macros;
        public readonly TokenFeeder tokens;

        public Stack<Selector> selection;
        public ValueManager values;

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
        internal List<FunctionDefinition> functionsDefined = new List<FunctionDefinition>();
        internal Stack<string> fileOffset;

        public string projectName = "DefaultProject";
        public string projectDesc = "Change with 'SETPROJECT DESC Your Text'";

        
        public string baseFileName;                // The base file name for all the functions.
        List<string> currentFile;           // The lines of the current file being written to.

        public string FileOffset
        {
            get
            {
                return fileOffset.Peek();
            }
        }

        StringBuilder addLineBuffer = new StringBuilder();
        /// <summary>
        /// Finish the current line and append it to the file.
        /// </summary>
        /// <param name="line"></param>
        public void FinishRaw(string line, bool modifyBuffer = true)
        {
            if (modifyBuffer)
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

        public void NewFileOffset(string fileOffset)
        {
            // THIS IS OBSOLETE
            if(currentFile.Count > 0)
            {
                functionsToBeWritten.Add(new MCFunction(baseFileName, FileOffset, currentFile));
                currentFile.Clear();
            }

            if (string.IsNullOrWhiteSpace(fileOffset))
                fileOffset = "";

            if (functionsToBeWritten.Any(mcf => mcf.fileOffset == fileOffset ||
                (mcf.fileOffset != null && mcf.fileOffset.Equals(fileOffset))))
            {
                MCFunction first = functionsToBeWritten.First(mcf => mcf.fileOffset == fileOffset ||
                    (mcf.fileOffset != null && mcf.fileOffset.Equals(fileOffset)));
                currentFile.AddRange(first.content);
            }
            else
            {
                this.fileOffset.Pop();
                this.fileOffset.Push(fileOffset);
            }
        }
        public void ApplyCurrentFile()
        {
            if (currentFile.Count > 0)
            {
                functionsToBeWritten.Add(new MCFunction(baseFileName, FileOffset, currentFile));
                currentFile.Clear();
            }
        }
        public void PushFileOffset(string fileOffset)
        {
            if (functionsToBeWritten.Any(mcf => mcf.fileOffset == fileOffset ||
                (mcf.fileOffset != null && mcf.fileOffset.Equals(fileOffset))))
            {
                MCFunction first = functionsToBeWritten.First(mcf => mcf.fileOffset == fileOffset ||
                    (mcf.fileOffset != null && mcf.fileOffset.Equals(fileOffset)));
                currentFile.AddRange(first.content);
            }

            this.fileOffset.Push(fileOffset);
        }
        public string PopFileOffset()
        {
            return fileOffset.Pop();
        }

        /// <summary>
        /// Replace preprocessor variables in a piece code with their respective values.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ReplacePPV(string input)
        {
            var all = ppv.AsEnumerable();

            foreach (var entry in all)
            {
                switch (entry.Value.alt)
                {
                    case Dynamic.AltType.NONE:
                        input = input.Replace('$' + entry.Key, entry.Value.data.s);
                        break;
                    case Dynamic.AltType.VECTOR:
                        input = input.Replace('$' + entry.Key, $"@e[type=armor_stand,name=\"{GHOST_TAG}{entry.Value.data.altData}\"");
                        break;
                    default:
                        break;
                }
            }
            return input;
        }
        public bool HasPPV(string name)
        {
            if (name.StartsWith("$"))
                name = name.Substring(1);
            return ppv.ContainsKey(name);
        }
        public bool TryGetPPV(string name, out Dynamic value)
        {
            if (name.StartsWith("$"))
                name = name.Substring(1);
            return ppv.TryGetValue(name, out value);
        }
        public Executor(Token[] tokens, bool debug, string baseFileName)
        {
            this.debug = debug; 
            this.baseFileName = baseFileName;
            projectName = baseFileName;
            currentFile = new List<string>();
            macros = new Dictionary<string, Macro>();
            fileOffset = new Stack<string>();
            fileOffset.Push("");

            ppv = new Dictionary<string, Dynamic>();
            selection = new Stack<Selector>();
            values = new ValueManager();

            selection.Push(new Selector()
            {
                core = Selector.Core.s
            });

            this.tokens = new TokenFeeder(tokens);

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
            functionsToBeWritten.Add(new MCFunction(baseFileName, FileOffset, currentFile));
            currentFile.Clear();
        }
        /// <summary>
        /// Run a set of tokens.
        /// </summary>
        /// <param name="tokens"></param>
        public void RunSection(TokenFeeder tokens)
        {
            Token token = null;
            try
            {
                while(tokens.HasNext())
                {
                    token = tokens.Next();

                    if(selection.Count > 1)
                        SetRaw(selection.Peek().GetAsPrefix());

                    token.Execute(this, tokens);
                }
            } catch(TokenException texc)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("EXECUTION ERROR:\n" +
                    $"\tLINE: {texc.token.line}\n" +
                    $"\tCOMPILED AS: {token}\n" +
                    $"\tDESCRIPTION: {texc.desc}\n");
                Console.ReadLine();
                Environment.Exit(0);
                return;
            } catch(Exception exc)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("INTERNAL EXECUTION ERROR:\n" +
                    $"\tTOKEN WAS: {token}\n" +
                    $"\tMESSAGE: {exc.Message}\n" +
                    $"\tDESCRIPTION: {exc}\n");
                Console.ReadLine();
                Environment.Exit(0);
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
