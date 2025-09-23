using System.Linq;

namespace mc_compiled.NBT.Structures;

public struct EntityListNBT
{
    private readonly EntityNBT[] entities;

    public EntityListNBT(params EntityNBT[] entities) { this.entities = entities; }

    public NBTList ToNBT()
    {
        if (this.entities == null)
            return new NBTList
            {
                name = "entities",
                listType = TAG.Compound,
                values = []
            };
        return new NBTList
        {
            name = "entities",
            listType = TAG.Compound,
            values = (from entity in this.entities select entity.ToNBT("")).ToArray<NBTNode>()
        };
    }
}