﻿using mc_compiled.NBT;

namespace mc_compiled.Modding;

public struct StructureFile : IAddonFile
{
    public string name;
    public string directory;
    public StructureNBT structure;

    public string CommandReference
    {
        get
        {
            if (this.directory == null)
                return this.name;

            return '"' + this.directory + '/' + this.name + '"';
        }
    }

    public StructureFile(string name, string directory, StructureNBT structure)
    {
        this.name = name;
        this.directory = directory;
        this.structure = structure;
    }

    public string GetExtendedDirectory()
    {
        return this.directory;
    }
    public string GetOutputFile()
    {
        return $"{this.name}.mcstructure";
    }
    public byte[] GetOutputData()
    {
        return FileWriterNBT.GetBytes(this.structure.ToNBT());
    }
    public OutputLocation GetOutputLocation()
    {
        return OutputLocation.b_STRUCTURES;
    }
}