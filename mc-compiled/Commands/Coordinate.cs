using System;

namespace mc_compiled.Commands;

/// <summary>
///     An integer or float with the option of being relative or facing offset.
/// </summary>
public struct Coordinate : IComparable<Coordinate>
{
    public static readonly Coordinate zero = new(0, false, false, false);
    public static readonly Coordinate here = new(0, false, true, false);
    public static readonly Coordinate facingHere = new(0, false, false, true);

    public decimal valueDecimal;
    public int valueInteger;

    public readonly bool isDecimal;
    private readonly bool isRelative;
    private readonly bool isFacingOffset;

    public static implicit operator Coordinate(int convert) { return new Coordinate(convert, false, false, false); }
    public static implicit operator Coordinate(decimal convert) { return new Coordinate(convert, true, false, false); }
    public Coordinate(decimal value, bool isDecimal, bool isRelative, bool isFacingOffset)
    {
        this.valueDecimal = value;
        this.valueInteger = value > int.MaxValue ? int.MaxValue : (int) Math.Round(value);
        this.isDecimal = isDecimal;
        this.isRelative = isRelative;
        this.isFacingOffset = isFacingOffset;
    }
    public Coordinate(int value, bool isDecimal, bool isRelative, bool isFacingOffset)
    {
        this.valueDecimal = value;
        this.valueInteger = value;
        this.isDecimal = isDecimal;
        this.isRelative = isRelative;
        this.isFacingOffset = isFacingOffset;
    }
    public Coordinate(Coordinate other)
    {
        this.valueDecimal = other.valueDecimal;
        this.valueInteger = other.valueInteger;
        this.isDecimal = other.isDecimal;
        this.isRelative = other.isRelative;
        this.isFacingOffset = other.isFacingOffset;
    }

    /// <summary>
    ///     Parse this coordinate value. Returns null if not succeeded.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Coordinate? Parse(string str)
    {
        if (str == null)
            return null;

        str = str.Trim();

        bool relative = str.StartsWith("~");
        bool lookOffset = str.StartsWith("^");

        str = str.TrimEnd('f');

        if (relative)
            str = str[1..];
        if (lookOffset)
            str = str[1..];

        if (int.TryParse(str, out int i))
            return new Coordinate(i, false, relative, lookOffset);
        if (decimal.TryParse(str, out decimal d))
            return new Coordinate(d, true, relative, lookOffset);

        return new Coordinate(0, false, relative, lookOffset);
    }
    /// <summary>
    ///     Determine if the size of these corner points can be determined at compile-time
    ///     by ensuring every coordinate is either relative or exact.
    /// </summary>
    /// <param name="coords"></param>
    /// <returns></returns>
    public static bool SizeKnown(params Coordinate[] coords)
    {
        if (coords.Length == 0)
            return true;

        bool relative = coords[0].isRelative;

        for (int i = 1; i < coords.Length; i++)
        {
            Coordinate a = coords[i];
            if (a.isRelative != relative)
                return false;
        }

        return true;
    }

    /// <summary>
    ///     Calculates the volume of a shape defined by two sets of coordinates.
    /// </summary>
    /// <param name="x1">The x-coordinate of the first point.</param>
    /// <param name="y1">The y-coordinate of the first point.</param>
    /// <param name="z1">The z-coordinate of the first point.</param>
    /// <param name="x2">The x-coordinate of the second point.</param>
    /// <param name="y2">The y-coordinate of the second point.</param>
    /// <param name="z2">The z-coordinate of the second point.</param>
    /// <returns>The volume of the shape.</returns>
    public static long GetVolume(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2)
    {
        (int, int, int) tuple = GetSize(x1, y1, z1, x2, y2, z2);
        return tuple.Item1 * (long) tuple.Item2 * tuple.Item3;
    }
    public static long GetVolume((int, int, int) tuple) { return tuple.Item1 * (long) tuple.Item2 * tuple.Item3; }
    /// <summary>
    ///     Gets the size of a coordinate in each dimension.
    /// </summary>
    /// <param name="x1">The first x-coordinate.</param>
    /// <param name="y1">The first y-coordinate.</param>
    /// <param name="z1">The first z-coordinate.</param>
    /// <param name="x2">The second x-coordinate.</param>
    /// <param name="y2">The second y-coordinate.</param>
    /// <param name="z2">The second z-coordinate.</param>
    /// <returns>
    ///     The size of the coordinate in each dimension as a tuple (sizeX, sizeY, sizeZ).
    /// </returns>
    public static (int, int, int) GetSize(Coordinate x1,
        Coordinate y1,
        Coordinate z1,
        Coordinate x2,
        Coordinate y2,
        Coordinate z2)
    {
        int sizeX = Math.Abs(x2.valueInteger - x1.valueInteger);
        int sizeY = Math.Abs(y2.valueInteger - y1.valueInteger);
        int sizeZ = Math.Abs(z2.valueInteger - z1.valueInteger);

        return (sizeX, sizeY, sizeZ);
    }

