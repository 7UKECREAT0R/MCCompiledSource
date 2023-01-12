using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Functions
{
    internal class FunctionComparator : Comparer<Function>
    {
        internal static readonly FunctionComparator Instance = new FunctionComparator();

        public override int Compare(Function x, Function y)
        {
            int a = x.Importance;
            int b = y.Importance;
            return b - a;
        }
    }
}
