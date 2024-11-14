using System;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions;
using mc_compiled.MCC.Functions.Types;
using Newtonsoft.Json.Linq;

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
            JArray errors = [];

            foreach (int line in this.lines)
            {
                JObject error = new JObject();
                error["line"] = line;
                error["error"] = this.message.Base64Encode();
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
        internal List<string> ppvs = [];
        internal List<VariableStructure> variables = [];
        internal List<FunctionStructure> functions = [];
        internal List<MacroStructure> macros = [];

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
                .Where(func => func.AdvertiseOverLSP)
                .Select(func => FunctionStructure.Wrap(func, lint))
                .Distinct());

            // harvest macros
            lint.macros.AddRange(executor.macros
                .Select(macro => MacroStructure.Wrap(macro)));
            return lint;
        }

        JArray PPVToJSON()
        {
            JArray json = [];
            foreach (string ppv in this.ppvs)
                json.Add(ppv);
            return json;
        }
        JArray FunctionsToJSON()
        {
            JArray array = [];
            foreach(FunctionStructure function in this.functions)
                array.Add(function.ToJSON());
            return array;
        }
        JArray MacrosToJSON()
        {
            JArray array = [];
            foreach(MacroStructure macro in this.macros)
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
            return $"LintStructure: {this.ppvs.Count} PPV, {this.variables.Count} VARS, {this.functions.Count} FUNCS, {this.macros.Count} MACROS";
        }
    }

    public readonly struct FunctionStructure : IEquatable<FunctionStructure>
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
            this.args = [..args];
        }
        public static FunctionStructure Wrap(Function function, LintStructure parent)
        {
            // now readable :)
            // thanks past luke :ok_hand:
            string returnType = function.Returns;

            int count = function.ParameterCount;
            List<VariableStructure> variables = [];
            FunctionParameter[] parameters = function.Parameters;

            for (int i = 0; i < count; i++)
            {
                FunctionParameter parameter = parameters[i];

                switch(parameter)
                {
                    case RuntimeFunctionParameterDynamic runtimeParameterAny:
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

        public override bool Equals(object obj)
        {
            if (obj is not FunctionStructure structure)
                return false;
            if (!this.name.Equals(structure.name))
                return false;
            if (this.returnType != null)
            {
                if (!this.returnType.Equals(structure.returnType))
                    return false;
            }
            if(structure.args.Count != this.args.Count)
                return false;

            for(int i = 0; i < this.args.Count; i++)
            {
                if (!this.args[i].Equals(structure.args[i]))
                    return false;
            }

            return true;
        }
        public override int GetHashCode()
        {
            int hashCode = 1090742913;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.returnType);

            foreach(VariableStructure structure in this.args)
            {
                hashCode ^= structure.GetHashCode();
            }

            return hashCode;
        }

        public JObject ToJSON()
        {
            JObject json = new JObject();
            json["name"] = this.name;
            json["arguments"] = VariableStructure.Join(this.args);
            json["docs"] = this.docs.Base64Encode();
            json["return"] = this.returnType;
            return json;
        }

        public bool Equals(FunctionStructure other)
        {
            return this.name == other.name && this.returnType == other.returnType && this.docs == other.docs && Equals(this.args, other.args);
        }
        public static bool operator ==(FunctionStructure left, FunctionStructure right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(FunctionStructure left, FunctionStructure right)
        {
            return !(left == right);
        }
    }
    public readonly struct VariableStructure : IEquatable<VariableStructure>
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
            return new VariableStructure(name, "T", docs ?? Executor.UNDOCUMENTED_TEXT);
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
            json["name"] = this.name;
            json["type"] = this.type;
            json["docs"] = this.docs.Base64Encode();
            return json;
        }
        public static JArray Join(List<VariableStructure> variables)
        {
            return new JArray(variables.Select(variable => variable.ToJSON()).ToArray<object>());
        }
        public override bool Equals(object obj)
        {
            return obj is VariableStructure structure && this.name == structure.name && this.type == structure.type;
        }
        public override int GetHashCode()
        {
            int hashCode = -1614644627;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.type);
            return hashCode;
        }

        public static bool operator ==(VariableStructure left, VariableStructure right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(VariableStructure left, VariableStructure right)
        {
            return !(left == right);
        }
        public bool Equals(VariableStructure other)
        {
            return this.name == other.name && this.type == other.type && this.docs == other.docs;
        }
    }
    public readonly struct MacroStructure(string name, string[] arguments, string docs)
    {
        public readonly string name = name;
        public readonly string[] arguments = arguments;
        public readonly string docs = docs;

        public static MacroStructure Wrap(Macro macro)
        {
            return new MacroStructure(macro.name, macro.argNames,
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
}
