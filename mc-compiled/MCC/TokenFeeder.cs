using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Like a stream of tokens
    /// </summary>
    public class TokenFeeder
    {
        private Token[] tokens;
        private int readerIndex;

        public readonly int length;

        public TokenFeeder(params Token[] tokens)
        {
            this.tokens = tokens;
            readerIndex = 0;
            length = tokens.Length;
        }
        public Token[] GetArray()
        {
            return tokens;
        }
        public void Reset()
        {
            readerIndex = 0;
            return;
        }

        /// <summary>
        /// Whether this token feeder has another Token in it.
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            return readerIndex < length;
        }
        /// <summary>
        /// Get the next Token and increment the counter.
        /// </summary>
        /// <returns></returns>
        public Token Next()
        {
            if (readerIndex >= length)
                return null;
            return tokens[readerIndex++];
        }
        /// <summary>
        /// Peek at the next Token but don't increment the counter.
        /// </summary>
        /// <returns></returns>
        public Token Peek()
        {
            if (readerIndex >= length)
                return null;
            return tokens[readerIndex];
        }
        /// <summary>
        /// Peek at the next <code>count</code> items. Returned array length isn't guaranteed if at the end of the stream.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Token[] Peek(int count)
        {
            if (count < 0)
                return null;
            if (readerIndex + count >= length)
                count = length - readerIndex - 1;

            if (count < 1)
                return new Token[0];

            Token[] ret = new Token[count];
            for (int i = 0; i < count; i++)
                ret[i] = tokens[readerIndex + i];
            return ret;
        }


        public override int GetHashCode()
        {
            int hash = 0;
            foreach(Token t in tokens)
            {
                if (hash == 0)
                    hash = t.GetHashCode();
                else hash ^= t.GetHashCode();
            }
            return hash;
        }
    }
}
