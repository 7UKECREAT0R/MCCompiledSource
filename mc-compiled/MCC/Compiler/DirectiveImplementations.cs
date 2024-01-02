﻿using mc_compiled.Commands;
using mc_compiled.Commands.Native;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.MCC.CustomEntities;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.Modding;
using mc_compiled.NBT;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.Commands.Execute;
using mc_compiled.Modding.Resources.Localization;

namespace mc_compiled.MCC.Compiler
{
    public static class DirectiveImplementations
    {
        public static void ResetState()
        {
            scatterFile = 0;
        }
        public static int scatterFile = 0;

        public static void _var(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            List<dynamic> values = new List<dynamic>();
            while (tokens.NextIs<IPreprocessor>())
                values.Add(tokens.Next<IPreprocessor>().GetValue());

            executor.SetPPV(varName, values.ToArray());
        }
        public static void _inc(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            if (executor.TryGetPPV(varName, out dynamic[] value))
            {
                try
                {
                    for (int i = 0; i < value.Length; i++)
                        value[i] += 1;
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Couldn't increment this value.");
                }
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _dec(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            if (executor.TryGetPPV(varName, out dynamic[] value))
            {
                try
                {
                    for (int i = 0; i < value.Length; i++)
                        value[i] -= 1;
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Couldn't decrement this value.");
                }
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _add(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            IPreprocessor otherToken = tokens.Next<IPreprocessor>();

            dynamic[] others;
            if (otherToken is TokenIdentifier)
            {
                if (executor.TryGetPPV((otherToken as TokenIdentifier).word, out dynamic[] ppv))
                    others = ppv;
                else throw new StatementException(tokens, "Couldn't find preprocessor variable named '" + varName + "'.");
            }
            else
            {
                List<dynamic> inputs = new List<dynamic>();
                inputs.Add((otherToken as IPreprocessor).GetValue());
                while (tokens.NextIs<IPreprocessor>())
                    inputs.Add(tokens.Next<IPreprocessor>().GetValue());
                others = inputs.ToArray();
            }

            if (executor.TryGetPPV(varName, out dynamic[] values))
            {
                dynamic[] outputs = new dynamic[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    dynamic a = values[i];

                    dynamic other;
                    if (others.Length > i)
                        other = others[i];
                    else
                    {
                        outputs[i] = a;
                        continue;
                    }

                    try
                    {
                        outputs[i] = a + other;
                    }
                    catch (Exception)
                    {
                        throw new StatementException(tokens, "Couldn't add these values.");
                    }
                }
                executor.SetPPV(varName, outputs);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _sub(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            IPreprocessor otherToken = tokens.Next<IPreprocessor>();

            dynamic[] others;
            if (otherToken is TokenIdentifier)
            {
                if (executor.TryGetPPV((otherToken as TokenIdentifier).word, out dynamic[] ppv))
                    others = ppv;
                else throw new StatementException(tokens, "Couldn't find preprocessor variable named '" + varName + "'.");
            }
            else
            {
                List<dynamic> inputs = new List<dynamic>();
                inputs.Add((otherToken as IPreprocessor).GetValue());
                while (tokens.NextIs<IPreprocessor>())
                    inputs.Add(tokens.Next<IPreprocessor>().GetValue());
                others = inputs.ToArray();
            }

            if (executor.TryGetPPV(varName, out dynamic[] values))
            {
                dynamic[] outputs = new dynamic[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    dynamic a = values[i];

                    dynamic other;
                    if (others.Length > i)
                        other = others[i];
                    else
                    {
                        outputs[i] = a;
                        continue;
                    }

                    try
                    {
                        outputs[i] = a - other;
                    }
                    catch (Exception)
                    {
                        throw new StatementException(tokens, "Couldn't subtract these values.");
                    }
                }
                executor.SetPPV(varName, outputs);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _mul(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            IPreprocessor otherToken = tokens.Next<IPreprocessor>();

            dynamic[] others;
            if (otherToken is TokenIdentifier)
            {
                if (executor.TryGetPPV((otherToken as TokenIdentifier).word, out dynamic[] ppv))
                    others = ppv;
                else throw new StatementException(tokens, "Couldn't find preprocessor variable named '" + varName + "'.");
            }
            else
            {
                List<dynamic> inputs = new List<dynamic>();
                inputs.Add((otherToken as IPreprocessor).GetValue());
                while (tokens.NextIs<IPreprocessor>())
                    inputs.Add(tokens.Next<IPreprocessor>().GetValue());
                others = inputs.ToArray();
            }

            if (executor.TryGetPPV(varName, out dynamic[] values))
            {
                dynamic[] outputs = new dynamic[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    dynamic a = values[i];

                    dynamic other;
                    if (others.Length > i)
                        other = others[i];
                    else
                    {
                        outputs[i] = a;
                        continue;
                    }

                    try
                    {
                        outputs[i] = a * other;
                    }
                    catch (Exception)
                    {
                        throw new StatementException(tokens, "Couldn't multiply these values.");
                    }
                }
                executor.SetPPV(varName, outputs);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _div(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            IPreprocessor otherToken = tokens.Next<IPreprocessor>();

            dynamic[] others;
            if (otherToken is TokenIdentifier)
            {
                if (executor.TryGetPPV((otherToken as TokenIdentifier).word, out dynamic[] ppv))
                    others = ppv;
                else throw new StatementException(tokens, "Couldn't find preprocessor variable named '" + varName + "'.");
            }
            else
            {
                List<dynamic> inputs = new List<dynamic>();
                inputs.Add((otherToken as IPreprocessor).GetValue());
                while (tokens.NextIs<IPreprocessor>())
                    inputs.Add(tokens.Next<IPreprocessor>().GetValue());
                others = inputs.ToArray();
            }

            if (executor.TryGetPPV(varName, out dynamic[] values))
            {
                dynamic[] outputs = new dynamic[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    dynamic a = values[i];

                    dynamic other;
                    if (others.Length > i)
                        other = others[i];
                    else
                    {
                        outputs[i] = a;
                        continue;
                    }

                    try
                    {
                        outputs[i] = a / other;
                    }
                    catch (Exception)
                    {
                        throw new StatementException(tokens, "Couldn't divide these values.");
                    }
                }
                executor.SetPPV(varName, outputs);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _mod(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            IPreprocessor otherToken = tokens.Next<IPreprocessor>();

            dynamic[] others;
            if (otherToken is TokenIdentifier)
            {
                if (executor.TryGetPPV((otherToken as TokenIdentifier).word, out dynamic[] ppv))
                    others = ppv;
                else throw new StatementException(tokens, "Couldn't find preprocessor variable named '" + varName + "'.");
            }
            else
            {
                List<dynamic> inputs = new List<dynamic>();
                inputs.Add((otherToken as IPreprocessor).GetValue());
                while (tokens.NextIs<IPreprocessor>())
                    inputs.Add(tokens.Next<IPreprocessor>().GetValue());
                others = inputs.ToArray();
            }

            if (executor.TryGetPPV(varName, out dynamic[] values))
            {
                dynamic[] outputs = new dynamic[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    dynamic a = values[i];

                    dynamic other;
                    if (others.Length > i)
                        other = others[i];
                    else
                    {
                        outputs[i] = a;
                        continue;
                    }

                    try
                    {
                        outputs[i] = a % other;
                    }
                    catch (Exception)
                    {
                        throw new StatementException(tokens, "Couldn't modulo these values.");
                    }
                }
                executor.SetPPV(varName, outputs);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _pow(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            IPreprocessor otherToken = tokens.Next<IPreprocessor>();

            dynamic[] others;
            if (otherToken is TokenIdentifier)
            {
                if (executor.TryGetPPV((otherToken as TokenIdentifier).word, out dynamic[] ppv))
                    others = ppv;
                else throw new StatementException(tokens, "Couldn't find preprocessor variable named '" + varName + "'.");
            }
            else
            {
                List<dynamic> inputLiterals = new List<dynamic>();
                inputLiterals.Add((otherToken as IPreprocessor).GetValue());
                while (tokens.NextIs<IPreprocessor>())
                    inputLiterals.Add(tokens.Next<IPreprocessor>().GetValue());
                others = inputLiterals.ToArray();
            }

            if (executor.TryGetPPV(varName, out dynamic[] inputs))
            {
                dynamic[] outputs = new dynamic[inputs.Length];
                for (int i = 0; i < inputs.Length; i++)
                {
                    dynamic input = inputs[i];

                    dynamic other;
                    if (others.Length > i)
                        other = others[i];
                    else
                    {
                        outputs[i] = input;
                        continue;
                    }

                    if (!(other is int))
                        throw new StatementException(tokens, "Can only exponentiate to an integer value.");
                    int count = (int)other;

                    try
                    {
                        outputs[i] = input;
                        for (int x = 1; x < count; x++)
                            outputs[i] *= input;
                    }
                    catch (Exception)
                    {
                        throw new StatementException(tokens, "Couldn't pow these values.");
                    }
                }
                executor.SetPPV(varName, outputs);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _swap(Executor executor, Statement tokens)
        {
            string aName = tokens.Next<TokenIdentifier>().word;
            string bName = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(aName, out dynamic[] a))
            {
                if (executor.TryGetPPV(bName, out dynamic[] b))
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
            IPreprocessor otherToken = tokens.Next<IPreprocessor>();
            dynamic[] others = new dynamic[] { otherToken.GetValue() };

            if (otherToken is TokenIdentifier)
                if (executor.TryGetPPV((otherToken as TokenIdentifier).word, out dynamic[] ppv))
                    others = ppv;

            // if the next block/statement should be run
            bool run = true;

            if (executor.TryGetPPV(varName, out dynamic[] firsts))
            {
                for (int i = 0; i < firsts.Length; i++)
                {
                    dynamic a = firsts[i];

                    dynamic other;
                    if (others.Length > i)
                        other = others[i];
                    else
                        throw new StatementException(tokens, "Preprocessor variable lengths didn't match.");

                    try
                    {
                        switch (compare.GetCompareType())
                        {
                            case TokenCompare.Type.EQUAL:
                                run &= a == other;
                                break;
                            case TokenCompare.Type.NOT_EQUAL:
                                run &= a != other;
                                break;
                            case TokenCompare.Type.LESS:
                                run &= a < other;
                                break;
                            case TokenCompare.Type.LESS_OR_EQUAL:
                                run &= a <= other;
                                break;
                            case TokenCompare.Type.GREATER:
                                run &= a > other;
                                break;
                            case TokenCompare.Type.GREATER_OR_EQUAL:
                                run &= a >= other;
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        throw new StatementException(tokens, "Could not compare those two types.");
                    }
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

                if (run)
                {
                    block.openAction = null;
                    block.CloseAction = null;
                } else
                {
                    block.openAction = (e) =>
                    {
                        for (int i = 0; i < block.statementsInside; i++)
                            e.Next();
                    };
                    block.CloseAction = null;
                }
                return;
            }
            else if (!run)
                executor.Next(); // skip the next statement
        }
        public static void _else(Executor executor, Statement tokens)
        {
            bool run = !executor.GetLastIfResult();

            if (executor.NextIs<StatementOpenBlock>())
            {
                StatementOpenBlock block = executor.Peek<StatementOpenBlock>();
                if (run)
                {
                    block.openAction = null;
                    block.CloseAction = null;
                }
                else
                {
                    block.openAction = (e) =>
                    {
                        block.CloseAction = null;
                        for (int i = 0; i < block.statementsInside; i++)
                            e.Next();
                    };
                }
                return;
            }
            else if (!run)
                executor.Next(); // skip the next statement
        }
        public static void _repeat(Executor executor, Statement tokens)
        {
            bool useRange;
            Range range = default;
            int amount = default;

            if (tokens.NextIs<TokenIntegerLiteral>())
            {
                useRange = false;
                amount = tokens.Next<TokenIntegerLiteral>().number;
            }
            else
            {
                range = tokens.Next<TokenRangeLiteral>().range;
                useRange = !range.single;
                amount = range.min.GetValueOrDefault();

                if (range.IsUnbounded)
                    throw new StatementException(tokens, "Range parameter must have a start and end when used in $repeat.");
            }

            string tracker = null;

            if (tokens.HasNext && tokens.NextIs<TokenIdentifier>())
                tracker = tokens.Next<TokenIdentifier>().word;

            Statement[] statements = executor.NextExecutionSet();

            if(useRange)
            {
                int min = range.min.Value;
                int max = range.max.Value;
                for (int i = min; i <= max; i++)
                {
                    if (tracker != null)
                        executor.SetPPV(tracker, new dynamic[] { i });
                    executor.ExecuteSubsection(statements);
                }
            } else
            {
                for (int i = 0; i < amount; i++)
                {
                    if (tracker != null)
                        executor.SetPPV(tracker, new dynamic[] { i });
                    executor.ExecuteSubsection(statements);
                }
            }
        }
        public static void _log(Executor executor, Statement tokens)
        {
            if (executor.linting)
                return;

            List<string> strings = new List<string>();

            while(tokens.HasNext)
            {
                Token next = tokens.Next();

                if (next is IPreprocessor preprocessor)
                    strings.Add(preprocessor.GetValue().ToString());
                else
                    strings.Add(next.DebugString());
            }

            Console.WriteLine("[LOG] {0}", string.Join(" ", strings));
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

            string docs = executor.GetDocumentationString(out _);
            Macro macro = new Macro(macroName, docs, args.ToArray(), statements);
            executor.RegisterMacro(macro);
        }
        public static void _macrocall(Executor executor, Statement tokens)
        {
            string macroName = tokens.Next<TokenIdentifier>().word;
            Macro? _lookedUp = executor.LookupMacro(macroName);

            if (!_lookedUp.HasValue)
                throw new StatementException(tokens, "Macro '" + macroName + "' does not exist.");

            Macro lookedUp = _lookedUp.Value;
            string[] argNames = lookedUp.argNames;
            dynamic[][] args = new dynamic[argNames.Length][];

            // get input variables
            for (int i = 0; i < argNames.Length; i++)
            {
                if (!tokens.HasNext)
                    throw new StatementException(tokens, "Missing argument '" + argNames[i] + "' in macro call.");

                if (tokens.NextIs<TokenUnresolvedPPV>())
                {
                    args[i] = executor.ResolvePPV(tokens.Next<TokenUnresolvedPPV>(), tokens);
                    continue;
                }

                if (!tokens.NextIs<IPreprocessor>())
                    throw new StatementException(tokens, "Invalid argument type for '" + argNames[i] + "' in macro call.");

                args[i] = new dynamic[] { tokens.Next<IPreprocessor>().GetValue() };
            }

            // save variables which collide with this macro's args.
            Dictionary<string, dynamic[]> collidedValues
                = new Dictionary<string, dynamic[]>();
            foreach (string arg in lookedUp.argNames)
                if (executor.TryGetPPV(arg, out dynamic[] value))
                    collidedValues[arg] = value;

            // set input variables
            for (int i = 0; i < argNames.Length; i++)
                executor.SetPPV(argNames[i], args[i]);

            // call macro
            try
            {
                executor.ExecuteSubsection(lookedUp.statements);
            } catch(StatementException e)
            {
                // set it so that the error is placed at the location of the call and the macro.
                int[] exceptionLines = e.statement.Lines.Concat(tokens.Lines).ToArray();
                string exceptionSource = tokens.Source + ": " + e.statement.Source;
                e.statement.SetSource(exceptionLines, exceptionSource);
                throw;
            }

            // restore variables
            foreach (var kv in collidedValues)
                executor.SetPPV(kv.Key, kv.Value);
        }
        public static void _include(Executor executor, Statement tokens)
        {
            string file = tokens.Next<TokenStringLiteral>();
            if (!file.EndsWith(".mcc"))
                file += ".mcc";

            if (!System.IO.File.Exists(file))
                throw new StatementException(tokens, "Cannot find file '" + file + "'.");

            Token[] includedTokens = Tokenizer.TokenizeFile(file);

            if (Program.DEBUG)
            {
                Console.WriteLine("\t[INCLUDE]\tA detailed overview of the tokenization results follows:");
                Console.WriteLine(string.Join("", from t in includedTokens select t.DebugString()));
                Console.WriteLine();
                Console.WriteLine("\t[INCLUDE]\tReconstruction of the processed code through tokens:");
                Console.WriteLine(string.Join(" ", from t in includedTokens select t.AsString()));
                Console.WriteLine();
            }

            Statement[] statements = Assembler.AssembleTokens(includedTokens);

            if (Program.DEBUG)
            {
                Console.WriteLine("\t[INCLUDE]\tThe overview of assembled statements is as follows:");
                Console.WriteLine(string.Join("\n", from s in statements select s.ToString()));
                Console.WriteLine();
            }

            executor.ExecuteSubsection(statements);
        }
        public static void _strfriendly(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;

            string input;
            if (tokens.NextIs<TokenIdentifier>(false))
                input = tokens.Next<TokenIdentifier>().word;
            else
                input = output;

            if (executor.TryGetPPV(input, out dynamic[] value))
            {
                dynamic[] results = new dynamic[value.Length];
                for (int r = 0; r < value.Length; r++)
                {
                    string str = value[r].ToString();
                    string[] parts = str.Split('_', '-', ' ');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        char[] part = parts[i].ToCharArray();
                        for (int c = 0; c < part.Length; c++)
                            part[c] = (c == 0) ? char.ToUpper(part[c]) : char.ToLower(part[c]);
                        parts[i] = new string(part);
                    }
                    results[r] = string.Join(" ", parts);

                }
                executor.SetPPV(output, results);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _strupper(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;

            string input;
            if (tokens.NextIs<TokenIdentifier>(false))
                input = tokens.Next<TokenIdentifier>().word;
            else
                input = output;

            if (executor.TryGetPPV(input, out dynamic[] value))
            {
                dynamic[] results = new dynamic[value.Length];
                for (int r = 0; r < value.Length; r++)
                {
                    string str = value[r].ToString();
                    results[r] = str.ToUpper();
                }
                executor.SetPPV(output, results);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _strlower(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;

            string input;
            if (tokens.NextIs<TokenIdentifier>(false))
                input = tokens.Next<TokenIdentifier>().word;
            else
                input = output;

            if (executor.TryGetPPV(input, out dynamic[] value))
            {
                dynamic[] results = new dynamic[value.Length];
                for (int r = 0; r < value.Length; r++)
                {
                    string str = value[r].ToString();
                    results[r] = str.ToLower();
                }
                executor.SetPPV(output, results);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _sum(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;

            string input;
            if (tokens.NextIs<TokenIdentifier>(false))
                input = tokens.Next<TokenIdentifier>().word;
            else
                input = output;

            if (executor.TryGetPPV(input, out dynamic[] values))
            {
                try
                {
                    dynamic result = values[0];
                    for (int i = 1; i < values.Length; i++)
                        result += values[i];
                    executor.SetPPV(output, new dynamic[] { result });
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Couldn't add these values.");
                }
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _median(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;

            string input;
            if (tokens.NextIs<TokenIdentifier>(false))
                input = tokens.Next<TokenIdentifier>().word;
            else
                input = output;

            if (executor.TryGetPPV(input, out dynamic[] values))
            {
                try
                {
                    int len = values.Length;
                    if (len < 2)
                    {
                        executor.SetPPV(output, new dynamic[] { values[0] });
                        return;
                    }
                    else if (len % 2 == 0)
                    {
                        int mid = len / 2;
                        dynamic first = values[mid];
                        dynamic second = values[mid - 1];
                        dynamic result = (first + second) / 2;
                        executor.SetPPV(output, new dynamic[] { result });
                    }
                    else
                    {
                        dynamic result = values[len / 2]; // truncates to middle index
                        executor.SetPPV(output, new dynamic[] { result });
                    }
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Couldn't calculate median of these values.");
                }
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _mean(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;

            string input;
            if (tokens.NextIs<TokenIdentifier>(false))
                input = tokens.Next<TokenIdentifier>().word;
            else
                input = output;

            if (executor.TryGetPPV(input, out dynamic[] values))
            {
                try
                {
                    int length = values.Length;
                    dynamic result = values[0];
                    for (int i = 1; i < length; i++)
                        result += values[i];
                    result /= length;
                    executor.SetPPV(output, new dynamic[] { result });
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Couldn't add/divide these values.");
                }
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _sort(Executor executor, Statement tokens)
        {
            string sortDirection = tokens.Next<TokenIdentifier>().word.ToUpper();
            string variable = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(variable, out dynamic[] values))
            {
                try
                {
                    List<dynamic> listValues = values.ToList();
                    listValues.Sort();

                    if (sortDirection.StartsWith("DE"))
                        listValues.Reverse();

                    executor.SetPPV(variable, listValues.ToArray());
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Couldn't sort these values.");
                }
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + variable + "' does not exist.");
        }
        public static void _reverse(Executor executor, Statement tokens)
        {
            string variable = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(variable, out dynamic[] values))
            {
                if (values.Length < 2)
                    return;

                try
                {
                    // reverse the order.
                    int end = values.Length - 1;
                    int max = values.Length / 2;
                    for(int i = 0; i < max; i++)
                    {
                        int e = end - i;
                        dynamic temp = values[i];
                        values[i] = values[e];
                        values[e] = temp;
                    }

                    executor.SetPPV(variable, values);
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Couldn't reverse these values.");
                }
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + variable + "' does not exist.");
        }
        public static void _unique(Executor executor, Statement tokens)
        {
            string variable = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(variable, out dynamic[] values))
            {
                if (values.Length < 2)
                    return;

                HashSet<object> items = new HashSet<object>();
                
                foreach (dynamic value in values)
                    items.Add(value);

                executor.SetPPV(variable, items.ToArray());
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + variable + "' does not exist.");
        }
        public static void _iterate(Executor executor, Statement tokens)
        {
            string input = tokens.Next<TokenIdentifier>().word;
            string current = tokens.Next<TokenIdentifier>().word;

            if (!executor.TryGetPPV(input, out dynamic[] values))
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");

            Statement[] statements = executor.NextExecutionSet();

            void IterateArray(JArray array)
            {
                int item = -1;
                foreach (JToken arrayItem in array)
                {
                    item++;
                    if (PreprocessorUtils.TryUnwrapToken(arrayItem, out object obj))
                    {
                        if (obj == null)
                            throw new StatementException(tokens, $"Couldn't unwrap JSON token to be placed in a preprocessor variable: {arrayItem.ToString()}");

                        executor.SetPPV(current, new dynamic[] { obj });
                        executor.ExecuteSubsection(statements);
                    }
                    else
                        throw new StatementException(tokens, $"JSON Error: Cannot store token of type '{arrayItem.Type}' in a preprocessor variable.");
                }
                return;
            }

            foreach (dynamic value in values)
            {
                if (value is TokenJSONLiteral jsonLiteral)
                {
                    JToken token = jsonLiteral.token;
                    if (token is JArray array)
                        IterateArray(array);
                }
                else if(value is JArray array)
                {
                    IterateArray(array);
                }
                else
                {
                    executor.SetPPV(current, new dynamic[] { value });
                    executor.ExecuteSubsection(statements);
                }
            }
        }
        public static void _len(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;

            if(tokens.NextIs<TokenStringLiteral>(false))
            {
                // String
                string inputString = tokens.Next<TokenStringLiteral>().text;
                executor.SetPPV(output, new dynamic[] { inputString.Length });
                return;
            }
            else if(tokens.NextIs<TokenJSONLiteral>())
            {
                // JSON Array
                JToken inputJSON = tokens.Next<TokenJSONLiteral>();

                if (inputJSON is JArray array)
                {
                    executor.SetPPV(output, new dynamic[] { array.Count });
                    return;
                }
                else
                    throw new StatementException(tokens, "Attempted to get the length of a non-array JSON token.");
            }

            // Preprocessor Variable
            string input = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(input, out dynamic[] values))
            {
                int length = values.Length;
                executor.SetPPV(output, new dynamic[] { length });
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _json(Executor executor, Statement tokens)
        {
            JToken json;

            if(tokens.NextIs<TokenJSONLiteral>())
                json = tokens.Next<TokenJSONLiteral>().token;
            else
            {
                string file = tokens.Next<TokenStringLiteral>();
                json = executor.LoadJSONFile(file, tokens);
            }

            string output = tokens.Next<TokenIdentifier>().word;

            if(tokens.NextIs<TokenStringLiteral>())
            {
                string accessor = tokens.Next<TokenStringLiteral>();
                string[] accessParts = PreprocessorUtils.ParseAccessor(accessor);

                // crawl the tree
                foreach (string _access in accessParts)
                {
                    string access = _access.Trim();
                    if (json.Type == JTokenType.Array)
                    {
                        JArray array = json as JArray;
                        if (!int.TryParse(access, out int index))
                            throw new StatementException(tokens, $"JSON Error: Array at '{array.Path}' requires index to access. Given: {access}");
                        if (index < 0)
                            throw new StatementException(tokens, $"JSON Error: Index given was less than 0.");
                        if (index >= array.Count)
                            throw new StatementException(tokens, $"JSON Error: Array at '{array.Path}' only contains {array.Count} items. Given: {index + 1}");
                        json = array[index];
                        continue;
                    }
                    else if (json.Type == JTokenType.Object)
                    {
                        JObject obj = json as JObject;
                        if (!obj.TryGetValue(access, out json))
                            throw new StatementException(tokens, $"JSON Error: Cannot find child '{access}' under token {obj.Path}.");
                        continue;
                    }
                    else
                        throw new StatementException(tokens, $"JSON Error: Unexpected end of JSON tree at {json.Path}.");
                }
            }

            if(PreprocessorUtils.TryUnwrapToken(json, out object unwrapped))
            {
                executor.SetPPV(output, new[] { unwrapped });
                return;
            } else
                throw new StatementException(tokens, $"JSON Error: Cannot store token of type '{json.Type}' in a preprocessor variable.");
        }
        public static void _call(Executor executor, Statement tokens)
        {
            string functionName = tokens.Next<TokenStringLiteral>().text;

            if (!executor.functions.TryGetFunctions(functionName, out Function[] functions))
                throw new StatementException(tokens, $"Could not find function by the name '{functionName}'");

            Token[] remainingTokens = tokens.GetRemainingTokens();
            Token[] finalTokens = new Token[remainingTokens.Length + 3];

            int line = tokens.Lines[0];
            Array.Copy(remainingTokens, 0, finalTokens, 2, remainingTokens.Length);
            finalTokens[0] = new TokenIdentifierFunction(functionName, functions, line);
            finalTokens[1] = new TokenOpenParenthesis(line);
            finalTokens[finalTokens.Length - 1] = new TokenCloseParenthesis(line);


            StatementFunctionCall callStatement = new StatementFunctionCall(finalTokens);
            callStatement.SetSource(tokens.Lines, tokens.Source);

            callStatement
                .ClonePrepare(executor)
                .Run0(executor);
        }

        public static void mc(Executor executor, Statement tokens)
        {
            string command = tokens.Next<TokenStringLiteral>();
            executor.AddCommand(command);
        }
        public static void globalprint(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            List<JSONRawTerm> terms = executor.FString(str, tokens, out bool advanced);

            string[] commands;

            if (advanced)
                commands = Command.Execute().As(Selector.ALL_PLAYERS).AtSelf().RunOver(Executor.ResolveRawText(terms, "tellraw @s "));
            else
                commands = Executor.ResolveRawText(terms, "tellraw @a ");

            CommandFile file = executor.CurrentFile;
            executor.AddCommands(commands, "print", $"Called in a globalprint command located in {file.CommandReference} line {executor.NextLineNumber}");
        }
        public static void print(Executor executor, Statement tokens)
        {
            Selector player;

            player = tokens.NextIs<TokenSelectorLiteral>(false) ?
                tokens.Next<TokenSelectorLiteral>() : Selector.SELF;

            string str = tokens.Next<TokenStringLiteral>();
            List<JSONRawTerm> terms = executor.FString(str, tokens, out bool _);
            string[] commands = Executor.ResolveRawText(terms, $"tellraw {player} ");

            CommandFile file = executor.CurrentFile;
            executor.AddCommands(commands, "print", $"Called in a print command located in {file.CommandReference} line {executor.NextLineNumber}");
        }
        public static void lang(Executor executor, Statement tokens)
        {
            string locale = tokens.Next<TokenIdentifier>().word;

            const bool DEFAULT_MERGE = true;

            // create preprocessor variable if it doesn't exist.
            if (!executor.ppv.TryGetValue(LanguageManager.MERGE_PPV, out _))
                executor.ppv[LanguageManager.MERGE_PPV] = new dynamic[] { DEFAULT_MERGE };

            if (Program.DEBUG)
                Console.WriteLine("Set locale to '{0}'", locale);

            if (executor.linting)
                return; // due to no project being given in linting operations, it's not necessary.

            executor.SetLocale(locale);
        }
        public static void define(Executor executor, Statement tokens)
        {
            string docs = executor.GetDocumentationString(out bool hadDocumentation);

            ScoreboardManager.ValueDefinition def = executor
                .scoreboard.GetNextValueDefinition(tokens);
            
            // create the new scoreboard value.
            ScoreboardValue value = def.Create(executor.scoreboard, tokens);
            value.Documentation = docs;

            // register it to the executor.
            executor.scoreboard.TryThrowForDuplicate(value, tokens);
            executor.scoreboard.Add(value);

            // all the rest of this is getting the commands to define the variable.
            List<string> commands = new List<string>();

            if (hadDocumentation && Program.DECORATE)
                commands.AddRange(docs.Trim().Split('\n').Select(str => "# " + str.Trim()));

            commands.AddRange(value.CommandsDefine());

            if (def.defaultValue != null)
            {
                switch (def.defaultValue)
                {
                    case TokenLiteral literal:
                        commands.AddRange(value.AssignLiteral(literal, tokens));
                        break;
                    case TokenIdentifierValue identifier:
                        commands.AddRange(value.Assign(identifier.value, tokens));
                        break;
                    default:
                        throw new StatementException(tokens, $"Cannot assign value of type {def.defaultValue.GetType().Name} into a variable");
                }
            }

            // add the commands to the executor.
            CommandFile file = executor.CurrentFile;
            executor.AddCommands(commands, "define" + value.Name, $"Called when defining the value '{value.Name}' in {file.CommandReference} line {executor.NextLineNumber}");
        }
        public static void init(Executor executor, Statement tokens)
        {
            ScoreboardValue value;
            List<string> commands = new List<string>();

            while (tokens.HasNext)
            {
                if (tokens.NextIs<IInformationless>())
                    continue;

                value = tokens.Next<TokenIdentifierValue>().value;
                commands.AddRange(value.CommandsInit(value.clarifier.CurrentString));
            }

            executor.AddCommands(commands, null, null, true);
        }
        public static void @if(Executor executor, Statement tokens)
        {
            if (!executor.HasNext)
                throw new StatementException(tokens, "Unexpected end of file after if-statement.");

            // 1.1 rework (post new-execute)
            ComparisonSet set = ComparisonSet.GetComparisons(executor, tokens);
            set.InvertAll(false);
            set.Run(executor, tokens);
        }
        public static void @else(Executor executor, Statement tokens)
        {
            if (!executor.HasNext)
                throw new StatementException(tokens, "Unexpected end of file after else-statement.");

            PreviousComparisonStructure set = executor.GetLastCompare();

            if (set == null)
                throw new StatementException(tokens, "No if-statement was found in front of this else-statement at this scope level.");

            ScoreboardValue resultBoolean = set.resultStore;

            bool cancel = set.cancel;
            string prefix = "";

            if (!cancel)
            {
                prefix = Command.Execute()
                    .WithSubcommand(new SubcommandUnless(set.conditionalUsed))
                    .Run();
            }

            Statement nextStatement = executor.Seek();

            if (nextStatement is StatementOpenBlock openBlock)
            {
                // only do the block stuff if necessary.
                if (openBlock.statementsInside > 0)
                {
                    if (cancel)
                    {
                        openBlock.openAction = (e) =>
                        {
                            openBlock.CloseAction = null;
                            for (int i = 0; i < openBlock.statementsInside; i++)
                                e.Next();
                        };

                        set.Dispose();
                    }
                    else
                    {
                        if (openBlock.meaningfulStatementsInside == 1)
                        {
                            // modify prepend buffer as if 1 statement was there
                            executor.AppendCommandPrepend(prefix);
                            openBlock.openAction = null;
                            openBlock.CloseAction = (e) =>
                            {
                                set.Dispose();
                            };
                        }
                        else
                        {
                            CommandFile blockFile = Executor.GetNextGeneratedFile("branch");

                            if (Program.DECORATE)
                            {
                                blockFile.Add($"# Run if the previous condition {set.sourceStatement} did not run.");
                                blockFile.AddTrace(executor.CurrentFile);
                            }

                            string command = prefix + Command.Function(blockFile);
                            executor.AddCommand(command);

                            openBlock.openAction = (e) =>
                            {
                                e.PushFile(blockFile);
                            };
                            openBlock.CloseAction = (e) =>
                            {
                                set.Dispose();
                                e.PopFile();
                            };
                        }
                    }
                }
                else
                {
                    openBlock.openAction = null;
                    openBlock.CloseAction = null;

                    executor.DeferAction(e =>
                    {
                        set.Dispose();
                    });
                }
            }
            else
            {
                if (cancel)
                {
                    while (executor.HasNext && executor.Peek().Skip)
                        executor.Next();
                    executor.Next();
                }
                else
                    executor.AppendCommandPrepend(prefix);

                executor.DeferAction(e =>
                {
                    set.Dispose();
                });
            }
        }
        public static void give(Executor executor, Statement tokens)
        {
            Selector player = tokens.Next<TokenSelectorLiteral>();

            string itemName = tokens.Next<TokenStringLiteral>();
            string itemNameComp = itemName.ToUpper();
            bool needsStructure = false;

            int count = 1;
            int data = 0;
            bool keep = false;
            bool lockInventory = false;
            bool lockSlot = false;
            List<string> loreLines = new List<string>();
            List<string> canPlaceOn = new List<string>();
            List<string> canDestroy = new List<string>();
            List<Tuple<Enchantment, int>> enchants = new List<Tuple<Enchantment, int>>();
            string displayName = null;

            ItemTagBookData? book = null;
            List<string> bookPages = null;
            ItemTagCustomColor? color = null;

            if (tokens.NextIs<TokenIntegerLiteral>())
            {
                count = tokens.Next<TokenIntegerLiteral>();

                if (tokens.NextIs<TokenIntegerLiteral>())
                    data = tokens.Next<TokenIntegerLiteral>();
            }

            TokenBuilderIdentifier builderIdentifier;

            while (executor.NextBuilderField(ref tokens, out builderIdentifier))
            {
                string builderField = builderIdentifier.BuilderField;

                switch (builderField)
                {
                    case "KEEP":
                        keep = true;
                        needsStructure = true;
                        break;
                    case "LOCKINVENTORY":
                        lockInventory = true;
                        needsStructure = true;
                        break;
                    case "LOCKSLOT":
                        lockSlot = true;
                        needsStructure = true;
                        break;
                    case "CANPLACEON":
                        canPlaceOn.Add(tokens.Next<TokenStringLiteral>());
                        break;
                    case "CANDESTROY":
                        canDestroy.Add(tokens.Next<TokenStringLiteral>());
                        break;
                    case "ENCHANT":
                        ParsedEnumValue parsedEnchantment = tokens.Next<TokenIdentifierEnum>().value;
                        parsedEnchantment.RequireType<Enchantment>(tokens);
                        Enchantment enchantment = (Enchantment)parsedEnchantment.value;
                        int level = tokens.Next<TokenIntegerLiteral>();
                        enchants.Add(new Tuple<Enchantment, int>(enchantment, level));
                        needsStructure = true;
                        break;
                    case "NAME":
                        displayName = tokens.Next<TokenStringLiteral>();
                        needsStructure = true;
                        break;
                    case "LORE":
                        loreLines.Add(tokens.Next<TokenStringLiteral>());
                        needsStructure = true;
                        break;
                    default:
                        break;
                }
                if (itemNameComp.Equals("WRITTEN_BOOK"))
                {
                    switch (builderField)
                    {
                        case "TITLE":
                            if (book == null)
                                book = new ItemTagBookData();
                            ItemTagBookData bookData0 = book.Value;
                            bookData0.title = tokens.Next<TokenStringLiteral>();
                            book = bookData0;
                            needsStructure = true;
                            break;
                        case "AUTHOR":
                            if (book == null)
                                book = new ItemTagBookData();
                            ItemTagBookData bookData1 = book.Value;
                            bookData1.author = tokens.Next<TokenStringLiteral>();
                            book = bookData1;
                            needsStructure = true;
                            break;
                        case "PAGE":
                            if (book == null)
                                book = new ItemTagBookData();
                            if (bookPages == null)
                                bookPages = new List<string>();
                            bookPages.Add(tokens.Next<TokenStringLiteral>().text.Replace("\\n", "\n"));
                            needsStructure = true;
                            break;
                    }
                }
                if (itemNameComp.StartsWith("LEATHER_"))
                {
                    if(builderField.Equals("DYE"))
                    {
                        color = new ItemTagCustomColor()
                        {
                            r = (byte)tokens.Next<TokenIntegerLiteral>(),
                            g = (byte)tokens.Next<TokenIntegerLiteral>(),
                            b = (byte)tokens.Next<TokenIntegerLiteral>()
                        };
                        needsStructure = true;
                        continue;
                    }
                }
            }

            // create a structure file since this item is too complex
            if (needsStructure)
            {
                if (bookPages != null) {
                    ItemTagBookData bookData = book.Value;
                    bookData.pages = bookPages.ToArray();
                    book = bookData;
                }

                ItemStack item = new ItemStack()
                {
                    id = itemName,
                    count = count,
                    damage = data,
                    keep = keep,
                    lockMode = lockInventory ? NBT.ItemLockMode.LOCK_IN_INVENTORY :
                        lockSlot ? NBT.ItemLockMode.LOCK_IN_SLOT : NBT.ItemLockMode.NONE,
                    displayName = displayName,
                    lore = loreLines.ToArray(),
                    enchantments = enchants.Select(e => new EnchantmentEntry(e.Item1, e.Item2)).ToArray(),
                    canPlaceOn = canPlaceOn.ToArray(),
                    canDestroy = canDestroy.ToArray(),
                    bookData = book,
                    customColor = color
                };
                StructureFile file = new StructureFile("item" + item.GetHashCode(),
                    Executor.MCC_GENERATED_FOLDER, StructureNBT.SingleItem(item));
                executor.AddExtraFile(file);

                string cmd = Command.StructureLoad(file.CommandReference, Coord.here, Coord.here, Coord.here,
                    StructureRotation._0_degrees, StructureMirror.none, true, false);

                if (player.NonSelf)
                    executor.AddCommand(Command.Execute().As(player).AtSelf().Run(cmd));
                else
                    executor.AddCommand(cmd);
                return;
            }

            List<string> json = new List<string>();

            if (keep)
                json.Add("\"keep_on_death\":{}");

            if (lockSlot)
                json.Add("\"item_lock\":{\"mode\":\"lock_in_slot\"}");
            else if (lockInventory)
                json.Add("\"item_lock\":{\"mode\":\"lock_in_inventory\"}");

            if (canPlaceOn.Count > 0)
            {
                string blocks = string.Join(",", canPlaceOn.Select(c => $"\"{c}\""));
                json.Add($"\"minecraft:can_place_on\":{{\"blocks\":[{blocks}]}}");
            }
            if (canDestroy.Count > 0)
            {
                string blocks = string.Join(",", canDestroy.Select(c => $"\"{c}\""));
                json.Add($"\"minecraft:can_destroy\":{{\"blocks\":[{blocks}]}}");
            }

            string command = Command.Give(player.ToString(), itemName, count, data);
            if (json.Count > 0)
                command += $" {{{string.Join(",", json)}}}";

            executor.AddCommand(command);
        }
        public static void tp(Executor executor, Statement tokens)
        {
            bool GetCheckForBlocks()
            {
                if (tokens.NextIs<TokenBooleanLiteral>())
                    return tokens.Next<TokenBooleanLiteral>();
                else
                    return false;
            }
            void ParseArgs(string selector)
            {
                Coord x = tokens.Next<TokenCoordinateLiteral>();
                Coord y = tokens.Next<TokenCoordinateLiteral>();
                Coord z = tokens.Next<TokenCoordinateLiteral>();

                if(tokens.NextIs<TokenIdentifier>())
                {
                    string id = tokens.Next<TokenIdentifier>().word;
                    if(id.ToUpper().Equals("FACING"))
                    {
                        if (tokens.NextIs<TokenCoordinateLiteral>())
                        {
                            Coord fx = tokens.Next<TokenCoordinateLiteral>();
                            Coord fy = tokens.Next<TokenCoordinateLiteral>();
                            Coord fz = tokens.Next<TokenCoordinateLiteral>();
                            executor.AddCommand(Command.TeleportFacing(selector, x, y, z, fx, fy, fz, GetCheckForBlocks()));
                        }
                        else if (tokens.NextIs<TokenSelectorLiteral>(true))
                        {
                            Selector facingEntity = tokens.Next<TokenSelectorLiteral>().selector;
                            executor.AddCommand(Command.TeleportFacing(selector, x, y, z, facingEntity.ToString(), GetCheckForBlocks()));
                        }
                    }
                }
                else if (tokens.NextIs<TokenCoordinateLiteral>())
                {
                    Coord ry = tokens.Next<TokenCoordinateLiteral>();
                    Coord rx = tokens.Next<TokenCoordinateLiteral>();
                    executor.AddCommand(Command.Teleport(selector, x, y, z, ry, rx, GetCheckForBlocks()));
                }
                else
                    executor.AddCommand(Command.Teleport(selector, x, y, z, GetCheckForBlocks()));
            }

            if(tokens.NextIs<TokenCoordinateLiteral>())
            {
                ParseArgs(Selector.SELF.ToString());
                return;
            }

            Selector entity = tokens.Next<TokenSelectorLiteral>();

            if (tokens.NextIs<TokenSelectorLiteral>())
            {
                TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
                executor.AddCommand(Command.Teleport(entity.ToString(), selector.selector.ToString(), GetCheckForBlocks()));
                return;
            }

            ParseArgs(entity.ToString());
        }
        public static void move(Executor executor, Statement tokens)
        {
            Selector selector = tokens.Next<TokenSelectorLiteral>();

            string direction = tokens.Next<TokenIdentifier>().word;
            float amount = tokens.Next<TokenNumberLiteral>().GetNumber();

            bool checkForBlocks = false;
            if (tokens.NextIs<TokenBooleanLiteral>())
                checkForBlocks = tokens.Next<TokenBooleanLiteral>();

            Coord x = Coord.herefacing;
            Coord y = Coord.herefacing;
            Coord z = Coord.herefacing;

            switch (direction.ToUpper())
            {
                case "LEFT":
                    x = new Coord(amount, true, false, true);
                    break;
                case "RIGHT":
                    x = new Coord(-amount, true, false, true);
                    break;
                case "UP":
                    y = new Coord(amount, true, false, true);
                    break;
                case "DOWN":
                    y = new Coord(-amount, true, false, true);
                    break;
                case "FORWARD":
                case "FORWARDS":
                    z = new Coord(amount, true, false, true);
                    break;
                case "BACKWARD":
                case "BACKWARDS":
                    z = new Coord(-amount, true, false, true);
                    break;
            }

            executor.AddCommand(Command.Teleport(selector.ToString(), x, y, z, checkForBlocks));
        }
        public static void face(Executor executor, Statement tokens)
        {
            Selector selector = tokens.Next<TokenSelectorLiteral>();

            if (tokens.NextIs<TokenSelectorLiteral>())
            {
                TokenSelectorLiteral other = tokens.Next<TokenSelectorLiteral>();
                executor.AddCommand(Command.TeleportFacing(selector.ToString(), Coord.here, Coord.here, Coord.here, other.ToString()));
            }
            else
            {
                Coord x = tokens.Next<TokenCoordinateLiteral>();
                Coord y = tokens.Next<TokenCoordinateLiteral>();
                Coord z = tokens.Next<TokenCoordinateLiteral>();

                executor.AddCommand(Command.TeleportFacing(selector.ToString(), Coord.here, Coord.here, Coord.here, x, y, z));
            }
        }
        public static void rotate(Executor executor, Statement tokens)
        {
            Selector selector = tokens.Next<TokenSelectorLiteral>();

            TokenNumberLiteral number = tokens.Next<TokenNumberLiteral>();
            Coord ry, rx = Coord.here;

            if (number is TokenDecimalLiteral)
                ry = new Coord(number.GetNumber(), true, true, false);
            else
                ry = new Coord(number.GetNumberInt(), false, true, false);

            if (tokens.HasNext && tokens.NextIs<TokenNumberLiteral>())
            {
                number = tokens.Next<TokenNumberLiteral>();
                if (number is TokenDecimalLiteral)
                    rx = new Coord(number.GetNumber(), true, true, false);
                else
                    rx = new Coord(number.GetNumberInt(), false, true, false);
            }

            executor.AddCommand(Command.Teleport(selector.ToString(), Coord.here, Coord.here, Coord.here, ry, rx));
        }
        public static void setblock(Executor executor, Statement tokens)
        {
            Coord x = tokens.Next<TokenCoordinateLiteral>();
            Coord y = tokens.Next<TokenCoordinateLiteral>();
            Coord z = tokens.Next<TokenCoordinateLiteral>();
            string block = tokens.Next<TokenStringLiteral>();
            OldHandling handling = OldHandling.replace;

            int data = 0;
            if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                data = tokens.Next<TokenIntegerLiteral>();

            if (tokens.NextIs<TokenIdentifierEnum>())
            {
                ParsedEnumValue enumValue = tokens.Next<TokenIdentifierEnum>().value;
                enumValue.RequireType<OldHandling>(tokens);
                handling = (OldHandling)enumValue.value;
            }

            executor.AddCommand(Command.SetBlock(x, y, z, block, data, handling));
        }
        public static void fill(Executor executor, Statement tokens)
        {
            OldHandling handling = OldHandling.replace;

            if (tokens.NextIs<TokenIdentifierEnum>())
            {
                ParsedEnumValue enumValue = tokens.Next<TokenIdentifierEnum>().value;
                enumValue.RequireType<OldHandling>(tokens);
                handling = (OldHandling)enumValue.value;
            }

            string block = tokens.Next<TokenStringLiteral>();
            Coord x1 = tokens.Next<TokenCoordinateLiteral>();
            Coord y1 = tokens.Next<TokenCoordinateLiteral>();
            Coord z1 = tokens.Next<TokenCoordinateLiteral>();
            Coord x2 = tokens.Next<TokenCoordinateLiteral>();
            Coord y2 = tokens.Next<TokenCoordinateLiteral>();
            Coord z2 = tokens.Next<TokenCoordinateLiteral>();

            int data = 0;
            if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                data = tokens.Next<TokenIntegerLiteral>();

            executor.AddCommand(Command.Fill(x1, y1, z1, x2, y2, z2, block, data, handling));
        }
        public static void scatter(Executor executor, Statement tokens)
        {
            string block = tokens.Next<TokenStringLiteral>();
            int percent = tokens.Next<TokenIntegerLiteral>();
            Coord x1 = tokens.Next<TokenCoordinateLiteral>();
            Coord y1 = tokens.Next<TokenCoordinateLiteral>();
            Coord z1 = tokens.Next<TokenCoordinateLiteral>();
            Coord x2 = tokens.Next<TokenCoordinateLiteral>();
            Coord y2 = tokens.Next<TokenCoordinateLiteral>();
            Coord z2 = tokens.Next<TokenCoordinateLiteral>();

            if (!Coord.SizeKnown(x1, y1, z1, x2, y2, z2))
                throw new StatementException(tokens, "Scatter command requires all coordinate arguments to be relative or exact. (the size needs to be known at compile time.)");

            string seed = null;
            if (tokens.HasNext && tokens.NextIs<TokenStringLiteral>())
                seed = tokens.Next<TokenStringLiteral>();

            // generate a structure file for this zone.
            int sizeX = Math.Abs(x2.valuei - x1.valuei) + 1;
            int sizeY = Math.Abs(y2.valuei - y1.valuei) + 1;
            int sizeZ = Math.Abs(z2.valuei - z1.valuei) + 1;

            long totalBlocks = ((long)sizeX) * ((long)sizeY) * ((long)sizeZ);

            if (totalBlocks > 1000000) // one million
                Executor.Warn("Warning: Scatter zone is " + totalBlocks + " blocks. This could cause extreme performance problems or the command may not even work at all.", tokens);

            if (executor.linting)
                return; // no need to run all that where it isn't even going to be used...

            int[,,] blocks = new int[sizeX, sizeY, sizeZ];
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    for (int z = 0; z < sizeZ; z++)
                        blocks[x, y, z] = 0;

            StructureNBT structure = new StructureNBT()
            {
                formatVersion = 1,
                size = new VectorIntNBT(sizeX, sizeY, sizeZ),
                worldOrigin = new VectorIntNBT(0, 0, 0),

                palette = new PaletteNBT(new PaletteEntryNBT(block)),
                entities = new EntityListNBT(new EntityNBT[0]),
                indices = new BlockIndicesNBT(blocks)
            };

            string fileName = "scatter_" + scatterFile++;
            StructureFile file = new StructureFile(fileName,
                Executor.MCC_GENERATED_FOLDER, structure);
            executor.project.WriteSingleFile(file);

            blocks = null;
            structure = default;

            Coord minX = Coord.Min(x1, x2);
            Coord minY = Coord.Min(y1, y2);
            Coord minZ = Coord.Min(z1, z2);

            if (seed == null)
            {
                executor.AddCommand(Command.StructureLoad(file.CommandReference, minX, minY, minZ,
                    StructureRotation._0_degrees, StructureMirror.none, false, true, false, percent));
            }
            else
            {
                executor.AddCommand(Command.StructureLoad(file.CommandReference, minX, minY, minZ,
                    StructureRotation._0_degrees, StructureMirror.none, false, true, false, percent, seed));
            }
        }
        public static void replace(Executor executor, Statement tokens)
        {
            string src = tokens.Next<TokenStringLiteral>();
            int srcData = -1;
            if (tokens.NextIs<TokenIntegerLiteral>())
                srcData = tokens.Next<TokenIntegerLiteral>();

            Coord x1 = tokens.Next<TokenCoordinateLiteral>();
            Coord y1 = tokens.Next<TokenCoordinateLiteral>();
            Coord z1 = tokens.Next<TokenCoordinateLiteral>();
            Coord x2 = tokens.Next<TokenCoordinateLiteral>();
            Coord y2 = tokens.Next<TokenCoordinateLiteral>();
            Coord z2 = tokens.Next<TokenCoordinateLiteral>();

            string dst = tokens.Next<TokenStringLiteral>();
            int dstData = -1;
            if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                dstData = tokens.Next<TokenIntegerLiteral>();

            executor.AddCommand(Command.Fill(x1, y1, z1, x2, y2, z2, src, srcData, dst, dstData));
        }
        public static void kill(Executor executor, Statement tokens)
        {
            Selector selector = Selector.SELF;

            if (tokens.NextIs<TokenSelectorLiteral>())
                selector = tokens.Next<TokenSelectorLiteral>();

            executor.AddCommand(Command.Kill(selector.ToString()));
        }
        public static void remove(Executor executor, Statement tokens)
        {
            var file = new CommandFile(true, "silent_remove", Executor.MCC_GENERATED_FOLDER);

            file.Add(new[] {
                Command.Teleport(Coord.here, new Coord(-99999, false, true, false), Coord.here),
                Command.Kill()
            });

            executor.DefineSTDFile(file);

            Selector selector = Selector.SELF;

            if (tokens.NextIs<TokenSelectorLiteral>())
                selector = tokens.Next<TokenSelectorLiteral>();

            executor.PushAlignSelector(ref selector);
            executor.AddCommand(Command.Function(file));
        }
        public static void globaltitle(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenIdentifier>(false))
            {
                string word = tokens.Next<TokenIdentifier>().word.ToUpper();
                if (word.Equals("TIMES"))
                {
                    int fadeIn = tokens.Next<TokenIntegerLiteral>();
                    int stay = tokens.Next<TokenIntegerLiteral>();
                    int fadeOut = tokens.Next<TokenIntegerLiteral>();
                    executor.AddCommand(Command.TitleTimes("@a", fadeIn, stay, fadeOut));
                    return;
                }
                else if (word.Equals("SUBTITLE"))
                {
                    string str = tokens.Next<TokenStringLiteral>();
                    List<JSONRawTerm> terms = executor.FString(str, tokens, out bool advanced);
                    string[] commands;

                    if (advanced)
                        commands = Command.Execute().As(Selector.ALL_PLAYERS).AtSelf().RunOver(Executor.ResolveRawText(terms, "titleraw @s subtitle "));
                    else
                        commands = Executor.ResolveRawText(terms, "titleraw @a subtitle ");

                    CommandFile file = executor.CurrentFile;
                    executor.AddCommands(commands, "subtitle", $"Called in a global-subtitle command located in {file.CommandReference} line {executor.NextLineNumber}");
                    return;
                }
                else
                    throw new StatementException(tokens, $"Invalid globaltitle subcommand '{word}'. Must be 'times' or 'subtitle'.");
            }

            if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                List<JSONRawTerm> terms = executor.FString(str, tokens, out bool advanced);
                string[] commands;

                if (advanced)
                    commands = Command.Execute().As(Selector.ALL_PLAYERS).AtSelf().RunOver(Executor.ResolveRawText(terms, "title @s title "));
                else
                    commands = Executor.ResolveRawText(terms, "titleraw @a title ");

                CommandFile file = executor.CurrentFile;
                executor.AddCommands(commands, "title", $"Called in a globaltitle command located in {file.CommandReference} line {executor.NextLineNumber}");
                return;
            }
        }
        public static void title(Executor executor, Statement tokens)
        {
            Selector player;

            if (tokens.NextIs<TokenSelectorLiteral>(false))
                player = tokens.Next<TokenSelectorLiteral>();
            else
                player = Selector.SELF;

            if (tokens.NextIs<TokenIdentifier>(false))
            {
                string word = tokens.Next<TokenIdentifier>().word.ToUpper();
                if (word.Equals("TIMES"))
                {
                    int fadeIn = tokens.Next<TokenIntegerLiteral>();
                    int stay = tokens.Next<TokenIntegerLiteral>();
                    int fadeOut = tokens.Next<TokenIntegerLiteral>();
                    executor.AddCommand(Command.TitleTimes(player.ToString(), fadeIn, stay, fadeOut));
                    return;
                }
                else if (word.Equals("SUBTITLE"))
                {
                    string str = tokens.Next<TokenStringLiteral>();
                    List<JSONRawTerm> terms = executor.FString(str, tokens, out bool _);
                    string[] commands;

                    commands = Executor.ResolveRawText(terms, $"titleraw {player} subtitle ");

                    CommandFile file = executor.CurrentFile;
                    executor.AddCommands(commands, "subtitle", $"Called in a subtitle command located in {file.CommandReference} line {executor.NextLineNumber}");
                    return;
                }
                else
                    throw new StatementException(tokens, $"Invalid title subcommand '{word}'. Must be 'times' or 'subtitle'.");
            }

            if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                List<JSONRawTerm> terms = executor.FString(str, tokens, out bool advanced);
                string[] commands;

                commands = Executor.ResolveRawText(terms, $"titleraw {player} title ");

                CommandFile file = executor.CurrentFile;
                executor.AddCommands(commands, "title", $"Called in a title command located in {file.CommandReference} line {executor.NextLineNumber}");
                return;
            }
        }
        public static void globalactionbar(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenIdentifier>(false))
            {
                string word = tokens.Next<TokenIdentifier>().word.ToUpper();
                int fadeIn = tokens.Next<TokenIntegerLiteral>();
                int stay = tokens.Next<TokenIntegerLiteral>();
                int fadeOut = tokens.Next<TokenIntegerLiteral>();
                executor.AddCommand(Command.TitleTimes("@a", fadeIn, stay, fadeOut));
                return;
            }
            else if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                List<JSONRawTerm> terms = executor.FString(str, tokens, out bool advanced);
                string[] commands;

                if (advanced)
                    commands = Command.Execute().As(Selector.ALL_PLAYERS).AtSelf().RunOver(Executor.ResolveRawText(terms, "titleraw @s actionbar "));
                else
                    commands = Executor.ResolveRawText(terms, "titleraw @a actionbar ");

                CommandFile file = executor.CurrentFile;
                executor.AddCommands(commands, "actionbar", $"Called in a global-actionbar command located in {file.CommandReference} line {executor.NextLineNumber}");
                return;
            }
            else throw new StatementException(tokens, "Invalid information given to globalactionbar.");
        }
        public static void actionbar(Executor executor, Statement tokens)
        {
            Selector player;

            if (tokens.NextIs<TokenSelectorLiteral>(false))
                player = tokens.Next<TokenSelectorLiteral>();
            else
                player = Selector.SELF;

            if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                List<JSONRawTerm> terms = executor.FString(str, tokens, out bool _);
                string[] commands;

                commands = Executor.ResolveRawText(terms, $"titleraw {player} actionbar ");

                CommandFile file = executor.CurrentFile;
                executor.AddCommands(commands, "actionbar", $"Called in an actionbar command located in {file.CommandReference} line {executor.NextLineNumber}");
                return;
            }
            else throw new StatementException(tokens, "Invalid information given to actionbar command.");
        }
        public static void say(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            executor.AddCommand(Command.Say(str));
        }
        public static void halt(Executor executor, Statement tokens)
        {
            var file = new CommandFile(true, "halt_execution", Executor.MCC_GENERATED_FOLDER);

            if (!executor.HasSTDFile(file))
            {
                // recursively call self until function command limit reached
                file.Add(Command.Function(file));
                executor.DefineSTDFile(file);
            }

            executor.UnreachableCode();
            executor.AddCommand(Command.Function(file));
        }
        public static void damage(Executor executor, Statement tokens)
        {
            Selector target = tokens.Next<TokenSelectorLiteral>();

            int damage = tokens.Next<TokenIntegerLiteral>();
            DamageCause cause = DamageCause.all;
            Selector blame = null;

            if(tokens.NextIs<TokenIdentifierEnum>())
            {
                TokenIdentifierEnum idEnum = tokens.Next<TokenIdentifierEnum>();
                idEnum.value.RequireType<DamageCause>(tokens);
                cause = (DamageCause)idEnum.value.value;
            }

            if (tokens.NextIs<TokenSelectorLiteral>())
            {
                TokenSelectorLiteral value = tokens.Next<TokenSelectorLiteral>();
                blame = value.selector;
            }
            else if(tokens.NextIs<TokenCoordinateLiteral>())
            {
                // spawn dummy entity
                Coord x = tokens.Next<TokenCoordinateLiteral>();
                Coord y = tokens.Next<TokenCoordinateLiteral>();
                Coord z = tokens.Next<TokenCoordinateLiteral>();

                executor.RequireFeature(tokens, Feature.DUMMIES);
                const string damagerEntity = "_dmg_from";
                string[] commands = new string[]
                {
                    // create dummy entity at location
                    executor.entities.dummies.Create(damagerEntity, false, x, y, z),

                    // hit entity from dummy entity
                    Command.Damage(target.ToString(), damage, cause,
                        executor.entities.dummies.GetStringSelector(damagerEntity, false)),

                    // send kill event to dummy entity
                    executor.entities.dummies.Destroy(damagerEntity, false)
                };

                CommandFile file = executor.CurrentFile;
                executor.AddCommands(commands, "damagefrom", $"Creates a dummy entity and uses it to attack the entity from the location ({x} {y} {z}). {file.CommandReference} line {executor.NextLineNumber}");
                return;
            }

            string command;
            if (blame == null)
                command = Command.Damage(target.ToString(), damage, cause);
            else
            {
                if (blame.SelectsMultiple)
                    blame.count = new Count(1);
                command = Command.Damage(target.ToString(), damage, cause, blame.ToString());
            }

            executor.AddCommand(command);
        }
        public static void dummy(Executor executor, Statement tokens)
        {
            executor.RequireFeature(tokens, Feature.DUMMIES);

            string word = tokens.Next<TokenIdentifier>().word.ToUpper();
            string name = tokens.Next<TokenStringLiteral>();

            string tag = null;
            if (tokens.NextIs<TokenStringLiteral>())
                tag = tokens.Next<TokenStringLiteral>();

            if (word.Equals("CREATE"))
            {
                Coord x = Coord.here;
                Coord y = Coord.here;
                Coord z = Coord.here;

                if (tokens.NextIs<TokenCoordinateLiteral>())
                    x = tokens.Next<TokenCoordinateLiteral>();
                if (tokens.NextIs<TokenCoordinateLiteral>())
                    y = tokens.Next<TokenCoordinateLiteral>();
                if (tokens.NextIs<TokenCoordinateLiteral>())
                    z = tokens.Next<TokenCoordinateLiteral>();

                if (tag == null)
                {
                    string command = executor.entities.dummies.Create(name, false, x, y, z);
                    executor.AddCommand(command);
                }
                else
                {
                    string selector = executor.entities.dummies.GetStringSelector(name, true);
                    string[] commands = new string[]
                    {
                        executor.entities.dummies.Create(name, true, x, y, z),
                        Command.Tag(selector, tag),
                        Command.Event(selector, DummyManager.TAGGABLE_EVENT_REMOVE_NAME)
                    };

                    CommandFile file = executor.CurrentFile;
                    executor.AddCommands(commands, "createDummy", $"Spawns a dummy entity named '{name}' with the tag {tag} at ({x} {y} {z}). {file.CommandReference} line {executor.NextLineNumber}");
                }
                return;
            }
            else if (word.Equals("SINGLE"))
            {
                Coord x = Coord.here;
                Coord y = Coord.here;
                Coord z = Coord.here;

                if (tokens.NextIs<TokenCoordinateLiteral>())
                    x = tokens.Next<TokenCoordinateLiteral>();
                if (tokens.NextIs<TokenCoordinateLiteral>())
                    y = tokens.Next<TokenCoordinateLiteral>();
                if (tokens.NextIs<TokenCoordinateLiteral>())
                    z = tokens.Next<TokenCoordinateLiteral>();

                CommandFile file = executor.CurrentFile;

                if (tag == null)
                {
                    executor.AddCommands(new string[]
                    {
                        executor.entities.dummies.Destroy(name, false),
                        executor.entities.dummies.Create(name, false, x, y, z)
                    }, "singletonDummy", $"Spawns a singleton dummy entity named '{name}' at ({x} {y} {z}). {file.CommandReference} line {executor.NextLineNumber}");
                }
                else
                {
                    string selector = executor.entities.dummies.GetStringSelector(name, true);
                    executor.AddCommands(new string[]
                    {
                        executor.entities.dummies.Destroy(name, false, tag),
                        executor.entities.dummies.Create(name, true, x, y, z),
                        Command.Tag(selector, tag),
                        Command.Event(selector, DummyManager.TAGGABLE_EVENT_REMOVE_NAME)
                    }, "singletonDummy", $"Spawns a singleton dummy entity named '{name}' with the tag {tag} at ({x} {y} {z}). {file.CommandReference} line {executor.NextLineNumber}");
                }
                return;
            }
            else if (word.Equals("REMOVE"))
            {
                executor.AddCommand(executor.entities.dummies.Destroy(name, false, tag));
                return;
            }
            else
                throw new StatementException(tokens, $"Invalid mode for dummy command: {word}. Valid options are CREATE, SINGLE, or REMOVE");
        }
        public static void tag(Executor executor, Statement tokens)
        {
            string selected = tokens.Next<TokenSelectorLiteral>().selector.ToString();
            string word = tokens.Next<TokenIdentifier>().word.ToUpper();

            if (word.Equals("ADD"))
            {
                string tag = tokens.Next<TokenStringLiteral>();
                executor.definedTags.Add(tag);
                executor.AddCommand(Command.Tag(selected, tag));
            } else if (word.Equals("REMOVE"))
            {
                string tag = tokens.Next<TokenStringLiteral>();
                executor.AddCommand(Command.TagRemove(selected, tag));
            }
            else
                throw new StatementException(tokens, $"Invalid mode for tag command: {word}. Valid options are ADD, REMOVE");
        }
        public static void explode(Executor executor, Statement tokens)
        {
            executor.RequireFeature(tokens, Feature.EXPLODERS);

            Coord x, y, z;

            if(tokens.NextIs<TokenCoordinateLiteral>())
            {
                x = tokens.Next<TokenCoordinateLiteral>();
                y = tokens.Next<TokenCoordinateLiteral>();
                z = tokens.Next<TokenCoordinateLiteral>();
            } 
            else
            {
                x = Coord.here;
                y = Coord.here;
                z = Coord.here;
            }

            int power, delay;
            bool fire, breaks;

            // Get the first two integers
            if (tokens.NextIs<TokenIntegerLiteral>())
            {
                power = tokens.Next<TokenIntegerLiteral>();

                if (tokens.NextIs<TokenIntegerLiteral>())
                    delay = tokens.Next<TokenIntegerLiteral>();
                else
                    delay = 0;
            }
            else
            {
                power = 3;
                delay = 0;
            }

            // The two booleans
            if (tokens.NextIs<TokenBooleanLiteral>())
            {
                fire = tokens.Next<TokenBooleanLiteral>();

                if (tokens.NextIs<TokenBooleanLiteral>())
                    breaks = tokens.Next<TokenBooleanLiteral>();
                else
                    breaks = true;
            }
            else
            {
                fire = false;
                breaks = true;
            }

            string command = executor.entities.exploders.CreateExplosion(x, y, z, power, delay, fire, breaks);
            executor.AddCommand(command);
        }
        public static void clear(Executor executor, Statement tokens)
        {
            string selector;
            string command;

            if (tokens.NextIs<TokenSelectorLiteral>())
                selector = tokens.Next<TokenSelectorLiteral>().selector.ToString();
            else
                selector = null;

            if (selector != null)
            {
                if (tokens.NextIs<TokenStringLiteral>())
                {
                    string item = tokens.Next<TokenStringLiteral>();

                    if (tokens.NextIs<TokenIntegerLiteral>())
                    {
                        int data = tokens.Next<TokenIntegerLiteral>();
                        if (tokens.NextIs<TokenIntegerLiteral>())
                        {
                            int maxCount = tokens.Next<TokenIntegerLiteral>();
                            command = Command.Clear(selector, item, data, maxCount);
                        }
                        else
                            command = Command.Clear(selector, item, data);
                    }
                    else
                        command = Command.Clear(selector, item);
                }
                else
                    command = Command.Clear(selector);
            } else
                command = Command.Clear();

            executor.AddCommand(command);
        }
        public static void effect(Executor executor, Statement tokens)
        {
            string selector = tokens.Next<TokenSelectorLiteral>().selector.ToString();

            if(!tokens.NextIs<TokenIdentifierEnum>())
            {
                string word = tokens.Next<TokenIdentifier>().word.ToUpper();

                if(word.Equals("CLEAR"))
                {
                    executor.AddCommand(Command.EffectClear(selector));
                    return;
                }

                if(word.StartsWith("C"))
                    throw new StatementException(tokens, "Invalid option for effect command. (did you mean 'clear'?)");
                else
                    throw new StatementException(tokens, "Invalid option for effect command.");
            }

            string command;
            TokenIdentifierEnum effectToken = tokens.Next<TokenIdentifierEnum>();
            ParsedEnumValue parsedEffect = effectToken.value;
            parsedEffect.RequireType<PotionEffect>(tokens);
            PotionEffect effect = (PotionEffect)parsedEffect.value;

            if (tokens.NextIs<TokenIntegerLiteral>())
            {
                int seconds = tokens.Next<TokenIntegerLiteral>().Scaled(IntMultiplier.s);

                if (tokens.NextIs<TokenIntegerLiteral>())
                {
                    int amplifier = tokens.Next<TokenIntegerLiteral>();

                    if(tokens.NextIs<TokenBooleanLiteral>())
                    {
                        bool hideParticles = tokens.Next<TokenBooleanLiteral>();
                        command = Command.Effect(selector, effect, seconds, amplifier, hideParticles);
                    }
                    else
                        command = Command.Effect(selector, effect, seconds, amplifier);
                }
                else
                    command = Command.Effect(selector, effect, seconds);
            }
            else
                command = Command.Effect(selector, effect);

            executor.AddCommand(command);
            return;
        }
        public static void playsound(Executor executor, Statement tokens)
        {
            string soundId = tokens.Next<TokenStringLiteral>();

            if (!tokens.HasNext)
            {
                executor.AddCommand(Command.PlaySound(soundId));
                return;
            }

            Selector filter = tokens.Next<TokenSelectorLiteral>();

            if(!tokens.NextIs<TokenCoordinateLiteral>())
            {
                executor.AddCommand(Command.PlaySound(soundId, filter.ToString()));
                return;
            }

            Coord x = tokens.Next<TokenCoordinateLiteral>();
            Coord y = tokens.Next<TokenCoordinateLiteral>();
            Coord z = tokens.Next<TokenCoordinateLiteral>();

            if(!tokens.NextIs<TokenNumberLiteral>())
            {
                executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z));
                return;
            }

            float volume = tokens.Next<TokenNumberLiteral>().GetNumber();

            if (!tokens.NextIs<TokenNumberLiteral>())
            {
                executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z, volume));
                return;
            }

            float pitch = tokens.Next<TokenNumberLiteral>().GetNumber();

            if (!tokens.NextIs<TokenNumberLiteral>())
            {
                executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z, volume, pitch));
                return;
            }

            float minVolume = tokens.Next<TokenNumberLiteral>().GetNumber();
            executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z, volume, pitch, minVolume));
            return;
        }
        public static void particle(Executor executor, Statement tokens)
        {
            string particleId = tokens.Next<TokenStringLiteral>();

            if(tokens.NextIs<TokenCoordinateLiteral>())
            {
                Coord x = tokens.Next<TokenCoordinateLiteral>();
                Coord y = tokens.Next<TokenCoordinateLiteral>();
                Coord z = tokens.Next<TokenCoordinateLiteral>();
                executor.AddCommand(Command.Particle(particleId, x, y, z));
                return;
            }

            executor.AddCommand(Command.Particle(particleId,
                Coord.here, Coord.here, Coord.here));
        }

        public static void execute(Executor executor, Statement tokens)
        {
            ExecuteBuilder builder = new ExecuteBuilder();

            do
            {
                string _subcommand = tokens.Next<TokenIdentifier>().word.ToUpper();
                Subcommand subcommand = Subcommand.GetSubcommandForKeyword(_subcommand, tokens);

                if (subcommand.TerminatesChain)
                    throw new StatementException(tokens, $"Subcommand '{_subcommand}' is not allowed here as it terminates the chain.");

                // match subcommand pattern now, if any
                TypePattern[] patterns = subcommand.Patterns;
                if (patterns != null && patterns.Length > 0)
                {
                    IEnumerable<MatchResult> results = patterns.Select(pattern => pattern.Check(tokens.GetRemainingTokens()));

                    if (results.All(result => !result.match))
                    {
                        // get the closest matched pattern
                        MatchResult closest = results.Aggregate((a, b) => a.accuracy > b.accuracy ? a : b);
                        var missingArgs = closest.missing.Select(m => m.ToString());
                        throw new StatementException(tokens, "Subcommand - Missing argument(s): " + string.Join(", ", missingArgs));
                    }
                }

                // parse from tokens
                subcommand.FromTokens(tokens);

                // add to builder
                builder.WithSubcommand(subcommand);

            } while(tokens.NextIs<TokenIdentifier>(false));

            // --- find statement or code-block nabbed from Comparison::Run ---

            if (!executor.HasNext)
                throw new StatementException(tokens, "Unexpected end of file when running comparison.");

            Statement next = executor.Seek();

            if (next is StatementOpenBlock openBlock)
            {
                // only do the block stuff if necessary.
                if (openBlock.meaningfulStatementsInside == 0)
                {
                    openBlock.openAction = null;
                    openBlock.CloseAction = null;
                    return; // do nothing
                }

                string finalExecute = builder
                    .WithSubcommand(new SubcommandRun())
                    .Build(out _);

                if (openBlock.meaningfulStatementsInside == 1)
                {
                    // modify prepend buffer as if 1 statement was there
                    executor.AppendCommandPrepend(finalExecute);
                    openBlock.openAction = null;
                    openBlock.CloseAction = null;
                }
                else
                {
                    CommandFile blockFile = Executor.GetNextGeneratedFile("execute");

                    if(Program.DECORATE)
                    {
                        CommandFile file = executor.CurrentFile;
                        string subcommandsString = builder.BuildClean(out _);
                        blockFile.Add($"# Run under the following execute subcommands: [{subcommandsString}]");
                        blockFile.AddTrace(file);
                    }

                    string command = finalExecute + Command.Function(blockFile);
                    executor.AddCommand(command);

                    openBlock.openAction = (e) =>
                    {
                        e.PushFile(blockFile);
                    };
                    openBlock.CloseAction = (e) =>
                    {
                        e.PopFile();
                    };
                }
            }
            else
            {
                string finalExecute = builder
                    .WithSubcommand(new SubcommandRun())
                    .Build(out _);
                executor.AppendCommandPrepend(finalExecute);
            }
        }
        public static void feature(Executor executor, Statement tokens)
        {
            string featureStr = tokens.Next<TokenIdentifier>().word.ToUpper();
            Feature feature = Feature.NO_FEATURES;

            foreach(Feature possibleFeature in FeatureManager.FEATURE_LIST)
            {
                if (featureStr.Equals(possibleFeature.ToString().ToUpper()))
                {
                    feature = possibleFeature;
                    break;
                }
            }

            if (feature == Feature.NO_FEATURES)
                throw new StatementException(tokens, "No valid feature specified.");

            executor.project.EnableFeature(feature);
            FeatureManager.OnFeatureEnabled(executor, feature);

            if (Program.DEBUG && !executor.project.linting)
                Console.WriteLine("Feature enabled: {0}", feature);
        }
        public static void function(Executor executor, Statement tokens)
        {
            // pull attributes
            List<IAttribute> attributes = new List<IAttribute>();

            void FindAttributes()
            {
                while (tokens.NextIs<TokenAttribute>())
                {
                    var _attribute = tokens.Next<TokenAttribute>();
                    attributes.Add(_attribute.attribute);
                }
            }

            FindAttributes();

            // normal definition
            string functionName = tokens.Next<TokenIdentifier>().word;
            bool usesFolders = functionName.Contains('.');
            string[] folders = null;

            if (usesFolders)
            {
                var split = functionName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 1)
                    folders = split.Take(split.Length - 1).ToArray();
                else
                {
                    usesFolders = false; // user wrote beyond shit code; fix it up for them
                    functionName = functionName.Trim('.');
                }
            }

            List<RuntimeFunctionParameter> parameters = new List<RuntimeFunctionParameter>();

            FindAttributes();

            if (tokens.NextIs<TokenOpenParenthesis>())
                tokens.Next();

            bool hasBegunOptionals = false;

            // this is where the directive takes in function parameters
            while (tokens.HasNext && tokens.NextIs<TokenIdentifier>(false))
            {
                // fetch a parameter definition
                ScoreboardManager.ValueDefinition def = executor.scoreboard.GetNextValueDefinition(tokens);

                // don't let users define non-optional parameters if they already specified one.
                if (def.defaultValue == null && hasBegunOptionals)
                    throw new StatementException(tokens, "All parameters proceeding an optional parameter must also be optional.");
                if(def.defaultValue != null)
                    hasBegunOptionals = true;
                
                ScoreboardValue value = def.Create(executor.scoreboard, tokens);
                value.clarifier.IsGlobal = true;

                executor.scoreboard.TryThrowForDuplicate(value, tokens);
                executor.scoreboard.Add(value);

                parameters.Add(new RuntimeFunctionParameter(value, def.defaultValue));
            }

            // see if last statement was a comment, and use that for documentation
            string docs = executor.GetDocumentationString(out bool hadDocumentation);

            // the actual name of the function file
            string actualName = usesFolders ? functionName.Substring(functionName.LastIndexOf('.') + 1) : functionName;

            // constructor
            var function = new RuntimeFunction(functionName, actualName, docs, attributes.ToArray(), false)
            {
                documentation = docs,
                isAddedToExecutor = true
            };
            
            function.AddParameters(parameters);
            function.SignalToAttributes(tokens);

            if (!function.isExtern)
            {
                // force hash the parameters so that they can be unique.
                foreach (RuntimeFunctionParameter parameter in parameters)
                    parameter.RuntimeDestination.ForceHash(functionName);

                // add decoration to it if documentation was given
                if (hadDocumentation && Program.DECORATE)
                {
                    function.AddCommands(docs.Trim().Split('\n').Select(str => "# " + str.Trim()));
                    function.AddCommand("");
                }
            }

            // folders, if specified via dots
            if (usesFolders)
                function.file.Folders = folders;

            // register it with the compiler
            executor.functions.RegisterFunction(function);

            // get the function's parameters
            var allRuntimeDestinations = function.Parameters
                .Where(p => p is RuntimeFunctionParameter)
                .Select(p => ((RuntimeFunctionParameter)p).RuntimeDestination);

            // ...and define them
            foreach(ScoreboardValue runtimeDestination in allRuntimeDestinations)
                executor.AddCommandsInit(runtimeDestination.CommandsDefine());

            if (executor.NextIs<StatementOpenBlock>())
            {
                if (function.isExtern)
                    throw new StatementException(tokens, "Extern functions cannot have a body.");

                StatementOpenBlock openBlock = executor.Peek<StatementOpenBlock>();

                openBlock.openAction = (e) =>
                {
                    e.PushFile(function.file);
                };
                openBlock.CloseAction = (e) =>
                {
                    e.PopFile();
                };
                return;
            }
            else if(!function.isExtern)
                throw new StatementException(tokens, "No block following function definition.");
        }
        public static void @return(Executor executor, Statement tokens)
        {
            RuntimeFunction activeFunction = executor.CurrentFile.runtimeFunction;

            if (activeFunction == null)
                throw new StatementException(tokens, "Cannot return a value outside of a function.");

            if (tokens.NextIs<TokenIdentifierValue>())
            {
                var token = tokens.Next<TokenIdentifierValue>();
                activeFunction.TryReturnValue(token.value, executor, tokens);
            }
            else
            {
                var token = tokens.Next<TokenLiteral>();
                activeFunction.TryReturnValue(token, tokens, executor);
            }
        }
        public static void @for(Executor executor, Statement tokens)
        {
            var selector = tokens.Next<TokenSelectorLiteral>();
            
            Coord x = Coord.here,
                  y = Coord.here,
                  z = Coord.here;

            if(tokens.NextIs<TokenIdentifier>(false))
            {
                string identifier = tokens.Next<TokenIdentifier>().word;

                if(identifier.ToUpper().Equals("AT"))
                {
                    if (tokens.NextIs<TokenCoordinateLiteral>())
                        x = tokens.Next<TokenCoordinateLiteral>();
                    if (tokens.NextIs<TokenCoordinateLiteral>())
                        y = tokens.Next<TokenCoordinateLiteral>();
                    if (tokens.NextIs<TokenCoordinateLiteral>())
                        z = tokens.Next<TokenCoordinateLiteral>();
                }
            }

            if (!executor.HasNext)
                throw new StatementException(tokens, "Unexpected end-of-file after for-statement.");

            if(executor.NextIs<StatementOpenBlock>())
            {
                StatementOpenBlock block = executor.Peek<StatementOpenBlock>();
                CommandFile file = Executor.GetNextGeneratedFile("for");

                if(Program.DECORATE)
                {
                    if(x.HasEffect || y.HasEffect || z.HasEffect)
                        file.Add($"# Run for every entity that matches {selector}, and is run at ({x} {y} {z}).");
                    else
                        file.Add($"# Run for every entity that matches {selector}.");

                    file.AddTrace(executor.CurrentFile);
                }

                string command = Command.Execute()
                    .As(selector)
                    .AtSelf()
                    .Positioned(x, y, z)
                    .Run(Command.Function(file));

                executor.AddCommand(command);

                block.openAction = (e) =>
                {
                    e.PushFile(file);
                };
                block.CloseAction = (e) =>
                {
                    e.PopFile();
                };
            }
            else
            {
                string commandPrefix = Command.Execute()
                    .As(selector)
                    .AtSelf()
                    .Positioned(x, y, z)
                    .Run();
                executor.AppendCommandPrepend(commandPrefix);
            }
        }
    }
}