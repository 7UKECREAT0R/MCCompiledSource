using mc_compiled.Commands.Native;
using mc_compiled.Json;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.ServerWebSocket;
using mc_compiled.MCC.SyntaxHighlighting;
using mc_compiled.Modding;
using mc_compiled.Modding.Behaviors;
using mc_compiled.NBT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
// ReSharper disable CommentTypo

namespace mc_compiled
{
    internal static class Program
    {
        private const string APP_ID = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
        
        private static bool NO_PAUSE;
        private static bool REGOLITH;

        public static bool DECORATE;
        public static bool EXPORT_ALL;
        public static bool DEBUG;

        private static void Help()
        {
            Console.Write("\nmc-compiled.exe --help\n");
            Console.Write("\tShow the help menu for this application.\n\n");
            Console.Write("\nmc-compiled.exe --version\n");
            Console.Write("\tView this MCCompiled version. Accessed through code with $compilerversion\n\n");
            Console.Write("mc-compiled.exe --manifest <projectName>\n");
            Console.Write("\tGenerate a behavior pack manifest with valid GUIDs.\n\n");
            Console.Write("mc-compiled.exe --search [options...]\n");
            Console.Write("\tSearch for and compile MCC files in all subdirectories.\n\n");
            Console.Write("mc-compiled.exe --syntax [exporter...]\n");
            Console.Write("\tExport language information into a file. Not specifying an exporter will list them off.\n\n");
            Console.Write("mc-compiled.exe <file> [options...]\n");
            Console.Write("\tCompile a .mcc file into the resulting .mcfunction files.\n");
            Console.Write("\t  [-dm | --daemon]\t\t\tInitialize to allow background compilation of the same file every time it is modified.\n");
            Console.Write("\t  [-db | --debug]\t\t\tDebug information during compilation. Hits compilation time for large projects.\n");
            Console.Write("\t  [-dc | --decorate]\t\t\tDecorate the compiled file with original source code and extra helpful information for pathing, debugging, etc...\n");
            Console.Write("\t  [-ea | --export_all]\t\t\tExports all functions, even if they are unused. Use the `export` attribute to mark individual functions for export.\n");
            Console.Write("\t  [-np | --nopause]\t\t\tDoes not wait for user input to close application.\n");
            Console.Write("\t  [-obp | --outputbp] <directory>\tOutput behaviors to a specific directory. Use ?project to denote project name.\n");
            Console.Write("\t  [-orp | --outputrp] <directory>\tOutput resources to a specific directory. Use ?project to denote project name.\n");
            Console.Write("\t  [-od | --outputdevelopment]\t\tOutput files to the com.mojang development_x_packs directory.\n");
            Console.Write("\t  [-p | --project] <name>\t\tRun the compilation with the given project name. Defaults to using the input file's name.\n");
            Console.Write("\t  [-ppv | --variable] <name> <value>\tRun the compilation with the given preprocessor variable already set.\n");
            Console.Write("\t  [--search]\t\t\t\tSearch for and compile all mcc files in the current directory and subdirectories.\n");
        }

        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length < 1 || args[0].Equals("--help"))
            {
                Help();
                return;
            }

            string[] files = { args[0] };
            var inputPPVs = new List<InputPPV>();
            bool debug = false;
            bool search = false;
            bool daemon = false;
            string obp = "?project_BP"; // ?project is the only part that changes.
            string orp = "?project_RP"; // ?project is the only part that changes.
            string projectName = null;

