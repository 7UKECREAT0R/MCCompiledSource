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
        string selector, objective, forceValue;
        public JSONScore(string selector, string objective, string forceValue = null)
        {
            this.selector = selector;
            this.objective = objective;
            this.forceValue = forceValue;
        }
        public override string GetString()
        {
            if (forceValue == null)
                return $@"{{""score"": {{""name"":""{selector}"", ""objective"": ""{objective}""}}}}";
            else
                return $@"{{""score"": {{""name"":""{selector}"", ""objective"": ""{objective}"", ""value"": ""{forceValue}""}}}}";
        }
        public override string PreviewString()
        {
            if (forceValue == null)
                return "[SCORE " + objective + " OF " + selector + ']';
            else
                return "[SCORE " + objective + " OF " + selector + ", FORCE: " + forceValue + ']';
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
