﻿using mc_compiled.Commands;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.Compiler.TypeSystem.Implementations;
using mc_compiled.MCC.Functions.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal class FunctionRoundCompiletime : CompiletimeFunction
    {
        public FunctionRoundCompiletime() : base("round", "compiletimeRound", "decimal ?", "Rounds the given value to the nearest integer, or does nothing if it is already an integer.")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float number = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            float result = (float)Math.Round(number, MidpointRounding.AwayFromZero);

            return new TokenDecimalLiteral(result, statement.Lines[0]);
        }
    }
    internal class FunctionRoundRuntime : GenerativeFunction
    {
        public FunctionRoundRuntime() : base("round", "runtimeRound", "decimal ?", "Rounds the given value to the nearest integer, or does nothing if it is already an integer.")
        {
            this.AddParameter(
                new RuntimeFunctionParameterDynamic(this, "number", "runtime_round_num").WithAcceptedTypes(Typedef.FIXED_DECIMAL)
            );
        }
        public override void GenerateCode(CommandFile output, int uniqueIdentifier, Executor executor, Statement statement, out ScoreboardValue resultValue)
        {
            ScoreboardValue input = (this.parameters[0] as RuntimeFunctionParameterDynamic).RuntimeDestination;
            int precision = ((FixedDecimalData)input.data).precision;
            int coeff = (int)Math.Pow(10, precision);

            string temp1Name = this.CreateUniqueTempValueName(uniqueIdentifier);
            string temp2Name = this.CreateUniqueTempValueName(uniqueIdentifier);
            ScoreboardValue temp1 = new ScoreboardValue(temp1Name, true, Typedef.INTEGER, executor.scoreboard);
            ScoreboardValue temp2 = new ScoreboardValue(temp2Name, true, Typedef.INTEGER, executor.scoreboard);
            executor.scoreboard.Add(temp1);
            executor.scoreboard.Add(temp2);
            executor.AddCommandsInit(temp1.CommandsDefine());
            executor.AddCommandsInit(temp2.CommandsDefine());

            this.TryReturnValue(input, executor, statement, out resultValue);

            // do everything in the return value
            output.Add(Command.ScoreboardSet(temp1, coeff));
            output.Add(Command.ScoreboardOpSet(temp2, input));
            output.Add(Command.ScoreboardOpMod(temp2, temp1));

            // needs branch for logic
            CommandFile roundUp = new CommandFile(output.IsInUse, output.name + "roundUp", output.folder);
            executor.AddExtraFile(roundUp);
            if (Program.DECORATE)
                roundUp.AddTrace(output, statement.Lines[0]);
            roundUp.Add(Command.ScoreboardOpSub(resultValue, temp2));
            roundUp.Add(Command.ScoreboardAdd(resultValue, coeff));

            // if mod is 500 or more, round up
            output.Add(Command.Execute().IfScore(temp2, new Range(500, null)).Run(Command.Function(roundUp)));
            // else, just subtract mod
            output.Add(Command.Execute().IfScore(temp2, new Range(null, 499)).Run(Command.ScoreboardOpSub(resultValue, temp2)));
        }
    }

    internal class FunctionFloorCompiletime : CompiletimeFunction
    {
        public FunctionFloorCompiletime() : base("floor", "compiletimeFloor", "decimal ?", "Rounds down the given value to the nearest integer.")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float number = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            float result = (float)Math.Floor(number);

            return new TokenDecimalLiteral(result, statement.Lines[0]);
        }
    }
    internal class FunctionFloorRuntime : GenerativeFunction
    {
        public FunctionFloorRuntime() : base("floor", "runtimeFloor", "decimal ?", "Rounds down the given value to the nearest integer.")
        {
            this.AddParameter(
                new RuntimeFunctionParameterDynamic(this, "number", "runtime_floor_num").WithAcceptedTypes(Typedef.FIXED_DECIMAL)
            );
        }
        public override void GenerateCode(CommandFile output, int uniqueIdentifier, Executor executor, Statement statement, out ScoreboardValue resultValue)
        {
            ScoreboardValue input = (this.parameters[0] as RuntimeFunctionParameterDynamic).RuntimeDestination;
            int precision = ((FixedDecimalData)input.data).precision;
            int coeff = (int)Math.Pow(10, precision);

            string temp1Name = this.CreateUniqueTempValueName(uniqueIdentifier);
            string temp2Name = this.CreateUniqueTempValueName(uniqueIdentifier);
            ScoreboardValue temp1 = new ScoreboardValue(temp1Name, true, Typedef.INTEGER, executor.scoreboard);
            ScoreboardValue temp2 = new ScoreboardValue(temp2Name, true, Typedef.INTEGER, executor.scoreboard);
            executor.scoreboard.Add(temp1);
            executor.scoreboard.Add(temp2);
            executor.AddCommandsInit(temp1.CommandsDefine());
            executor.AddCommandsInit(temp2.CommandsDefine());

            this.TryReturnValue(input, executor, statement, out resultValue);

            // do everything in the return value
            output.Add(Command.ScoreboardSet(temp1, coeff));
            output.Add(Command.ScoreboardOpSet(temp2, input));
            output.Add(Command.ScoreboardOpMod(temp2, temp1));
            output.Add(Command.ScoreboardOpSub(resultValue, temp2));
        }
    }

    internal class FunctionCeilingCompiletime : CompiletimeFunction
    {
        public FunctionCeilingCompiletime() : base("ceiling", "compiletimeCeiling", "decimal ?", "Rounds up the given value to the nearest integer.")
        {
            this.AddParameter(
                new CompiletimeFunctionParameter<TokenNumberLiteral>("number")
            );
        }
        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            float number = ((this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenNumberLiteral).GetNumber();
            float result = (float)Math.Ceiling(number);

            return new TokenDecimalLiteral(result, statement.Lines[0]);
        }
    }
    internal class FunctionCeilingRuntime : GenerativeFunction
    {
        public FunctionCeilingRuntime() : base("ceiling", "runtimeCeiling", "decimal ?", "Rounds up the given value to the nearest integer.")
        {
            this.AddParameter(
                new RuntimeFunctionParameterDynamic(this, "number", "runtime_floor_num").WithAcceptedTypes(Typedef.FIXED_DECIMAL)
            );
        }
        public override void GenerateCode(CommandFile output, int uniqueIdentifier, Executor executor, Statement statement, out ScoreboardValue resultValue)
        {
            ScoreboardValue input = (this.parameters[0] as RuntimeFunctionParameterDynamic).RuntimeDestination;
            int precision = ((FixedDecimalData)input.data).precision;
            int coeff = (int)Math.Pow(10, precision);

            string temp1Name = this.CreateUniqueTempValueName(uniqueIdentifier);
            string temp2Name = this.CreateUniqueTempValueName(uniqueIdentifier);
            ScoreboardValue temp1 = new ScoreboardValue(temp1Name, true, Typedef.INTEGER, executor.scoreboard);
            ScoreboardValue temp2 = new ScoreboardValue(temp2Name, true, Typedef.INTEGER, executor.scoreboard);
            executor.scoreboard.Add(temp1);
            executor.scoreboard.Add(temp2);
            executor.AddCommandsInit(temp1.CommandsDefine());
            executor.AddCommandsInit(temp2.CommandsDefine());

            this.TryReturnValue(input, executor, statement, out resultValue);

            // do everything in the return value
            output.Add(Command.ScoreboardSet(temp1, coeff));
            output.Add(Command.ScoreboardOpSet(temp2, input));
            output.Add(Command.ScoreboardOpMod(temp2, temp1));

            // needs branch for logic
            CommandFile roundUp = new CommandFile(output.IsInUse, output.name + "roundUp", output.folder);
            executor.AddExtraFile(roundUp);
            if (Program.DECORATE)
                roundUp.AddTrace(output, statement.Lines[0]);
            roundUp.Add(Command.ScoreboardOpSub(temp1, temp2));
            roundUp.Add(Command.ScoreboardOpAdd(resultValue, temp1));

            // if mod is 1 or more, round up
            output.Add(Command.Execute().IfScore(temp2, new Range(1, null)).Run(Command.Function(roundUp)));
        }
    }
}