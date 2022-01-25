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
        /// Limit this double to a specific number of digits.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static float FixPoint(this float f, int digits)
        {
            float pow = (float)Math.Pow(10, digits);
            f *= pow;
            f = (float)Math.Floor(f);
            return f / pow;
        }
        /// <summary>
        /// Remove the decimal point from this float, to a certain precision.
        /// <code>12.34567.ToFixedInt(3) = 12345</code>
        /// </summary>
        /// <param name="f"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static int ToFixedInt(this float f, int digits)
        {
            f *= (float)Math.Pow(10, digits);
            return (int)(Math.Floor(f));
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
