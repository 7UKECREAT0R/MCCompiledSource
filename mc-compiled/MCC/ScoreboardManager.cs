using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Manages the virtual scoreboard.
    /// </summary>
    public class ScoreboardManager
    {
        /// <summary>
        /// A type of value that can be defined.
        /// </summary>
        public enum ValueType
        {
            /// <summary>
            /// Infer type from right-hand side of definition.
            /// </summary>
            INFER,
            /// <summary>
            /// Invalid value type.
            /// </summary>
            INVALID,

            /// <summary>
            /// An integral value.
            /// </summary>
            INT,
            /// <summary>
            /// A decimal value with a set precision.
            /// </summary>
            DECIMAL,
            /// <summary>
            /// A boolean (true/false) value.
            /// </summary>
            BOOL,
            /// <summary>
            /// A time value, represented in ticks.
            /// </summary>
            TIME,
            /// <summary>
            /// A preprocessor variable.
            /// </summary>
            PPV
        }
        /// <summary>
        /// Returns a shortcode for the given <see cref="ScoreboardManager.ValueType"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>A 3 character code representing the given ValueType</returns>
        internal static string GetShortcodeFor(ScoreboardManager.ValueType type)
        {
            switch (type)
            {
                case ValueType.INFER:
                case ValueType.INVALID:
                    return "XXX";
                case ValueType.INT:
                    return "INT";
                case ValueType.DECIMAL:
                    return "DEC";
                case ValueType.BOOL:
                    return "BLN";
                case ValueType.TIME:
                    return "TME";
                case ValueType.PPV:
                    return "PPV";
                default:
                    throw new Exception($"No shortcode implementation for type {type}.");
            }
        }

        /// <summary>
        /// A shallow variable definition used in structs, defines, functions, etc...
        /// </summary>
        internal struct ValueDefinition
        {
            internal IAttribute[] attributes;
            internal string name;
            internal ValueType type;
            internal int decimalPrecision;

            internal Token defaultValue;

            internal ValueDefinition(IAttribute[] attributes, string name, ValueType type, int decimalPrecision = 0, Token defaultValue = null)
            {
                this.attributes = attributes;
                this.name = name;
                this.type = type;
                this.decimalPrecision = decimalPrecision;
                this.defaultValue = defaultValue;
            }
            /// <summary>
            /// Create a scoreboard value based off of this definition.
            /// </summary>
            /// <returns></returns>
            internal ScoreboardValue Create(ScoreboardManager sb, Statement tokens)
            {
                return ScoreboardValue.CreateByType(type, name, false, sb).WithAttributes(attributes, tokens);
            }
            internal void InferType(Statement tokens)
            {
                // check if it's a literal.
                if (this.defaultValue is TokenLiteral literal)
                {
                    this.type = literal.GetScoreboardValueType();
                    if (this.type == ScoreboardManager.ValueType.INVALID)
                        throw new StatementException(tokens, $"Input \"{literal.AsString()}\" cannot be stored in a value.");
                    if (literal is TokenDecimalLiteral @decimal)
                        this.decimalPrecision = @decimal.number.GetPrecision();
                }
                // check if it's a runtime value.
                else if (this.defaultValue is TokenIdentifierValue identifier)
                {
                    this.type = identifier.value.valueType;
                    if (identifier.value is ScoreboardValueDecimal @decimal)
                        this.decimalPrecision = @decimal.precision;
                }
                else
                    throw new StatementException(tokens, $"Cannot assign value of type {this.defaultValue.GetType().Name} into a variable");
            }
        }

        public readonly Executor executor;
        public readonly TempManager temps;
        internal readonly HashSet<ScoreboardValue> values;

        /// <summary>
        /// Create a new ScoreboardManager tied to the given <see cref="Executor"/>. Changes will reflect in the <see cref="Executor"/> in various ways.
        /// </summary>
        /// <param name="executor"></param>
        public ScoreboardManager(Executor executor)
        {
            temps = new TempManager(this, executor);
            values = new HashSet<ScoreboardValue>();
            this.executor = executor;
        }

        /// <summary>
        /// I wrote this a while ago, and to be honest, I have no
        /// idea what it does or is supposed to do, except that it works.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="commands">The commands</param>
        public void AddToStringScoreboards(ScoreboardValue value, params ScoreboardValue[] commands)
        {
            string name = value.Name;

            if (temps.DefinedTemps.Contains(name))
                return;

            executor.AddCommandsInit(commands.SelectMany(sb => sb.CommandsDefine()));
            temps.DefinedTemps.Add(name);
        }

        /// <summary>
        /// Attempts to throw a <see cref="StatementException"/> if there is a duplicate value with the same name 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="callingStatement"></param>
        public void TryThrowForDuplicate(ScoreboardValue value, Statement callingStatement)
        {
            ScoreboardValue find = values.FirstOrDefault(v => v.Name == value.Name);

            if (find != null)
            {
                if (value.valueType != find.valueType)
                    throw new StatementException(callingStatement, $"Value \"{find.Name}\" already exists with type {find.valueType}.");
            }
        }
        /// <summary>
        /// Add a scoreboard value to the cache.
        /// </summary>
        /// <param name="value"></param>
        public void Add(ScoreboardValue value)
        {
            if (values.Contains(value))
                return;
            values.Add(value);
        }
        /// <summary>
        /// Add a set of scoreboard values to the cache.
        /// </summary>
        /// <param name="value"></param>
        public void AddRange(IEnumerable<ScoreboardValue> values)
        {
            foreach (ScoreboardValue value in values)
                Add(value);
        }

        /// <summary>
        /// Create a scoreboard value from a literal value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="literal"></param>
        /// <param name="forExceptions"></param>
        /// <returns></returns>
        public ScoreboardValue CreateFromLiteral(string name, bool global, TokenLiteral literal, Statement forExceptions)
        {
            if (literal is TokenIntegerLiteral)
                return new ScoreboardValueInteger(name, global, this);
            else if (literal is TokenBooleanLiteral)
                return new ScoreboardValueBoolean(name, global, this);
            else if (literal is TokenDecimalLiteral)
            {
                float number = (literal as TokenDecimalLiteral).number;
                int precision = number.GetPrecision();
                return new ScoreboardValueDecimal(name, precision, global, this);
            }
            else throw new StatementException(forExceptions, "Internal Error: Attempted to " +
                    $"create a scoreboard value for invalid literal type {literal.GetType()}.");
        }
        /// <summary>
        /// Fetch a value/field definition from this statement. e.g., 'int coins = 3', 'decimal 3 thing', 'bool isPlaying'.
        /// This method automatically performs type inference if possible.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        internal ValueDefinition GetNextValueDefinition(Statement tokens)
        {
            List<IAttribute> attributes = new List<IAttribute>();
            while(tokens.NextIs<TokenAttribute>())
            {
                var _attribute = tokens.Next<TokenAttribute>();
                attributes.Add(_attribute.attribute);
            }
            IAttribute[] attributesArray = attributes.ToArray();

            ValueType type = ValueType.INFER;
            string name = null;

            if (tokens.NextIs<TokenIdentifier>())
            {
                TokenIdentifier identifier = tokens.Next<TokenIdentifier>();
                string typeWord = identifier.word.ToUpper();
                switch (typeWord)
                {
                    case "INT":
                        type = ValueType.INT;
                        break;
                    case "DECIMAL":
                        type = ValueType.DECIMAL;
                        break;
                    case "BOOL":
                        type = ValueType.BOOL;
                        break;
                    case "TIME":
                        type = ValueType.TIME;
                        break;
                    case "PPV":
                        type = ValueType.PPV;
                        break;
                    default:
                        name = identifier.Convert(executor, TokenIdentifier.CONVERT_STRING) as TokenStringLiteral;
                        break;
                }
            }

            // the default value to set it to.
            Token defaultValue = null;

            if (type == ValueType.DECIMAL)
            {
                if (!tokens.NextIs<TokenIntegerLiteral>())
                    throw new StatementException(tokens, $"No precision specified for decimal value");
                int precision = tokens.Next<TokenIntegerLiteral>();
                if (precision > 3)
                {
                    ConsoleColor oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Executor.Warn("Decimal precisions >3 could begin to break. Avoid multiplication/division on larger numbers.");
                    Console.ForegroundColor = oldColor;
                }
                name = tokens.Next<TokenStringLiteral>();
                if (tokens.NextIs<TokenAssignment>())
                {
                    tokens.Next();
                    defaultValue = tokens.Next();
                }
                return new ValueDefinition(attributesArray, name, ValueType.DECIMAL, precision, defaultValue);
            }

            if (name == null)
            {
                if (!tokens.NextIs<TokenStringLiteral>())
                    throw new StatementException(tokens, "No name specified after type.");
                name = tokens.Next<TokenStringLiteral>();
            }

            if (tokens.NextIs<TokenAssignment>())
            {
                tokens.Next();
                defaultValue = tokens.Next();
            }

            ValueDefinition definition = new ValueDefinition(attributesArray, name, type, default, defaultValue);

            // try to infer type based on the default value.
            if (type == ValueType.INFER)
            {
                if (defaultValue == null)
                    throw new StatementException(tokens, $"Cannot infer value \"{name}\"'s type because there is no default value in declaration.");

                definition.InferType(tokens);
            }

            return definition;
        }

        /// <summary>
        /// Get a scoreboard value by its INTERNAL NAME.
        /// </summary>
        /// <returns>Null if not found.</returns>
        public ScoreboardValue GetByName(string internalName) =>
            values.FirstOrDefault(v => v.Name.Equals(internalName));
        /// <summary>
        /// Get a scoreboard value by any of its USER-EXPOSED NAMES.
        /// </summary>
        /// <param name="accessor"></param>
        /// <returns>Null if not found.</returns>
        public ScoreboardValue GetByAccessor(string accessor)
        {
            foreach (ScoreboardValue value in values)
            {
                string[] names = value.GetAccessibleNames();
                if (names.Contains(accessor))
                    return value;
            }
            return null;
        }
        /// <summary>
        /// Tries to get a scoreboard value by its INTERNAL NAME.
        /// </summary>
        /// <returns>True if found and output is set.</returns>
        public bool TryGetByName(string internalName, out ScoreboardValue output)
        {
            foreach (ScoreboardValue value in values)
                if (value.Name.Equals(internalName))
                {
                    output = value;
                    return true;
                }

            output = null;
            return false;
        }
        /// <summary>
        /// Tries to get a scoreboard value by any of its USER-EXPOSED NAMES.
        /// </summary>
        /// <param name="accessor"></param>
        /// <returns>True if found and output is set.</returns>
        public bool TryGetByAccessor(string accessor, out ScoreboardValue output)
        {
            foreach (ScoreboardValue value in values)
            {
                string[] names = value.GetAccessibleNames();
                if (names.Contains(accessor))
                {
                    output = value;
                    return true;
                }
            }
            output = null;
            return false;
        }
    }


}
