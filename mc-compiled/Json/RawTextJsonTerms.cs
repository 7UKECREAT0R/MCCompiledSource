using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Json
{
    /// <summary>
    /// Term in a JSON rawtext sequence.
    /// </summary>
    public abstract class JSONRawTerm
    {
        public abstract string GetString();
        public abstract string PreviewString();
    }

    public class JSONText : JSONRawTerm
    {
        string text;
        public JSONText(string text)
        {
            this.text = text;
        }
        public override string GetString()
        {
            return $@"{{""text"": ""{text}""}}";
        }
        public override string PreviewString()
        {
            return '[' + text + ']';
        }
    }
    public class JSONScore : JSONRawTerm
    {
        string selector, objective;
        public JSONScore(string selector, string objective)
        {
            this.selector = selector;
            this.objective = objective;
        }
        public override string GetString()
        {
            return $@"{{""score"": {{""name"":""{selector}"", ""objective"": ""{objective}""}}}}";
        }
        public override string PreviewString()
        {
            return "[SCORE " + objective + " OF " + selector + ']';
        }
    }
    public class JSONSelector : JSONRawTerm
    {
        string selector;
        public JSONSelector(string selector)
        {
            this.selector = selector;
        }
        public override string GetString()
        {
            return $@"{{""selector"": ""{selector}""}}";
        }
        public override string PreviewString()
        {
            return '[' + selector + ']';
        }
    }
}