            for (int i = 0; i < args.Length; i++)
            {
                string word = args[i].ToUpper();
                switch (word)
                {
                    case "-EA":
                    case "--EXPORT_ALL":
                        EXPORT_ALL = true;
                        break;
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
                    case "--DAEMON":
                    case "-DM":
                        daemon = true;
                        NO_PAUSE = true;
                        break;
                    case "--OUTPUTBP":
                    case "-OBP":
                        obp = args[++i];
                        break;
                    case "--OUTPUTRP":
                    case "-ORP":
                        orp = args[++i];
                        break;
                    case "--OUTPUTDEVELOPMENT":
                    case "-OD":
                        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        string comMojang = Path.Combine(localAppData, "Packages", APP_ID, "LocalState", "games", "com.mojang");
                        obp = Path.Combine(comMojang, "development_behavior_packs") + "\\?project_BP";
                        orp = Path.Combine(comMojang, "development_resource_packs") + "\\?project_RP";
                        break;
                    case "--VARIABLE":
                    case "-PPV":
                        string ppvName = args[++i];
                        string ppvValue = args[++i];
                        inputPPVs.Add(new InputPPV(ppvName, ppvValue));
                        break;
                    case "--VERSION":
                        Console.WriteLine("MCCompiled Version " + Executor.MCC_VERSION);
                        Console.WriteLine("Andrew L. Criswell II, 2023");
                        return;
                    case "--PROJECT":
                    case "-P":
                        projectName = args[++i];
                        break;
                }
            }

            // load enums and directives
            Commands.CommandEnumParser.Init();
            Directives.LoadFromLanguage(debug);

            string fileUpper = files[0].ToUpper();

