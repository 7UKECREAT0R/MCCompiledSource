using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// A scoreboard value that can be written to.
    /// </summary>
    public abstract class ScoreboardValue : ICloneable
    {
        public static readonly char[] SUPPORTED_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        /// <summary>
        /// Convert a string to a standardized hash.
        /// </summary>
        /// <param name="input">The string to hash.</param>
        /// <returns>A unique identifier for the string that consists of 8 characters.</returns>
        public static string StandardizedHash(string input)
        {
            int hash = input.GetHashCode();
            byte[] bytes = BitConverter.GetBytes(hash);
            char[] chars = new char[8];

            for (int i = 0; i < 4; i++)
            {
                byte b = bytes[i];
                byte lower = (byte)((b << 2) % SUPPORTED_CHARS.Length);
                byte upper = (byte)((b >> 2) % SUPPORTED_CHARS.Length);

                char c1 = SUPPORTED_CHARS[lower];
                char c2 = SUPPORTED_CHARS[upper];

                chars[(i * 2)] = c1;
                chars[(i * 2) + 1] = c2;
            }

            return new string(chars);
        }

        public const string RETURN_NAME = "_mcc_retn";
        public const int MAX_NAME_LENGTH = 16;

        string baseName;
        string aliasName = null;

        /// <summary>
        /// The internal name that represents the scoreboard objective in the compiled result.
        /// </summary>
        public string Name
        {
            get => baseName;
            set
            {
                if (value.Length > 16)
                    baseName = StandardizedHash(value);
                else
                    baseName = value;
            }
        }
        /// <summary>
        /// The name used to reference this variable in the code.
        /// </summary>
        public string AliasName
        {
            get => aliasName ?? baseName;
            set => aliasName = value;
        }

        /// <summary>
        /// Require this variable's internal name to be hashed and hidden behind an alias.
        /// </summary>
        public void ForceHash()
        {
            aliasName = baseName;
            baseName = StandardizedHash(aliasName);
        }

        public List<IAttribute> attributes;
        public readonly Clarifier clarifier;
        public readonly ScoreboardManager.ValueType valueType;
        internal readonly ScoreboardManager manager;

        public ScoreboardValue(string baseName, bool global, ScoreboardManager.ValueType valueType, ScoreboardManager manager, Statement forExceptions)
        {
            int len = baseName.Length;

            this.manager = manager;
            this.Name = baseName;

            // hash was given to baseName
            if (baseName.Length > 16)
                aliasName = baseName;

            this.attributes = new List<IAttribute>();
            this.clarifier = new Clarifier(global);
            this.valueType = valueType;
        }
        /// <summary>
        /// Add attributes to this scoreboard value.
        /// </summary>
        /// <param name="attributes">The attributes to add.</param>
        /// <returns>This object for chaining.</returns>
        public ScoreboardValue WithAttributes(IEnumerable<IAttribute> attributes)
        {
            this.attributes.AddRange(attributes);

            foreach (IAttribute attribute in attributes)
                attribute.OnAddedValue(this);

            return this;
        }
        /// <summary>
        /// Perform a fully implemented deep clone of this scoreboard value.
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Returns a shallow memberwise clone of some value as a return value.
        /// </summary>
        /// <param name="returning"></param>
        /// <returns></returns>
        public static ScoreboardValue AsReturnValue(ScoreboardValue returning)
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
        public static ScoreboardValue AsReturnValue(TokenLiteral literal, ScoreboardManager sb, Statement forExceptions)
        {
            if (literal is TokenStringLiteral)
                throw new StatementException(forExceptions, "Cannot return a string.");
            if(literal is TokenSelectorLiteral)
                throw new StatementException(forExceptions, "Cannot return a selector.");

            if (literal is TokenIntegerLiteral)
                return new ScoreboardValueInteger(RETURN_NAME, false, sb, forExceptions);
            else if (literal is TokenBooleanLiteral)
                return new ScoreboardValueBoolean(RETURN_NAME, false, sb, forExceptions);
            else if (literal is TokenDecimalLiteral)
            {
                float number = (literal as TokenDecimalLiteral).number;
                int precision = number.GetPrecision();
                return new ScoreboardValueDecimal(RETURN_NAME, precision, false, sb, forExceptions);
            }

            throw new StatementException(forExceptions, "Cannot return this literal.");
        }

        public static implicit operator string(ScoreboardValue value) => value.Name;

        /// <summary>
        /// Gets the keyword associated with this scoreboard value type. Used by the language server to communicate variable types.
        /// </summary>
        /// <returns></returns>
        public abstract string GetTypeKeyword();
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

        /// <summary>
        /// Compare this scoreboard value to another literal value.
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="selector"></param>
        /// <param name="ctype"></param>
        /// <param name="literal"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
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

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Name.GetHashCode();
                hashCode += (int)valueType;

                return hashCode;
            }
        }
    }

    public class ScoreboardValueInteger : ScoreboardValue
    {
        public ScoreboardValueInteger(string name, bool global, ScoreboardManager manager, Statement forExceptions,
            ScoreboardManager.ValueType valueType = ScoreboardManager.ValueType.INT) : base(name, global, valueType, manager, forExceptions) { }

        public override string GetTypeKeyword() => "int";
        public override string[] CommandsDefine(string prefix = "")
        {
            return new[] { Command.ScoreboardCreateObjective(prefix + Name, AliasName) };
        }
        public override string[] CommandsInit(string prefix = "")
        {
            return new[] { Command.ScoreboardAdd("@a", prefix + Name, 0) };
        }
        public override string[] CommandsSetLiteral(string accessor, string selector, TokenLiteral token, string prefix = "")
        {
            if (token == null || token is TokenNullLiteral)
                return new string[] { Command.ScoreboardSet(selector, prefix + Name, 0) };

            if (token is TokenStringLiteral)
                return new string[] { };

            if (token is TokenNumberLiteral number)
                return new string[] { Command.ScoreboardSet(selector, prefix + Name, number.GetNumberInt())};

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
                new ScoresEntry(prefix + Name, range)
            }, new string[0]);
        }
        public override string[] CommandsRawTextSetup(string accessor, string selector, ref int index, string prefix = "")
        {
            return new string[0];
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, ref int index, string prefix = "")
        {
            return new[] { new JSONScore(selector, prefix + Name) };
        }

        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH;
        public override string[] GetAccessibleNames() =>
            new[] { AliasName };

        public override string[] CommandsAddLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions)
        {
            if (other is TokenNumberLiteral)
                return new[] { Command.ScoreboardAdd(selector, Name, (other as TokenNumberLiteral).GetNumberInt()) };
            else
                throw new StatementException(forExceptions, "Attempted to add invalid literal to value '" + Name + "'");
        }
        public override string[] CommandsSubLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions)
        {
            if (other is TokenNumberLiteral)
                return new[] { Command.ScoreboardSubtract(selector, Name, (other as TokenNumberLiteral).GetNumberInt()) };
            else
                throw new StatementException(forExceptions, "Attempted to subtract invalid literal from value '" + Name + "'");
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
                    Command.ScoreboardOpSwap(selector, Name, other), // now both values are to the wrong base.
                    Command.ScoreboardOpDiv(selector, Name, temp),
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
        public ScoreboardValueTime(string name, bool global, ScoreboardManager manager, Statement forExceptions) :
            base(name, global, manager, forExceptions, ScoreboardManager.ValueType.TIME) { }

        public override string GetTypeKeyword() => "time";
        public override string[] CommandsRawTextSetup(string accessor, string selector, ref int index, string prefix = "")
        {
            string minutes = SB_MINUTES + index;
            string seconds = SB_SECONDS + index;
            string temporary = SB_TEMP + index;
            string constant = SB_CONST + index;

            manager.AddToStringScoreboards(this,
                new ScoreboardValueInteger(minutes, false, manager, null),
                new ScoreboardValueInteger(seconds, false, manager, null),
                new ScoreboardValueInteger(temporary, false, manager, null),
                new ScoreboardValueInteger(constant, false, manager, null));

            return new string[]
            {
                Command.ScoreboardSet("@a", constant, 20),
                Command.ScoreboardOpSet(selector, temporary, prefix + Name),
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

                // append an extra 0 if its a single number.
                new JSONScore(selector, seconds).CreateVariant(
                    new[] { new JSONText(":0") },
                    new Range(0, 9),
                    new[] { new JSONText(":") }),

                new JSONScore(selector, seconds)
            };
        }

        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH;
        public override string[] GetAccessibleNames() =>
            new[] { AliasName };
    }
    public sealed class ScoreboardValueDecimal : ScoreboardValue
    {
        public readonly int precision;

        public const string SB_WHOLE = "_mcc_d_whole";
        public const string SB_PART = "_mcc_d_part";
        public const string SB_TEMP = "_mcc_d_temp";
        public const string SB_BASE = "_mcc_d_base";

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="precision">The precision of the value.</param>
        /// <param name="manager">The ScoreboardManager managing this new value.</param>
        /// <param name="forExceptions">The statement that caused an exception, if any.</param>
        public ScoreboardValueDecimal(string name, int precision, bool global, ScoreboardManager manager, Statement forExceptions) :
            base(name, global, ScoreboardManager.ValueType.DECIMAL, manager, forExceptions)
        {
            this.precision = precision;
        }
        /// <summary>
        /// Balances a decimal value so that its precision is equal to this scoreboard value's precision.
        /// Outputs a scoreboard value holding a copied value of 'other', but with precision balanced.
        /// </summary>
        /// <param name="selector">The selector to use.</param>
        /// <param name="other">The other value to be balanced.</param>
        /// <param name="balancedCopy">[IF RETURNED TRUE] The scoreboard value holding the newly balanced copy of 'other'.</param>
        /// <param name="commands">[IF RETURNED TRUE] The commands needed to balance the copy.</param>
        /// <returns>If a balance was performed.</returns>
        public bool BalancePrecisionWith(string selector, ScoreboardValueDecimal other,
            out ScoreboardValueDecimal balancedCopy, out string[] commands)
        {
            if (precision == other.precision)
            {
                balancedCopy = null;
                commands = null;
                return false;
            }

            if (precision > other.precision)
            {
                int precisionDiff = precision - other.precision;
                ScoreboardValue temp1 = other.manager.RequestTemp();
                ScoreboardValue temp2 = other.manager.RequestTemp(other);
                commands = new string[]
                {
                    Command.ScoreboardSet(selector, temp1, (int)Math.Pow(10, precisionDiff)),
                    Command.ScoreboardOpSet(selector, temp2, other.Name),
                    Command.ScoreboardOpMul(selector, temp2, temp1)
                };
                other.manager.ReleaseTemp();
                other.manager.ReleaseTemp();
                balancedCopy = temp2 as ScoreboardValueDecimal;
                return true;
            } else
            {
                // precision < other.precision
                int precisionDiff = other.precision - precision;
                ScoreboardValue temp1 = other.manager.RequestTemp();
                ScoreboardValue temp2 = other.manager.RequestTemp(other);
                commands = new string[]
                {
                    Command.ScoreboardSet(selector, temp1, (int)Math.Pow(10, precisionDiff)),
                    Command.ScoreboardOpSet(selector, temp2, other.Name),
                    Command.ScoreboardOpDiv(selector, temp2, temp1)
                };
                other.manager.ReleaseTemp();
                other.manager.ReleaseTemp();
                balancedCopy = temp2 as ScoreboardValueDecimal;
                return true;
            }
        }
        /// <summary>
        /// Basically Command.ScoreboardOpSet but balances 'other' to fit into this scoreboard value's precision properly.
        /// </summary>
        /// <param name="selector">The selector to use.</param>
        /// <param name="other"></param>
        /// <returns></returns>
        public string[] BalancePrecisionInto(string selector, ScoreboardValueDecimal other)
        {
            if (precision == other.precision)
            {
                return new[] { Command.ScoreboardOpSet(selector, Name, other) };
            }

            if (precision > other.precision)
            {
                int precisionDiff = precision - other.precision;
                ScoreboardValue temp1 = other.manager.RequestTemp();
                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, temp1, (int)Math.Pow(10, precisionDiff)),
                    Command.ScoreboardOpSet(selector, Name, other.Name),
                    Command.ScoreboardOpMul(selector, Name, temp1)
                };
                other.manager.ReleaseTemp();
                return commands;
            }
            else
            {
                // precision < other.precision
                int precisionDiff = other.precision - precision;
                ScoreboardValue temp1 = other.manager.RequestTemp();
                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, temp1, (int)Math.Pow(10, precisionDiff)),
                    Command.ScoreboardOpSet(selector, Name, other.Name),
                    Command.ScoreboardOpDiv(selector, Name, temp1)
                };
                other.manager.ReleaseTemp();
                return commands;
            }
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }
        public override string GetTypeKeyword() => "decimal " + precision;
        public override string[] CommandsDefine(string prefix = "")
        {
            return new[] {
                Command.ScoreboardCreateObjective(prefix + Name)
            };
        }
        public override string[] CommandsInit(string prefix = "")
        {
            return new[] {
                Command.ScoreboardAdd("@a", prefix + Name, 0)
            };
        }
        public override string[] CommandsSetLiteral(string accessor, string selector, TokenLiteral token, string prefix = "")
        {
            if (token == null || token is TokenNullLiteral)
                return new string[] { Command.ScoreboardSet(selector, prefix + Name, 0) };

            if (token is TokenStringLiteral)
                return new string[] { };

            if (token is TokenIntegerLiteral)
            {
                int integer = (token as TokenIntegerLiteral).number;
                return new string[] {
                    Command.ScoreboardSet(selector, prefix + Name, integer.ToFixedPoint(precision)),
                };
            }
            if (token is TokenDecimalLiteral)
            {
                TokenDecimalLiteral literal = token as TokenDecimalLiteral;
                return new string[] {
                    Command.ScoreboardSet(selector, prefix + Name, literal.number.ToFixedPoint(precision))
                };
            }

            if (token is TokenBooleanLiteral)
                return new string[] { };

            return new string[] { };
        }
        public override Tuple<ScoresEntry[], string[]> CompareToLiteral(string accessor, string selector, TokenCompare.Type ctype, TokenNumberLiteral literal, string prefix = "")
        {
            int exp = (int)Math.Pow(10, precision);
            float _number = literal.GetNumber();
            int number = _number.ToFixedPoint(precision);

            Range range;
            switch (ctype)
            {
                case TokenCompare.Type.EQUAL:
                    range = new Range(number, false);
                    break;
                case TokenCompare.Type.NOT_EQUAL:
                    range = new Range(number, true);
                    break;
                case TokenCompare.Type.LESS_THAN:
                    range = new Range(null, number - 1);
                    break;
                case TokenCompare.Type.LESS_OR_EQUAL:
                    range = new Range(null, number);
                    break;
                case TokenCompare.Type.GREATER_THAN:
                    range = new Range(number + 1, null);
                    break;
                case TokenCompare.Type.GREATER_OR_EQUAL:
                    range = new Range(number, null);
                    break;
                default:
                    range = new Range();
                    break;
            }

            return new Tuple<ScoresEntry[], string[]>(new[]
            {
                new ScoresEntry(prefix + Name, range)
            }, new string[0]);
        }

        public override string[] CommandsRawTextSetup(string accessor, string selector, ref int index, string prefix = "")
        {
            string whole = SB_WHOLE + index;
            string part = SB_PART + index;
            string temporary = SB_TEMP + index;
            string tempBase = SB_BASE + index;

            manager.AddToStringScoreboards(this,
                new ScoreboardValueInteger(whole, false, manager, null),
                new ScoreboardValueInteger(part, false, manager, null),
                new ScoreboardValueInteger(temporary, false, manager, null),
                new ScoreboardValueInteger(tempBase, false, manager, null));

            return new string[]
            {
                Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                Command.ScoreboardOpSet(selector, temporary, Name),
                Command.ScoreboardOpDiv(selector, temporary, tempBase),
                Command.ScoreboardOpSet(selector, whole, temporary),
                Command.ScoreboardOpMul(selector, temporary, tempBase),
                Command.ScoreboardOpSet(selector, part, Name),
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
            new[] { AliasName };

        public override string[] CommandsAddLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions)
        {
            if (other is TokenIntegerLiteral)
            {
                int value = (other as TokenIntegerLiteral);
                value = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardAdd(selector, Name, value) };
            }
            else if(other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral);
                int number = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardAdd(selector, Name, number) };
            }
            else
                throw new StatementException(forExceptions, "Attempted to add invalid literal to value '" + Name + "'");
        }
        public override string[] CommandsSubLiteral(string selector, TokenLiteral other, string thisAccessor, Statement forExceptions)
        {
            if (other is TokenIntegerLiteral)
            {
                int value = (other as TokenIntegerLiteral);
                value = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardSubtract(selector, Name, value) };
            }
            else if (other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral);
                int number = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardSubtract(selector, Name, number) };
            }
            else
                throw new StatementException(forExceptions, "Attempted to add invalid literal to value '" + Name + "'");
        }

        public override string[] CommandsSet(string selector, ScoreboardValue other, string thisAccessor, string thatAccessor)
        {
            if (other is ScoreboardValueInteger)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(selector, Name, other),
                    Command.ScoreboardOpMul(selector, Name, tempBase),
                };

                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                // convert bases if necessary
                return BalancePrecisionInto(selector, @decimal);
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
                    Command.ScoreboardOpAdd(selector, Name, tempAccumulator),
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                if(BalancePrecisionWith(selector, @decimal, out ScoreboardValueDecimal balancedCopy, out string[] commands))
                {
                    List<string> commandList = new List<string>(commands.Length + 1);
                    commandList.AddRange(commands);
                    commandList.Add(Command.ScoreboardOpAdd(selector, Name, balancedCopy.Name));
                    return commandList.ToArray();
                }

                return new[] { Command.ScoreboardOpAdd(selector, Name, other.Name) };
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
                    Command.ScoreboardOpSub(selector, Name, tempBase),
                };

                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                if (BalancePrecisionWith(selector, @decimal, out ScoreboardValueDecimal balancedCopy, out string[] commands))
                {
                    List<string> commandList = new List<string>(commands.Length + 1);
                    commandList.AddRange(commands);
                    commandList.Add(Command.ScoreboardOpSub(selector, Name, balancedCopy.Name));
                    return commandList.ToArray();
                }

                return new[] { Command.ScoreboardOpSub(selector, Name, other.Name) };
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
                    Command.ScoreboardOpMul(selector, Name, other),
                };
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                if (BalancePrecisionWith(selector, @decimal, out ScoreboardValueDecimal balancedCopy, out string[] commands))
                {
                    List<string> commandList = new List<string>(commands.Length + 3);
                    commandList.AddRange(commands);
                    commandList.Add(Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)));
                    commandList.Add(Command.ScoreboardOpMul(selector, Name, balancedCopy.Name));
                    commandList.Add(Command.ScoreboardOpDiv(selector, Name, tempBase.Name));
                    manager.ReleaseTemp();
                    return commandList.ToArray();
                }

                commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpMul(selector, Name, other.Name),
                    Command.ScoreboardOpDiv(selector, Name, tempBase.Name)
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
                    Command.ScoreboardOpDiv(selector, Name, other),
                };
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                if (BalancePrecisionWith(selector, @decimal, out ScoreboardValueDecimal balancedCopy, out string[] commands))
                {
                    List<string> commandList = new List<string>(commands.Length + 3);
                    commandList.AddRange(commands);
                    commandList.Add(Command.ScoreboardSet(selector, tempBase.Name, (int)Math.Pow(10, precision)));
                    commandList.Add(Command.ScoreboardOpMul(selector, Name, tempBase.Name));
                    commandList.Add(Command.ScoreboardOpDiv(selector, Name, balancedCopy.Name));
                    manager.ReleaseTemp();
                    return commandList.ToArray();
                }

                commands = new string[]
                {
                    Command.ScoreboardSet(selector, tempBase.Name, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpMul(selector, Name, tempBase.Name),
                    Command.ScoreboardOpDiv(selector, Name, other.Name)
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
                    Command.ScoreboardOpMod(selector, Name, tempBase)
                };
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                if (BalancePrecisionWith(selector, @decimal, out ScoreboardValueDecimal balancedCopy, out string[] commands))
                {
                    List<string> commandList = new List<string>(commands.Length + 1);
                    commandList.AddRange(commands);
                    commandList.Add(Command.ScoreboardOpMod(selector, Name, balancedCopy.Name));
                    return commandList.ToArray();
                }

                return new[] { Command.ScoreboardOpMod(selector, Name, other.Name) };
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
                    Command.ScoreboardOpSwap(selector, Name, other), // now both values are to the wrong base.
                    Command.ScoreboardOpMul(selector, Name, temp),
                    Command.ScoreboardOpDiv(selector, other, temp)
                };

                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                if(precision == @decimal.precision)
                {
                    return new[] { Command.ScoreboardOpSwap(selector, Name, other) };
                }

                string[] commands = new string[4];
                ScoreboardValue temp = manager.RequestTemp();

                commands[0] = Command.ScoreboardOpSwap(selector, Name, other);

                // swap precisions using the difference between them.
                if (precision > @decimal.precision)
                {
                    int precisionDiff = precision - @decimal.precision;
                    commands[1] = Command.ScoreboardSet(selector, temp, (int)Math.Pow(10, precisionDiff));
                    commands[2] = Command.ScoreboardOpMul(selector, Name, temp);
                    commands[3] = Command.ScoreboardOpDiv(selector, other.Name, temp);
                }
                else
                {
                    // precision < other.precision
                    int precisionDiff = @decimal.precision - precision;
                    commands[1] = Command.ScoreboardSet(selector, temp, (int)Math.Pow(10, precisionDiff));
                    commands[2] = Command.ScoreboardOpDiv(selector, Name, temp);
                    commands[3] = Command.ScoreboardOpMul(selector, other.Name, temp);
                }

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
    public sealed class ScoreboardValueBoolean : ScoreboardValueInteger
    {
        public ScoreboardValueBoolean(string name, bool global, ScoreboardManager manager, Statement forExceptions) :
            base(name, global, manager, forExceptions, ScoreboardManager.ValueType.BOOL) { }

        public override string GetTypeKeyword() => "bool";
        public override string[] CommandsDefine(string prefix = "")
        {
            return new[] { Command.ScoreboardCreateObjective(prefix + Name) };
        }
        public override string[] CommandsInit(string prefix = "")
        {
            return new[] { Command.ScoreboardAdd("@a", prefix + Name, 0) }; // init to false
        }
        public override string[] CommandsSetLiteral(string accessor, string selector, TokenLiteral token, string prefix = "")
        {
            if (token == null || token is TokenNullLiteral)
                return new string[] { Command.ScoreboardSet(selector, prefix + Name, 0) };

            if (token is TokenStringLiteral)
                return new string[] { };

            if (token is TokenNumberLiteral)
                return new string[] { Command.ScoreboardSet(selector, prefix + Name,
                    (token as TokenNumberLiteral).GetNumberInt() % 2)};

            if (token is TokenBooleanLiteral)
                return new string[] { Command.ScoreboardSet(selector, prefix + Name,
                    (token as TokenBooleanLiteral).boolean ? 1 : 0)};

            return new string[] { };
        }
        public override Tuple<ScoresEntry[], string[]> CompareToLiteral(string accessor, string selector, TokenCompare.Type ctype, TokenNumberLiteral literal, string prefix = "")
        {
            if (!(literal is TokenBooleanLiteral))
                throw new Exception("You can only compare boolean variables to booleans (true or false).");

            bool input = literal as TokenBooleanLiteral;
            int src = input ? 1 : 0;

            Range range;
            switch (ctype)
            {
                case TokenCompare.Type.EQUAL:
                    range = new Range(src, true);
                    break;
                case TokenCompare.Type.NOT_EQUAL:
                    range = new Range(src, false);
                    break;

                // seriously if you use others what the hell are you doing lol?
                default:
                    throw new Exception("Boolean variables only support == and != (not sure why you tried that tbh)");
            }

            return new Tuple<ScoresEntry[], string[]>(new[]
            {
                new ScoresEntry(prefix + Name, range)
            }, new string[0]);
        }

        public override string[] CommandsRawTextSetup(string accessor, string selector, ref int index, string prefix = "")
        {
            return new string[0];
        }
        public override JSONRawTerm[] ToRawText(string accessor, string selector, ref int index, string prefix = "")
        {
            // you can change these!
            manager.executor.TryGetPPV("_true", out dynamic[] trueValues);
            manager.executor.TryGetPPV("_false", out dynamic[] falseValues);

            return new JSONRawTerm[]
            {
                new JSONScore(selector, prefix + Name).CreateVariant(
                    new[] { new JSONText(trueValues[0].ToString()) },
                    new Range(1, false),
                    new[] { new JSONText(falseValues[0].ToString()) }
                )
            };
        }

        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH;
        public override string[] GetAccessibleNames() =>
            new[] { AliasName };
    }
    public sealed class ScoreboardValueStruct : ScoreboardValue
    {
        public readonly StructDefinition structure;

        public ScoreboardValueStruct(string name, bool global, StructDefinition structure, ScoreboardManager manager, Statement forExceptions) :
            base(name, global, ScoreboardManager.ValueType.STRUCT, manager, forExceptions)
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
            structure.GetAccessor(Name, accessor);
        /// <summary>
        /// Fully resolve a field:name accessor to its appropriate scoreboard value.
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="allowMissingAccessor">Whether to allow the struct name on its own without a field accessor.</param>
        /// <returns></returns>
        public ScoreboardValue FullyResolveAccessor(string accessor, bool allowMissingAccessor = false)
        {
            if (accessor.IndexOf(':') == -1)
            {
                if (allowMissingAccessor)
                    return this;
                throw new Exception("Struct accessor '" + accessor + "' didn't have a field specified.");
            }

            return structure.GetFieldFromAccessor(accessor);
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }
        public override string GetTypeKeyword() => structure.name;
        public override string[] CommandsDefine(string prefix = "")
        {
            return structure.GetFields(Name).SelectMany(f => f.CommandsDefine(prefix)).ToArray();
        }
        public override string[] CommandsInit(string prefix = "")
        {
            return structure.GetFields(Name).SelectMany(f => f.CommandsInit(prefix)).ToArray();
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
                ScoreboardValue[] values = structure.GetFields(Name);
                List<string> commands = new List<string>();
                int remaining = values.Count();
                foreach (ScoreboardValue f in values)
                {
                    commands.AddRange(f.CommandsRawTextSetup(accessor, selector, ref index, prefix));
                    remaining--;
                    if (remaining > 1)
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
                ScoreboardValue[] values = structure.GetFields(Name);
                List<JSONRawTerm> commands = new List<JSONRawTerm>();
                int remaining = values.Count();
                foreach (ScoreboardValue f in values)
                {
                    commands.AddRange(f.ToRawText(accessor, selector, ref index, prefix));
                    remaining--;
                    if(remaining > 1)
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
            string[] qualified = structure.GetFullyQualifiedNames(AliasName).ToArray();
            string[] ret = new string[qualified.Length + 1];
            for (int i = 0; i < qualified.Length; i++)
                ret[i] = qualified[i];
            ret[qualified.Length] = Name;
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
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(Name, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(Name, i);
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
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(Name, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(Name, i);
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
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(Name, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(Name, i);
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
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(Name, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(Name, i);
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
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(Name, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(Name, i);
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
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(Name, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(Name, i);
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
                        ScoreboardValue fieldDst = structure.GetFieldByIndex(Name, i);
                        ScoreboardValue fieldSrc = structB.structure.GetFieldByIndex(Name, i);
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