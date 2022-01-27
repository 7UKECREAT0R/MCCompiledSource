using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Native
{
    public struct LegacyEnchantmentObject
    {
        // If level needs to be parsed later.
        public string stringLevel;
        public bool hasStringLevel;

        public LegacyEnchantmentObject Resolve(MCC.LegacyExecutor caller)
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

        public LegacyEnchantmentObject(string id, int level = 1)
        {
            this.id = id;
            this.level = level;

            stringLevel = null;
            hasStringLevel = false;

        }
        public LegacyEnchantmentObject(string id, string level)
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
