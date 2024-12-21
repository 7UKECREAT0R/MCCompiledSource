using System.Collections.Generic;
using mc_compiled.MCC.Compiler.TypeSystem.Implementations;

namespace mc_compiled.MCC.Compiler.TypeSystem;

public abstract partial class Typedef
{
    /// <summary>
    ///     Boolean type, no data.
    /// </summary>
    public static readonly Typedef BOOLEAN = new TypedefBoolean();
    /// <summary>
    ///     Fixed-point decimal type, data is <see cref="FixedDecimalData" />.
    /// </summary>
    public static readonly Typedef FIXED_DECIMAL = new TypedefFixedDecimal();
    /// <summary>
    ///     Integer type, no data.
    /// </summary>
    public static readonly Typedef INTEGER = new TypedefInteger();
    /// <summary>
    ///     Time type, no data.
    /// </summary>
    public static readonly Typedef TIME = new TypedefTime();

    public static readonly Typedef[] ALL_TYPES =
    [
        INTEGER,
        FIXED_DECIMAL,
        BOOLEAN,
        TIME
    ];

    private static readonly Dictionary<ScoreboardManager.ValueType, Typedef> _FROM_VALUE_TYPE = new();
    private static readonly Dictionary<string, Typedef> _FROM_KEYWORD = new();
    static Typedef()
    {
        foreach (Typedef type in ALL_TYPES)
        {
            _FROM_VALUE_TYPE[type.TypeEnum] = type;
            _FROM_KEYWORD[type.TypeKeyword] = type;
        }
    }

    /// <summary>
    ///     Attempts to get a typedef bound to the given value type.
    /// </summary>
    /// <param name="valueType">The <see cref="ScoreboardManager.ValueType" /> to search for.</param>
    /// <param name="type">The found type, if this method returns true.</param>
    /// <returns>If a type was found.</returns>
    public static bool FromValueType(ScoreboardManager.ValueType valueType, out Typedef type)
    {
        return _FROM_VALUE_TYPE.TryGetValue(valueType, out type);
    }
    /// <summary>
    ///     Attempts to get a typedef bound to the given keyword.
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <param name="type">The found type, if this method returns true.</param>
    /// <returns>If a type was found.</returns>
    public static bool FromKeyword(string keyword, out Typedef type)
    {
        return _FROM_KEYWORD.TryGetValue(keyword, out type);
    }
}