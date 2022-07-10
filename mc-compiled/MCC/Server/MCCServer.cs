using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Server
{
    /// <summary>
    /// MCCompiled Language Server for fast and efficient cross-application linting and compilation.
    /// </summary>
    public class MCCServer : IDisposable
    {

        public const int PORT = 11830;
        public readonly string ADDRESS = $"http://localhost:{PORT}/";
        public readonly string COMPILE_ADDRESS = $"http://localhost:{PORT}/compile/";
        public readonly string[] VALID_ACTIONS =
        {
            "lint", "compile", "version", "close", "heartbeat"
        };

        public readonly HttpListener server;

        string orp, obp;
        bool running;
        public MCCServer(string orp, string obp)
        {
            server = new HttpListener();
            server.Prefixes.Add(ADDRESS);
            server.Start();
            running = true;

            this.orp = orp;
            this.obp = obp;

            // synchronously wait for requests to come in
            BeginListening().GetAwaiter().GetResult();

            Dispose();
            return;
        }

        static string GetAction(string url)
        {
            int index = url.IndexOf('/', 8);

            if (index == -1)
                return "";
            if (url.Length <= index + 1)
                return "";

            string chunk = url.Substring(index + 1);
            index = chunk.IndexOf('/');

            if (index == -1)
                return chunk;
            else
                return chunk.Substring(0, index);
        }
        static string GetArgument(string url)
        {
            int index = url.LastIndexOf('/');

            if (index == -1)
                return "";
            if (url.Length <= index + 1)
                return "";

            return url.Substring(index + 1);
        }


        /// <summary>
        /// Begin listening 
        /// </summary>
        async Task BeginListening()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Now listening for HTTP traffic on address '{0}'.", ADDRESS);
            Console.ForegroundColor = ConsoleColor.White;

            while(running)
            {
                // wait for request to come in
                HttpListenerContext context = await server.GetContextAsync();

                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string url = request.Url.AbsoluteUri;

                byte[] result;

                if (!url.Contains("favicon"))
                {
                    string action = GetAction(url);
                    string arg = GetArgument(url);
                    string method = request.HttpMethod;
                    string host = request.UserHostName;

                    string resultString = await RunAction(action, arg, request, response);
                    result = Encoding.UTF8.GetBytes(resultString);
                }
                else
                    result = new byte[0];

                response.ContentEncoding = Encoding.UTF8;
                response.AddHeader("Access-Control-Allow-Origin", "*");
                await response.OutputStream.WriteAsync(result, 0, result.Length);
                response.Close();
            }
        }
        async Task<string> RunAction(string action, string argument, HttpListenerRequest request, HttpListenerResponse response)
        {
            if (!VALID_ACTIONS.Contains(action))
            {
                response.StatusCode = 400;
                response.ContentType = "application/json";
                return "{ \"response\": \"Invalid action.\" }";
            }

            if(action.Equals("heartbeat"))
            {
                response.StatusCode = 200;
                response.ContentType = "application/json";
                return "{ \"response\": \"OK\" }";
            }

            if (action.Equals("lint"))
            {
                // compile and gather information without emitting files
                if (request.ContentLength64 > (long)int.MaxValue)
                {
                    response.StatusCode = 413;
                    response.ContentType = "application/json";
                    return "{ \"response\": \"Code is too long.\" }";
                }

                int length = (int)request.ContentLength64;

                if (Program.DEBUG)
                    Console.WriteLine("Linting {0} bytes of code...", length);

                byte[] buffer = new byte[length];
                await request.InputStream.ReadAsync(buffer, 0, length);
                string code = Encoding.UTF8.GetString(buffer);

                try
                {
                    Token[] tokens = new Tokenizer(code).Tokenize();
                    Statement[] statements = Assembler.AssembleTokens(tokens);
                    Executor executor = new Executor(statements, new Program.InputPPV[0], argument, obp, orp);
                    executor.Linter().Execute();

                    // gather information.
                    LintStructure lint = LintStructure.Harvest(executor);

                    if (Program.DEBUG)
                        Console.WriteLine("\tSuccess. " + lint.ToString());

                    string json = lint.ToJSON();
                    executor.Cleanup(); // free executor resources now
                    response.ContentType = "application/json";
                    return json;
                }
                catch (TokenizerException exc)
                {
                    if (Program.DEBUG)
                        Console.WriteLine("\tError. " + exc.Message);
                    response.StatusCode = 200;
                    response.ContentType = "application/json";
                    return ErrorStructure.Wrap(exc).ToJSON();
                }
                catch (StatementException exc)
                {
                    if (Program.DEBUG)
                        Console.WriteLine("\tError. " + exc.Message);
                    response.StatusCode = 200;
                    response.ContentType = "application/json";
                    return ErrorStructure.Wrap(exc).ToJSON();
                } catch(Exception exc)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                        throw;
                    if (Program.DEBUG)
                        Console.WriteLine("\tFatal Error:\n\n" + exc.ToString());
                    Console.WriteLine(exc.ToString());
                    response.StatusCode = 500;
                    response.ContentType = "application/json";
                    return "{ \"response\": \"Fatal Error\" }";
                }
            }

            if (action.Equals("compile"))
            {
                if (request.ContentLength64 > (long)int.MaxValue)
                {
                    response.StatusCode = 413;
                    response.ContentType = "application/json";
                    return "{ \"response\": \"Code is too long.\" }";
                }

                int length = (int)request.ContentLength64;

                if (Program.DEBUG)
                    Console.WriteLine("Compiling {0} bytes of code...", length);

                byte[] buffer = new byte[length];
                await request.InputStream.ReadAsync(buffer, 0, length);
                string code = Encoding.UTF8.GetString(buffer);

                Program.PrepareToCompile();
                bool success = Program.RunMCCompiledCode(code, COMPILE_ADDRESS + argument + ".mcc", new Program.InputPPV[0], obp, orp);

                response.StatusCode = success ? 200 : 500;
                response.ContentType = "application/json";
                return success ? "{}" : "{ \"response\": \"Encountered error.\" }";
            }

            if (action.Equals("version"))
            {
                if (Program.DEBUG)
                    Console.WriteLine("Returning version {0}.", Compiler.Executor.MCC_VERSION);

                response.StatusCode = 200;
                response.ContentType = "application/json";
                return "{ \"version\": \"" + Compiler.Executor.MCC_VERSION + "\" }";
            }

            if (action.Equals("close"))
            {
                if (Program.DEBUG)
                    Console.WriteLine("Shutting down server...");

                response.StatusCode = 200;
                running = false;
                response.ContentType = "application/json";
                return "{ \"response\": \"Closing MCCompiled server.\" }";
            }

            return null;
        }

        bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed)
                return;

            if(server != null && server.IsListening)
                server.Stop();

            _isDisposed = true;
        }
    }

    public class ErrorStructure
    {
        public enum During
        {
            tokenizer, execution
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

        public string ToJSON() =>
            $@"{{ ""type"": ""error"", ""during"": ""{during}"", ""line"": {line}, ""message"": ""{message}"" }}";
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
            lint.variables.AddRange(executor.scoreboard.definedTempVars.Select(str => VariableStructure.Int(str)));
            lint.functions.AddRange(executor.functions.Select(func => FunctionStructure.Wrap(func, lint)));
            return lint;
        }

        string PPVToJSON()
        {
            int c = ppvs.Count;
            if (c == 0)
                return "[]";
            if (c == 1)
                return "[ \"" + ppvs[0] + "\" ]";

            return "[ \"" + string.Join("\", \"", ppvs) + "\" ]";
        }
        string FunctionsToJSON()
        {
            int c = functions.Count;
            if (c == 0)
                return "[]";
            if (c == 1)
                return "[ " + functions[0].ToJSON() + " ]";

            return "[ " + string.Join(", ", functions.Select(f => f.ToJSON())) + " ]";
        }
        public string ToJSON()
        {
            return $@"{{ ""type"": ""success"", ""ppvs"": {PPVToJSON()}, ""variables"": [{VariableStructure.Join(this.variables)}], ""functions"": {FunctionsToJSON()} }}";
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
            // i know this is barely readable
            string returnType = function.returnValue == null ? null : function.returnValue.GetTypeKeyword();

            int count = function.ParameterCount;
            List<VariableStructure> variables = new List<VariableStructure>();

            for (int i = 0; i < count; i++)
            {
                FunctionParameter parameter = function.parameters[i];

                if (parameter.IsScoreboard)
                    variables.Add(VariableStructure.Wrap(parameter.scoreboard));
                else if (parameter.IsPPV)
                    parent.ppvs.Add(parameter.ppvName);
            }

            return new FunctionStructure(function.name, returnType, variables.ToArray());
        }
        public string ToJSON()
        {
            if(returnType == null)
                return $@"{{ ""name"": ""{name}"", ""arguments"": [{VariableStructure.Join(this.args)}], ""return"": null }}";
            else
                return $@"{{ ""name"": ""{name}"", ""arguments"": [{VariableStructure.Join(this.args)}], ""return"": ""{returnType}"" }}";
        }
    }
    public struct VariableStructure
    {
        public const string TYPE_INT = "int";
        public const string TYPE_DECIMAL = "decimal";
        public const string TYPE_BOOL = "bool";
        public const string TYPE_TIME = "time";
        public const string TYPE_STRUCT = "struct";

        public readonly string name;
        public readonly string type;
        public int precision;
        public string structName;

        private VariableStructure(string name, string type, int precision, string structName)
        {
            this.name = name;
            this.type = type;
            this.precision = precision;
            this.structName = structName;
        }
        public static VariableStructure Wrap(ScoreboardValue value)
        {
            VariableStructure structure = new VariableStructure(value.AliasName, value.GetTypeKeyword(), 0, null);

            if (value is ScoreboardValueDecimal)
                structure.precision = (value as ScoreboardValueDecimal).precision;
            if (value is ScoreboardValueStruct)
                structure.structName = (value as ScoreboardValueStruct).structure.name;

            return structure;
        }
        public static VariableStructure Wrap(FunctionParameter parameter)
        {
            if (!parameter.IsScoreboard)
                throw new Exception("Attempted to wrap non-scoreboard FunctionParameter into a VariableStructure during linting.");

            ScoreboardValue value = parameter.scoreboard;
            VariableStructure structure = new VariableStructure(value.AliasName, value.GetTypeKeyword(), 0, null);

            if (value is ScoreboardValueDecimal)
                structure.precision = (value as ScoreboardValueDecimal).precision;
            if (value is ScoreboardValueStruct)
                structure.structName = (value as ScoreboardValueStruct).structure.name;

            return structure;
        }
        public string ToJSON()
        {
            if (type.Equals(TYPE_DECIMAL))
                return $@"{{ ""name"": ""{name}"", ""type"": ""decimal"", ""precision"": {precision} }}";
            if (type.Equals(TYPE_STRUCT))
                return $@"{{ ""name"": ""{name}"", ""type"": ""struct"", ""structName"": ""{structName}"" }}";

            return $@"{{ ""name"": ""{name}"", ""type"": ""{type}"" }}";
        }
        public static string Join(List<VariableStructure> variables)
        {
            int count = variables.Count;

            if (count == 0)
                return "";
            if (count == 1)
                return variables[0].ToJSON();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < count; i++)
            {
                bool last = i == count - 1;
                sb.Append(variables[i].ToJSON());
                if (i != count - 1)
                    sb.Append(',');
            }

            return sb.ToString();
        }

        public static VariableStructure Int(string name) =>
            new VariableStructure(name, TYPE_INT, 0, null);
        public static VariableStructure Decimal(string name, int precision) =>
            new VariableStructure(name, TYPE_DECIMAL, precision, null);
        public static VariableStructure Bool(string name) =>
            new VariableStructure(name, TYPE_BOOL, 0, null);
        public static VariableStructure Time(string name) =>
            new VariableStructure(name, TYPE_BOOL, 0, null);
        public static VariableStructure Struct(string name, string structName) =>
            new VariableStructure(name, TYPE_DECIMAL, 0, structName);
    }
}
