using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Selectors
{
    /// <summary>
    /// Represents selector option which limits based on count. Only here for future proofing.
    /// </summary>
    public struct Count
    {
        public const int NONE = -1;
        public int count;
        
        public Count(int count)
        {
            this.count = count;
        }
        public static Count Parse(string str)
        {
            return new Count(int.Parse(str));
        }
        public string GetSection()
        {
            if (count == NONE)
                return null;
            return "c=" + count;
        }
    }
}
