﻿using System.Collections.Generic;
using mc_compiled.MCC.Functions.Types;
// ReSharper disable PossibleNullReferenceException

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal class FunctionGetValueByName : CompiletimeFunction
    {
        public FunctionGetValueByName() : base("getValue", "retrieveValueByName", "value", "Gets and returns the value with the given name. Works same as specifying the identifier of a value.")
        {
            AddParameter(
                new CompiletimeFunctionParameter<TokenStringLiteral>("name")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Token[] allParameters, Executor executor,
            Statement statement)
        {
            string name = (((CompiletimeFunctionParameter)this.Parameters[0]).CurrentValue as TokenStringLiteral).text;
            if (!executor.scoreboard.TryGetByUserFacingName(name, out ScoreboardValue value))
                throw new StatementException(statement, $"Couldn't find a value with the name '{name}'.");
            
            return new TokenIdentifierValue(name, value, statement.Lines[0]);
        }
    }
}