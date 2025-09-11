using System.Collections.Generic;

namespace mc_compiled.NBT.Structures.BlockPositionData;

public class FurnaceBlockEntityDataNBT(
    int x,
    int y,
    int z,
    short burnDuration = 0,
    short burnTime = 0,
    short cookTime = 0,
    int storedXP = 0,
    bool isMovable = true)
    : ContainerBlockEntityDataNBT(CommonBlockEntityIdentifiers.Furnace, x, y, z, isMovable)
{
    protected override List<NBTNode> GetNodes()
    {
        List<NBTNode> root = base.GetNodes();
        root.Add(new NBTShort {name = "BurnDuration", value = burnDuration});
        root.Add(new NBTShort {name = "BurnTime", value = burnTime});
        root.Add(new NBTShort {name = "CookTime", value = cookTime});
        root.Add(new NBTInt {name = "StoredXPInt", value = storedXP});
        return root;
    }
}