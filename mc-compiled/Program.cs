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
        public static bool DEBUG = false;
        static void Help()
        {
            Console.Write("\nmc-compiled.exe --help\n");
            Console.Write("\nmc-compiled.exe --jsonbuilder\n");
            Console.Write("mc-compiled.exe <file> [-d] [--daemon]\n");
            Console.Write("\tCompile a .mcc file into the resulting .mcfunction files.\n\tIf the -jsonbuilder option is specified, the rawtext json builder is opened instead.\n\n");
            Console.Write("\tOptions:\n");
            Console.Write("\t  -d\tDebug information during compilation.\n");
            Console.Write("\t  --daemon\tInitialize to allow background compilation of the same file every time it is modified.\n");
            Console.Write("\t  --nopause\tDoes not wait for user input to close application.\n");
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
                if (args[i].Equals("-d"))
                    debug = true;
                if (args[i].Equals("--daemon"))
                {
                    daemon = true;
                    NO_PAUSE = true;
                }
                if (args[i].Equals("--nopause"))
                    NO_PAUSE = true;
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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Debug Enabled");
                Console.ForegroundColor = ConsoleColor.White;
            }
            if (daemon)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[DAEMON] WATCHING FILE: {file}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            DEBUG = debug;

            string folder = Path.GetDirectoryName(Path.GetFullPath(file));
            file = Path.GetFileName(file);
            Directory.SetCurrentDirectory(folder);

            if (daemon)
            {
                FileSystemWatcher watcher = new FileSystemWatcher(folder);
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Filter = Path.GetFileName(file);

                while(true)
                {
                    Console.Clear();
                    RunMCCompiled(file);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[daemon] listening for next update...");
                    Console.ForegroundColor = ConsoleColor.White;
                    watcher.WaitForChanged(WatcherChangeTypes.Changed);
                    System.Threading.Thread.Sleep(100);
                }
            }

            RunMCCompiled(file);
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
                int line = thrower.Line;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error has occurred during compilation:\n" +
                    $"\tLN{line} {thrower.ToString()}:\n\t\t{message}\n\nCompilation cannot be continued.");
                if(!NO_PAUSE)
                    Console.ReadLine();
                return;
            }
        }
    }
}