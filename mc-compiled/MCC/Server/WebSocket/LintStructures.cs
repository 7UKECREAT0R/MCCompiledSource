using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using Newtonsoft.Json.Linq;

namespace mc_compiled.MCC.ServerWebSocket;

public class LegacyErrorStructure
{
    public enum During
    {
        tokenizer, execution, unknown
    }

    public readonly During during;
    public readonly int[] lines;
    public readonly string message;

    public LegacyErrorStructure(During during, int[] lines, string message)
    {
        this.during = during;
        this.lines = lines;
        this.message = message;
    }
    public static LegacyErrorStructure Wrap(TokenizerException exception)
    {
        return new LegacyErrorStructure(During.tokenizer, exception.lines, exception.Message);
    }
    public static LegacyErrorStructure Wrap(StatementException exception)
    {
        return new LegacyErrorStructure(During.execution, exception.statement.Lines, exception.Message);
    }
    public static LegacyErrorStructure Wrap(FeederException exception)
    {
        return new LegacyErrorStructure(During.tokenizer, exception.feeder.Lines, exception.Message);
    }
    public static LegacyErrorStructure Wrap(Exception exception, int[] lines)
    {
        return new LegacyErrorStructure(During.unknown, lines, exception.ToString());
    }

    public string ToJSON()
    {
        JArray errors = [];

        foreach (int line in this.lines)
        {
            var error = new JObject();
            error["line"] = line;
            error["error"] = this.message.Base64Encode();
            errors.Add(error);
        }

        var json = new JObject();
        json["action"] = "seterrors";
        json["errors"] = errors;

        return json.ToString();
    }
}

public class LegacyLintStructure
{
    private readonly List<LegacyFunctionStructure> functions = [];
    private readonly List<LegacyMacroStructure> macros = [];
    private readonly List<string> ppvs = [];
    private readonly List<LegacyVariableStructure> variables = [];

    public static LegacyLintStructure Harvest(Emission emission)
    {
        var lint = new LegacyLintStructure();

        // harvest PPVs
        lint.ppvs.AddRange(emission.DefinedPPVs);

        // harvest variables
        lint.variables.AddRange(emission.DefinedValues.Select(LegacyVariableStructure.Wrap));

        // harvest functions
        lint.functions.AddRange(emission.DefinedFunctions
            .Where(func => func.AdvertiseOverLSP)
            .Select(func => LegacyFunctionStructure.Wrap(func, lint))
            .Distinct());

        // harvest macros
        lint.macros.AddRange(emission.DefinedMacros.Select(LegacyMacroStructure.Wrap));
        return lint;
    }

    private JArray PPVToJSON()
    {
        JArray json = [];
        foreach (string ppv in this.ppvs)
            json.Add(ppv);
        return json;
    }
    private JArray FunctionsToJSON()
    {
        JArray array = [];
        foreach (LegacyFunctionStructure function in this.functions)
            array.Add(function.ToJSON());
        return array;
    }
    private JArray MacrosToJSON()
    {
        JArray array = [];
        foreach (LegacyMacroStructure macro in this.macros)
            array.Add(macro.ToJSON());
        return array;
    }
    public string ToJSON()
    {
        var json = new JObject();
        json["action"] = "setsymbols";
        json["ppvs"] = PPVToJSON();
        json["variables"] = LegacyVariableStructure.Join(this.variables);
        json["functions"] = FunctionsToJSON();
        json["macros"] = MacrosToJSON();
        return json.ToString();
    }

    public override string ToString()
    {
        return
            $"LintStructure: {this.ppvs.Count} PPV, {this.variables.Count} VARS, {this.functions.Count} FUNCS, {this.macros.Count} MACROS";
    }
}

