using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Compiler.TypeSystem;

namespace mc_compiled.MCC;

/// <summary>
///     Manages the virtual scoreboard.
/// </summary>
public class ScoreboardManager
{
    /// <summary>
    ///     A type of value that can be defined.
    /// </summary>
    public enum ValueType
    {
        /// <summary>
        ///     Infer type from right-hand side of definition.
        /// </summary>
        INFER,
        /// <summary>
        ///     Invalid value type.
        /// </summary>
        INVALID,

        /// <summary>
        ///     An integral value.
        /// </summary>
        INT,
        /// <summary>
        ///     A decimal value with a set precision.
        /// </summary>
        FIXED_DECIMAL,
        /// <summary>
        ///     A boolean (true/false) value.
        /// </summary>
        BOOL,
        /// <summary>
        ///     A time value, represented in ticks.
        /// </summary>
        TIME,
        /// <summary>
        ///     A preprocessor variable.
        /// </summary>
        PPV
    }

    public readonly Executor executor;
    public readonly TempManager temps;
    internal readonly HashSet<ScoreboardValue> values;

    /// <summary>
    ///     Create a new ScoreboardManager tied to the given <see cref="Executor" />. Changes will reflect in the
    ///     <see cref="Executor" /> in various ways.
    /// </summary>
    /// <param name="executor"></param>
    public ScoreboardManager(Executor executor)
    {
        this.temps = new TempManager(this, executor);
        this.values = [];
        this.executor = executor;
    }

    /// <summary>
    ///     Define all of the given non-null scoreboard values if they haven't already. Places them in the 'init' file.
    /// </summary>
    /// <param name="newValues">The values to define.</param>
    public void DefineMany(params ScoreboardValue[] newValues)
    {
        DefineMany((IEnumerable<ScoreboardValue>) newValues);
    }
    /// <summary>
    ///     Define all of the given non-null scoreboard values if they haven't already. Places them in the 'init' file.
    /// </summary>
    /// <param name="newValues">The values to define.</param>
    public void DefineMany(IEnumerable<ScoreboardValue> newValues)
    {
        foreach (ScoreboardValue value in newValues)
        {
            if (value == null)
                continue;

            string name = value.InternalName;

            if (this.temps.DefinedTemps.Contains(name))
                continue;

            this.temps.DefinedTemps.Add(name);
            this.temps.DefinedTempsRecord.Add(name);
            this.executor.AddCommandsInit(value.CommandsDefine());
        }
    }

    /// <summary>
    ///     Attempts to throw a <see cref="StatementException" /> if there is a duplicate value with the same name.
    ///     Does not throw if the value exactly matches another value.
    /// </summary>
    /// <param name="value">The scoreboard value to check for duplicates of.</param>
    /// <param name="callingStatement">The statement that is calling this method.</param>
    /// <param name="identicalDuplicate">
    ///     An output parameter indicating if the value is an identical duplicate of an existing
    ///     value.
    /// </param>
    public void TryThrowForDuplicate(ScoreboardValue value, Statement callingStatement, out bool identicalDuplicate)
    {
        ScoreboardValue find = this.values.FirstOrDefault(v =>
            v.InternalName.Equals(value.InternalName) || v.Name.Equals(value.Name));

        if (find == null)
        {
            identicalDuplicate = false;
            return;
        }

        if (find.Equals(value))
        {
            identicalDuplicate = true;
            return; // identical copy
        }

        if (find.Name.Equals(find.InternalName))
            throw new StatementException(callingStatement, $"Value \"{find.Name}\" already exists.");

        throw new StatementException(callingStatement,
            $"Value \"{find.Name}\" already exists (internally, \"{find.InternalName}\").");
    }
    /// <summary>
    ///     Add a scoreboard value to the cache.
    /// </summary>
    /// <param name="value"></param>
    public void Add(ScoreboardValue value)
    {
        this.values.Add(value);
    }
    /// <summary>
    ///     Add a set of scoreboard values to the cache.
    /// </summary>
    /// <param name="values">The values to add.</param>
    public void AddRange(IEnumerable<ScoreboardValue> values)
    {
        foreach (ScoreboardValue value in values)
            Add(value);
    }

