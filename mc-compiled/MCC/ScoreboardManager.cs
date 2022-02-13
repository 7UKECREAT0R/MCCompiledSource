using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Manages the virtual scoreboard.
    /// </summary>
    public class ScoreboardManager
    {
        private const string TEMP_PREFIX = "_mcc_tmp";
        private int tempIndex;
        private Stack<int> tempStack;

        public readonly Executor executor;
        public readonly List<string> definedTempVars;
        readonly Dictionary<string, StructDefinition> structs;
        readonly List<ScoreboardValue> values;

        public ScoreboardManager(Executor executor)
        {
            tempIndex = 0;
            tempStack = new Stack<int>();

            definedTempVars = new List<string>();
            structs = new Dictionary<string, StructDefinition>();
            values = new List<ScoreboardValue>();
            this.executor = executor;
        }

        /// <summary>
        /// Define a struct. Overwrites the existing one if it exists.
        /// </summary>
        /// <param name="def"></param>
        public void DefineStruct(StructDefinition def) =>
            structs[def.name] = def;
        /// <summary>
        /// Get a struct by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public StructDefinition GetStruct(string name) =>
            structs[name.ToUpper()];
        /// <summary>
        /// Tries to get a struct by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="def"></param>
        /// <returns>True if the struct was found.</returns>
        public bool TryGetStruct(string name, out StructDefinition def) =>
            structs.TryGetValue(name.ToUpper(), out def);

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
        /// Request a temp variable be created and initialized at the top of the file.
        /// </summary>
        /// <returns></returns>
        public ScoreboardValueInteger RequestTemp()
        {
            string name = TEMP_PREFIX + tempIndex;
            var created = new ScoreboardValueInteger(name, this, null);
            name += created.GetMaxNameLength().ToString(); // use maxNameLength like an ID
            if (!definedTempVars.Contains(name))
            {
                definedTempVars.Add(name);
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
            created.baseName = TEMP_PREFIX + tempIndex;
            string name = created.baseName + clone.GetMaxNameLength(); // make an 'id' out of this

            foreach (string accessor in created.GetAccessibleNames())
            {
                if (!definedTempVars.Contains(name))
                {
                    definedTempVars.Add(name);
                    executor.AddCommandsHead(created.CommandsInit());
                    executor.AddCommandsHead(created.CommandsDefine());
                }
            }
            tempIndex++;
            return created;
        }
        /// <summary>
        /// Request a temp value that is able to hold this literal.
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        public ScoreboardValue RequestTemp(TokenLiteral literal, Statement forExceptions)
        {
            ScoreboardValue created = CreateFromLiteral(TEMP_PREFIX + tempIndex, literal, forExceptions);
            string name = created.baseName + created.GetMaxNameLength(); // make an 'id' out of this

            foreach (string accessor in created.GetAccessibleNames())
            {
                if (!definedTempVars.Contains(name))
                {
                    definedTempVars.Add(name);
                    executor.AddCommandsHead(created.CommandsInit());
                    executor.AddCommandsHead(created.CommandsDefine());
                }
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

        public ScoreboardValue CreateFromLiteral(string name, TokenLiteral literal, Statement forExceptions)
        {
            if (literal is TokenIntegerLiteral)
                return new ScoreboardValueInteger(name, this, forExceptions);
            else if (literal is TokenBooleanLiteral)
                return new ScoreboardValueBoolean(name, this, forExceptions);
            else if (literal is TokenDecimalLiteral)
            {
                float number = (literal as TokenDecimalLiteral).number;
                int precision = number.GetPrecision();
                return new ScoreboardValueDecimal(name, precision, this, forExceptions);
            }
            else throw new StatementException(forExceptions, "Internal Error: Attempted to " +
                    $"create a scoreboard value for invalid literal type {literal.GetType()}.");
        }

        /// <summary>
        /// Save the current temp level to be recalled later using PopTempState().
        /// </summary>
        public void PushTempState() =>
            tempStack.Push(tempIndex);
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
            values.FirstOrDefault(v => v.baseName.Equals(baseName));
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
                {
                    if (value is ScoreboardValueStruct)
                    {
                        ScoreboardValueStruct @struct = value as ScoreboardValueStruct;
                        ScoreboardValue internalValue = @struct.FullyResolveAccessor(accessor);
                        return internalValue;
                    }
                    return value;
                }
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
                if (value.baseName.Equals(baseName))
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
        public bool TryGetByAccessor(string accessor, out ScoreboardValue output)
        {
            foreach (ScoreboardValue value in values)
            {
                string[] names = value.GetAccessibleNames();
                if (names.Contains(accessor))
                {
                    if (value is ScoreboardValueStruct)
                    {
                        ScoreboardValueStruct @struct = value as ScoreboardValueStruct;
                        ScoreboardValue internalValue = @struct.FullyResolveAccessor(accessor);
                        output = internalValue;
                        return true;
                    }
                    else
                    {
                        output = value;
                        return true;
                    }
                }
            }
            output = null;
            return false;
        }

    }
}
