using mc_compiled.Commands;
using mc_compiled.MCC.Attributes;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.Functions.Types
{
    /// <summary>
    /// A function which generates runtime code as it is called.<br />
    /// <br />
    /// A delicate mix between compile-time and run-time features, doing as much work as possible on the front end as compared to
    /// runtime functions which are entirely performed in runtime. Generative functions generate runtime code as needed.
    /// </summary>
    internal abstract class GenerativeFunction : Function
    {
        private static readonly string VARIANTS_FOLDER = Executor.MCC_GENERATED_FOLDER + "/generated";

        private Dictionary<int, Tuple<CommandFile, ScoreboardValue>> createdVariants;
        private Dictionary<int, int> tempValues;
        protected readonly List<FunctionParameter> parameters;

        public readonly string aliasedName; // user-facing name (keyword)
        public string name;                 // name used internally if the normal name won't work.
        public string returns;              // what this function says it returns, use 'T' for a type that matches the inputs.
        public string documentation;        // docs

        public GenerativeFunction(string aliasedName, string name, string returns, string documentation)
        {
            this.aliasedName = aliasedName;
            this.name = name;
            this.documentation = documentation;
            this.returns = returns;
            this.parameters = new List<FunctionParameter>();
            this.createdVariants = new Dictionary<int, Tuple<CommandFile, ScoreboardValue>>();
            this.tempValues = new Dictionary<int, int>();
        }
        /// <summary>
        /// Generates a unique temp value name for this function.
        /// </summary>
        /// <param name="uniqueIdentifier"></param>
        /// <returns></returns>
        protected string CreateUniqueTempValueName(int uniqueIdentifier)
        {
            string result;

            if(tempValues.TryGetValue(uniqueIdentifier, out int temp))
            {
                result = $"{name}_call{uniqueIdentifier}_{temp}";
                temp += 1;
                tempValues[uniqueIdentifier] = temp;
            }
            else
            {
                result = $"{name}_call{uniqueIdentifier}_0";
                tempValues[uniqueIdentifier] = 1;
            }

            return result;
        }

        /// <summary>
        /// Generate code for the current input parameters, outputting into `output`. Everything else is handled by <see cref="GenerativeFunction"/>'s <see cref="CallFunction(List{string}, Executor, Statement)"/> implementation.
        /// </summary>
        /// <param name="output">The file to emit the generated code into.</param>
        /// <param name="uniqueIdentifier">A unique identifier representing this iteration of the code generation.</param>
        /// <param name="executor"></param>
        /// <param name="statement"></param>
        public abstract void GenerateCode(CommandFile output, int uniqueIdentifier, Executor executor, Statement statement, out ScoreboardValue resultValue);

        /// <summary>
        /// Adds a parameter to this function.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>This object for chaining.</returns>
        public GenerativeFunction AddParameter(FunctionParameter parameter)
        {
            this.parameters.Add(parameter);
            return this;
        }
        /// <summary>
        /// Adds multiple parameters to this function.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>This object for chaining.</returns>
        public GenerativeFunction AddParameters(IEnumerable<FunctionParameter> parameters)
        {
            this.parameters.AddRange(parameters);
            return this;
        }
        /// <summary>
        /// Adds multiple runtime parameters to this function.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>This object for chaining.</returns>
        public GenerativeFunction AddParameters(params FunctionParameter[] parameters)
        {
            this.parameters.AddRange(parameters);
            return this;
        }

        public override string Keyword => aliasedName ?? name;
        public override string Returns => returns;
        public override string Documentation => documentation;
        public override FunctionParameter[] Parameters => this.parameters.ToArray();
        public override int ParameterCount => this.parameters.Count;
        public override string[] Aliases => null;
        public override int Importance => 1; // more important than run-time, but less important than compile-time.
        public override bool ImplicitCall => false; // can never implicitly call these functions.

        /// <summary>
        /// Get the command needed to return the given value. If multiple commands are needed, the branch will be made and returned as a function call.
        /// </summary>
        /// <param name="value">The value to try to return.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="caller">The statement to blame for when everything explodes.</param>
        /// <returns>The commands needed to perform the return.</returns>
        public string TryReturnValue(ScoreboardValue value, Executor executor, Statement caller, out ScoreboardValue returnedTo)
        {
            ScoreboardManager sb = executor.scoreboard;
            ScoreboardValue clone = ScoreboardValue.AsReturnValue(value, caller);
            returnedTo = clone;

            if (sb.temps.DefinedTemps.Add(clone.InternalName))
            {
                sb.temps.DefinedTempsRecord.Add(clone.InternalName);
                executor.AddCommandsInit(clone.CommandsDefine());
            }

            // register return type with executor
            executor.definedReturnedTypes.Add(value.type);

            // assign
            string[] commands = clone.Assign(value, caller).ToArray();

            // branch file
            if (commands.Length > 1)
            {
                CommandFile branch = Executor.GetNextGeneratedFile($"returnFrom_{this.name}_{value.InternalName}");
                branch.Add(commands);
                executor.AddExtraFile(branch);
                return Command.Function(branch);
            }
            else if (commands.Length == 1)
                return commands[0];
            else
                throw new StatementException(caller, $"Cannot return into generated value {clone}? (Create an issue on GitHub or let me know in the Discord)");
        }
        /// <summary>
        /// Get the command needed to return the given value. If multiple commands are needed, the branch will be made and returned as a function call.
        /// </summary>
        /// <param name="parameterName">The name of the parameter being returned. Needed to name the branch file if needed.</param>
        /// <param name="value">The value to try to return.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="caller">The statement to blame for when everything explodes.</param>
        /// <returns>The commands needed to perform the return.</returns>
        public string TryReturnValue(string parameterName, TokenLiteral value, Executor executor, Statement caller, out ScoreboardValue returnedTo)
        {
            ScoreboardManager sb = executor.scoreboard;
            ScoreboardValue variable = ScoreboardValue.AsReturnValue(value, sb, caller);
            returnedTo = variable;

            if (sb.temps.DefinedTemps.Add(variable.InternalName))
            {
                sb.temps.DefinedTempsRecord.Add(variable.InternalName);
                executor.AddCommandsInit(variable.CommandsDefine());
            }

            // assign
            string[] commands = variable.AssignLiteral(value, caller).ToArray();

            // branch file
            if (commands.Length > 1)
            {
                CommandFile branch = Executor.GetNextGeneratedFile($"returnFrom_{this.name}_{parameterName}");
                branch.Add(commands);
                executor.AddExtraFile(branch);
                return Command.Function(branch);
            }
            else if (commands.Length == 1)
                return commands[0];
            else
                throw new StatementException(caller, $"Cannot return into generated value {variable}? (Create an issue on GitHub or let me know in the Discord)");
        }

        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            int signature = GetHashCode();

            if(!this.createdVariants.TryGetValue(signature, out Tuple<CommandFile, ScoreboardValue> file))
            {
                // this is a new combination of input types, thus the code should be re-generated.
                CommandFile newFile = new CommandFile(true, "call" + name + '_' + signature.ToString().Replace('-', '_'), VARIANTS_FOLDER);
                executor.AddExtraFile(newFile);

                if (Program.DECORATE)
                    newFile.AddTrace(executor.CurrentFile, statement.Lines[0]);

                // generate code as per super implementation
                GenerateCode(newFile, signature, executor, statement, out ScoreboardValue resultValue);
                file = new Tuple<CommandFile, ScoreboardValue>(newFile, resultValue);
            }

            // run the function
            commandBuffer.Add(Command.Function(file.Item1));

            // if there's nothing to 'return', send back a null literal.
            if (file.Item2 == null)
                return new TokenNullLiteral(statement.Lines[0]);

            ScoreboardValue returnHolder = executor.scoreboard.temps.RequestCopy(file.Item2, true);
            commandBuffer.AddRange(returnHolder.Assign(file.Item2, statement));
            return new TokenIdentifierValue(returnHolder.Name, returnHolder, statement.Lines[0]);
        }
        
        public override int GetHashCode()
        {
            int hash = (this.name ?? this.aliasedName).GetHashCode();

            // calculate hash by input parameter types
            foreach (FunctionParameter parameter in parameters)
            {
                if (parameter is CompiletimeFunctionParameter compiletimeFunctionParameter)
                {
                    Token token = compiletimeFunctionParameter.CurrentValue;
                    if (token is TokenIdentifierValue value)
                        hash ^= value.value.type.GetHashCode();
                }
            }

            return hash;
        }
    }
}
