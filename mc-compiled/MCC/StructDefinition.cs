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
        readonly Dictionary<string, ScoreboardValue> values;

        /// <summary>
        /// Create a struct definition using these scoreboard values as templates.
        /// </summary>
        /// <param name="values"></param>
        public StructDefinition(params ScoreboardValue[] values)
        {
            this.values = new Dictionary<string, ScoreboardValue>();

            int a = 0;
            int b = 0;

            foreach (ScoreboardValue value in values)
            {
                if (value is ScoreboardValueStruct)
                    throw new Compiler.TokenException(null, "Cannot contain struct inside of another struct.");

                if(a >= fieldNamesInternal.Length)
                {
                    a = 0;
                    b++;
                }

                string name = value.baseName;
                value.baseName = fieldNamesInternal[a++] + fieldNamesInternal[b];
                this.values[name] = value;
            }
        }

        /// <summary>
        /// Get the internal 2-letter name for a field.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>Null if no </returns>
        public string GetFieldId(string fieldName)
        {
            if (values.TryGetValue(fieldName, out ScoreboardValue value))
                return value.baseName;
            return null;
        }
        public ScoreboardValue GetField(string fieldName)
        {
            if (values.TryGetValue(fieldName, out ScoreboardValue value))
                return value;
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
            return values.Values.ToArray();
        }
        /// <summary>
        /// Get all the internal accessor names of the struct.
        /// </summary>
        /// <returns></returns>
        public string[] GetInternalFieldNames()
        {
            return values.Values.Select(f => f.baseName).ToArray();
        }
        /// <summary>
        /// Get all the accessor names of the struct.
        /// </summary>
        /// <returns></returns>
        public string[] GetFieldNames()
        {
            return values.Keys.ToArray();
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