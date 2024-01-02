using System;
using System.Collections.Generic;
using System.Text;

namespace mc_compiled.MCC.ServerWebSocket
{
    public static class HttpHelper
    {
        /// <summary>
        /// Parses an HTTP request from this string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseHTTP(this string data)
        {
            string[] lines = data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> entries = new Dictionary<string, string>();

            // load into dictionary
            foreach (string line in lines)
            {
                int index = line.IndexOf(':');
                if (index == -1)
                    continue;
                string key = line.Substring(0, index);
                int offset = char.IsWhiteSpace(line[index + 1]) ? 2 : 1;
                string value = line.Substring(index + offset);

                entries[key] = value;
            }

            return entries;
        }
        /// <summary>
        /// Returns a string representing a properly constructed HTTP request from this dictionary's data.
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static string ToHTTP(this Dictionary<string, string> entries, string header)
        {
            const string nl = "\r\n";
            StringBuilder sb = new StringBuilder();

            sb.Append(header);
            sb.Append(nl);

            foreach(KeyValuePair<string, string> entry in entries)
            {
                string a = entry.Key;
                string b = entry.Value;
                sb.Append(a);
                sb.Append(": ");
                sb.Append(b);
                sb.Append(nl);
            }

            // final newline
            sb.Append(nl);

            // return final string
            return sb.ToString();
        }

        public static byte ReverseOrder(this byte b)
        {
            // authored by Sánchez, Juan

            byte reversed = 0;

            for(byte mask = 0b10000000; ((int)mask) > 0; mask >>= 1)
            {
                reversed = (byte)(reversed >> 1);

                // check if there is a 1 where the mask is currently
                byte temp = (byte)(b & mask);
                
                // insert a 1 on the left if there was a 1
                if (temp != 0)
                    reversed = (byte)(reversed | 0b10000000);
            }

            return reversed;
        }
        public static sbyte ReverseOrder(this sbyte b)
        {
            // authored by Sánchez, Juan

            sbyte reversed = 0;

            // don't care if c# thinks i'm stupid
            unchecked
            {
                for (sbyte mask = (sbyte)0b10000000; ((int)mask) > 0; mask >>= 1)
                {
                    reversed = (sbyte)(reversed >> 1);

                    // check if there is a 1 where the mask is currently
                    sbyte temp = (sbyte)(b & mask);

                    // insert a 1 on the left if there was a 1
                    if (temp != 0)
                        reversed = (sbyte)(reversed | 0b10000000);
                }
            }

            return reversed;
        }

        /// <summary>
        /// Encodes this string in Base64.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Base64Encode(this string source)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(source));
        }
        /// <summary>
        /// Decodes this string from Base64.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Base64Decode(this string source)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(source));
        }
    }
}
