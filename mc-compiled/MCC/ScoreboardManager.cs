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
                switch (type)
                {
                    case ScoreboardManager.ValueType.INT:
                        return new ScoreboardValueInteger(name, false, sb, tokens).WithAttributes(attributes, tokens);
                    case ScoreboardManager.ValueType.DECIMAL:
                        return new ScoreboardValueDecimal(name, decimalPrecision, false, sb, tokens).WithAttributes(attributes, tokens);
                    case ScoreboardManager.ValueType.BOOL:
                        return new ScoreboardValueBoolean(name, false, sb, tokens).WithAttributes(attributes, tokens);
                    case ScoreboardManager.ValueType.TIME:
                        return new ScoreboardValueTime(name, false, sb, tokens).WithAttributes(attributes, tokens);
                    default:
                        throw new StatementException(tokens, "something terrible happened when trying to use value definition");
                }
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

        private const string TEMP_PREFIX = "_mcc_tmp";
        private int tempIndex;
        private Stack<int> tempStack;

        public readonly Executor executor;
        public readonly HashSet<ScoreboardValue> definedTempVars;
        internal readonly HashSet<ScoreboardValue> values;

        public ScoreboardManager(Executor executor)
        {
            tempIndex = 0;
            tempStack = new Stack<int>();

            definedTempVars = new HashSet<ScoreboardValue>();
            values = new HashSet<ScoreboardValue>();
            this.executor = executor;
        }
        public void AddToStringScoreboards(ScoreboardValue value, params ScoreboardValue[] commands)
        {
            if (definedTempVars.Contains(value))
                return;

            executor.AddCommandsHead(commands.SelectMany(sb => sb.CommandsDefine()));
            definedTempVars.Add(value);
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
        /// Makes a request for an unused temporary int variable. Created as needed.
        /// </summary>
        /// <returns></returns>
        public ScoreboardValueInteger RequestTemp()
        {
            string name = TEMP_PREFIX + tempIndex;
            var created = new ScoreboardValueInteger(name, false, this, null);
            
            if (definedTempVars.Add(created))
            {
                executor.AddCommandsHead(created.CommandsInit());
                executor.AddCommandsHead(created.CommandsDefine());
            }

            tempIndex++;
            return created;
        }
        /// <summary>
        /// Request a temp variable be created, cloning the properties of another scoreboard value.
        /// </summary>
        /// <returns></returns>
        public ScoreboardValue RequestTemp(ScoreboardValue clone)
        {
            ScoreboardValue created = clone.Clone() as ScoreboardValue;
            created.Name = TEMP_PREFIX + tempIndex;
            string name = created.Name + clone.GetMaxNameLength(); // make an 'id' out of this

            if (definedTempVars.Add(created))
            {
                executor.AddCommandsHead(created.CommandsInit());
                executor.AddCommandsHead(created.CommandsDefine());
            }

            tempIndex++;
            return created;
        }
        /// <summary>
        /// Request a temp value that is able to hold this literal.
        /// </summary>
        /// <param name="literal">The literal that will be able to be stored into a </param>
        /// <returns></returns>
        public ScoreboardValue RequestTemp(TokenLiteral literal, bool global, Statement forExceptions)
        {
            string name = TEMP_PREFIX + tempIndex;
            ScoreboardValue created = CreateFromLiteral(name, global, literal, forExceptions);

            if (definedTempVars.Add(created))
            {
                executor.AddCommandsHead(created.CommandsInit());
                executor.AddCommandsHead(created.CommandsDefine());
            }

            tempIndex++;
            return created;
        }
        /// <summary>
        /// Release the temp variable that was most recently created.
        /// Should be called always when done using variable from RequestTemp().
        /// </summary>
        public void ReleaseTemp() =>
            tempIndex--;

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
                return new ScoreboardValueInteger(name, global, this, forExceptions);
            else if (literal is TokenBooleanLiteral)
                return new ScoreboardValueBoolean(name, global, this, forExceptions);
            else if (literal is TokenDecimalLiteral)
            {
                float number = (literal as TokenDecimalLiteral).number;
                int precision = number.GetPrecision();
                return new ScoreboardValueDecimal(name, precision, global, this, forExceptions);
            }
            else throw new StatementException(forExceptions, "Internal Error: Attempted to " +
                    $"create a scoreboard value for invalid literal type {literal.GetType()}.");
        }
        /// <summary>
        /// Fetch a value/field definition from this statement. e.g., 'int coins = 3', 'decimal 3 thing', 'customStruct xyz'.
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
                    Executor.Warn("Decimal precisions >3 could begin to break with numbers greater than 1.");
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
        /// Save the current temp level to be recalled later using PopTempState().
        /// </summary>
        public TempStateContract PushTempState()
        {
            tempStack.Push(tempIndex);
            return new TempStateContract(this);
        }
        /// <summary>
        /// Restore the temp level to what it was at the last PushTempState() call.
        /// </summary>
        public void PopTempState() =>
            tempIndex = tempStack.Pop();

        /// <summary>
        /// Get a scoreboard value by its BASE NAME.
        /// </summary>
        /// <returns>Null if not found.</returns>
        public ScoreboardValue GetByName(string baseName) =>
            values.FirstOrDefault(v => v.Name.Equals(baseName));
        /// <summary>
        /// Get a scoreboard value by some accessor of it. e.g. name, name:a, name:field
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
        /// Tries to get a scoreboard value by its BASE NAME.
        /// </summary>
        /// <returns>True if found and output is set.</returns>
        public bool TryGetByName(string baseName, out ScoreboardValue output)
        {
            foreach (ScoreboardValue value in values)
                if (value.Name.Equals(baseName))
                {
                    output = value;
                    return true;
                }

            output = null;
            return false;
        }
        /// <summary>
        /// Tries to get a scoreboard value by some accessor of it. e.g. name, name:a, name:field
        /// </summary>
        /// <param name="accessor"></param>
        /// <returns>True if found and output is set.</returns>
        public bool TryGetByAccessor(string accessor, out ScoreboardValue output, bool allowMissingAccessor = false)
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

    /// <summary>
    /// A contract given by <see cref="ScoreboardManager.PushTempState"/> used to release the state through disposal.
    /// This does not have to be disposed if the called does not want to use its features.
    /// </summary>
    public class TempStateContract : IDisposable
    {
        private bool _isDisposed;
        private ScoreboardManager parent;

        internal TempStateContract(ScoreboardManager parent)
        {
            this.parent = parent;
        }
        public void Dispose()
        {
            if (_isDisposed)
                return;

            parent.PopTempState(); // <------ the big kahuna
            _isDisposed = true;
        }
    }
}
