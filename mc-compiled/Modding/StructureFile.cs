using mc_compiled.NBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding
{
    public struct StructureFile : IAddonFile
    {
        public string name;
        public string directory;
        public StructureNBT structure;

        public string CommandReference
        {
            get
            {
                if (directory == null)
                    return name;

                return '"' + directory + '/' + name + '"';
            }
        }

        public StructureFile(string name, string directory, StructureNBT structure)
        {
            this.name = name;
            this.directory = directory;
            this.structure = structure;
        }

        public string GetExtendedDirectory() =>
            directory;
        public string GetOutputFile() =>
            $"{name}.mcstructure";
        public byte[] GetOutputData() =>
            FileWriterNBT.GetBytes(structure.ToNBT());
        public OutputLocation GetOutputLocation() =>
            OutputLocation.b_STRUCTURES;
    }
}
