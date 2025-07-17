using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Native;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler.Async;
using mc_compiled.MCC.Compiler.TypeSystem;
using mc_compiled.MCC.CustomEntities;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using mc_compiled.Modding;
using mc_compiled.Modding.Behaviors.Dialogue;
using mc_compiled.Modding.Resources;
using mc_compiled.Modding.Resources.Localization;
using mc_compiled.NBT;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using Range = mc_compiled.Commands.Range;

namespace mc_compiled.MCC.Compiler;

public static class DirectiveImplementations
{
    public static void ResetState()
    {
        // empty
    }

    // ReSharper disable once ReturnTypeCanBeEnumerable.Local
    /// <summary>
    ///     Gathers either a preprocessor variable's values, or a list of dynamic values in the source file.
    /// </summary>
    /// <param name="executor">The executor object used for executing statements.</param>
    /// <param name="tokens">The statement tokens.</param>
    /// <param name="allowUnwrapping">
    ///     Allow being able to unwrap the input PPV if it is a single JSON Array with only raw
    ///     objects.
    /// </param>
    /// <param name="forcePPV">Optional. The name of the preprocessor variable to force fetch rather than pull from `tokens`.</param>
    /// <returns>An array of dynamic values.</returns>
    /// <exception cref="StatementException">Thrown when a preprocessor variable with the provided identifier is not found.</exception>
    private static dynamic[] FetchPPVOrDynamics(Executor executor,
        Statement tokens,
        bool allowUnwrapping,
        string forcePPV = null)
    {
        string ppvName = null;

        if (forcePPV != null)
            ppvName = forcePPV;
        else if (tokens.NextIs<TokenIdentifier>(false, false))
            ppvName = tokens.Next<TokenIdentifier>("ppv identifier").word;

        // if ppv name was either specified or forced, get and (optionally) unwrap it.
        if (ppvName != null)
            return GetPPVAndUnwrap(executor, tokens, allowUnwrapping, ppvName);

        // pull tokens until they're no longer able to be stored in a ppv.
        var others = new List<dynamic>();

        while (tokens.NextIs<IPreprocessor>(false))
        {
            object aboutToAdd = tokens.Next<IPreprocessor>("preprocessor-supported value").GetValue();

            // unwrap array, if possible and allowed.
            if (allowUnwrapping && aboutToAdd is JArray jsonArray)
                if (jsonArray.All(PreprocessorUtils.CanTokenBeUnwrapped))
                {
                    others.AddRange(jsonArray
                        .Select(unwrap =>
                        {
                            PreprocessorUtils.TryUnwrapToken(unwrap, out object obj);
                            return obj;
                        }));
                    continue;
                }

            others.Add(aboutToAdd);
        }

        return others.ToArray();
    }
    private static dynamic[] GetPPVAndUnwrap(Executor executor, Statement tokens, bool allowUnwrapping, string ppvName)
    {
        if (!executor.TryGetPPV(ppvName, out PreprocessorVariable ppv))
            throw new StatementException(tokens, $"Couldn't find preprocessor variable named '{ppvName}'.");

        if (!allowUnwrapping)
            return ppv.ToArray();

        if (ppv.Length != 1 || ppv[0] is not JArray jsonArray)
            return ppv.ToArray();

        if (!jsonArray.All(PreprocessorUtils.CanTokenBeUnwrapped))
            return ppv.ToArray();

        return jsonArray
            .Select(unwrap =>
            {
                PreprocessorUtils.TryUnwrapToken(unwrap, out object obj);
                return obj;
            })
            .ToArray();
    }

