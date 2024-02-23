namespace mc_compiled.MCC
{
    /// <summary>
    /// Provides ways of doing estimated fixed point math.
    /// </summary>
    public static class FixedPoint
    {
        /// <summary>
        /// Convert this decimal to a fixed point number with a set precision.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="precision">The target precision.</param>
        /// <returns></returns>
        public static int ToFixedPoint(this decimal number, byte precision)
        {
            long powerOfTen = 1;
            for (int i = 0; i < precision; i++)
                powerOfTen *= 10;
            
            return (int)decimal.Floor(number * powerOfTen);
        }
        
        /// <summary>
        /// Raises this integer to a power of 10 that represents this fixed point number.
        /// </summary>
        /// <param name="integer"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static int ToFixedPoint(this int integer, byte precision)
        {
            long powerOfTen = 1;
            for (int i = 0; i < precision; i++)
                powerOfTen *= 10;
            
            return integer * (int)powerOfTen;
        }
        
        /// <summary>
        /// Get the level of precision needed to represent this decimal value.<br />
        /// <br />
        /// Equivalent to:
        ///     <code>decimal.GetBits(number)[3] >> 16</code>
        /// </summary>
        /// <param name="number">The number to get the precision of.</param>
        /// <returns></returns>
        public static byte GetPrecision(this decimal number) => (byte)(decimal.GetBits(number)[3] >> 16);
    }
}
