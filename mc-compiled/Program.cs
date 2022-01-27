using mc_compiled.Commands.Native;
using mc_compiled.Json;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using mc_compiled.Modding;
using mc_compiled.NBT;
using System;
using System.Collections.Generic;
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
            Console.Write("\t  -r\tDisable the macro recursion guard.\n\n");
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
                item.enchantments = new LegacyEnchantmentObject[]
                {
                    new LegacyEnchantmentObject("protection", 10),
                    new LegacyEnchantmentObject("frost walker", 5),
                    new LegacyEnchantmentObject("unbreaking", 20)
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

            // assemble tokens into coherent 'statements' ex:
            // Statement[] assembled = tokenizer.Assemble(tokens)

            /*Executor executor = new Executor(null, debug, decor,
                System.IO.Path.GetFileNameWithoutExtension(file));

            executor.Run();

            BehaviorPack pack = executor.GetAsPack();
            Tuple<string, ItemStack>[] items = executor.GetItemDefinitions();
            List<StructureFile> itemStructures = new List<StructureFile>();

            foreach(var kvp in items)
            {
                string name = kvp.Item1;
                ItemStack stack = kvp.Item2;
                itemStructures.Add(new StructureFile
                    (name, StructureNBT.SingleItem(stack)));
            }

            if (pack.structures != null)
            {
                List<StructureFile> structureFiles = pack.structures.ToList();
                structureFiles.AddRange(itemStructures);
                pack.structures = structureFiles.ToArray();
            } else  pack.structures = itemStructures.ToArray();*/

            Console.WriteLine("Writing files...");
            //pack.Write();
            Console.WriteLine("Finished");

            if (DEBUG)
                Console.ReadLine();
            else
                System.Threading.Thread.Sleep(3000);
        }
    }
}