public readonly struct LegacyFunctionStructure : IEquatable<LegacyFunctionStructure>
{
    public readonly string name;
    public readonly string returnType;
    public readonly string docs;
    public readonly List<LegacyVariableStructure> args;

    public LegacyFunctionStructure(string name, string returnType, string docs, params LegacyVariableStructure[] args)
    {
        this.name = name;
        this.returnType = returnType;
        this.docs = docs;
        this.args = [..args];
    }
    public static LegacyFunctionStructure Wrap(Function function, LegacyLintStructure parent)
    {
        // now readable :)
        // thanks past luke :ok_hand:
        string returnType = function.Returns;

        int count = function.ParameterCount;
        List<LegacyVariableStructure> variables = [];
        FunctionParameter[] parameters = function.Parameters;

        for (int i = 0; i < count; i++)
        {
            FunctionParameter parameter = parameters[i];

            switch (parameter)
            {
                case RuntimeFunctionParameterDynamic runtimeParameterAny:
                    variables.Add(LegacyVariableStructure.Any(runtimeParameterAny.aliasName));
                    break;
                case RuntimeFunctionParameter runtimeParameter:
                    variables.Add(LegacyVariableStructure.Wrap(runtimeParameter.RuntimeDestination));
                    break;
                case CompiletimeFunctionParameter compileTimeParameter:
                    variables.Add(new LegacyVariableStructure(compileTimeParameter.name,
                        compileTimeParameter.GetRequiredTypeName(), Executor.UNDOCUMENTED_TEXT));
                    break;
            }
        }

        string docs = function.Documentation ?? Executor.UNDOCUMENTED_TEXT;
        return new LegacyFunctionStructure(function.Keyword, returnType, docs, variables.ToArray());
    }

    public override bool Equals(object obj)
    {
        if (obj is not LegacyFunctionStructure structure)
            return false;
        if (!this.name.Equals(structure.name))
            return false;
        if (this.returnType != null)
            if (!this.returnType.Equals(structure.returnType))
                return false;
        if (structure.args.Count != this.args.Count)
            return false;

        for (int i = 0; i < this.args.Count; i++)
            if (!this.args[i].Equals(structure.args[i]))
                return false;

        return true;
    }
    public override int GetHashCode()
    {
        int hashCode = 1090742913;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.name);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.returnType);

        foreach (LegacyVariableStructure structure in this.args)
            hashCode ^= structure.GetHashCode();

        return hashCode;
    }

    public JObject ToJSON()
    {
        var json = new JObject();
        json["name"] = this.name;
        json["arguments"] = LegacyVariableStructure.Join(this.args);
        json["docs"] = this.docs.Base64Encode();
        json["return"] = this.returnType;
        return json;
    }

    public bool Equals(LegacyFunctionStructure other)
    {
        return this.name == other.name && this.returnType == other.returnType && this.docs == other.docs &&
               Equals(this.args, other.args);
    }
    public static bool operator ==(LegacyFunctionStructure left, LegacyFunctionStructure right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(LegacyFunctionStructure left, LegacyFunctionStructure right)
    {
        return !(left == right);
    }
}

public readonly struct LegacyVariableStructure : IEquatable<LegacyVariableStructure>
{
    public readonly string name;
    public readonly string type;
    public readonly string docs;

    public LegacyVariableStructure(string name, string type, string docs)
    {
        this.name = name;
        this.type = type;
        this.docs = docs;
    }
    public static LegacyVariableStructure Any(string name, string docs = null)
    {
        return new LegacyVariableStructure(name, "T", docs ?? Executor.UNDOCUMENTED_TEXT);
    }
    public static LegacyVariableStructure Wrap(ScoreboardValue value)
    {
        var structure = new LegacyVariableStructure(value.Name, value.GetExtendedTypeKeyword(), value.Documentation);
        return structure;
    }
    public static LegacyVariableStructure Wrap(RuntimeFunctionParameter parameter)
    {
        ScoreboardValue value = parameter.RuntimeDestination;
        var structure = new LegacyVariableStructure(value.Name, value.GetExtendedTypeKeyword(), value.Documentation);

        return structure;
    }
    public JObject ToJSON()
    {
        var json = new JObject();
        json["name"] = this.name;
        json["type"] = this.type;
        json["docs"] = this.docs.Base64Encode();
        return json;
    }
    public static JArray Join(List<LegacyVariableStructure> variables)
    {
        return new JArray(variables.Select(variable => variable.ToJSON()).ToArray<object>());
    }
    public override bool Equals(object obj)
    {
        return obj is LegacyVariableStructure structure && this.name == structure.name && this.type == structure.type;
    }
    public override int GetHashCode()
    {
        int hashCode = -1614644627;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.name);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.type);
        return hashCode;
    }

    public static bool operator ==(LegacyVariableStructure left, LegacyVariableStructure right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(LegacyVariableStructure left, LegacyVariableStructure right)
    {
        return !(left == right);
    }
    public bool Equals(LegacyVariableStructure other)
    {
        return this.name == other.name && this.type == other.type && this.docs == other.docs;
    }
}

public readonly struct LegacyMacroStructure(string name, string[] arguments, string docs)
{
    public readonly string name = name;
    public readonly string[] arguments = arguments;
    public readonly string docs = docs;

    public static LegacyMacroStructure Wrap(Macro macro)
    {
        return new LegacyMacroStructure(macro.name, macro.argNames,
            macro.documentation ?? Executor.UNDOCUMENTED_TEXT);
    }

    public JObject ToJSON()
    {
        var json = new JObject
        {
            ["name"] = this.name,
            ["arguments"] = new JArray(this.arguments.Cast<object>().ToArray()),
            ["docs"] = this.docs.Base64Encode()
        };
        return json;
    }
}