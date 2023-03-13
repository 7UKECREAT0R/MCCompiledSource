﻿using mc_compiled.Commands;
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
using System.Windows.Media.TextFormatting;

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
        public Clarifier clarifier { get; protected set; }
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
        public ScoreboardValue WithAttributes(IEnumerable<IAttribute> attributes, Statement callingStatement)
        {
            this.attributes.AddRange(attributes);

            foreach (IAttribute attribute in attributes)
                attribute.OnAddedValue(this, callingStatement);

            return this;
        }
        /// <summary>
        /// Perform a fully implemented deep clone of this scoreboard value.
        /// Does not clone 
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            ScoreboardValue clone = MemberwiseClone() as ScoreboardValue;
            clone.clarifier = this.clarifier.Clone();
            clone.attributes = new List<IAttribute>(this.attributes);

            return clone;
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
        /// Top-Level defined. Returns GetTypeKeyword() but with attributes/extra things included.
        /// </summary>
        /// <returns></returns>
        public string GetExtendedTypeKeyword()
        {
            StringBuilder sb = new StringBuilder();

            if (clarifier.IsGlobal)
                sb.Append("global ");

            sb.Append(GetTypeKeyword());
            return sb.ToString();
        }
        /// <summary>
        /// Gets the keyword(s) associated with this scoreboard value type. Used by the language server to communicate variable types.
        /// </summary>
        /// <returns></returns>
        public abstract string GetTypeKeyword();
        /// <summary>
        /// Get the commands to define this value.
        /// </summary>
        /// <returns></returns>
        public abstract string[] CommandsDefine();
        /// <summary>
        /// Get the commands to initialize this value for all players.
        /// </summary>
        /// <returns></returns>
        public abstract string[] CommandsInit();

        /// <summary>
        /// Get the commands to set this value to any given literal.
        /// </summary>
        /// <param name="token">The literal that is being used as the value.</param>
        /// <returns></returns>
        public abstract string[] CommandsSetLiteral(TokenLiteral token);

        /// <summary>
        /// Compare this scoreboard value to another literal value.
        /// </summary>
        /// <param name="ctype"></param>
        /// <param name="literal"></param>
        /// <returns></returns>
        public abstract Tuple<ScoresEntry[], string[]> CompareToLiteral(TokenCompare.Type ctype, TokenNumberLiteral literal);
        /// <summary>
        /// Setup temporary variables before printing this variable as rawtext.
        /// </summary>
        /// <param name="index">The index to use to identify scoreboard values.</param>
        /// <returns></returns>
        public abstract string[] CommandsRawTextSetup(ref int index);
        /// <summary>
        /// Convert this scoreboard value to its expression as rawtext.
        /// Basically the Minecraft equivalent of ToString().
        /// Called immediately after ToRawTextSetup(string).
        /// </summary>
        /// <param name="index">The index to use to identify scoreboard values.</param>
        /// <returns></returns>
        public abstract JSONRawTerm[] ToRawText(ref int index);

        public string[] CommandsFromOperation(ScoreboardValue other, TokenArithmatic.Type type)
        {
            switch (type)
            {
                case TokenArithmatic.Type.ADD:
                    return CommandsAdd(other);
                case TokenArithmatic.Type.SUBTRACT:
                    return CommandsSub(other);
                case TokenArithmatic.Type.MULTIPLY:
                    return CommandsMul(other);
                case TokenArithmatic.Type.DIVIDE:
                    return CommandsDiv(other);
                case TokenArithmatic.Type.MODULO:
                    return CommandsMod(other);
                case TokenArithmatic.Type.SWAP:
                    return CommandsSwap(other);
                default:
                    break;
            }
            return null;
        }
        public abstract string[] CommandsAddLiteral(TokenLiteral other, Statement forExceptions);
        public abstract string[] CommandsSubLiteral(TokenLiteral other, Statement forExceptions);

        /// <summary>
        /// this = other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract string[] CommandsSet(ScoreboardValue other);
        /// <summary>
        /// this += other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract string[] CommandsAdd(ScoreboardValue other);
        /// <summary>
        /// this -= other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract string[] CommandsSub(ScoreboardValue other);
        /// <summary>
        /// this *= other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract string[] CommandsMul(ScoreboardValue other);
        /// <summary>
        /// this /= other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract string[] CommandsDiv(ScoreboardValue other);
        /// <summary>
        /// this %= other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract string[] CommandsMod(ScoreboardValue other);
        /// <summary>
        /// Swap this and other.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract string[] CommandsSwap(ScoreboardValue other);

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
        public override string[] CommandsDefine()
        {
            return new[] { Command.ScoreboardCreateObjective(Name, AliasName) };
        }
        public override string[] CommandsInit()
        {
            return new[] {
                Command.ScoreboardAdd("@a", Name, 0) // init to 0
            };
        }
        public override string[] CommandsSetLiteral(TokenLiteral token)
        {
            if (token == null || token is TokenNullLiteral)
                return new string[] { Command.ScoreboardSet(this, 0) };

            if (token is TokenStringLiteral)
                return new string[] { };

            if (token is TokenNumberLiteral number)
                return new string[] { Command.ScoreboardSet(this, number.GetNumberInt())};

            if (token is TokenBooleanLiteral)
                return new string[] { };

            return new string[] { };
        }
        public override Tuple<ScoresEntry[], string[]> CompareToLiteral(TokenCompare.Type ctype, TokenNumberLiteral literal)
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
                new ScoresEntry(Name, range)
            }, new string[0]);
        }
        public override string[] CommandsRawTextSetup(ref int index)
        {
            return new string[0];
        }
        public override JSONRawTerm[] ToRawText(ref int index)
        {
            return new[] { new JSONScore(clarifier.CurrentString, Name) };
        }

        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH;
        public override string[] GetAccessibleNames() =>
            new[] { AliasName };

        public override string[] CommandsAddLiteral(TokenLiteral other, Statement forExceptions)
        {
            if (other is TokenNumberLiteral)
                return new[] { Command.ScoreboardAdd(this, (other as TokenNumberLiteral).GetNumberInt()) };
            else
                throw new StatementException(forExceptions, "Attempted to add invalid literal to value '" + Name + "'");
        }
        public override string[] CommandsSubLiteral(TokenLiteral other, Statement forExceptions)
        {
            if (other is TokenNumberLiteral)
                return new[] { Command.ScoreboardSubtract(this, (other as TokenNumberLiteral).GetNumberInt()) };
            else
                throw new StatementException(forExceptions, "Attempted to subtract invalid literal from value '" + Name + "'");
        }

        public override string[] CommandsSet(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpSet(this, other)};

            if(other is ScoreboardValueDecimal)
            {
                // floor the decimal value
                ScoreboardValue tempBase = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(this, other),
                    Command.ScoreboardOpDiv(this, tempBase)
                };

                manager.ReleaseTemp();
                return commands;
            }

            return new string[0];
        }
        public override string[] CommandsAdd(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpAdd(this, other) };

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(temp, other),
                    Command.ScoreboardOpDiv(temp, tempBase),
                    Command.ScoreboardOpAdd(this, temp)
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            return new string[0];
        }
        public override string[] CommandsSub(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpSub(this, other) };

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(temp, other),
                    Command.ScoreboardOpDiv(temp, tempBase),
                    Command.ScoreboardOpSub(this, temp)
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            return new string[0];
        }
        public override string[] CommandsMul(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpMul(this, other) };

            if (other is ScoreboardValueDecimal)
            {
                // set this to the whole part of the decimal value (floor)
                ScoreboardValue tempBase = manager.RequestTemp();
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(temp, other),
                    Command.ScoreboardOpDiv(temp, tempBase),
                    Command.ScoreboardOpMul(this, temp)
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            return new string[0];
        }
        public override string[] CommandsDiv(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpDiv(this, other) };

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(temp, other),
                    Command.ScoreboardOpDiv(temp, tempBase),
                    Command.ScoreboardOpDiv(this, temp)
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            return new string[0];
        }
        public override string[] CommandsMod(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpMod(this, other) };

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[] {
                    Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(temp, other),
                    Command.ScoreboardOpDiv(temp, tempBase),
                    Command.ScoreboardOpMod(this, temp)
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            return new string[0];
        }
        public override string[] CommandsSwap(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
                return new[] { Command.ScoreboardOpSwap(this, other) };

            if (other is ScoreboardValueDecimal)
            {
                ScoreboardValue temp = manager.RequestTemp();
                int precision = (other as ScoreboardValueDecimal).precision;

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(temp, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSwap(this, other), // now both values are to the wrong base.
                    Command.ScoreboardOpDiv(this, temp),
                    Command.ScoreboardOpMul(other, temp)
                };

                manager.ReleaseTemp();
                return commands;
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
        public override string[] CommandsRawTextSetup(ref int index)
        {
            string _minutes = SB_MINUTES + index;
            string _seconds = SB_SECONDS + index;
            string _temporary = SB_TEMP + index;
            string _constant = SB_CONST + index;

            ScoreboardValue minutes = new ScoreboardValueInteger(_minutes, false, manager, null);
            ScoreboardValue seconds = new ScoreboardValueInteger(_seconds, false, manager, null);
            ScoreboardValue temporary = new ScoreboardValueInteger(_temporary, false, manager, null);
            ScoreboardValue constant = new ScoreboardValueInteger(_constant, false, manager, null);

            manager.AddToStringScoreboards(this,
                minutes, seconds, temporary, constant);

            return new string[]
            {
                Command.ScoreboardSet(constant, 20),
                Command.ScoreboardOpSet(temporary, this),
                Command.ScoreboardOpDiv(temporary, constant),
                Command.ScoreboardOpSet(seconds, temporary),
                Command.ScoreboardSet(constant, 60),
                Command.ScoreboardOpDiv(temporary, constant),
                Command.ScoreboardOpSet(minutes, temporary),
                Command.ScoreboardOpMul(temporary, constant),
                Command.ScoreboardOpSub(seconds, temporary)
            };
        }
        public override JSONRawTerm[] ToRawText(ref int index)
        {
            string minutes = SB_MINUTES + index;
            string seconds = SB_SECONDS + index;

            return new JSONRawTerm[]
            {
                new JSONScore(clarifier.CurrentString, minutes),

                // append an extra 0 if its a single number.
                new JSONScore(clarifier.CurrentString, seconds).CreateVariant(
                    new[] { new JSONText(":0") },
                    new Range(0, 9),
                    new[] { new JSONText(":") }),

                new JSONScore(clarifier.CurrentString, seconds)
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
                    Command.ScoreboardSet(temp1, (int)Math.Pow(10, precisionDiff)),
                    Command.ScoreboardOpSet(temp2, other),
                    Command.ScoreboardOpMul(temp2, temp1)
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
                    Command.ScoreboardSet(temp1, (int)Math.Pow(10, precisionDiff)),
                    Command.ScoreboardOpSet(temp2, other),
                    Command.ScoreboardOpDiv(temp2, temp1)
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
                return new[] { Command.ScoreboardOpSet(this, other) };
            }

            if (precision > other.precision)
            {
                int precisionDiff = precision - other.precision;
                ScoreboardValue temp1 = other.manager.RequestTemp();
                string[] commands = new string[]
                {
                    Command.ScoreboardSet(temp1, (int)Math.Pow(10, precisionDiff)),
                    Command.ScoreboardOpSet(this, other),
                    Command.ScoreboardOpMul(this, temp1)
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
                    Command.ScoreboardSet(temp1, (int)Math.Pow(10, precisionDiff)),
                    Command.ScoreboardOpSet(this, other),
                    Command.ScoreboardOpDiv(this, temp1)
                };
                other.manager.ReleaseTemp();
                return commands;
            }
        }

        public override object Clone()
        {
            ScoreboardValueDecimal clone = MemberwiseClone() as ScoreboardValueDecimal;
            clone.clarifier = this.clarifier.Clone();
            clone.attributes = new List<IAttribute>(this.attributes);

            return clone;
        }
        public override string GetTypeKeyword() => "decimal " + precision;
        public override string[] CommandsDefine()
        {
            return new[] {
                Command.ScoreboardCreateObjective(Name)
            };
        }
        public override string[] CommandsInit()
        {
            return new[] {
                Command.ScoreboardAdd("@a", Name, 0)
            };
        }
        public override string[] CommandsSetLiteral(TokenLiteral token)
        {
            if (token == null || token is TokenNullLiteral)
                return new string[] { Command.ScoreboardSet(this, 0) };

            if (token is TokenStringLiteral)
                return new string[] { };

            if (token is TokenIntegerLiteral)
            {
                int integer = (token as TokenIntegerLiteral).number;
                return new string[] {
                    Command.ScoreboardSet(this, integer.ToFixedPoint(precision)),
                };
            }
            if (token is TokenDecimalLiteral)
            {
                TokenDecimalLiteral literal = token as TokenDecimalLiteral;
                return new string[] {
                    Command.ScoreboardSet(this, literal.number.ToFixedPoint(precision))
                };
            }

            if (token is TokenBooleanLiteral)
                return new string[] { };

            return new string[] { };
        }
        public override Tuple<ScoresEntry[], string[]> CompareToLiteral(TokenCompare.Type ctype, TokenNumberLiteral literal)
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
                new ScoresEntry(Name, range)
            }, new string[0]);
        }

        public override string[] CommandsRawTextSetup(ref int index)
        {
            string _whole = SB_WHOLE + index;
            string _part = SB_PART + index;
            string _temporary = SB_TEMP + index;
            string _tempBase = SB_BASE + index;

            var whole = new ScoreboardValueInteger(_whole, false, manager, null);
            var part = new ScoreboardValueInteger(_part, false, manager, null);
            var temporary = new ScoreboardValueInteger(_temporary, false, manager, null);
            var tempBase = new ScoreboardValueInteger(_tempBase, false, manager, null);

            manager.AddToStringScoreboards(this,
                whole, part, temporary, tempBase);

            return new string[]
            {
                Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                Command.ScoreboardOpSet(temporary, this),
                Command.ScoreboardOpDiv(temporary, tempBase),
                Command.ScoreboardOpSet(whole, temporary),
                Command.ScoreboardOpMul(temporary, tempBase),
                Command.ScoreboardOpSet(part, this),
                Command.ScoreboardOpSub(part, temporary),
                Command.ScoreboardSet(temporary, -1),
                Command.Execute().IfScore(part, new Range(null, -1)).Run(Command.ScoreboardOpMul(part, temporary))
            };
        }
        public override JSONRawTerm[] ToRawText(ref int index)
        {
            string whole = SB_WHOLE + index;
            string part = SB_PART + index;

            return new JSONRawTerm[]
            {
                new JSONScore(clarifier.CurrentString, whole),
                new JSONText("."),
                new JSONScore(clarifier.CurrentString, part)
            };
        }

        public override int GetMaxNameLength() =>
           MAX_NAME_LENGTH - 2;
        public override string[] GetAccessibleNames() =>
            new[] { AliasName };

        public override string[] CommandsAddLiteral(TokenLiteral other, Statement forExceptions)
        {
            if (other is TokenIntegerLiteral)
            {
                int value = (other as TokenIntegerLiteral);
                value = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardAdd(this, value) };
            }
            else if(other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral);
                int number = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardAdd(this, number) };
            }
            else
                throw new StatementException(forExceptions, "Attempted to add invalid literal to value '" + Name + "'");
        }
        public override string[] CommandsSubLiteral(TokenLiteral other, Statement forExceptions)
        {
            if (other is TokenIntegerLiteral)
            {
                int value = (other as TokenIntegerLiteral);
                value = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardSubtract(this, value) };
            }
            else if (other is TokenDecimalLiteral)
            {
                float value = (other as TokenDecimalLiteral);
                int number = value.ToFixedPoint(precision);
                return new[] { Command.ScoreboardSubtract(this, number) };
            }
            else
                throw new StatementException(forExceptions, "Attempted to add invalid literal to value '" + Name + "'");
        }

        public override string[] CommandsSet(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(this, other),
                    Command.ScoreboardOpMul(this, tempBase),
                };

                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                // convert bases if necessary
                return BalancePrecisionInto(clarifier.CurrentString, @decimal);
            }

            return new string[0];
        }
        public override string[] CommandsAdd(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
            {
                ScoreboardValue tempAccumulator = manager.RequestTemp();
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSet(tempAccumulator, other),
                    Command.ScoreboardOpMul(tempAccumulator, tempBase),
                    Command.ScoreboardOpAdd(this, tempAccumulator),
                };

                manager.ReleaseTemp();
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                if(BalancePrecisionWith(clarifier.CurrentString, @decimal, out ScoreboardValueDecimal balancedCopy, out string[] commands))
                {
                    List<string> commandList = new List<string>(commands.Length + 1);
                    commandList.AddRange(commands);
                    commandList.Add(Command.ScoreboardOpAdd(this, balancedCopy));
                    return commandList.ToArray();
                }

                return new[] { Command.ScoreboardOpAdd(this, other) };
            }
            
            return new string[0];
        }
        public override string[] CommandsSub(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpMul(tempBase, other),
                    Command.ScoreboardOpSub(this, tempBase),
                };

                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                if (BalancePrecisionWith(clarifier.CurrentString, @decimal, out ScoreboardValueDecimal balancedCopy, out string[] commands))
                {
                    List<string> commandList = new List<string>(commands.Length + 1);
                    commandList.AddRange(commands);
                    commandList.Add(Command.ScoreboardOpSub(this, balancedCopy));
                    return commandList.ToArray();
                }

                return new[] { Command.ScoreboardOpSub(this, other) };
            }

            return new string[0];
        }
        public override string[] CommandsMul(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
            {
                string[] commands = new string[]
                {
                    Command.ScoreboardOpMul(this, other),
                };
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                if (BalancePrecisionWith(clarifier.CurrentString, @decimal, out ScoreboardValueDecimal balancedCopy, out string[] commands))
                {
                    List<string> commandList = new List<string>(commands.Length + 3);
                    commandList.AddRange(commands);
                    commandList.Add(Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)));
                    commandList.Add(Command.ScoreboardOpMul(this, balancedCopy));
                    commandList.Add(Command.ScoreboardOpDiv(this, tempBase));
                    manager.ReleaseTemp();
                    return commandList.ToArray();
                }

                commands = new string[]
                {
                    Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpMul(this, other),
                    Command.ScoreboardOpDiv(this, tempBase)
                };
                manager.ReleaseTemp();
                return commands;
            }

            return new string[0];
        }
        public override string[] CommandsDiv(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
            {
                string[] commands = new string[]
                {
                    Command.ScoreboardOpDiv(this, other),
                };
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                if (BalancePrecisionWith(clarifier.CurrentString, @decimal, out ScoreboardValueDecimal balancedCopy, out string[] commands))
                {
                    List<string> commandList = new List<string>(commands.Length + 3);
                    commandList.AddRange(commands);
                    commandList.Add(Command.ScoreboardSet(tempBase, (int)Math.Pow(10, precision)));
                    commandList.Add(Command.ScoreboardOpMul(this, tempBase));
                    commandList.Add(Command.ScoreboardOpDiv(this, balancedCopy));
                    manager.ReleaseTemp();
                    return commandList.ToArray();
                }

                commands = new string[]
                {
                    Command.ScoreboardSet(this, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpMul(this, tempBase),
                    Command.ScoreboardOpDiv(this, other)
                };
                manager.ReleaseTemp();
                return commands;
            }

            return new string[0];
        }
        public override string[] CommandsMod(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
            {
                ScoreboardValue tempBase = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(this, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpMul(this, other),
                    Command.ScoreboardOpMod(this, tempBase)
                };
                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                if (BalancePrecisionWith(clarifier.CurrentString, @decimal, out ScoreboardValueDecimal balancedCopy, out string[] commands))
                {
                    List<string> commandList = new List<string>(commands.Length + 1);
                    commandList.AddRange(commands);
                    commandList.Add(Command.ScoreboardOpMod(this, balancedCopy));
                    return commandList.ToArray();
                }

                return new[] { Command.ScoreboardOpMod(this, other) };
            }

            return new string[0];
        }
        public override string[] CommandsSwap(ScoreboardValue other)
        {
            if (other is ScoreboardValueInteger)
            {
                ScoreboardValue temp = manager.RequestTemp();

                string[] commands = new string[]
                {
                    Command.ScoreboardSet(this, (int)Math.Pow(10, precision)),
                    Command.ScoreboardOpSwap(this, other), // now both values are to the wrong base.
                    Command.ScoreboardOpMul(this, temp),
                    Command.ScoreboardOpDiv(this, temp)
                };

                manager.ReleaseTemp();
                return commands;
            }

            if (other is ScoreboardValueDecimal @decimal)
            {
                if(precision == @decimal.precision)
                {
                    return new[] { Command.ScoreboardOpSwap(this, other) };
                }

                string[] commands = new string[4];
                ScoreboardValue temp = manager.RequestTemp();

                commands[0] = Command.ScoreboardOpSwap(this, other);

                // swap precisions using the difference between them.
                if (precision > @decimal.precision)
                {
                    int precisionDiff = precision - @decimal.precision;
                    commands[1] = Command.ScoreboardSet(temp, (int)Math.Pow(10, precisionDiff));
                    commands[2] = Command.ScoreboardOpMul(this, temp);
                    commands[3] = Command.ScoreboardOpDiv(other, temp);
                }
                else
                {
                    // precision < other.precision
                    int precisionDiff = @decimal.precision - precision;
                    commands[1] = Command.ScoreboardSet(temp, (int)Math.Pow(10, precisionDiff));
                    commands[2] = Command.ScoreboardOpDiv(this, temp);
                    commands[3] = Command.ScoreboardOpMul(other, temp);
                }

                return commands;
            }

            return new string[0];
        }
    }
    public sealed class ScoreboardValueBoolean : ScoreboardValueInteger
    {
        public ScoreboardValueBoolean(string name, bool global, ScoreboardManager manager, Statement forExceptions) :
            base(name, global, manager, forExceptions, ScoreboardManager.ValueType.BOOL) { }

        public override string GetTypeKeyword() => "bool";
        public override string[] CommandsDefine()
        {
            return new[] { Command.ScoreboardCreateObjective(Name) };
        }
        public override string[] CommandsInit()
        {
            return new[] {
                Command.ScoreboardAdd("@a", Name, 0) // init to false
            };
        }
        public override string[] CommandsSetLiteral(TokenLiteral token)
        {
            if (token == null || token is TokenNullLiteral)
                return new string[] { Command.ScoreboardSet(this, 0) };

            if (token is TokenStringLiteral)
                return new string[] { };

            if (token is TokenNumberLiteral)
                return new string[] { Command.ScoreboardSet(this,
                    (token as TokenNumberLiteral).GetNumberInt() % 2)};

            if (token is TokenBooleanLiteral)
                return new string[] { Command.ScoreboardSet(this,
                    (token as TokenBooleanLiteral).boolean ? 1 : 0)};

            return new string[] { };
        }
        public override Tuple<ScoresEntry[], string[]> CompareToLiteral(TokenCompare.Type ctype, TokenNumberLiteral literal)
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
                new ScoresEntry(Name, range)
            }, new string[0]);
        }

        public override string[] CommandsRawTextSetup(ref int index)
        {
            return new string[0];
        }
        public override JSONRawTerm[] ToRawText(ref int index)
        {
            // you can change these!
            manager.executor.TryGetPPV("_true", out dynamic[] trueValues);
            manager.executor.TryGetPPV("_false", out dynamic[] falseValues);

            return new JSONRawTerm[]
            {
                new JSONScore(clarifier.CurrentString, Name).CreateVariant(
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
}