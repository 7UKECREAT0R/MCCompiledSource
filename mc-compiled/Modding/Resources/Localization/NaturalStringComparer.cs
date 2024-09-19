using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace mc_compiled.Modding.Resources.Localization
{
    public class NaturalStringComparer : IComparer<string>
    {
        private static readonly Regex _regex = new Regex(@"(\d+|\D+)", RegexOptions.Compiled);

        public int Compare(string x, string y)
        {
            if (x == null || y == null)
                return string.Compare(x, y, StringComparison.Ordinal);

            string[] xParts = _regex.Matches(x).Cast<Match>().Select(m => m.Value).ToArray();
            string[] yParts = _regex.Matches(y).Cast<Match>().Select(m => m.Value).ToArray();

            for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
            {
                bool xPartIsNumber = int.TryParse(xParts[i], out int xPartNumber);
                bool yPartIsNumber = int.TryParse(yParts[i], out int yPartNumber);

                if (xPartIsNumber && yPartIsNumber)
                {
                    int comparison = xPartNumber.CompareTo(yPartNumber);
                    if (comparison != 0)
                        return comparison;
                }
                else
                {
                    int comparison = string.Compare(xParts[i], yParts[i], StringComparison.Ordinal);
                    if (comparison != 0)
                        return comparison;
                }
            }

            return xParts.Length.CompareTo(yParts.Length);
        }
    }
}