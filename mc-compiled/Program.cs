using mc_compiled.Commands.Native;
using mc_compiled.Json;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding;
using mc_compiled.NBT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled
{
    class Program
    {
        public static bool NO_PAUSE = false;
        public static bool DECORATE = false;
        public static bool DEBUG = false;
        public static bool BASIC_OUTPUT = false;
        static void Help()
        {
            Console.Write("\nmc-compiled.exe --help\n");
            Console.Write("\nmc-compiled.exe --jsonbuilder\n");
            Console.Write("mc-compiled.exe <file> [--debug] [--daemon] [--nopause] [--decorate]\n");
            Console.Write("\tCompile a .mcc file into the resulting .mcfunction files.\n\tIf the -jsonbuilder option is specified, the rawtext json builder is opened instead.\n\n");
            Console.Write("\tOptions:\n");
            Console.Write("\t  --debug\tDebug information during compilation.\n");
            Console.Write("\t  --daemon\tInitialize to allow background compilation of the same file every time it is modified.\n");
            Console.Write("\t  --nopause\tDoes not wait for user input to close application.\n");
            Console.Write("\t  --decorate\tDecorate the compiled file with original source code (is a bit broken).\n");
            Console.Write("\t  --basic\tOutput raw files rather than structuring a behavior pack.\n");
        }
        [STAThread]
        static void Main(string[] args)
        {
            if(args.Length < 1 || args[0].Equals("--help"))
            {
                Help();
                return;
            }

            string file = args[0];
            bool debug = false;
            bool daemon = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--debug"))
                    debug = true;
                if (args[i].Equals("--nopause"))
                    NO_PAUSE = true;
                if (args[i].Equals("--decorate"))
                    DECORATE = true;
                if (args[i].Equals("--basic"))
                    BASIC_OUTPUT = true;
                if (args[i].Equals("--daemon"))
                {
                    daemon = true;
                    NO_PAUSE = true;
                }
            }

            new Definitions(debug);

            if (file.ToUpper().Equals("--JSONBUILDER"))
            {
                RawTextJsonBuilder builder = new RawTextJsonBuilder();
                builder.ConsoleInterface();
                return;
            }

            if (debug)
            {
                DEBUG = true;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Debug Enabled");
                Console.ForegroundColor = ConsoleColor.White;
            }
            if (daemon)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[daemon] watching file: {file}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // initialize enum constants
            Commands.CommandEnumParser.Init();

            string folder = Path.GetDirectoryName(Path.GetFullPath(file));
            string projectName = Path.GetFileNameWithoutExtension(file);
            file = Path.GetFileName(file);
            Directory.SetCurrentDirectory(folder);

            bool firstRun = true;
            if (daemon)
            {
                FileSystemWatcher watcher = new FileSystemWatcher(folder);
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Filter = Path.GetFileName(file);

                while(true)
                {
                    if (firstRun)
                        firstRun = false;
                    else
                        Console.Clear();

                    PrepareToCompile(projectName);
                    RunMCCompiled(file);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[daemon] listening for next update...");
                    Console.ForegroundColor = ConsoleColor.White;
                    watcher.WaitForChanged(WatcherChangeTypes.Changed);
                    System.Threading.Thread.Sleep(100);
                }
            }

            PrepareToCompile(projectName);
            RunMCCompiled(file);
        }
        public static void PrepareToCompile(string projectName)
        {
            // reset all that icky static stuff
            StatementOpenBlock.ResetBranchFile();
            Commands.Command.ResetState();
            Tokenizer.CURRENT_LINE = 0;
            DirectiveImplementations.ResetState();

            // clean output folder
            string folder = projectName + "/";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            /*if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (file.EndsWith("manifest.json"))
                        continue;
                    File.Delete(file);
                }
            } else
            {
                Directory.CreateDirectory(folder);
            }*/
        }
        public static void RunMCCompiled(string file)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Token[] tokens = Tokenizer.TokenizeFile(file);

                if (DEBUG)
                {
                    Console.WriteLine("\tA detailed overview of the tokenization results follows:");
                    Console.WriteLine(string.Join("", from t in tokens select t.DebugString()));
                    Console.WriteLine();
                    Console.WriteLine("\tReconstruction of the processed code through tokens:");
                    Console.WriteLine(string.Join(" ", from t in tokens select t.AsString()));
                    Console.WriteLine();
                }

                Statement[] statements = Assembler.AssembleTokens(tokens);

                if (DEBUG)
                {
                    Console.WriteLine("\tThe overview of assembled statements is as follows:");
                    Console.WriteLine(string.Join("\n", from s in statements select s.ToString()));
                    Console.WriteLine();
                }

                Executor executor = new Executor(statements, Path.GetFileNameWithoutExtension(file));
                executor.Execute();

                Console.WriteLine("Writing files...");
                executor.WriteAllFiles();
                stopwatch.Stop();

                Console.WriteLine($"Completed in {stopwatch.Elapsed.TotalSeconds} seconds.");

                if (!NO_PAUSE)
                    Console.ReadLine();
            }
            catch (TokenizerException exc)
            {
                int line = exc.line;
                string message = exc.Message;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem encountered during tokenization of file:\n" +
                    $"\tLINE {line}: {message}\n\nTokenization cannot be continued.");
                if (!NO_PAUSE)
                    Console.ReadLine();
                return;
            }
            catch (StatementException exc)
            {
                Statement thrower = exc.statement;
                string message = exc.Message;
                int _line = thrower.Line;
                string line = _line == -1 ? "??" : _line.ToString();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error has occurred during compilation:\n" +
                    $"\tLINE {line} {thrower.ToString()}:\n\t\t{message}\n\nCompilation cannot be continued.");
                if(!NO_PAUSE)
                    Console.ReadLine();
                return;
            }
        }
    }
}