using mc_compiled.MCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using mc_compiled.NBT;

namespace mc_compiled.Modding
{
    /// <summary>
    /// The root behaviorpack object. Can be written as a file.
    /// </summary>
    public class BehaviorPack
    {
        public string packName = "mcc-default";
        public Manifest manifest = new Manifest("mcc-default", "MCCompiled Default Behavior Pack");

        public MCFunction[] functions;
        public StructureFile[] structures;

        public void Write()
        {
            manifest.name = packName;

            if(Directory.Exists(packName))
                Directory.Delete(packName, true);

            Directory.CreateDirectory(packName);

            string manifestFile = $"{packName}\\manifest.json";
            File.WriteAllText(manifestFile, manifest.ToString());

            if(functions != null && functions.Length > 0)
            {
                string functionsLocation = $"{packName}\\functions";
                Directory.CreateDirectory(functionsLocation);
                foreach(MCFunction function in functions)
                    function.WriteFile(functionsLocation);
            }
            if(structures != null && structures.Length > 0)
            {
                string structuresLocation = $"{packName}\\structures";
                Directory.CreateDirectory(structuresLocation);
                foreach(StructureFile structure in structures)
                {
                    string structureFile = $"{structuresLocation}\\{structure.name}.mcstructure";
                    FileWriterNBT writer = new FileWriterNBT(structureFile, structure.structure.ToNBT());
                    writer.Write();
                }
            }
        }
    }
}
