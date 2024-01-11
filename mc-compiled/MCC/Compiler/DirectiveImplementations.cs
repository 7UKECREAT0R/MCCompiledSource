using mc_compiled.Commands;
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
using JetBrains.Annotations;
using Microsoft.CSharp.RuntimeBinder;
// ReSharper disable IdentifierTypo

namespace mc_compiled.MCC.Compiler
{
    public static class DirectiveImplementations
    {
        public static void ResetState()
        {
            scatterFile = 0;
        }
        private static int scatterFile = 0;

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        /// <summary>
        /// Gathers either a preprocessor variable's values, or a list of dynamic values in the source file.
        /// </summary>
        /// <param name="executor">The executor object used for executing statements.</param>
        /// <param name="tokens">The statement tokens.</param>
        /// <param name="forcePPV">Optional. The name of the preprocessor variable to force fetch rather than pull from `tokens`.</param>
        /// <returns>An array of dynamic values.</returns>
        /// <exception cref="StatementException">Thrown when a preprocessor variable with the provided identifier is not found.</exception>
        private static dynamic[] FetchPPVOrDynamics(Executor executor, Statement tokens, string forcePPV = null)
        {
            if (forcePPV != null)
            {
                if (executor.TryGetPPV(forcePPV, out PreprocessorVariable ppv))
                {
                    return ppv.ToArray();
                }

                throw new StatementException(tokens, $"Couldn't find preprocessor variable named '{forcePPV}'.");
            }

            var token = tokens.Next<IPreprocessor>();
            if (tokens.NextIs<TokenIdentifier>(false))
            {
                var identifier = tokens.Next<TokenIdentifier>();
                if (executor.TryGetPPV(identifier.word, out PreprocessorVariable ppv))
                {
                    return ppv.ToArray();
                }
                throw new StatementException(tokens, $"Couldn't find preprocessor variable named '{identifier.word}'.");
            }
            
            var others = new List<dynamic> {token.GetValue()};
            while (tokens.NextIs<IPreprocessor>())
            {
                others.Add(tokens.Next<IPreprocessor>().GetValue());
            }

            return others.ToArray();
        }
        
