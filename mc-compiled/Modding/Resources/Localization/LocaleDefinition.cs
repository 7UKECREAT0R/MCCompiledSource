using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Resources.Localization
{
    /// <summary>
    /// Defines a locale, e.g., 'en_US', and pairs it with a lang file.
    /// </summary>
    internal class LocaleDefinition
    {
        internal readonly string locale;
        internal Lang file;

        internal LocaleDefinition(string locale, Lang file)
        {
            this.locale = locale;
            this.file = file;
        }

        public override int GetHashCode()
        {
            return locale.GetHashCode();
        }
    }
}
