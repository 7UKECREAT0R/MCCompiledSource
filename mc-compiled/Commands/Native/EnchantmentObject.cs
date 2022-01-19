using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Native
{
    public struct EnchantmentObject
    {
        // If level needs to be parsed later.
        public string stringLevel;
        public bool hasStringLevel;

        public EnchantmentObject Resolve(MCC.LegacyExecutor caller)
        {
            if(!hasStringLevel)
                return this;
            stringLevel = caller.ReplacePPV(stringLevel);
            id = caller.ReplacePPV(id);
            level = int.Parse(stringLevel);
            hasStringLevel = false;
            return this;
        }

        public string id;
        public int level;

        public EnchantmentObject(string id, int level = 1)
        {
            this.id = id;
            this.level = level;

            stringLevel = null;
            hasStringLevel = false;

        }
        public EnchantmentObject(string id, string level)
        {
            this.id = id;
            this.level = 0;

            stringLevel = level;
            hasStringLevel = true;
        }

        public override int GetHashCode()
        {
            if (hasStringLevel)
                return id.GetHashCode() ^ stringLevel.GetHashCode();
            return id.GetHashCode() + level;
        }
    }
}
