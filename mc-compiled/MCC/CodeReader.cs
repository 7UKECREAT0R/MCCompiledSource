using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Reads through and analyzes code.
    /// </summary>
    public class CodeReader
    {
        char[] characters;
        string content;

        int index;
        readonly int end;

        public CodeReader(string content)
        {
            string[] lines = content.Split('\n');
            content = string.Join("\n", from line in lines select line.Trim());

            content = content.Replace("\r\n", "\n");    // remove carriage returns
            content = content.Replace("{\n", "\n{\n");  // format generalization
            content = content.Replace("\n} ", "\n}\n"); // format generalization
            if (!content.EndsWith("\n"))                //
                content += "\n";                        // newline at EOF
            characters = content.ToCharArray();
            this.content = content;

            index = 0;
            end = characters.Length;
        }

        /// <summary>
        /// Read a line and return it. Advances the reader to the start of the next line.
        /// </summary>
        /// <returns></returns>
        public string ReadLine()
        {
            int start = index;

            if (index < characters.Length)
            {
                while (characters[index++] != '\n')
                    if (index >= characters.Length)
                        return null;
            }
            else return null;

            int end = index;
            int length = end - start - 1; // cut off newline
            return content.Substring(start, length);
        }
        /// <summary>
        /// Find the next index of a character starting at the reader position.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public int SearchForCharacter(char c)
        {
            int search = index;
            while (characters[++search] != c)
                if (search + 1 >= characters.Length)
                    return -1;
            return search;
        }
        /// <summary>
        /// Find the next index of a character starting at an index.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public int SearchForCharacterFrom(char c, int start)
        {
            if (start < 0)
                return -1;
            if (start >= end)
                return -1;

            while (characters[++start] != c)
                if (start + 1 >= end)
                    return -1;

            return start;
        }

        /// <summary>
        /// Get a block as defined by a starting and ending char, starting at <code>reader - scrollBack</code>
        /// </summary>
        /// <param name="startChar"></param>
        /// <param name="endChar"></param>
        /// <returns></returns>
        public string GetBlock(char startChar, char endChar, int scrollBack = 3)
        {
            int level = 0;
            int read = index - scrollBack;
            if (read < 0) read = 0;

            int startIndex = read - 1;
            while(characters[++startIndex] != startChar)
            {
                if (startIndex + 1 >= characters.Length)
                    throw new Exception("Couldn't find a starting character for Block Search");
            }
            
            read = startIndex - 1;
            bool eof = true;
            for (int i = read; i < characters.Length; i++)
            {
                char c = characters[i];
                if (c == startChar)
                    level++;
                if (c == endChar)
                {
                    level--;
                    if (level <= 0)
                    {
                        eof = false;
                        index = i;
                        break;
                    }
                }
            }

            if (eof)
                throw new Exception("No closer for Block Search. (Current level " + level + ")");

            // Trim 3 characters off start and 1 off end.
            int length = index - read - 4;
            int start = read + 3;

            return content.Substring(start, length);
        }
    }
}