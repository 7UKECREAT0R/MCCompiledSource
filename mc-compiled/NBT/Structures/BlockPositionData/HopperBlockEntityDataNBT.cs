using System.Collections.Generic;

namespace mc_compiled.NBT.Structures.BlockPositionData;

public class HopperBlockEntityDataNBT(int x, int y, int z, int transferCooldown = 0, bool isMovable = true)
    : ContainerBlockEntityDataNBT(CommonBlockEntityIdentifiers.Hopper, x, y, z, isMovable)
{
    protected override List<NBTNode> GetNodes()
    {
        List<NBTNode> root = base.GetNodes();
        root.Add(new NBTInt {name = "TransferCooldown", value = transferCooldown});
        return root;
    }
}