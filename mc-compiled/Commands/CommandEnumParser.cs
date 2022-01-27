using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands
{
    public static class CommandEnumParser
    {
        static Dictionary<string, object> parser = new Dictionary<string, object>();

        public static bool TryParse(string input, out object result) =>
            parser.TryGetValue(input.ToUpper(), out result);
        public static void Put(object value) =>
            parser[value.ToString().ToUpper()] = value;
        public static void Put(string key, object value) =>
            parser[key.ToUpper()] = value;
    }

    [System.AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    sealed class EnumParsableAttribute : Attribute
    {
        public EnumParsableAttribute(Type type)
        {
            if (!type.IsEnum)
                return;

            IEnumerable<object> array = type.GetEnumValues().Cast<object>();
            foreach(object value in array)
            {
                string key = value.ToString();
                CommandEnumParser.Put(key, value);
            }
        }
    }
}
