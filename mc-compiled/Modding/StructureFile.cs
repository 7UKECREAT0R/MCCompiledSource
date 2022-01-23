using mc_compiled.NBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding
{
    public struct StructureFile : IBehaviorOutput
    {
        public string name;
        public StructureNBT structure;

        public StructureFile(string name, StructureNBT structure)
        {
            this.name = name;
            this.structure = structure;
        }

        public string GetOutputDirectory() =>
            "structures\\";
        public string GetOutputFile() =>
            $"{name}.mcstructure";
        public byte[] GetOutputData() =>
            FileWriterNBT.GetBytes(structure.ToNBT());
    }
}
