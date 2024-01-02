using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Compiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.MCC.Compiler.TypeSystem.Implementations;

namespace mc_compiled.MCC
{
    /// <summary>
    /// A scoreboard value that can be written to.
    /// </summary>
    public sealed class ScoreboardValue : ICloneable
    {
        internal static readonly char[] HASH_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        internal static readonly char[] SUPPORTED_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_".ToCharArray();
        internal const string RETURN_NAME = "_return";
        internal const int MAX_NAME_LENGTH = 256;
        
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
                byte lower = (byte)((b << 2) % HASH_CHARS.Length);
                byte upper = (byte)((b >> 2) % HASH_CHARS.Length);

                char c1 = HASH_CHARS[lower];
                char c2 = HASH_CHARS[upper];

                chars[(i * 2)] = c1;
                chars[(i * 2) + 1] = c2;
            }

            return new string(chars);
        }
        
        internal readonly ScoreboardManager manager;
        public readonly Typedef type;
        public readonly ITypeStructure data;
        public readonly List<IAttribute> attributes;
        
        string internalName;
        string name;
        string documentation;
        
        /// <summary>
        /// The clarifier that this scoreboard value uses.
        /// </summary>
        public Clarifier clarifier { get; private set; }
        /// <summary>
        /// The internal name that represents the scoreboard objective in the compiled result.
        /// </summary>
        public string InternalName
        {
            get => internalName;
            private set => internalName = value.Length > MAX_NAME_LENGTH ? StandardizedHash(value) : value;
        }
        /// <summary>
        /// The name used to reference this variable in the user's code.
        /// </summary>
        public string Name
        {
            get => name ?? internalName;
            set => name = value;
        }
        /// <summary>
        /// The documentation tied to this scoreboard value. Can not return null.
        /// </summary>
        public string Documentation
        {
            get => documentation ?? Executor.UNDOCUMENTED_TEXT;
            set => documentation = string.IsNullOrWhiteSpace(value) ? null : value;
        }
        
        /// <summary>
        /// Require this variable's internal name to be hashed and hidden behind an alias (if no alias exists yet).
        /// </summary>
        /// <param name="nonce">A nonce string to append to the previous name when hashing.</param>
        public void ForceHash(string nonce = "")
        {
            if (name == null)
                name = internalName;

            internalName = StandardizedHash(name + nonce);
        }
        public ScoreboardValue(string name, bool global, Typedef type, ScoreboardManager manager)
        {
            this.manager = manager;
            this.InternalName = name;

            // hash was given to baseName
            if (name.Length > MAX_NAME_LENGTH)
                this.name = name;

            this.attributes = new List<IAttribute>();
            this.clarifier = new Clarifier(global);
            this.type = type;
        }
        public ScoreboardValue(string name, bool global, Typedef type, ITypeStructure data, ScoreboardManager manager)
        {
            this.manager = manager;
            this.InternalName = name;

            // hash was given to baseName
            if (name.Length > MAX_NAME_LENGTH)
                this.name = name;

            this.attributes = new List<IAttribute>();
            this.clarifier = new Clarifier(global);
            this.data = data;
            this.type = type;
        }
        public ScoreboardValue(Typedef type, Clarifier clarifier, ITypeStructure data, string internalName, string name, string documentation,
            ScoreboardManager manager)
        {
            this.attributes = new List<IAttribute>();
            this.type = type;
            this.clarifier = clarifier;
            this.data = data;
            this.internalName = internalName;
            this.name = name;
            this.documentation = documentation;
            this.manager = manager;
        }

        /// <summary>
        /// Add attributes to this scoreboard value.
        /// </summary>
        /// <param name="newAttributes">The attributes to add.</param>
        /// <param name="callingStatement">The calling statement for exceptions to blame.</param>
        /// <returns>This object for chaining.</returns>
        public ScoreboardValue WithAttributes(IEnumerable<IAttribute> newAttributes, Statement callingStatement)
        {
            foreach (IAttribute attribute in newAttributes)
            {
                attributes.Add(attribute);
                attribute.OnAddedValue(this, callingStatement);
            }
            
            return this;
        }

        /// <summary>
        /// Perform a fully implemented deep clone of this scoreboard value, with specified fields changed. Fully supports changes to type, name, data, clarifier, etc...
        /// </summary>
        /// <param name="callingStatement">The statement to blame when something explodes.</param>
        /// <param name="newType">If specified, the type to change the cloned ScoreboardValue to.</param>
        /// <param name="newClarifier">If specified, the Clarifier to change in the cloned ScoreboardValue.</param>
        /// <param name="newData">If specified, the data to change in the cloned ScoreboardValue.</param>
        /// <param name="newInternalName">If specified, the internal name to change the cloned ScoreboardValue to.</param>
        /// <param name="newName">If specified, the name to change the cloned ScoreboardValue to.</param>
        /// <returns></returns>
        public ScoreboardValue Clone(Statement callingStatement, Typedef newType = null, Clarifier newClarifier = null, ITypeStructure newData = null, string newInternalName = null, string newName = null)
        {
            Typedef type = newType ?? this.type;
            Clarifier clarifier = newClarifier ?? this.clarifier.Clone();
            ITypeStructure data = newData ?? (this.data == null ? null : type.CloneData(this.data));
            string internalName = newInternalName ?? this.internalName;
            string name = newName ?? this.name;

            var clone = new ScoreboardValue(type, clarifier, data, internalName, name, null, manager);
            clone.WithAttributes(attributes, callingStatement);

            return clone;
        }
        /// <summary>
        /// Perform a fully implemented deep clone of this scoreboard value. Use <see cref="Clone(mc_compiled.MCC.Compiler.Statement,mc_compiled.MCC.Compiler.TypeSystem.Typedef,mc_compiled.MCC.Compiler.Clarifier,object,string,string)"/>
        /// to change readonly fields where needed.
        /// </summary>
        /// <returns></returns>
        public object Clone() => Clone(null);

        /// <summary>
        /// Returns a deep clone of some value as a return value.
        /// </summary>
        /// <param name="returning">The scoreboard value being returned.</param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        public static ScoreboardValue AsReturnValue(ScoreboardValue returning, Statement callingStatement)
        {
            return returning.Clone(callingStatement,
                newClarifier: Clarifier.Global(),
                newName: RETURN_NAME,
                newInternalName: RETURN_NAME);
        }
        /// <summary>
        /// Create a return value based off of a literal.
        /// </summary>
        /// <param name="literal">The literal it's attempting to return.</param>
        /// <param name="sb">The scoreboard manager to attach to the newly created value.</param>
        /// <param name="forExceptions">The statement to blame if everything explodes.</param>
        /// <returns></returns>
        public static ScoreboardValue AsReturnValue(TokenLiteral literal, ScoreboardManager sb, Statement forExceptions)
        {
            switch (literal)
            {
                case TokenStringLiteral _:
                    throw new StatementException(forExceptions, "Cannot return a string.");
                case TokenSelectorLiteral _:
                    throw new StatementException(forExceptions, "Cannot return a selector.");
                case TokenIntegerLiteral _:
                    return new ScoreboardValue(RETURN_NAME, true, Typedef.INTEGER, sb);
                case TokenBooleanLiteral _:
                    return new ScoreboardValue(RETURN_NAME, true, Typedef.BOOLEAN, sb);
                case TokenDecimalLiteral decimalLiteral:
                {
                    float number = decimalLiteral.number;
                    int precision = number.GetPrecision();
                    var data = new FixedDecimalData(precision);
                    return new ScoreboardValue(RETURN_NAME, true, Typedef.FIXED_DECIMAL, data, sb);
                }
                default:
                    throw new StatementException(forExceptions, "Cannot return this literal.");
            }
        }

        /// <summary>
        /// Returns how this variable definition might look. but with attributes/extra things included.
        /// </summary>
        /// <returns></returns>
        public string GetExtendedTypeKeyword()
        {
            var sb = new StringBuilder();

            foreach (string code in attributes
                         .Select(attribute => attribute.GetCodeRepresentation())
                         .Where(code => code != null))
            {
                sb.Append(code);
                sb.Append(' ');
            }

            sb.Append(type.TypeKeyword);
            return sb.ToString();
        }

        /// <summary>
        /// Returns all of the scoreboard objectives associated with this value.
        /// </summary>
        /// <returns></returns>
        public string[] GetObjectives()
        {
            return type.GetObjectives(this);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = InternalName.GetHashCode();
                hashCode += (int)type.TypeEnum;

                if (this.data != null)
                    hashCode ^= this.data.TypeHashCode();

                return hashCode;
            }
        }

        /// <summary>
        /// Returns if this value needs to be converted in order to be compatible with 'other'.
        /// Same as <code>a.type != b.type</code>, but implementation aware (like with decimals with precisions not matching).
        /// </summary>
        /// <param name="other">The other scoreboard value in the operation</param>
        /// <returns></returns>
        public bool NeedsToBeConvertedFor(ScoreboardValue other)
        {
            return this.type.NeedsToBeConvertedTo(this, other);
        }
        /// <summary>
        /// Returns the commands needed to convert this value and store it in 'other'.
        /// Please check <see cref="NeedsToBeConvertedFor"/> first before using this.
        /// </summary>
        /// <param name="destination">The destination scoreboard value.</param>
        /// <param name="callingStatement">The calling statement, for exceptions.</param>
        public IEnumerable<string> CommandsConvert(ScoreboardValue destination, Statement callingStatement)
        {
            if (!this.type.CanConvertTo(destination.type))
                throw new StatementException(callingStatement,
                    $"Could not convert value \"{Name}\" to type \"{destination.type.TypeKeyword}\"");
            return this.type.ConvertTo(this, destination);
        }
        /// <summary>
        /// Returns the commands needed to define this value, as per its type.
        /// </summary>
        public IEnumerable<string> CommandsDefine()
        {
            string[] objectives = this.type.GetObjectives(this);
            return objectives.Select(Command.ScoreboardCreateObjective);
        }
        /// <summary>
        /// Returns the commands needed to initialize this value for the active clarifier.
        /// </summary>
        public IEnumerable<string> CommandsInit()
        {
            string selector = this.clarifier.CurrentString;
            return CommandsInit(selector);
        }
        /// <summary>
        /// Returns the commands needed to initialize this value for the given selector.
        /// </summary>
        /// <param name="selector">The Minecraft selector to initialize for.</param>
        public IEnumerable<string> CommandsInit(string selector)
        {
            string[] objectives = this.type.GetObjectives(this);
            return objectives.Select(objective => Command.ScoreboardAdd(selector, objective, 0));
        }
        /// <summary>
        /// Setup temporary variables, return the commands to set it up, and return the JSON raw terms needed to display this value.
        /// <br/>
        /// <b>Either field may be null, in which it will count as empty.</b>
        /// </summary>
        /// <param name="index">The index of the rawtext value, used to uniquely identify the temp variables and whatnot so they don't collide with other rawtexts.</param>
        public Tuple<string[], JSONRawTerm[]> ToRawText(ref int index)
        {
            return this.type.ToRawText(this, ref index);
        }

        /// <summary>
        /// Check <see cref="Typedef.CanCompareAlone"/> before attempting this.
        /// Returns the comparisons needed to compare this value alone. (booleans)
        /// </summary>
        /// <returns>null if <see cref="Typedef.CanCompareAlone"/> is false.</returns>
        internal ConditionalSubcommandScore[] CompareAlone(bool invert)
        {
            return type.CompareAlone(invert, this);
        }

        /// <summary>
        /// Compare a value with this type to a literal value. Returns both the setup commands needed, and the score comparisons needed.
        /// <br/>
        /// <b>Either field may be null, in which it will count as empty.</b>
        /// </summary>
        /// <param name="comparisonType">The comparison type.</param>
        /// <param name="literal">The literal to compare to.</param>
        /// <param name="callingStatement"></param>
        internal Tuple<string[], ConditionalSubcommandScore[]> CompareToLiteral(TokenCompare.Type comparisonType,
            TokenLiteral literal, Statement callingStatement)
        {
            return type.CompareToLiteral(comparisonType, this, literal, callingStatement);
        }
        
        /// <summary>
        /// Returns the commands needed to assign a literal to this value.
        /// </summary>
        /// <param name="literal">The literal to assign to this.</param>
        /// <param name="callingStatement">The calling statement.</param>
        public IEnumerable<string> AssignLiteral(TokenLiteral literal, Statement callingStatement)
        {
            return this.type.AssignLiteral(this, literal, callingStatement);
        }

        /// <summary>
        /// Returns the commands needed to add a literal to this value.
        /// </summary>
        /// <param name="literal">The literal to add to this.</param>
        /// <param name="callingStatement">The calling statement.</param>
        public IEnumerable<string> AddLiteral(TokenLiteral literal, Statement callingStatement)
        {
            return this.type.AddLiteral(this, literal, callingStatement);
        }

        /// <summary>
        /// Returns the commands needed to subtract a literal from this value.
        /// </summary>
        /// <param name="literal">The literal to subtract from this.</param>
        /// <param name="callingStatement">The calling statement.</param>
        public IEnumerable<string> SubtractLiteral(TokenLiteral literal, Statement callingStatement)
        {
            return this.type.SubtractLiteral(this, literal, callingStatement);
        }

        /// <summary>
        /// Calls one of this object's operation methods (Add, Subtract, Multiply, etc...) based on the given arithmetic type.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="type"></param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        public IEnumerable<string> Operation(ScoreboardValue other, TokenArithmetic.Type type,
            Statement callingStatement)
        {
            switch (type)
            {
                case TokenArithmetic.Type.ADD: return Add(other, callingStatement);
                case TokenArithmetic.Type.SUBTRACT: return Subtract(other, callingStatement);
                case TokenArithmetic.Type.MULTIPLY: return Multiply(other, callingStatement);
                case TokenArithmetic.Type.DIVIDE: return Divide(other, callingStatement);
                case TokenArithmetic.Type.MODULO: return Modulo(other, callingStatement);
                case TokenArithmetic.Type.SWAP: return Swap(other, callingStatement);
                default:
                    throw new StatementException(callingStatement, $"Unknown arithmetic type '{type}'.");
            }
        }
        
        /// <summary>
        /// Returns the commands needed to assign another value to self.
        /// </summary>
        /// <param name="other">The </param>
        /// <param name="callingStatement"></param>
        /// <returns></returns>
        public IEnumerable<string> Assign(ScoreboardValue other, Statement callingStatement)
        {
            if (this.NeedsToBeConvertedFor(other))
                return other.CommandsConvert(this, callingStatement);

            return this.type._Assign(this, other, callingStatement);
        }
        /// <summary>
        /// Returns the commands needed to add another value to this.
        /// <code>
        ///     this += other
        /// </code>
        /// </summary>
        /// <param name="other">B</param>
        /// <param name="callingStatement"></param>
        /// <returns>The commands needed to perform the operation.</returns>
        public IEnumerable<string> Add(ScoreboardValue other, Statement callingStatement)
        {
            var commands = new List<string>();
            ScoreboardValue b;
            
            if (this.NeedsToBeConvertedFor(other))
            {
                // create temp to hold it in
                ScoreboardManager manager = other.manager;
                b = manager.temps.RequestCopy(this);
                
                // convert 'other' into the new temp
                commands.AddRange(other.CommandsConvert(b, callingStatement));
            }
            else
                b = other;
            
            commands.AddRange(this.type._Add(this, b, callingStatement));
            return commands;
        }
        /// <summary>
        /// Returns the commands needed to subtract another value from this.
        /// <code>
        ///     this -= other
        /// </code>
        /// </summary>
        /// <param name="other">B</param>
        /// <param name="callingStatement"></param>
        /// <returns>The commands needed to perform the operation.</returns>
        public IEnumerable<string> Subtract(ScoreboardValue other, Statement callingStatement)
        {
            var commands = new List<string>();
            ScoreboardValue b;
            
            if (this.NeedsToBeConvertedFor(other))
            {
                // create temp to hold it in
                ScoreboardManager manager = other.manager;
                b = manager.temps.RequestCopy(this);
                
                // convert 'other' into the new temp
                commands.AddRange(other.CommandsConvert(b, callingStatement));
            }
            else
                b = other;
            
            commands.AddRange(this.type._Subtract(this, b, callingStatement));
            return commands;
        }
        /// <summary>
        /// Returns the commands needed to multiply another value with this.
        /// <code>
        ///     this *= other
        /// </code>
        /// </summary>
        /// <param name="other">B</param>
        /// <param name="callingStatement"></param>
        /// <returns>The commands needed to perform the operation.</returns>
        public IEnumerable<string> Multiply(ScoreboardValue other, Statement callingStatement)
        {
            var commands = new List<string>();
            ScoreboardValue b;
            
            if (this.NeedsToBeConvertedFor(other))
            {
                // create temp to hold it in
                ScoreboardManager manager = other.manager;
                b = manager.temps.RequestCopy(this);
                
                // convert 'other' into the new temp
                commands.AddRange(other.CommandsConvert(b, callingStatement));
            }
            else
                b = other;
            
            commands.AddRange(this.type._Multiply(this, b, callingStatement));
            return commands;
        }
        /// <summary>
        /// Returns the commands needed to divide this with another value.
        /// <code>
        ///     this /= other
        /// </code>
        /// </summary>
        /// <param name="other">B</param>
        /// <param name="callingStatement"></param>
        /// <returns>The commands needed to perform the operation.</returns>
        public IEnumerable<string> Divide(ScoreboardValue other, Statement callingStatement)
        {
            var commands = new List<string>();
            ScoreboardValue b;
            
            if (this.NeedsToBeConvertedFor(other))
            {
                // create temp to hold it in
                ScoreboardManager manager = other.manager;
                b = manager.temps.RequestCopy(this);
                
                // convert 'other' into the new temp
                commands.AddRange(other.CommandsConvert(b, callingStatement));
            }
            else
                b = other;
            
            commands.AddRange(this.type._Divide(this, b, callingStatement));
            return commands;
        }
        /// <summary>
        /// Returns the commands needed to modulo this with another value.
        /// <code>
        ///     this %= other
        /// </code>
        /// </summary>
        /// <param name="other">B</param>
        /// <param name="callingStatement"></param>
        /// <returns>The commands needed to perform the operation.</returns>
        public IEnumerable<string> Modulo(ScoreboardValue other, Statement callingStatement)
        {
            var commands = new List<string>();
            ScoreboardValue b;
            
            if (this.NeedsToBeConvertedFor(other))
            {
                // create temp to hold it in
                ScoreboardManager manager = other.manager;
                b = manager.temps.RequestCopy(this);
                
                // convert 'other' into the new temp
                commands.AddRange(other.CommandsConvert(b, callingStatement));
            }
            else
                b = other;
            
            commands.AddRange(this.type._Modulo(this, b, callingStatement));
            return commands;
        }
        /// <summary>
        /// Returns the commands needed to swap this with another value.
        /// <code>
        ///     this %= other
        /// </code>
        /// </summary>
        /// <param name="other">B</param>
        /// <param name="callingStatement"></param>
        /// <returns>The commands needed to perform the operation.</returns>
        public IEnumerable<string> Swap(ScoreboardValue other, Statement callingStatement)
        {
            var commands = new List<string>();
            
            if (this.NeedsToBeConvertedFor(other))
            {
                // create temp to hold it in
                ScoreboardManager manager = other.manager;
                
                ScoreboardValue a = manager.temps.RequestCopy(this);
                ScoreboardValue b = manager.temps.RequestCopy(other);
                
                // convert both types into the temp variables
                commands.AddRange(b.Assign(this, callingStatement));
                commands.AddRange(a.Assign(other, callingStatement));
                
                // assign to their destinations, assume compatible
                commands.AddRange(other.type._Assign(other, b, callingStatement));
                commands.AddRange(this.type._Assign(this, a, callingStatement));
                return commands;
            }

            string[] thisObjectives = this.type.GetObjectives(this);
            string[] otherObjectives = other.type.GetObjectives(other);
            string thisSelector = this.clarifier.CurrentString;
            string otherSelector = other.clarifier.CurrentString;
            
            for (int i = 0; i < thisObjectives.Length; i++)
            {
                string objA = thisObjectives[i];
                string objB = otherObjectives[i];
                commands.Add(Command.ScoreboardOpSwap(thisSelector, objA, otherSelector, objB));
            }
            return commands;
        }
    }
}