using System;
using System.Text;

namespace mc_compiled.Modding;

/// <summary>
///     States that this class can be output to a behavior file.
/// </summary>
public interface IAddonFile
{
    /// <summary>
    ///     A reference to the output of this addon file in a minecraft command.
    /// </summary>
    string CommandReference { get; }

    /// <summary>
    ///     Returns non-null if this file wishes to extend from the base OutputLocation.
    /// </summary>
    /// <returns></returns>
    string GetExtendedDirectory();
    /// <summary>
    ///     Get the file name and extension for this output.
    /// </summary>
    /// <returns></returns>
    string GetOutputFile();

    /// <summary>
    ///     Get the data to write into the file.
    /// </summary>
    /// <returns></returns>
    byte[] GetOutputData();

    /// <summary>
    ///     Indicates base location of the output file.
    /// </summary>
    /// <returns>The output location of the file.</returns>
    OutputLocation GetOutputLocation();
}

public sealed class RawFile : IAddonFile
{
    public string data;

    public string outputFile;
    public RawFile(string outputFile, string data)
    {
        this.outputFile = outputFile;
        this.data = data;
    }

    public string CommandReference => throw new NotImplementedException();
    public string GetExtendedDirectory()
    {
        return null;
    }

    public byte[] GetOutputData()
    {
        return Encoding.UTF8.GetBytes(this.data);
    }
    public string GetOutputFile()
    {
        return this.outputFile;
    }

    public OutputLocation GetOutputLocation()
    {
        return OutputLocation.NONE;
    }
}

public static class OutputLocationExtensions
{
    public static bool IsBehavior(this OutputLocation output)
    {
        return output switch
        {
            OutputLocation.b_ROOT or OutputLocation.b_ANIMATIONS or OutputLocation.b_ANIMATION_CONTROLLERS
                or OutputLocation.b_BLOCKS or OutputLocation.b_BIOMES or OutputLocation.b_DIALOGUE
                or OutputLocation.b_ENTITIES or OutputLocation.b_FEATURES or OutputLocation.b_FEATURE_RULES
                or OutputLocation.b_FUNCTIONS or OutputLocation.b_ITEMS or OutputLocation.b_LOOT_TABLES
                or OutputLocation.b_RECIPES or OutputLocation.b_SCRIPTS__CLIENT or OutputLocation.b_SCRIPTS__SERVER
                or OutputLocation.b_SCRIPTS__GAMETESTS or OutputLocation.b_SPAWN_RULES or OutputLocation.b_TEXTS
                or OutputLocation.b_TRADING or OutputLocation.b_STRUCTURES => true,
            OutputLocation.r_ROOT or OutputLocation.r_ANIMATION_CONTROLLERS or OutputLocation.r_ANIMATIONS
                or OutputLocation.r_ATTACHABLES or OutputLocation.r_ENTITY or OutputLocation.r_FOGS
                or OutputLocation.r_MODELS__ENTITY or OutputLocation.r_MODELS__BLOCKS or OutputLocation.r_PARTICLES
                or OutputLocation.r_ITEMS or OutputLocation.r_RENDER_CONTROLLERS or OutputLocation.r_SOUNDS
                or OutputLocation.r_TEXTS or OutputLocation.r_TEXTURES__ENVIRONMENT
                or OutputLocation.r_TEXTURES__BLOCKS or OutputLocation.r_TEXTURES__ENTITY
                or OutputLocation.r_TEXTURES__ITEMS or OutputLocation.r_TEXTURES__PARTICLE
                or OutputLocation.r_UI => false,
            _ => true
        };
    }
}

/// <summary>
///     Represents an output location
/// </summary>
public enum OutputLocation
{
    NONE,

    b_ROOT,
    b_ANIMATIONS,
    b_ANIMATION_CONTROLLERS,
    b_BLOCKS,
    b_BIOMES,
    b_DIALOGUE,
    b_ENTITIES,
    b_FEATURES,
    b_FEATURE_RULES,
    b_FUNCTIONS,
    b_ITEMS,
    b_LOOT_TABLES,
    b_RECIPES,
    b_SCRIPTS__CLIENT,
    b_SCRIPTS__SERVER,
    b_SCRIPTS__GAMETESTS,
    b_SPAWN_RULES,
    b_TEXTS,
    b_TRADING,
    b_STRUCTURES,

    r_ROOT,
    r_ANIMATION_CONTROLLERS,
    r_ANIMATIONS,
    r_ATTACHABLES,
    r_ENTITY,
    r_FOGS,
    r_MODELS__ENTITY,
    r_MODELS__BLOCKS,
    r_PARTICLES,
    r_ITEMS,
    r_RENDER_CONTROLLERS,
    r_SOUNDS,
    r_TEXTS,
    r_TEXTURES__ENVIRONMENT,
    r_TEXTURES__BLOCKS,
    r_TEXTURES__ENTITY,
    r_TEXTURES__ITEMS,
    r_TEXTURES__PARTICLE,
    r_UI
}