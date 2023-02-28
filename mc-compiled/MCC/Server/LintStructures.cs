using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Server
{
    public class ErrorStructure
    {
        public enum During
        {
            tokenizer, execution, unknown
        }

        public readonly During during;
        public readonly int line;
        public readonly string message;

        public ErrorStructure(During during, int line, string message)
        {
            this.during = during;
            this.line = line;
            this.message = message;
        }
        public static ErrorStructure Wrap(TokenizerException exception)
        {
            return new ErrorStructure(During.tokenizer, exception.line, exception.Message);
        }
        public static ErrorStructure Wrap(StatementException exception)
        {
            return new ErrorStructure(During.execution, exception.statement.Line, exception.Message);
        }
        public static ErrorStructure Wrap(FeederException exception)
        {
            return new ErrorStructure(During.tokenizer, exception.feeder.Line, exception.Message);
        }
        public static ErrorStructure Wrap(Exception exception, int line)
        {
            return new ErrorStructure(During.unknown, line, exception.ToString());
        }

        public string ToJSON()
        {
            JObject error = new JObject();
            error["line"] = line;
            error["error"] = message.Base64Encode();

            JArray errors = new JArray();
            errors.Add(error);

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

        public LintStructure() { }
        public static LintStructure Harvest(Executor executor)
        {
            LintStructure lint = new LintStructure();
            lint.ppvs.AddRange(executor.PPVNames);
            lint.variables.AddRange(executor.scoreboard.values.Select(sb => VariableStructure.Wrap(sb)));
            lint.functions.AddRange(executor.functions
                .FetchAll()
                .Where(func => func is RuntimeFunction)
                .Select(func => FunctionStructure.Wrap(func, lint)));
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
        public string ToJSON()
        {
            JObject json = new JObject();
            json["action"] = "setsymbols";
            json["ppvs"] = PPVToJSON();
            json["variables"] = VariableStructure.Join(this.variables);
            json["functions"] = FunctionsToJSON();
            return json.ToString();
        }

        public override string ToString()
        {
            return $"LintStructure: {ppvs.Count} PPV, {variables.Count} VARS, {functions.Count} FUNCS";
        }
    }

    public struct FunctionStructure
    {
        public readonly string name;
        public readonly string returnType;
        public readonly List<VariableStructure> args;

        public FunctionStructure(string name, string returnType, params VariableStructure[] args)
        {
            this.name = name;
            this.returnType = returnType;
            this.args = new List<VariableStructure>(args);
        }
        public static FunctionStructure Wrap(Function function, LintStructure parent)
        {
            // now readable :)
            string returnType = function.Returns;

            int count = function.ParameterCount;
            List<VariableStructure> variables = new List<VariableStructure>();
            FunctionParameter[] parameters = function.Parameters;

            for (int i = 0; i < count; i++)
            {
                FunctionParameter parameter = parameters[i];

                if (parameter is RuntimeFunctionParameter runtimeParameter)
                {
                    variables.Add(VariableStructure.Wrap(runtimeParameter.runtimeDestination));
                }
            }

            return new FunctionStructure(function.Keyword, returnType, variables.ToArray());
        }
        public JObject ToJSON()
        {
            JObject json = new JObject();
            json["name"] = name;
            json["arguments"] = VariableStructure.Join(this.args);
            json["return"] = returnType;
            return json;
        }
    }
    public struct VariableStructure
    {
        public readonly string name;
        public readonly string type;

        public VariableStructure(string name, string type)
        {
            this.name = name;
            this.type = type;
        }
        public static VariableStructure Wrap(ScoreboardValue value)
        {
            VariableStructure structure = new VariableStructure(value.AliasName, value.GetExtendedTypeKeyword());
            return structure;
        }
        public static VariableStructure Wrap(RuntimeFunctionParameter parameter)
        {
            ScoreboardValue value = parameter.runtimeDestination;
            VariableStructure structure = new VariableStructure(value.AliasName, value.GetExtendedTypeKeyword());

            return structure;
        }
        public JObject ToJSON()
        {
            JObject json = new JObject();
            json["name"] = name;
            json["type"] = type;
            return json;
        }
        public static JArray Join(List<VariableStructure> variables)
        {
            return new JArray(variables.Select(variable => variable.ToJSON()).ToArray());
        }
    }
}