    /// <summary>
    ///     Get a Minecraft-command supported string for this coordinate.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string s;
        bool anyRelative = this.isRelative | this.isFacingOffset;

        if (this.isDecimal)
            s = this.valueDecimal == decimal.Zero && anyRelative ? "" : this.valueDecimal.ToString();
        else
            s = this.valueInteger == 0 && anyRelative ? "" : this.valueInteger.ToString();

        if (this.isRelative)
            return '~' + s;
        if (this.isFacingOffset)
            return '^' + s;

        return s;
    }

    /// <summary>
    ///     Return the smaller of the two coords.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Coordinate Min(Coordinate a, Coordinate b)
    {
        if (a <= b)
            return a;
        return b;
    }
    /// <summary>
    ///     Return the larger of the two coords.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Coordinate Max(Coordinate a, Coordinate b)
    {
        if (a >= b)
            return a;
        return b;
    }

    /// <summary>
    ///     Returns if this coord has any effect on the resulting location. (aka, non-relative and non-zero)
    /// </summary>
    public bool HasEffect
    {
        get
        {
            if (this.isRelative || this.isFacingOffset)
                return this.isDecimal ? this.valueDecimal != decimal.Zero : this.valueInteger != 0;

            return true;
        }
    }
    public override bool Equals(object obj)
    {
        return obj is Coordinate coordinate && this.valueDecimal == coordinate.valueDecimal &&
               this.valueInteger == coordinate.valueInteger && this.isDecimal == coordinate.isDecimal &&
               this.isRelative == coordinate.isRelative && this.isFacingOffset == coordinate.isFacingOffset;
    }
    public override int GetHashCode()
    {
        int hashCode = 1648134579;
        hashCode = hashCode * -1521134295 + this.valueDecimal.GetHashCode();
        hashCode = hashCode * -1521134295 + this.valueInteger.GetHashCode();
        hashCode = hashCode * -1521134295 + this.isDecimal.GetHashCode();
        hashCode = hashCode * -1521134295 + this.isRelative.GetHashCode();
        hashCode = hashCode * -1521134295 + this.isFacingOffset.GetHashCode();
        return hashCode;
    }
    public int CompareTo(Coordinate other)
    {
        if (!this.isDecimal && !other.isDecimal)
            return this.valueInteger.CompareTo(other.valueInteger);
        return this.valueDecimal.CompareTo(other.valueDecimal);
    }

    public static bool operator ==(Coordinate a, Coordinate b) { return a.Equals(b); }
    public static bool operator !=(Coordinate a, Coordinate b) { return !a.Equals(b); }
    public static bool operator <=(Coordinate a, Coordinate b) { return a < b || a == b; }
    public static bool operator >=(Coordinate a, Coordinate b) { return a > b || a == b; }
    public static bool operator <(Coordinate a, Coordinate b)
    {
        if (!a.isDecimal && !b.isDecimal)
            return a.valueInteger < b.valueInteger;
        return a.valueDecimal < b.valueDecimal;
    }
    public static bool operator >(Coordinate a, Coordinate b)
    {
        if (!a.isDecimal && !b.isDecimal)
            return a.valueInteger > b.valueInteger;
        return a.valueDecimal > b.valueDecimal;
    }
    public static bool operator ==(Coordinate a, int b) { return a.valueInteger == b; }
    public static bool operator !=(Coordinate a, int b) { return a.valueInteger != b; }
    public static bool operator <=(Coordinate a, int b) { return a < b || a == b; }
    public static bool operator >=(Coordinate a, int b) { return a > b || a == b; }
    public static bool operator <(Coordinate a, int b) { return a.valueInteger < b; }
    public static bool operator >(Coordinate a, int b) { return a.valueInteger < b; }
    public static bool operator ==(Coordinate a, decimal b) { return a.valueDecimal == b; }
    public static bool operator !=(Coordinate a, decimal b) { return a.valueDecimal != b; }
    public static bool operator <=(Coordinate a, decimal b) { return a < b || a == b; }
    public static bool operator >=(Coordinate a, decimal b) { return a > b || a == b; }
    public static bool operator <(Coordinate a, decimal b) { return a.valueDecimal < b; }
    public static bool operator >(Coordinate a, decimal b) { return a.valueDecimal < b; }

    public static Coordinate operator -(Coordinate a)
    {
        a.valueInteger *= -1;
        a.valueDecimal *= decimal.MinusOne;
        return a;
    }
    public static Coordinate operator +(Coordinate a, Coordinate b)
    {
        if (a.isDecimal || b.isDecimal)
            return new Coordinate(a.valueDecimal + b.valueDecimal, true, a.isRelative, a.isFacingOffset);
        return new Coordinate(a.valueInteger + b.valueInteger, false, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator -(Coordinate a, Coordinate b)
    {
        if (a.isDecimal || b.isDecimal)
            return new Coordinate(a.valueDecimal - b.valueDecimal, true, a.isRelative, a.isFacingOffset);
        return new Coordinate(a.valueInteger - b.valueInteger, false, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator *(Coordinate a, Coordinate b)
    {
        if (a.isDecimal || b.isDecimal)
            return new Coordinate(a.valueDecimal * b.valueDecimal, true, a.isRelative, a.isFacingOffset);
        return new Coordinate(a.valueInteger * b.valueInteger, false, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator /(Coordinate a, Coordinate b)
    {
        if (a.isDecimal || b.isDecimal)
            return new Coordinate(a.valueDecimal / b.valueDecimal, true, a.isRelative, a.isFacingOffset);
        return new Coordinate(a.valueInteger / b.valueInteger, false, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator %(Coordinate a, Coordinate b)
    {
        if (a.isDecimal || b.isDecimal)
            return new Coordinate(a.valueDecimal % b.valueDecimal, true, a.isRelative, a.isFacingOffset);
        return new Coordinate(a.valueInteger % b.valueInteger, false, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator +(Coordinate a, int b)
    {
        if (a.isDecimal)
            return new Coordinate(a.valueDecimal + b, true, a.isRelative, a.isFacingOffset);
        return new Coordinate(a.valueInteger + b, false, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator -(Coordinate a, int b)
    {
        if (a.isDecimal)
            return new Coordinate(a.valueDecimal - b, true, a.isRelative, a.isFacingOffset);
        return new Coordinate(a.valueInteger - b, false, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator *(Coordinate a, int b)
    {
        if (a.isDecimal)
            return new Coordinate(a.valueDecimal * b, true, a.isRelative, a.isFacingOffset);
        return new Coordinate(a.valueInteger * b, false, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator /(Coordinate a, int b)
    {
        if (a.isDecimal)
            return new Coordinate(a.valueDecimal / b, true, a.isRelative, a.isFacingOffset);
        return new Coordinate(a.valueInteger / b, false, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator %(Coordinate a, int b)
    {
        if (a.isDecimal)
            return new Coordinate(a.valueDecimal % b, true, a.isRelative, a.isFacingOffset);
        return new Coordinate(a.valueInteger % b, false, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator +(Coordinate a, decimal b)
    {
        return new Coordinate(a.valueDecimal + b, true, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator -(Coordinate a, decimal b)
    {
        return new Coordinate(a.valueDecimal - b, true, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator *(Coordinate a, decimal b)
    {
        return new Coordinate(a.valueDecimal * b, true, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator /(Coordinate a, decimal b)
    {
        return new Coordinate(a.valueDecimal / b, true, a.isRelative, a.isFacingOffset);
    }
    public static Coordinate operator %(Coordinate a, decimal b)
    {
        return new Coordinate(a.valueDecimal % b, true, a.isRelative, a.isFacingOffset);
    }
}