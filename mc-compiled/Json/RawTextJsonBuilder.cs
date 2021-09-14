using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace mc_compiled.Json
{
    /// <summary>
    /// Utility for building rawtext json for minecraft.
    /// </summary>
    class RawTextJsonBuilder
    {
        List<JSONRawTerm> terms;

        string copiedString = null;

        public RawTextJsonBuilder()
        {
            terms = new List<JSONRawTerm>();
        }
        public void ClearTerms()
        {
            terms.Clear();
        }
        public void AddTerm(JSONRawTerm term)
        {
            terms.Add(term);
        }
        public string BuildPreviewString()
        {
            return string.Join(" ", from term in terms select term.PreviewString());
        }
        public string BuildString()
        {
            string inner = string.Join(", ", from term in terms select term.GetString());
            return $@"{{""rawtext"":[{inner}]}}";
        }
        public void ConsoleInterface()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(
@"┌─ MCC ── JSON RawText Builder ───────>
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
                    copiedString = BuildString();
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
└─────────────────────────────────┴───────────────────────┘");
                    Console.Write("> ");
                    string text = Console.ReadLine();
                    string comp = text.ToUpper();
                    if (comp.StartsWith("TEXT"))
                    {
                        terms.Add(new JSONText(text.Substring(5)));
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
