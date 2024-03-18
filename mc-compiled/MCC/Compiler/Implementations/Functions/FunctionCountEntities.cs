using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal class FunctionCountEntities : GenerativeFunction
    {
        public FunctionCountEntities() : base(
            "countEntities",
            "countEntities",
            "int",
            "Returns the number of entities that match the given selector.")
        {
            AddParameter(
                new CompiletimeFunctionParameter<TokenSelectorLiteral>("selector")
            );
        }
        public override void GenerateCode(CommandFile output, int uniqueIdentifier, Executor executor,
            Statement statement,
            out ScoreboardValue resultValue)
        {
            Selector selector =
                ((TokenSelectorLiteral) ((CompiletimeFunctionParameter) this.Parameters[0]).CurrentValue).selector;

            // create an objective to hold the counter
            string countName = CreateUniqueTempValueName(uniqueIdentifier);
            ScoreboardValue count = new ScoreboardValue(countName, true, Typedef.INTEGER, executor.scoreboard);
            executor.scoreboard.Add(count);
            executor.AddCommandsInit(count.CommandsDefine());

            // set out parameter
            resultValue = count;

            // set count = 0
            output.Add(Command.ScoreboardSet(count, 0));

            // for @selector: count += 1
            output.Add(Command.Execute().As(selector).Run(Command.ScoreboardAdd(count, 1)));
        }
    }
}