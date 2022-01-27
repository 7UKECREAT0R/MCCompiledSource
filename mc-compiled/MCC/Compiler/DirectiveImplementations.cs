﻿using mc_compiled.Commands;
using mc_compiled.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    public static class DirectiveImplementations
    {
        public static void _CheckElse(Executor executor, Statement tokens, bool runIt)
        {
            if (executor.NextIs<StatementDirective>())
            {
                StatementDirective directive = executor.Peek<StatementDirective>();
                if (directive.directive.call != _else)
                    return;

                executor.Next();

                if (runIt)
                    return;

                // skip over
                if (executor.NextIs<StatementOpenBlock>())
                {
                    StatementOpenBlock block = executor.Next<StatementOpenBlock>();
                    for (int i = 0; i < block.statementsInside; i++)
                        executor.Next(); // skip this block
                }
                else
                    executor.Next(); // skip the next statement
            }
        }

        public static void _var(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object value = tokens.Next<IObjectable>().GetObject();

            executor.SetPPV(varName, value);
        }
        public static void _inc(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value++;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _dec(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value--;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _add(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object other = tokens.Next<IObjectable>().GetObject();

            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value += other;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _sub(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object other = tokens.Next<IObjectable>().GetObject();

            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value -= other;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _mul(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object other = tokens.Next<IObjectable>().GetObject();

            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value *= other;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _div(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object other = tokens.Next<IObjectable>().GetObject();

            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value /= other;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _mod(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object other = tokens.Next<IObjectable>().GetObject();

            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value %= other;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _swap(Executor executor, Statement tokens)
        {
            string aName = tokens.Next<TokenIdentifier>().word;
            string bName = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(aName, out dynamic a))
            {
                if (executor.TryGetPPV(bName, out dynamic b))
                {
                    executor.SetPPV(aName, b);
                    executor.SetPPV(bName, a);
                }
                else
                    throw new StatementException(tokens, "Preprocessor variable '" + bName + "' does not exist.");
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + aName + "' does not exist.");
        }
        public static void _if(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            TokenCompare compare = tokens.Next<TokenCompare>();
            object other = tokens.Next<IObjectable>().GetObject();

            // if the next block/statement should be run
            bool run = false;

            if(executor.TryGetPPV(varName, out dynamic a))
            {
                switch (compare.GetCompareType())
                {
                    case TokenCompare.Type.EQUAL:
                        run = a == other;
                        break;
                    case TokenCompare.Type.NOT_EQUAL:
                        run = a != other;
                        break;
                    case TokenCompare.Type.LESS_THAN:
                        run = a < other;
                        break;
                    case TokenCompare.Type.LESS_OR_EQUAL:
                        run = a <= other;
                        break;
                    case TokenCompare.Type.GREATER_THAN:
                        run = a > other;
                        break;
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                        run = a >= other;
                        break;
                    default:
                        run = false;
                        break;
                }
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");

            if (!executor.HasNext)
                throw new StatementException(tokens, "End of file after $if statement.");

            executor.SetLastIfResult(run);

            if (executor.NextIs<StatementOpenBlock>())
            {
                StatementOpenBlock block = executor.Peek<StatementOpenBlock>();
                block.shouldRun = run;
                return;
            }
            else if(!run)
                executor.Next(); // skip the next statement
        }
        public static void _else(Executor executor, Statement tokens)
        {
            bool run = !executor.GetLastIfResult();

            if (executor.NextIs<StatementOpenBlock>())
            {
                StatementOpenBlock block = executor.Peek<StatementOpenBlock>();
                block.shouldRun = run;
                return;
            }
            else if (!run)
                executor.Next(); // skip the next statement
        }
        public static void _repeat(Executor executor, Statement tokens)
        {
            int amount = tokens.Next<TokenIntegerLiteral>();
            string tracker = null;

            if (tokens.HasNext && tokens.NextIs<TokenIdentifier>())
                tracker = tokens.Next<TokenIdentifier>().word;

            int skipAfter = 0;
            Statement[] statements;

            if (executor.NextIs<StatementOpenBlock>())
            {
                StatementOpenBlock block = executor.Next<StatementOpenBlock>();
                skipAfter = block.statementsInside;
                statements = executor.Peek(skipAfter);
                executor.Next<StatementCloseBlock>(); // skip that
            } else
            {
                skipAfter = 0;
                statements = new[] { executor.Next() };
            }

            for (int i = 0; i < amount; i++)
            {
                if (tracker != null)
                    executor.SetPPV(tracker, i);
                executor.ExecuteSubsection(statements);
            }

            for (int i = 0; i < skipAfter; i++)
                executor.Next();
        }
        public static void _log(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            Console.WriteLine("[LOG] {0}", str);
        }
        public static void _macro(Executor executor, Statement tokens)
        {
            if (executor.NextIs<StatementOpenBlock>())
                _macrodefine(executor, tokens);
            else
                _macrocall(executor, tokens);
        }
        public static void _macrodefine(Executor executor, Statement tokens)
        {
            string macroName = tokens.Next<TokenIdentifier>().word;

            List<string> args = new List<string>();
            while (tokens.HasNext && tokens.NextIs<TokenIdentifier>())
                args.Add(tokens.Next<TokenIdentifier>().word);

            StatementOpenBlock block = executor.Next<StatementOpenBlock>();
            int count = block.statementsInside;
            Statement[] statements = executor.Peek(count);

            if (count < 1)
                throw new StatementException(tokens, "Cannot have empty macro.");
            for (int i = 0; i < count; i++)
                executor.Next(); // skip over those

            executor.Next<StatementCloseBlock>();

            Macro macro = new Macro(macroName, args.ToArray(), statements);
            executor.AddMacro(macro);
        }
        public static void _macrocall(Executor executor, Statement tokens)
        {
            string macroName = tokens.Next<TokenIdentifier>().word;
            Macro? _lookedUp = executor.LookupMacro(macroName);

            if (!_lookedUp.HasValue)
                throw new StatementException(tokens, "Macro '" + macroName + "' does not exist.");

            Macro lookedUp = _lookedUp.Value;
            string[] argNames = lookedUp.argNames;
            object[] args = new object[argNames.Length];

            // get input variables
            for (int i = 0; i < argNames.Length; i++) {
                if (!tokens.HasNext)
                    throw new StatementException(tokens, "Missing argument '" + argNames[i] + "' in macro call.");
                if(!tokens.NextIs<IObjectable>())
                    throw new StatementException(tokens, "Invalid argument type for '" + argNames[i] + "' in macro call.");
                args[i] = tokens.Next<IObjectable>().GetObject();
            }

            // save variables which collide with this macro's args.
            Dictionary<string, dynamic> collidedValues
                = new Dictionary<string, dynamic>();
            foreach (string arg in lookedUp.argNames)
                if (executor.TryGetPPV(arg, out dynamic value))
                    collidedValues[arg] = value;

            // set input variables
            for (int i = 0; i < argNames.Length; i++)
                executor.SetPPV(argNames[i], args[i]);

            // call macro
            executor.ExecuteSubsection(lookedUp.statements);

            // restore variables
            foreach (var kv in collidedValues)
                executor.SetPPV(kv.Key, kv.Value);
        }
        public static void _include(Executor executor, Statement tokens)
        {
            string file = tokens.Next<TokenStringLiteral>();

            if (!System.IO.File.Exists(file))
                throw new StatementException(tokens, "Cannot find file '" + file + "'.");

            Token[] includedTokens = Tokenizer.TokenizeFile(file);
            // TODO assemble statements
            // executor.ExecuteSubsection(statements);
        }
        public static void _strfriendly(Executor executor, Statement tokens)
        {
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

            if(executor.TryGetPPV(input, out dynamic value))
            {
                string str = value.ToString();
                string[] parts = str.Split('_', '-', ' ');
                for (int i = 0; i < parts.Length; i++)
                {
                    char[] part = parts[i].ToCharArray();
                    for (int c = 0; c < part.Length; c++)
                        part[c] = (c == 0) ? char.ToUpper(part[c]) : char.ToLower(part[c]);
                    parts[i] = new string(part);
                }
                executor.SetPPV(output, string.Join(" ", parts));
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _strupper(Executor executor, Statement tokens)
        {
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(input, out dynamic value))
            {
                string str = value.ToString();
                executor.SetPPV(output, str.ToUpper());
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _strlower(Executor executor, Statement tokens)
        {
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(input, out dynamic value))
            {
                string str = value.ToString();
                executor.SetPPV(output, str.ToLower());
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }

        public static void mc(Executor executor, Statement tokens)
        {
            string command = tokens.Next<TokenStringLiteral>();
            executor.AddCommand(command);
        }
        public static void select(Executor executor, Statement tokens)
        {
            TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
            executor.ActiveSelector = selector.core;
        }
        public static void print(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            RawTextJsonBuilder builder = new RawTextJsonBuilder();
            builder.AddTerms(executor.FString(str));

            string output = builder.BuildString();
            executor.AddCommand(Command.Tellraw(output));
        }
        public static void printp(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            RawTextJsonBuilder builder = new RawTextJsonBuilder();
            builder.AddTerms(executor.FString(str));

            string output = builder.BuildString();
            executor.AddCommand(Command.Tellraw(executor.ActiveSelectorStr, output));
        }
        public static void define(Executor executor, Statement tokens)
        {
            if(tokens.NextIs<TokenIdentifierStruct>())
            {
                TokenIdentifierStruct _struct = tokens.Next<TokenIdentifierStruct>();
                TokenStringLiteral @string = tokens.Next<TokenStringLiteral>();
                ScoreboardValueStruct value = new ScoreboardValueStruct
                    (@string, _struct.@struct, executor.scoreboard);
                executor.scoreboard.Add(value);
                return;
            }

            const int TYPE_INT = 0;
            const int TYPE_DECIMAL = 1;
            const int TYPE_BOOL = 2;
            const int TYPE_TIME = 3;

            int type = TYPE_INT;

            if(tokens.NextIs<TokenIdentifier>())
            {
                TokenIdentifier identifier = tokens.Next<TokenIdentifier>();
                string typeWord = identifier.word.ToUpper();
                switch (typeWord)
                {
                    case "INT":
                        type = TYPE_INT;
                        break;
                    case "DECIMAL":
                        type = TYPE_DECIMAL;
                        break;
                    case "BOOL":
                        type = TYPE_BOOL;
                        break;
                    case "TIME":
                        type = TYPE_TIME;
                        break;
                    default:
                        throw new Exception($"Invalid type identifier '{typeWord}'.");
                }
            }

            if (type == TYPE_DECIMAL)
            {
                if (!tokens.NextIs<TokenIntegerLiteral>())
                    throw new Exception($"No precision specified for decimal value");
                int precision = tokens.Next<TokenIntegerLiteral>();
                string decimalName = tokens.Next<TokenStringLiteral>();
                ScoreboardValueDecimal decimalValue = new ScoreboardValueDecimal
                    (decimalName, precision, executor.scoreboard);
                executor.scoreboard.Add(decimalValue);
                return;
            }

            string name = tokens.Next<TokenStringLiteral>();
            ScoreboardValue value;

            if (type == TYPE_INT)
                value = new ScoreboardValueInteger(name, executor.scoreboard);
            else if (type == TYPE_BOOL)
                value = new ScoreboardValueBoolean(name, executor.scoreboard);
            else if (type == TYPE_TIME)
                value = new ScoreboardValueTime(name, executor.scoreboard);
            else
                throw new Exception($"Variable type corrupted for '{name}'.");

            executor.scoreboard.Add(value);
        }
        public static void init(Executor executor, Statement tokens)
        {
            TokenIdentifierValue _value = tokens.Next<TokenIdentifierValue>();
            executor.AddCommands(_value.value.CommandsInit());
        }
        public static void @if(Executor executor, Statement tokens)
        {

        }
        public static void @else(Executor executor, Statement tokens)
        {

        }
        public static void give(Executor executor, Statement tokens)
        {

        }
        public static void tp(Executor executor, Statement tokens)
        {

        }
        public static void face(Executor executor, Statement tokens)
        {

        }
        public static void place(Executor executor, Statement tokens)
        {

        }
        public static void fill(Executor executor, Statement tokens)
        {

        }
        public static void replace(Executor executor, Statement tokens)
        {

        }
        public static void kill(Executor executor, Statement tokens)
        {

        }
        public static void title(Executor executor, Statement tokens)
        {

        }
        public static void halt(Executor executor, Statement tokens)
        {

        }

    }
}
