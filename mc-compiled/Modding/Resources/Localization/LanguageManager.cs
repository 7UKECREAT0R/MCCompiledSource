using mc_compiled.MCC.Compiler;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mc_compiled.Modding.Resources.Localization
{
    /// <summary>
    /// Doubles as the language manager and the languages.json file.
    /// </summary>
    internal class LanguageManager : IAddonFile
    {
        /// <summary>
        /// PPV that indicates if identical lang entries should be merged into a single key.
        /// </summary>
        public const string MERGE_PPV = "_lang_merge";

        internal Executor executor;
        internal HashSet<LocaleDefinition> locales;

        internal LanguageManager(Executor executor)
        {
            this.executor = executor;
            this.locales = new HashSet<LocaleDefinition>();
        }
        
        /// <summary>
        /// Defines a locale and returns its definition based on the existing files. Creates the necessary files if they haven't been yet.
        /// </summary>
        /// <returns></returns>
        internal LocaleDefinition DefineLocale(string locale)
        {
            LocaleDefinition lookupDummy = new LocaleDefinition(locale, null);

            if(this.locales.TryGetValue(lookupDummy, out LocaleDefinition actual))
                return actual;

            string fileName = locale + ".lang";
            string path = executor.project.GetOutputFileLocationFull(OutputLocation.r_TEXTS, fileName);

            Lang lang;

            // fetch lang file if it exists. otherwise, create one.
            if (System.IO.File.Exists(path))
            {
                string file = executor.LoadFileString(path);
                lang = Lang.Parse(locale, file);
            }
            else
                lang = new Lang(locale);

            executor.AddExtraFile(lang);
            LocaleDefinition localeDefinition = lang.GetOrCreateLocaleDefinition();
            this.locales.Add(localeDefinition);
            return localeDefinition;
        }

        public string CommandReference => throw new NotImplementedException();
        public string GetExtendedDirectory() => null;

        public byte[] GetOutputData()
        {
            JArray array = new JArray(from l in locales select l.locale);
            string json = array.ToString(Newtonsoft.Json.Formatting.Indented);
            return Encoding.UTF8.GetBytes(json);
        }
        public string GetOutputFile() => "languages.json";
        public OutputLocation GetOutputLocation() => OutputLocation.r_TEXTS;
    }
}
