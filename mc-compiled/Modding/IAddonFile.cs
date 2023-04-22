﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding
{
    /// <summary>
    /// States that this class can be output to a behavior file.
    /// </summary>
    public interface IAddonFile
    {
        /// <summary>
        /// A reference to the output of this addon file in a minecraft command.
        /// </summary>
        string CommandReference { get; }

        /// <summary>
        /// Returns non-null if this file wishes to extend from the base OutputLocation.
        /// </summary>
        /// <returns></returns>
        string GetExtendedDirectory();
        /// <summary>
        /// Get the file name and extension for this output.
        /// </summary>
        /// <returns></returns>
        string GetOutputFile();

        /// <summary>
        /// Get the data to write into the file.
        /// </summary>
        /// <returns></returns>
        byte[] GetOutputData();

        /// <summary>
        /// Indicates base location of the output file.
        /// </summary>
        /// <returns>The output location of the file.</returns>
        OutputLocation GetOutputLocation();
    }

    public sealed class RawFile : IAddonFile
    {
        public RawFile(string outputFile, string data)
        {
            this.outputFile = outputFile;
            this.data = data;
        }

        public string outputFile;
        public string data;

        public string CommandReference => throw new NotImplementedException();
        public string GetExtendedDirectory() => null;

        public byte[] GetOutputData() => Encoding.UTF8.GetBytes(data);
        public string GetOutputFile() => outputFile;

        public OutputLocation GetOutputLocation() => OutputLocation.NONE;
    }

    public static class OutputLocationExtensions
    {
        public static bool IsBehavior(this OutputLocation output)
        {
            switch (output)
            {
                case OutputLocation.b_ROOT:
                case OutputLocation.b_ANIMATIONS:
                case OutputLocation.b_ANIMATION_CONTROLLERS:
                case OutputLocation.b_BLOCKS:
                case OutputLocation.b_BIOMES:
                case OutputLocation.b_ENTITIES:
                case OutputLocation.b_FEATURES:
                case OutputLocation.b_FEATURE_RULES:
                case OutputLocation.b_FUNCTIONS:
                case OutputLocation.b_ITEMS:
                case OutputLocation.b_LOOT_TABLES:
                case OutputLocation.b_RECIPES:
                case OutputLocation.b_SCRIPTS_CLIENT:
                case OutputLocation.b_SCRIPTS_SERVER:
                case OutputLocation.b_SCRIPTS_GAMETESTS:
                case OutputLocation.b_SPAWN_RULES:
                case OutputLocation.b_TEXTS:
                case OutputLocation.b_TRADING:
                case OutputLocation.b_STRUCTURES:
                    return true;
                case OutputLocation.r_ROOT:
                case OutputLocation.r_ANIMATION_CONTROLLERS:
                case OutputLocation.r_ANIMATIONS:
                case OutputLocation.r_ATTACHABLES:
                case OutputLocation.r_ENTITY:
                case OutputLocation.r_FOGS:
                case OutputLocation.r_MODELS_ENTITY:
                case OutputLocation.r_MODELS_BLOCKS:
                case OutputLocation.r_PARTICLES:
                case OutputLocation.r_ITEMS:
                case OutputLocation.r_RENDER_CONTROLLERS:
                case OutputLocation.r_SOUNDS:
                case OutputLocation.r_TEXTS:
                case OutputLocation.r_TEXTURES_ENVIRONMENT:
                case OutputLocation.r_TEXTURES_BLOCKS:
                case OutputLocation.r_TEXTURES_ENTITY:
                case OutputLocation.r_TEXTURES_ITEMS:
                case OutputLocation.r_TEXTURES_PARTICLE:
                case OutputLocation.r_UI:
                    return false;
                default:
                    return true;
            }
        }
    }
    /// <summary>
    /// Represents an output location 
    /// </summary>
    public enum OutputLocation
    {
        NONE,

        b_ROOT,
        b_ANIMATIONS,
        b_ANIMATION_CONTROLLERS,
        b_BLOCKS,
        b_BIOMES,
        b_ENTITIES,
        b_FEATURES,
        b_FEATURE_RULES,
        b_FUNCTIONS,
        b_ITEMS,
        b_LOOT_TABLES,
        b_RECIPES,
        b_SCRIPTS_CLIENT,
        b_SCRIPTS_SERVER,
        b_SCRIPTS_GAMETESTS,
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
        r_MODELS_ENTITY,
        r_MODELS_BLOCKS,
        r_PARTICLES,
        r_ITEMS,
        r_RENDER_CONTROLLERS,
        r_SOUNDS,
        r_TEXTS,
        r_TEXTURES_ENVIRONMENT,
        r_TEXTURES_BLOCKS,
        r_TEXTURES_ENTITY, 
        r_TEXTURES_ITEMS,
        r_TEXTURES_PARTICLE,
        r_UI
    }
}
