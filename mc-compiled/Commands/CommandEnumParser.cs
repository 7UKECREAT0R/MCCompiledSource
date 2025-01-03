using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands;

public static class CommandEnumParser
{
    private static readonly Dictionary<string, ParsedEnumValue> parser = new(StringComparer.OrdinalIgnoreCase);

    public static bool TryParse(string input, out ParsedEnumValue result)
    {
        return parser.TryGetValue(input.ToUpper(), out result);
    }

    public static void Put(ParsedEnumValue value) { parser[value.ToString().ToUpper()] = value; }
    public static void Put(string key, ParsedEnumValue value) { parser[key.ToUpper()] = value; }

    /// <summary>
    ///     Initialize the global CommandEnumParser.
    /// </summary>
    public static void Init()
    {
        parser.Clear();

        if (GlobalContext.Debug)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Initializing enum parser...");
            Console.ForegroundColor = ConsoleColor.White;
        }

        IEnumerable<Type> allEnums = from a in Assembly.GetExecutingAssembly().GetTypes()
            where a.IsEnum
            select a;

        foreach (Type thisEnum in allEnums)
            thisEnum.GetCustomAttributes(); // calls their constructors

        // ReSharper disable once InvertIf
        if (GlobalContext.Debug)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Registered " + parser.Count + " unique parser identifiers.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}

public readonly struct ParsedEnumValue
{
    public static ParsedEnumValue None(string word) { return new ParsedEnumValue(null, word, true); }

    public readonly bool isNone = false;
    public readonly Type enumType;
    public readonly object value;

    public ParsedEnumValue(Type enumType, object value, bool isNone = false)
    {
        this.isNone = isNone;
        this.enumType = enumType;
        this.value = value;
    }
    /// <summary>
    ///     Returns if this enum value is of a certain type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns></returns>
    public bool IsType<T>() where T : Enum
    {
        if (this.isNone)
            return false;

        string src = typeof(T).Name;
        return this.enumType.Name.Equals(src);
    }

    /// <summary>
    ///     Requires this enum value to be of a certain type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="MCC.Compiler.StatementException">If the given enum is not of a certain type.</exception>
    // ReSharper disable once PureAttributeOnVoidMethod
    [Pure]
    public void RequireType<T>(Statement thrower) where T : Enum
    {
        if (!this.isNone && IsType<T>())
            return;

        string reqEnumName = typeof(T).Name;
        string givenEnumName = this.enumType?.Name ?? (this.value == null ? "unknown" : $"text \"{this.value}\"");

        string[] possibleValues = Enum.GetNames(typeof(T));
        throw new StatementException(thrower,
            $"Must specify {reqEnumName}; Given {givenEnumName}. Possible values: {string.Join(", ", possibleValues)}");
    }
}

[AttributeUsage(AttributeTargets.Enum)]
internal sealed class EnumParsableAttribute : Attribute
{
    public EnumParsableAttribute(Type type)
    {
        if (!type.IsEnum)
            return;

        IEnumerable<object> array = type.GetEnumValues().Cast<object>();
        foreach (object value in array)
        {
            string key = value.ToString()!;
            var finalValue = new ParsedEnumValue(type, value);
            CommandEnumParser.Put(key, finalValue);

            if (!key.Contains("_"))
                continue;

            // Might use a dot.
            finalValue = new ParsedEnumValue(type, value);
            CommandEnumParser.Put(key.Replace('_', '.'), finalValue);
        }
    }
}