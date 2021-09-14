using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding
{
    public struct UUID
    {
        public static readonly char[] validChars = "1234567890abcdef".ToCharArray();
        public readonly string identifier;

        private UUID(string identifier)
        {
            this.identifier = identifier;
        }
        public static UUID CreateNew(string seed)
        {
            Random random = new Random(seed.GetHashCode());
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < 8; i++)
                builder.Append(validChars[random.Next(validChars.Length)]);
            builder.Append('-');
            for (int i = 0; i < 4; i++)
                builder.Append(validChars[random.Next(validChars.Length)]);
            builder.Append('-');
            for (int i = 0; i < 4; i++)
                builder.Append(validChars[random.Next(validChars.Length)]);
            builder.Append('-');
            for (int i = 0; i < 12; i++)
                builder.Append(validChars[random.Next(validChars.Length)]);

            return new UUID(builder.ToString());
        }
        public override string ToString()
        {
            return identifier;
        }
    }
}
