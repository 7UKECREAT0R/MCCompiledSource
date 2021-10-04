using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    public enum FunctionSecurity
    {
        NONE,
        SECURE,
        HARSH
    }
    public struct FunctionDefinition
    {
        public string name;
        public string[] args;
        public FunctionSecurity security;

        public bool isTick;
        public bool isDelay;
        public bool isNamespaced;

        public int delayTicks;
        public string theNamespace;

        /// <summary>
        /// Get the fully qualified file-system name of this function including its namespace, if specified.
        /// </summary>
        public string FullName
        {
            get
            {
                if (isNamespaced)
                    return theNamespace + System.IO.Path.DirectorySeparatorChar + name;
                else return name;
            }
        }
        
        public static FunctionDefinition Parse(string def)
        {
            FunctionDefinition func = new FunctionDefinition();

            // namespace(utils) delay(100) purge amount
            List<string> args = new List<string>();
            string[] words = def.Split(' ');
            bool setName = false;
            foreach(string word in words)
            {
                string upper = word.ToUpper();
                bool modifier = false;
                if(upper.Equals("_TICK"))
                {
                    modifier = true;
                    func.isTick = true;
                } else if(upper.StartsWith("DELAY("))
                {
                    try
                    {
                        string delayTime = word.Substring(6);
                        delayTime = delayTime.Substring(0, delayTime.Length - 1);
                        modifier = true;
                        func.isDelay = true;
                        func.delayTicks = int.Parse(delayTime);
                    } catch(FormatException)
                    {
                        throw new Exception($"Invalid delay specified \"{upper}\"");
                    }
                } else if(upper.StartsWith("NAMESPACE("))
                {
                    string delayTime = word.Substring(10);
                    func.theNamespace = delayTime.Substring(0, delayTime.Length - 1);
                    modifier = true;
                    func.isNamespaced = true;
                } else if(upper.Equals("SECURE"))
                {
                    modifier = true;
                    func.security = FunctionSecurity.SECURE;
                } else if (upper.Equals("HARSH"))
                {
                    modifier = true;
                    func.security = FunctionSecurity.HARSH;
                }

                if (!modifier && !setName)
                {
                    func.name = word;
                    setName = true;
                    continue;
                } else if(!modifier)
                    args.Add(word);
            }

            func.args = args.ToArray();
            return func;
        }
    }
}
