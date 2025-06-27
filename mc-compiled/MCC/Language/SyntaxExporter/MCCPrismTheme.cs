using System;
using System.Linq;
using JetBrains.Annotations;

namespace mc_compiled.MCC.Language.SyntaxExporter;

/// <summary>
///     Constants for the default MCCompiled Prism theme.
/// </summary>
public static class MCCPrismTheme
{
    public const string DEFAULT_FONT = "Jetbrains Mono";
    public static readonly ThemeColor DEFAULT_TEXT = new("FFFFFF");
    public static readonly ThemeColor COMMENTS = new("3E8C42");
    public static readonly ThemeColor NUMBERS = new("E0C1FF");
    public static readonly ThemeColor OPERATORS = new("E0C1FF");
    public static readonly ThemeColor BLOCK_BRACKETS = new("00FF40");
    public static readonly ThemeColor DELIMITERS = new("DDB3FF");
    public static readonly ThemeColor DELIMITERS_ALTERNATE = new("C0C0C0");
    public static readonly ThemeColor SELECTORS = new("FF4F4F");
    public static readonly ThemeColor PREPROCESSOR_DIRECTIVES = new("0BA4DD");
    public static readonly ThemeColor REGULAR_DIRECTIVES = new("EE5BAF");

    /// <summary>
    ///     Language-specific keywords like <c>true, false, not, and, null</c>, etc...
    /// </summary>
    public static readonly ThemeColor LANGUAGE_KEYWORDS = new("E0C1FF");
    /// <summary>
    ///     Type keywords like <c>int, decimal, bool</c>, etc...
    /// </summary>
    public static readonly ThemeColor TYPE_KEYWORDS = new("FF8080");
    /// <summary>
    ///     Conditional/execute-command keywords like <c>until, count, any, block, blocks, positioned, rotated</c>, etc...
    /// </summary>
    public static readonly ThemeColor CONDITIONAL_KEYWORDS = new("D98250");
    /// <summary>
    ///     Subcommand keywords like <c>survival, creative, canplaceon:, times, subtitle</c> etc...
    /// </summary>
    public static readonly ThemeColor SUBCOMMAND_KEYWORDS = new("D7AEFF");
}

/// <summary>
///     Represents a color in the RGB color model. The <see cref="ThemeColor" /> class provides
///     necessary properties and methods to hold and manipulate RGB color values. Each color component
///     (<see cref="red" />, <see cref="green" />, and <see cref="blue" />) is represented as
///     an integer in the range 0-255.
/// </summary>
/// <remarks>
///     The <see cref="ThemeColor" /> class is immutable and is designed to be used for defining consistent
///     color schemes or themes within a given application.
/// </remarks>
public struct ThemeColor
{
    [PublicAPI]
    public byte blue;
    [PublicAPI]
    public byte green;
    [PublicAPI]
    public byte red;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ThemeColor" /> struct using simple RGB values.
    /// </summary>
    /// <param name="red"></param>
    /// <param name="green"></param>
    /// <param name="blue"></param>
    public ThemeColor(byte red, byte green, byte blue)
    {
        this.blue = blue;
        this.green = green;
        this.red = red;
    }
    /// <summary>
    ///     Initializes a new instance of the <see cref="ThemeColor" /> struct using a hexadecimal color string.
    /// </summary>
    /// <param name="hexColor">A string representing the color in hexadecimal format (e.g., "FF0000" for red).</param>
    /// <exception cref="ArgumentException">Thrown when the hex string is invalid or malformed.</exception>
    public ThemeColor(string hexColor) : this()
    {
        if (string.IsNullOrEmpty(hexColor))
            throw new ArgumentException("Hex color string cannot be null or empty.", nameof(hexColor));

        hexColor = hexColor.TrimStart('#');

        if (hexColor.Length != 6)
            throw new ArgumentException("Hex color string must be exactly six characters long.", nameof(hexColor));

        if (!hexColor.All(char.IsAsciiHexDigit))
            throw new ArgumentException("Invalid hex color string format.", nameof(hexColor));

        this.red = Convert.ToByte(hexColor[..2], 16);
        this.green = Convert.ToByte(hexColor[2..4], 16);
        this.blue = Convert.ToByte(hexColor[4..6], 16);
    }

    /// <summary>
    ///     Gets the RGB color representation of this <see cref="ThemeColor" /> instance as a comma-separated string.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> representing the current <see cref="red" />, <see cref="green" />,
    ///     and <see cref="blue" /> components of this <see cref="ThemeColor" /> instance, formatted as
    ///     `<see cref="red" />,<see cref="green" />,<see cref="blue" />`.
    /// </returns>
    public string RGBCommas => $"{this.red},{this.green},{this.blue}";
    /// <summary>
    ///     Gets the RGB color representation of this <see cref="ThemeColor" /> instance as
    ///     a space-separated string.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> representing the current <see cref="red" />, <see cref="green" />,
    ///     and <see cref="blue" /> components of this <see cref="ThemeColor" /> instance, formatted as
    ///     `<see cref="red" /> <see cref="green" /> <see cref="blue" />`.
    /// </returns>
    public string RGBSpaces => $"{this.red} {this.green} {this.blue}";
    /// <summary>
    ///     Gets the hexadecimal color representation of this <see cref="ThemeColor" /> instance with
    ///     a leading hash symbol (`#`).
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> containing the hash-prefixed hexadecimal color representation in the
    ///     format `#RRGGBB`, where `RR`, `GG`, and `BB` are the two-character uppercase hexadecimal
    ///     values of the respective <see cref="red" />, <see cref="green" />, and <see cref="blue" />
    ///     components.
    /// </returns>
    public string HexWithHash => $"#{this.red:X2}{this.green:X2}{this.blue:X2}";
    /// <summary>
    ///     Gets the hexadecimal color representation of this <see cref="ThemeColor" /> instance without
    ///     the preceding hash symbol (`#`).
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> containing the two-character hexadecimal values for the
    ///     <see cref="red" />, <see cref="green" />, and <see cref="blue" /> components,
    ///     concatenated without delimiters or a hash prefix.
    /// </returns>
    public string Hex => $"{this.red:X2}{this.green:X2}{this.blue:X2}";
    /// <summary>
    ///     Gets the CSS-formatted `rgb` color representation of this <see cref="ThemeColor" /> instance.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> in the form of "rgb(red, green, blue)", using the <see cref="red" />,
    ///     <see cref="green" />, and <see cref="blue" /> integer fields of the <see cref="ThemeColor" />
    ///     instance.
    /// </returns>
    public string CssRGB => $"rgb({this.red}, {this.green}, {this.blue})";
}