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
        readonly Dictionary<string, ScoreboardValue> fields;
        public readonly string name;

        /// <summary>
        /// Create a struct definition using these scoreboard values as templates.
        /// </summary>
        /// <param name="values"></param>
        public StructDefinition(string name, params ScoreboardValue[] values)
        {
            this.name = name.ToUpper();
            this.fields = new Dictionary<string, ScoreboardValue>();

            int a = 0;
            int b = 0;

            foreach (ScoreboardValue value in values)
            {
                if (value is ScoreboardValueStruct)
                    throw new Compiler.LegacyTokenException(null, "Cannot contain struct inside of another struct.");

                if(a >= fieldNamesInternal.Length)
                {
                    a = 0;
                    b++;
                }

                string key = value.baseName;
                value.baseName = fieldNamesInternal[a++] + fieldNamesInternal[b];
                this.fields[key] = value;
            }
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
                return value.baseName;
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
            string id = GetFieldId(fieldName);

            if(id == null)
                throw new Exception("Invalid field for struct " + name + ": '" + fieldName + "'");

            return GetField(baseName, id);
        }
        public ScoreboardValue GetField(string baseName, string id)
        {
            if (fields.TryGetValue(id, out ScoreboardValue _value))
            {
                ScoreboardValue value = _value.Clone() as ScoreboardValue;
                value.baseName = baseName + ':' + value.baseName;
            }
            return null;
        }
        public ScoreboardValue GetFieldByIndex(string baseName, int index)
        {
            ScoreboardValue _value = fields.ElementAt(index).Value;
            ScoreboardValue value = _value.Clone() as ScoreboardValue;
            value.baseName = baseName + ':' + value.baseName;
            return null;
        }
        /// <summary>
        /// Get the string required to access this field in a struct instance.
        /// </summary>
        /// <param name="base"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetAccessor(string @base, string fieldName)
        {
            string id = GetFieldId(fieldName);

            if (id == null)
                return @base + ":XX";

            return @base + ':' + id;
        }

        /// <summary>
        /// Get all of the fields in this struct.
        /// </summary>
        /// <returns></returns>
        public ScoreboardValue[] GetFields()
        {
            return fields.Values.ToArray();
        }
        /// <summary>
        /// Get all the internal accessor names of the struct.
        /// </summary>
        /// <returns></returns>
        public string[] GetInternalFieldNames()
        {
            return fields.Values.Select(f => f.baseName).ToArray();
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