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
        /// <param name="d"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static double FixPoint(this double d, int digits)
        {
            double pow = Math.Pow(10, digits);
            d *= pow;
            d = Math.Floor(d);
            return d / pow;
        }
        /// <summary>
        /// Remove the decimal point from this double, to a certain precision.
        /// <code>12.34567.ToFixedInt(3) = 12345</code>
        /// </summary>
        /// <param name="d"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static int ToFixedInt(this double d, int digits)
        {
            d *= Math.Pow(10, digits);
            return (int)(Math.Floor(d));
        }
        /// <summary>
        /// Get the level of precision needed to represent this double.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int GetPrecision(this double d)
        {
            string str = d.ToString();
            return str.Length - str.IndexOf('.') - 1;
        }
    }
}