    [UsedImplicitly]
    public static void _var(Executor executor, Statement tokens)
    {
        string varName = tokens.Next<TokenIdentifier>("variable name").word;
        varName.ThrowIfWhitespace("variable name", tokens);

        if (tokens.NextIs<IPreprocessor>(true))
        {
            var values = new List<dynamic>();
            while (tokens.NextIs<IPreprocessor>(true))
                values.Add(tokens.Next<IPreprocessor>("variable value").GetValue());
            executor.SetPPV(varName, values.ToArray());
        }
        else
        {
            // empty preprocessor variable definition
            executor.SetPPV(varName);
        }
    }
    [UsedImplicitly]
    public static void _inc(Executor executor, Statement tokens)
    {
        string varName = tokens.Next<TokenIdentifier>("variable name").word;
        varName.ThrowIfWhitespace("variable name", tokens);

        if (executor.TryGetPPV(varName, out PreprocessorVariable value))
            try
            {
                for (int i = 0; i < value.Length; i++)
                    value[i] += 1;
            }
            catch (Exception)
            {
                throw new StatementException(tokens, "Couldn't increment this value.");
            }
        else
            throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
    }
    [UsedImplicitly]
    public static void _dec(Executor executor, Statement tokens)
    {
        string varName = tokens.Next<TokenIdentifier>("variable name").word;
        varName.ThrowIfWhitespace("variable name", tokens);

        if (executor.TryGetPPV(varName, out PreprocessorVariable value))
            try
            {
                for (int i = 0; i < value.Length; i++)
                    value[i] -= 1;
            }
            catch (Exception)
            {
                throw new StatementException(tokens, "Couldn't decrement this value.");
            }
        else
            throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
    }
    [UsedImplicitly]
    public static void _add(Executor executor, Statement tokens)
    {
        string varName = tokens.Next<TokenIdentifier>("variable name").word;
        varName.ThrowIfWhitespace("variable name", tokens);

        dynamic[] a = FetchPPVOrDynamics(executor, tokens, false, varName);
        dynamic[] b = FetchPPVOrDynamics(executor, tokens, true);

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
        string varName = tokens.Next<TokenIdentifier>("variable name").word;
        varName.ThrowIfWhitespace("variable name", tokens);

        dynamic[] a = FetchPPVOrDynamics(executor, tokens, false, varName);
        dynamic[] b = FetchPPVOrDynamics(executor, tokens, true);

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
        string varName = tokens.Next<TokenIdentifier>("variable name").word;
        varName.ThrowIfWhitespace("variable name", tokens);

        dynamic[] a = FetchPPVOrDynamics(executor, tokens, false, varName);
        dynamic[] b = FetchPPVOrDynamics(executor, tokens, true);

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
        string varName = tokens.Next<TokenIdentifier>("variable name").word;
        varName.ThrowIfWhitespace("variable name", tokens);

        dynamic[] a = FetchPPVOrDynamics(executor, tokens, false, varName);
        dynamic[] b = FetchPPVOrDynamics(executor, tokens, true);

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
        string varName = tokens.Next<TokenIdentifier>("variable name").word;
        varName.ThrowIfWhitespace("variable name", tokens);

        dynamic[] a = FetchPPVOrDynamics(executor, tokens, false, varName);
        dynamic[] b = FetchPPVOrDynamics(executor, tokens, true);

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
        string varName = tokens.Next<TokenIdentifier>("variable name").word;
        varName.ThrowIfWhitespace("variable name", tokens);

        dynamic[] a = FetchPPVOrDynamics(executor, tokens, false, varName);
        dynamic[] b = FetchPPVOrDynamics(executor, tokens, true);

        int bIndex = 0;
        dynamic[] result = new dynamic[a.Length];

        for (int aIndex = 0; aIndex < a.Length; aIndex++)
        {
            dynamic aValue = a[aIndex];
            bIndex %= b.Length;
            dynamic bValue = b[bIndex++];

            if (bValue is not int count)
                throw new StatementException(tokens, "Can only exponentiate using an integer value.");

            try
            {
                result[aIndex] = aValue;
                for (int x = 1; x < count; x++)
                    result[aIndex] *= aValue;
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
        string aName = tokens.Next<TokenIdentifier>("variable name a").word;
        string bName = tokens.Next<TokenIdentifier>("variable name b").word;

        aName.ThrowIfWhitespace("variable name a", tokens);
        bName.ThrowIfWhitespace("variable name b", tokens);

        if (aName.Equals(bName))
            return;

        if (executor.TryGetPPV(aName, out PreprocessorVariable a))
        {
            if (executor.TryGetPPV(bName, out PreprocessorVariable b))
            {
                executor.SetPPVCopy(aName, b);
                executor.SetPPVCopy(bName, a);
            }
            else
            {
                throw new StatementException(tokens, "Preprocessor variable '" + bName + "' does not exist.");
            }
        }
        else
        {
            throw new StatementException(tokens, "Preprocessor variable '" + aName + "' does not exist.");
        }
    }
    [UsedImplicitly]
    public static void _append(Executor executor, Statement tokens)
    {
        string ppvName = tokens.Next<TokenIdentifier>("variable name").word;
        ppvName.ThrowIfWhitespace("variable name", tokens);

        if (!executor.TryGetPPV(ppvName, out PreprocessorVariable modify))
            throw new StatementException(tokens, $"Couldn't find preprocessor variable named '{ppvName}'.");

        dynamic[] items = FetchPPVOrDynamics(executor, tokens, true);

        if (items.Length == 1)
            modify.Append(items[0]);
        else
            modify.AppendRange(items);
    }
    [UsedImplicitly]
    public static void _prepend(Executor executor, Statement tokens)
    {
        string ppvName = tokens.Next<TokenIdentifier>("variable name").word;
        ppvName.ThrowIfWhitespace("variable name", tokens);

        if (!executor.TryGetPPV(ppvName, out PreprocessorVariable modify))
            throw new StatementException(tokens, $"Couldn't find preprocessor variable named '{ppvName}'.");

        dynamic[] items = FetchPPVOrDynamics(executor, tokens, true);

        if (items.Length == 1)
            modify.Prepend(items[0]);
        else
            modify.PrependRange(items);
    }
    [UsedImplicitly]
    public static void _if(Executor executor, Statement tokens)
    {
        dynamic[] tokensA = FetchPPVOrDynamics(executor, tokens, true);
        TokenCompare.Type compare = tokens.Next<TokenCompare>("comparison operator").GetCompareType();
        dynamic[] tokensB = FetchPPVOrDynamics(executor, tokens, true);

        // if the next block/statement should be run
        bool result = true;

        if (tokensA.Length != tokensB.Length)
        {
            if (compare != TokenCompare.Type.NOT_EQUAL)
                throw new StatementException(tokens,
                    "Lengths of left and right sides didn't match, and thus could not be compared.");
        }
        else
        {
            for (int i = 0; i < tokensA.Length; i++)
            {
                dynamic a = tokensA[i];
                dynamic b = tokensB[i];

                try
                {
                    result &= compare switch
                    {
                        TokenCompare.Type.EQUAL => a == b,
                        TokenCompare.Type.NOT_EQUAL => a != b,
                        TokenCompare.Type.LESS => a < b,
                        TokenCompare.Type.LESS_OR_EQUAL => a <= b,
                        TokenCompare.Type.GREATER => a > b,
                        TokenCompare.Type.GREATER_OR_EQUAL => a >= b,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                catch (Exception)
                {
                    throw new StatementException(tokens, "Could not compare those two types.");
                }
            }
        }

        if (!executor.HasNext)
            throw new StatementException(tokens, "End of file after $if statement.");

        executor.SetLastIfResult(result);

        if (executor.NextIs<StatementOpenBlock>())
        {
            var block = executor.Peek<StatementOpenBlock>();
            block.ignoreAsync = true;

            if (result)
                block.openAction = null;
            else
                block.openAction = e =>
                {
                    for (int i = 0; i < block.statementsInside; i++)
                        e.Next();
                };

            block.CloseAction = null;
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
            block.ignoreAsync = true;
            if (run)
            {
                block.openAction = null;
                block.CloseAction = null;
            }
            else
            {
                block.openAction = e =>
                {
                    block.CloseAction = null;
                    for (int i = 0; i < block.statementsInside; i++)
                        e.Next();
                };
            }
        }
        else if (!run)
        {
            executor.Next(); // skip the next statement
        }
    }
    [UsedImplicitly]
    public static void _assert(Executor executor, Statement tokens)
    {
        dynamic[] tokensA = FetchPPVOrDynamics(executor, tokens, true);
        TokenCompare.Type compare = tokens.Next<TokenCompare>("comparison operator").GetCompareType();
        dynamic[] tokensB = FetchPPVOrDynamics(executor, tokens, true);

        string message = tokens.NextIs<TokenStringLiteral>(true)
            ? tokens.Next<TokenStringLiteral>("message").text
            : null;

        // if the assertion passed
        bool result = true;

        // the 'message' parameter may have gotten swept up in the
        // FetchPPVOrDynamics call, so we'll try to extract it here.
        if (tokensA.Length == tokensB.Length - 1)
        {
            dynamic lastToken = tokensB.Last();
            if (lastToken is string messageLiteral)
            {
                tokensB = tokensB.Take(tokensB.Length - 1).ToArray();
                message = messageLiteral;
            }
        }

        if (tokensA.Length != tokensB.Length)
        {
            if (compare != TokenCompare.Type.NOT_EQUAL)
                throw new StatementException(tokens,
                    "Lengths of left and right sides didn't match, and thus could not be compared.");
        }
        else
        {
            for (int i = 0; i < tokensA.Length; i++)
            {
                dynamic a = tokensA[i];
                dynamic b = tokensB[i];

                try
                {
                    result &= compare switch
                    {
                        TokenCompare.Type.EQUAL => a == b,
                        TokenCompare.Type.NOT_EQUAL => a != b,
                        TokenCompare.Type.LESS => a < b,
                        TokenCompare.Type.LESS_OR_EQUAL => a <= b,
                        TokenCompare.Type.GREATER => a > b,
                        TokenCompare.Type.GREATER_OR_EQUAL => a >= b,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                catch (RuntimeBinderException)
                {
                    throw new StatementException(tokens, "Could not compare those two types.");
                }
            }
        }

        if (result)
            return;

        if (message == null)
        {
            string leftSide = tokensA.Length > 1 ? $"[{string.Join(", ", tokensA)}]" : tokensA[0].ToString();
            string rightSide = tokensB.Length > 1 ? $"[{string.Join(", ", tokensB)}]" : tokensB[0].ToString();
            throw new StatementException(tokens, $"Assertion failed: {leftSide} {compare} {rightSide}");
        }

        throw new StatementException(tokens, message);
    }
    [UsedImplicitly]
    public static void _repeat(Executor executor, Statement tokens)
    {
        bool useRange;
        Range range = default;
        int amount;

        if (tokens.NextIs<TokenIntegerLiteral>(false))
        {
            useRange = false;
            amount = tokens.Next<TokenIntegerLiteral>("repetitions").number;
        }
        else
        {
            range = tokens.Next<TokenRangeLiteral>("range").range;
            useRange = !range.single;
            amount = range.min.GetValueOrDefault();

            if (range.IsUnbounded)
                throw new StatementException(tokens, "Range parameter must have a start and end when used in $repeat.");
        }

        string tracker = null;

        if (tokens.NextIs<TokenIdentifier>(true))
        {
            tracker = tokens.Next<TokenIdentifier>("variable name").word;
            tracker.ThrowIfWhitespace("variable name", tokens);
        }

        Statement[] statements = executor.NextExecutionSet(true);

        if (useRange)
        {
            if (range.min == null || range.max == null)
                throw new StatementException(tokens,
                    $"Iterating over a range must have minimum and maximum bounds. (got {range})");

            int min = range.min.Value;
            int max = range.max.Value;
            for (int i = min; i <= max; i++)
            {
                if (tracker != null)
                    executor.SetPPV(tracker, i);
                executor.ExecuteSubsection(statements);
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                if (tracker != null)
                    executor.SetPPV(tracker, i);
                executor.ExecuteSubsection(statements);
            }
        }
    }
    [UsedImplicitly]
    public static void _log(Executor executor, Statement tokens)
    {
        var strings = new List<string>();

        while (tokens.HasNext)
        {
            Token next = tokens.Next();

            if (next is IPreprocessor preprocessor)
                strings.Add(preprocessor.GetValue().ToString());
            else
                strings.Add(next.DebugString());
        }

        if (executor.emission.isLinting)
            return;

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
    private static void _macrodefine(Executor executor, Statement tokens)
    {
        string macroName = tokens.Next<TokenIdentifier>("macro name").word;
        macroName.ThrowIfWhitespace("macro name", tokens);
        string docs = executor.GetDocumentationString(out _);

        var args = new List<string>();
        while (tokens.NextIs<TokenIdentifier>(false))
        {
            string argName = tokens.Next<TokenIdentifier>("macro argument name").word;
            argName.ThrowIfWhitespace("macro argument name", tokens);
            args.Add(argName);
        }

        var block = executor.Next<StatementOpenBlock>();
        block.ignoreAsync = true;
        int count = block.statementsInside;
        Statement[] statements = executor.Peek(count);

        for (int i = 0; i < count; i++)
            executor.Next(); // skip over those

        executor.Next<StatementCloseBlock>();
        var macro = new Macro(macroName, docs, args.ToArray(), statements, executor.isLibrary);
        executor.RegisterMacro(macro);
    }
    private static void _macrocall(Executor executor, Statement tokens)
    {
        string macroName = tokens.Next<TokenIdentifier>("macro name").word;
        macroName.ThrowIfWhitespace("macro name", tokens);
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

            if (tokens.NextIs<TokenIdentifierPreprocessor>(false))
            {
                // ReSharper disable once RedundantEnumerableCastCall
                args[i] = executor
                    .ResolvePPV(tokens.Next<TokenIdentifierPreprocessor>("preprocessor"), tokens)
                    .Cast<dynamic>()
                    .ToArray();
                continue;
            }

            if (!tokens.NextIs<IPreprocessor>(false))
                throw new StatementException(tokens, "Invalid argument type for '" + argNames[i] + "' in macro call.");

            args[i] = [tokens.Next<IPreprocessor>(null).GetValue()];
        }

        // save variables which collide with this macro's args.
        var collidedValues = new Dictionary<string, PreprocessorVariable>();
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
        }
        catch (StatementException e)
        {
            if (lookedUp.isFromLibrary)
            {
                // only show the error at the call site, since the macro's code is located out-of-file.
                e.statement.SetSource(tokens.Lines, tokens.Source + ": " + e.statement.Source, tokens.SourceFile);
            }
            else
            {
                // set it so that the error is placed at the location of the call and the macro.
                int[] exceptionLines = e.statement.Lines.Concat(tokens.Lines).ToArray();
                string exceptionSource = tokens.Source + ": " + e.statement.Source;
                e.statement.SetSource(exceptionLines, exceptionSource, tokens.SourceFile);
            }

            throw;
        }

        // restore variables
        foreach (KeyValuePair<string, PreprocessorVariable> kv in collidedValues)
            executor.ppv[kv.Key] = kv.Value;
    }

    [UsedImplicitly]
    public static void _include(Executor executor, Statement tokens)
    {
        const string LIBS_FOLDER = "libs";
        string file = tokens.Next<TokenStringLiteral>("file name");

        if (!file.EndsWith(".mcc"))
            file += ".mcc";

        if (!File.Exists(file))
        {
            // try checking TemporaryFilesManager
            file = Path.Combine(LIBS_FOLDER, file);
            if (TemporaryFilesManager.HasFile(file))
                file = TemporaryFilesManager.ResolveFile(file);
            else
                throw new StatementException(tokens, "Cannot find file/library '" + file + "'.");
        }
        else
        {
            file = Path.GetFullPath(file);
        }

        executor.workspace.OpenFile(file);
        Statement[] statements = executor.workspace.GetParsedStatements(file, false);

        string previousDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = Path.GetDirectoryName(file) ?? previousDirectory;
        executor.ExecuteLibrary(statements);
        Environment.CurrentDirectory = previousDirectory;
    }
    [UsedImplicitly]
    public static void _strfriendly(Executor executor, Statement tokens)
    {
        string output = tokens.Next<TokenIdentifier>("output/input").word;
        output.ThrowIfWhitespace("output", tokens);
        string input = tokens.NextIs<TokenIdentifier>(false, false)
            ? tokens.Next<TokenIdentifier>("input").word
            : output;

        if (executor.TryGetPPV(input, out PreprocessorVariable value))
        {
            dynamic[] results = new dynamic[value.Length];
            for (int r = 0; r < value.Length; r++)
            {
                if (value[r] is not string str)
                    continue;

                string[] words = str.Split('_', '-', ' ');

                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];

                    // edge case: if a word starts with capital and ends with lowercase, it's already fine
                    if (char.IsUpper(word[0]) && char.IsLower(word[^1]))
                        continue;

                    // edge case: short words should not be capitalized
                    // ...unless it's at the start of the string
                    if (word.Length <= 3 && i > 0)
                    {
                        words[i] = word.ToLower();
                        continue;
                    }

                    bool doUpperCase = true;
                    char[] chars = word.ToCharArray()
                        .Select(c =>
                        {
                            // don't do anything if it's not a letter
                            if (!char.IsLetter(c))
                                return c;
                            // lower-case letter
                            if (!doUpperCase)
                                return char.ToLower(c);
                            // upper-case letter (first letter character)
                            doUpperCase = false;
                            return char.ToUpper(c);
                        }).ToArray();

                    words[i] = new string(chars);
                }

                results[r] = string.Join(" ", words);
            }

            executor.SetPPV(output, results);
        }
        else
        {
            throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
    }
    [UsedImplicitly]
    public static void _strupper(Executor executor, Statement tokens)
    {
        string output = tokens.Next<TokenIdentifier>("output/input").word;
        output.ThrowIfWhitespace("output", tokens);
        string input = tokens.NextIs<TokenIdentifier>(false, false)
            ? tokens.Next<TokenIdentifier>("input").word
            : output;

        if (executor.TryGetPPV(input, out PreprocessorVariable value))
        {
            dynamic[] results = new dynamic[value.Length];
            for (int r = 0; r < value.Length; r++)
            {
                if (value[r] is not string str)
                    continue;
                results[r] = str.ToUpper();
            }

            executor.SetPPV(output, results);
        }
        else
        {
            throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
    }
    [UsedImplicitly]
    public static void _strlower(Executor executor, Statement tokens)
    {
        string output = tokens.Next<TokenIdentifier>("output/input").word;
        output.ThrowIfWhitespace("output", tokens);
        string input = tokens.NextIs<TokenIdentifier>(false, false)
            ? tokens.Next<TokenIdentifier>("input").word
            : output;

        if (executor.TryGetPPV(input, out PreprocessorVariable value))
        {
            dynamic[] results = new dynamic[value.Length];
            for (int r = 0; r < value.Length; r++)
            {
                if (value[r] is not string str)
                    continue;
                results[r] = str.ToLower();
            }

            executor.SetPPV(output, results);
        }
        else
        {
            throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
    }
    [UsedImplicitly]
    public static void _sum(Executor executor, Statement tokens)
    {
        string output = tokens.Next<TokenIdentifier>("output/input").word;
        output.ThrowIfWhitespace("output", tokens);
        string input = tokens.NextIs<TokenIdentifier>(false, false)
            ? tokens.Next<TokenIdentifier>("input").word
            : output;

        if (executor.TryGetPPV(input, out PreprocessorVariable values))
            try
            {
                dynamic result = values[0];
                for (int i = 1; i < values.Length; i++)
                    result += values[i];
                executor.SetPPV(output, result);
            }
            catch (RuntimeBinderException)
            {
                throw new StatementException(tokens, "Couldn't add these values.");
            }
        else
            throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
    }
    [UsedImplicitly]
    public static void _median(Executor executor, Statement tokens)
    {
        string output = tokens.Next<TokenIdentifier>("output/input").word;
        output.ThrowIfWhitespace("output", tokens);
        string input = tokens.NextIs<TokenIdentifier>(false, false)
            ? tokens.Next<TokenIdentifier>("input").word
            : output;

        if (executor.TryGetPPV(input, out PreprocessorVariable values))
            try
            {
                int len = values.Length;
                if (len == 1)
                {
                    executor.SetPPV(output, [values[0]]);
                }
                else if (len % 2 == 0)
                {
                    int mid = len / 2;
                    dynamic first = values[mid];
                    dynamic second = values[mid - 1];
                    dynamic result = (first + second) / 2;
                    executor.SetPPV(output, [result]);
                }
                else
                {
                    dynamic result = values[len / 2]; // truncates to middle index
                    executor.SetPPV(output, [result]);
                }
            }
            catch (RuntimeBinderException)
            {
                throw new StatementException(tokens, "Couldn't calculate median of these values.");
            }
        else
            throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
    }
    [UsedImplicitly]
    public static void _mean(Executor executor, Statement tokens)
    {
        string output = tokens.Next<TokenIdentifier>("output/input").word;
        output.ThrowIfWhitespace("output", tokens);
        string input = tokens.NextIs<TokenIdentifier>(false, false)
            ? tokens.Next<TokenIdentifier>("input").word
            : output;

        if (executor.TryGetPPV(input, out PreprocessorVariable values))
            try
            {
                int length = values.Length;

                if (length == 1)
                {
                    executor.SetPPV(output, [values[0]]);
                    return;
                }

                dynamic result = values[0];
                for (int i = 1; i < length; i++)
                    result += values[i];
                result /= length;
                executor.SetPPV(output, [result]);
            }
            catch (RuntimeBinderException)
            {
                throw new StatementException(tokens, "Couldn't add/divide these values.");
            }
        else
            throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
    }
    [UsedImplicitly]
    public static void _sort(Executor executor, Statement tokens)
    {
        string sortDirection = tokens.Next<TokenIdentifier>("sort direction").word.ToUpper();
        string variable = tokens.Next<TokenIdentifier>("output").word;
        variable.ThrowIfWhitespace("output", tokens);

        if (executor.TryGetPPV(variable, out PreprocessorVariable values))
            try
            {
                List<dynamic> listValues = values.ToList();
                listValues.Sort();

                if (sortDirection.StartsWith("DE"))
                    listValues.Reverse();

                executor.SetPPV(variable, listValues.ToArray());
            }
            catch (RuntimeBinderException)
            {
                throw new StatementException(tokens, "Couldn't sort these values.");
            }
        else
            throw new StatementException(tokens, "Preprocessor variable '" + variable + "' does not exist.");
    }
    [UsedImplicitly]
    public static void _reverse(Executor executor, Statement tokens)
    {
        string variable = tokens.Next<TokenIdentifier>("variable name").word;
        variable.ThrowIfWhitespace("variable name", tokens);

        if (executor.TryGetPPV(variable, out PreprocessorVariable values))
        {
            if (values.Length < 2)
                return;

            try
            {
                // reverse the order.
                int end = values.Length - 1;
                int max = values.Length / 2;
                for (int i = 0; i < max; i++)
                {
                    int e = end - i;
                    (values[i], values[e]) = (values[e], values[i]);
                }
            }
            catch (RuntimeBinderException)
            {
                throw new StatementException(tokens, "Couldn't reverse these values.");
            }
        }
        else
        {
            throw new StatementException(tokens, "Preprocessor variable '" + variable + "' does not exist.");
        }
    }
    [UsedImplicitly]
    public static void _unique(Executor executor, Statement tokens)
    {
        string variable = tokens.Next<TokenIdentifier>("variable name").word;
        variable.ThrowIfWhitespace("variable name", tokens);

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
        {
            throw new StatementException(tokens, "Preprocessor variable '" + variable + "' does not exist.");
        }
    }

    [UsedImplicitly]
    public static void _iterate(Executor executor, Statement tokens)
    {
        string current;

        Statement[] statements;
        if (tokens.NextIs<TokenJSONLiteral>(false))
        {
            var json = tokens.Next<TokenJSONLiteral>("json");
            current = tokens.Next<TokenIdentifier>("current token name").word;
            current.ThrowIfWhitespace("current token name", tokens);
            statements = executor.NextExecutionSet(true);

            JToken jsonToken = json.token;

            switch (jsonToken)
            {
                case JArray array:
                    IterateArray(array);
                    break;
                case JObject obj:
                    IterateStrings(obj.Properties().Select(prop => prop.Name));
                    break;
                default:
                    throw new StatementException(tokens, $"Can't iterate over JSON object of type {jsonToken.Type}.");
            }
        }
        else
        {
            string input = tokens.Next<TokenIdentifier>("variable name").word;
            current = tokens.Next<TokenIdentifier>("current token name").word;
            current.ThrowIfWhitespace("current token name", tokens);

            if (!executor.TryGetPPV(input, out PreprocessorVariable values))
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");

            statements = executor.NextExecutionSet(true);

            foreach (dynamic value in values)
            {
                executor.SetPPV(current, value);
                executor.ExecuteSubsection(statements);
            }
        }

        return;

        void IterateArray(IEnumerable<JToken> array)
        {
            foreach (JToken arrayItem in array)
                if (PreprocessorUtils.TryUnwrapToken(arrayItem, out object obj))
                {
                    if (obj == null)
                        throw new StatementException(tokens,
                            $"Couldn't unwrap JSON token to be placed in a preprocessor variable: {arrayItem}");

                    executor.SetPPV(current, obj);
                    executor.ExecuteSubsection(statements);
                }
                else
                {
                    throw new StatementException(tokens,
                        $"JSON Error: Cannot store token of type '{arrayItem.Type}' in a preprocessor variable.");
                }
        }

        void IterateStrings(IEnumerable<string> array)
        {
            foreach (string str in array)
            {
                executor.SetPPV(current, str);
                executor.ExecuteSubsection(statements);
            }
        }
    }
    [UsedImplicitly]
    public static void _len(Executor executor, Statement tokens)
    {
        string output = tokens.Next<TokenIdentifier>("output").word;
        output.ThrowIfWhitespace("output", tokens);

        // JSON Array
        if (tokens.NextIs<TokenJSONLiteral>(false))
        {
            JToken inputJSON = tokens.Next<TokenJSONLiteral>(null);

            if (inputJSON is not JArray array)
                throw new StatementException(tokens, "Cannot get the length of a non-array JSON input.");

            executor.SetPPV(output, array.Count);
            return;
        }

        // String Literal
        if (tokens.NextIs<TokenStringLiteral>(false, false))
        {
            string inputString = tokens.Next<TokenStringLiteral>(null);
            executor.SetPPV(output, inputString.Length);
            return;
        }

        // Preprocessor Variable
        string input = tokens.Next<TokenIdentifier>("ppv").word;
        input.ThrowIfWhitespace("ppv", tokens);

        if (executor.TryGetPPV(input, out PreprocessorVariable values))
        {
            int length = values.Length;
            executor.SetPPV(output, length);
        }
        else
        {
            throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
    }
    [UsedImplicitly]
    public static void _json(Executor executor, Statement tokens)
    {
        JToken json;

        if (tokens.NextIs<TokenJSONLiteral>(false))
        {
            json = tokens.Next<TokenJSONLiteral>("json").token;
        }
        else
        {
            string file = tokens.Next<TokenStringLiteral>("file");
            json = executor.LoadJSONFile(file, tokens);
        }

        string output = tokens.Next<TokenIdentifier>("output").word;
        output.ThrowIfWhitespace("output", tokens);

        if (tokens.NextIs<TokenStringLiteral>(false))
        {
            string accessor = tokens.Next<TokenStringLiteral>("path");
            IEnumerable<string> accessParts = PreprocessorUtils.ParseAccessor(accessor);

            // crawl the tree
            foreach (string _access in accessParts)
            {
                string access = _access.Trim();
                switch (json.Type)
                {
                    case JTokenType.Array:
                    {
                        var array = (JArray) json;
                        if (!int.TryParse(access, out int index))
                            throw new StatementException(tokens,
                                $"JSON Error: Array at '{array.Path}' requires index to access. Given: {access}");
                        if (index < 0)
                            throw new StatementException(tokens, "JSON Error: Index given was less than 0.");
                        if (index >= array.Count)
                            throw new StatementException(tokens,
                                $"JSON Error: Array at '{array.Path}' only contains {array.Count} items. Given: {index + 1}");
                        json = array[index];
                        continue;
                    }
                    case JTokenType.Object:
                    {
                        var obj = (JObject) json;

                        if (!obj.TryGetValue(access, out json))
                            throw new StatementException(tokens,
                                $"JSON Error: Cannot find child '{access}' under token {obj.Path}.");
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
                        throw new StatementException(tokens,
                            $"JSON Error: Unexpected end of JSON tree at {json.Path}.");
                }
            }
        }

        if (!PreprocessorUtils.TryUnwrapToken(json, out object unwrapped))
            throw new StatementException(tokens,
                $"JSON Error: Cannot store token of type '{json.Type}' in a preprocessor variable.");

        executor.SetPPV(output, unwrapped);
    }
    [UsedImplicitly]
    public static void _call(Executor executor, Statement tokens)
    {
        string functionName = tokens.Next<TokenStringLiteral>("function name").text;
        functionName.ThrowIfWhitespace("function name", tokens);

        if (!executor.functions.TryGetFunctions(functionName, out Function[] functions))
            throw new StatementException(tokens, $"Could not find a function by the name '{functionName}'");

        Token[] remainingTokens = tokens.GetRemainingTokens().ToArray();
        int line = tokens.Lines[0];

        // construct a literal function call and then run it
        var finalTokens = new Token[remainingTokens.Length + 3];
        Array.Copy(remainingTokens, 0, finalTokens, 2, remainingTokens.Length);
        finalTokens[0] = new TokenIdentifierFunction(functionName, functions, line);
        finalTokens[1] = new TokenOpenParenthesis(line);
        finalTokens[^1] = new TokenCloseParenthesis(line);

        var callStatement = new StatementFunctionCall(finalTokens);
        callStatement.SetSource(tokens.Lines, tokens.Source, tokens.SourceFile);

        callStatement
            .ClonePrepare(executor)
            .Run0(executor);
    }

    [UsedImplicitly]
    public static void mc(Executor executor, Statement tokens)
    {
        string command = tokens.Next<TokenStringLiteral>("command");
        if (command.StartsWith('/'))
            command = command[1..];
        executor.AddCommand(command);
    }
    [UsedImplicitly]
    public static void globalprint(Executor executor, Statement tokens)
    {
        string str = tokens.Next<TokenStringLiteral>("format string");
        List<JSONRawTerm> terms = executor.FString(str, "print_a", tokens, out bool advanced);

        string[] commands = advanced
            ? Command.Execute().As(Selector.ALL_PLAYERS).AtSelf().RunOver(Executor.ResolveRawText(terms, "tellraw @s "))
            : Executor.ResolveRawText(terms, "tellraw @a ");

        CommandFile file = executor.CurrentFile;
        executor.AddCommands(commands, "print",
            $"Called in a globalprint command located in {file.CommandReference} line {executor.NextLineNumber}");
    }
    [UsedImplicitly]
    public static void print(Executor executor, Statement tokens)
    {
        Selector player = tokens.NextIs<TokenSelectorLiteral>(false, false)
            ? tokens.Next<TokenSelectorLiteral>("player")
            : Selector.SELF;

        if (player.AnyNonPlayers)
            throw new StatementException(tokens, $"The selector {player} may target non-players.");

        string str = tokens.Next<TokenStringLiteral>("format string");
        List<JSONRawTerm> terms = executor.FString(str, "print_" + player.core, tokens, out bool _);
        string[] commands = Executor.ResolveRawText(terms, $"tellraw {player} ");

        CommandFile file = executor.CurrentFile;
        executor.AddCommands(commands, "print",
            $"Called in a print command located in {file.CommandReference} line {executor.NextLineNumber}");
    }
    [UsedImplicitly]
    public static void lang(Executor executor, Statement tokens)
    {
        string locale = tokens.Next<TokenIdentifier>("locale").word;
        locale.ThrowIfWhitespace("locale", tokens);

        if (GlobalContext.Debug)
            Console.WriteLine("Set locale to '{0}'", locale);

        const bool DEFAULT_MERGE = true;

        // create a preprocessor variable if it doesn't exist.
        if (!executor.ppv.TryGetValue(LanguageManager.MERGE_PPV, out _))
            executor.ppv[LanguageManager.MERGE_PPV] = new PreprocessorVariable(DEFAULT_MERGE);

        executor.SetLocale(locale);
    }
    [UsedImplicitly]
    public static void define(Executor executor, Statement tokens)
    {
        string docs = executor.GetDocumentationString(out bool hadDocumentation);

        ScoreboardManager.ValueDefinition def = ScoreboardManager.GetNextValueDefinition(executor, tokens);

        // create the new scoreboard value.
        ScoreboardValue value = def.Create(executor.scoreboard, tokens);
        if (hadDocumentation)
            value.Documentation = docs;
        executor.AddCommandsInit(value.CommandsDefine());

        // register it to the executor.
        executor.scoreboard.TryThrowForDuplicate(value, tokens, out bool identicalDuplicate);

        if (!identicalDuplicate)
            executor.scoreboard.Add(value);

        if (def.defaultValue == null)
            return;

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
                throw new StatementException(tokens,
                    $"Cannot assign value of type {def.defaultValue.GetType().Name} into a variable");
        }

        bool inline = commands.Count == 1;

        if (hadDocumentation && GlobalContext.Decorate)
            commands.AddRange(docs.Trim().Split('\n').Select(str => "# " + str.Trim()));

        // add the commands to the executor.
        CommandFile file = executor.CurrentFile;
        executor.AddCommands(commands, "define" + value.Name,
            $"Called when defining the value '{value.Name}' in {file.CommandReference} line {executor.NextLineNumber}",
            inline);
    }
    [UsedImplicitly]
    public static void init(Executor executor, Statement tokens)
    {
        var commands = new List<string>();

        while (tokens.HasNext)
        {
            if (tokens.NextIs<IUselessInformation>(false))
                continue;

            ScoreboardValue value = tokens.Next<TokenIdentifierValue>("init").value;
            commands.AddRange(value.CommandsInit(value.clarifier.CurrentString));
        }

        executor.AddCommands(commands, null, null, true);
    }
    [UsedImplicitly]
    public static void ifStatement(Executor executor, Statement tokens)
    {
        if (!executor.HasNext)
            throw new StatementException(tokens, "Unexpected end of file after if-statement.");

        // 1.1 rework (post new-execute)
        ComparisonSet set = ComparisonSet.GetComparisons(executor, tokens);
        set.InvertAll(false);
        set.Run(executor, tokens);
    }
    [UsedImplicitly]
    public static void elseStatement(Executor executor, Statement tokens)
    {
        if (!executor.HasNext)
            throw new StatementException(tokens, "Unexpected end of file after else-statement.");

        PreviousComparisonStructure set = executor.GetLastCompare();

        if (set == null)
            throw new StatementException(tokens,
                "No if-statement was found in front of this else-statement at this scope level.");

        bool cancel = set.cancel;
        string prefix = "";

        if (!cancel)
            prefix = Command.Execute()
                .WithSubcommand(new SubcommandUnless(set.conditionalUsed))
                .Run();

        Statement nextStatement = executor.Seek();

        if (nextStatement is StatementOpenBlock openBlock)
        {
            // only do the block stuff if necessary.
            if (openBlock.statementsInside > 0)
            {
                if (cancel)
                {
                    openBlock.openAction = e =>
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
                        openBlock.CloseAction = _ =>
                        {
                            set.Dispose();
                        };
                    }
                    else
                    {
                        CommandFile blockFile = Executor.GetNextGeneratedFile("branch", false);

                        if (GlobalContext.Decorate)
                        {
                            blockFile.Add($"# Run if the previous condition {set.sourceStatement} did not run.");
                            blockFile.AddTrace(executor.CurrentFile);
                        }

                        string command = prefix + Command.Function(blockFile);
                        executor.AddCommand(command);

                        openBlock.openAction = e =>
                        {
                            e.PushFile(blockFile);
                        };
                        openBlock.CloseAction = e =>
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

                executor.DeferAction(_ =>
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
            {
                executor.AppendCommandPrepend(prefix);
            }

            executor.DeferAction(_ =>
            {
                set.Dispose();
            });
        }
    }
    [UsedImplicitly]
    public static void whileLoop(Executor executor, Statement tokens)
    {
        Statement nextStatement = executor.Seek();
        if (nextStatement == null)
            throw new StatementException(tokens, "Unexpected end of file after repeat-statement.");

        Statement[] repeatStatements = nextStatement is StatementOpenBlock _openBlock
            ? executor.Peek(1, _openBlock.statementsInside)
            : [nextStatement];
        bool isAsync = executor.async.IsInAsync && repeatStatements.Any(s => s.DoesAsyncSplit);

        if (isAsync) // temporary throw while this is unsupported.
            throw AsyncManager.UnsupportedException(tokens);

        CommandFile sustainLoop = Executor.GetNextGeneratedFile("whileSustain", false);
        CommandFile loopCode = Executor.GetNextGeneratedFile("while", false);

        // sustainLoop should contain the comparison and then call loopCode if it succeeds
        executor.PushFile(sustainLoop);
        ComparisonSet set = ComparisonSet.GetComparisons(executor, tokens);
        set.RunCommand(Command.Function(loopCode), executor, tokens);
        executor.PopFile();

        Action<Executor> whenCodeIsOver = exec =>
        {
            loopCode.Add(Command.Function(sustainLoop));
            exec.PopFile();
        };

        executor.AddCommand(Command.Function(sustainLoop));

        if (nextStatement is StatementOpenBlock openBlock)
        {
            openBlock.SetLangContext("while");
            openBlock.openAction = exec =>
            {
                exec.PushFile(loopCode);
            };
            openBlock.CloseAction = whenCodeIsOver;
        }
        else
        {
            executor.PushFile(loopCode);
            executor.DeferAction(whenCodeIsOver);
        }
    }
    [UsedImplicitly]
    public static void repeat(Executor executor, Statement tokens)
    {
        Token repetitions = tokens.Next();
        bool isValue = repetitions is TokenIdentifierValue;

        Statement nextStatement = executor.Seek();
        if (nextStatement == null)
            throw new StatementException(tokens, "Unexpected end of file after repeat-statement.");

        Statement[] repeatStatements = nextStatement is StatementOpenBlock _openBlock
            ? executor.Peek(1, _openBlock.statementsInside)
            : [nextStatement];
        bool isAsync = executor.async.IsInAsync && repeatStatements.Any(s => s.DoesAsyncSplit);

        if (isAsync) // temporary throw while this is unsupported.
            throw AsyncManager.UnsupportedException(tokens);

        ScoreboardValue storeIn;
        if (tokens.NextIs<TokenIdentifier>(false))
        {
            string storeInIdentifier = tokens.Next<TokenIdentifier>("store in").word;
            storeInIdentifier.ThrowIfWhitespace("store in", tokens);

            // check if that name is in use already by another scoreboard value
            if (executor.scoreboard.TryGetByUserFacingName(storeInIdentifier, out ScoreboardValue existing))
            {
                // constraints
                if (existing.type.TypeEnum != ScoreboardManager.ValueType.INT)
                    throw new StatementException(tokens,
                        $"Value '{storeInIdentifier}' must be an int in order to be reused in a loop (currently {existing.GetExtendedTypeKeyword()}).");

                storeIn = existing.clarifier.IsGlobal
                    ? existing
                    : // use existing, all constraints match
                    existing.Clone(tokens, newClarifier: Clarifier.Global()); // reinterpret as global
            }
            else
            {
                // create a new scoreboard value for the identifier.
                storeIn = new ScoreboardValue(Typedef.INTEGER, Clarifier.Global(),
                        null,
                        storeInIdentifier,
                        storeInIdentifier,
                        $"Contains the current iteration for the loop: '{tokens.Source}'",
                        executor.scoreboard)
                    .WithAttributes([new AttributeGlobal()], tokens);
                executor.scoreboard.Add(storeIn);
            }
        }
        else
        {
            // store in an anonymous value which will not be shown to the user.
            storeIn = new ScoreboardValue(Typedef.INTEGER, Clarifier.Global(),
                null,
                "_anonymous_repeat_scope" + executor.depth,
                null,
                $"Contains the current iteration for the loop: '{tokens.Source}'",
                executor.scoreboard);
        }

        executor.AddCommandsInit(storeIn.CommandsDefine());

        string repetitionsString;
        var startLoopCommands = new List<string>();

        if (isValue)
        {
            ScoreboardValue value = ((TokenIdentifierValue) repetitions).value;
            startLoopCommands.AddRange(storeIn.Assign(value, tokens));
            startLoopCommands.AddRange(
                storeIn.SubtractLiteral(new TokenIntegerLiteral(1, IntMultiplier.none, tokens.Lines[0]),
                    tokens)); // minus one because it's exclusive (0..i-1)
            repetitionsString = $"{{{value.Name}}}";
        }
        else
        {
            int n = ((TokenNumberLiteral) repetitions).GetNumberInt() - 1; // minus one because it's exclusive (0..i-1)
            if (n < 1)
                throw new StatementException(tokens, "Code inside repeat-statement will never be run.");

            var literal = new TokenIntegerLiteral(n, IntMultiplier.none, tokens.Lines[0]);
            startLoopCommands.AddRange(storeIn.AssignLiteral(literal, tokens));
            repetitionsString = $"{n}";
        }

        CommandFile sustainLoop = Executor.GetNextGeneratedFile("repeatSustain", false);
        CommandFile loopCode = Executor.GetNextGeneratedFile("repeat", false);

        sustainLoop.Add(Command.Execute()
            .IfScore(storeIn, new Range(0, null))
            .Run(Command.Function(loopCode)));
        startLoopCommands.Add(Command.Function(sustainLoop));

        Action<Executor> whenCodeIsOver = exec =>
        {
            loopCode.Add(storeIn.SubtractLiteral(new TokenIntegerLiteral(1, IntMultiplier.none, tokens.Lines[0]),
                tokens));
            loopCode.Add(Command.Function(sustainLoop));
            exec.PopFile();
        };

        CommandFile file = executor.CurrentFile;

        executor.AddExtraFile(sustainLoop);
        executor.AddCommands(startLoopCommands, "beginRepeat",
            $"Begins a loop that repeats {repetitionsString} times. Located in {file.CommandReference} line {executor.NextLineNumber}.");

        if (nextStatement is StatementOpenBlock openBlock)
        {
            openBlock.SetLangContext("repeat");
            openBlock.openAction = exec =>
            {
                exec.PushFile(loopCode);
            };
            openBlock.CloseAction = whenCodeIsOver;
        }
        else
        {
            executor.PushFile(loopCode);
            executor.DeferAction(whenCodeIsOver);
        }
    }
    [UsedImplicitly]
    public static void assert(Executor executor, Statement tokens)
    {
        executor.MarkAssertionOnFileStack();

        ComparisonSet set = ComparisonSet.GetComparisons(executor, tokens);
        set.InvertAll(true);

        CommandFile file = Executor.GetNextGeneratedFile("failAssertion", false);
        IEnumerable<ScoreboardValue> values = set.GetAssertionTargets();

        // construct assertion failed message based on all values in this comparison set
        const string red = "§c";
        file.Add(Command.Tellraw(new RawTextJsonBuilder().AddTerm(new JSONText(
            $"{red}Assertion failed! {set.GetDescription()} ({tokens.Source.Trim()})")).BuildString()));
        foreach (ScoreboardValue value in values)
            file.Add(Command.Tellraw("@s",
                new RawTextJsonBuilder().AddTerms(
                        new JSONText($"{red}    - {value.Name} was "),
                        new JSONScore(value)
                    )
                    .BuildString()));
        file.Add(GenerateHaltCommand(executor));

        executor.AddExtraFile(file);
        set.RunCommand(Command.Function(file), executor, tokens);
    }
    [UsedImplicitly]
    public static void throwError(Executor executor, Statement tokens)
    {
        string text = tokens.Next<TokenStringLiteral>("throw message");
        List<JSONRawTerm> json = executor.FString(text, "throw", tokens, out bool _);
        json.Insert(0, new JSONText("§c"));

        string[] fs = Executor.ResolveRawText(json, "tellraw @s ");
        string[] commands = new string[fs.Length + 2];

        commands[0] = Command.Tellraw("@s",
            new RawTextJsonBuilder().AddTerm(new JSONText($"Error thrown at line {tokens.Lines[0]}:")).BuildString());
        commands[1] = Command.Tellraw("@s",
            new RawTextJsonBuilder().AddTerm(new JSONText($"\t- {tokens.Source.Trim()}")).BuildString());
        for (int i = 0; i < fs.Length; i++)
            commands[i + 2] = fs[i];

        executor.AddCommandsClean(commands, "throwError",
            $"Called in a throw command located in {executor.CurrentFile.CommandReference} line {executor.NextLineNumber}");
        executor.AddCommands(GenerateHaltCommand(executor), "throwErrorHalt",
            "Stops the execution of the code after a throw by expending the command limit.");
    }
    [UsedImplicitly]
    public static void give(Executor executor, Statement tokens)
    {
        Selector player = tokens.Next<TokenSelectorLiteral>("player");

        string itemName = tokens.Next<TokenStringLiteral>("item");
        itemName.ThrowIfWhitespace("item", tokens);
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

        if (tokens.NextIs<TokenIntegerLiteral>(false))
        {
            count = tokens.Next<TokenIntegerLiteral>("count");
            if (count < 1)
                throw new StatementException(tokens, "Item count cannot be less than 1.");

            if (tokens.NextIs<TokenIntegerLiteral>(false))
                data = tokens.Next<TokenIntegerLiteral>("data");
        }

        while (executor.NextBuilderField(ref tokens, out TokenBuilderIdentifier builderIdentifier))
        {
            string builderField = builderIdentifier.BuilderField;

            switch (builderField)
            {
                case "KEEP":
                    keep = true;
                    break;
                case "LOCKINVENTORY":
                    lockInventory = true;
                    break;
                case "LOCKSLOT":
                    lockSlot = true;
                    break;
                case "CANPLACEON":
                    canPlaceOn.Add(tokens.Next<TokenStringLiteral>("can place on block"));
                    break;
                case "CANDESTROY":
                    canDestroy.Add(tokens.Next<TokenStringLiteral>("can destroy block"));
                    break;
                case "ENCHANT":
                    RecognizedEnumValue parsedEnchantment = tokens.Next<TokenIdentifierEnum>("enchantment").value;
                    parsedEnchantment.RequireType<Enchantment>(tokens);
                    var enchantment = (Enchantment) parsedEnchantment.value;
                    int level = tokens.Next<TokenIntegerLiteral>("level");
                    if (level < 1)
                        throw new StatementException(tokens, "Enchantment level cannot be less than 1.");
                    enchants.Add(new Tuple<Enchantment, int>(enchantment, level));
                    needsStructure = true;
                    break;
                case "NAME":
                    displayName = tokens.Next<TokenStringLiteral>("display name");
                    needsStructure = true;
                    break;
                case "LORE":
                    loreLines.Add(tokens.Next<TokenStringLiteral>("lore line"));
                    needsStructure = true;
                    break;
                case "TITLE":
                    if (!itemNameComp.Equals("WRITTEN_BOOK"))
                        throw new StatementException(tokens,
                            "Property 'title' can only be used on item 'written_book'.");
                    if (book == null)
                        book = new ItemTagBookData();
                    ItemTagBookData bookData0 = book.Value;
                    bookData0.title = tokens.Next<TokenStringLiteral>("title");
                    book = bookData0;
                    needsStructure = true;
                    break;
                case "AUTHOR":
                    if (!itemNameComp.Equals("WRITTEN_BOOK"))
                        throw new StatementException(tokens,
                            "Property 'author' can only be used on item 'written_book'.");
                    if (book == null)
                        book = new ItemTagBookData();
                    ItemTagBookData bookData1 = book.Value;
                    bookData1.author = tokens.Next<TokenStringLiteral>("author");
                    book = bookData1;
                    needsStructure = true;
                    break;
                case "PAGE":
                    if (!itemNameComp.Equals("WRITTEN_BOOK"))
                        throw new StatementException(tokens,
                            "Property 'page' can only be used on item 'written_book'.");
                    if (book == null)
                        book = new ItemTagBookData();
                    if (bookPages == null)
                        bookPages = [];
                    bookPages.Add(tokens.Next<TokenStringLiteral>("page contents").text.Replace("\\n", "\n"));
                    needsStructure = true;
                    break;
                case "DYE" when itemNameComp.StartsWith("LEATHER_"):
                    if (!itemNameComp.StartsWith("LEATHER_"))
                        throw new StatementException(tokens, "Property 'dye' can only be used on leather items.");
                    color = new ItemTagCustomColor
                    {
                        r = (byte) tokens.Next<TokenIntegerLiteral>("red"),
                        g = (byte) tokens.Next<TokenIntegerLiteral>("green"),
                        b = (byte) tokens.Next<TokenIntegerLiteral>("blue")
                    };
                    needsStructure = true;
                    break;
                default:
                    throw new StatementException(tokens, $"Invalid property for item: '{builderIdentifier.word}'");
            }
        }

        // create a structure file since this item is too complex
        if (needsStructure)
        {
            if (bookPages != null)
            {
                ItemTagBookData bookData = book.Value;
                bookData.pages = bookPages.ToArray();
                book = bookData;
            }

            var item = new ItemStack
            {
                id = itemName,
                count = count,
                damage = data,
                keep = keep,
                lockMode = lockInventory ? ItemLockMode.LOCK_IN_INVENTORY :
                    lockSlot ? ItemLockMode.LOCK_IN_SLOT : ItemLockMode.NONE,
                displayName = displayName,
                lore = loreLines.ToArray(),
                enchantments = enchants.Select(e => new EnchantmentEntry(e.Item1, e.Item2)).ToArray(),
                canPlaceOn = canPlaceOn.ToArray(),
                canDestroy = canDestroy.ToArray(),
                bookData = book,
                customColor = color
            };

            string fileName = Executor.GetNextGeneratedName(item.DescriptiveFileName, false, true);
            var file = new StructureFile(fileName, Executor.MCC_GENERATED_FOLDER, StructureNBT.SingleItem(item));
            executor.AddExtraFile(file);

            string cmd = Command.StructureLoad(file.CommandReference, Coordinate.here, Coordinate.here, Coordinate.here,
                StructureRotation._0_degrees, StructureMirror.none, true, false);

            cmd = player.NonSelf ? Command.Execute().As(player).AtSelf().Run(cmd) : Command.Execute().AtSelf().Run(cmd);

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
        if (tokens.NextIs<TokenCoordinateLiteral>(false))
        {
            ParseArgs(Selector.SELF);
            return;
        }

        Selector entity = tokens.Next<TokenSelectorLiteral>("entities");

        if (tokens.NextIs<TokenSelectorLiteral>(false))
        {
            Selector selector = tokens.Next<TokenSelectorLiteral>("other").selector;
            if (selector.SelectsMultiple)
                throw new StatementException(tokens, $"Selector '{selector}' may target more than one entity.");
            executor.AddCommand(Command.Teleport(entity.ToString(), selector.ToString(), GetCheckForBlocks()));
            return;
        }

        ParseArgs(entity);
        return;

        void ParseArgs(Selector selector)
        {
            Coordinate x = tokens.Next<TokenCoordinateLiteral>("x");
            Coordinate y = tokens.Next<TokenCoordinateLiteral>("y");
            Coordinate z = tokens.Next<TokenCoordinateLiteral>("z");

            if (tokens.NextIs<TokenIdentifier>(false))
            {
                string id = tokens.Next<TokenIdentifier>(null).word;
                // ReSharper disable once InvertIf
                if (id.ToUpper().Equals("FACING"))
                {
                    if (tokens.NextIs<TokenCoordinateLiteral>(false))
                    {
                        Coordinate fx = tokens.Next<TokenCoordinateLiteral>("facing x");
                        Coordinate fy = tokens.Next<TokenCoordinateLiteral>("facing y");
                        Coordinate fz = tokens.Next<TokenCoordinateLiteral>("facing z");
                        executor.AddCommand(Command.TeleportFacing(selector.ToString(), x, y, z, fx, fy, fz,
                            GetCheckForBlocks()));
                    }
                    else if (tokens.NextIs<TokenSelectorLiteral>(true))
                    {
                        Selector facingEntity = tokens.Next<TokenSelectorLiteral>("facing target").selector;
                        executor.AddCommand(Command.TeleportFacing(selector.ToString(), x, y, z,
                            facingEntity.ToString(), GetCheckForBlocks()));
                    }
                }
            }
            else if (tokens.NextIs<TokenCoordinateLiteral>(false))
            {
                Coordinate ry = tokens.Next<TokenCoordinateLiteral>("rotation y");
                Coordinate rx = tokens.Next<TokenCoordinateLiteral>("rotation x");
                executor.AddCommand(Command.Teleport(selector.ToString(), x, y, z, ry, rx, GetCheckForBlocks()));
            }
            else
            {
                executor.AddCommand(Command.Teleport(selector.ToString(), x, y, z, GetCheckForBlocks()));
            }
        }

        bool GetCheckForBlocks()
        {
            if (tokens.NextIs<TokenBooleanLiteral>(true))
                return tokens.Next<TokenBooleanLiteral>("check for blocks");
            return false;
        }
    }
    [UsedImplicitly]
    public static void move(Executor executor, Statement tokens)
    {
        Selector selector = tokens.Next<TokenSelectorLiteral>("entities");

        string direction = tokens.Next<TokenIdentifier>("direction").word;
        decimal amount = tokens.Next<TokenNumberLiteral>("distance").GetNumber();

        bool checkForBlocks = false;
        if (tokens.NextIs<TokenBooleanLiteral>(true))
            checkForBlocks = tokens.Next<TokenBooleanLiteral>("check for blocks");

        Coordinate x = Coordinate.facingHere;
        Coordinate y = Coordinate.facingHere;
        Coordinate z = Coordinate.facingHere;

        switch (direction.ToUpper())
        {
            case "LEFT":
                x = new Coordinate(amount, true, false, true);
                break;
            case "RIGHT":
                x = new Coordinate(-amount, true, false, true);
                break;
            case "UP":
                y = new Coordinate(amount, true, false, true);
                break;
            case "DOWN":
                y = new Coordinate(-amount, true, false, true);
                break;
            case "FORWARD":
            case "FORWARDS":
                z = new Coordinate(amount, true, false, true);
                break;
            case "BACKWARD":
            case "BACKWARDS":
                z = new Coordinate(-amount, true, false, true);
                break;
        }

        executor.AddCommand(Command.Teleport(selector.ToString(), x, y, z, checkForBlocks));
    }
    [UsedImplicitly]
    public static void face(Executor executor, Statement tokens)
    {
        Selector selector = tokens.Next<TokenSelectorLiteral>("entities");

        if (tokens.NextIs<TokenSelectorLiteral>(false))
        {
            var other = tokens.Next<TokenSelectorLiteral>("other");
            executor.AddCommand(Command.TeleportFacing(selector.ToString(), Coordinate.here, Coordinate.here,
                Coordinate.here, other.ToString()));
        }
        else
        {
            Coordinate x = tokens.Next<TokenCoordinateLiteral>("x");
            Coordinate y = tokens.Next<TokenCoordinateLiteral>("y");
            Coordinate z = tokens.Next<TokenCoordinateLiteral>("z");

            executor.AddCommand(Command.TeleportFacing(selector.ToString(), Coordinate.here, Coordinate.here,
                Coordinate.here, x, y, z));
        }
    }
    [UsedImplicitly]
    public static void rotate(Executor executor, Statement tokens)
    {
        Selector selector = tokens.Next<TokenSelectorLiteral>("entities");

        var number = tokens.Next<TokenNumberLiteral>("amount y");
        Coordinate rx = Coordinate.here;

        Coordinate ry = number is TokenDecimalLiteral
            ? new Coordinate(number.GetNumber(), true, true, false)
            : new Coordinate(number.GetNumberInt(), false, true, false);

        if (tokens.NextIs<TokenNumberLiteral>(true))
        {
            number = tokens.Next<TokenNumberLiteral>("amount x");
            rx = number is TokenDecimalLiteral
                ? new Coordinate(number.GetNumber(), true, true, false)
                : new Coordinate(number.GetNumberInt(), false, true, false);
        }

        executor.AddCommand(Command.Teleport(selector.ToString(), Coordinate.here, Coordinate.here, Coordinate.here, ry,
            rx));
    }
    [UsedImplicitly]
    public static void setblock(Executor executor, Statement tokens)
    {
        Coordinate x = tokens.Next<TokenCoordinateLiteral>("x");
        Coordinate y = tokens.Next<TokenCoordinateLiteral>("y");
        Coordinate z = tokens.Next<TokenCoordinateLiteral>("z");
        string block = tokens.Next<TokenStringLiteral>("block");
        block.ThrowIfWhitespace("block", tokens);
        var handling = OldHandling.replace;

        int data = 0;
        if (tokens.NextIs<TokenIntegerLiteral>(false))
            data = tokens.Next<TokenIntegerLiteral>("data");

        if (tokens.NextIs<TokenIdentifierEnum>(true))
        {
            RecognizedEnumValue enumValue
                = tokens.Next<TokenIdentifierEnum>("old block handling").value;
            enumValue.RequireType<OldHandling>(tokens);
            handling = (OldHandling) enumValue.value;
        }

        executor.AddCommand(Command.SetBlock(x, y, z, block, data, handling));
    }
    [UsedImplicitly]
    public static void fill(Executor executor, Statement tokens)
    {
        Coordinate x1 = tokens.Next<TokenCoordinateLiteral>("x1");
        Coordinate y1 = tokens.Next<TokenCoordinateLiteral>("y1");
        Coordinate z1 = tokens.Next<TokenCoordinateLiteral>("z1");
        Coordinate x2 = tokens.Next<TokenCoordinateLiteral>("x2");
        Coordinate y2 = tokens.Next<TokenCoordinateLiteral>("y2");
        Coordinate z2 = tokens.Next<TokenCoordinateLiteral>("z2");
        if (x1 > x2)
            (x2, x1) = (x1, x2);
        if (y1 > y2)
            (y2, y1) = (y1, y2);
        if (z1 > z2)
            (z2, z1) = (z1, z2);

        string block = tokens.Next<TokenStringLiteral>("block");
        var handling = OldHandling.replace;

        if (tokens.NextIs<TokenIdentifierEnum>(false))
        {
            RecognizedEnumValue enumValue = tokens.Next<TokenIdentifierEnum>("old block handling").value;
            enumValue.RequireType<OldHandling>(tokens);
            handling = (OldHandling) enumValue.value;
        }

        int data = 0;
        if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>(true))
            data = tokens.Next<TokenIntegerLiteral>("data");

        executor.AddCommand(Command.Fill(x1, y1, z1, x2, y2, z2, block, data, handling));
    }
    [UsedImplicitly]
    public static void scatter(Executor executor, Statement tokens)
    {
        string block = tokens.Next<TokenStringLiteral>("block");
        block.ThrowIfWhitespace("block", tokens);
        int percent = tokens.Next<TokenIntegerLiteral>("percentage");
        Coordinate x1 = tokens.Next<TokenCoordinateLiteral>("x1");
        Coordinate y1 = tokens.Next<TokenCoordinateLiteral>("y1");
        Coordinate z1 = tokens.Next<TokenCoordinateLiteral>("z1");
        Coordinate x2 = tokens.Next<TokenCoordinateLiteral>("x2");
        Coordinate y2 = tokens.Next<TokenCoordinateLiteral>("y2");
        Coordinate z2 = tokens.Next<TokenCoordinateLiteral>("z2");

        if (!Coordinate.SizeKnown(x1, y1, z1, x2, y2, z2))
            throw new StatementException(tokens,
                "Scatter command requires all coordinate arguments to be relative or static. (the size needs to be known at compile time.)");

        string seed = null;
        if (tokens.NextIs<TokenStringLiteral>(true))
            seed = tokens.Next<TokenStringLiteral>(null);

        // generate a structure file for this zone.
        long sizeX = Math.Abs(x2.valueInteger - x1.valueInteger) + 1;
        long sizeY = Math.Abs(y2.valueInteger - y1.valueInteger) + 1;
        long sizeZ = Math.Abs(z2.valueInteger - z1.valueInteger) + 1;
        long totalBlocks = sizeX * sizeY * sizeZ;

        if (totalBlocks > 1_000_000)
            Executor.Warn(
                "Warning: Scatter zone is " + totalBlocks +
                " blocks. This could cause extreme performance problems or the command may not even work at all.",
                tokens);

        if (executor.emission.isLinting)
            return; // no need to run allat when it isn't even going to be used...

        int[,,] blocks = new int[sizeX, sizeY, sizeZ];
        for (int x = 0; x < sizeX; x++)
        for (int y = 0; y < sizeY; y++)
        for (int z = 0; z < sizeZ; z++)
            blocks[x, y, z] = 0;

        var structure = new StructureNBT
        {
            formatVersion = 1,
            size = new VectorIntNBT((int) sizeX, (int) sizeY, (int) sizeZ),
            worldOrigin = new VectorIntNBT(0, 0, 0),
            palette = new PaletteNBT(new PaletteEntryNBT(block)),
            entities = new EntityListNBT([]),
            indices = new BlockIndicesNBT(blocks)
        };

        string fileName = Executor.GetNextGeneratedName("scatter_" + Command.Util.StripNamespace(block), false, true);
        var file = new StructureFile(fileName, Executor.MCC_GENERATED_FOLDER, structure);
        executor.emission.WriteSingleFile(file);

        Coordinate minX = Coordinate.Min(x1, x2);
        Coordinate minY = Coordinate.Min(y1, y2);
        Coordinate minZ = Coordinate.Min(z1, z2);

        if (seed == null)
            executor.AddCommand(Command.StructureLoad(file.CommandReference, minX, minY, minZ,
                StructureRotation._0_degrees, StructureMirror.none, false, true, false, percent));
        else
            executor.AddCommand(Command.StructureLoad(file.CommandReference, minX, minY, minZ,
                StructureRotation._0_degrees, StructureMirror.none, false, true, false, percent, seed));
    }
    [UsedImplicitly]
    public static void replace(Executor executor, Statement tokens)
    {
        string src = tokens.Next<TokenStringLiteral>("source block");
        src.ThrowIfWhitespace("source block", tokens);
        int srcData = -1;
        if (tokens.NextIs<TokenIntegerLiteral>(false))
            srcData = tokens.Next<TokenIntegerLiteral>("source data");

        Coordinate x1 = tokens.Next<TokenCoordinateLiteral>("x1");
        Coordinate y1 = tokens.Next<TokenCoordinateLiteral>("y1");
        Coordinate z1 = tokens.Next<TokenCoordinateLiteral>("z1");
        Coordinate x2 = tokens.Next<TokenCoordinateLiteral>("x2");
        Coordinate y2 = tokens.Next<TokenCoordinateLiteral>("y2");
        Coordinate z2 = tokens.Next<TokenCoordinateLiteral>("z2");

        string dst = tokens.Next<TokenStringLiteral>("destination block");
        dst.ThrowIfWhitespace("destination block", tokens);
        int dstData = -1;
        if (tokens.NextIs<TokenIntegerLiteral>(true))
            dstData = tokens.Next<TokenIntegerLiteral>("destination data");

        executor.AddCommand(Command.Fill(x1, y1, z1, x2, y2, z2, src, srcData, dst, dstData));
    }
    [UsedImplicitly]
    public static void kill(Executor executor, Statement tokens)
    {
        Selector selector = Selector.SELF;

        if (tokens.NextIs<TokenSelectorLiteral>(true))
            selector = tokens.Next<TokenSelectorLiteral>("entities");

        executor.AddCommand(Command.Kill(selector.ToString()));
    }
    [UsedImplicitly]
    public static void remove(Executor executor, Statement tokens)
    {
        var file = new CommandFile(true, "silent_remove", Executor.MCC_GENERATED_FOLDER);

        file.Add([
            Command.Teleport(Coordinate.here, new Coordinate(-99999, false, true, false), Coordinate.here),
            Command.Kill(Selector.SELF.ToString())
        ]);

        executor.DefineSTDFile(file);

        Selector selector = Selector.SELF;

        if (tokens.NextIs<TokenSelectorLiteral>(true))
            selector = tokens.Next<TokenSelectorLiteral>("entities");

        executor.PushAlignSelector(ref selector);
        executor.AddCommand(Command.Function(file));
    }
    [UsedImplicitly]
    public static void globaltitle(Executor executor, Statement tokens)
    {
        if (tokens.NextIs<TokenIdentifier>(false, false))
        {
            string word = tokens.Next<TokenIdentifier>(null).word;
            switch (word.ToUpper())
            {
                case "TIMES":
                {
                    int fadeIn = tokens.Next<TokenIntegerLiteral>("fade in");
                    int stay = tokens.Next<TokenIntegerLiteral>("sustain");
                    int fadeOut = tokens.Next<TokenIntegerLiteral>("fade out");
                    executor.AddCommand(Command.TitleTimes("@a", fadeIn, stay, fadeOut));
                    return;
                }
                case "SUBTITLE":
                {
                    string str = tokens.Next<TokenStringLiteral>("subtitle");
                    List<JSONRawTerm> terms = executor.FString(str, "subtitle_a", tokens, out bool advanced);

                    string[] commands = advanced
                        ? Command.Execute().As(Selector.ALL_PLAYERS).AtSelf()
                            .RunOver(Executor.ResolveRawText(terms, "titleraw @s subtitle "))
                        : Executor.ResolveRawText(terms, "titleraw @a subtitle ");

                    CommandFile file = executor.CurrentFile;
                    executor.AddCommands(commands, "subtitle",
                        $"Called in a global-subtitle command located in {file.CommandReference} line {executor.NextLineNumber}");
                    return;
                }
                default:
                    throw new StatementException(tokens,
                        $"Invalid globaltitle subcommand '{word}'. Must be 'times' or 'subtitle'.");
            }
        }

        // ReSharper disable once InvertIf
        if (tokens.NextIs<TokenStringLiteral>(true))
        {
            string str = tokens.Next<TokenStringLiteral>("title");
            List<JSONRawTerm> terms = executor.FString(str, "title_a", tokens, out bool advanced);

            string[] commands = advanced
                ? Command.Execute().As(Selector.ALL_PLAYERS).AtSelf()
                    .RunOver(Executor.ResolveRawText(terms, "title @s title "))
                : Executor.ResolveRawText(terms, "titleraw @a title ");

            CommandFile file = executor.CurrentFile;
            executor.AddCommands(commands, "title",
                $"Called in a globaltitle command located in {file.CommandReference} line {executor.NextLineNumber}");
        }
    }
    [UsedImplicitly]
    public static void title(Executor executor, Statement tokens)
    {
        Selector player = tokens.NextIs<TokenSelectorLiteral>(false, false)
            ? tokens.Next<TokenSelectorLiteral>("players")
            : Selector.SELF;

        if (player.AnyNonPlayers)
            throw new StatementException(tokens, $"The selector {player} may target non-players.");

        if (tokens.NextIs<TokenIdentifier>(false, false))
        {
            string word = tokens.Next<TokenIdentifier>(null).word.ToUpper();
            switch (word)
            {
                case "TIMES":
                {
                    int fadeIn = tokens.Next<TokenIntegerLiteral>("fade in");
                    int stay = tokens.Next<TokenIntegerLiteral>("sustain");
                    int fadeOut = tokens.Next<TokenIntegerLiteral>("fade out");
                    executor.AddCommand(Command.TitleTimes(player.ToString(), fadeIn, stay, fadeOut));
                    return;
                }
                case "SUBTITLE":
                {
                    string str = tokens.Next<TokenStringLiteral>("subtitle");
                    List<JSONRawTerm> terms = executor.FString(str, "subtitle_" + player.core, tokens, out bool _);

                    string[] commands = Executor.ResolveRawText(terms, $"titleraw {player} subtitle ");

                    CommandFile file = executor.CurrentFile;
                    executor.AddCommands(commands, "subtitle",
                        $"Called in a subtitle command located in {file.CommandReference} line {executor.NextLineNumber}");
                    return;
                }
                default:
                    throw new StatementException(tokens,
                        $"Invalid title subcommand '{word}'. Must be 'times' or 'subtitle'.");
            }
        }

        // ReSharper disable once InvertIf
        if (tokens.NextIs<TokenStringLiteral>(true))
        {
            string str = tokens.Next<TokenStringLiteral>("title");
            List<JSONRawTerm> terms = executor.FString(str, "title_" + player.core, tokens, out bool _);
            string[] commands = Executor.ResolveRawText(terms, $"titleraw {player} title ");
            CommandFile file = executor.CurrentFile;
            executor.AddCommands(commands, "title",
                $"Called in a title command located in {file.CommandReference} line {executor.NextLineNumber}");
        }
    }
    [UsedImplicitly]
    public static void globalactionbar(Executor executor, Statement tokens)
    {
        string str = tokens.Next<TokenStringLiteral>("actionbar");
        List<JSONRawTerm> terms = executor.FString(str, "actionbar_a", tokens, out bool advanced);

        string[] commands = advanced
            ? Command.Execute().As(Selector.ALL_PLAYERS).AtSelf()
                .RunOver(Executor.ResolveRawText(terms, "titleraw @s actionbar "))
            : Executor.ResolveRawText(terms, "titleraw @a actionbar ");

        CommandFile file = executor.CurrentFile;
        executor.AddCommands(commands, "actionbar",
            $"Called in a global-actionbar command located in {file.CommandReference} line {executor.NextLineNumber}");
    }
    [UsedImplicitly]
    public static void actionbar(Executor executor, Statement tokens)
    {
        Selector player = tokens.NextIs<TokenSelectorLiteral>(false, false)
            ? tokens.Next<TokenSelectorLiteral>("players")
            : Selector.SELF;

        if (player.AnyNonPlayers)
            throw new StatementException(tokens, $"The selector {player} may target non-players.");

        string str = tokens.Next<TokenStringLiteral>("actionbar");
        List<JSONRawTerm> terms = executor.FString(str, "actionbar_" + player.core, tokens, out bool _);

        string[] commands = Executor.ResolveRawText(terms, $"titleraw {player} actionbar ");

        CommandFile file = executor.CurrentFile;
        executor.AddCommands(commands, "actionbar",
            $"Called in an actionbar command located in {file.CommandReference} line {executor.NextLineNumber}");
    }
    [UsedImplicitly]
    public static void say(Executor executor, Statement tokens)
    {
        string str = tokens.Next<TokenStringLiteral>("text");
        executor.AddCommand(Command.Say(str));
    }
    [UsedImplicitly]
    public static void camera(Executor executor, Statement tokens)
    {
        Selector playersSelector = tokens.Next<TokenSelectorLiteral>("players").selector;
        string players = playersSelector.ToString();
        string rootSubcommand = tokens.Next<TokenIdentifier>("subcommand").word.ToUpper();

        if (playersSelector.AnyNonPlayers)
            throw new StatementException(tokens, $"The selector {players} may target non-players.");

        string finalCommand;

        switch (rootSubcommand)
        {
            case "CLEAR":
            {
                finalCommand = Command.CameraClear(players);
                break;
            }
            case "FADE":
            {
                bool hasTime = false, hasColor = false;
                decimal fadeInSeconds = 0.0M,
                    holdSeconds = 0.0M,
                    fadeOutSeconds = 0.0M;
                int red = 0, green = 0, blue = 0;

                while (tokens.NextIs<TokenIdentifier>(true))
                {
                    string fadeSubcommand = tokens.Next<TokenIdentifier>("fade subcommand").word.ToUpper();

                    if (fadeSubcommand.Equals("TIME"))
                    {
                        if (hasTime)
                            throw new StatementException(tokens,
                                "Camera fade subcommand 'time' may only be used once.");

                        fadeInSeconds = tokens.Next<TokenNumberLiteral>("fade in seconds").GetNumber();
                        holdSeconds = tokens.Next<TokenNumberLiteral>("hold seconds").GetNumber();
                        fadeOutSeconds = tokens.Next<TokenNumberLiteral>("fade out seconds").GetNumber();
                        hasTime = true;
                    }
                    else if (fadeSubcommand.Equals("COLOR"))
                    {
                        if (hasColor)
                            throw new StatementException(tokens,
                                "Camera fade subcommand 'color' may only be used once.");

                        red = tokens.Next<TokenIntegerLiteral>("red").number;
                        green = tokens.Next<TokenIntegerLiteral>("green").number;
                        blue = tokens.Next<TokenIntegerLiteral>("blue").number;
                        hasColor = true;
                    }
                    else
                    {
                        throw new StatementException(tokens,
                            "Invalid camera fade subcommand '" + fadeSubcommand.ToLower() +
                            "'. Must be 'time' or 'color'.");
                    }
                }

                if (hasTime && hasColor)
                    finalCommand = Command.CameraFade(players, fadeInSeconds, holdSeconds, fadeOutSeconds, red, green,
                        blue);
                else if (hasTime)
                    finalCommand = Command.CameraFade(players, fadeInSeconds, holdSeconds, fadeOutSeconds);
                else if (hasColor)
                    finalCommand = Command.CameraFade(players, red, green, blue);
                else
                    finalCommand = Command.CameraFade(players);
                break;
            }
            case "SET":
            {
                string preset = tokens.Next<TokenStringLiteral>("preset").text;
                preset.ThrowIfWhitespace("preset", tokens);
                if (preset.IndexOf(':') == -1)
                {
                    // try to parse a minecraft-defined CameraPreset
                    if (Enum.TryParse(preset, true, out CameraPreset cameraPreset))
                        preset = "minecraft:" + cameraPreset;
                    else
                        throw new StatementException(tokens,
                            "Invalid camera preset '" + preset + "'. Did you forget the namespace?");
                }

                CameraBuilder builder = Command.Camera(players, preset);

                while (tokens.NextIs<TokenIdentifier>(true, false))
                {
                    string subcommand = tokens.Next<TokenIdentifier>("camera set subcommand").word.ToUpper();
                    switch (subcommand)
                    {
                        case "DEFAULT":
                            goto done;
                        case "ENTITY_OFFSET":
                        {
                            decimal x = tokens.Next<TokenCoordinateLiteral>("entity offset x").GetNumber();
                            decimal y = tokens.Next<TokenCoordinateLiteral>("entity offset y").GetNumber();
                            decimal z = tokens.Next<TokenCoordinateLiteral>("entity offset z").GetNumber();
                            builder = builder.WithEntityOffset(x, y, z, tokens);
                            break;
                        }
                        case "VIEW_OFFSET":
                        {
                            decimal x = tokens.Next<TokenCoordinateLiteral>("view offset x").GetNumber();
                            decimal y = tokens.Next<TokenCoordinateLiteral>("view offset y").GetNumber();
                            builder = builder.WithViewOffset(x, y, tokens);
                            break;
                        }
                        case "EASE":
                        {
                            decimal duration = tokens.Next<TokenNumberLiteral>("ease duration").GetNumber();
                            RecognizedEnumValue _easeType = tokens.Next<TokenIdentifierEnum>("ease type").value;

                            _easeType.RequireType<Easing>(tokens);

                            var easeType = (Easing) _easeType.value;
                            builder = builder.WithEasing(easeType, duration, tokens);
                            break;
                        }
                        case "FACING":
                        {
                            if (tokens.NextIs<TokenSelectorLiteral>(false, false))
                            {
                                Selector faceEntity = tokens.Next<TokenSelectorLiteral>(null);
                                if (faceEntity.SelectsMultiple)
                                    throw new StatementException(tokens,
                                        "Camera set subcommand 'facing' can't target multiple entities. Perhaps use [c=1]?");
                                builder = builder.WithFacing(faceEntity.ToString(), tokens);
                            }
                            else
                            {
                                Coordinate x = tokens.Next<TokenCoordinateLiteral>("facing x or entity");
                                Coordinate y = tokens.Next<TokenCoordinateLiteral>("facing y");
                                Coordinate z = tokens.Next<TokenCoordinateLiteral>("facing z");
                                builder = builder.WithFacing(x, y, z, tokens);
                            }

                            break;
                        }
                        case "POSITIONED":
                        case "POSITION":
                        case "POS":
                        {
                            Coordinate x = tokens.Next<TokenCoordinateLiteral>("position x");
                            Coordinate y = tokens.Next<TokenCoordinateLiteral>("position y");
                            Coordinate z = tokens.Next<TokenCoordinateLiteral>("position z");
                            builder = builder.WithPosition(x, y, z, tokens);
                            break;
                        }
                        case "ROTATED":
                        case "ROTATION":
                        case "ROT":
                        {
                            decimal x = tokens.Next<TokenCoordinateLiteral>("rotation x").GetNumber();
                            decimal y = tokens.Next<TokenCoordinateLiteral>("rotation y").GetNumber();
                            builder = builder.WithRotation(x, y, tokens);
                            break;
                        }
                        default:
                            throw new StatementException(tokens,
                                $"Invalid camera set subcommand '{subcommand.ToLower()}'.");
                    }
                }

                done:

                finalCommand = builder.Build();
                break;
            }
            default:
                throw new StatementException(tokens,
                    $"Invalid camera subcommand '{rootSubcommand.ToLower()}'. Must be 'clear', 'fade', or 'set'.");
        }

        executor.AddCommand(finalCommand);
    }

    [UsedImplicitly]
    public static void halt(Executor executor, Statement tokens)
    {
        IEnumerable<string> commands = GenerateHaltCommand(executor);
        executor.AddCommands(commands, "startHaltExecution",
            "Prepares to halt the code by expending the command limit.");
        executor.UnreachableCode();
    }
    private static IEnumerable<string> GenerateHaltCommand(Executor executor)
    {
        var file = new CommandFile(true, "haltExecution", Executor.MCC_GENERATED_FOLDER);
        var commands = new List<string>
        {
            Command.Function(file)
        };
        if (executor.async.IsInAsync)
        {
            // we also need to stop the async state
            IEnumerable<string> haltCommands = executor.async.CurrentFunction.CommandsHalt();
            commands.InsertRange(0, haltCommands);
        }

        if (executor.HasSTDFile(file))
            return commands;

        // recursively call self until function command limit reached
        file.Add(Command.Function(file));
        executor.DefineSTDFile(file);

        return commands;
    }

    [UsedImplicitly]
    public static void summon(Executor executor, Statement tokens)
    {
        string entityType = tokens.Next<TokenIdentifier>("entity type").word;
        entityType.ThrowIfWhitespace("entity type", tokens);
        Coordinate x = Coordinate.here;
        Coordinate y = Coordinate.here;
        Coordinate z = Coordinate.here;

        // summon <entityType> <nameTag> [spawnPosition: x y z]
        if (tokens.NextIs<TokenStringLiteral>(false, false))
        {
            string nameTagShortSyntax = tokens.Next<TokenStringLiteral>(null);

            if (tokens.NextIs<TokenCoordinateLiteral>(true))
                GetSpawnPosition();

            executor.AddCommand(Command.Summon(entityType, nameTagShortSyntax, x, y, z));
            return;
        }

        // summon <entityType>
        if (!tokens.NextIs<TokenCoordinateLiteral>(false))
        {
            executor.AddCommand(Command.Summon(entityType));
            return;
        }

        // summon <entityType> <spawnPosition: x y z> ...
        GetSpawnPosition();

        // summon <entityType> <spawnPosition: x y z> <rotation: y x>
        if (tokens.NextIs<TokenCoordinateLiteral>(false))
        {
            Coordinate rotationY = tokens.Next<TokenCoordinateLiteral>("rotation y");
            Coordinate rotationX = tokens.Next<TokenCoordinateLiteral>("rotation x");

            // summon <entityType> <spawnPosition: x y z> <rotation: y x> <spawnEvent>
            if (tokens.NextIs<TokenStringLiteral>(true, false))
            {
                string spawnEvent = tokens.Next<TokenStringLiteral>("spawn event");
                spawnEvent.ThrowIfWhitespace("spawn event", tokens);

                // summon <entityType> <spawnPosition: x y z> <rotation: y x> <spawnEvent> <nameTag>
                if (tokens.NextIs<TokenStringLiteral>(true, false))
                {
                    string nameTag = tokens.Next<TokenStringLiteral>("name tag");
                    executor.AddCommand(Command.Summon(entityType, x, y, z, rotationY, rotationX, nameTag, spawnEvent));
                    return;
                }

                executor.AddCommand(Command.SummonWithEvent(entityType, x, y, z, rotationY, rotationX, spawnEvent));
                return;
            }

            executor.AddCommand(Command.Summon(entityType, x, y, z, rotationY, rotationX));
            return;
        }

        // summon <entityType> <spawnPosition: x y z> facing ...
        if (tokens.NextIs<TokenIdentifier>(true, false))
        {
            string word = tokens.Next<TokenIdentifier>(null).word;
            if (!word.Equals("facing", StringComparison.OrdinalIgnoreCase))
            {
                if (word.StartsWith("f", StringComparison.OrdinalIgnoreCase))
                    throw new StatementException(tokens,
                        $"Unknown summon subcommand: '{word}' (did you mean 'facing'?)");
                throw new StatementException(tokens, $"Unknown summon subcommand: '{word}'");
            }

            // summon <entityType> <spawnPosition: x y z> facing <entity>
            if (tokens.NextIs<TokenSelectorLiteral>(false, false))
            {
                Selector faceEntity = tokens.Next<TokenSelectorLiteral>("look at entity");
                string faceSelector = faceEntity.ToString();

                // summon <entityType> <spawnPosition: x y z> facing <entity> <spawnEvent>
                if (tokens.NextIs<TokenStringLiteral>(true, false))
                {
                    string spawnEvent = tokens.Next<TokenStringLiteral>("spawn event");
                    spawnEvent.ThrowIfWhitespace("spawn event", tokens);

                    // summon <entityType> <spawnPosition: x y z> facing <entity> <spawnEvent> <nameTag>
                    if (tokens.NextIs<TokenStringLiteral>(true, false))
                    {
                        string nameTag = tokens.Next<TokenStringLiteral>("name tag");
                        executor.AddCommand(
                            Command.SummonFacing(entityType, x, y, z, faceSelector, nameTag, spawnEvent));
                        return;
                    }

                    executor.AddCommand(Command.SummonFacingWithEvent(entityType, x, y, z, faceSelector, spawnEvent));
                    return;
                }

                executor.AddCommand(Command.SummonFacing(entityType, x, y, z, faceSelector));
                return;
            }

            // summon <entityType> <spawnPosition: x y z> facing <face: x y z>
            // ReSharper disable once InvertIf
            if (tokens.NextIs<TokenCoordinateLiteral>(false, false))
            {
                Coordinate faceX = tokens.Next<TokenCoordinateLiteral>("face x");
                Coordinate faceY = tokens.Next<TokenCoordinateLiteral>("face y");
                Coordinate faceZ = tokens.Next<TokenCoordinateLiteral>("face z");

                // summon <entityType> <spawnPosition: x y z> facing <face: x y z> <spawnEvent>
                if (tokens.NextIs<TokenStringLiteral>(true, false))
                {
                    string spawnEvent = tokens.Next<TokenStringLiteral>("spawn event");
                    spawnEvent.ThrowIfWhitespace("spawn event", tokens);

                    // summon <entityType> <spawnPosition: x y z> facing <face: x y z> <spawnEvent> <nameTag>
                    if (tokens.NextIs<TokenStringLiteral>(true, false))
                    {
                        string nameTag = tokens.Next<TokenStringLiteral>("name tag");
                        executor.AddCommand(Command.SummonFacing(entityType, x, y, z, faceX, faceY, faceZ, nameTag,
                            spawnEvent));
                        return;
                    }

                    executor.AddCommand(Command.SummonFacingWithEvent(entityType, x, y, z, faceX, faceY, faceZ,
                        spawnEvent));
                    return;
                }

                executor.AddCommand(Command.SummonFacing(entityType, x, y, z, faceX, faceY, faceZ));
                return;
            }

            throw new StatementException(tokens, "Where the entity should face was not specified.");
        }

        executor.AddCommand(Command.Summon(entityType, x, y, z));
        return;

        void GetSpawnPosition()
        {
            x = tokens.Next<TokenCoordinateLiteral>("spawn position x");
            y = tokens.Next<TokenCoordinateLiteral>("spawn position y");
            z = tokens.Next<TokenCoordinateLiteral>("spawn position z");
        }
    }
    [UsedImplicitly]
    public static void damage(Executor executor, Statement tokens)
    {
        Selector target = tokens.Next<TokenSelectorLiteral>("targets");

        int damage = tokens.Next<TokenIntegerLiteral>("amount");
        if (damage < 0)
            throw new StatementException(tokens, "Damage amount cannot be less than 0.");

        var cause = DamageCause.all;
        Selector blame = null;

        if (tokens.NextIs<TokenIdentifierEnum>(false))
        {
            var idEnum = tokens.Next<TokenIdentifierEnum>("cause");
            idEnum.value.RequireType<DamageCause>(tokens);
            cause = (DamageCause) idEnum.value.value;
        }

        if (tokens.NextIs<TokenSelectorLiteral>(false))
        {
            var value = tokens.Next<TokenSelectorLiteral>("blame");
            blame = value.selector;
        }
        else if (tokens.NextIs<TokenCoordinateLiteral>(true))
        {
            // spawn dummy entity
            Coordinate x = tokens.Next<TokenCoordinateLiteral>("blame x");
            Coordinate y = tokens.Next<TokenCoordinateLiteral>("blame y");
            Coordinate z = tokens.Next<TokenCoordinateLiteral>("blame z");

            executor.RequireFeature(tokens, Feature.DUMMIES);
            const string damagerEntity = "_dmg_from";
            string[] commands =
            [
                // create dummy entity at location
                executor.entities.dummies.Create(damagerEntity, false, x, y, z),

                // hit entity from dummy entity
                Command.Damage(target.ToString(), damage, cause,
                    executor.entities.dummies.GetStringSelector(damagerEntity, false)),

                // send kill event to dummy entity
                executor.entities.dummies.Destroy(damagerEntity, false)
            ];

            CommandFile file = executor.CurrentFile;
            executor.AddCommands(commands, "damageFrom",
                $"Creates a dummy entity and uses it to attack the entity from the location ({x} {y} {z}). {file.CommandReference} line {executor.NextLineNumber}");
            return;
        }

        string command;
        if (blame == null)
        {
            command = Command.Damage(target.ToString(), damage, cause);
        }
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
        string word = tokens.Next<TokenIdentifier>("subcommand").word.ToUpper();

        string name;
        string tag = null;

        switch (word)
        {
            case "CREATE":
            {
                name = tokens.Next<TokenStringLiteral>("name");
                if (tokens.NextIs<TokenStringLiteral>(false))
                {
                    tag = tokens.Next<TokenStringLiteral>("tag");
                    tag.ThrowIfWhitespace("tag", tokens);
                }

                Coordinate x = Coordinate.here;
                Coordinate y = Coordinate.here;
                Coordinate z = Coordinate.here;

                if (tokens.NextIs<TokenCoordinateLiteral>(true))
                    x = tokens.Next<TokenCoordinateLiteral>("x");
                if (tokens.NextIs<TokenCoordinateLiteral>(true))
                    y = tokens.Next<TokenCoordinateLiteral>("y");
                if (tokens.NextIs<TokenCoordinateLiteral>(true))
                    z = tokens.Next<TokenCoordinateLiteral>("z");

                if (tag == null)
                {
                    string command = executor.entities.dummies.Create(name, false, x, y, z);
                    executor.AddCommand(command);
                }
                else
                {
                    string selector = executor.entities.dummies.GetStringSelector(name, true);
                    string[] commands =
                    [
                        executor.entities.dummies.Create(name, true, x, y, z),
                        Command.Tag(selector, tag),
                        Command.Event(selector, DummyManager.TAGGABLE_EVENT_REMOVE_NAME)
                    ];

                    CommandFile file = executor.CurrentFile;
                    executor.AddCommands(commands, "createDummy",
                        $"Spawns a dummy entity named '{name}' with the tag {tag} at ({x} {y} {z}). {file.CommandReference} line {executor.NextLineNumber}");
                }

                break;
            }
            case "SINGLE":
            {
                name = tokens.Next<TokenStringLiteral>("name");
                if (tokens.NextIs<TokenStringLiteral>(false))
                {
                    tag = tokens.Next<TokenStringLiteral>("tag");
                    tag.ThrowIfWhitespace("tag", tokens);
                }

                Coordinate x = Coordinate.here;
                Coordinate y = Coordinate.here;
                Coordinate z = Coordinate.here;

                if (tokens.NextIs<TokenCoordinateLiteral>(true))
                    x = tokens.Next<TokenCoordinateLiteral>("x");
                if (tokens.NextIs<TokenCoordinateLiteral>(true))
                    y = tokens.Next<TokenCoordinateLiteral>("y");
                if (tokens.NextIs<TokenCoordinateLiteral>(true))
                    z = tokens.Next<TokenCoordinateLiteral>("z");

                CommandFile file = executor.CurrentFile;

                if (tag == null)
                {
                    executor.AddCommands([
                            executor.entities.dummies.Destroy(name, false),
                            executor.entities.dummies.Create(name, false, x, y, z)
                        ], "singletonDummy",
                        $"Spawns a singleton dummy entity named '{name}' at ({x} {y} {z}). {file.CommandReference} line {executor.NextLineNumber}");
                }
                else
                {
                    string selector = executor.entities.dummies.GetStringSelector(name, true);
                    executor.AddCommands([
                            executor.entities.dummies.Destroy(name, false, tag),
                            executor.entities.dummies.Create(name, true, x, y, z),
                            Command.Tag(selector, tag),
                            Command.Event(selector, DummyManager.TAGGABLE_EVENT_REMOVE_NAME)
                        ], "singletonDummy",
                        $"Spawns a singleton dummy entity named '{name}' with the tag {tag} at ({x} {y} {z}). {file.CommandReference} line {executor.NextLineNumber}");
                }

                break;
            }
            case "REMOVEALL":
                if (tokens.NextIs<TokenStringLiteral>(true, false))
                {
                    tag = tokens.Next<TokenStringLiteral>("tag");
                    tag.ThrowIfWhitespace("tag", tokens);
                }

                executor.AddCommand(executor.entities.dummies.DestroyAll(tag));
                break;
            case "REMOVE":
                name = tokens.Next<TokenStringLiteral>("name");
                if (tokens.NextIs<TokenStringLiteral>(true, false))
                {
                    tag = tokens.Next<TokenStringLiteral>("tag");
                    tag.ThrowIfWhitespace("tag", tokens);
                }

                executor.AddCommand(executor.entities.dummies.Destroy(name, false, tag));
                break;
            default:
                throw new StatementException(tokens,
                    $"Invalid mode for dummy command: {word}. Valid options are CREATE, SINGLE, REMOVEALL or REMOVE");
        }
    }
    [UsedImplicitly]
    public static void tag(Executor executor, Statement tokens)
    {
        string selected = tokens.Next<TokenSelectorLiteral>("enitites").selector.ToString();
        string word = tokens.Next<TokenIdentifier>("subcommand").word.ToUpper();

        switch (word)
        {
            case "ADD":
            {
                string tag = tokens.Next<TokenStringLiteral>("tag");
                tag.ThrowIfWhitespace("tag", tokens);

                executor.definedTags.Add(tag);
                executor.AddCommand(Command.Tag(selected, tag));
                break;
            }
            case "REMOVE":
            {
                string tag = tokens.Next<TokenStringLiteral>("tag");
                tag.ThrowIfWhitespace("tag", tokens);

                executor.AddCommand(Command.TagRemove(selected, tag));
                break;
            }
            default:
                throw new StatementException(tokens,
                    $"Invalid mode for tag command: '{word.ToLower()}'. Valid options are 'add' and 'remove'.");
        }
    }
    [UsedImplicitly]
    public static void explode(Executor executor, Statement tokens)
    {
        executor.RequireFeature(tokens, Feature.EXPLODERS);

        Coordinate x, y, z;

        if (tokens.NextIs<TokenCoordinateLiteral>(false))
        {
            x = tokens.Next<TokenCoordinateLiteral>("x");
            y = tokens.Next<TokenCoordinateLiteral>("y");
            z = tokens.Next<TokenCoordinateLiteral>("z");
        }
        else
        {
            x = Coordinate.here;
            y = Coordinate.here;
            z = Coordinate.here;
        }

        int power, delay;
        bool fire, breaks;

        // Get the first two integers
        if (tokens.NextIs<TokenIntegerLiteral>(true))
        {
            power = tokens.Next<TokenIntegerLiteral>("power");
            if (power < 0)
                throw new StatementException(tokens, "Explosion power cannot be less than 0.");

            delay = tokens.NextIs<TokenIntegerLiteral>(true) ? tokens.Next<TokenIntegerLiteral>("delay") : 0;
            if (delay < 0)
                throw new StatementException(tokens, "Explosion delay cannot be less than 0.");
        }
        else
        {
            power = 3;
            delay = 0;
        }

        // The two booleans
        if (tokens.NextIs<TokenBooleanLiteral>(true))
        {
            fire = tokens.Next<TokenBooleanLiteral>("does fire");

            breaks = tokens.NextIs<TokenBooleanLiteral>(true)
                ? tokens.Next<TokenBooleanLiteral>("breaks blocks")
                : true;
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
        string command;
        string selector = tokens.NextIs<TokenSelectorLiteral>(false)
            ? tokens.Next<TokenSelectorLiteral>("entities").selector.ToString()
            : null;

        if (selector != null)
        {
            if (tokens.NextIs<TokenStringLiteral>(true))
            {
                string item = tokens.Next<TokenStringLiteral>("item");
                item.ThrowIfWhitespace("item", tokens);

                if (tokens.NextIs<TokenIntegerLiteral>(true))
                {
                    int data = tokens.Next<TokenIntegerLiteral>("data");
                    if (tokens.NextIs<TokenIntegerLiteral>(true))
                    {
                        int maxCount = tokens.Next<TokenIntegerLiteral>("max count");
                        if (maxCount < 0)
                            throw new StatementException(tokens, "Max count cannot be less than 0.");
                        command = Command.Clear(selector, item, data, maxCount);
                    }
                    else
                    {
                        command = Command.Clear(selector, item, data);
                    }
                }
                else
                {
                    command = Command.Clear(selector, item);
                }
            }
            else
            {
                command = Command.Clear(selector);
            }
        }
        else
        {
            command = Command.Clear();
        }

        executor.AddCommand(command);
    }
    [UsedImplicitly]
    public static void effect(Executor executor, Statement tokens)
    {
        string selector = tokens.Next<TokenSelectorLiteral>("entities").selector.ToString();

        if (!tokens.NextIs<TokenIdentifierEnum>(false))
        {
            string word = tokens.Next<TokenIdentifier>("subcommand").word.ToUpper();

            if (word.Equals("CLEAR"))
            {
                executor.AddCommand(Command.EffectClear(selector));
                return;
            }

            if (word.StartsWith("C"))
                throw new StatementException(tokens, "Invalid option for effect command. (did you mean 'clear'?)");
            throw new StatementException(tokens, "Invalid option for effect command.");
        }

        string command;
        var effectToken = tokens.Next<TokenIdentifierEnum>("effect");
        RecognizedEnumValue parsedEffect = effectToken.value;
        parsedEffect.RequireType<PotionEffect>(tokens);
        var effect = (PotionEffect) parsedEffect.value;

        if (tokens.NextIs<TokenIntegerLiteral>(true))
        {
            int seconds = tokens.Next<TokenIntegerLiteral>("duration").Scaled(IntMultiplier.s);
            if (seconds < 0)
                throw new StatementException(tokens, "Effect time cannot be less than 0.");

            if (tokens.NextIs<TokenIntegerLiteral>(true))
            {
                int amplifier = tokens.Next<TokenIntegerLiteral>("amplifier");
                if (amplifier < 0)
                    throw new StatementException(tokens, "Effect amplifier cannot be less than 0.");

                if (tokens.NextIs<TokenBooleanLiteral>(true))
                {
                    bool hideParticles = tokens.Next<TokenBooleanLiteral>("hide particles");
                    command = Command.Effect(selector, effect, seconds, amplifier, hideParticles);
                }
                else
                {
                    command = Command.Effect(selector, effect, seconds, amplifier);
                }
            }
            else
            {
                command = Command.Effect(selector, effect, seconds);
            }
        }
        else
        {
            command = Command.Effect(selector, effect);
        }

        executor.AddCommand(command);
    }
    [UsedImplicitly]
    public static void playsound(Executor executor, Statement tokens)
    {
        string soundId = tokens.Next<TokenStringLiteral>("sound");
        soundId.ThrowIfWhitespace("sound", tokens);

        Selector filter = tokens.NextIs<TokenSelectorLiteral>(true)
            ? tokens.Next<TokenSelectorLiteral>("filter")
            : Selector.SELF;

        bool wasSoundFile;

        if (filter.AnyNonPlayers)
            throw new StatementException(tokens, $"The selector {filter} may target non-players.");

        if (!tokens.NextIs<TokenCoordinateLiteral>(false))
        {
            soundId = ProcessSoundIdAsFile(SoundCategory.ui, out wasSoundFile);

            executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), Coordinate.here, Coordinate.here,
                Coordinate.here,
                wasSoundFile ? 10_000f : 1.0f,
                1.0f,
                wasSoundFile ? 10_000f : 1.0f));
            return;
        }

        Coordinate x = tokens.Next<TokenCoordinateLiteral>("x");
        Coordinate y = tokens.Next<TokenCoordinateLiteral>("y");
        Coordinate z = tokens.Next<TokenCoordinateLiteral>("z");
        soundId = ProcessSoundIdAsFile(SoundCategory.ui, out wasSoundFile);

        if (!tokens.NextIs<TokenNumberLiteral>(true))
        {
            if (wasSoundFile)
                executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z, 10_000f, 1.0f, 10_000f));
            executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z));
            return;
        }

        decimal volume = tokens.Next<TokenNumberLiteral>("volume").GetNumber();

        if (!tokens.NextIs<TokenNumberLiteral>(true))
        {
            if (wasSoundFile)
                executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z, (float) volume, 1.0f,
                    10_000f));
            executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z, (float) volume));
            return;
        }

        decimal pitch = tokens.Next<TokenNumberLiteral>("pitch").GetNumber();

        if (!tokens.NextIs<TokenNumberLiteral>(true))
        {
            if (wasSoundFile)
                executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z, (float) volume,
                    (float) pitch, 10_000f));
            executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z, (float) volume, (float) pitch));
            return;
        }

        decimal minVolume = tokens.Next<TokenNumberLiteral>("minimum volume").GetNumber();
        executor.AddCommand(Command.PlaySound(soundId, filter.ToString(), x, y, z, (float) volume, (float) pitch,
            (float) minVolume));

        return;

        string ProcessSoundIdAsFile(SoundCategory category, out bool valid)
        {
            string extension = Path.GetExtension(soundId);

            if (string.IsNullOrEmpty(extension))
            {
                valid = false;
                return soundId;
            }

            switch (extension.ToUpper())
            {
                case ".WAV":
                case ".OGG":
                case ".FSB":
                    break;
                default:
                    valid = false;
                    return soundId;
            }

            // this is an audio file.
            executor.RequireFeature(tokens, Feature.AUDIOFILES);

            if (!File.Exists(soundId))
                throw new StatementException(tokens, $"Audio file '{soundId}' not found.");

            SoundDefinition definition = executor.AddNewSoundDefinition(soundId, category, tokens);
            valid = true;
            return definition.CommandReference;
        }
    }
    [UsedImplicitly]
    public static void particle(Executor executor, Statement tokens)
    {
        string particleId = tokens.Next<TokenStringLiteral>("particle");
        particleId.ThrowIfWhitespace("particle", tokens);

        if (tokens.NextIs<TokenCoordinateLiteral>(true))
        {
            Coordinate x = tokens.Next<TokenCoordinateLiteral>("x");
            Coordinate y = tokens.Next<TokenCoordinateLiteral>("y");
            Coordinate z = tokens.Next<TokenCoordinateLiteral>("z");
            executor.AddCommand(Command.Particle(particleId, x, y, z));
            return;
        }

        executor.AddCommand(Command.Particle(particleId,
            Coordinate.here, Coordinate.here, Coordinate.here));
    }

    [UsedImplicitly]
    public static void gamemode(Executor executor, Statement tokens)
    {
        RecognizedEnumValue gamemode = tokens.Next<TokenIdentifierEnum>("gamemode").value;
        gamemode.RequireType<GameMode>(tokens);
        var mode = (GameMode) gamemode.value;
        string target;

        if (tokens.NextIs<TokenSelectorLiteral>(true))
        {
            Selector selector = tokens.Next<TokenSelectorLiteral>("players").selector;
            if (selector.AnyNonPlayers)
                throw new StatementException(tokens, $"The selector {selector} may target non-players.");
            target = selector.ToString();
        }
        else
        {
            target = Selector.SELF.ToString();
        }

        executor.AddCommand(Command.Gamemode(target, mode));
    }

    [UsedImplicitly]
    public static void execute(Executor executor, Statement tokens)
    {
        var builder = new ExecuteBuilder();

        do
        {
            string _subcommand = tokens.Next<TokenIdentifier>("subcommand").word.ToUpper();
            var subcommand = Subcommand.GetSubcommandForKeyword(_subcommand, tokens);

            if (subcommand.TerminatesChain)
                throw new StatementException(tokens,
                    $"Subcommand '{_subcommand}' is not allowed here as it terminates the chain.");

            // parse from tokens
            subcommand.FromTokens(tokens);

            // add to builder
            builder.WithSubcommand(subcommand);
        } while (tokens.NextIs<TokenIdentifier>(false, false));

        // --- find statement or code-block nabbed from Comparison::Run ---

        if (!executor.HasNext)
            throw new StatementException(tokens, "Unexpected end of file when running comparison.");

        Statement next = executor.Seek();

        if (next is StatementOpenBlock openBlock)
        {
            openBlock.ignoreAsync = true;

            // only do the block stuff if necessary.
            if (openBlock.meaningfulStatementsInside == 0)
            {
                openBlock.openAction = null;
                openBlock.CloseAction = null;
                return; // do nothing
            }

            // tiny contextual information without being too long
            var langContext = new StringBuilder("execute");
            if (builder.TryGetFirst(out SubcommandAs _as))
                langContext.Append("_as_" + _as.entity.core);
            if (builder.TryGetFirst(out SubcommandIf _))
                langContext.Append("_if");
            if (builder.TryGetFirst(out SubcommandUnless _))
                langContext.Append("_unless");
            openBlock.SetLangContext(langContext.ToString());

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
                CommandFile blockFile = Executor.GetNextGeneratedFile("execute", false);

                if (GlobalContext.Decorate)
                {
                    CommandFile file = executor.CurrentFile;
                    string subcommandsString = builder.BuildClean(out _);
                    blockFile.Add($"# Run under the following execute subcommands: [{subcommandsString}]");
                    blockFile.AddTrace(file);
                }

                string command = finalExecute + Command.Function(blockFile);
                executor.AddCommand(command);

                openBlock.openAction = e =>
                {
                    e.PushFile(blockFile);
                };
                openBlock.CloseAction = e =>
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
        string featureStr = tokens.Next<TokenIdentifier>("feature").word.ToUpper();
        Feature feature = FeatureManager.FEATURE_LIST
            .FirstOrDefault(possibleFeature =>
                featureStr.Equals(possibleFeature.ToString(), StringComparison.OrdinalIgnoreCase));

        if (feature == Feature.NO_FEATURES)
            throw new StatementException(tokens, "No valid feature specified.");

        executor.emission.EnableFeature(feature);
        FeatureManager.OnFeatureEnabled(executor, feature);

        if (GlobalContext.Debug && !executor.emission.isLinting)
            Console.WriteLine("Feature enabled: {0}", feature);
    }
    [UsedImplicitly]
    public static void function(Executor executor, Statement tokens)
    {
        // temporary fix for a bug
        if (executor.async.IsInAsync)
            throw new StatementException(tokens,
                "Defining functions is unsupported inside an async context. This is a bug that's extremely difficult to fix.");

        // pull attributes
        var attributes = new List<IAttribute>();
        var asyncTarget = AsyncTarget.Local;
        bool isAsync = false;

        FindAttributes();

        // normal definition
        string functionName = tokens.Next<TokenIdentifier>("function name").word;
        functionName.ThrowIfWhitespace("function name", tokens);

        bool usesFolders = functionName.Contains('.');
        string[] folders = null;

        if (usesFolders)
        {
            string[] split = functionName.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 1)
            {
                folders = split.Take(split.Length - 1).ToArray();
                if (folders[0].Equals(Executor.MCC_GENERATED_FOLDER, StringComparison.OrdinalIgnoreCase))
                    throw new StatementException(tokens,
                        $"The folder '{Executor.MCC_GENERATED_FOLDER}' is reserved for MCCompiled code.");
            }
            else
            {
                throw new StatementException(tokens,
                    "Found leading/trailing period in function name. Did you mean to specify a folder?");
            }
        }

        var parameters = new List<RuntimeFunctionParameter>();

        FindAttributes();

        if (tokens.NextIs<TokenOpenParenthesis>(false))
            tokens.Next();

        bool hasBegunOptionals = false;

        // this is where the directive takes in function parameters
        while (tokens.NextIs<TokenIdentifier>(false))
        {
            // fetch a parameter definition
            ScoreboardManager.ValueDefinition def = ScoreboardManager.GetNextValueDefinition(executor, tokens);

            // don't let users define non-optional parameters if they already specified one.
            if (def.defaultValue == null && hasBegunOptionals)
                throw new StatementException(tokens,
                    "All parameters proceeding an optional parameter must also be optional.");
            if (def.defaultValue != null)
                hasBegunOptionals = true;

            ScoreboardValue value = def.Create(executor.scoreboard, tokens);
            value.clarifier.IsGlobal = true;

            executor.scoreboard.TryThrowForDuplicate(value, tokens, out bool identicalDuplicate);

            if (!identicalDuplicate)
                executor.scoreboard.Add(value);

            parameters.Add(new RuntimeFunctionParameter(value, def.defaultValue));
        }

        // see if the last statement was a comment and use that for documentation
        string docs = executor.GetDocumentationString(out bool hadDocumentation);

        // the actual name of the function file
        string actualName = usesFolders ? functionName[(functionName.LastIndexOf('.') + 1)..] : functionName;

        // constructor
        RuntimeFunction function;

        if (isAsync)
        {
            function = executor.async.StartNewAsyncFunction(tokens,
                functionName, actualName, docs,
                attributes.ToArray(), asyncTarget);
            function.isAddedToExecutor = true;

            // have to manually register the base file with the executor because
            // it never gets pushed/popped from the file stack, only the different stages
            executor.AddExtraFile(function.file);
        }
        else
        {
            function = new RuntimeFunction(tokens, functionName, actualName, docs, attributes.ToArray())
            {
                documentation = docs,
                isAddedToExecutor = true
            };
        }

        // folders, if specified via dots
        if (usesFolders)
            function.file.Folders = folders;

        function.AddParameters(parameters);
        function.SignalToAttributes(tokens);

        if (!function.isExtern)
        {
            // force hash the parameters so that they can be unique.
            foreach (RuntimeFunctionParameter parameter in parameters)
                parameter.RuntimeDestination.ForceHash(functionName);

            // add decoration to it if documentation was given
            if (hadDocumentation && GlobalContext.Decorate)
            {
                function.AddCommands(docs.Trim().Split('\n').Select(str => "# " + str.Trim()));
                function.AddCommand("");
            }
        }

        bool functionDefinedAsPartial = function.HasAttribute<AttributePartial>();

        // check for duplicates and try to extend the partial function
        bool cancelRegistry = false;
        if (executor.functions.TryGetFunctions(functionName, out Function[] existingFunctions))
        {
            // this is likely an overload of another function, change the file name
            string newName = Executor.GetNextGeneratedName(function.file.name + "_overload", true, true);
            function.file.name = newName;
            function.internalName = newName;

            // loop through all functions and see if one matches parameters
            foreach (Function existingFunction in existingFunctions)
            {
                if (existingFunction is not RuntimeFunction currentFunction)
                    continue;
                if (currentFunction.ParameterCount != function.ParameterCount)
                    continue;

                bool allParametersMatch = true;
                for (int i = 0; i < function.ParameterCount; i++)
                {
                    var parameterSource = (RuntimeFunctionParameter) function.Parameters[i];
                    var parameterOther = (RuntimeFunctionParameter) currentFunction.Parameters[i];
                    ScoreboardManager.ValueType typeSource = parameterSource.RuntimeDestination.type.TypeEnum;
                    ScoreboardManager.ValueType typeOther = parameterOther.RuntimeDestination.type.TypeEnum;

                    // ReSharper disable once InvertIf
                    // only care about the type of the parameter, not the name
                    if (typeSource != typeOther)
                    {
                        allParametersMatch = false;
                        break;
                    }
                }

                if (!allParametersMatch)
                    continue;

                // check if both are defined as partial
                bool otherIsPartial = currentFunction.HasAttribute<AttributePartial>();
                if (functionDefinedAsPartial && otherIsPartial)
                {
                    function = currentFunction;
                    cancelRegistry = true;
                    break;
                }

                if (functionDefinedAsPartial || otherIsPartial)
                    // one of them is partial, yet the other is not.
                    throw functionDefinedAsPartial
                        ? new StatementException(tokens,
                            $"Function '{functionName}' already exists; it was not originally defined as partial, so it cannot be extended.")
                        : new StatementException(tokens,
                            $"Function '{functionName}' already exists; were you intending to use the 'partial' attribute to extend it?");

                // neither function is partial, this is just an overlap.
                if (function.ParameterCount != 0)
                    throw new StatementException(tokens,
                        $"Function '{functionName}' already exists with these parameter types.");
                throw new StatementException(tokens,
                    $"Function '{functionName}' already exists without parameters.");
            }
        }

        if (!cancelRegistry)
        {
            // register it with the compiler
            executor.functions.RegisterFunction(function);

            // get the function's parameters
            IEnumerable<ScoreboardValue> allRuntimeDestinations = function.Parameters
                .Where(p => p is RuntimeFunctionParameter)
                .Select(p => ((RuntimeFunctionParameter) p).RuntimeDestination);

            // ...and define them
            executor.scoreboard.DefineMany(allRuntimeDestinations);
        }

        if (function == null)
            throw new StatementException(tokens, "No other partial function matched this partial function (?)");
        if (tokens.NextIs<TokenCloseParenthesis>(false))
            tokens.Next();

        if (executor.NextIs<StatementOpenBlock>())
        {
            if (function.isExtern)
                throw new StatementException(tokens, "External functions cannot have a body.");

            var openBlock = executor.Peek<StatementOpenBlock>();

            openBlock.SetLangContext("func_" + functionName.ToLower());
            openBlock.openAction = function.BlockOpenAction;
            openBlock.CloseAction = function.BlockCloseAction;
            openBlock.ignoreAsync = true;
            openBlock.closer.ignoreAsync = true;
        }
        else if (!function.isExtern)
        {
            throw new StatementException(tokens, "No block following function definition.");
        }

        return;

        void FindAttributes()
        {
            while (tokens.NextIs<TokenAttribute>(false))
            {
                var _attribute = tokens.Next<TokenAttribute>("attribute");
                IAttribute attribute = _attribute.attribute;

                if (attributes.Any(a => a.GetType() == attribute.GetType()))
                    throw new StatementException(tokens,
                        $"Attribute '{attribute.GetDebugString()}' is already present on this function.");

                if (attribute is AttributeAsync attributeAsync)
                {
                    isAsync = true;
                    asyncTarget = attributeAsync.target;
                }

                attributes.Add(attribute);
            }
        }
    }
    [UsedImplicitly]
    public static void test(Executor executor, Statement tokens)
    {
        executor.RequireFeature(tokens, Feature.TESTS);

        // normal definition
        string testName = tokens.Next<TokenIdentifier>("test name").word;
        testName.ThrowIfWhitespace("test name", tokens);

        bool usesFolders = testName.Contains('.');
        string[] folders = null;

        if (usesFolders)
        {
            string[] split = testName.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 1)
            {
                folders = split.Take(split.Length - 1).ToArray();
            }
            else
            {
                usesFolders = false; // user wrote beyond-shit code; fix it up for them
                testName = testName.Trim('.');
            }
        }

        // see if last statement was a comment, and use that for documentation
        string docs = executor.GetDocumentationString(out bool _);

        // the actual name of the function file
        string actualName = usesFolders ? testName[(testName.LastIndexOf('.') + 1)..] : testName;

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

        if (!executor.NextIs<StatementOpenBlock>())
            return;

        var openBlock = executor.Peek<StatementOpenBlock>();

        openBlock.SetLangContext("test_" + testName.ToLower());
        openBlock.openAction = e =>
        {
            e.PushFile(test.file);
        };
        openBlock.CloseAction = e =>
        {
            e.PopFile();
        };
    }
    [UsedImplicitly]
    public static void returnFromFunction(Executor executor, Statement tokens)
    {
        RuntimeFunction activeFunction = executor.CurrentFile.runtimeFunction;

        // exceptions for when this command is run inside an async function are performed inside `AsyncFunction#TryReturnValue`

        if (activeFunction == null)
            throw new StatementException(tokens, "Cannot return a value outside of a function.");

        if (tokens.NextIs<TokenIdentifierValue>(false))
        {
            var token = tokens.Next<TokenIdentifierValue>("return value");
            activeFunction.TryReturnValue(token.value, executor, tokens);
        }
        else
        {
            if (!tokens.NextIs<TokenLiteral>(true))
                return;

            var token = tokens.Next<TokenLiteral>("return token");
            activeFunction.TryReturnValue(token, tokens, executor);
        }
    }
    [UsedImplicitly]
    public static void dialogue(Executor executor, Statement tokens)
    {
        string word = tokens.Next<TokenIdentifier>("subcommand").word.ToUpper();
        DialogueManager dialogueRegistry = executor.GetDialogueRegistry();

        switch (word)
        {
            case "OPEN":
            {
                Selector npc = tokens.Next<TokenSelectorLiteral>("npc");
                Selector players = tokens.Next<TokenSelectorLiteral>("players");

                if (!npc.AnyNonPlayers)
                    throw new StatementException(tokens, $"Selector '{npc}' will never target an NPC.");
                if (players.AnyNonPlayers)
                    throw new StatementException(tokens, $"Selector '{players}' may target non-players.");

                if (tokens.NextIs<TokenStringLiteral>(false))
                {
                    string sceneTag = tokens.Next<TokenStringLiteral>("scene");
                    sceneTag.ThrowIfWhitespace("scene", tokens);

                    executor.AddCommand(dialogueRegistry.TryGetScene(sceneTag, out Scene scene)
                        ? Command.DialogueOpen(npc.ToString(), players.ToString(), scene)
                        : Command.DialogueOpen(npc.ToString(), players.ToString(), sceneTag));
                    break;
                }

                executor.AddCommand(Command.DialogueOpen(npc.ToString(), players.ToString()));
                break;
            }
            case "CHANGE":
            {
                Selector npc = tokens.Next<TokenSelectorLiteral>("npc");
                string sceneTag = tokens.Next<TokenStringLiteral>("scene");
                sceneTag.ThrowIfWhitespace("scene", tokens);

                if (!npc.AnyNonPlayers)
                    throw new StatementException(tokens, $"Selector '{npc}' will never target an NPC.");

                if (tokens.NextIs<TokenSelectorLiteral>(true))
                {
                    Selector players = tokens.Next<TokenSelectorLiteral>("player");

                    if (players.AnyNonPlayers)
                        throw new StatementException(tokens, $"Selector '{players}' may target non-players.");

                    executor.AddCommand(Command.DialogueChange(npc.ToString(), sceneTag, players.ToString()));
                    break;
                }

                executor.AddCommand(Command.DialogueChange(npc.ToString(), sceneTag));
                break;
            }
            case "NEW":
            {
                string newSceneTag = tokens.Next<TokenStringLiteral>("scene name");
                newSceneTag.ThrowIfWhitespace("scene name", tokens);

                if (dialogueRegistry.TryGetScene(newSceneTag, out _))
                    throw new StatementException(tokens, $"Dialogue scene '{newSceneTag}' already exists.");
                if (!executor.NextIs<StatementOpenBlock>())
                    throw new StatementException(tokens,
                        "Dialogue definition must be followed by a block containing the fields for the dialogue.");

                Statement[] statements = executor.NextExecutionSet(true);
                string npcName = null;
                string text = null;
                string[] onOpen = null;
                string[] onClose = null;
                var buttons = new List<Button>();

                for (int i = 0; i < statements.Length; i++)
                {
                    Statement current = statements[i];

                    if (current is StatementUnknown unknown)
                    {
                        if (!unknown.NextIs<TokenBuilderIdentifier>(false))
                            throw new StatementException(tokens, $"Unexpected field in dialogue definition: {unknown}");
                        unknown = (StatementUnknown) unknown.ClonePrepare(executor);
                        string builderField = unknown.Next<TokenBuilderIdentifier>(null).BuilderField;

                        switch (builderField)
                        {
                            case "NAME":
                                npcName = unknown.Next<TokenStringLiteral>("name");
                                break;
                            case "TEXT":
                                text = unknown.Next<TokenStringLiteral>("text");
                                break;
                            case "BUTTON":
                                string buttonText = unknown.Next<TokenStringLiteral>("button name");
                                string[] buttonCommands = BuildCommandsFromNextExecutionSet("dialogue_button");
                                buttonCommands = CompressCommandsToFile(buttonCommands, $"scene{newSceneTag}_press",
                                    $"Called when the button '{buttonText}' is pressed by a player in the dialogue '{newSceneTag}'. @s refers to the player, not the NPC.");
                                var newButton = new Button(buttonCommands);

                                if (executor.HasLocale)
                                {
                                    string langEntryName = Executor.GetNextGeneratedName(
                                        Executor.MCC_TRANSLATE_PREFIX + newSceneTag + ".button", true, true);
                                    langEntryName = executor.SetLocaleEntry(langEntryName, buttonText, tokens, true)
                                        ?.key;
                                    if (langEntryName == null)
                                        newButton.NameString = buttonText;
                                    else
                                        newButton.NameTranslate = langEntryName;
                                }
                                else
                                {
                                    newButton.NameString = buttonText;
                                }

                                buttons.Add(newButton);
                                break;
                            case "ONOPEN":
                                onOpen = BuildCommandsFromNextExecutionSet("dialogue_onopen");
                                break;
                            case "ONCLOSE":
                                onClose = BuildCommandsFromNextExecutionSet("dialogue_onclose");
                                break;
                        }
                    }

                    continue;

                    string[] BuildCommandsFromNextExecutionSet(string langIdentifier)
                    {
                        i += 1;
                        if (i >= statements.Length)
                            return [];

                        Statement firstStatement = statements[i];

                        if (firstStatement is not StatementOpenBlock openBlock)
                            return [];

                        openBlock.SetLangContext(langIdentifier);

                        // skip open block
                        i += 1;

                        // pull statements inside into their own array
                        int insideCount = openBlock.statementsInside;
                        if (insideCount < 1)
                            return [];

                        var inside = new Statement[insideCount];
                        for (int j = 0; j < insideCount; j++)
                            inside[j] = statements[i + j];

                        // skip close block
                        i += 1;

                        // turn decoration off temporarily.
                        // contract-release pattern.
                        using ContextContract context = GlobalContext.NewInherit();
                        context.heldContext.decorate = false;

                        // create virtual file to hold commands inside code
                        var tempFile = new CommandFile(true, "temp");
                        executor.PushFile(tempFile);
                        executor.ExecuteSubsection(inside);
                        executor.PopFileDiscard();
                        return tempFile.commands.ToArray();
                    }
                }

                if (npcName == null)
                    throw new StatementException(tokens, "Field 'name' must be specified.");
                if (text == null)
                    throw new StatementException(tokens, "Field 'text' must be specified.");

                Scene newScene = new Scene(newSceneTag)
                {
                    openCommands = CompressCommandsToFile(onOpen, $"scene{newSceneTag}_open",
                        $"Called when the dialogue '{newSceneTag}' is opened by a player. @s refers to the player, not the NPC."),
                    closeCommands = CompressCommandsToFile(onClose, $"scene{newSceneTag}_close",
                        $"Called when the dialogue '{newSceneTag}' is closed by a player. @s refers to the player, not the NPC.")
                }.AddButtons(buttons);

                if (executor.HasLocale)
                {
                    string npcNameTranslationKey =
                        Executor.GetNextGeneratedName(Executor.MCC_TRANSLATE_PREFIX + newSceneTag + ".name", true,
                            true);
                    string npcNameEscapedNewlines = npcName.Replace("\\n", "%1");
                    npcNameTranslationKey = executor
                        .SetLocaleEntry(npcNameTranslationKey, npcNameEscapedNewlines, tokens, true)?.key;
                    if (npcNameTranslationKey == null)
                        newScene.NPCNameString = npcNameEscapedNewlines;
                    else
                        newScene.NPCNameTranslate = npcNameTranslationKey;
                }
                else
                {
                    newScene.NPCNameString = npcName;
                }

                if (executor.HasLocale || text.Contains("\\n"))
                {
                    if (!executor.HasLocale)
                        throw new StatementException(tokens,
                            "Use of '\\n' in dialogue texts requires localization to be enabled.");
                    string textTranslationKey =
                        Executor.GetNextGeneratedName(Executor.MCC_TRANSLATE_PREFIX + newSceneTag + ".text", true,
                            true);
                    string textEscapedNewlines = text.Replace("\\n", "%1");
                    textTranslationKey = executor.SetLocaleEntry(textTranslationKey, textEscapedNewlines, tokens, true)
                        ?.key;
                    if (textTranslationKey == null)
                        newScene.TextString = textEscapedNewlines;
                    else
                        newScene.TextTranslate = textTranslationKey;
                }
                else
                {
                    newScene.TextString = text;
                }

                dialogueRegistry.AddScene(newScene);
                break;
            }
        }

        return;

        string[] CompressCommandsToFile(string[] commands, string fileName, string decoratorDescription)
        {
            if (commands == null)
                return null;
            if (commands.Length < 2)
            {
                for (int i = 0; i < commands.Length; i++)
                    commands[i] = Command.Execute()
                        .As(Selector.INVOKER)
                        .AtSelf()
                        .Run(commands[i]);
                return commands;
            }

            CommandFile file = Executor.GetNextGeneratedFile(fileName, false);
            executor.AddExtraFile(file);

            if (GlobalContext.Decorate)
            {
                file.Add("# " + decoratorDescription);
                file.Add("");
            }

            file.Add(commands);
            return
            [
                Command.Execute()
                    .As(Selector.INVOKER)
                    .AtSelf()
                    .Run(Command.Function(file.CommandReference))
            ];
        }
    }
    [UsedImplicitly]
    public static void forEntities(Executor executor, Statement tokens)
    {
        var selector = tokens.Next<TokenSelectorLiteral>("entities");

        Coordinate x = Coordinate.here,
            y = Coordinate.here,
            z = Coordinate.here;

        if (tokens.NextIs<TokenIdentifier>(false))
        {
            string identifier = tokens.Next<TokenIdentifier>("subcommand").word;

            if (!identifier.ToUpper().Equals("AT"))
                throw new StatementException(tokens, "Invalid subcommand for the 'for' command. Did you mean 'at'?");

            if (tokens.NextIs<TokenCoordinateLiteral>(true))
                x = tokens.Next<TokenCoordinateLiteral>("x");
            if (tokens.NextIs<TokenCoordinateLiteral>(true))
                y = tokens.Next<TokenCoordinateLiteral>("y");
            if (tokens.NextIs<TokenCoordinateLiteral>(true))
                z = tokens.Next<TokenCoordinateLiteral>("z");
        }

        if (!executor.HasNext)
            throw new StatementException(tokens, "Unexpected end-of-file after for-statement.");

        if (executor.NextIs<StatementOpenBlock>())
        {
            var block = executor.Peek<StatementOpenBlock>();
            block.ignoreAsync = true;
            block.SetLangContext("for_" + selector.selector.core);

            CommandFile file = Executor.GetNextGeneratedFile("for", false);

            if (GlobalContext.Decorate)
            {
                if (x.HasEffect || y.HasEffect || z.HasEffect)
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

            block.openAction = e =>
            {
                e.PushFile(file);
            };
            block.CloseAction = e =>
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
    [UsedImplicitly]
    public static void await(Executor executor, Statement tokens)
    {
        if (!executor.async.IsInAsync)
            throw new StatementException(tokens, "The await command can only be used in an async context.");

        AsyncFunction async = executor.async.CurrentFunction;

        // await <time>
        if (tokens.NextIs<TokenIntegerLiteral>(false))
        {
            int ticks = tokens
                .Next<TokenIntegerLiteral>("time")
                .Scaled(IntMultiplier.none);

            async.FinishStage(ticks);
            async.StartNewStage();
            return;
        }

        // await until <conditions>
        // await while <conditions>
        if (tokens.NextIs<TokenIdentifier>(false))
        {
            string word = tokens.Next<TokenIdentifier>("until/while").word;

            bool isWhile = word.ToUpper() switch
            {
                "UNTIL" => false,
                "WHILE" => true,
                _ => throw new StatementException(tokens,
                    $"Invalid await subcommand '{word}'. Must be 'until' or 'while'.")
            };

            ComparisonSet set = ComparisonSet.GetComparisons(executor, tokens);
            set.InvertAll(isWhile);

            async.FinishStageImmediate();
            async.StartNewStage();
            async.FinishStage(set, tokens);
            async.StartNewStage();
            return;
        }

        // await <async function call>
        // ReSharper disable once InvertIf
        if (tokens.NextIs<TokenAwaitable>(false))
        {
            var awaitable = tokens.Next<TokenAwaitable>("async");

            if (awaitable.function.target == AsyncTarget.Local && async.target == AsyncTarget.Global)
                throw new StatementException(tokens,
                    "Cannot await an async(local) function from an async(global) function; no way to know the entity to wait on.");

            // throws exception if deadlock is detected
            async.AddWaitsOn(awaitable.function, tokens);

            ComparisonSet set = [];

            // check for the called function's 'running' == false
            ScoreboardValue running = awaitable.function.runningValue;
            int lineNumber = tokens.Lines[0];

            var value = new ComparisonValue(
                new TokenIdentifierValue(running.Name, running, lineNumber),
                TokenCompare.Type.EQUAL,
                new TokenBooleanLiteral(false, lineNumber),
                false
            );

            set.Add(value);

            async.FinishStageImmediate();
            async.StartNewStage();
            async.FinishStage(set, tokens);
            async.StartNewStage();
            return;
        }

        throw new StatementException(tokens,
            $"Invalid await subcommand '{tokens.Next<Token>("subcommand").AsString()}'. Supports a time in ticks, an async function call, or while/until with condition(s).");
    }
}