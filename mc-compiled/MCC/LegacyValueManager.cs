using mc_compiled.Commands;
using mc_compiled.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Caches and tracks values and their types. Also holds onto and enforces struct definitions.
    /// In a sense, this class doubles as an advanced code generator too.
    /// </summary>
    public class LegacyValueManager
    {
        private readonly LegacyStructDefinition[] BUILT_IN_STRUCTS =
        {
            new LegacyStructDefinition("point", "x", "y"),
            new LegacyStructDefinition("color", "r", "g", "b")
        };

        List<LegacyStructDefinition> structs;
        Dictionary<string, LegacyValue> values;
        public int Count
        {
            get; private set;
        }

        public LegacyValueManager()
        {
            Count = 0;
            values = new Dictionary<string, LegacyValue>();

            structs = new List<LegacyStructDefinition>();
            structs.AddRange(BUILT_IN_STRUCTS);
        }
        /// <summary>
        /// Get a structure definition for a specific value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public LegacyStructDefinition GetStructForValue(LegacyValue value)
        {
            if (value.type != LegacyValueType.STRUCT)
                throw new ArgumentException("Value must be a struct instance.");
            return structs[value.information];
        }

        public string[] DefineValue(string expression)
        {
            string[] args = expression.Split(' ');

            if (args.Length < 1)
                throw new ArgumentException("No arguments specified to define expression.");

            if(args.Length == 1)
            {
                string valueName = args[0];
                return DefineValue(valueName, LegacyValueType.REGULAR, 0);
            } else
            {
                string valueName = args[args.Length - 1];
                string _type = args[0];

                if(structs.Any(sd => sd.name.Equals(_type)))
                {
                    int index = structs.IndexOf(structs.First(sd => sd.name.Equals(_type)));
                    return DefineValue(valueName, LegacyValueType.STRUCT, index);
                } else if(_type.ToUpper().Equals("DECIMAL"))
                {
                    if (!int.TryParse(args[1], out int precision))
                        throw new ArgumentException("No valid precision specified for decimal value.");
                    return DefineValue(valueName, LegacyValueType.DECIMAL, precision);
                } else
                {
                    return DefineValue(valueName, LegacyValueType.REGULAR, 0);
                }
            }
        }
        /// <summary>
        /// Define a value interally.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="information"></param>
        /// <returns>The commands to setup this value.</returns>
        public string[] DefineValue(string name, LegacyValueType type = LegacyValueType.REGULAR, int information = 0)
        {
            if(name.Any(c => !CommandLimits.SCOREBOARD_ALLOWED.Contains(c)))
                throw new ArgumentException($"Illegal character in value name \"{name}\"");
            if (name.Length > CommandLimits.SCOREBOARD_LIMIT)
                throw new ArgumentException($"Value name cannot be longer than {CommandLimits.SCOREBOARD_LIMIT} characters.");
            // Reserve 2 characters for the compressed struct parameters.
            if(type != LegacyValueType.REGULAR && name.Length > CommandLimits.SCOREBOARD_LIMIT - 2)
                throw new ArgumentException($"Typed value name cannot be longer than {CommandLimits.SCOREBOARD_LIMIT - 2} characters.");

            LegacyValue value = new LegacyValue(name, information, type);
            values.Add(name, value);
            Count++;

            string[] scoreboards = value.GetScoreboards(this);
            return (from s in scoreboards select $"scoreboard objectives add \"{s}\" dummy").ToArray();
        }

        public bool HasValue(string name)
        {
            int index;
            if ((index = name.IndexOf(':')) != -1)
            {
                if (name.EndsWith(":") || name.StartsWith(":"))
                    throw new ArgumentException("Value name cannot start or end with an accessor (: or .)");
                string main = name.Substring(0, index);
                string sub = name.Substring(index + 1);
                if (values.TryGetValue(main, out LegacyValue output))
                {
                    LegacyStructDefinition info = GetStructForValue(output);
                    return info.fields.Contains(sub);
                }
                else return false;
            }
            else
                return values.ContainsKey(name);
        }
        public bool TryGetValue(string name, out LegacyValue output)
        {
            int index;
            if((index = name.IndexOf(':')) != -1)
            {
                if (name.EndsWith(":") || name.StartsWith(":"))
                    throw new ArgumentException("Value name cannot start or end with an accessor (: or .)");
                string main = name.Substring(0, index);
                string sub = name.Substring(index + 1);
                if (values.TryGetValue(main, out output))
                {
                    LegacyStructDefinition info = GetStructForValue(output);
                    return info.fields.Contains(sub);
                }
                else return false;
            } else
                return values.TryGetValue(name, out output);
        }
        public LegacyValue GetValue(string name)
        {
            int index;
            if ((index = name.IndexOf(':')) != -1)
            {
                if (name.EndsWith(":") || name.StartsWith(":"))
                    throw new ArgumentException("Value name cannot start or end with an accessor (: or .)");
                string main = name.Substring(0, index);
                return values[main];
            }
            else
                return values[name];
        }
        public LegacyValue this[string valueName]
        {
            get { return values[valueName]; }
            private set { values[valueName] = value; }
        }

        public static string[] ExpressionAddConstant(LegacyValue source, string selector, Dynamic value)
        {
            if (source.type == LegacyValueType.REGULAR)
                return new[] { $"scoreboard players add {selector} {source} {value.data.i}" };
            else if (source.type == LegacyValueType.DECIMAL)
            {
                int p = source.information;
                string unit = ((int)Math.Pow(10, p)).ToString(); // 10^unit; 100:2 1000:3 etc...
                return new[]
                {
                    // Set decimal unit
$"scoreboard players set {selector} {LegacyExecutor.DECIMAL_UNIT} {unit}",

                    // Add whole/decimal part
$"scoreboard players add {selector} {source.WholePart} {value.GetWholePart()}",
$"scoreboard players add {selector} {source.DecimalPart} {value.GetDecimalPart(p)}",

                    // Carry leftover part, if any.
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} = {selector} {source.DecimalPart}",      // temp = value.d
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} /= {selector} {LegacyExecutor.DECIMAL_UNIT}",  // temp /= unit
$"scoreboard players operation {selector} {source.WholePart} += {selector} {LegacyExecutor.MATH_TEMP}",       // value.w += temp
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} *= {selector} {LegacyExecutor.DECIMAL_UNIT}",  // temp *= unit
$"scoreboard players operation {selector} {source.DecimalPart} -= {selector} {LegacyExecutor.MATH_TEMP}"      // value.d -= temp
                };
            }
            else return null;
        }
        public static string[] ExpressionSubtractConstant(LegacyValue source, string selector, Dynamic value)
        {
            if (source.type == LegacyValueType.REGULAR)
                return new[] { $"scoreboard players add {selector} {source} {value.data.i * -1}" };
            else if (source.type == LegacyValueType.DECIMAL)
            {
                if (value.data.d < 0.0)
                    return ExpressionAddConstant(source, selector, value.Inverse());

                int p = source.information;
                string unit = ((int)Math.Pow(10, p)).ToString(); // 10^unit; 100:2 1000:3 etc...
                string functionName = LegacyExecutor.DECIMAL_SUB_CARRY + source.name;

                string functionCallLine;
                if (selector.StartsWith("@p"))
                    functionCallLine = $"execute {selector} ~~~ execute @s[scores={{{source.DecimalPart}=..0}}] ~~~ function {functionName}";
                else
                    functionCallLine = $"execute {selector}[scores={{{source.DecimalPart}=..0}}] ~~~ function {functionName}";

                return new[]
                {
                    // Set decimal unit
$"scoreboard players set {selector} {LegacyExecutor.DECIMAL_UNIT} {unit}",

                    // Add whole/decimal part
$"scoreboard players add {selector} {source.WholePart} {value.GetWholePart()}",
$"scoreboard players add {selector} {source.DecimalPart} -{value.GetDecimalPart(p)}",

                    // Carry leftover part, if any.
                    functionCallLine
                };
            }
            else return null;
        }
        public static string[] ExpressionMultiplyConstant(LegacyValue source, string selector, Dynamic value)
        {
            if (source.type == LegacyValueType.REGULAR)
                return new[]
                {
$"scoreboard players set {selector} {LegacyExecutor.MATH_TEMP} {value.data.i}",
$"scoreboard players operation {selector} {source} *= {selector} {LegacyExecutor.MATH_TEMP}"
                };
            else if (source.type == LegacyValueType.DECIMAL)
            {
                int p = source.information;
                string unit = ((int)Math.Pow(10, p)).ToString(); // 10^unit; 100:2 1000:3 etc...
                return new[]
                {
                    // Set decimal unit
$"scoreboard players set {selector} {LegacyExecutor.DECIMAL_UNIT} {unit}",

                    // Add whole/decimal part
$"scoreboard players set {selector} {LegacyExecutor.MATH_TEMP} {value.GetWholePart()}",
$"scoreboard players operation {selector} {source.WholePart} *= {selector} {LegacyExecutor.MATH_TEMP}",
$"scoreboard players set {selector} {LegacyExecutor.MATH_TEMP} {value.GetDecimalPart()}",
$"scoreboard players operation {selector} {source.DecimalPart} *= {selector} {LegacyExecutor.MATH_TEMP}",

                    // Carry leftover part, if any.
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} = {selector} {source.DecimalPart}",      // temp = value.d
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} /= {selector} {LegacyExecutor.DECIMAL_UNIT}",  // temp /= unit
$"scoreboard players operation {selector} {source.WholePart} += {selector} {LegacyExecutor.MATH_TEMP}",       // value.w += temp
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} *= {selector} {LegacyExecutor.DECIMAL_UNIT}",  // temp *= unit
$"scoreboard players operation {selector} {source.DecimalPart} -= {selector} {LegacyExecutor.MATH_TEMP}"      // value.d -= temp
                };
            }
            else return null;
        }
        public static string[] ExpressionDivideConstant(LegacyValue source, string selector, Dynamic value)
        {
            if (source.type == LegacyValueType.REGULAR)
            {
                if (value.data.i == 0)
                    throw new Exception("Cannot divide by zero.");
                return new[]
                {
$"scoreboard players set {selector} {LegacyExecutor.MATH_TEMP} {value.data.i}",
$"scoreboard players operation {selector} {source} /= {selector} {LegacyExecutor.MATH_TEMP}"
                };
            }
            else if (source.type == LegacyValueType.DECIMAL)
            {
                throw new Exception("Decimal over decimal division is not supported.");
            }
            else return null;
        }
        public static string[] ExpressionModuloConstant(LegacyValue source, string selector, Dynamic value)
        {
            if (source.type == LegacyValueType.REGULAR)
            {
                if (value.data.i == 0)
                    throw new Exception("Cannot divide by zero.");
                return new[]
                {
$"scoreboard players set {selector} {LegacyExecutor.MATH_TEMP} {value.data.i}",
$"scoreboard players operation {selector} {source} %= {selector} {LegacyExecutor.MATH_TEMP}"
                };
            }
            else if (source.type == LegacyValueType.DECIMAL)
            {
                throw new Exception("Decimal over decimal modulus is not supported.");
            }
            else return null;
        }
        public static string[] ExpressionSetConstant(LegacyValue source, string selector, Dynamic value)
        {
            if (source.type == LegacyValueType.REGULAR)
            {
                return new[]
                {
$"scoreboard players set {selector} {source} {value.data.i}",
                };
            }
            else if (source.type == LegacyValueType.DECIMAL)
            {
                int precision = source.information;
                int mult = (int)Math.Pow(10, precision);
                long wholePart = value.GetWholePart();
                long decimalPart = (long)Math.Round(value.GetDecimalPart() * mult);
                return new[]
                {
$"scoreboard players set {selector} {source.WholePart} {wholePart}",
$"scoreboard players set {selector} {source.DecimalPart} {decimalPart}",
                };
            }
            else return null;
        }

        public static string[] ExpressionAddValue(LegacyValue source, LegacyValue b, string selector)
        {
            if (source.type == LegacyValueType.REGULAR)
                return new[] { $"scoreboard players operation {selector} {source} += {selector} {(b.type == LegacyValueType.DECIMAL ? b.WholePart : b.ToString())}" };
            else if (source.type == LegacyValueType.DECIMAL)
            {
                int p = source.information;
                string unit = ((int)Math.Pow(10, p)).ToString(); // 10^unit; 100:2 1000:3 etc...
                return new[]
                {
                    // Set decimal unit
$"scoreboard players set {selector} {LegacyExecutor.DECIMAL_UNIT} {unit}",

                    // Add whole/decimal part
$"scoreboard players operation {selector} {source.WholePart} += {selector} {b.WholePart}",
$"scoreboard players operation {selector} {source.DecimalPart} += {selector} {b.DecimalPart}",

                    // Carry leftover part, if any.
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} = {selector} {source.DecimalPart}",      // temp = value.d
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} /= {selector} {LegacyExecutor.DECIMAL_UNIT}",  // temp /= unit
$"scoreboard players operation {selector} {source.WholePart} += {selector} {LegacyExecutor.MATH_TEMP}",       // value.w += temp
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} *= {selector} {LegacyExecutor.DECIMAL_UNIT}",  // temp *= unit
$"scoreboard players operation {selector} {source.DecimalPart} -= {selector} {LegacyExecutor.MATH_TEMP}"      // value.d -= temp
                };
            }
            else return null;
        }
        public static string[] ExpressionSubtractValue(LegacyValue source, LegacyValue b, string selector)
        {
            if (source.type == LegacyValueType.REGULAR)
                return new[] { $"scoreboard players operation {selector} {source} -= {selector} {(b.type == LegacyValueType.DECIMAL ? b.WholePart : b.ToString())}" };
            else if (source.type == LegacyValueType.DECIMAL)
            {
                int p = source.information;
                string unit = ((int)Math.Pow(10, p)).ToString(); // 10^unit; 100:2 1000:3 etc...
                string functionName = LegacyExecutor.DECIMAL_SUB_CARRY + source.name;

                string functionCallLine;
                if (selector.StartsWith("@p"))
                    functionCallLine = $"execute {selector} ~~~ execute @s[scores={{{source.DecimalPart}=..0}}] ~~~ function {functionName}";
                else
                    functionCallLine = $"execute {selector}[scores={{{source.DecimalPart}=..0}}] ~~~ function {functionName}";

                return new[]
                {
                    // Set decimal unit
$"scoreboard players set {selector} {LegacyExecutor.DECIMAL_UNIT} {unit}",

                    // Add whole/decimal part
$"scoreboard players operation {selector} {source.WholePart} -= {selector} {b.WholePart}",
$"scoreboard players operation {selector} {source.DecimalPart} -= {selector} {b.DecimalPart}",

                    // Carry leftover part, if any.
                    functionCallLine
                };
            }
            else return null;
        }
        public static string[] ExpressionMultiplyValue(LegacyValue source, LegacyValue b, string selector)
        {
            if (source.type == LegacyValueType.REGULAR)
                return new[]
                {
$"scoreboard players operation {selector} {source} *= {selector} {(b.type==LegacyValueType.DECIMAL?b.WholePart:b.ToString())}"
                };
            else if (source.type == LegacyValueType.DECIMAL)
            {
                int p = source.information;
                string unit = ((int)Math.Pow(10, p)).ToString(); // 10^unit; 100:2 1000:3 etc...
                return new[]
                {
                    // Set decimal unit
$"scoreboard players set {selector} {LegacyExecutor.DECIMAL_UNIT} {unit}",

                    // Add whole/decimal part
$"scoreboard players operation {selector} {source.WholePart} *= {selector} {b.WholePart}",
$"scoreboard players operation {selector} {source.DecimalPart} *= {selector} {b.DecimalPart}",

                    // Carry leftover part, if any.
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} = {selector} {source.DecimalPart}",      // temp = value.d
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} /= {selector} {LegacyExecutor.DECIMAL_UNIT}",  // temp /= unit
$"scoreboard players operation {selector} {source.WholePart} += {selector} {LegacyExecutor.MATH_TEMP}",       // value.w += temp
$"scoreboard players operation {selector} {LegacyExecutor.MATH_TEMP} *= {selector} {LegacyExecutor.DECIMAL_UNIT}",  // temp *= unit
$"scoreboard players operation {selector} {source.DecimalPart} -= {selector} {LegacyExecutor.MATH_TEMP}"      // value.d -= temp
                };
            }
            else return null;
        }
        public static string[] ExpressionDivideValue(LegacyValue source, LegacyValue b, string selector)
        {
            if (source.type == LegacyValueType.REGULAR)
            {
                return new[]
                {
$"scoreboard players operation {selector} {source} /= {selector} {(b.type==LegacyValueType.DECIMAL?b.WholePart:b.ToString())}"
                };
            }
            else if (source.type == LegacyValueType.DECIMAL || b.type == LegacyValueType.DECIMAL)
            {
                throw new Exception("Decimal over decimal division is not supported.");
            }
            else return null;
        }
        public static string[] ExpressionModuloValue(LegacyValue source, LegacyValue b, string selector)
        {
            if (source.type == LegacyValueType.REGULAR)
            {
                return new[]
                {
$"scoreboard players operation {selector} {source} %= {selector} {(b.type==LegacyValueType.DECIMAL?b.WholePart:b.ToString())}"
                };
            }
            else if (source.type == LegacyValueType.DECIMAL || b.type == LegacyValueType.DECIMAL)
            {
                throw new Exception("Decimal over decimal modulus is not supported.");
            }
            else return null;
        }
        public static string[] ExpressionSetValue(LegacyValue source, LegacyValue b, string selector)
        {
            if (source.type == LegacyValueType.REGULAR)
            {
                return new[]
                {
$"scoreboard players operation {selector} {source} = {selector} {b}",
                };
            }
            else if (source.type == LegacyValueType.DECIMAL)
            {
                if (b.type == LegacyValueType.DECIMAL)
                {
                    return new[] {
$"scoreboard players operation {selector} {source.WholePart} = {selector} {b.WholePart}",
$"scoreboard players operation {selector} {source.DecimalPart} = {selector} {b.DecimalPart}",
                    };
                }
                else if (b.type == LegacyValueType.REGULAR)
                {
                    return new[] {
$"scoreboard players operation {selector} {source.WholePart} = {selector} {b}",
$"scoreboard players set {selector} {source.DecimalPart} 0",
                    };
                }
                else throw new NotImplementedException("Assigning decimal value from struct field is not supported yet.");
            }
            else return null;
        }
    }
    /// <summary>
    /// An operation on two values. If (value % 2 == 1), then a temp scoreboard objective is needed.
    /// </summary>
    enum LegacyValueOperation : byte
    {
        ADD = 0,
        SUB = 2,
        MUL = 3,
        SET = 4,
        DIV = 5,
        MOD = 7,
    }
    public enum LegacyValueType
    {
        REGULAR,
        DECIMAL,
        STRUCT, // Built-in types use this endpoint too.
    }
    public struct LegacyValue
    {
        public string name;
        public int information;
        public LegacyValueType type;

        public LegacyValue(string name, int information = 0, LegacyValueType type = LegacyValueType.REGULAR)
        {
            this.name = name;
            this.information = information;
            this.type = type;
        }
        public LegacyValue(string name, Dynamic copyFrom)
        {
            if (copyFrom.type == Dynamic.Type.STRING)
            {
                this.name = copyFrom.data.s;
                information = 0;
                type = LegacyValueType.REGULAR;
            }
            else if (copyFrom.type == Dynamic.Type.DECIMAL)
            {
                this.name = name;
                information = copyFrom.data.d.GetPrecision();
                type = LegacyValueType.DECIMAL;
            }
            else
            {
                this.name = name;
                information = 0;
                type = LegacyValueType.REGULAR;
            }
        }
        /// <summary>
        /// Get all the scoreboard entry names that this value uses.
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public string[] GetScoreboards(LegacyValueManager manager)
        {
            switch (type)
            {
                case LegacyValueType.REGULAR:
                    return new string[] { name };
                case LegacyValueType.DECIMAL:
                    return new string[] { $"{name}:w", $"{name}:d" };
                case LegacyValueType.STRUCT:
                    LegacyStructDefinition info = manager.GetStructForValue(this);
                    string[] result = new string[info.internalFields.Length];
                    for (int i = 0; i < result.Length; i++)
                        result[i] = $"{name}:{info.internalFields[i]}";
                    return result;
                default:
                    return null;
            }
        }
        /// <summary>
        /// Converts this value to its representation as rawtext.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public JSONRawTerm[] ToRawText(LegacyValueManager manager, string selector)
        {
            switch (type)
            {
                case LegacyValueType.REGULAR:
                    return new JSONRawTerm[] { new JSONScore(selector, name) };
                case LegacyValueType.DECIMAL:
                    return new JSONRawTerm[]
                    {
                        new JSONScore(selector, WholePart),
                        new JSONText("."),
                        new JSONScore(selector, DecimalPart)
                    };
                case LegacyValueType.STRUCT:
                    LegacyStructDefinition info = manager.GetStructForValue(this);
                    JSONRawTerm[] ret = new JSONRawTerm[(info.fields.Length * 3 - 1) + 2];
                    ret[0] = new JSONText("(");
                    ret[ret.Length - 1] = new JSONText(")");
                    int write = 1;
                    for (int i = 0; i < ret.Length; i++)
                    {
                        ret[write++] = new JSONText(info.fields[i] + ": ");
                        ret[write++] = new JSONScore(selector, name + ":" + info.internalFields[i]);
                        if (i + 1 != ret.Length)
                            ret[write++] = new JSONText(", ");
                    }
                    return ret;
                default:
                    return new JSONRawTerm[] { new JSONScore(selector, name) };
            }
        }
        public override string ToString()
        {
            return name;
        }

        public string WholePart
        {
            get { return name + ":w"; }
        }
        public string DecimalPart
        {
            get { return name + ":d"; }
        }
    }
    public struct LegacyStructDefinition
    {
        private static readonly char[] TRANSLATION_KEYS = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_".ToCharArray();
        private static readonly int MAX_FIELDS = TRANSLATION_KEYS.Length;

        public string name;
        public string[] fields;
        public char[] internalFields;

        public LegacyStructDefinition(string name, params string[] fields)
        {
            this.name = name;
            fields = fields.Distinct().ToArray();

            if (fields.Any(f => f.Any(c => !TRANSLATION_KEYS.Contains(c))))
                throw new ArgumentException("One of the fields in this struct has a non-standard character.");

            int len = fields.Length;
            if (len > MAX_FIELDS)
                throw new ArgumentException("Too many fields in user-defined struct. Max is " + MAX_FIELDS);

            this.fields = fields;

            internalFields = new char[len];
            for(int i = 0; i < len; i++)
            {
                string current = fields[i];
                if (current.Length == 1)
                    internalFields[i] = current[0];
                else internalFields[i] = TRANSLATION_KEYS[i];
            }
        }
    }
}