        [UsedImplicitly]
        public static void _var(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            var values = new List<dynamic>();
            while (tokens.NextIs<IPreprocessor>())
                values.Add(tokens.Next<IPreprocessor>().GetValue());

            executor.SetPPV(varName, values.ToArray());
        }
        [UsedImplicitly]
        public static void _inc(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            if (executor.TryGetPPV(varName, out PreprocessorVariable value))
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
        [UsedImplicitly]
        public static void _dec(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            if (executor.TryGetPPV(varName, out PreprocessorVariable value))
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
        
        [UsedImplicitly]
        public static void _add(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            dynamic[] a = FetchPPVOrDynamics(executor, tokens, varName);
            dynamic[] b = FetchPPVOrDynamics(executor, tokens);

            int bIndex = 0;
            dynamic[] result = new dynamic[a.Length];
            
            for (int aIndex = 0; aIndex < a.Length; aIndex++)
            {
                dynamic aValue = a[aIndex];
                bIndex %= b.Length;
                dynamic bValue = b[bIndex++];
                
                try
                {
                    result[aIndex] = aValue + bValue;
                }
                catch (RuntimeBinderException)
                {
                    throw new StatementException(tokens, $"Couldn't add these values: {aValue} + {bValue}");
                }
            }
            executor.SetPPV(varName, result);
        }
        [UsedImplicitly]
        public static void _sub(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            dynamic[] a = FetchPPVOrDynamics(executor, tokens, varName);
            dynamic[] b = FetchPPVOrDynamics(executor, tokens);

            int bIndex = 0;
            dynamic[] result = new dynamic[a.Length];
            
            for (int aIndex = 0; aIndex < a.Length; aIndex++)
            {
                dynamic aValue = a[aIndex];
                bIndex %= b.Length;
                dynamic bValue = b[bIndex++];
                
                try
                {
                    result[aIndex] = aValue - bValue;
                }
                catch (RuntimeBinderException)
                {
                    throw new StatementException(tokens, $"Couldn't add these values: {aValue} + {bValue}");
                }
            }
            executor.SetPPV(varName, result);
        }
        [UsedImplicitly]
        public static void _mul(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            dynamic[] a = FetchPPVOrDynamics(executor, tokens, varName);
            dynamic[] b = FetchPPVOrDynamics(executor, tokens);
            
            int bIndex = 0;
            dynamic[] result = new dynamic[a.Length];
            
            for (int aIndex = 0; aIndex < a.Length; aIndex++)
            {
                dynamic aValue = a[aIndex];
                bIndex %= b.Length;
                dynamic bValue = b[bIndex++];
                
                try
                {
                    result[aIndex] = aValue * bValue;
                }
                catch (RuntimeBinderException)
                {
                    throw new StatementException(tokens, $"Couldn't multiply these values: {aValue} * {bValue}");
                }
            }
            executor.SetPPV(varName, result);
        }
        [UsedImplicitly]
        public static void _div(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            dynamic[] a = FetchPPVOrDynamics(executor, tokens, varName);
            dynamic[] b = FetchPPVOrDynamics(executor, tokens);

            int bIndex = 0;
            dynamic[] result = new dynamic[a.Length];
            
            for (int aIndex = 0; aIndex < a.Length; aIndex++)
            {
                dynamic aValue = a[aIndex];
                bIndex %= b.Length;
                dynamic bValue = b[bIndex++];
                
                try
                {
                    result[aIndex] = aValue / bValue;
                }
                catch (RuntimeBinderException)
                {
                    throw new StatementException(tokens, $"Couldn't divide these values: {aValue} / {bValue}");
                }
            }
            executor.SetPPV(varName, result);
        }
        [UsedImplicitly]
        public static void _mod(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            dynamic[] a = FetchPPVOrDynamics(executor, tokens, varName);
            dynamic[] b = FetchPPVOrDynamics(executor, tokens);
            
            int bIndex = 0;
            dynamic[] result = new dynamic[a.Length];
            
            for (int aIndex = 0; aIndex < a.Length; aIndex++)
            {
                dynamic aValue = a[aIndex];
                bIndex %= b.Length;
                dynamic bValue = b[bIndex++];
                
                try
                {
                    result[aIndex] = aValue % bValue;
                }
                catch (RuntimeBinderException)
                {
                    throw new StatementException(tokens, $"Couldn't mod these values: {aValue} % {bValue}");
                }
            }
            executor.SetPPV(varName, result);
        }
        [UsedImplicitly]
        public static void _pow(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            dynamic[] a = FetchPPVOrDynamics(executor, tokens, varName);
            dynamic[] b = FetchPPVOrDynamics(executor, tokens);
            
            int bIndex = 0;
            dynamic[] result = new dynamic[a.Length];
            
            for (int aIndex = 0; aIndex < a.Length; aIndex++)
            {
                dynamic aValue = a[aIndex];
                bIndex %= b.Length;
                dynamic bValue = b[bIndex++];
                
                try
                {
                    result[aIndex] = Math.Pow(aValue, bValue);
                }
                catch (RuntimeBinderException)
                {
                    throw new StatementException(tokens, $"Couldn't raise {aValue} to the power of {bValue}");
                }
            }
            executor.SetPPV(varName, result);
        }
        [UsedImplicitly]
        public static void _swap(Executor executor, Statement tokens)
        {
            string aName = tokens.Next<TokenIdentifier>().word;
            string bName = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(aName, out PreprocessorVariable a))
            {
                if (executor.TryGetPPV(bName, out PreprocessorVariable b))
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
        [UsedImplicitly]
        public static void _if(Executor executor, Statement tokens)
        {
            PreprocessorVariable tokensA, tokensB;

            if (tokens.NextIs<TokenIdentifier>(false))
            {
                string varName = tokens.Next<TokenIdentifier>().word;
                if (!executor.TryGetPPV(varName, out tokensA))
                    throw new StatementException(tokens, $"Preprocessor variable '{varName}' does not exist.");
            }
            else
            {
                var firstToken = tokens.Next<IPreprocessor>();
                tokensA = new PreprocessorVariable { firstToken.GetValue() };
            }

            var compare = tokens.Next<TokenCompare>();

            if (tokens.NextIs<TokenIdentifier>(false))
            {
                string varName = tokens.Next<TokenIdentifier>().word;
                if (!executor.TryGetPPV(varName, out tokensB))
                    throw new StatementException(tokens, $"Preprocessor variable '{varName}' does not exist.");
            }
            else
            {
                var secondToken = tokens.Next<IPreprocessor>();
                tokensB = new PreprocessorVariable { secondToken.GetValue() };
            }

            if(tokensA.Length != tokensB.Length)
                throw new StatementException(tokens, "Preprocessor variable lengths didn't match.");

            // if the next block/statement should be run
            bool result = true;

            for (int i = 0; i < tokensA.Length; i++)
            {
                dynamic a = tokensA[i];
                dynamic b = tokensB[i];

                try
                {
                    switch (compare.GetCompareType())
                    {
                        case TokenCompare.Type.EQUAL:
                            result &= a == b;
                            break;
                        case TokenCompare.Type.NOT_EQUAL:
                            result &= a != b;
                            break;
                        case TokenCompare.Type.LESS:
                            result &= a < b;
                            break;
                        case TokenCompare.Type.LESS_OR_EQUAL:
                            result &= a <= b;
                            break;
                        case TokenCompare.Type.GREATER:
                            result &= a > b;
                            break;
                        case TokenCompare.Type.GREATER_OR_EQUAL:
                            result &= a >= b;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Could not compare those two types.");
                }
            }

            if (!executor.HasNext)
                throw new StatementException(tokens, "End of file after $if statement.");

            executor.SetLastIfResult(result);

            if (executor.NextIs<StatementOpenBlock>())
            {
                var block = executor.Peek<StatementOpenBlock>();

                if (result)
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
            
            if (!result)
                executor.Next(); // skip the next statement
        }
        [UsedImplicitly]
        public static void _else(Executor executor, Statement tokens)
        {
            bool run = !executor.GetLastIfResult();

            if (executor.NextIs<StatementOpenBlock>())
            {
                var block = executor.Peek<StatementOpenBlock>();
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
            }
            else if (!run)
                executor.Next(); // skip the next statement
        }
        [UsedImplicitly]
        public static void _assert(Executor executor, Statement tokens)
        {
            PreprocessorVariable tokensA, tokensB;

            if (tokens.NextIs<TokenIdentifier>(false))
            {
                string varName = tokens.Next<TokenIdentifier>().word;
                if (!executor.TryGetPPV(varName, out tokensA))
                    throw new StatementException(tokens, $"Preprocessor variable '{varName}' does not exist.");
            }
            else
            {
                var firstToken = tokens.Next<IPreprocessor>();
                tokensA = new PreprocessorVariable { firstToken.GetValue() };
            }

            var compare = tokens.Next<TokenCompare>();

            if (tokens.NextIs<TokenIdentifier>(false))
            {
                string varName = tokens.Next<TokenIdentifier>().word;
                if (!executor.TryGetPPV(varName, out tokensB))
                    throw new StatementException(tokens, $"Preprocessor variable '{varName}' does not exist.");
            }
            else
            {
                var secondToken = tokens.Next<IPreprocessor>();
                tokensB = new PreprocessorVariable { secondToken.GetValue() };
            }

            if (tokensA.Length != tokensB.Length)
                throw new StatementException(tokens, "Preprocessor variable lengths didn't match.");

            // if the next block/statement should be run
            bool result = true;

            for (int i = 0; i < tokensA.Length; i++)
            {
                dynamic a = tokensA[i];
                dynamic b = tokensB[i];

                try
                {
                    switch (compare.GetCompareType())
                    {
                        case TokenCompare.Type.EQUAL:
                            result &= a == b;
                            break;
                        case TokenCompare.Type.NOT_EQUAL:
                            result &= a != b;
                            break;
                        case TokenCompare.Type.LESS:
                            result &= a < b;
                            break;
                        case TokenCompare.Type.LESS_OR_EQUAL:
                            result &= a <= b;
                            break;
                        case TokenCompare.Type.GREATER:
                            result &= a > b;
                            break;
                        case TokenCompare.Type.GREATER_OR_EQUAL:
                            result &= a >= b;
                            break;
                    }
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Could not compare those two types.");
                }
            }

            if (result)
                return;
            
            string leftSide = tokensA.Length > 1 ? $"[{string.Join(", ", tokensA)}]" : tokensA[0].ToString();
            string rightSide = tokensB.Length > 1 ? $"[{string.Join(", ", tokensB)}]" : tokensB[0].ToString();
            throw new StatementException(tokens, $"Assertion failed: {leftSide} {compare} {rightSide}");
        }
        [UsedImplicitly]
        public static void _repeat(Executor executor, Statement tokens)
        {
            bool useRange;
            Range range = default;
            int amount;

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
                if (range.min == null || range.max == null)
                    throw new StatementException(tokens, $"Iterating over a range must have minimum and maximum bounds. (got {range})");
                
                int min = range.min.Value;
                int max = range.max.Value;
                for (int i = min; i <= max; i++)
                {
                    if (tracker != null)
                        executor.SetPPV(tracker, new PreprocessorVariable { i });
                    executor.ExecuteSubsection(statements);
                }
            } else
            {
                for (int i = 0; i < amount; i++)
                {
                    if (tracker != null)
                        executor.SetPPV(tracker, new PreprocessorVariable { i });
                    executor.ExecuteSubsection(statements);
                }
            }
        }
        [UsedImplicitly]
        public static void _log(Executor executor, Statement tokens)
        {
            if (executor.linting)
                return;

            var strings = new List<string>();

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
        [UsedImplicitly]
        public static void _macro(Executor executor, Statement tokens)
        {
            if (executor.NextIs<StatementOpenBlock>())
                _macrodefine(executor, tokens);
            else
                _macrocall(executor, tokens);
        }
        [UsedImplicitly]
        private static void _macrodefine(Executor executor, Statement tokens)
        {
            string macroName = tokens.Next<TokenIdentifier>().word;

            var args = new List<string>();
            while (tokens.HasNext && tokens.NextIs<TokenIdentifier>())
                args.Add(tokens.Next<TokenIdentifier>().word);

            var block = executor.Next<StatementOpenBlock>();
            int count = block.statementsInside;
            Statement[] statements = executor.Peek(count);

            if (count < 1)
                throw new StatementException(tokens, "Cannot have empty macro.");
            for (int i = 0; i < count; i++)
                executor.Next(); // skip over those

            executor.Next<StatementCloseBlock>();

            string docs = executor.GetDocumentationString(out _);
            var macro = new Macro(macroName, docs, args.ToArray(), statements);
            executor.RegisterMacro(macro);
        }

        private static void _macrocall(Executor executor, Statement tokens)
        {
            string macroName = tokens.Next<TokenIdentifier>().word;
            Macro? _lookedUp = executor.LookupMacro(macroName);

            if (!_lookedUp.HasValue)
                throw new StatementException(tokens, "Macro '" + macroName + "' does not exist.");

            Macro lookedUp = _lookedUp.Value;
            string[] argNames = lookedUp.argNames;
            var args = new PreprocessorVariable[argNames.Length];

            // get input variables
            for (int i = 0; i < argNames.Length; i++)
            {
                if (!tokens.HasNext)
                    throw new StatementException(tokens, "Missing argument '" + argNames[i] + "' in macro call.");

                if (tokens.NextIs<TokenUnresolvedPPV>())
                {
                    // ReSharper disable once RedundantEnumerableCastCall
                    args[i] = new PreprocessorVariable(executor
                        .ResolvePPV(tokens.Next<TokenUnresolvedPPV>(), tokens)
                        .Cast<dynamic>());
                    continue;
                }

                if (!tokens.NextIs<IPreprocessor>())
                    throw new StatementException(tokens, "Invalid argument type for '" + argNames[i] + "' in macro call.");

                args[i] = new PreprocessorVariable { tokens.Next<IPreprocessor>().GetValue() };
            }

            // save variables which collide with this macro's args.
            var collidedValues
                = new Dictionary<string, PreprocessorVariable>();
            foreach (string arg in lookedUp.argNames)
                if (executor.TryGetPPV(arg, out PreprocessorVariable value))
                    collidedValues[arg] = value.Clone();

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
            foreach (KeyValuePair<string, PreprocessorVariable> kv in collidedValues)
                executor.SetPPV(kv.Key, kv.Value);
        }
        [UsedImplicitly]
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
        [UsedImplicitly]
        public static void _strfriendly(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;
            string input = tokens.NextIs<TokenIdentifier>(false) ?
                tokens.Next<TokenIdentifier>().word :
                output;

            if (executor.TryGetPPV(input, out PreprocessorVariable value))
            {
                dynamic[] results = new dynamic[value.Length];
                for (int r = 0; r < value.Length; r++)
                {
                    if (!(value[r] is string str))
                        continue;
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
        [UsedImplicitly]
        public static void _strupper(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;
            string input = tokens.NextIs<TokenIdentifier>(false) ?
                tokens.Next<TokenIdentifier>().word :
                output;

            if (executor.TryGetPPV(input, out PreprocessorVariable value))
            {
                dynamic[] results = new dynamic[value.Length];
                for (int r = 0; r < value.Length; r++)
                {
                    if (!(value[r] is string str))
                        continue;
                    results[r] = str.ToUpper();
                }
                executor.SetPPV(output, results);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        [UsedImplicitly]
        public static void _strlower(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;
            string input = tokens.NextIs<TokenIdentifier>(false) ?
                tokens.Next<TokenIdentifier>().word :
                output;

            if (executor.TryGetPPV(input, out PreprocessorVariable value))
            {
                dynamic[] results = new dynamic[value.Length];
                for (int r = 0; r < value.Length; r++)
                {
                    if (!(value[r] is string str))
                        continue;
                    results[r] = str.ToLower();
                }
                executor.SetPPV(output, results);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        [UsedImplicitly]
        public static void _sum(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;
            string input = tokens.NextIs<TokenIdentifier>(false) ?
                tokens.Next<TokenIdentifier>().word :
                output;

            if (executor.TryGetPPV(input, out PreprocessorVariable values))
            {
                try
                {
                    dynamic result = values[0];
                    for (int i = 1; i < values.Length; i++)
                        result += values[i];
                    executor.SetPPV(output, result);
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Couldn't add these values.");
                }
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        [UsedImplicitly]
        public static void _median(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;
            string input = tokens.NextIs<TokenIdentifier>(false) ?
                tokens.Next<TokenIdentifier>().word :
                output;

            if (executor.TryGetPPV(input, out PreprocessorVariable values))
            {
                try
                {
                    int len = values.Length;
                    if (len == 1)
                    {
                        executor.SetPPV(output, new[] { values[0] });
                    }
                    else if (len % 2 == 0)
                    {
                        int mid = len / 2;
                        dynamic first = values[mid];
                        dynamic second = values[mid - 1];
                        dynamic result = (first + second) / 2;
                        executor.SetPPV(output, new[] { result });
                    }
                    else
                    {
                        dynamic result = values[len / 2]; // truncates to middle index
                        executor.SetPPV(output, new[] { result });
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
        [UsedImplicitly]
        public static void _mean(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;
            string input = tokens.NextIs<TokenIdentifier>(false) ?
                tokens.Next<TokenIdentifier>().word :
                output;

            if (executor.TryGetPPV(input, out PreprocessorVariable values))
            {
                try
                {
                    int length = values.Length;

                    if (length == 1)
                    {
                        executor.SetPPV(output, new[] { values[0] });
                        return;
                    }
                    
                    dynamic result = values[0];
                    for (int i = 1; i < length; i++)
                        result += values[i];
                    result /= length;
                    executor.SetPPV(output, new[] { result });
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Couldn't add/divide these values.");
                }
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        [UsedImplicitly]
        public static void _sort(Executor executor, Statement tokens)
        {
            string sortDirection = tokens.Next<TokenIdentifier>().word.ToUpper();
            string variable = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(variable, out PreprocessorVariable values))
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
        [UsedImplicitly]
        public static void _reverse(Executor executor, Statement tokens)
        {
            string variable = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(variable, out PreprocessorVariable values))
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
                        (values[i], values[e]) = (values[e], values[i]);
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
        [UsedImplicitly]
        public static void _unique(Executor executor, Statement tokens)
        {
            string variable = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(variable, out PreprocessorVariable values))
            {
                if (values.Length < 2)
                    return;

                var items = new HashSet<object>();
                
                foreach (dynamic value in values)
                    items.Add(value);

                executor.SetPPV(variable, items.ToArray());
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + variable + "' does not exist.");
        }

        /// <summary>
        /// Iterates over values of a preprocessor variable and executes a set of statements for each value.
        /// </summary>
        /// <param name="executor">The executor object used to execute statements.</param>
        /// <param name="tokens">The statement tokens.</param>
        [UsedImplicitly]
        public static void _iterate(Executor executor, Statement tokens)
        {
            string input = tokens.Next<TokenIdentifier>().word;
            string current = tokens.Next<TokenIdentifier>().word;

            if (!executor.TryGetPPV(input, out PreprocessorVariable values))
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");

            Statement[] statements = executor.NextExecutionSet();

            foreach (dynamic value in values)
            {
                switch (value)
                {
                    case TokenJSONLiteral jsonLiteral:
                    {
                        JToken token = jsonLiteral.token;
                        if (token is JArray array)
                            IterateArray(array);
                        break;
                    }
                    case JArray array:
                        IterateArray(array);
                        break;
                    default:
                        executor.SetPPV(current, value);
                        executor.ExecuteSubsection(statements);
                        break;
                }
            }

            return;

            void IterateArray(JArray array)
            {
                foreach (JToken arrayItem in array)
                {
                    if (PreprocessorUtils.TryUnwrapToken(arrayItem, out object obj))
                    {
                        if (obj == null)
                            throw new StatementException(tokens, $"Couldn't unwrap JSON token to be placed in a preprocessor variable: {arrayItem.ToString()}");

                        executor.SetPPV(current, obj);
                        executor.ExecuteSubsection(statements);
                    }
                    else
                        throw new StatementException(tokens, $"JSON Error: Cannot store token of type '{arrayItem.Type}' in a preprocessor variable.");
                }
            }
        }
        [UsedImplicitly]
        public static void _len(Executor executor, Statement tokens)
        {
            string output = tokens.Next<TokenIdentifier>().word;

            if(tokens.NextIs<TokenStringLiteral>(false))
            {
                // String
                string inputString = tokens.Next<TokenStringLiteral>().text;
                executor.SetPPV(output, inputString.Length);
                return;
            }

            if(tokens.NextIs<TokenJSONLiteral>())
            {
                // JSON Array
                JToken inputJSON = tokens.Next<TokenJSONLiteral>();

                if (!(inputJSON is JArray array))
                    throw new StatementException(tokens, "Cannot get the length of a non-array JSON token.");
                
                executor.SetPPV(output, array.Count);
                return;

            }

            // Preprocessor Variable
            string input = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(input, out PreprocessorVariable values))
            {
                int length = values.Length;
                executor.SetPPV(output, length);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        [UsedImplicitly]
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
                IEnumerable<string> accessParts = PreprocessorUtils.ParseAccessor(accessor);

                // crawl the tree
                foreach (string _access in accessParts)
                {
                    string access = _access.Trim();
                    switch (json.Type)
                    {
                        case JTokenType.Array:
                        {
                            var array = (JArray)json;
                            if (!int.TryParse(access, out int index))
                                throw new StatementException(tokens, $"JSON Error: Array at '{array.Path}' requires index to access. Given: {access}");
                            if (index < 0)
                                throw new StatementException(tokens, $"JSON Error: Index given was less than 0.");
                            if (index >= array.Count)
                                throw new StatementException(tokens, $"JSON Error: Array at '{array.Path}' only contains {array.Count} items. Given: {index + 1}");
                            json = array[index];
                            continue;
                        }
                        case JTokenType.Object:
                        {
                            var obj = (JObject)json;
                            
                            if (!obj.TryGetValue(access, out json))
                                throw new StatementException(tokens, $"JSON Error: Cannot find child '{access}' under token {obj.Path}.");
                            continue;
                        }
                        // unsupported
                        case JTokenType.None:
                        case JTokenType.Constructor:
                        case JTokenType.Property:
                        case JTokenType.Comment:
                        case JTokenType.Integer:
                        case JTokenType.Float:
                        case JTokenType.String:
                        case JTokenType.Boolean:
                        case JTokenType.Null:
                        case JTokenType.Undefined:
                        case JTokenType.Date:
                        case JTokenType.Raw:
                        case JTokenType.Bytes:
                        case JTokenType.Guid:
                        case JTokenType.Uri:
                        case JTokenType.TimeSpan:
                        default:
                            throw new StatementException(tokens, $"JSON Error: Unexpected end of JSON tree at {json.Path}.");
                    }
                }
            }

            if(PreprocessorUtils.TryUnwrapToken(json, out object unwrapped))
            {
                executor.SetPPV(output, new[] { unwrapped });
                return;
            } else
                throw new StatementException(tokens, $"JSON Error: Cannot store token of type '{json.Type}' in a preprocessor variable.");
        }
        [UsedImplicitly]
        public static void _call(Executor executor, Statement tokens)
        {
            string functionName = tokens.Next<TokenStringLiteral>().text;

            if (!executor.functions.TryGetFunctions(functionName, out Function[] functions))
                throw new StatementException(tokens, $"Could not find function by the name '{functionName}'");

            Token[] remainingTokens = tokens.GetRemainingTokens();
            var finalTokens = new Token[remainingTokens.Length + 3];

            int line = tokens.Lines[0];
            Array.Copy(remainingTokens, 0, finalTokens, 2, remainingTokens.Length);
            finalTokens[0] = new TokenIdentifierFunction(functionName, functions, line);
            finalTokens[1] = new TokenOpenParenthesis(line);
            finalTokens[finalTokens.Length - 1] = new TokenCloseParenthesis(line);


            var callStatement = new StatementFunctionCall(finalTokens);
            callStatement.SetSource(tokens.Lines, tokens.Source);

            callStatement
                .ClonePrepare(executor)
                .Run0(executor);
        }

        [UsedImplicitly]
        public static void mc(Executor executor, Statement tokens)
        {
            string command = tokens.Next<TokenStringLiteral>();
            executor.AddCommand(command);
        }
        [UsedImplicitly]
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
        [UsedImplicitly]
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
        [UsedImplicitly]
        public static void lang(Executor executor, Statement tokens)
        {
            string locale = tokens.Next<TokenIdentifier>().word;

            const bool DEFAULT_MERGE = true;

            // create preprocessor variable if it doesn't exist.
            if (!executor.ppv.TryGetValue(LanguageManager.MERGE_PPV, out _))
                executor.ppv[LanguageManager.MERGE_PPV] = new PreprocessorVariable(LanguageManager.MERGE_PPV, DEFAULT_MERGE);

            if (Program.DEBUG)
                Console.WriteLine("Set locale to '{0}'", locale);

            if (executor.linting)
                return; // due to no project being given in linting operations, it's not necessary.

            executor.SetLocale(locale);
        }
        [UsedImplicitly]
        public static void define(Executor executor, Statement tokens)
        {
            string docs = executor.GetDocumentationString(out bool hadDocumentation);

            ScoreboardManager.ValueDefinition def = executor
                .scoreboard.GetNextValueDefinition(tokens);
            
            // create the new scoreboard value.
            ScoreboardValue value = def.Create(executor.scoreboard, tokens);
            if(hadDocumentation)
                value.Documentation = docs;
            executor.AddCommandsInit(value.CommandsDefine());

            // register it to the executor.
            executor.scoreboard.TryThrowForDuplicate(value, tokens);
            executor.scoreboard.Add(value);

            if (def.defaultValue != null)
            {
                // all the rest of this is getting the commands to define the variable.
                var commands = new List<string>();

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

                bool inline = commands.Count == 1;

                if (hadDocumentation && Program.DECORATE)
                    commands.AddRange(docs.Trim().Split('\n').Select(str => "# " + str.Trim()));

                // add the commands to the executor.
                CommandFile file = executor.CurrentFile;
                executor.AddCommands(commands, "define" + value.Name, $"Called when defining the value '{value.Name}' in {file.CommandReference} line {executor.NextLineNumber}", inline);
            }
        }
        [UsedImplicitly]
        public static void init(Executor executor, Statement tokens)
        {
            ScoreboardValue value;
            var commands = new List<string>();

            while (tokens.HasNext)
            {
                if (tokens.NextIs<IInformationless>())
                    continue;

                value = tokens.Next<TokenIdentifierValue>().value;
                commands.AddRange(value.CommandsInit(value.clarifier.CurrentString));
            }

            executor.AddCommands(commands, null, null, true);
        }
        [UsedImplicitly]
        public static void @if(Executor executor, Statement tokens)
        {
            if (!executor.HasNext)
                throw new StatementException(tokens, "Unexpected end of file after if-statement.");

            // 1.1 rework (post new-execute)
            ComparisonSet set = ComparisonSet.GetComparisons(executor, tokens);
            set.InvertAll(false);
            set.Run(executor, tokens);
        }
        [UsedImplicitly]
        public static void @else(Executor executor, Statement tokens)
        {
            if (!executor.HasNext)
                throw new StatementException(tokens, "Unexpected end of file after else-statement.");

            PreviousComparisonStructure set = executor.GetLastCompare();

            if (set == null)
                throw new StatementException(tokens, "No if-statement was found in front of this else-statement at this scope level.");

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
        [UsedImplicitly]
        public static void assert(Executor executor, Statement tokens)
        {
            executor.CurrentFile.MarkAssertion();

            ComparisonSet set = ComparisonSet.GetComparisons(executor, tokens);
            set.InvertAll(true);

            CommandFile file = Executor.GetNextGeneratedFile("failAssertion");
            IEnumerable<ScoreboardValue> values = set.GetAssertionTargets();

            // construct assertion failed message based on all values in this comparison set
            const string red = "§c";
            file.Add(Command.Tellraw(new RawTextJsonBuilder().AddTerm(new JSONText(red + "Assertion failed! " + set.GetDescription())).BuildString()));
            foreach(ScoreboardValue value in values)
            {
                file.Add(Command.Tellraw("@s",
                    new RawTextJsonBuilder().AddTerms(
                        new JSONText($"{red}    - {value.Name} was "),
                        new JSONScore(value)
                    )
                .BuildString()));
            }
            file.Add(halt_command(executor, tokens));

            executor.AddExtraFile(file);
            set.RunCommand(Command.Function(file), executor, tokens);
            return;
        }
        [UsedImplicitly]
        public static void @throw(Executor executor, Statement tokens)
        {
            string text = tokens.Next<TokenStringLiteral>();
            List<JSONRawTerm> json = executor.FString(text, tokens, out bool _);
            string[] commands = Executor.ResolveRawText(json, "tellraw @s ");

            executor.AddCommandsClean(commands, "throwError", $"Called in a throw command located in {executor.CurrentFile.CommandReference} line {executor.NextLineNumber}");
            executor.AddCommand(halt_command(executor, tokens));
            return;
        }
        [UsedImplicitly]
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
            var loreLines = new List<string>();
            var canPlaceOn = new List<string>();
            var canDestroy = new List<string>();
            var enchants = new List<Tuple<Enchantment, int>>();
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

            while (executor.NextBuilderField(ref tokens, out TokenBuilderIdentifier builderIdentifier))
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
                        var enchantment = (Enchantment)parsedEnchantment.value;
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
                // ReSharper disable once InvertIf
                if (itemNameComp.StartsWith("LEATHER_"))
                {
                    // ReSharper disable once InvertIf
                    if(builderField.Equals("DYE"))
                    {
                        color = new ItemTagCustomColor()
                        {
                            r = (byte)tokens.Next<TokenIntegerLiteral>(),
                            g = (byte)tokens.Next<TokenIntegerLiteral>(),
                            b = (byte)tokens.Next<TokenIntegerLiteral>()
                        };
                        needsStructure = true;
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

                var item = new ItemStack()
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
                var file = new StructureFile("item" + item.GetHashCode(),
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

            var json = new List<string>();

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
        [UsedImplicitly]
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
                    // ReSharper disable once InvertIf
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
                var selector = tokens.Next<TokenSelectorLiteral>();
                executor.AddCommand(Command.Teleport(entity.ToString(), selector.selector.ToString(), GetCheckForBlocks()));
                return;
            }

            ParseArgs(entity.ToString());
        }
        [UsedImplicitly]
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
        [UsedImplicitly]
        public static void face(Executor executor, Statement tokens)
        {
            Selector selector = tokens.Next<TokenSelectorLiteral>();

            if (tokens.NextIs<TokenSelectorLiteral>())
            {
                var other = tokens.Next<TokenSelectorLiteral>();
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
        [UsedImplicitly]
        public static void rotate(Executor executor, Statement tokens)
        {
            Selector selector = tokens.Next<TokenSelectorLiteral>();

            var number = tokens.Next<TokenNumberLiteral>();
            Coord rx = Coord.here;

            Coord ry = number is TokenDecimalLiteral ?
                new Coord(number.GetNumber(), true, true, false) :
                new Coord(number.GetNumberInt(), false, true, false);

            if (tokens.HasNext && tokens.NextIs<TokenNumberLiteral>())
            {
                number = tokens.Next<TokenNumberLiteral>();
                rx = number is TokenDecimalLiteral ?
                    new Coord(number.GetNumber(), true, true, false) :
                    new Coord(number.GetNumberInt(), false, true, false);
            }

            executor.AddCommand(Command.Teleport(selector.ToString(), Coord.here, Coord.here, Coord.here, ry, rx));
        }
        [UsedImplicitly]
        public static void setblock(Executor executor, Statement tokens)
        {
            Coord x = tokens.Next<TokenCoordinateLiteral>();
            Coord y = tokens.Next<TokenCoordinateLiteral>();
            Coord z = tokens.Next<TokenCoordinateLiteral>();
            string block = tokens.Next<TokenStringLiteral>();
            var handling = OldHandling.replace;

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
        [UsedImplicitly]
        public static void fill(Executor executor, Statement tokens)
        {
            var handling = OldHandling.replace;

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
        [UsedImplicitly]
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

            var structure = new StructureNBT()
            {
                formatVersion = 1,
                size = new VectorIntNBT(sizeX, sizeY, sizeZ),
                worldOrigin = new VectorIntNBT(0, 0, 0),

                palette = new PaletteNBT(new PaletteEntryNBT(block)),
                entities = new EntityListNBT(new EntityNBT[0]),
                indices = new BlockIndicesNBT(blocks)
            };

            string fileName = "scatter_" + scatterFile++;
            var file = new StructureFile(fileName,
                Executor.MCC_GENERATED_FOLDER, structure);
            executor.project.WriteSingleFile(file);

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
        [UsedImplicitly]
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
        [UsedImplicitly]
        public static void kill(Executor executor, Statement tokens)
        {
            Selector selector = Selector.SELF;

            if (tokens.NextIs<TokenSelectorLiteral>())
                selector = tokens.Next<TokenSelectorLiteral>();

            executor.AddCommand(Command.Kill(selector.ToString()));
        }
        [UsedImplicitly]
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
        [UsedImplicitly]
        public static void globaltitle(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenIdentifier>(false))
            {
                string word = tokens.Next<TokenIdentifier>().word;
                switch (word.ToUpper())
                {
                    case "TIMES":
                    {
                        int fadeIn = tokens.Next<TokenIntegerLiteral>();
                        int stay = tokens.Next<TokenIntegerLiteral>();
                        int fadeOut = tokens.Next<TokenIntegerLiteral>();
                        executor.AddCommand(Command.TitleTimes("@a", fadeIn, stay, fadeOut));
                        return;
                    }
                    case "SUBTITLE":
                    {
                        string str = tokens.Next<TokenStringLiteral>();
                        List<JSONRawTerm> terms = executor.FString(str, tokens, out bool advanced);

                        string[] commands = advanced ?
                            Command.Execute().As(Selector.ALL_PLAYERS).AtSelf().RunOver(Executor.ResolveRawText(terms, "titleraw @s subtitle ")) :
                            Executor.ResolveRawText(terms, "titleraw @a subtitle ");

                        CommandFile file = executor.CurrentFile;
                        executor.AddCommands(commands, "subtitle", $"Called in a global-subtitle command located in {file.CommandReference} line {executor.NextLineNumber}");
                        return;
                    }
                    default:
                        throw new StatementException(tokens, $"Invalid globaltitle subcommand '{word}'. Must be 'times' or 'subtitle'.");
                }
            }

            // ReSharper disable once InvertIf
            if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                List<JSONRawTerm> terms = executor.FString(str, tokens, out bool advanced);
                string[] commands;

                commands = advanced ?
                    Command.Execute().As(Selector.ALL_PLAYERS).AtSelf().RunOver(Executor.ResolveRawText(terms, "title @s title ")) :
                    Executor.ResolveRawText(terms, "titleraw @a title ");

                CommandFile file = executor.CurrentFile;
                executor.AddCommands(commands, "title", $"Called in a globaltitle command located in {file.CommandReference} line {executor.NextLineNumber}");
            }
        }
        [UsedImplicitly]
        public static void title(Executor executor, Statement tokens)
        {
            Selector player;

            player = tokens.NextIs<TokenSelectorLiteral>(false) ?
                tokens.Next<TokenSelectorLiteral>() :
                Selector.SELF;

            if (tokens.NextIs<TokenIdentifier>(false))
            {
                string word = tokens.Next<TokenIdentifier>().word.ToUpper();
                switch (word)
                {
                    case "TIMES":
                    {
                        int fadeIn = tokens.Next<TokenIntegerLiteral>();
                        int stay = tokens.Next<TokenIntegerLiteral>();
                        int fadeOut = tokens.Next<TokenIntegerLiteral>();
                        executor.AddCommand(Command.TitleTimes(player.ToString(), fadeIn, stay, fadeOut));
                        return;
                    }
                    case "SUBTITLE":
                    {
                        string str = tokens.Next<TokenStringLiteral>();
                        List<JSONRawTerm> terms = executor.FString(str, tokens, out bool _);

                        string[] commands = Executor.ResolveRawText(terms, $"titleraw {player} subtitle ");

                        CommandFile file = executor.CurrentFile;
                        executor.AddCommands(commands, "subtitle", $"Called in a subtitle command located in {file.CommandReference} line {executor.NextLineNumber}");
                        return;
                    }
                    default:
                        throw new StatementException(tokens, $"Invalid title subcommand '{word}'. Must be 'times' or 'subtitle'.");
                }
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
        [UsedImplicitly]
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
        [UsedImplicitly]
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
        [UsedImplicitly]
        public static void say(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            executor.AddCommand(Command.Say(str));
        }
        private static string halt_command(Executor executor, Statement tokens)
        {
            var file = new CommandFile(true, "halt_execution", Executor.MCC_GENERATED_FOLDER);

            if (!executor.HasSTDFile(file))
            {
                // recursively call self until function command limit reached
                file.Add(Command.Function(file));
                executor.DefineSTDFile(file);
            }

            executor.UnreachableCode();
            return Command.Function(file);
        }
        [UsedImplicitly]
        public static void halt(Executor executor, Statement tokens)
        {
            string command = halt_command(executor, tokens);
            executor.AddCommand(command);
        }
        [UsedImplicitly]
        public static void damage(Executor executor, Statement tokens)
        {
            Selector target = tokens.Next<TokenSelectorLiteral>();

            int damage = tokens.Next<TokenIntegerLiteral>();
            var cause = DamageCause.all;
            Selector blame = null;

            if(tokens.NextIs<TokenIdentifierEnum>())
            {
                var idEnum = tokens.Next<TokenIdentifierEnum>();
                idEnum.value.RequireType<DamageCause>(tokens);
                cause = (DamageCause)idEnum.value.value;
            }

            if (tokens.NextIs<TokenSelectorLiteral>())
            {
                var value = tokens.Next<TokenSelectorLiteral>();
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
        [UsedImplicitly]
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
        [UsedImplicitly]
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
        [UsedImplicitly]
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
        [UsedImplicitly]
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
        [UsedImplicitly]
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
            var effectToken = tokens.Next<TokenIdentifierEnum>();
            ParsedEnumValue parsedEffect = effectToken.value;
            parsedEffect.RequireType<PotionEffect>(tokens);
            var effect = (PotionEffect)parsedEffect.value;

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
        [UsedImplicitly]
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
        [UsedImplicitly]
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
        
        [UsedImplicitly]
        public static void execute(Executor executor, Statement tokens)
        {
            var builder = new ExecuteBuilder();

            do
            {
                string _subcommand = tokens.Next<TokenIdentifier>().word.ToUpper();
                var subcommand = Subcommand.GetSubcommandForKeyword(_subcommand, tokens);

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
        [UsedImplicitly]
        public static void feature(Executor executor, Statement tokens)
        {
            string featureStr = tokens.Next<TokenIdentifier>().word.ToUpper();
            var feature = Feature.NO_FEATURES;

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
        [UsedImplicitly]
        public static void function(Executor executor, Statement tokens)
        {
            // pull attributes
            var attributes = new List<IAttribute>();

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
                    usesFolders = false; // user wrote beyond-shit code; fix it up for them
                    functionName = functionName.Trim('.');
                }
            }

            var parameters = new List<RuntimeFunctionParameter>();

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
            var function = new RuntimeFunction(tokens, functionName, actualName, docs, attributes.ToArray(), false)
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

                var openBlock = executor.Peek<StatementOpenBlock>();

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
        [UsedImplicitly]
        public static void test(Executor executor, Statement tokens)
        {
            executor.RequireFeature(tokens, Feature.TESTS);

            // normal definition
            string testName = tokens.Next<TokenIdentifier>().word;

            bool usesFolders = testName.Contains('.');
            string[] folders = null;

            if (usesFolders)
            {
                var split = testName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 1)
                    folders = split.Take(split.Length - 1).ToArray();
                else
                {
                    usesFolders = false; // user wrote beyond-shit code; fix it up for them
                    testName = testName.Trim('.');
                }
            }

            // see if last statement was a comment, and use that for documentation
            string docs = executor.GetDocumentationString(out bool hadDocumentation);

            // the actual name of the function file
            string actualName = usesFolders ? testName.Substring(testName.LastIndexOf('.') + 1) : testName;

            // constructor
            var test = new TestFunction(tokens, testName, actualName, docs)
            {
                documentation = docs,
                isAddedToExecutor = true
            };

            // folders, if specified via dots
            if (usesFolders)
                test.file.Folders = folders;

            // register it with the compiler
            executor.functions.RegisterTest(test);

            if (executor.NextIs<StatementOpenBlock>())
            {
                var openBlock = executor.Peek<StatementOpenBlock>();

                openBlock.openAction = (e) =>
                {
                    e.PushFile(test.file);
                };
                openBlock.CloseAction = (e) =>
                {
                    e.PopFile();
                };

                return;
            }
        }
        [UsedImplicitly]
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
        [UsedImplicitly]
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
                var block = executor.Peek<StatementOpenBlock>();
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