            if (fileUpper.Equals("--SYNTAX"))
            {
                string _target = (args.Length == 1) ? null : args[1];

                if (_target == "*")
                {
                    ConsoleColor color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Exporting all output targets...");
                    Console.ForegroundColor = ConsoleColor.White;

                    foreach (KeyValuePair<string, SyntaxTarget> st in Syntax.syntaxTargets)
                    {
                        string stOutput = st.Value.GetFile();
                        Console.WriteLine("\tExporting syntax file for target '{0}'... ({1})", st.Key, stOutput);

                        using (FileStream outputStream = File.Open(stOutput, FileMode.Create))
                        using (TextWriter writer = new StreamWriter(outputStream))
                        {
                            st.Value.Write(writer);
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Completed exporting for {0} output targets.", Syntax.syntaxTargets.Count);
                    Console.ForegroundColor = color;
                    return;
                }

                SyntaxTarget target = null;
                if (_target != null)
                {
                    Console.WriteLine("Looking up target {0}...", _target);
                    target = Syntax.syntaxTargets[_target];
                }

                if (target == null)
                {
                    Console.WriteLine("Syntax Targets");
                    Console.WriteLine("\t*: All available output targets individually.");
                    foreach (KeyValuePair<string, SyntaxTarget> t in Syntax.syntaxTargets)
                        Console.WriteLine("\t{0}: {1}", t.Key, t.Value.Describe());
                    return;
                }

                string outputFile = target.GetFile();

                using (FileStream outputStream = File.Open(outputFile, FileMode.Create))
                using (TextWriter writer = new StreamWriter(outputStream))
                {
                    ConsoleColor color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Exporting syntax file for target '{0}'...", _target);
                    target.Write(writer);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Completed. Output file: {0}", outputFile);
                    Console.ForegroundColor = color;
                }
                return;
            }
            if (fileUpper.Equals("--SERVER"))
            {
                _ = new Definitions(debug);
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string comMojang = Path.Combine(localAppData, "Packages", APP_ID, "LocalState", "games", "com.mojang");
                obp = Path.Combine(comMojang, "development_behavior_packs") + "\\?project_BP";
                orp = Path.Combine(comMojang, "development_resource_packs") + "\\?project_RP";
                NO_PAUSE = true;

                DEBUG = debug;
                using (MCCServer server = new MCCServer(orp, obp))
                {
                    server.StartServer();
                }

                return;
            }
            if (fileUpper.Equals("--PROTOCOL"))
            {
                var config = new RegistryConfiguration();
                config.Install();
                return;
            }
            if (fileUpper.Equals("--PROTOCOL_REMOVE"))
            {
                var config = new RegistryConfiguration();
                config.Uninstall();
                return;
            }
            if (fileUpper.Equals("--INFO"))
            {
                Console.WriteLine("V{0}", Executor.MCC_VERSION);
                Console.WriteLine("L{0}", System.Reflection.Assembly.GetExecutingAssembly().Location);
                return;
            }
            if (fileUpper.Equals("--FROMPROTOCOL"))
            {
                // check for existing MCCompiled processes.
                Process[] mccProcesses = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                if (mccProcesses.Length > 1)
                    return;

                // okay... step up to the job.
                _ = new Definitions(debug);
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string comMojang = Path.Combine(localAppData, "Packages", APP_ID, "LocalState", "games", "com.mojang");
                obp = Path.Combine(comMojang, "development_behavior_packs") + "\\?project_BP";
                orp = Path.Combine(comMojang, "development_resource_packs") + "\\?project_RP";
                DECORATE = false;
                NO_PAUSE = true;
                DEBUG = debug;
                using (var server = new MCCServer(orp, obp))
                {
                    server.StartServer();
                }
                return;
            }
            if (fileUpper.Equals("--JSONBUILDER"))
            {
                _ = new Definitions(debug);
                var builder = new RawTextJsonBuilder();
                builder.ConsoleInterface();
                return;
            }
            if (fileUpper.Equals("--EMPTYSTRUCTURE"))
            {
                int size = int.Parse(args[1]);

                StructureFile empty = new StructureFile("empty", null, new StructureNBT()
                {
                    entities = new EntityListNBT(Array.Empty<EntityNBT>()),
                    worldOrigin = new VectorIntNBT(0, 0, 0),
                    size = new VectorIntNBT(size, size, size),
                    palette = new PaletteNBT(
                        new PaletteEntryNBT("air")
                    ),
                    indices = new BlockIndicesNBT(new int[size, size, size])
                });

                File.WriteAllBytes("empty.mcstructure", empty.GetOutputData());
                Console.WriteLine("Wrote empty structure to empty.mcstructure");
                return;
            }
            if (fileUpper.Equals("--TESTITEMS"))
            {
                _ = new Definitions(debug);
                /*ItemStack item = new ItemStack()
                {
                    id = "minecraft:stick",
                    count = 1,
                    displayName = "§bSuper Stick",
                    lore = new string[]
                    {
                        "This is the super stick.",
                        "§lHe knok bak"
                    },
                    enchantments = new EnchantmentEntry[]
                    {
                        new EnchantmentEntry(Commands.Enchantment.knockback, 50)
                    }
                };
                StructureNBT nbt = StructureNBT.SingleItem(item);
                StructureFile itemFile = new StructureFile("stick", "tests", nbt);
                File.WriteAllBytes("testitem0.mcstructure", itemFile.GetOutputData());*/

                var empty = new StructureFile("empty", null, new StructureNBT()
                {
                    entities = new EntityListNBT(Array.Empty<EntityNBT>()),
                    worldOrigin = new VectorIntNBT(0, 0, 0),
                    size = new VectorIntNBT(100, 100, 100),
                    palette = new PaletteNBT(
                        new PaletteEntryNBT("air")
                    ),
                    indices = new BlockIndicesNBT(new int[100, 100, 100])
                });


                /*item = new ItemStack()
                {
                    id = "minecraft:written_book",
                    count = 1,
                    bookData = new ItemTagBookData()
                    {
                        author = "lukecreator",
                        title = "The MCCompiled Wiki",
                        pages = new string[]
                        {
                            "AAAAAPErheikuhrfewughreeeee\n\n-end of page 1",
                            "WHWWwhoooahhhhahhh mc compiled whw\n\n-end of page 2",
                            "The End"
                        }
                    }
                };
                nbt = StructureNBT.SingleItem(item);
                itemFile = new StructureFile("testitem", "tests", nbt);*/
                File.WriteAllBytes("test.mcstructure", empty.GetOutputData());

                /*item = new ItemStack()
                {
                    id = "minecraft:leather_chestplate",
                    count = 1,
                    enchantments = new EnchantmentEntry[]
                    {
                        new EnchantmentEntry(Commands.Enchantment.protection, 20)
                    },
                    customColor = new ItemTagCustomColor()
                    {
                        r = 255,
                        g = 0,
                        b = 0
                    }
                };
                nbt = StructureNBT.SingleItem(item);
                itemFile = new StructureFile("testitem", "tests", nbt);
                File.WriteAllBytes("testitem2.mcstructure", itemFile.GetOutputData());*/

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Written test items to 'test.mcstructure'");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }
            if (fileUpper.Equals("--TESTLOOT"))
            {
                _ = new Definitions(debug);
                var table = new LootTable("test");
                table.pools.Add(new LootPool(6, new LootEntry[]
                {
                    new LootEntry(LootEntry.EntryType.item, "minecraft:iron_sword")
                        .WithFunction(new LootFunctionEnchant(new EnchantmentEntry("sharpness", 20)))
                        .WithFunction(new LootFunctionDurability(0.5f))
                        .WithFunction(new LootFunctionName("§lSuper Sword"))
                        .WithFunction(new LootFunctionLore(
                            "§cHi! This is a line of lore.",
                            "§6Here's another line.")),
                    new LootEntry(LootEntry.EntryType.item, "minecraft:book")
                        .WithFunction(new LootFunctionBook("Test Book", "lukecreator",
                            "yo welcome to the first page!\nSecond line.",
                            "Second page!")),
                    new LootEntry(LootEntry.EntryType.item, "minecraft:leather_chestplate")
                        .WithFunction(new LootFunctionName("Random Enchant"))
                        .WithFunction(new LootFunctionRandomEnchant(true))
                        .WithFunction(new LootFunctionRandomDye()),
                    new LootEntry(LootEntry.EntryType.item, "minecraft:leather_leggings")
                        .WithFunction(new LootFunctionName("Simulated Enchant"))
                        .WithFunction(new LootFunctionSimulateEnchant(20, 40)),
                    new LootEntry(LootEntry.EntryType.item, "minecraft:leather_boots")
                        .WithFunction(new LootFunctionName("Gear Enchant"))
                        .WithFunction(new LootFunctionRandomEnchantGear(1.0f)),
                    new LootEntry(LootEntry.EntryType.item, "minecraft:cooked_beef")
                        .WithFunction(new LootFunctionCount(2, 64))
                }));

                File.WriteAllBytes("testloot.json", table.GetOutputData());
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Written test table to 'testloot.json'");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }
            if (fileUpper.Equals("--MANIFEST"))
            {
                string rest = string.Join(" ", args).Substring(11);
                Manifest manifest = new Manifest(OutputLocation.b_ROOT, Guid.NewGuid(), rest, "TODO set description")
                    .WithModule(Manifest.Module.BehaviorData(rest));
                File.WriteAllBytes("manifest.json", manifest.GetOutputData());
                Console.WriteLine("Wrote a new 'manifest.json' to current directory.");
                return;
            }

            if (fileUpper.Equals("--SEARCH"))
            {
                REGOLITH = false;
                search = true;
                NO_PAUSE = true;
                files = GetMCCFilesInDirectory();

                if (files.Length == 0)
                {
                    Console.Error.WriteLine("No MCC files found.");
                    return;
                }
            }
            if (fileUpper.Equals("--REGOLITH"))
            {
                obp = "BP";
                orp = "RP";
                REGOLITH = true;
                search = true;
                NO_PAUSE = true;
                files = GetMCCFilesInDirectory();

                if (files.Length == 0)
                {
                    Console.Error.WriteLine("No MCC files found. Skipping filter.");
                    return;
                }
            }

            if (debug)
            {
                DEBUG = true;
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Debug Enabled");
                Console.ForegroundColor = oldColor;
            }

            if (daemon & !REGOLITH)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(search
                    ? $"[daemon] Watching directory: {Directory.GetCurrentDirectory()}"
                    : $"[daemon] Watching files: {string.Join("\n", files.Select(f => "\t- " + f))}");
                Console.ForegroundColor = oldColor;
            }

            // load definitions.def
            _ = new Definitions(debug);

            bool firstRun = true;
            if (daemon & !REGOLITH)
            {
                FileSystemWatcher watcher;
                if (search)
                {
                    watcher = new FileSystemWatcher(Directory.GetCurrentDirectory());
                    watcher.NotifyFilter = NotifyFilters.LastWrite;
                    watcher.Filter = "*.mcc";
                } else
                {
                    watcher = new FileSystemWatcher(Directory.GetCurrentDirectory());
                    watcher.NotifyFilter = NotifyFilters.LastWrite;
                    watcher.Filter = $"{Path.GetFileNameWithoutExtension(files[0])}.mcc";
                }

                Console.TreatControlCAsInput = true;
                string changedFile = null;

                while (true)
                {
                    if (firstRun)
                    {
                        PrepareToCompile();
                        firstRun = false;
                        foreach (string file in files)
                        {
                            CleanDirectory(obp, file);
                            CleanDirectory(orp, file);
                            RunMCCompiled(file, inputPPVs.ToArray(), obp, orp, projectName);
                        }
                    }
                    else
                    {
                        Console.Clear();
                        PrepareToCompile();
                        CleanDirectory(obp, changedFile);
                        CleanDirectory(orp, changedFile);
                        RunMCCompiled(changedFile, inputPPVs.ToArray(), obp, orp, projectName);
                    }

                    ConsoleColor oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[daemon] listening for next update...");
                    Console.ForegroundColor = oldColor;
                    while (true)
                    {
                        WaitForChangedResult e = watcher.WaitForChanged(WatcherChangeTypes.Changed, 100);

                        // flush stdin
                        while (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo key = Console.ReadKey();
                            if (key.Modifiers == ConsoleModifiers.Control &&
                                key.Key == ConsoleKey.C)
                                goto compileEnd;
                        }

                        if (e.TimedOut)
                            continue;
                        
                        changedFile = e.Name;
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
                }
                compileEnd:
                watcher.Dispose();
                return;
            }

            PrepareToCompile();

            if (REGOLITH)
            {
                foreach (string file in files)
                {
                    if (RunMCCompiled(file, inputPPVs.ToArray(), obp, orp, projectName))
                        File.Delete(file); // delete if compilation succeeded, otherwise might be another format
                }
            } else
            {
                foreach (string file in files)
                {
                    CleanDirectory(obp, file);
                    CleanDirectory(orp, file);
                    RunMCCompiled(file, inputPPVs.ToArray(), obp, orp, projectName);
                }
            }
        }
        internal static void PrepareToCompile()
        {
            // reset all that icky static stuff
            Executor.ResetGeneratedNames();
            Commands.Command.ResetState();
            Tokenizer.CURRENT_LINE = 0;
            DirectiveImplementations.ResetState();
        }

        private static readonly string[] CLEAN_FILTERS = new[]
        {
            "*.mcstructure",
            "*.mcfunction"
        };
        private static void CleanDirectory(string cleanFolder, string file)
        {
            cleanFolder = cleanFolder.Replace("?project", Path.GetFileNameWithoutExtension(file));

            if (!Directory.Exists(cleanFolder))
                return;
            
            var files = new List<string>();

            foreach (string filter in CLEAN_FILTERS)
                files.AddRange(Directory.GetFiles(cleanFolder, filter, SearchOption.AllDirectories));

            foreach (string del in files)
                File.Delete(del);
        }

        /// <summary>
        /// Compile a file with MCCompiled using the existing options.
        /// </summary>
        /// <param name="file">The path to the file that needs to be compiled.</param>
        /// <param name="preprocessorVariables">An array of preprocessor variables to be used during compilation.</param>
        /// <param name="outputBP">The path for the output behavior pack.</param>
        /// <param name="outputRP">The path for the output resource pack.</param>
        /// <param name="projectName">The name of the project being compiled. Default is null.</param>
        /// <param name="silentErrors">Determines whether error messages should be suppressed. Default is false.</param>
        /// <returns>Returns a boolean value indicating whether the compilation succeeded.</returns>
        private static bool RunMCCompiled(string file, InputPPV[] preprocessorVariables, string outputBP,
            string outputRP, string projectName = null, bool silentErrors = false)
        {
            string content = File.ReadAllText(file);
            return RunMCCompiledCode(content, file, preprocessorVariables, outputBP, outputRP, projectName, silentErrors);
        }

        /// <summary>
        /// Compile a file with MCCompiled using the existing options.
        /// </summary>
        /// <param name="code">The code to compile.</param>
        /// <param name="file">The file path of the code.</param>
        /// <param name="preprocessorVariables">An array of preprocessor variables.</param>
        /// <param name="outputBP">The base path for the output files.</param>
        /// <param name="outputRP">The relative path for the output files.</param>
        /// <param name="projectName">The name of the project. If null, the name is retrieved from the file path.</param>
        /// <param name="silentErrors">A flag indicating whether to suppress the errors during compilation. Default is false.</param>
        /// <returns>If the compilation succeeded.</returns>
        internal static bool RunMCCompiledCode(string code, string file, InputPPV[] preprocessorVariables,
            string outputBP, string outputRP, string projectName = null, bool silentErrors = false)
        {
            if (projectName == null)
                projectName = Path.GetFileNameWithoutExtension(file);

            outputBP = outputBP.Replace("?project", projectName);
            outputRP = outputRP.Replace("?project", projectName);

            try
            {
                var stopwatch = Stopwatch.StartNew();
                Token[] tokens = new Tokenizer(code).Tokenize();

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

                var executor = new Executor(statements, preprocessorVariables, projectName, outputBP, outputRP);
                executor.Execute();

                stopwatch.Stop();
                Console.WriteLine($"Compiled in {stopwatch.Elapsed.TotalSeconds} seconds.");

                Console.WriteLine("Writing files...");
                executor.project.WriteAllFiles();

                Console.WriteLine($"Completed.");

                if (!NO_PAUSE)
                    Console.ReadLine();

                return true;
            }
            catch (TokenizerException exc)
            {
                if (DEBUG && Debugger.IsAttached)
                    throw;
                if (silentErrors)
                    return false;

                int[] _lines = exc.lines;
                string lines = string.Join(", ", _lines.Select(line => line < 0 ? "??" : line.ToString()));
                string message = exc.Message;
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Problem encountered during tokenization of file:\n" +
                                        $"\t{Path.GetFileName(file)}:{lines} -- {message}\n\nTokenization cannot be continued.");
                Console.ForegroundColor = oldColor;
                if (!NO_PAUSE)
                    Console.ReadLine();
                return false;
            }
            catch (StatementException exc)
            {
                if (DEBUG && Debugger.IsAttached)
                    throw;
                if (silentErrors)
                    return false;

                Statement thrower = exc.statement;
                string message = exc.Message;
                int[] _lines = thrower.Lines;
                string lines = string.Join(", ", _lines.Select(line => line < 0 ? "??" : line.ToString()));

                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("An error has occurred during compilation:\n" +
                                        $"\t{Path.GetFileName(file)}:{lines} -- {thrower}:\n\t\t{message}\n\nCompilation cannot be continued.");
                Console.ForegroundColor = oldColor;
                if (!NO_PAUSE)
                    Console.ReadLine();
                return false;
            }
            catch (FeederException exc)
            {
                if (DEBUG && Debugger.IsAttached)
                    throw;
                if (silentErrors)
                    return false;

                TokenFeeder thrower = exc.feeder;
                string message = exc.Message;
                int[] _lines = thrower.Lines;
                string lines = string.Join(", ", _lines.Select(line => line < 0 ? "??" : line.ToString()));

                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("An error has occurred during compilation:\n" +
                    $"\t{Path.GetFileName(file)}:{lines} -- {thrower}:\n\t\t{message}\n\nCompilation cannot be continued.");
                Console.ForegroundColor = oldColor;
                if (!NO_PAUSE)
                    Console.ReadLine();
                return false;
            }
        }

        /// <summary>
        /// Returns an array of the paths to all MCCompiled files in the <see cref="Directory.GetCurrentDirectory"/>.
        /// </summary>
        /// <returns></returns>
        private static string[] GetMCCFilesInDirectory()
        {
            return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mcc", SearchOption.AllDirectories);
        }
        
        internal struct InputPPV
        {
            internal readonly string name;
            internal readonly object value;

            internal InputPPV(string name, string value)
            {
                this.name = name;

                switch (value.ToUpper())
                {
                    case "TRUE":
                        this.value = true;
                        return;
                    case "FALSE":
                        this.value = false;
                        return;
                }

                if (int.TryParse(value, out int integer))
                {
                    this.value = integer;
                    return;
                }
                if (float.TryParse(value, out float floating))
                {
                    this.value = floating;
                    return;
                }

                this.value = value;
            }
        }
    }

}