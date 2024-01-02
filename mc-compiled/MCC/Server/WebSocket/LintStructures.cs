using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.ServerWebSocket
{
    public class ErrorStructure
    {
        public enum During
        {
            tokenizer, execution, unknown
        }

        public readonly During during;
        public readonly int[] lines;
        public readonly string message;

        public ErrorStructure(During during, int[] lines, string message)
        {
            this.during = during;
            this.lines = lines;
            this.message = message;
        }
        public static ErrorStructure Wrap(TokenizerException exception)
        {
            return new ErrorStructure(During.tokenizer, exception.lines, exception.Message);
        }
        public static ErrorStructure Wrap(StatementException exception)
        {
            return new ErrorStructure(During.execution, exception.statement.Lines, exception.Message);
        }
        public static ErrorStructure Wrap(FeederException exception)
        {
            return new ErrorStructure(During.tokenizer, exception.feeder.Lines, exception.Message);
        }
        public static ErrorStructure Wrap(Exception exception, int[] lines)
        {
            return new ErrorStructure(During.unknown, lines, exception.ToString());
        }
        

        public string ToJSON()
        {
            JArray errors = new JArray();

            foreach (int line in lines)
            {
                JObject error = new JObject();
                error["line"] = line;
                error["error"] = message.Base64Encode();
                errors.Add(error);
            }

            JObject json = new JObject();
            json["action"] = "seterrors";
            json["errors"] = errors;

            return json.ToString();
        }
    }
    public class LintStructure
    {
        internal List<string> ppvs = new List<string>();
        internal List<VariableStructure> variables = new List<VariableStructure>();
        internal List<FunctionStructure> functions = new List<FunctionStructure>();
        internal List<MacroStructure> macros = new List<MacroStructure>();

        public LintStructure() { }
        public static LintStructure Harvest(Executor executor)
        {
            LintStructure lint = new LintStructure();

            // harvest PPVs
            lint.ppvs.AddRange(executor.PPVNames);

            // harvest variables
            lint.variables.AddRange(executor.scoreboard.values.Select(sb => VariableStructure.Wrap(sb)));

            // harvest functions
            lint.functions.AddRange(executor.functions
                .FetchAll()
                .Where(func => !(func is AttributeFunction))
                .Select(func => FunctionStructure.Wrap(func, lint)));

            // harvest macros
            lint.macros.AddRange(executor.macros
                .Select(macro => MacroStructure.Wrap(macro)));
            return lint;
        }

        JArray PPVToJSON()
        {
            JArray json = new JArray();
            foreach (string ppv in ppvs)
                json.Add(ppv);
            return json;
        }
        JArray FunctionsToJSON()
        {
            JArray array = new JArray();
            foreach(FunctionStructure function in functions)
                array.Add(function.ToJSON());
            return array;
        }
        JArray MacrosToJSON()
        {
            JArray array = new JArray();
            foreach(MacroStructure macro in macros)
                array.Add(macro.ToJSON());
            return array;
        }
        public string ToJSON()
        {
            JObject json = new JObject();
            json["action"] = "setsymbols";
            json["ppvs"] = PPVToJSON();
            json["variables"] = VariableStructure.Join(this.variables);
            json["functions"] = FunctionsToJSON();
            json["macros"] = MacrosToJSON();
            return json.ToString();
        }

        public override string ToString()
        {
            return $"LintStructure: {ppvs.Count} PPV, {variables.Count} VARS, {functions.Count} FUNCS, {macros.Count} MACROS";
        }
    }

    public struct FunctionStructure
    {
        public readonly string name;
        public readonly string returnType;
        public readonly string docs;
        public readonly List<VariableStructure> args;

        public FunctionStructure(string name, string returnType, string docs, params VariableStructure[] args)
        {
            this.name = name;
            this.returnType = returnType;
            this.docs = docs;
            this.args = new List<VariableStructure>(args);
        }
        public static FunctionStructure Wrap(Function function, LintStructure parent)
        {
            // now readable :)
            // thanks past luke :ok_hand:

            string returnType = function.Returns;

            int count = function.ParameterCount;
            List<VariableStructure> variables = new List<VariableStructure>();
            FunctionParameter[] parameters = function.Parameters;

            for (int i = 0; i < count; i++)
            {
                FunctionParameter parameter = parameters[i];

                switch(parameter)
                {
                    case RuntimeFunctionParameterAny runtimeParameterAny:
                        variables.Add(VariableStructure.Any(runtimeParameterAny.aliasName));
                        break;
                    case RuntimeFunctionParameter runtimeParameter:
                        variables.Add(VariableStructure.Wrap(runtimeParameter.RuntimeDestination));
                        break;
                    case CompiletimeFunctionParameter compileTimeParameter:
                        variables.Add(new VariableStructure(compileTimeParameter.name, compileTimeParameter.GetRequiredTypeName(), Executor.UNDOCUMENTED_TEXT));
                        break;
                }
            }

            string docs = function.Documentation ?? Executor.UNDOCUMENTED_TEXT;
            return new FunctionStructure(function.Keyword, returnType, docs, variables.ToArray());
        }
        public JObject ToJSON()
        {
            JObject json = new JObject();
            json["name"] = name;
            json["arguments"] = VariableStructure.Join(this.args);
            json["docs"] = docs.Base64Encode();
            json["return"] = returnType;
            return json;
        }
    }
    public struct VariableStructure
    {
        public readonly string name;
        public readonly string type;
        public readonly string docs;

        public VariableStructure(string name, string type, string docs)
        {
            this.name = name;
            this.type = type;
            this.docs = docs;
        }
        public static VariableStructure Any(string name, string docs = null)
        {
            return new VariableStructure(name, "any", docs ?? Executor.UNDOCUMENTED_TEXT);
        }
        public static VariableStructure Wrap(ScoreboardValue value)
        {
            VariableStructure structure = new VariableStructure(value.Name, value.GetExtendedTypeKeyword(), value.Documentation);
            return structure;
        }
        public static VariableStructure Wrap(RuntimeFunctionParameter parameter)
        {
            ScoreboardValue value = parameter.RuntimeDestination;
            VariableStructure structure = new VariableStructure(value.Name, value.GetExtendedTypeKeyword(), value.Documentation);

            return structure;
        }
        public JObject ToJSON()
        {
            JObject json = new JObject();
            json["name"] = name;
            json["type"] = type;
            json["docs"] = docs.Base64Encode();
            return json;
        }
        public static JArray Join(List<VariableStructure> variables)
        {
            return new JArray(variables.Select(variable => variable.ToJSON()).ToArray());
        }
    }
    public struct MacroStructure
    {
        public readonly string name;
        public readonly string[] arguments;
        public readonly string docs;

        public MacroStructure(string name, string[] arguments, string docs)
        {
            this.name = name;
            this.arguments = arguments;
            this.docs = docs;
        }
        public static MacroStructure Wrap(Macro macro)
        {
            return new MacroStructure(macro.name, macro.argNames,
                macro.documentation ?? Executor.UNDOCUMENTED_TEXT);
        }

        public JObject ToJSON()
        {
            JObject json = new JObject();
            json["name"] = name;
            json["arguments"] = new JArray(arguments);
            json["docs"] = docs.Base64Encode();
            return json;
        }
    }
}