    /// <summary>
    ///     Fetch a value/field definition from this statement. e.g., 'int coins = 3', 'decimal 3 thing', 'bool isPlaying'.
    ///     This method automatically performs type inference if possible.
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    internal static ValueDefinition GetNextValueDefinition(Statement tokens)
    {
        var attributes = new List<IAttribute>();

        FindAttributes();

        Typedef type = null;
        ITypeStructure data = null;
        string name = null;

        if (tokens.NextIs<TokenIdentifier>(false))
        {
            var identifier = tokens.Next<TokenIdentifier>(null);
            string typeWord = identifier.word.ToUpper();
            Typedef[] types = Typedef.ALL_TYPES;

            type = types.FirstOrDefault(t => t.TypeKeyword.Equals(typeWord));

            if (type == null)
            {
                name = identifier.word;
            }
            else
            {
                // process input tokens, if needed
                TypePattern pattern = type.SpecifyPattern;
                if (pattern != null)
                {
                    IEnumerable<Token> remaining = tokens.GetRemainingTokens();
                    MatchResult match = pattern.Check(remaining.ToArray());

                    if (match.match)
                    {
                        data = type.AcceptPattern(tokens);
                    }
                    else
                    {
                        MultiType[] missing = match.missing;
                        string list = string.Join(", ", missing.Select(m => m.ToString()));
                        throw new StatementException(tokens,
                            $"Missing argument(s) for type definition '{type.TypeKeyword}': {list}");
                    }
                }
            }
        }

        FindAttributes();

        if (name == null)
        {
            if (!tokens.NextIs<TokenStringLiteral>(false))
                throw new StatementException(tokens, "No name specified after type.");
            name = tokens.Next<TokenStringLiteral>(null);
        }

        // the default value to set it to.
        Token defaultValue = null;

        if (tokens.NextIs<TokenAssignment>(false))
        {
            tokens.Next();
            defaultValue = tokens.Next();
        }

        IAttribute[] attributesArray = attributes.ToArray();
        var definition = new ValueDefinition(attributesArray, name, type, dataObject: data, defaultValue: defaultValue);

        // try to infer type based on the default value.
        if (type != null)
            return definition;
        if (defaultValue == null)
            throw new StatementException(tokens,
                $"Cannot infer value \"{name}\"s type because there is no default value in its declaration. Hint: 'define name = 123'");

        definition.InferType(tokens);

        return definition;

        void FindAttributes()
        {
            while (tokens.NextIs<TokenAttribute>(false))
            {
                var _attribute = tokens.Next<TokenAttribute>(null);
                attributes.Add(_attribute.attribute);
            }
        }
    }

    /// <summary>
    ///     Tries to get a scoreboard value by its INTERNAL NAME.
    /// </summary>
    /// <returns>True if found and output is set.</returns>
    public bool TryGetByInternalName(string internalName, out ScoreboardValue output)
    {
        output = this.values.FirstOrDefault(value => value.InternalName.Equals(internalName));
        return output != null;
    }
    /// <summary>
    ///     Tries to get a scoreboard value by its user-facing name.
    /// </summary>
    /// <returns>true if found and output is set.</returns>
    public bool TryGetByUserFacingName(string name, out ScoreboardValue output)
    {
        output = this.values.FirstOrDefault(value => value.Name.Equals(name));
        return output != null;
    }

    /// <summary>
    ///     A shallow variable definition used in structs, defines, functions, etc...
    /// </summary>
    internal struct ValueDefinition
    {
        internal IAttribute[] attributes;
        internal string name;
        internal Typedef type;

        // either of these could be valid, or both null; dataObject takes precedence.
        internal ITypeStructure dataObject;
        internal TokenLiteral[] data;

        internal readonly Token defaultValue;

        internal ValueDefinition(IAttribute[] attributes, string name, Typedef type,
            TokenLiteral[] data = null, ITypeStructure dataObject = null, Token defaultValue = null)
        {
            this.attributes = attributes;
            this.name = name;
            this.type = type;
            this.data = data;
            this.dataObject = dataObject;
            this.defaultValue = defaultValue;
        }
        /// <summary>
        ///     Create a scoreboard value based off of this definition.
        /// </summary>
        /// <returns></returns>
        internal ScoreboardValue Create(ScoreboardManager sb, Statement tokens)
        {
            ITypeStructure localData = this.dataObject;

            if (localData == null)
                if (this.type.SpecifyPattern != null)
                {
                    // check pattern
                    MatchResult result = this.type.SpecifyPattern.Check(tokens);

                    if (!result.match)
                    {
                        MultiType[] missingTokens = result.missing;
                        IEnumerable<string> missingTokensStrings = missingTokens.Select(mt => mt.ToString());
                        throw new StatementException(tokens,
                            $"Value type \"{this.type.TypeKeyword}\" missing argument(s): {string.Join(", ", missingTokensStrings)}");
                    }

                    // digest pattern
                    localData = this.type.AcceptPattern(tokens);
                }

            ScoreboardValue value = new ScoreboardValue(this.name, false, this.type, localData, sb)
                .WithAttributes(this.attributes, tokens);
            return value;
        }
        internal void InferType(Statement tokens)
        {
            switch (this.defaultValue)
            {
                // check if it's a literal.
                case TokenLiteral literal:
                {
                    this.type = literal.GetTypedef();

                    if (this.type == null)
                        throw new StatementException(tokens,
                            $"Input '{literal.AsString()}' cannot be stored in a value.");

                    if (this.type.SpecifyPattern != null)
                    {
                        if (this.type.CanAcceptLiteralForData(literal))
                            this.dataObject = this.type.AcceptLiteral(literal);
                        else
                            this.data = tokens
                                .GetRemainingTokens()
                                .OfType<TokenLiteral>()
                                .ToArray();
                    }

                    break;
                }
                // check if it's a runtime value.
                case TokenIdentifierValue identifier:
                {
                    this.type = identifier.value.type;
                    this.dataObject = identifier.value.data;
                    break;
                }
                default:
                    throw new StatementException(tokens,
                        $"Cannot assign value of type {this.defaultValue.GetType().Name} into a variable");
            }
        }
    }
}