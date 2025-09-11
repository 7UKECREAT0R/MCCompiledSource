using System.Collections.Generic;

namespace mc_compiled.NBT.Structures.BlockPositionData;

/// <summary>
///     Represents block entity data for a sign in the game, supporting customizable
///     text on both front and back of the sign, along with their respective colors
///     and additional properties.
///     Extends <see cref="WaxableBlockEntityDataNBT" /> by adding fields
///     and logic specific to signs.
/// </summary>
public class SignBlockEntityDataNBT(
    int x,
    int y,
    int z,
    bool isEditable, // inverse is applied to the "IsWaxed" property to make this work
    string frontText,
    string backText,
    int frontTextColor = -16777216,
    int backTextColor = -16777216,
    bool isMovable = true)
    : WaxableBlockEntityDataNBT(CommonBlockEntityIdentifiers.Sign, x, y, z, !isEditable, isMovable)
{
    /// <summary>
    ///     The text on the back of the sign.
    /// </summary>
    private readonly string backText = backText;
    /// <summary>
    ///     The color of the back text. Contributors: needs RnD to figure out the format, but <c>-16777216</c> is the default!
    /// </summary>
    private readonly int backTextColor = backTextColor;

    /// <summary>
    ///     The text on the front of the sign.
    /// </summary>
    private readonly string frontText = frontText;

    /// <summary>
    ///     The color of the front text. Contributors: needs RnD to figure out the format, but <c>-16777216</c> is the default!
    /// </summary>
    private readonly int frontTextColor = frontTextColor;

    protected override List<NBTNode> GetNodes()
    {
        List<NBTNode> root = base.GetNodes();
        root.Add(new NBTCompound
        {
            name = "BackText",
            values =
            [
                new NBTString {name = "FilteredText", value = string.Empty},
                new NBTByte {name = "HideGlowOutline", value = 0},
                new NBTByte {name = "IgnoreLighting", value = 0},
                new NBTByte {name = "PersistFormatting", value = 1},
                new NBTInt {name = "SignTextColor", value = this.backTextColor},
                new NBTString {name = "Text", value = this.backText},
                new NBTString {name = "TextOwner", value = string.Empty},
                new NBTEnd()
            ]
        });
        root.Add(new NBTCompound
        {
            name = "FrontText",
            values =
            [
                new NBTString {name = "FilteredText", value = string.Empty},
                new NBTByte {name = "HideGlowOutline", value = 0},
                new NBTByte {name = "IgnoreLighting", value = 0},
                new NBTByte {name = "PersistFormatting", value = 1},
                new NBTInt {name = "SignTextColor", value = this.frontTextColor},
                new NBTString {name = "Text", value = this.frontText},
                new NBTString {name = "TextOwner", value = string.Empty},
                new NBTEnd()
            ]
        });

        return root;
    }
}