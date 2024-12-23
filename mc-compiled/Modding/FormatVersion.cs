﻿using System;

namespace mc_compiled.Modding;

public struct FormatVersion
{
    // https://wiki.bedrock.dev/guide/format-version.html#format-versions-per-asset-type
    public static readonly FormatVersion r_ENTITY = new(1, 10, 0);
    public static readonly FormatVersion r_ANIMATION = new(1, 10, 0);
    public static readonly FormatVersion r_ATTACHABLE = new(1, 10, 0);
    public static readonly FormatVersion r_MODEL = new(1, 12, 0);
    public static readonly FormatVersion r_PARTICLE = new(1, 10, 0);
    public static readonly FormatVersion r_RENDER_CONTROLLER = new(1, 10, 0);
    public static readonly FormatVersion r_PARTICLES = new(1, 10, 0);
    public static readonly FormatVersion r_SOUNDS = new(1, 18, 10);
    public static readonly FormatVersion b_ANIMATION_CONTROLLER = new(1, 10, 0);
    public static readonly FormatVersion b_ENTITY = new(1, 16, 0);
    public static readonly FormatVersion b_ITEM = new(1, 10);
    public static readonly FormatVersion b_RECIPE = new(1, 16);
    public static readonly FormatVersion b_SPAWN_RULE = new(1, 8, 0);
    public static readonly FormatVersion b_DIALOGUE = new(1, 17, 0);

    private readonly int release;
    private readonly int major;
    private readonly int? minor;

    private FormatVersion(int release, int major, int? minor = null)
    {
        this.release = release;
        this.major = major;
        this.minor = minor;
    }
    public static FormatVersion Parse(string version)
    {
        string[] parts = version.Split('.');

        if (parts.Length < 2)
            throw new Exception("Format version was missing information.");

        int release, major;
        int? minor;

        if (parts.Length == 2)
        {
            release = int.Parse(parts[0]);
            major = int.Parse(parts[1]);
            minor = null;
        }
        else
        {
            release = int.Parse(parts[0]);
            major = int.Parse(parts[1]);
            minor = int.Parse(parts[2]);
        }

        if (parts.Length > 3 || release < 1 || major < 0 || minor < 0)
            throw new Exception($"Format version '{version}' malformed.");

        return new FormatVersion(release, major, minor);
    }

    /// <summary>
    ///     Convert this format version to a proper string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (this.minor.HasValue)
            return $"{this.release}.{this.major}.{this.minor.Value}";

        return $"{this.release}.{this.major}";
    }
}