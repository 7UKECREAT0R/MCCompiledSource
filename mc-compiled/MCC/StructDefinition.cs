using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Defines a struct with multiple variables inside of it.
    /// </summary>
    public class StructDefinition
    {
        static readonly string[] fieldNamesInternal =
            "abcdefghijklmnopqrstuvwxyz".ToCharArray().Select(c => c.ToString()).ToArray();

        // field name, value
        internal readonly Dictionary<string, ScoreboardValue> fields;

        public readonly string name;

        int a = 0, b = 0;

        /// <summary>
        /// Create a struct definition using these scoreboard values as templates.
        /// </summary>
        /// <param name="values"></param>
        public StructDefinition(string name, Compiler.Statement exception, params ScoreboardValue[] values)
        {
            this.name = name.ToUpper();
            this.fields = new Dictionary<string, ScoreboardValue>();

            foreach (ScoreboardValue value in values)
            {
                if (value is ScoreboardValueStruct)
                    throw new Compiler.StatementException(exception, "Cannot contain struct inside of another struct.");

                string fieldName = value.Name;
                value.Name = GetNextKey();
                this.fields[fieldName] = value;
            }
        }
        internal string GetNextKey()
        {
            if (a >= fieldNamesInternal.Length)
            {
                a = 0;
                b++;
            }

            return fieldNamesInternal[a++] + fieldNamesInternal[b];
        }

        /// <summary>
        /// Create a new scoreboard value using this struct as a template.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ScoreboardValueStruct Create(string name, ScoreboardManager manager, Compiler.Statement forExceptions) =>
            new ScoreboardValueStruct(name, this, manager, forExceptions);
        public override bool Equals(object obj)
        {
            if (!(obj is StructDefinition))
                return false;
            StructDefinition other = obj as StructDefinition;
            return name.Equals(other.name);
        }
        public override int GetHashCode() =>
            name.GetHashCode();

        /// <summary>
        /// Get the internal 2-letter name for a field.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>Null if no </returns>
        public string GetFieldId(string fieldName)
        {
            if (fields.TryGetValue(fieldName, out ScoreboardValue value))
                return value.Name;
            return null;
        }
        /// <summary>
        /// Get the number of fields defined in this struct.
        /// </summary>
        /// <returns></returns>
        public int GetFieldCount() =>
            fields.Count;
        public ScoreboardValue GetFieldFromAccessor(string accessor)
        {
            string[] parts = accessor.Split(':');

            if (parts.Length < 2)
                throw new Exception("Struct accessor was not in format NAME:FIELD");

            string baseName = parts[0];
            string fieldName = parts[1];

            return GetField(baseName, fieldName);
        }
        public ScoreboardValue GetField(string baseName, string fieldName)
        {
            if (fields.TryGetValue(fieldName, out ScoreboardValue _value))
            {
                ScoreboardValue value = _value.Clone() as ScoreboardValue;
                value.Name = baseName + ':' + value.Name;
                return value;
            }
            return null;
        }
        public ScoreboardValue GetFieldByIndex(string baseName, int index)
        {
            ScoreboardValue _value = fields.ElementAt(index).Value;
            ScoreboardValue value = _value.Clone() as ScoreboardValue;
            value.Name = baseName + ':' + value.Name;
            return value;
        }
        /// <summary>
        /// Get the string required to access this field in a struct instance.
        /// </summary>
        /// <param name="baseName"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetAccessor(string baseName, string fieldName)
        {
            string id = GetFieldId(fieldName);

            if (id == null)
                return baseName + ":XX";

            return baseName + ':' + id;
        }

        /// <summary>
        /// Get all of the fields in this struct.
        /// </summary>
        /// <returns></returns>
        public ScoreboardValue[] GetFields(string baseName)
        {
            return GetFieldNames().Select(str => GetField(baseName, str)).ToArray();
        }
        /// <summary>
        /// Get all the internal accessor names of the struct.
        /// </summary>
        /// <returns></returns>
        public string[] GetInternalFieldNames()
        {
            return fields.Values.Select(f => f.Name).ToArray();
        }
        /// <summary>
        /// Get all the accessor names of the struct.
        /// </summary>
        /// <returns></returns>
        public string[] GetFieldNames()
        {
            return fields.Keys.ToArray();
        }
        /// <summary>
        /// Get all of the fully qualified strings that can be used to access this struct.
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public string[] GetFullyQualifiedNames(string variable)
        {
            return GetFieldNames().Select(f => variable + ':' + f).ToArray();
        }
        /// <summary>
        /// Get all of the fully qualified scoreboard values.
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public string[] GetFullyQualifiedInternalNames(string variable)
        {
            return GetInternalFieldNames().Select(f => variable + ':' + f).ToArray();
        }
    }
}