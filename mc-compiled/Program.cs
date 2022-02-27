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
            Console.Write("\tShow the help menu for this application.\n\n");
            Console.Write("mc-compiled.exe --jsonbuilder\n");
            Console.Write("\tOpen a user-interface to build JSON rawtext.\n\n");
            Console.Write("mc-compiled.exe --manifest <projectName>\n");
            Console.Write("\tGenerate a behavior pack manifest with valid GUIDs.\n\n");
            Console.Write("mc-compiled.exe <file> [options...]\n");
            Console.Write("\tCompile a .mcc file into the resulting .mcfunction files.\n\n");
            Console.Write("\tOptions:\n");
            Console.Write("\t  -b | --basic\t\tOnly output function/structure files. No behavior pack data.\n");
            Console.Write("\t  -dm | --daemon\tInitialize to allow background compilation of the same file every time it is modified.\n");
            Console.Write("\t  -db | --debug\t\tDebug information during compilation.\n");
            Console.Write("\t  -dc | --decorate\tDecorate the compiled file with original source code (is a bit broken).\n");
            Console.Write("\t  -np | --nopause\tDoes not wait for user input to close application.\n");
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

            for (int i = 1; i < args.Length; i++)
            {
                string word = args[i].ToUpper();
                switch(word)
                {
                    case "--DEBUG":
                    case "-DB":
                        debug = true;
                        break;
                    case "--NOPAUSE":
                    case "-NP":
                        NO_PAUSE = true;
                        break;
                    case "--DECORATE":
                    case "-DC":
                        DECORATE = true;
                        break;
                    case "--BASIC":
                    case "-B":
                        BASIC_OUTPUT = true;
                        break;
                    case "--DAEMON":
                    case "-DM":
                        daemon = true;
                        NO_PAUSE = true;
                        break;
                }
            }
            if (file.ToUpper().Equals("--JSONBUILDER"))
            {
                new Definitions(debug);
                RawTextJsonBuilder builder = new RawTextJsonBuilder();
                builder.ConsoleInterface();
                return;
            }
            if (file.ToUpper().Equals("--MANIFEST"))
            {
                string rest = string.Join(" ", args).Substring(11);
                Manifest manifest = new Manifest(OutputLocation.BEHAVIORS, Guid.NewGuid(), rest, "TODO set description")
                    .WithModule(Manifest.Module.BehaviorData(rest));
                File.WriteAllBytes("manifest.json", manifest.GetOutputData());
                Console.WriteLine("Wrote a new 'manifest.json' to current directory.");
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

            // initialize definitions resolver
            new Definitions(debug);
            // initialize enum constants
            Commands.CommandEnumParser.Init();

            // set working directory
            string rootFolder = Path.GetDirectoryName(Path.GetFullPath(file));
            Directory.SetCurrentDirectory(rootFolder);
            string projectName = Path.GetFileNameWithoutExtension(file);
            string projectBehaviorsFolder = Path.Combine("development_behavior_packs", projectName);
            string projectResourcesFolder = Path.Combine("development_resource_packs", projectName);
            Directory.CreateDirectory(projectBehaviorsFolder);
            Directory.CreateDirectory(projectResourcesFolder);
            file = Path.GetFileName(file);

            bool firstRun = true;
            if (daemon)
            {
                FileSystemWatcher watcher = new FileSystemWatcher(rootFolder);
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Filter = $"{projectName}.mcc";

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
            Executor.ResetGeneratedFile();
            Commands.Command.ResetState();
            Tokenizer.CURRENT_LINE = 0;
            DirectiveImplementations.ResetState();

            // clean/create output folder
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "development_behavior_packs", projectName);
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(folder, "*.mcstructure", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(folder, "*.mcfunction", SearchOption.AllDirectories));

            foreach (string file in files)
                File.Delete(file);
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