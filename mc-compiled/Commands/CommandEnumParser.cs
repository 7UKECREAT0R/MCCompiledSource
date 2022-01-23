using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands
{
    public static class CommandEnumParser
    {
        static Dictionary<string, Enum> parser = new Dictionary<string, Enum>();

        public static bool TryParse(string input, out Enum result) =>
            parser.TryGetValue(input.ToUpper(), out result);
        public static void Put(Enum value) =>
            parser[value.ToString().ToUpper()] = value;
        public static void Put(string key, Enum value) =>
            parser[key.ToUpper()] = value;
    }

    [System.AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    sealed class EnumParsableAttribute : Attribute
    {
        public EnumParsableAttribute(Type type)
        {
            if (!type.IsEnum)
                return;

            IEnumerable<Enum> array = type.GetEnumValues().Cast<Enum>();
            foreach(Enum value in array)
            {
                string key = value.ToString();
                CommandEnumParser.Put(key, value);
            }
        }
    }
}
