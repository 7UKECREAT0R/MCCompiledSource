using mc_compiled.Commands;
using mc_compiled.Commands.Native;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.Modding;
using mc_compiled.NBT;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            while (tokens.NextIs<IObjectable>())
                values.Add(tokens.Next<IObjectable>().GetObject());

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
            IObjectable otherToken = tokens.Next<IObjectable>();

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
                inputs.Add((otherToken as IObjectable).GetObject());
                while (tokens.NextIs<IObjectable>())
                    inputs.Add(tokens.Next<IObjectable>().GetObject());
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
            IObjectable otherToken = tokens.Next<IObjectable>();

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
                inputs.Add((otherToken as IObjectable).GetObject());
                while (tokens.NextIs<IObjectable>())
                    inputs.Add(tokens.Next<IObjectable>().GetObject());
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
            IObjectable otherToken = tokens.Next<IObjectable>();

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
                inputs.Add((otherToken as IObjectable).GetObject());
                while (tokens.NextIs<IObjectable>())
                    inputs.Add(tokens.Next<IObjectable>().GetObject());
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
            IObjectable otherToken = tokens.Next<IObjectable>();

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
                inputs.Add((otherToken as IObjectable).GetObject());
                while (tokens.NextIs<IObjectable>())
                    inputs.Add(tokens.Next<IObjectable>().GetObject());
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
            IObjectable otherToken = tokens.Next<IObjectable>();

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
                inputs.Add((otherToken as IObjectable).GetObject());
                while (tokens.NextIs<IObjectable>())
                    inputs.Add(tokens.Next<IObjectable>().GetObject());
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
            IObjectable otherToken = tokens.Next<IObjectable>();

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
                inputLiterals.Add((otherToken as IObjectable).GetObject());
                while (tokens.NextIs<IObjectable>())
                    inputLiterals.Add(tokens.Next<IObjectable>().GetObject());
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
            IObjectable otherToken = tokens.Next<IObjectable>();
            dynamic[] others = new dynamic[] { otherToken.GetObject() };

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
                            case TokenCompare.Type.LESS_THAN:
                                run &= a < other;
                                break;
                            case TokenCompare.Type.LESS_OR_EQUAL:
                                run &= a <= other;
                                break;
                            case TokenCompare.Type.GREATER_THAN:
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
                block.aligns = false;
                block.shouldRun = run;
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
                block.aligns = false;
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
                executor.Next<StatementOpenBlock>(); // skip that
            }
            else
            {
                skipAfter = 0;
                statements = new[] { executor.Next() };
            }

            for (int i = 0; i < amount; i++)
            {
                if (tracker != null)
                    executor.SetPPV(tracker, new dynamic[] { i });
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
            if (executor.HasNext && executor.NextIs<StatementOpenBlock>())
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
            object[] args = new object[argNames.Length];

            // get input variables
            for (int i = 0; i < argNames.Length; i++)
            {
                if (!tokens.HasNext)
                    throw new StatementException(tokens, "Missing argument '" + argNames[i] + "' in macro call.");
                if (!tokens.NextIs<IObjectable>())
                    throw new StatementException(tokens, "Invalid argument type for '" + argNames[i] + "' in macro call.");
                args[i] = tokens.Next<IObjectable>().GetObject();
            }

            // save variables which collide with this macro's args.
            Dictionary<string, dynamic[]> collidedValues
                = new Dictionary<string, dynamic[]>();
            foreach (string arg in lookedUp.argNames)
                if (executor.TryGetPPV(arg, out dynamic[] value))
                    collidedValues[arg] = value;

            // set input variables
            for (int i = 0; i < argNames.Length; i++)
                executor.SetPPV(argNames[i], new dynamic[] { args[i] });

            // call macro
            executor.ExecuteSubsection(lookedUp.statements);

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
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

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
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

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
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

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
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

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
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

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
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

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
        public static void _get(Executor executor, Statement tokens)
        {
            string input = tokens.Next<TokenIdentifier>().word;
            int index = tokens.Next<TokenIntegerLiteral>();
            string output = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(input, out dynamic[] values))
            {
                if (index >= values.Length)
                    throw new StatementException(tokens, $"Index {index} is too large for preprocessor variable '{input}'. Max: {values.Length - 1}");
                if (index < 0)
                    throw new StatementException(tokens, $"Index cannot be less than zero (was {index}).");

                dynamic result = values[index];
                executor.SetPPV(output, new dynamic[] { result });
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _len(Executor executor, Statement tokens)
        {
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(input, out dynamic[] values))
                executor.SetPPV(output, new dynamic[] { values.Length });
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _json(Executor executor, Statement tokens)
        {
            string file = tokens.Next<TokenStringLiteral>();
            string output = tokens.Next<TokenIdentifier>().word;
            string accessor = tokens.Next<TokenStringLiteral>();

            JToken json = executor.LoadJSONFile(file);
            string[] accessParts = accessor.Split('/', ',');

            foreach (string _access in accessParts)
            {
                string access = _access.Trim();
                if (json.Type == JTokenType.Array)
                {
                    JArray array = json as JArray;
                    if (!int.TryParse(access, out int index))
                        throw new StatementException(tokens, $"JSON Error: Array at '{array.Path}' requires index to access. Given: {access}");
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
                    throw new StatementException(tokens, $"JSON Error: Unexpected end of JSON at {json.Path}.");
            }

            try
            {
                dynamic[] final;
                JToken[] parsers;
                if (json.Type == JTokenType.Array)
                {
                    final = new dynamic[(json as JArray).Count];
                    parsers = (json as JArray).ToArray();
                }
                else
                {
                    final = new dynamic[1];
                    parsers = new JToken[] { json };
                }

                for (int i = 0; i < parsers.Length; i++)
                {
                    json = parsers[i];
                    switch (json.Type)
                    {
                        case JTokenType.None:
                        case JTokenType.Object:
                        case JTokenType.Array:
                        case JTokenType.Constructor:
                        case JTokenType.Property:
                        case JTokenType.Comment:
                        case JTokenType.Raw:
                        case JTokenType.Bytes:
                            throw new StatementException(tokens, $"JSON Error: Invalid token type: {json.Type}");
                        case JTokenType.Null:
                        case JTokenType.Undefined:
                            break;
                        case JTokenType.Integer:
                            final[i] = json.Value<int>();
                            break;
                        case JTokenType.Float:
                            final[i] = json.Value<float>();
                            break;
                        case JTokenType.String:
                            final[i] = json.Value<string>();
                            break;
                        case JTokenType.Boolean:
                            final[i] = json.Value<bool>();
                            break;
                        case JTokenType.Date:
                            final[i] = json.Value<DateTime>().ToString();
                            break;
                        case JTokenType.Guid:
                            final[i] = json.Value<Guid>().ToString();
                            break;
                        case JTokenType.Uri:
                            final[i] = json.Value<Uri>().OriginalString;
                            break;
                        case JTokenType.TimeSpan:
                            final[i] = json.Value<TimeSpan>().TotalSeconds * 20; // ticks
                            break;
                        default:
                            break;
                    }
                }

                executor.SetPPV(output, final);
            }
            catch (Exception e)
            {
                throw new StatementException(tokens, $"JSON Error: {e.Message}");
            }
        }

        public static void mc(Executor executor, Statement tokens)
        {
            string command = tokens.Next<TokenStringLiteral>();
            executor.AddCommand(command);
        }
        public static void select(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenStringLiteral>())
            {
                string name = tokens.Next<TokenStringLiteral>();
                string type = null;
                if (name.Contains(':'))
                {
                    string[] strs = name.Split(':');
                    name = strs[0].Trim();
                    if (strs.Length > 1)
                        type = strs[1].Trim();
                }
                if (string.IsNullOrEmpty(name))
                    name = null;
                if (string.IsNullOrEmpty(type))
                    type = null;

                executor.ActiveSelector = new Selector()
                {
                    core = Selector.Core.e,
                    entity = new Entity(name, false, type, null)
                };
                return;
            }
            TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
            executor.ActiveSelector = selector;
        }
        public static void globalprint(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            List<JSONRawTerm> terms = executor.FString(str, out bool advanced);

            string[] commands;

            if (advanced)
                commands = executor.ResolveRawText(terms, Command.Execute("@a", Coord.here, Coord.here, Coord.here, "tellraw @s "));
            else
                commands = executor.ResolveRawText(terms, "tellraw @a ");

            executor.AddCommands(commands, "print");
        }
        public static void print(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            List<JSONRawTerm> terms = executor.FString(str, out bool advanced);
            string[] commands;

            if (advanced)
            {
                executor.PushSelectorExecute();
                string selector = executor.ActiveSelectorStr;
                string baseCommand = "tellraw " + selector + ' ';
                commands = executor.ResolveRawText(terms, baseCommand);
                executor.PopSelector();
            }
            else
            {
                string selector = executor.ActiveSelectorStr;
                string baseCommand = "tellraw " + selector + ' ';
                commands = executor.ResolveRawText(terms, baseCommand);
            }

            executor.AddCommands(commands, "print");
        }
        public static void define(Executor executor, Statement tokens)
        {
            ScoreboardManager.ValueDefinition def = executor
                .scoreboard.GetNextValueDefinition(tokens);
            ScoreboardValue value = def.Create(executor.scoreboard, tokens);
            executor.scoreboard.Add(value);
            List<string> commands = new List<string>();
            commands.AddRange(value.CommandsDefine());

            if (def.defaultValue != null)
            {
                executor.PushSelectorExecute();
                string sel = executor.ActiveSelectorStr;
                if (def.defaultValue is TokenLiteral)
                    commands.AddRange(value.CommandsSetLiteral(value.baseName, sel, def.defaultValue as TokenLiteral));
                else if (def.defaultValue is TokenIdentifierValue)
                {
                    TokenIdentifierValue identifier = def.defaultValue as TokenIdentifierValue;
                    commands.AddRange(value.CommandsSet(sel, identifier.value, value.baseName, identifier.Accessor));
                }
                else
                    throw new StatementException(tokens, $"Cannot assign value of type {def.defaultValue.GetType().Name} into a variable");
                executor.PopSelector();
            }

            executor.AddCommands(commands, "define" + value.baseName);
        }
        public static void init(Executor executor, Statement tokens)
        {
            ScoreboardValue value;

            if (tokens.NextIs<TokenStringLiteral>())
            {
                string name = tokens.Next<TokenStringLiteral>();
                if (!executor.scoreboard.TryGetByAccessor(name, out value, true))
                    throw new StatementException(tokens, $"Attempted to initialize undefined variable '{name}'.");
            }
            else
                value = tokens.Next<TokenIdentifierValue>().value;

            executor.AddCommands(value.CommandsInit(), "init" + value.baseName, true);
        }
        public static void @if(Executor executor, Statement tokens) =>
            @if(executor, tokens, false);
        public static void @if(Executor executor, Statement tokens, bool invert)
        {
            executor.PushSelectorExecute();
            Selector selector = new Selector(executor.ActiveSelector);
            Token[] tokensUsed = tokens.GetRemainingTokens();

            executor.scoreboard.PushTempState();
            List<string> commands = new List<string>();

            do
            {
                if (tokens.NextIs<TokenAnd>())
                    tokens.Next();

                string entity = executor.ActiveSelectorStr;
                bool not = invert;
                bool isScore = tokens.NextIs<TokenIdentifierValue>();
                TokenIdentifier currentToken = tokens.Next<TokenIdentifier>();
                string word = currentToken.word.ToUpper();

                // check for word "not"

                if (word.ToUpper().Equals("NOT"))
                {
                    not = !invert;
                    isScore = tokens.NextIs<TokenIdentifierValue>();
                    currentToken = tokens.Next<TokenIdentifier>();
                    word = currentToken.word.ToUpper();
                }

                if (isScore)
                {
                    TokenIdentifierValue a = currentToken as TokenIdentifierValue;

                    // if <boolean> {}
                    if (!tokens.HasNext || !tokens.NextIs<TokenCompare>())
                    {
                        selector.scores.checks.Add(new ScoresEntry(a.value, new Range(1, not)));
                    }
                    // if <value> <comp> <other>
                    else if (tokens.NextIs<TokenCompare>())
                    {
                        TokenCompare compare = tokens.Next<TokenCompare>();
                        TokenCompare.Type ctype = compare.GetCompareType();

                        // invert the type (bad code on their part tbh)
                        if (not)
                            switch (ctype)
                            {
                                case TokenCompare.Type.EQUAL:
                                    ctype = TokenCompare.Type.NOT_EQUAL;
                                    break;
                                case TokenCompare.Type.NOT_EQUAL:
                                    ctype = TokenCompare.Type.EQUAL;
                                    break;
                                case TokenCompare.Type.LESS_THAN:
                                    ctype = TokenCompare.Type.GREATER_OR_EQUAL;
                                    break;
                                case TokenCompare.Type.LESS_OR_EQUAL:
                                    ctype = TokenCompare.Type.GREATER_THAN;
                                    break;
                                case TokenCompare.Type.GREATER_THAN:
                                    ctype = TokenCompare.Type.LESS_OR_EQUAL;
                                    break;
                                case TokenCompare.Type.GREATER_OR_EQUAL:
                                    ctype = TokenCompare.Type.LESS_THAN;
                                    break;
                                default:
                                    break;
                            }

                        // if <value> <comp> identifier
                        if (tokens.NextIs<TokenIdentifierValue>())
                        {
                            TokenIdentifierValue b = tokens.Next<TokenIdentifierValue>();
                            ScoreboardValue temp = executor.scoreboard.RequestTemp(a.value);
                            commands.AddRange(temp.CommandsSet(entity, a.value, a.word, b.word));
                            commands.AddRange(temp.CommandsSub(entity, b.value, a.word, b.word));
                            Range check;

                            switch (ctype)
                            {
                                case TokenCompare.Type.EQUAL:
                                    check = new Range(0, false);
                                    break;
                                case TokenCompare.Type.NOT_EQUAL:
                                    check = new Range(0, true);
                                    break;
                                case TokenCompare.Type.LESS_THAN:
                                    check = new Range(null, -1);
                                    break;
                                case TokenCompare.Type.LESS_OR_EQUAL:
                                    check = new Range(null, 0);
                                    break;
                                case TokenCompare.Type.GREATER_THAN:
                                    check = new Range(1, null);
                                    break;
                                case TokenCompare.Type.GREATER_OR_EQUAL:
                                    check = new Range(0, null);
                                    break;
                                default:
                                    check = new Range();
                                    break;
                            }
                            selector.scores.checks.Add(new ScoresEntry(temp, check));
                            // if <value> <comp> 1234.5
                        }
                        else if (tokens.NextIs<TokenNumberLiteral>())
                        {
                            TokenNumberLiteral number = tokens.Next<TokenNumberLiteral>();
                            var output = a.value.CompareToLiteral(a.word, entity, ctype, number);
                            selector.scores.checks.AddRange(output.Item1);
                            commands.AddRange(output.Item2);
                        }
                        else
                        {
                            executor.PopSelector();
                            throw new StatementException(tokens, "Attempted to compare value with invalid token.");
                        }
                    }
                    else break;
                }

                if (word.Equals("BLOCK"))
                {
                    Coord x = tokens.Next<TokenCoordinateLiteral>();
                    Coord y = tokens.Next<TokenCoordinateLiteral>();
                    Coord z = tokens.Next<TokenCoordinateLiteral>();
                    string block = tokens.Next<TokenStringLiteral>();
                    int? data = null;

                    if (tokens.NextIs<TokenIntegerLiteral>())
                        data = tokens.Next<TokenIntegerLiteral>();

                    BlockCheck blockCheck = new BlockCheck(x, y, z, block, data);

                    if (not)
                    {
                        ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                        commands.AddRange(new[] {
                            Command.ScoreboardSet(entity, inverter, 0),
                            blockCheck.AsStoreIn(entity, inverter)
                        });
                        selector.blockCheck = BlockCheck.DISABLED;
                        selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                    }
                    else
                        selector.blockCheck = blockCheck;
                }
                else if (word.Equals("NEAR"))
                {
                    Coord x = tokens.Next<TokenCoordinateLiteral>();
                    Coord y = tokens.Next<TokenCoordinateLiteral>();
                    Coord z = tokens.Next<TokenCoordinateLiteral>();
                    int radius = tokens.Next<TokenIntegerLiteral>();

                    int? minRadius = null;
                    if (tokens.NextIs<TokenIntegerLiteral>())
                        minRadius = tokens.Next<TokenIntegerLiteral>();

                    Area area = new Area(x, y, z, minRadius, radius);

                    if (not && minRadius != null)
                    {
                        ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                        commands.AddRange(new[] {
                            Command.ScoreboardSet(entity, inverter, 0),
                            area.AsStoreIn(entity, inverter)
                        });
                        selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                    }
                    else if (not)
                    {
                        area.radiusMin = area.radiusMax;
                        area.radiusMax = 999999f;
                        selector.area = area;
                    }
                    else
                        selector.area = area;
                }
                else if (word.Equals("INSIDE"))
                {
                    Coord x = tokens.Next<TokenCoordinateLiteral>();
                    Coord y = tokens.Next<TokenCoordinateLiteral>();
                    Coord z = tokens.Next<TokenCoordinateLiteral>();
                    int sizeX = tokens.Next<TokenIntegerLiteral>();
                    int sizeY = tokens.Next<TokenIntegerLiteral>();
                    int sizeZ = tokens.Next<TokenIntegerLiteral>();

                    Area area = new Area(x, y, z, null, null, sizeX, sizeY, sizeZ);

                    if (not)
                    {
                        ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                        commands.AddRange(new[] {
                            Command.ScoreboardSet(entity, inverter, 0),
                            area.AsStoreIn(entity, inverter)
                        });
                        selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                    }
                    else
                        selector.area = area;
                }
                else if (word.Equals("ROTATION"))
                {
                    char axis = char.ToUpper(tokens.Next<TokenIdentifier>().word[0]);
                    int rotMin = tokens.Next<TokenIntegerLiteral>();
                    int rotMax = tokens.Next<TokenIntegerLiteral>();
                    if(rotMin > rotMax)
                    {
                        int temp = rotMin;
                        rotMin = rotMax;
                        rotMax = temp;
                    }

                    if (axis == 'X')
                    {
                        if (not)
                        {
                            ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                            commands.AddRange(new[] {
                                Command.ScoreboardSet(entity, inverter, 0),
                                $"execute {entity}[rxm={rotMin},rx={rotMax}] ~~~ scoreboard players set @s {inverter.baseName} 1"
                            });
                            selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                        }
                        else
                        {
                            selector.entity.rotXMin = rotMin;
                            selector.entity.rotXMax = rotMax;
                        }
                    }
                    else if (axis == 'Y')
                    {
                        if (not)
                        {
                            ScoreboardValue inverter = executor.scoreboard.RequestTemp();

                            commands.AddRange(new[] {
                                Command.ScoreboardSet(entity, inverter, 0),
                                $"execute {entity}[rym={rotMin},ry={rotMax}] ~~~ scoreboard players set @s {inverter.baseName} 1"
                            });
                            selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                        }
                        else
                        {
                            selector.entity.rotYMin = rotMin;
                            selector.entity.rotYMax = rotMax;
                        }
                    }
                    else
                        throw new StatementException(tokens, "Invalid rotation axis. Valid options can be X or Y.");
                }
                else if (word.Equals("TYPE"))
                {
                    string type = tokens.Next<TokenStringLiteral>();
                    if (not) type = '!' + type;
                    selector.entity.type = type;
                }
                else if (word.Equals("FAMILY"))
                {
                    string family = tokens.Next<TokenStringLiteral>();
                    if (not) family = '!' + family;
                    selector.entity.family = family;
                }
                else if (word.Equals("TAG"))
                {
                    string tag = tokens.Next<TokenStringLiteral>();
                    selector.tags.Add(new Tag(tag, not));
                }
                else if (word.Equals("MODE"))
                {
                    GameMode gameMode;

                    if (tokens.NextIs<TokenIdentifierEnum>())
                        gameMode = (GameMode)tokens.Next<TokenIdentifierEnum>().value;
                    else
                        gameMode = (GameMode)tokens.Next<TokenIntegerLiteral>().number;

                    selector.player.gamemode = gameMode;
                    selector.player.gamemodeNot = not;
                }
                else if (word.Equals("LEVEL"))
                {
                    int levelMin = tokens.Next<TokenIntegerLiteral>();
                    int? levelMax = null;

                    if (tokens.NextIs<TokenIntegerLiteral>())
                        levelMax = tokens.Next<TokenIntegerLiteral>();

                    if (not && levelMax == null)
                    {
                        selector.player.levelMin = 0;
                        selector.player.levelMax = levelMin;
                    }
                    else if (not)
                    {
                        Player invertCondition = new Player(null, levelMin, levelMax);
                        ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                        commands.AddRange(new[] {
                            Command.ScoreboardSet(entity, inverter, 0),
                            invertCondition.AsStoreIn(entity, inverter)
                        });
                        selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                    }
                    else
                    {
                        selector.player.levelMin = levelMin;
                        selector.player.levelMax = levelMax;
                    }
                }
                else if (word.Equals("NAME"))
                {
                    string name = tokens.Next<TokenStringLiteral>();
                    selector.entity.nameNot = not;
                    selector.entity.name = name;
                }
                else if (word.Equals("LIMIT"))
                {
                    if (not)
                        throw new StatementException(tokens, "Cannot invert a limit check.");

                    int count = tokens.Next<TokenIntegerLiteral>();
                    selector.count = new Count(count);
                }
                // repeat all that as long as there continues to be an &
            } while (tokens.NextIs<TokenAnd>());

            if (commands.Count > 0)
                executor.AddCommandsClean(commands, "if_setup", true);

            // done with temporary variables
            executor.scoreboard.PopTempState();
            executor.PopSelector();

            // the selector is now ready to use and commands are setup
            executor.SetLastCompare(tokensUsed);
            string prefix = selector.GetAsPrefix();
            executor.AppendCommandPrepend(prefix);

            if (!executor.HasNext)
                throw new StatementException(tokens, "Unexpected end-of-file after if/else statement.");

            StatementOpenBlock opener = null;
            if (executor.NextIs<StatementOpenBlock>())
            {
                opener = executor.Peek<StatementOpenBlock>();

                // waste of a branching file, so treat as 1 statement.
                if (opener.statementsInside == 1)
                {
                    // skip open block
                    executor.Next();

                    // make close block only pop selector
                    StatementCloseBlock closer = executor.Peek<StatementCloseBlock>(1);
                    closer.popFile = false;
                    executor.PushSelector(true);
                    return;
                }
            }

            if (opener == null)
            {
                executor.PushSelector(true);
                executor.PopSelectorAfterNext();
            }
            else
            {
                CommandFile nextBranchFile = Executor.GetNextGeneratedFile("branch");
                opener.aligns = true;
                opener.shouldRun = true;
                opener.TargetFile = nextBranchFile;
                executor.AddCommand(Command.Function(nextBranchFile));
                return;
            }
        }
        public static void @else(Executor executor, Statement tokens)
        {
            Token[] toRun = executor.GetLastCompare();
            @if(executor, new StatementDirective(null, toRun), true);
        }
        public static void give(Executor executor, Statement tokens)
        {
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

            if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
            {
                count = tokens.Next<TokenIntegerLiteral>();

                if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                    data = tokens.Next<TokenIntegerLiteral>();
            }

            while (tokens.HasNext && tokens.NextIs<TokenBuilderIdentifier>())
            {
                TokenBuilderIdentifier builderIdentifier = tokens.Next<TokenBuilderIdentifier>();
                string builderField = builderIdentifier.builderField.ToUpper();

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
                        Enchantment enchantment = (Enchantment)tokens.Next<TokenIdentifierEnum>().value;
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
                StructureFile file = new StructureFile("item" + item.GetHashCode(), StructureNBT.SingleItem(item));
                executor.AddExtraFile(file);
                Selector active = executor.ActiveSelector;

                string cmd = Command.StructureLoad(file.name, Coord.here, Coord.here, Coord.here,
                    StructureRotation._0_degrees, StructureMirror.none, true, false);

                if (active.NeedsAlign)
                    executor.AddCommand(Command.Execute(active.ToString(), Coord.here, Coord.here, Coord.here, cmd));
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

            string command = Command.Give(executor.ActiveSelectorStr, itemName, count, data);
            if (json.Count > 0)
                command += $" {{{string.Join(",", json)}}}";

            executor.AddCommand(command);
        }
        public static void tp(Executor executor, Statement tokens)
        {
            executor.PushSelectorExecute();
            if (tokens.NextIs<TokenSelectorLiteral>())
            {
                TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
                executor.AddCommand(Command.Teleport(selector.selector.ToString()));
            }
            else
            {
                Coord x = tokens.Next<TokenCoordinateLiteral>();
                Coord y = tokens.Next<TokenCoordinateLiteral>();
                Coord z = tokens.Next<TokenCoordinateLiteral>();

                if (tokens.HasNext && tokens.NextIs<TokenCoordinateLiteral>())
                {
                    Coord ry = tokens.Next<TokenCoordinateLiteral>();
                    Coord rx = tokens.Next<TokenCoordinateLiteral>();
                    executor.AddCommand(Command.Teleport(executor.ActiveSelectorStr, x, y, z, ry, rx));
                }
                else
                    executor.AddCommand(Command.Teleport(executor.ActiveSelectorStr, x, y, z));
            }
            executor.PopSelector();
        }
        public static void tphere(Executor executor, Statement tokens)
        {
            Selector selector = tokens.Next<TokenSelectorLiteral>();
            Coord offsetX = Coord.here;
            Coord offsetY = Coord.here;
            Coord offsetZ = Coord.here;

            if (tokens.HasNext && tokens.NextIs<TokenCoordinateLiteral>())
            {
                offsetX = tokens.Next<TokenCoordinateLiteral>();
                offsetY = tokens.Next<TokenCoordinateLiteral>();
                offsetZ = tokens.Next<TokenCoordinateLiteral>();
            }

            executor.PushSelectorExecute();
            executor.AddCommand(Command.Teleport(selector.ToString(), offsetX, offsetY, offsetZ));
            executor.PopSelector();
        }
        public static void move(Executor executor, Statement tokens)
        {
            string direction = tokens.Next<TokenIdentifier>().word.ToUpper();
            float amount = tokens.Next<TokenNumberLiteral>().GetNumber();

            Coord x = Coord.herefacing;
            Coord y = Coord.herefacing;
            Coord z = Coord.herefacing;

            switch (direction)
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

            executor.PushSelectorExecute();
            executor.AddCommand(Command.Teleport(x, y, z));
            executor.PopSelector();
        }
        public static void face(Executor executor, Statement tokens)
        {
            executor.PushSelectorExecute();
            if (tokens.NextIs<TokenSelectorLiteral>())
            {
                TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
                executor.AddCommand(Command.TeleportFacing(Coord.here, Coord.here, Coord.here, selector.ToString()));
            }
            else
            {
                Coord x = tokens.Next<TokenCoordinateLiteral>();
                Coord y = tokens.Next<TokenCoordinateLiteral>();
                Coord z = tokens.Next<TokenCoordinateLiteral>();

                executor.AddCommand(Command.TeleportFacing(Coord.here, Coord.here, Coord.here, x, y, z));
            }
            executor.PopSelector();
        }
        public static void facehere(Executor executor, Statement tokens)
        {
            TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
            List<string> commands = new List<string>();

            // old dookie commands
            /*commands.AddRange(Command.UTIL.RequestPoint(out Selector point));
            commands.Add(Command.Execute(selector.ToString(), Coord.here, Coord.here, Coord.here,
                Command.TeleportFacing(Coord.here, Coord.here, Coord.here, point.ToString())));
            commands.AddRange(Command.UTIL.ReleasePoint());*/

            commands.Add(Command.Tag("@s", "_mcc_here"));
            commands.Add(Command.Execute(selector.selector.ToString(), Coord.here, Coord.here, Coord.here,
                Command.TeleportFacing(Coord.here, Coord.here, Coord.here, "@e[tag=\"_mcc_here\",c=1]")));
            commands.Add(Command.TagRemove("@s", "_mcc_here"));

            executor.PushSelectorExecute();
            executor.AddCommands(commands, "facehere");
            executor.PopSelector();
        }
        public static void rotate(Executor executor, Statement tokens)
        {
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

            executor.PushSelectorExecute();
            executor.AddCommand(Command.Teleport(Coord.here, Coord.here, Coord.here, ry, rx));
            executor.PopSelector();
        }
        public static void block(Executor executor, Statement tokens)
        {
            OldObjectHandling handling = OldObjectHandling.replace;

            if (tokens.NextIs<TokenIdentifierEnum>())
                handling = (OldObjectHandling)tokens.Next<TokenIdentifierEnum>().value;

            string block = tokens.Next<TokenStringLiteral>();
            Coord x = tokens.Next<TokenCoordinateLiteral>();
            Coord y = tokens.Next<TokenCoordinateLiteral>();
            Coord z = tokens.Next<TokenCoordinateLiteral>();

            int data = 0;
            if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                data = tokens.Next<TokenIntegerLiteral>();

            executor.PushSelectorExecute();
            executor.AddCommand(Command.SetBlock(x, y, z, block, data, handling));
            executor.PopSelector();
        }
        public static void fill(Executor executor, Statement tokens)
        {
            OldObjectHandling handling = OldObjectHandling.replace;

            if (tokens.NextIs<TokenIdentifierEnum>())
                handling = (OldObjectHandling)tokens.Next<TokenIdentifierEnum>().value;

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

            executor.PushSelectorExecute();
            executor.AddCommand(Command.Fill(x1, y1, z1, x2, y2, z2, block, data, handling));
            executor.PopSelector();
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

            if (x1.isRelative != y1.isRelative ||
                y1.isRelative != z1.isRelative ||
                z1.isRelative != x2.isRelative ||
                x2.isRelative != y2.isRelative ||
                y2.isRelative != z2.isRelative)
                throw new StatementException(tokens, "Scatter command requires all coordinate arguments to be relative or exact. (the size needs to be known at compile time.)");

            string seed = null;
            if (tokens.HasNext && tokens.NextIs<TokenStringLiteral>())
                seed = tokens.Next<TokenStringLiteral>();

            // generate a structure file for this zone.
            int sizeX = Math.Abs(x2.valuei - x1.valuei) + 1;
            int sizeY = Math.Abs(y2.valuei - y1.valuei) + 1;
            int sizeZ = Math.Abs(z2.valuei - z1.valuei) + 1;
            if (sizeX > 64 || sizeY > 256 || sizeZ > 64)
                throw new StatementException(tokens, "Scatter zone size cannot be larger than 64x256x64.");

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
            StructureFile file = new StructureFile(fileName, structure);
            executor.WriteFileNow(file);

            blocks = null;
            structure = default;
            file = default;
            GC.Collect();

            Coord minX = Coord.Min(x1, x2);
            Coord minY = Coord.Min(y1, y2);
            Coord minZ = Coord.Min(z1, z2);

            if (seed == null)
            {
                executor.PushSelectorExecute();
                executor.AddCommand(Command.StructureLoad(fileName, minX, minY, minZ,
                    StructureRotation._0_degrees, StructureMirror.none, false, true, percent));
                executor.PopSelector();
            }
            else
            {
                executor.PushSelectorExecute();
                executor.AddCommand(Command.StructureLoad(fileName, minX, minY, minZ,
                    StructureRotation._0_degrees, StructureMirror.none, false, true, percent, seed));
                executor.PopSelector();
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

            executor.PushSelectorExecute();
            executor.AddCommand(Command.Fill(x1, y1, z1, x2, y2, z2, src, srcData, dst, dstData));
            executor.PopSelector();
        }
        public static void kill(Executor executor, Statement tokens)
        {
            if (tokens.HasNext && tokens.NextIs<TokenSelectorLiteral>())
            {
                Selector selector = tokens.Next<TokenSelectorLiteral>();
                executor.AddCommand(Command.Kill(selector.ToString()));
                return;
            }
            executor.AddCommand(Command.Kill(executor.ActiveSelectorStr));
        }
        public static void remove(Executor executor, Statement tokens)
        {
            CommandFile file = new CommandFile("silent_remove", "_branching");
            file.Add(new[] {
                Command.Teleport(Coord.here, new Coord(-9999, false, true, false), Coord.here),
                Command.Kill()
            });
            executor.DefineSTDFile(file);

            if (tokens.HasNext && tokens.NextIs<TokenSelectorLiteral>())
            {
                Selector selector = tokens.Next<TokenSelectorLiteral>();
                executor.AddCommand(Command.Execute(selector.ToString(),
                    Coord.here, Coord.here, Coord.here, Command.Function(file)));
                return;
            }

            executor.PushSelectorExecute();
            executor.AddCommand(Command.Function(file));
            executor.PopSelector();
        }
        public static void globaltitle(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenIdentifier>())
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
                    List<JSONRawTerm> terms = executor.FString(str, out bool advanced);
                    string[] commands;

                    if (advanced)
                        commands = executor.ResolveRawText(terms, Command.Execute("@a", Coord.here, Coord.here, Coord.here, "titleraw @s subtitle "));
                    else
                        commands = executor.ResolveRawText(terms, "titleraw @a subtitle ");

                    executor.AddCommands(commands, "subtitle");
                    return;
                }
                else
                    throw new StatementException(tokens, $"Invalid globaltitle subcommand '{word}'. Must be 'times' or 'subtitle'.");
            }

            if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                List<JSONRawTerm> terms = executor.FString(str, out bool advanced);
                string[] commands;

                if (advanced)
                    commands = executor.ResolveRawText(terms, Command.Execute("@a", Coord.here, Coord.here, Coord.here, "title @s title "));
                else
                    commands = executor.ResolveRawText(terms, "titleraw @a title ");

                executor.AddCommands(commands, "title");
                return;
            }
        }
        public static void title(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenIdentifier>())
            {
                string word = tokens.Next<TokenIdentifier>().word.ToUpper();
                if (word.Equals("TIMES"))
                {
                    int fadeIn = tokens.Next<TokenIntegerLiteral>();
                    int stay = tokens.Next<TokenIntegerLiteral>();
                    int fadeOut = tokens.Next<TokenIntegerLiteral>();
                    executor.AddCommand(Command.TitleTimes(executor.ActiveSelectorStr, fadeIn, stay, fadeOut));
                    return;
                }
                else if (word.Equals("SUBTITLE"))
                {
                    string str = tokens.Next<TokenStringLiteral>();
                    List<JSONRawTerm> terms = executor.FString(str, out bool advanced);
                    string[] commands;

                    if (advanced)
                    {
                        executor.PushSelectorExecute();
                        string selector = executor.ActiveSelectorStr;
                        commands = executor.ResolveRawText(terms, $"titleraw {selector} subtitle ");
                        executor.PopSelector();
                    }
                    else
                    {
                        string selector = executor.ActiveSelectorStr;
                        commands = executor.ResolveRawText(terms, $"titleraw {selector} subtitle ");
                    }

                    executor.AddCommands(commands, "subtitle");
                    return;
                }
                else
                    throw new StatementException(tokens, $"Invalid title subcommand '{word}'. Must be 'times' or 'subtitle'.");
            }

            if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                List<JSONRawTerm> terms = executor.FString(str, out bool advanced);
                string[] commands;

                if (advanced)
                {
                    executor.PushSelectorExecute();
                    string selector = executor.ActiveSelectorStr;
                    commands = executor.ResolveRawText(terms, $"titleraw {selector} title ");
                    executor.PopSelector();
                }
                else
                {
                    string selector = executor.ActiveSelectorStr;
                    commands = executor.ResolveRawText(terms, $"titleraw {selector} title ");
                }

                executor.AddCommands(commands, "title");
                return;
            }
        }
        public static void globalactionbar(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenIdentifier>())
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
                List<JSONRawTerm> terms = executor.FString(str, out bool advanced);
                string[] commands;

                if (advanced)
                    commands = executor.ResolveRawText(terms, Command.Execute("@a", Coord.here, Coord.here, Coord.here, "titleraw @s actionbar "));
                else
                    commands = executor.ResolveRawText(terms, "titleraw @a actionbar ");

                executor.AddCommands(commands, "actionbar");
                return;
            }
            else throw new StatementException(tokens, "Invalid information given to globalactionbar.");
        }
        public static void actionbar(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenIdentifier>())
            {
                string word = tokens.Next<TokenIdentifier>().word.ToUpper();
                int fadeIn = tokens.Next<TokenIntegerLiteral>();
                int stay = tokens.Next<TokenIntegerLiteral>();
                int fadeOut = tokens.Next<TokenIntegerLiteral>();
                executor.AddCommand(Command.TitleTimes(executor.ActiveSelectorStr, fadeIn, stay, fadeOut));
                return;
            }
            else if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                List<JSONRawTerm> terms = executor.FString(str, out bool advanced);
                string[] commands;

                if (advanced)
                {
                    executor.PushSelectorExecute();
                    string selector = executor.ActiveSelectorStr;
                    commands = executor.ResolveRawText(terms, $"titleraw {selector} actionbar ");
                    executor.PopSelector();
                }
                else
                {
                    string selector = executor.ActiveSelectorStr;
                    commands = executor.ResolveRawText(terms, $"titleraw {selector} actionbar ");
                }

                executor.AddCommands(commands, "actionbar");
                return;
            }
            else throw new StatementException(tokens, "Invalid information given to actionbar.");
        }
        public static void say(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();

            executor.PushSelectorExecute();
            executor.AddCommand(Command.Say(str));
            executor.PopSelector();
        }
        public static void halt(Executor executor, Statement tokens)
        {
            CommandFile file = new CommandFile("halt_execution", Executor.MCC_GENERATED);

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
            int damage = tokens.Next<TokenIntegerLiteral>();
            DamageCause cause = DamageCause.all;
            Selector blame = null;

            if(tokens.NextIs<TokenIdentifierEnum>())
            {
                object value = tokens.Next<TokenIdentifierEnum>();
                if (!(value is DamageCause))
                    throw new StatementException(tokens, $"Invalid value given for damage cause: {value.ToString()}");
                cause = (DamageCause)value;
            }
            if (tokens.NextIs<TokenSelectorLiteral>())
            {
                TokenSelectorLiteral value = tokens.Next<TokenSelectorLiteral>();
                blame = value.selector;
            }

            string command;
            if (blame == null)
                command = Command.Damage(executor.ActiveSelectorStr, damage, cause);
            else
            {
                if (blame.SelectsMultiple)
                    blame.count = new Count(1);
                command = Command.Damage(executor.ActiveSelectorStr, damage, cause, blame.ToString());
            }

            executor.AddCommand(command);
        }

        public static void function(Executor executor, Statement tokens)
        {
            string functionName = tokens.Next<TokenIdentifier>().word;
            List<ScoreboardValue> args = new List<ScoreboardValue>();
            List<Token> defaults = new List<Token>(); // default values
            bool mustHaveDefault = false;

            if (tokens.NextIs<TokenOpenParenthesis>())
                tokens.Next();

            // this is where the directive takes in function parameters
            while (tokens.HasNext && tokens.NextIs<TokenIdentifier>())
            {
                var def = executor.scoreboard.GetNextValueDefinition(tokens);
                ScoreboardValue value = def.Create(executor.scoreboard, tokens);
                executor.scoreboard.Add(value);
                args.Add(value);

                if (def.defaultValue == null)
                {
                    if (mustHaveDefault)
                        throw new StatementException(tokens, "All parameters following a parameter with a default must also have defaults.");
                    defaults.Add(null);
                }
                else
                {
                    mustHaveDefault = true;
                    defaults.Add(def.defaultValue);
                }
            }

            Function function = new Function(functionName).AddParameters(args, defaults);
            executor.RegisterFunction(function);

            if (executor.NextIs<StatementOpenBlock>())
            {
                StatementOpenBlock openBlock = executor.Peek<StatementOpenBlock>();
                openBlock.aligns = true;
                openBlock.shouldRun = true;
                openBlock.TargetFile = function.File;
                return;
            }
            else
                throw new StatementException(tokens, "No block following function definition.");
        }
        public static void @return(Executor executor, Statement tokens)
        {
            Function activeFunction = executor.CurrentFile.userFunction;

            if (activeFunction == null)
                throw new StatementException(tokens, "Cannot return a value outside of a function.");

            string selector = executor.ActiveSelectorStr;

            if (tokens.NextIs<TokenIdentifierValue>())
            {
                TokenIdentifierValue token = tokens.Next<TokenIdentifierValue>();
                activeFunction.TryReturnValue(tokens, token.value, executor, selector);
            }
            else
            {
                TokenLiteral token = tokens.Next<TokenLiteral>();
                activeFunction.TryReturnValue(tokens, executor, token, selector);
            }
        }
        public static void @struct(Executor executor, Statement tokens)
        {
            string structName = tokens.Next<TokenIdentifier>().word;
            StructDefinition item = new StructDefinition(structName);

            if (!executor.HasNext || !executor.NextIs<StatementOpenBlock>())
                throw new StatementException(tokens, "No block after struct definition.");

            StatementOpenBlock blockOpen = executor.Next<StatementOpenBlock>();
            int count = blockOpen.statementsInside;

            StructDefinition definition = new StructDefinition(structName);

            // read every statement in the block assuming they're define format
            for (int i = 0; i < count; i++)
            {
                Statement statement = executor.Next();
                ScoreboardManager.ValueDefinition def = executor
                    .scoreboard.GetNextValueDefinition(statement);
                ScoreboardValue value = def.Create(executor.scoreboard, statement);
                string key = definition.GetNextKey();
                value.baseName = key;
                definition.fields[def.name] = value;
            }

            executor.scoreboard.DefineStruct(definition);

            if (!executor.HasNext)
                throw new StatementException(tokens, "Unexpected end-of-file following struct definition.");
            executor.Next<StatementCloseBlock>();
        }
    }
}