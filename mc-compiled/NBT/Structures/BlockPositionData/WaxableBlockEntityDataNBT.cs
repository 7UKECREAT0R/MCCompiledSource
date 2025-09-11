using System.Collections.Generic;

namespace mc_compiled.NBT.Structures.BlockPositionData;

public class WaxableBlockEntityDataNBT(string id, int x, int y, int z, bool isWaxed, bool isMovable = true)
    : BasicBlockEntityDataNBT(id, x, y, z, isMovable)
{
    /// <summary>
    ///     If the block has been waxed.
    /// </summary>
    private readonly bool isWaxed = isWaxed;

    protected override List<NBTNode> GetNodes()
    {
        List<NBTNode> root = base.GetNodes();
        root.Add(new NBTByte {name = "IsWaxed", value = this.isWaxed ? (byte) 1 : (byte) 0});
        return root;
    }
}