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

        public ScoreboardValue(string baseName, ScoreboardManager manager, Statement forExceptions)
        {
            int len = baseName.Length;
            int max = GetMaxNameLength();

            if (len > max)
                throw new StatementException(forExceptions, $"Cannot define variable named '{baseName}'. Max length for this type is {max}.");

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
                return new ScoreboardValueInteger(RETURN_NAME, sb, forExceptions);
            else if (literal is TokenBooleanLiteral)
                return new ScoreboardValueBoolean(RETURN_NAME, sb, forExceptions);
            else if (literal is TokenDecimalLiteral)
            {
                float number = (literal as TokenDecimalLiteral).number;
                int precision = number.GetPrecision();
                return new ScoreboardValueDecimal(RETURN_NAME, precision, sb, forExceptions);
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
        public abstract string[] CommandsRawTextSetup(string accessor, string selector, ref int index, string prefix = "");
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
        public abstract JSONRawTerm[] ToRawText(string accessor, string selector, ref int index, string prefix = "");

        public string[] CommandsFromOperation(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor, TokenArithmatic.Type type)
        {
            switch (type)
            {
                case TokenArithmatic.Type.ADD:
                    return CommandsAdd(selector, other, thisAccessor, thatAccessor);
                case TokenArithmatic.Type.SUBTRACT:
                    return CommandsSub(selector, other, thisAccessor, thatAccessor);
                case TokenArithmatic.Type.MULTIPLY:
                    return CommandsMul(selector, other, thisAccessor, thatAccessor);
                case TokenArithmatic.Type.DIVIDE:
                    return CommandsDiv(selector, other, thisAccessor, thatAccessor);
                case TokenArithmatic.Type.MODULO:
                    return CommandsMod(selector, other, thisAccessor, thatAccessor);
                case TokenArithmatic.Type.SWAP:
                    return CommandsSwap(selector, other, thisAccessor, thatAccessor);
                default:
                    break;
            }
            return null;
        }
        public abstract string[] CommandsAddLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions);
        public abstract string[] CommandsSubLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions);


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
        public ScoreboardValueInteger(string baseName, ScoreboardManager manager, Statement forExceptions) : base(baseName, manager, forExceptions) { }

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
        public override string[] CommandsRawTextSetup(string accessor, string selector, ref int index, string prefix = "")
        {
            return new string[0];
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, ref int index, string prefix = "")
        {
            return new[] { new JSONScore(selector, prefix + baseName) };
        }

        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH;
        public override string[] GetAccessibleNames() =>
            new[] { baseName };

        public override string[] CommandsAddLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions)
        {
            if (other is TokenNumberLiteral)
                return new[] { Command.ScoreboardAdd(selector, baseName, (other as TokenNumberLiteral).GetNumberInt()) };
            else
                throw new StatementException(forExceptions, "Attempted to add invalid literal to value '" + baseName + "'");
        }
        public override string[] CommandsSubLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions)
        {
            if (other is TokenNumberLiteral)
                return new[] { Command.ScoreboardSubtract(selector, baseName, (other as TokenNumberLiteral).GetNumberInt()) };
            else
                throw new StatementException(forExceptions, "Attempted to subtract invalid literal from value '" + baseName + "'");
        }

        public override string[] CommandsSet(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpSet(selector, this, other)};

            if(other is ScoreboardValueDecimal)
            {
                // floor the decimal value
                ScoreboardValue tempBase = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, this, other),
                    Command.ScoreboardOpDiv(selector, this, tempBase)
                };

                manager.ReleaseTemp();
                return commands;
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
                ScoreboardValue tempBase = manager.RequestTemp();
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp, other),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpAdd(selector, this, temp)
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
                return new[] { Command.ScoreboardOpSub(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp, other),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpSub(selector, this, temp)
                };

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
                return new[] { Command.ScoreboardOpMul(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                // set this to the whole part of the decimal value (floor)
                ScoreboardValue tempBase = manager.RequestTemp();
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp, other),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpMul(selector, this, temp)
                };

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
                return new[] { Command.ScoreboardOpDiv(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp, other),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpDiv(selector, this, temp)
                };

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
                return new[] { Command.ScoreboardOpMod(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, temp, other),
                    Command.ScoreboardOpDiv(selector, temp, tempBase),
                    Command.ScoreboardOpMod(selector, this, temp)
                };

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
                return new[] { Command.ScoreboardOpSwap(selector, this, other) };

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, temp, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSwap(selector, baseName, other), // now both values are to the wrong base.
                    Command.ScoreboardOpDiv(selector, baseName, temp),
                    Command.ScoreboardOpMul(selector, other, temp)
                };

                manager.ReleaseTemp();
                return commands;
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
        public ScoreboardValueTime(string baseName, ScoreboardManager manager, Statement forExceptions) : base(baseName, manager, forExceptions) { }

        public override string[] CommandsRawTextSetup(string accessor, string selector, ref int index, string prefix = "")
        {
            string minutes = SB_MINUTES + index;
            string seconds = SB_SECONDS + index;
            string temporary = SB_TEMP + index;
            string constant = SB_CONST + index;

            manager.AddToStringScoreboards(this,
                new ScoreboardValueInteger(minutes, manager, null),
                new ScoreboardValueInteger(seconds, manager, null),
                new ScoreboardValueInteger(temporary, manager, null),
                new ScoreboardValueInteger(constant, manager, null));

            return new string[]
            {
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
        public override JSONRawTerm[] ToRawText(string accessor, string selector, ref int index, string prefix = "")
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
        public readonly int precision;

        public const string SB_WHOLE = "_mcc_d_whole";
        public const string SB_PART = "_mcc_d_part";
        public const string SB_TEMP = "_mcc_d_temp";
        public const string SB_BASE = "_mcc_d_base";

        public ScoreboardValueDecimal(string baseName, int precision, ScoreboardManager manager, Statement forExceptions) : base(baseName, manager, forExceptions)
        {
            this.precision = precision;
        }

        public override string[] CommandsDefine(string prefix = "")
        {
            return new[] {
                Command.ScoreboardCreateObjective(prefix + baseName)
            };
        }
        public override string[] CommandsInit(string prefix = "")
        {
            return new[] {
                Command.ScoreboardAdd("@a", prefix + baseName, 0)
            };
        }
        public override string[] CommandsSetLiteral(string accessor, string selector, TokenLiteral token, string prefix = "")
        {
            if (token is TokenStringLiteral)
                return new string[] { };

            if (token is TokenIntegerLiteral)
            {
                int integer = (token as TokenIntegerLiteral).number;
                return new string[] {
                    Command.ScoreboardSet(selector, prefix + baseName, integer.ToFixedPoint(precision)),
                };
            }
            if (token is TokenDecimalLiteral)
            {
                TokenDecimalLiteral literal = token as TokenDecimalLiteral;
                return new string[] {
                    Command.ScoreboardSet(selector, prefix + baseName, literal.number.ToFixedPoint(precision))
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
            int number = _number.ToFixedPoint(precision);

            return new Tuple<ScoresEntry[], string[]>(new[]
            {
                new ScoresEntry(temp, range)
            }, new[]
            {
                Command.ScoreboardOpSet(selector, temp, baseName),
                Command.ScoreboardSubtract(selector, temp, number)
            });
        }

        public override string[] CommandsRawTextSetup(string accessor, string selector, ref int index, string prefix = "")
        {
            string whole = SB_WHOLE + index;
            string part = SB_PART + index;
            string temporary = SB_TEMP + index;
            string tempBase = SB_BASE + index;

            manager.AddToStringScoreboards(this,
                new ScoreboardValueInteger(whole, manager, null),
                new ScoreboardValueInteger(part, manager, null),
                new ScoreboardValueInteger(temporary, manager, null),
                new ScoreboardValueInteger(tempBase, manager, null));

            return new string[]
            {
                Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                Command.ScoreboardOpSet(selector, temporary, baseName),
                Command.ScoreboardOpDiv(selector, temporary, tempBase),
                Command.ScoreboardOpSet(selector, whole, temporary),
                Command.ScoreboardOpMul(selector, temporary, tempBase),
                Command.ScoreboardOpSet(selector, part, baseName),
                Command.ScoreboardOpSub(selector, part, temporary),
                Command.ScoreboardSet(selector, temporary, -1),
                Command.Execute(selector, Coord.here, Coord.here, Coord.here,
                    Command.Execute($"@s[scores={{{part}=..-1}}]",  Coord.here, Coord.here, Coord.here,
                    Command.ScoreboardOpMul("@s", part, temporary)))
            };
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, ref int index, string prefix = "")
        {
            string whole = SB_WHOLE + index;
            string part = SB_PART + index;

            return new JSONRawTerm[]
            {
                new JSONScore(selector, whole),
                new JSONText("."),
                new JSONScore(selector, part)
            };
        }

        public override int GetMaxNameLength() =>
           MAX_NAME_LENGTH - 2;
        public override string[] GetAccessibleNames() =>
            new[] { baseName };

        public override string[] CommandsAddLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions)
        {
            if (other is TokenIntegerLiteral)
            {
                int value = (other as TokenIntegerLiteral);
                value = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardAdd(selector, baseName, value) };
            }
            else if(other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral);
                int number = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardAdd(selector, baseName, number) };
            }
            else
                throw new StatementException(forExceptions, "Attempted to add invalid literal to value '" + baseName + "'");
        }
        public override string[] CommandsSubLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions)
        {
            if (other is TokenIntegerLiteral)
            {
                int value = (other as TokenIntegerLiteral);
                value = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardSubtract(selector, baseName, value) };
            }
            else if (other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral);
                int number = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardSubtract(selector, baseName, number) };
            }
            else
                throw new StatementException(forExceptions, "Attempted to add invalid literal to value '" + baseName + "'");
        }

        public override string[] CommandsSet(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, baseName, other),
                    Command.ScoreboardOpMul(selector, baseName, tempBase),
                };

                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();
                return new[]
                {
                    Command.ScoreboardOpSet(selector, baseName, other)
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
                ScoreboardValue tempAccumulator = manager.RequestTemp();
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, tempAccumulator, other),
                    Command.ScoreboardOpMul(selector, tempAccumulator, tempBase),
                    Command.ScoreboardOpAdd(selector, baseName, tempAccumulator),
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                string[] commands = new string[]
                {
                    Command.ScoreboardOpAdd(selector, baseName, other),
                };
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
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpMul(selector, tempBase, other),
                    Command.ScoreboardOpSub(selector, baseName, tempBase),
                };

                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                string[] commands = new string[]
                {
                    Command.ScoreboardOpSub(selector, baseName, other),
                };
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
                string[] commands = new string[]
                {
                    Command.ScoreboardOpMul(selector, baseName, other),
                };
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpMul(selector, baseName, other),
                    Command.ScoreboardOpDiv(selector, baseName, tempBase)
                };
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
                string[] commands = new string[]
                {
                    Command.ScoreboardOpDiv(selector, baseName, other),
                };
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpMul(selector, baseName, tempBase),
                    Command.ScoreboardOpDiv(selector, baseName, other)
                };
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
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpMul(selector, tempBase, other),
                    Command.ScoreboardOpMod(selector, baseName, tempBase)
                };
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                string[] commands = new string[]
                {
                    Command.ScoreboardOpMod(selector, baseName, other),
                };
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
                ScoreboardValue temp = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, temp, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSwap(selector, baseName, other), // now both values are to the wrong base.
                    Command.ScoreboardOpMul(selector, baseName, temp),
                    Command.ScoreboardOpDiv(selector, other, temp)
                };

                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal)
            {
                return new[]
                {
                    Command.ScoreboardOpSwap(selector, baseName, other)
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
        public ScoreboardValueBoolean(string baseName, ScoreboardManager manager, Statement forExceptions) : base(baseName, manager, forExceptions) { }

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

        public override string[] CommandsRawTextSetup(string accessor, string selector, ref int index, string prefix = "")
        {
            return new string[0];
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, ref int index, string prefix = "")
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

        public ScoreboardValueStruct(string baseName, StructDefinition structure, ScoreboardManager manager, Statement forExceptions) : base(baseName, manager, forExceptions)
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
        public ScoreboardValue FullyResolveAccessor(string accessor)
        {
            if (accessor.IndexOf(':') == -1)
                throw new Exception("Struct accessor '" + accessor + "' didn't have a field specified.");

            return structure.GetFieldFromAccessor(accessor);
        }

        public override string[] CommandsDefine(string prefix = "")
        {
            return structure.GetFields(baseName).SelectMany(f => f.CommandsDefine(prefix)).ToArray();
        }
        public override string[] CommandsInit(string prefix = "")
        {
            return structure.GetFields(baseName).SelectMany(f => f.CommandsInit(prefix)).ToArray();
        }
        public override string[] CommandsSetLiteral(string accessor, string selector, TokenLiteral token, string prefix = "")
        {
            ScoreboardValue value = FullyResolveAccessor(accessor);
            if (value == this)
                return null;
            return value.CommandsSetLiteral("", selector, token, prefix);
        }
        public override Tuple<ScoresEntry[], string[]> CompareToLiteral(string accessor, string selector, TokenCompare.Type ctype, TokenNumberLiteral literal, string prefix = "")
        {
            ScoreboardValue value = FullyResolveAccessor(accessor);
            if(value == this)
                return null;
            return value.CompareToLiteral(accessor, selector, ctype, literal, prefix);
        }

        public override string[] CommandsRawTextSetup(string accessor, string selector, ref int index, string prefix = "")
        {
            if (accessor.IndexOf(':') == -1)
            {
                ScoreboardValue[] values = structure.GetFields(baseName);
                List<string> commands = new List<string>();
                foreach(ScoreboardValue f in values)
                {
                    commands.AddRange(f.CommandsRawTextSetup(accessor, selector, ref index, prefix));
                    index++;
                }
                return commands.ToArray();
            }
            ScoreboardValue value = FullyResolveAccessor(accessor);
            return value.CommandsRawTextSetup("", selector, ref index, prefix);
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, ref int index, string prefix = "")
        {
            if (accessor.IndexOf(':') == -1)
            {
                ScoreboardValue[] values = structure.GetFields(baseName);
                List<JSONRawTerm> commands = new List<JSONRawTerm>();
                foreach (ScoreboardValue f in values)
                {
                    commands.AddRange(f.ToRawText(accessor, selector, ref index, prefix));
                    index++;
                }
                return commands.ToArray();
            }
            ScoreboardValue value = FullyResolveAccessor(accessor);
            return value.ToRawText("", selector, ref index, prefix);
        }

        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH - 5; // someName:ab:c
        public override string[] GetAccessibleNames()
        {
            string[] qualified = structure.GetFullyQualifiedNames(baseName).ToArray();
            string[] ret = new string[qualified.Length + 1];
            for (int i = 0; i < qualified.Length; i++)
                ret[i] = qualified[i];
            ret[qualified.Length] = baseName;
            return ret;
        }

        public override string[] CommandsAddLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions)
        {
            ScoreboardValue value = FullyResolveAccessor(thisAccessor);
            if (value == this)
                return null;
            return value.CommandsAddLiteral(selector, other, thisAccessor, forExceptions);
        }
        public override string[] CommandsSubLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions)
        {
            ScoreboardValue value = FullyResolveAccessor(thisAccessor);
            if (value == this)
                return null;
            return value.CommandsSubLiteral(selector, other, thisAccessor, forExceptions);
        }

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
            if (other is ScoreboardValueStruct || (thisAccessor == null && thatAccessor == null))
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