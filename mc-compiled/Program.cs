using mc_compiled.Commands.Native;
using mc_compiled.Json;
using mc_compiled.MCC;
using mc_compiled.MCC.Compilers;
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
                if (args[i].Equals("-r"))
                    Compiler.DISABLE_MACRO_GUARD = true;
            }

            new Definitions(debug);

            if (file.ToUpper().Equals("--TESTNBT"))
            {
                ItemStack item = new ItemStack("diamond_boots");
                item.count = 2;
                item.displayName = "mega boots";
                item.keep = true;
                item.enchantments = new Enchantment[]
                {
                    new Enchantment("protection", 10),
                    new Enchantment("frost walker", 5),
                    new Enchantment("unbreaking", 20)
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
                if (Compiler.DISABLE_MACRO_GUARD)
                    Console.WriteLine("\tMacro recursion allowed.");
            }

            DEBUG = debug;
            OBFUSCATE = obf;

            Compiler compiler = new DefaultCompiler();

            Token[] compiled = compiler.CompileFile(file);
            Executor executor = new Executor(compiled, debug, decor,
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
            } else  pack.structures = itemStructures.ToArray();

            Console.WriteLine("Writing files...");
            pack.Write();
            Console.WriteLine("Finished");
        }
    }
}