using System.Collections.Generic;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Compiler.Implementations.Functions;

internal class FunctionParseInteger : CompiletimeFunction
{
    // TODO document in the wiki

    internal FunctionParseInteger() : base("parseInt", "parseString_int", "int",
        "Parses an integer from a string. Will throw an error if the string is not an integer.")
    {
        AddParameter(new CompiletimeFunctionParameter<TokenStringLiteral>("input"));
    }

    public override Token CallFunction(List<string> commandBuffer,
        Token[] allParameters,
        Executor executor,
        Statement statement)
    {
        string input =
            ((CompiletimeFunctionParameter<TokenStringLiteral>) this.Parameters[0]).CurrentValue as TokenStringLiteral;

        if (int.TryParse(input, out int result))
            return new TokenIntegerLiteral(result, IntMultiplier.none, statement.Lines[0]);

        throw new StatementException(statement, $"Failed to parse input '{input}' as an integer.");
    }
}

internal class FunctionParseNumber : CompiletimeFunction
{
    // TODO document in the wiki

    internal FunctionParseNumber() : base("parseNumber", "parseString_decimal", "number",
        "Parses a number from a string. Will throw an error if the string is not a number.")
    {
        AddParameter(new CompiletimeFunctionParameter<TokenStringLiteral>("input"));
    }

    public override Token CallFunction(List<string> commandBuffer,
        Token[] allParameters,
        Executor executor,
        Statement statement)
    {
        string input =
            ((CompiletimeFunctionParameter<TokenStringLiteral>) this.Parameters[0]).CurrentValue as TokenStringLiteral;

        if (decimal.TryParse(input, out decimal result))
            return new TokenDecimalLiteral(result, statement.Lines[0]);

        throw new StatementException(statement, $"Failed to parse input '{input}' as a number.");
    }
}

internal class FunctionParseSelector : CompiletimeFunction
{
    // TODO document in the wiki

    internal FunctionParseSelector() : base("parseSelector", "parseString_selector", "selector",
        "Parses a selector from a string. Will throw an error if the string is not a selector.")
    {
        AddParameter(new CompiletimeFunctionParameter<TokenStringLiteral>("input"));
    }

    public override Token CallFunction(List<string> commandBuffer,
        Token[] allParameters,
        Executor executor,
        Statement statement)
    {
        string input =
            ((CompiletimeFunctionParameter<TokenStringLiteral>) this.Parameters[0]).CurrentValue as TokenStringLiteral;

        if (!input.StartsWith("@"))
            throw new StatementException(statement, $"Failed to parse input '{input}' as a selector.");

        Selector selector = Selector.Parse(input);
        if (selector == null)
            throw new StatementException(statement, $"Failed to parse input '{input}' as a selector.");

        return new TokenSelectorLiteral(selector, statement.Lines[0]);
    }
}

internal class FunctionParseBoolean : CompiletimeFunction
{
    // TODO document in the wiki

    internal FunctionParseBoolean() : base("parseBool", "parseString_bool", "bool",
        "Parses a boolean from a string. Will throw an error if the string is not a boolean.")
    {
        AddParameter(new CompiletimeFunctionParameter<TokenStringLiteral>("input"));
    }

    public override Token CallFunction(List<string> commandBuffer,
        Token[] allParameters,
        Executor executor,
        Statement statement)
    {
        string input =
            ((CompiletimeFunctionParameter<TokenStringLiteral>) this.Parameters[0]).CurrentValue as TokenStringLiteral;

        if (bool.TryParse(input, out bool result))
            return new TokenBooleanLiteral(result, statement.Lines[0]);

        throw new StatementException(statement, $"Failed to parse input '{input}' as a boolean.");
    }
}