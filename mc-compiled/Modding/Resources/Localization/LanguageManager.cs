using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using mc_compiled.MCC.Compiler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        private readonly bool isForBehaviorPack;
        internal Executor executor;
        internal HashSet<LocaleDefinition> locales;

        internal LanguageManager(Executor executor, bool isForBehaviorPack)
        {
            this.executor = executor;
            this.locales = [];
            this.isForBehaviorPack = isForBehaviorPack;
        }
        
        /// <summary>
        /// Defines a locale and returns its definition based on the existing files. Creates the necessary files if they haven't been yet.
        /// </summary>
        /// <returns></returns>
        internal LocaleDefinition DefineLocale(string locale)
        {
            var lookupDummy = new LocaleDefinition(locale, null);

            if(this.locales.TryGetValue(lookupDummy, out LocaleDefinition actual))
                return actual;

            string fileName = locale + ".lang";
            string path = this.executor.project.GetOutputFileLocationFull(OutputLocation.r_TEXTS, fileName);

            Lang lang;

            // fetch lang file if it exists. otherwise, create one.
            if (File.Exists(path))
            {
                string file = this.executor.LoadFileString(path);
                lang = Lang.Parse(locale, file, this.isForBehaviorPack);
            }
            else
                lang = new Lang(locale, this.isForBehaviorPack);

            this.executor.AddExtraFile(lang);
            LocaleDefinition localeDefinition = lang.GetOrCreateLocaleDefinition();
            this.locales.Add(localeDefinition);
            return localeDefinition;
        }

        public string CommandReference => throw new NotImplementedException();
        public string GetExtendedDirectory() => null;

        public byte[] GetOutputData()
        {
            var array = new JArray(from l in this.locales select l.locale);
            string json = array.ToString(Formatting.Indented);
            return Encoding.UTF8.GetBytes(json);
        }
        public string GetOutputFile() => "languages.json";
        public OutputLocation GetOutputLocation() => this.isForBehaviorPack ?
            OutputLocation.b_TEXTS : OutputLocation.r_TEXTS;
    }
}
