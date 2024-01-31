using mc_compiled.Commands.Selectors;
using mc_compiled.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using mc_compiled.MCC.Attributes;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Utilities for dealing with JSON/preprocessor stuff.
    /// </summary>
    public static class PreprocessorUtils
    {
        /// <summary>
        /// Determines whether the given JToken can be unwrapped to a C# object or surrounded with a literal.
        /// </summary>
        /// <param name="token">The JToken to check.</param>
        /// <returns>Returns true if the JToken can be unwrapped, false otherwise.</returns>
        public static bool CanTokenBeUnwrapped(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                case JTokenType.Object:
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Date:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                    return true;
                case JTokenType.None:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Returns if a JToken is a base type that can be converted into a non-json literal, outputting that token if true.
        /// </summary>
        /// <param name="token">The input token to check.</param>
        /// <param name="lineNumber">The line number to associate with the output literal, if created.</param>
        /// <param name="output">The output if this method returns true.</param>
        /// <returns>A boolean value indicating if the conversion was successful.</returns>
        public static bool TryGetLiteral(JToken token, int lineNumber, out TokenLiteral output)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                case JTokenType.Object:
                    output = new TokenJSONLiteral(token, lineNumber);
                    return true;

                case JTokenType.Integer:
                    output = new TokenIntegerLiteral(token.Value<int>(), IntMultiplier.none, lineNumber);
                    return true;
                case JTokenType.Float:
                    output = new TokenDecimalLiteral(token.Value<float>(), lineNumber);
                    return true;
                case JTokenType.String:
                    output = new TokenStringLiteral(token.Value<string>(), lineNumber);
                    return true;
                case JTokenType.Boolean:
                    output = new TokenBooleanLiteral(token.Value<bool>(), lineNumber);
                    return true;
                case JTokenType.Date:
                    output = new TokenStringLiteral(token.Value<DateTime>().ToString(), lineNumber);
                    return true;
                case JTokenType.Guid:
                    output = new TokenStringLiteral(token.Value<Guid>().ToString(), lineNumber);
                    return true;
                case JTokenType.Uri:
                    output = new TokenStringLiteral(token.Value<Uri>().OriginalString, lineNumber);
                    return true;
                case JTokenType.TimeSpan:
                    int time = (int)Math.Round(token.Value<TimeSpan>().TotalSeconds * 20d); // convert to ticks
                    output = new TokenIntegerLiteral(time, IntMultiplier.s, lineNumber);
                    return true;
                case JTokenType.None:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                default:
                    output = null;
                    return false;
            }
        }
        /// <summary>
        /// Returns if a JToken can be unwrapped into a C# object. Outputs the object in a parameter if true.
        /// </summary>
        /// <param name="token">The input token to unwrap.</param>
        /// <param name="obj">The object to output, if returning true.</param>
        /// <returns></returns>
        public static bool TryUnwrapToken(JToken token, out object obj)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                case JTokenType.Object:
                    obj = token;
                    return true;
                case JTokenType.Integer:
                    obj = token.Value<int>();
                    return true;
                case JTokenType.Float:
                    obj = token.Value<float>();
                    return true;
                case JTokenType.String:
                    obj = token.Value<string>();
                    return true;
                case JTokenType.Boolean:
                    obj = token.Value<bool>();
                    return true;
                case JTokenType.Date:
                    obj = token.Value<DateTime>().ToString();
                    return true;
                case JTokenType.Guid:
                    obj = token.Value<Guid>().ToString();
                    return true;
                case JTokenType.Uri:
                    obj = token.Value<Uri>().OriginalString;
                    return true;
                case JTokenType.TimeSpan:
                    obj = (int)Math.Round(token.Value<TimeSpan>().TotalSeconds * 20d); // convert to ticks
                    return true;
                case JTokenType.None:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                default:
                    obj = null;
                    return false;
            }
        }

        /// <summary>
        /// Wraps a dynamic value in its associated literal.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <param name="line">The line number the token should originate on.</param>
        /// <returns>null if the dynamic couldn't be wrapped.</returns>
        public static TokenLiteral DynamicToLiteral(dynamic value, int line)
        {
            switch (value)
            {
                case int integer:
                    return new TokenIntegerLiteral(integer, IntMultiplier.none, line);
                case float number:
                    return new TokenDecimalLiteral(number, line);
                case bool boolean:
                    return new TokenBooleanLiteral(boolean, line);
                case string text:
                    return new TokenStringLiteral(text, line);
                case Coord coordinate:
                    return new TokenCoordinateLiteral(coordinate, line);
                case Selector selector:
                    return new TokenSelectorLiteral(selector, line);
                case Range range:
                    return new TokenRangeLiteral(range, line);
                case JToken json:
                    return new TokenJSONLiteral(json, line);
                case IAttribute attribute:
                    return new TokenAttribute(attribute, line);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Parse the "JSONAccessor" format documented for MCCompiled.
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static IEnumerable<string> ParseAccessor(string tree)
        {
            char[] SEPARATORS = { '.', '/', '\\' };
            return tree.Split(SEPARATORS);
        }
    }
}
