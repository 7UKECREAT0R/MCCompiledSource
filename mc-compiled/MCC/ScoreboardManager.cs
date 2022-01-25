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
        private const string TEMP_PREFIX = "_mcc_temp";
        private int tempIndex;

        public readonly Executor executor;
        readonly List<int> definedTempVars;
        readonly Dictionary<string, StructDefinition> structs;
        readonly List<ScoreboardValue> values;

        public ScoreboardManager(Executor executor)
        {
            definedTempVars = new List<int>();
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
            var created = new ScoreboardValueInteger(TEMP_PREFIX + tempIndex, this);
            if (!definedTempVars.Contains(tempIndex))
            {
                definedTempVars.Add(tempIndex);
                executor.AddCommandsHead(created.CommandsInit()); // init at top of file
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
                    output = value;
                    return true;
                }
            }
            output = null;
            return false;
        }

    }
}
