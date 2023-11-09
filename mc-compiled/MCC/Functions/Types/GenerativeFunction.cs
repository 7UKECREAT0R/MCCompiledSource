using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// A function which generates runtime code as it is called.
    /// </summary>
    internal class GenerativeFunction : Function
    {
        public override string Keyword => throw new NotImplementedException();

        public override string Returns => throw new NotImplementedException();

        public override string Documentation => throw new NotImplementedException();

        public override string[] Aliases => throw new NotImplementedException();

        public override FunctionParameter[] Parameters => throw new NotImplementedException();

        public override int ParameterCount => throw new NotImplementedException();

        public override int Importance => throw new NotImplementedException();

        public override bool ImplicitCall => throw new NotImplementedException();

        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            throw new NotImplementedException();
        }
    }
}
