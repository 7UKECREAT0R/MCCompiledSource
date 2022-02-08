using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Initialize the global CommandEnumParser.
        /// </summary>
        public static void Init()
        {
            parser.Clear();

            IEnumerable<Type> allEnums = from a in Assembly.GetExecutingAssembly().GetTypes()
                                         where a.IsEnum
                                         select a;

            foreach(Type thisEnum in allEnums)
                thisEnum.GetCustomAttributes(); // calls their constructors

            if (Program.DEBUG)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Registered " + parser.Count + " unique parser identifiers.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
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
