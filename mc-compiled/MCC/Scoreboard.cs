using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// A scoreboard value that can be written to.
    /// </summary>
    public abstract class ScoreboardValue : ICloneable
    {
        public const string RETURN_NAME = "_mcc_retn";
        public const int MAX_NAME_LENGTH = 16;
        public string baseName;
        internal readonly ScoreboardManager manager;

        public ScoreboardValue(string baseName, ScoreboardManager manager)
        {
            int len = baseName.Length;
            int max = GetMaxNameLength();

            if (len > max)
                throw new Exception($"Cannot define variable named '{baseName}'. Max length for this type is {max}.");

            this.manager = manager;
            this.baseName = baseName;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
        /// <summary>
        /// Returns a shallow memberwise clone of some value as a return value.
        /// </summary>
        /// <param name="returning"></param>
        /// <returns></returns>
        public static ScoreboardValue GetReturnValue(ScoreboardValue returning)
        {
            ScoreboardValue clone = returning.Clone() as ScoreboardValue;
            clone.baseName = RETURN_NAME;
            return clone;
        }
        /// <summary>
        /// Create a return value based off of a literal.
        /// </summary>
        /// <param name="returning"></param>
        /// <returns></returns>
        public static ScoreboardValue GetReturnValue(TokenLiteral literal, ScoreboardManager sb, Statement forExceptions)
        {
            if (literal is TokenStringLiteral)
                throw new StatementException(forExceptions, "Cannot return a string.");
            if(literal is TokenSelectorLiteral)
                throw new StatementException(forExceptions, "Cannot return a selector.");

            if (literal is TokenIntegerLiteral)
                return new ScoreboardValueInteger(RETURN_NAME, sb);
            else if (literal is TokenBooleanLiteral)
                return new ScoreboardValueBoolean(RETURN_NAME, sb);
            else if (literal is TokenDecimalLiteral)
            {
                float number = (literal as TokenDecimalLiteral).number;
                int precision = number.GetPrecision();
                return new ScoreboardValueDecimal(RETURN_NAME, precision, sb);
            }

            throw new StatementException(forExceptions, "Cannot return this literal.");
        }

        public static implicit operator string(ScoreboardValue value) => value.baseName;


        /// <summary>
        /// Get the commands to define this value.
        /// </summary>
        /// <returns></returns>
        public abstract string[] CommandsDefine(string prefix = "");
        /// <summary>
        /// Get the commands to initialize this value for all players.
        /// </summary>
        /// <returns></returns>
        public abstract string[] CommandsInit(string prefix = "");
        /// <summary>
        /// Get the commands to set this value to any given literal.
        /// </summary>
        /// <param name="accessor">The variable name used to access this. Might be field of struct.</param>
        /// <param name="selector">The selector to set this for.</param>
        /// <param name="token">The literal that is being used as the value.</param>
        /// <returns></returns>
        /// <param name="prefix"></param>
        
        public abstract string[] CommandsSetLiteral(string accessor, string selector, TokenLiteral token, string prefix = "");
        public abstract Tuple<ScoresEntry[], string[]> CompareToLiteral(string accessor, string selector, TokenCompare.Type ctype, TokenNumberLiteral literal, string prefix = "");
        /// <summary>
        /// Setup temporary variables before printing this variable as rawtext.
        /// </summary>
        /// <param name="accessor">The string used to access this value.</param>
        /// <param name="selector"></param>
        /// <param name="index">The index to use to identify scoreboard values.</param>
        /// <returns></returns>
        /// <param name="prefix"></param>
        public abstract string[] CommandsRawTextSetup(string accessor, string selector, int index, string prefix = "");
        /// <summary>
        /// Convert this scoreboard value to its expression as rawtext.
        /// Basically the Minecraft equivalent of ToString().
        /// Called immediately after ToRawTextSetup(string).
        /// </summary>
        /// <param name="accessor">The string used to access this value.</param>
        /// <param name="selector"></param>
        /// <param name="index">The index to use to identify scoreboard values.</param>
        /// <returns></returns>
        /// <param name="prefix"></param>
        public abstract JSONRawTerm[] ToRawText(string accessor, string selector, int index, string prefix = "");

        /// <summary>
        /// this = other
        /// </summary>
        /// <param name="other"></param>
        /// <param name="thisAccessor">The way this value was accessed.</param>
        /// <param name="thatAccessor">The way the other value was accessed.</param>
        /// <returns></returns>
        public abstract string[] CommandsSet(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor);
        /// <summary>
        /// this += other
        /// </summary>
        /// <param name="other"></param>
        /// <param name="thisAccessor">The way this value was accessed.</param>
        /// <param name="thatAccessor">The way the other value was accessed.</param>
        /// <returns></returns>
        public abstract string[] CommandsAdd(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor);
        /// <summary>
        /// this -= other
        /// </summary>
        /// <param name="other"></param>
        /// <param name="thisAccessor">The way this value was accessed.</param>
        /// <param name="thatAccessor">The way the other value was accessed.</param>
        /// <returns></returns>
        public abstract string[] CommandsSub(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor);
        /// <summary>
        /// this *= other
        /// </summary>
        /// <param name="other"></param>
        /// <param name="thisAccessor">The way this value was accessed.</param>
        /// <param name="thatAccessor">The way the other value was accessed.</param>
        /// <returns></returns>
        public abstract string[] CommandsMul(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor);
        /// <summary>
        /// this /= other
        /// </summary>
        /// <param name="other"></param>
        /// <param name="thisAccessor">The way this value was accessed.</param>
        /// <param name="thatAccessor">The way the other value was accessed.</param>
        /// <returns></returns>
        public abstract string[] CommandsDiv(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor);
        /// <summary>
        /// this %= other
        /// </summary>
        /// <param name="other"></param>
        /// <param name="thisAccessor">The way this value was accessed.</param>
        /// <param name="thatAccessor">The way the other value was accessed.</param>
        /// <returns></returns>
        public abstract string[] CommandsMod(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor);
        /// <summary>
        /// Swap this and other.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="thisAccessor">The way this value was accessed.</param>
        /// <param name="thatAccessor">The way the other value was accessed.</param>
        /// <returns></returns>
        public abstract string[] CommandsSwap(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor);

        /// <summary>
        /// Get the maximum name length for this scoreboard value type.
        /// </summary>
        /// <returns></returns>
        public abstract int GetMaxNameLength();
        /// <summary>
        /// Get the accessible names that can be used to reference this variable.
        /// </summary>
        /// <returns></returns>
        public abstract string[] GetAccessibleNames();
    }

    public class ScoreboardValueInteger : ScoreboardValue
    {
        public ScoreboardValueInteger(string baseName, ScoreboardManager manager) : base(baseName, manager) { }

        public override string[] CommandsDefine(string prefix = "")
        {
            return new[] { Command.ScoreboardCreateObjective(prefix + baseName) };
        }
        public override string[] CommandsInit(string prefix = "")
        {
            return new[] { Command.ScoreboardAdd("@a", prefix + baseName, 0) };
        }
        public override string[] CommandsSetLiteral(string accessor, string selector, TokenLiteral token, string prefix = "")
        {
            if (token is TokenStringLiteral)
                return new string[] { };

            if (token is TokenNumberLiteral)
                return new string[] { Command.ScoreboardSet(selector, prefix + baseName,
                    (token as TokenNumberLiteral).GetNumberInt())};

            if (token is TokenBooleanLiteral)
                return new string[] { };

            return new string[] { };
        }
        public override Tuple<ScoresEntry[], string[]> CompareToLiteral(string accessor, string selector, TokenCompare.Type ctype, TokenNumberLiteral literal, string prefix = "")
        {
            int value = literal.GetNumberInt();

            Range range;
            switch (ctype)
            {
                case TokenCompare.Type.EQUAL:
                    range = new Range(value, false);
                    break;
                case TokenCompare.Type.NOT_EQUAL:
                    range = new Range(value, true);
                    break;
                case TokenCompare.Type.LESS_THAN:
                    range = new Range(null, value - 1);
                    break;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    range = new Range(null, value);
                    break;
                case TokenCompare.Type.GREATER_THAN:
                    range = new Range(value + 1, null);
                    break;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    range = new Range(value, null);
                    break;
                default:
                    range = new Range();
                    break;
            }

            return new Tuple<ScoresEntry[], string[]>(new[]
            {
                new ScoresEntry(prefix + baseName, range)
            }, new string[0]);
        }
        public override string[] CommandsRawTextSetup(string accessor, string selector, int index, string prefix = "")
        {
            return new string[0];
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, int index, string prefix = "")
        {
            return new[] { new JSONScore(selector, prefix + baseName) };
        }

        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH;
        public override string[] GetAccessibleNames() =>
            new[] { baseName };

        public override string[] CommandsSet(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpSet(selector, this, other)};

            if(other is ScoreboardValueDecimal)
            {
                // set this to the whole part of the decimal value (floor)
                ScoreboardValueDecimal cast = other as ScoreboardValueDecimal;
                return new[] { Command.ScoreboardOpSet(selector, this, cast.WholeName) };
            }

            if(other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsSet(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsAdd(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpAdd(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                // set this to the whole part of the decimal value (floor)
                ScoreboardValueDecimal cast = other as ScoreboardValueDecimal;
                return new[] { Command.ScoreboardOpAdd(selector, this, cast.WholeName) };
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsAdd(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsSub(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpSub(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                // set this to the whole part of the decimal value (floor)
                ScoreboardValueDecimal cast = other as ScoreboardValueDecimal;
                return new[] { Command.ScoreboardOpSub(selector, this, cast.WholeName) };
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsSub(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsMul(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpMul(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                // set this to the whole part of the decimal value (floor)
                ScoreboardValueDecimal cast = other as ScoreboardValueDecimal;
                return new[] { Command.ScoreboardOpMul(selector, this, cast.WholeName) };
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsMul(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsDiv(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpDiv(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                // set this to the whole part of the decimal value (floor)
                ScoreboardValueDecimal cast = other as ScoreboardValueDecimal;
                return new[] { Command.ScoreboardOpDiv(selector, this, cast.WholeName) };
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsDiv(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsMod(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpMod(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                // set this to the whole part of the decimal value (floor)
                ScoreboardValueDecimal cast = other as ScoreboardValueDecimal;
                return new[] { Command.ScoreboardOpMod(selector, this, cast.WholeName) };
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsMod(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsSwap(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpSwap(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                // set this to the whole part of the decimal value (floor)
                ScoreboardValueDecimal cast = other as ScoreboardValueDecimal;
                return new[] { Command.ScoreboardOpSwap(selector, this, cast.WholeName) };
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsSwap(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
    }
    public sealed class ScoreboardValueTime : ScoreboardValueInteger
    {
        public const string SB_MINUTES = "_mcc_t_mins";
        public const string SB_SECONDS = "_mcc_t_secs";
        public const string SB_TEMP = "_mcc_t_temp";
        public const string SB_CONST = "_mcc_t_const";
        public ScoreboardValueTime(string baseName, ScoreboardManager manager) : base(baseName, manager) { }

        public override string[] CommandsRawTextSetup(string accessor, string selector, int index, string prefix = "")
        {
            string minutes = SB_MINUTES + index;
            string seconds = SB_SECONDS + index;
            string temporary = SB_TEMP + index;
            string constant = SB_CONST + index;

            return new string[]
            {
                Command.ScoreboardCreateObjective(minutes),
                Command.ScoreboardCreateObjective(seconds),
                Command.ScoreboardCreateObjective(temporary),
                Command.ScoreboardCreateObjective(constant),

                Command.ScoreboardSet("@a", constant, 20),
                Command.ScoreboardOpSet(selector, temporary, prefix + baseName),
                Command.ScoreboardOpDiv(selector, temporary, constant),
                Command.ScoreboardOpSet(selector, seconds, temporary),
                Command.ScoreboardSet("@a", constant, 60),
                Command.ScoreboardOpDiv(selector, temporary, constant),
                Command.ScoreboardOpSet(selector, minutes, temporary),
                Command.ScoreboardOpMul(selector, temporary, constant),
                Command.ScoreboardOpSub(selector, seconds, temporary)
            };
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, int index, string prefix = "")
        {
            string minutes = SB_MINUTES + index;
            string seconds = SB_SECONDS + index;

            return new JSONRawTerm[]
            {
                new JSONScore(selector, minutes),
                new JSONText(":"),
                new JSONScore(selector, seconds)
            };
        }

        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH;
        public override string[] GetAccessibleNames() =>
            new[] { baseName };
    }
    public sealed class ScoreboardValueDecimal : ScoreboardValue
    {
        public const string WHOLE_SUFFIX = ":w";
        public const string DECIMAL_SUFFIX = ":d";
        public string WholeName { get => baseName + WHOLE_SUFFIX; }
        public string DecimalName { get => baseName + DECIMAL_SUFFIX; }

        public readonly int precision;

        public ScoreboardValueDecimal(string baseName, int precision, ScoreboardManager manager) : base(baseName, manager)
        {
            this.precision = precision;
        }

        public override string[] CommandsDefine(string prefix = "")
        {
            return new[] {
                Command.ScoreboardCreateObjective(prefix + WholeName),
                Command.ScoreboardCreateObjective(prefix + DecimalName)
            };
        }
        public override string[] CommandsInit(string prefix = "")
        {
            return new[] { Command.ScoreboardAdd("@a", prefix + baseName, 0) };
        }
        public override string[] CommandsSetLiteral(string accessor, string selector, TokenLiteral token, string prefix = "")
        {
            if (token is TokenStringLiteral)
                return new string[] { };

            if (token is TokenIntegerLiteral)
            {
                int integer = (token as TokenIntegerLiteral).number;
                return new string[] {
                    Command.ScoreboardSet(selector, prefix + WholeName, integer),
                    Command.ScoreboardSet(selector, prefix + DecimalName, 0)
                };
            }
            if (token is TokenDecimalLiteral)
            {
                TokenDecimalLiteral literal = token as TokenDecimalLiteral;
                float number = literal.number.FixPoint(precision);
                int wholePart = (int)Math.Floor(number);
                int decimalPart = (number - wholePart).ToFixedInt(precision);
                return new string[] {
                    Command.ScoreboardSet(selector, prefix + WholeName, wholePart),
                    Command.ScoreboardSet(selector, prefix + DecimalName, decimalPart)
                };
            }

            if (token is TokenBooleanLiteral)
                return new string[] { };

            return new string[] { };
        }
        public override Tuple<ScoresEntry[], string[]> CompareToLiteral(string accessor, string selector, TokenCompare.Type ctype, TokenNumberLiteral literal, string prefix = "")
        {
            ScoreboardValueInteger temp = manager.RequestTemp();

            Range range;
            switch (ctype)
            {
                case TokenCompare.Type.EQUAL:
                    range = new Range(0, false);
                    break;
                case TokenCompare.Type.NOT_EQUAL:
                    range = new Range(0, true);
                    break;
                case TokenCompare.Type.LESS_THAN:
                    range = new Range(null, -1);
                    break;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    range = new Range(null, 0);
                    break;
                case TokenCompare.Type.GREATER_THAN:
                    range = new Range(1, null);
                    break;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    range = new Range(0, null);
                    break;
                default:
                    range = new Range();
                    break;
            }

            int exp = (int)Math.Pow(10, precision);
            float _number = literal.GetNumber();
            int number = (int)Math.Round(_number * (float)exp);

            return new Tuple<ScoresEntry[], string[]>(new[]
            {
                new ScoresEntry(temp, range)
            }, new[]
            {
                Command.ScoreboardSet(selector, temp, exp),
                Command.ScoreboardOpMul(selector, temp, WholeName),
                Command.ScoreboardOpAdd(selector, temp, DecimalName),
                Command.ScoreboardSubtract(selector, temp, number)
            });
        }

        public override string[] CommandsRawTextSetup(string accessor, string selector, int index, string prefix = "")
        {
            return new string[0];
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, int index, string prefix = "")
        {
            return new JSONRawTerm[]
            {
                new JSONScore(selector, prefix + WholeName),
                new JSONText("."),
                new JSONScore(selector, prefix + DecimalName)
            };
        }

        public override int GetMaxNameLength() =>
           MAX_NAME_LENGTH - 2;
        public override string[] GetAccessibleNames() =>
            new[] { baseName };

        public override string[] CommandsSet(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
            {
                return new[]
                {
                    Command.ScoreboardOpSet(selector, WholeName, other),
                    Command.ScoreboardSet(selector, DecimalName, 0)
                };
            }

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValueDecimal b = other as ScoreboardValueDecimal;
                return new[]
                {
                    Command.ScoreboardOpSet(selector, WholeName, b.WholeName),
                    Command.ScoreboardOpSet(selector, DecimalName, b.DecimalName)
                };
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsSet(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsAdd(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
            {
                return new[]
                {
                    Command.ScoreboardOpAdd(selector, WholeName, other)
                };
            }

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValueDecimal b = other as ScoreboardValueDecimal;
                ScoreboardValue tempRemainder = manager.RequestTemp();
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpAdd(selector, WholeName, b.WholeName),
                    Command.ScoreboardOpAdd(selector, DecimalName, b.DecimalName),
                    Command.ScoreboardOpSet(selector, tempRemainder, DecimalName),
                    Command.ScoreboardOpDiv(selector, tempRemainder, tempBase),
                    Command.ScoreboardOpAdd(selector, WholeName, tempRemainder),
                    Command.ScoreboardOpMul(selector, tempRemainder, tempBase),
                    Command.ScoreboardOpSub(selector, DecimalName, tempRemainder)
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsAdd(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsSub(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
            {
                return new[]
                {
                    Command.ScoreboardOpSub(selector, WholeName, other)
                };
            }

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValueDecimal b = other as ScoreboardValueDecimal;
                ScoreboardValue temp = manager.RequestTemp();
                ScoreboardValue temp2 = manager.RequestTemp();
                ScoreboardValue tempBase = manager.RequestTemp();

                thisAccessor = thisAccessor.Replace(':', '#');
                thatAccessor = thatAccessor.Replace(':', '#');
                string functionName = "carry_" + thisAccessor + "_" + thatAccessor;
                CommandFile file = new CommandFile(functionName, "_math");

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, -1),
                    Command.ScoreboardOpSub(selector, WholeName, b.WholeName),
                    Command.ScoreboardOpSub(selector, DecimalName, b.DecimalName),
                    Command.Execute($"{selector}[scores={{{DecimalName}=..0}}]", Coord.here, Coord.here, Coord.here,
                        Command.Function(file)),
                };
                
                file.Add(new string[]
                {
                    Command.ScoreboardOpMul("@s", DecimalName, tempBase), // invert sign
                    Command.ScoreboardOpSet("@s", temp2, DecimalName),
                    Command.ScoreboardSet("@s", tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpAdd("@s", DecimalName, tempBase), // prep for ceil operation
                    Command.ScoreboardOpSet("@s", temp, DecimalName),
                    Command.ScoreboardOpDiv("@s", temp, tempBase),
                    Command.ScoreboardOpSub("@s", WholeName, temp),
                    Command.ScoreboardOpMul("@s", temp, tempBase),
                    Command.ScoreboardOpSub("@s", temp, temp2),
                    Command.ScoreboardOpSet("@s", DecimalName, temp)
                });

                manager.executor.AddExtraFile(file);
                manager.ReleaseTemp();
                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsSub(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsMul(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
            {
                ScoreboardValueInteger b = other as ScoreboardValueInteger;
                ScoreboardValue temp = manager.RequestTemp();
                ScoreboardValue temp2 = manager.RequestTemp();
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp2, b),

                    Command.ScoreboardOpSet(selector, temp, WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpMul(selector, temp2, tempBase),
                    Command.ScoreboardOpAdd(selector, temp, DecimalName),
                    Command.ScoreboardOpMul(selector, temp, temp2),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpSet(selector, WholeName, temp),
                    Command.ScoreboardOpSet(selector, DecimalName, temp),
                    Command.ScoreboardOpDiv(selector, WholeName, tempBase),
                    Command.ScoreboardOpSet(selector, temp, WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpSub(selector, DecimalName, temp),
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValueDecimal b = other as ScoreboardValueDecimal;
                ScoreboardValue temp = manager.RequestTemp();
                ScoreboardValue temp2 = manager.RequestTemp();
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp, WholeName),
                    Command.ScoreboardOpSet(selector, temp2, b.WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpMul(selector, temp2, tempBase),
                    Command.ScoreboardOpAdd(selector, temp, DecimalName),
                    Command.ScoreboardOpAdd(selector, temp2, b.DecimalName),
                    Command.ScoreboardOpMul(selector, temp, temp2),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpSet(selector, WholeName, temp),
                    Command.ScoreboardOpSet(selector, DecimalName, temp),
                    Command.ScoreboardOpDiv(selector, WholeName, tempBase),
                    Command.ScoreboardOpSet(selector, temp, WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpSub(selector, DecimalName, temp),
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsMul(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsDiv(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
            {
                ScoreboardValueInteger b = other as ScoreboardValueInteger;
                ScoreboardValue temp = manager.RequestTemp();
                ScoreboardValue temp2 = manager.RequestTemp();
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp2, b),

                    Command.ScoreboardOpSet(selector, temp, WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpMul(selector, temp2, tempBase),
                    Command.ScoreboardOpAdd(selector, temp, DecimalName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpDiv(selector, temp, temp2),
                    Command.ScoreboardOpSet(selector, WholeName, temp),
                    Command.ScoreboardOpSet(selector, DecimalName, temp),
                    Command.ScoreboardOpDiv(selector, WholeName, tempBase),
                    Command.ScoreboardOpSet(selector, temp, WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpSub(selector, DecimalName, temp),
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValueDecimal b = other as ScoreboardValueDecimal;
                ScoreboardValue temp = manager.RequestTemp();
                ScoreboardValue temp2 = manager.RequestTemp();
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp, WholeName),
                    Command.ScoreboardOpSet(selector, temp2, b.WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpMul(selector, temp2, tempBase),
                    Command.ScoreboardOpAdd(selector, temp, DecimalName),
                    Command.ScoreboardOpAdd(selector, temp2, b.DecimalName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpDiv(selector, temp, temp2),
                    Command.ScoreboardOpSet(selector, WholeName, temp),
                    Command.ScoreboardOpSet(selector, DecimalName, temp),
                    Command.ScoreboardOpDiv(selector, WholeName, tempBase),
                    Command.ScoreboardOpSet(selector, temp, WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpSub(selector, DecimalName, temp),
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsDiv(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsMod(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
            {
                ScoreboardValueInteger b = other as ScoreboardValueInteger;
                ScoreboardValue temp = manager.RequestTemp();
                ScoreboardValue temp2 = manager.RequestTemp();
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp2, b),

                    Command.ScoreboardOpSet(selector, temp, WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpMul(selector, temp2, tempBase),
                    Command.ScoreboardOpAdd(selector, temp, WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpMul(selector, temp2, tempBase),
                    Command.ScoreboardOpMod(selector, temp, temp2),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpSet(selector, DecimalName, temp),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpSet(selector, WholeName, temp),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpSub(selector, DecimalName, temp),
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValueDecimal b = other as ScoreboardValueDecimal;
                ScoreboardValue temp = manager.RequestTemp();
                ScoreboardValue temp2 = manager.RequestTemp();
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp, WholeName),
                    Command.ScoreboardOpSet(selector, temp2, b.WholeName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpMul(selector, temp2, tempBase),
                    Command.ScoreboardOpAdd(selector, temp, DecimalName),
                    Command.ScoreboardOpAdd(selector, temp2, b.DecimalName),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpMul(selector, temp2, tempBase),
                    Command.ScoreboardOpMod(selector, temp, temp2),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpSet(selector, DecimalName, temp),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpSet(selector, WholeName, temp),
                    Command.ScoreboardOpMul(selector, temp, tempBase),
                    Command.ScoreboardOpSub(selector, DecimalName, temp),
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsMod(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
        public override string[] CommandsSwap(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
            {
                string[] commands = new string[]
                {
                    Command.ScoreboardOpSwap(selector, WholeName, other),
                    Command.ScoreboardSet(selector, DecimalName, 0)
                };
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValueDecimal b = other as ScoreboardValueDecimal;
                return new[]
                {
                    Command.ScoreboardOpSwap(selector, WholeName, b.WholeName),
                    Command.ScoreboardOpSwap(selector, DecimalName, b.DecimalName)
                };
            }

            if (other is ScoreboardValueStruct)
            {
                ScoreboardValueStruct cast = other as ScoreboardValueStruct;
                ScoreboardValue b = cast.FullyResolveAccessor(thatAccessor);
                return CommandsSwap(selector, b, thisAccessor, "");
            }

            return new string[0];
        }
    }
    public sealed class ScoreboardValueBoolean : ScoreboardValueInteger
    {
        public ScoreboardValueBoolean(string baseName, ScoreboardManager manager) : base(baseName, manager) { }

        public override string[] CommandsDefine(string prefix = "")
        {
            return new[] { Command.ScoreboardCreateObjective(prefix + baseName) };
        }
        public override string[] CommandsInit(string prefix = "")
        {
            return new[] { Command.ScoreboardAdd("@a", prefix + baseName, 0) }; // init to false
        }
        public override string[] CommandsSetLiteral(string accessor, string selector, TokenLiteral token, string prefix = "")
        {
            if (token is TokenStringLiteral)
                return new string[] { };

            if (token is TokenNumberLiteral)
                return new string[] { Command.ScoreboardSet(selector, prefix + baseName,
                    (token as TokenNumberLiteral).GetNumberInt() % 2)};

            if (token is TokenBooleanLiteral)
                return new string[] { Command.ScoreboardSet(selector, prefix + baseName,
                    (token as TokenBooleanLiteral).boolean ? 1 : 0)};

            return new string[] { };
        }

        public override string[] CommandsRawTextSetup(string accessor, string selector, int index, string prefix = "")
        {
            return new string[0];
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, int index, string prefix = "")
        {
            return new JSONRawTerm[]
            {
                new JSONScore(selector, prefix + baseName)
            };
        }

        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH;
        public override string[] GetAccessibleNames() =>
            new[] { baseName };
    }
    public sealed class ScoreboardValueStruct : ScoreboardValue
    {
        public readonly StructDefinition structure;

        public ScoreboardValueStruct(string baseName, StructDefinition structure, ScoreboardManager manager) : base(baseName, manager)
        {
            this.structure = structure;
        }
        /// <summary>
        /// Resolve an accessor of a field to its internal scoreboard value name.
        /// something:firstField -> something:aa
        /// something:firstField -> something:ba
        /// </summary>
        /// <param name="accessor"></param>
        /// <returns></returns>
        public string ResolveAccessor(string accessor) =>
            structure.GetAccessor(baseName, accessor);
        /// <summary>
        /// Fully resolve a field:name accessor to its appropriate scoreboard value.
        /// </summary>
        /// <param name="accessor"></param>
        /// <returns></returns>
        public ScoreboardValue FullyResolveAccessor(string accessor) =>
            structure.GetFieldFromAccessor(accessor);

        public override string[] CommandsDefine(string prefix = "")
        {
            return structure.GetFields().SelectMany(f => f.CommandsDefine(prefix)).ToArray();
        }
        public override string[] CommandsInit(string prefix = "")
        {
            return structure.GetFields().SelectMany(f => f.CommandsInit(prefix)).ToArray();
        }
        public override string[] CommandsSetLiteral(string accessor, string selector, TokenLiteral token, string prefix = "")
        {
            ScoreboardValue value = FullyResolveAccessor(accessor);
            return value.CommandsSetLiteral("", selector, token, prefix);
        }
        public override Tuple<ScoresEntry[], string[]> CompareToLiteral(string accessor, string selector, TokenCompare.Type ctype, TokenNumberLiteral literal, string prefix = "")
        {
            ScoreboardValue value = FullyResolveAccessor(accessor);
            return value.CompareToLiteral(accessor, selector, ctype, literal, prefix);
        }

        public override string[] CommandsRawTextSetup(string accessor, string selector, int index, string prefix = "")
        {
            ScoreboardValue value = FullyResolveAccessor(accessor);
            return value.CommandsRawTextSetup("", selector, index, prefix);
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, int index, string prefix = "")
        {
            ScoreboardValue value = FullyResolveAccessor(accessor);
            return value.ToRawText("", selector, index, prefix);
        }

        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH - 5; // someName:ab:c
        public override string[] GetAccessibleNames() =>
            structure.GetFullyQualifiedNames(baseName);

        public override string[] CommandsSet(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if(other is ScoreboardValueStruct && thisAccessor == null && thatAccessor == null)
            {
                ScoreboardValueStruct structB = other as ScoreboardValueStruct;
                if (structure.Equals(structB.structure))
                {
                    List<string> commands = new List<string>();
                    int count = structure.GetFieldCount();
                    for (int i = 0; i < count; i++)
                    {
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(baseName, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(baseName, i);
                        commands.AddRange(fieldDst.CommandsSet(selector, fieldSrc, thisAccessor, thatAccessor));
                    }
                    return commands.ToArray();
                }
            }

            ScoreboardValue b = FullyResolveAccessor(thisAccessor);
            return b.CommandsSet(selector, other, "", "");
        }
        public override string[] CommandsAdd(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueStruct && thisAccessor == null && thatAccessor == null)
            {
                ScoreboardValueStruct structB = other as ScoreboardValueStruct;
                if (structure.Equals(structB.structure))
                {
                    List<string> commands = new List<string>();
                    int count = structure.GetFieldCount();
                    for (int i = 0; i < count; i++)
                    {
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(baseName, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(baseName, i);
                        commands.AddRange(fieldDst.CommandsAdd(selector, fieldSrc, thisAccessor, thatAccessor));
                    }
                    return commands.ToArray();
                }
            }

            ScoreboardValue b = FullyResolveAccessor(thisAccessor);
            return b.CommandsAdd(selector, other, "", "");
        }
        public override string[] CommandsSub(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueStruct && thisAccessor == null && thatAccessor == null)
            {
                ScoreboardValueStruct structB = other as ScoreboardValueStruct;
                if (structure.Equals(structB.structure))
                {
                    List<string> commands = new List<string>();
                    int count = structure.GetFieldCount();
                    for (int i = 0; i < count; i++)
                    {
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(baseName, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(baseName, i);
                        commands.AddRange(fieldDst.CommandsSub(selector, fieldSrc, thisAccessor, thatAccessor));
                    }
                    return commands.ToArray();
                }
            }

            ScoreboardValue b = FullyResolveAccessor(thisAccessor);
            return b.CommandsSub(selector, other, "", "");
        }
        public override string[] CommandsMul(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueStruct && thisAccessor == null && thatAccessor == null)
            {
                ScoreboardValueStruct structB = other as ScoreboardValueStruct;
                if (structure.Equals(structB.structure))
                {
                    List<string> commands = new List<string>();
                    int count = structure.GetFieldCount();
                    for (int i = 0; i < count; i++)
                    {
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(baseName, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(baseName, i);
                        commands.AddRange(fieldDst.CommandsMul(selector, fieldSrc, thisAccessor, thatAccessor));
                    }
                    return commands.ToArray();
                }
            }

            ScoreboardValue b = FullyResolveAccessor(thisAccessor);
            return b.CommandsMul(selector, other, "", "");
        }
        public override string[] CommandsDiv(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueStruct && thisAccessor == null && thatAccessor == null)
            {
                ScoreboardValueStruct structB = other as ScoreboardValueStruct;
                if (structure.Equals(structB.structure))
                {
                    List<string> commands = new List<string>();
                    int count = structure.GetFieldCount();
                    for (int i = 0; i < count; i++)
                    {
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(baseName, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(baseName, i);
                        commands.AddRange(fieldDst.CommandsDiv(selector, fieldSrc, thisAccessor, thatAccessor));
                    }
                    return commands.ToArray();
                }
            }

            ScoreboardValue b = FullyResolveAccessor(thisAccessor);
            return b.CommandsDiv(selector, other, "", "");
        }
        public override string[] CommandsMod(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueStruct && thisAccessor == null && thatAccessor == null)
            {
                ScoreboardValueStruct structB = other as ScoreboardValueStruct;
                if (structure.Equals(structB.structure))
                {
                    List<string> commands = new List<string>();
                    int count = structure.GetFieldCount();
                    for (int i = 0; i < count; i++)
                    {
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(baseName, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(baseName, i);
                        commands.AddRange(fieldDst.CommandsMod(selector, fieldSrc, thisAccessor, thatAccessor));
                    }
                    return commands.ToArray();
                }
            }

            ScoreboardValue b = FullyResolveAccessor(thisAccessor);
            return b.CommandsMod(selector, other, "", "");
        }
        public override string[] CommandsSwap(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueStruct && thisAccessor == null && thatAccessor == null)
            {
                ScoreboardValueStruct structB = other as ScoreboardValueStruct;
                if (structure.Equals(structB.structure))
                {
                    List<string> commands = new List<string>();
                    int count = structure.GetFieldCount();
                    for (int i = 0; i < count; i++)
                    {
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(baseName, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(baseName, i);
                        commands.AddRange(fieldDst.CommandsSwap(selector, fieldSrc, thisAccessor, thatAccessor));
                    }
                    return commands.ToArray();
                }
            }

            ScoreboardValue b = FullyResolveAccessor(thisAccessor);
            return b.CommandsSwap(selector, other, "", "");
        }
    }
}