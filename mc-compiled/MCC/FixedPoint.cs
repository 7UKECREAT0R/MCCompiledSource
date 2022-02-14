using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// Provides ways of doing estimated fixed point math.
    /// </summary>
    public static class FixedPoint
    {
        /// <summary>
        /// Convert this float to a fixed point number.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static int ToFixedPoint(this float f, int precision)
        {
            return (int)Math.Floor(f * (float)Math.Pow(10, precision));
        }
        /// <summary>
        /// Get the level of precision needed to represent this double.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetPrecision(this float d)
        {
            string str = d.ToString();
            return str.Length - str.IndexOf('.') - 1;
        }
    }
}
