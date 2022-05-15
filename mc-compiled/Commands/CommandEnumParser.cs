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
        static Dictionary<string, ParsedEnumValue> parser = new Dictionary<string, ParsedEnumValue>();

        public static bool TryParse(string input, out ParsedEnumValue result) =>
            parser.TryGetValue(input.ToUpper(), out result);

        public static void Put(ParsedEnumValue value) =>
            parser[value.ToString().ToUpper()] = value;
        public static void Put(string key, ParsedEnumValue value) =>
            parser[key.ToUpper()] = value;

        /// <summary>
        /// Initialize the global CommandEnumParser.
        /// </summary>
        public static void Init()
        {
            parser.Clear();

            if (Program.DEBUG)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Initializing enum parser...");
                Console.ForegroundColor = ConsoleColor.White;
            }

            IEnumerable<Type> allEnums = from a in Assembly.GetExecutingAssembly().GetTypes()
                                         where a.IsEnum
                                         select a;

            foreach(Type thisEnum in allEnums)
                thisEnum.GetCustomAttributes(); // calls their constructors

            if (Program.DEBUG)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Registered " + parser.Count + " unique parser identifiers.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
    public struct ParsedEnumValue
    {
        public readonly string enumName;
        public readonly object value;

        public ParsedEnumValue(string enumName, object value)
        {
            this.enumName = enumName;
            this.value = value;
        }
        /// <summary>
        /// Returns if this enum value is of a certain type.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <returns></returns>
        public bool IsType<T>() where T: System.Enum
        {
            return enumName.Equals(nameof(T));
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
                ParsedEnumValue finalValue = new ParsedEnumValue(type.Name, value);
                CommandEnumParser.Put(key, finalValue);
            }
        }
    }
}
