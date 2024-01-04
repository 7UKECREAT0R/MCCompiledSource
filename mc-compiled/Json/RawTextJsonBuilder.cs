using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace mc_compiled.Json
{
    /// <summary>
    /// Utility for building rawtext json for minecraft.
    /// </summary>
    public class RawTextJsonBuilder
    {
        List<JSONRawTerm> terms;

        string copiedString = null;

        public RawTextJsonBuilder()
        {
            terms = new List<JSONRawTerm>();
        }
        public RawTextJsonBuilder(RawTextJsonBuilder copy)
        {
            terms = new List<JSONRawTerm>();

            if (copy != null)
                terms.AddRange(copy.terms);
        }
        public RawTextJsonBuilder ClearTerms()
        {
            terms.Clear();
            return this;
        }
        public RawTextJsonBuilder AddTerm(JSONRawTerm term)
        {
            terms.Add(term);
            return this;
        }
        public RawTextJsonBuilder AddTerms(IEnumerable<JSONRawTerm> terms)
        {
            this.terms.AddRange(terms);
            return this;
        }
        public RawTextJsonBuilder AddTerms(params JSONRawTerm[] terms)
        {
            this.terms.AddRange(terms);
            return this;
        }
        public string BuildPreviewString()
        {
            return string.Join(" ", from term in terms select term.PreviewString());
        }

        /// <summary>
        /// Builds the terms inside this builder into the completed JSON.
        /// </summary>
        /// <returns></returns>
        public JObject Build()
        {
            JObject[] objects = (from term in terms select term.Build()).ToArray();

            return new JObject()
            {
                ["rawtext"] = new JArray(objects)
            };
        }
        /// <summary>
        /// Returns <see cref="Build"/>, but as a minimized JSON string.
        /// </summary>
        /// <returns></returns>
        public string BuildString()
        {
            return Build().ToString(Newtonsoft.Json.Formatting.None);
        }

        public void ConsoleInterface()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(
@"┌─ JSON RawText Builder ──────────┐
│ {0}
├─────────────────────────────────┐
│ [A] Add Item                    │
│ [R] Remove Item                 │
│                                 │
│ [Q] Quit                        │
│ [C] Copy Text                   │
└─────────────────────────────────┘", BuildPreviewString());

                if(copiedString != null)
                {
                    Console.WriteLine("Copied: " + copiedString);
                    copiedString = null;
                }

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Q)
                    return;
                if(key.Key == ConsoleKey.C)
                {
                    copiedString = Build().ToString(Newtonsoft.Json.Formatting.None);
                    Clipboard.SetText(copiedString);
                    continue;
                }

                if (key.Key == ConsoleKey.A)
                {
                    Console.WriteLine(
@"┌─────────────────────────────────┬───────────────────────┐
│ Enter the item you want to add. │      Description      │
│                                 │                       │
│ TEXT <text>                     │ Regular text.         │
│ SCORE <objective> <selector>    │ A scoreboard value.   │
│ SELECTOR <selector>             │ The name of a target. │
│ TRANSLATE <key>                 │ A translation key.    │
└─────────────────────────────────┴───────────────────────┘");
                    Console.Write("> ");
                    string text = Console.ReadLine();
                    string comp = text.ToUpper();
                    if (comp.StartsWith("TEXT"))
                    {
                        string str = text.Substring(5);
                        str = MCC.Definitions.GLOBAL_DEFS.ReplaceDefinitions(str);
                        terms.Add(new JSONText(str));
                        continue;
                    }
                    else if (comp.StartsWith("SCORE"))
                    {
                        if (text.Length < 7)
                            continue;
                        string epic = text.Substring(6);
                        int index = epic.IndexOf(' ');
                        string objective = epic.Substring(0, index);
                        string selector = epic.Substring(index + 1);
                        terms.Add(new JSONScore(selector, objective));
                        continue;
                    }
                    else if (comp.StartsWith("SELECTOR"))
                    {
                        terms.Add(new JSONSelector(text.Substring(9)));
                        continue;
                    }
                    else if (comp.StartsWith("TRANSLATE"))
                    {
                        terms.Add(new JSONTranslate(text.Substring(10)));
                        continue;
                    }
                }
                if (key.Key == ConsoleKey.R)
                {
                    int count = terms.Count;
                    if (count < 1)
                        continue;
                    terms.RemoveAt(count - 1);
                    continue;
                }
            }
        }
    }
}
