using System.Collections.Generic;

namespace mc_compiled.NBT.Structures.BlockPositionData;

/// <summary>
///     A block entity which is able to be powered by redstone.
/// </summary>
/// <param name="id"></param>
/// <param name="x"></param>
/// <param name="y"></param>
/// <param name="z"></param>
/// <param name="isPowered"></param>
/// <param name="isMovable"></param>
public class PowerableBlockEntityDataNBT(string id, int x, int y, int z, bool isPowered, bool isMovable = true)
    : BasicBlockEntityDataNBT(id, x, y, z, isMovable)
{
    /// <summary>
    ///     If the block is being powered by redstone right now.
    /// </summary>
    private readonly bool isPowered = isPowered;

    protected override List<NBTNode> GetNodes()
    {
        List<NBTNode> root = base.GetNodes();
        root.Add(new NBTByte {name = "powered", value = this.isPowered ? (byte) 1 : (byte) 0});
        return root;
    }
}