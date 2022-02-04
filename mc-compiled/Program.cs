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
        public static bool OBFUSCATE = false;
        public static bool DEBUG = false;
        public static bool DECORATE = false;

        static void Help()
        {
            Console.Write("\nmc-compiled.exe --help\n");
            Console.Write("\nmc-compiled.exe --jsonbuilder\n");
            Console.Write("mc-compiled.exe <file> [--decorate] [-o] [-d] [-r]\n");
            Console.Write("\tCompile a .mcc file into the resulting .mcfunction files.\n\tIf the -jsonbuilder option is specified, the rawtext json builder is opened instead.\n\n");
            Console.Write("\tOptions:\n");
            Console.Write("\t  --decorate\tDecorate and include source comments in the output files.\n");
            Console.Write("\t  -o\tObfuscate scoreboard names and other values.\n");
            Console.Write("\t  -d\tDebug information during compilation.\n");
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

            bool obf = false;
            bool debug = false;
            bool decor = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-o"))
                    obf = true;
                if (args[i].Equals("-d"))
                    debug = true;
                if (args[i].Equals("--decorate"))
                    decor = true;
            }

            new Definitions(debug);

            if (file.ToUpper().Equals("--TESTNBT"))
            {
                ItemStack item = new ItemStack("diamond_boots");
                item.count = 2;
                item.displayName = "mega boots";
                item.keep = true;
                item.enchantments = new EnchantmentEntry[]
                {
                    new EnchantmentEntry(Commands.Enchantment.protection, 10),
                    new EnchantmentEntry(Commands.Enchantment.frost_walker, 5),
                    new EnchantmentEntry(Commands.Enchantment.unbreaking, 20)
                };

                StructureNBT structure = StructureNBT.SingleItem(item);

                FileWriterNBT writer = new FileWriterNBT("test.nbt", structure.ToNBT());
                writer.Write();

                Console.WriteLine("Wrote test file.");
                return;
            }
            if (file.ToUpper().Equals("--JSONBUILDER"))
            {
                RawTextJsonBuilder builder = new RawTextJsonBuilder();
                builder.ConsoleInterface();
                return;
            }

            if(debug)
            {
                Console.WriteLine("Debug Enabled");
                Console.WriteLine("\tObfuscate: " + obf.ToString());
                Console.WriteLine("\tDecorate: " + decor.ToString());
            }

            DEBUG = debug;
            OBFUSCATE = obf;

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

                // shouldn't throw unless unintentional
                Statement[] statements = Assembler.AssembleTokens(tokens);

                if(DEBUG)
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

                if (DEBUG)
                    Console.ReadLine();
                else
                    System.Threading.Thread.Sleep(3000);
            } catch(TokenizerException exc)
            {
                int line = exc.line;
                string message = exc.Message;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Problem encountered during tokenization of file:\n" +
                    $"\tLINE {line}: {message}\n\nTokenization cannot be continued.");
                Console.ReadLine();
                return;
            } catch(StatementException exc)
            {
                Statement thrower = exc.statement;
                string message = exc.Message;
                int line = thrower.Line;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error has occurred during compilation:\n" +
                    $"\tLN{line} {thrower.ToString()}:\n\t\t{message}\n\nCompilation cannot be continued.");
                Console.ReadLine();
                return;
            }
        }
    }
}