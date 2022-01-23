using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    public class LegacyTokenUnknown : LegacyToken
    {
        public LegacyTokenUnknown()
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.UNKNOWN;
        }
        public override string ToString()
        {
            return "SERIOUS PROBLEM";
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            return;
        }
    }
    public class LegacyTokenComment : LegacyToken
    {
        public readonly string comment;
        public LegacyTokenComment(string comment)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.COMMENT;
            this.comment = comment;
        }
        public override string ToString()
        {
            return "COMMENT: " + comment;
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (!caller.decorate)
                return;

            LegacyToken last = tokens.PeekLast();
            if (!(last is LegacyTokenComment))
                caller.FinishRaw("", false);
            caller.FinishRaw("# " + caller.ReplacePPV(comment), false);
            return;
        }
    }
    public class LegacyTokenBlock : LegacyToken
    {
        public readonly LegacyTokenFeeder contents;
        public LegacyTokenBlock(LegacyToken[] contents)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.BLOCK;
            this.contents = new LegacyTokenFeeder(contents);
        }
        public override string ToString()
        {
            return "Block with " + contents.length + " elements in it.";
        }
        public override int GetHashCode()
        {
            return contents.GetHashCode();
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            contents.Reset();
            caller.RunSection(contents);
        }
        public void PlaceInFunction(LegacyExecutor caller, LegacyTokenFeeder tokens, LegacyFunctionDefinition function)
        {
            if (function.isNamespaced)
                caller.PushFile(function.name, function.theNamespace);
            else
                caller.PushFile(function.name);

            Execute(caller, tokens);

            caller.PopFile();
        }
    }

    public class LegacyTokenPPV : LegacyToken
    {
        public string name;
        public LegacyDynamic value;

        public LegacyTokenPPV(string expression)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPV;

            int index = expression.IndexOf(' ');
            name = expression.Substring(0, index);
            value = LegacyDynamic.Parse(expression.Substring(index + 1));
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (value.type == LegacyDynamic.Type.STRING && caller.TryGetPPV(value.data.s, out LegacyDynamic source))
                caller.ppv[name] = source;
            else
                caller.ppv[name] = value;
        }
        public override string ToString()
        {
            return $"[PP] Set variable {name} to {value}";
        }
    }
    public class LegacyTokenPPINC : LegacyToken
    {
        public string varName;

        public LegacyTokenPPINC(string varName)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPINC;
            this.varName = varName;
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if(caller.TryGetPPV(varName, out LegacyDynamic value))
            {
                value++;
                caller.ppv[varName] = value;
                return;
            } else
            {
                throw new TokenException(this, $"Variable {varName} doesn't exist.");
            }
        }
        public override string ToString()
        {
            return "[PP] Increment " + varName;
        }
    }
    public class LegacyTokenPPDEC : LegacyToken
    {
        public string varName;

        public LegacyTokenPPDEC(string varName)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPDEC;
            this.varName = varName;
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out LegacyDynamic value))
            {
                value--;
                caller.ppv[varName] = value;
                return;
            }
            else
            {
                throw new TokenException(this, $"Variable {varName} doesn't exist.");
            }
        }
        public override string ToString()
        {
            return "[PP] Decrement " + varName;
        }
    }
    public class LegacyTokenPPADD : LegacyToken
    {
        public string varName;

        public bool usePPV = false;
        public LegacyDynamic constant;
        public string ppv;

        public LegacyTokenPPADD(string expression)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPADD;

            int index = expression.IndexOf(' ');
            varName = expression.Substring(0, index);
            constant = LegacyDynamic.Parse(expression.Substring(index + 1));

            if (constant.type == LegacyDynamic.Type.STRING)
            {
                ppv = constant.data.s;
                usePPV = true;
            }
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out LegacyDynamic value))
            {
                LegacyDynamic constant = this.constant;
                if(usePPV && caller.TryGetPPV(ppv, out LegacyDynamic other))
                    constant = other;

                value += constant;
                caller.ppv[varName] = value;
            }
            else
            {
                throw new TokenException(this, $"Variable {varName} doesn't exist.");
            }
        }
        public override string ToString()
        {
            if (usePPV)
                return $"[PP] Add the value of {ppv} to {varName}";
            return $"[PP] Add {constant} to {varName}";
        }
    }
    public class LegacyTokenPPSUB : LegacyToken
    {
        public string varName;

        public bool usePPV = false;
        public LegacyDynamic constant;
        public string ppv;

        public LegacyTokenPPSUB(string expression)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPSUB;

            int index = expression.IndexOf(' ');
            varName = expression.Substring(0, index);
            constant = LegacyDynamic.Parse(expression.Substring(index + 1));

            if (constant.type == LegacyDynamic.Type.STRING)
            {
                ppv = constant.data.s;
                usePPV = true;
            }
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out LegacyDynamic value))
            {
                LegacyDynamic constant = this.constant;
                if (usePPV && caller.TryGetPPV(ppv, out LegacyDynamic other))
                    constant = other;

                value -= constant;
                caller.ppv[varName] = value;
            }
            else
            {
                throw new TokenException(this, $"Variable {varName} doesn't exist.");
            }
        }
        public override string ToString()
        {
            if (usePPV)
                return $"[PP] Subtract the value of {ppv} from {varName}";
            return $"[PP] Subtract {constant} from {varName}";
        }
    }
    public class LegacyTokenPPMUL : LegacyToken
    {
        public string varName;

        public bool usePPV = false;
        public LegacyDynamic constant;
        public string ppv;

        public LegacyTokenPPMUL(string expression)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPMUL;

            int index = expression.IndexOf(' ');
            varName = expression.Substring(0, index);
            constant = LegacyDynamic.Parse(expression.Substring(index + 1));

            if (constant.type == LegacyDynamic.Type.STRING)
            {
                ppv = constant.data.s;
                usePPV = true;
            }
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out LegacyDynamic value))
            {
                LegacyDynamic constant = this.constant;
                if (usePPV && caller.TryGetPPV(ppv, out LegacyDynamic other))
                    constant = other;

                value *= constant;
                caller.ppv[varName] = value;
            }
            else
            {
                throw new TokenException(this, $"Variable {varName} doesn't exist.");
            }
        }
        public override string ToString()
        {
            if (usePPV)
                return $"[PP] Multiply {varName} with the value of {ppv}";
            return $"[PP] Multiply {varName} with {constant}";
        }
    }
    public class LegacyTokenPPDIV : LegacyToken
    {
        public string varName;

        public bool usePPV = false;
        public LegacyDynamic constant;
        public string ppv;

        public LegacyTokenPPDIV(string expression)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPDIV;

            int index = expression.IndexOf(' ');
            varName = expression.Substring(0, index);
            constant = LegacyDynamic.Parse(expression.Substring(index + 1));

            if (constant.type == LegacyDynamic.Type.STRING)
            {
                ppv = constant.data.s;
                usePPV = true;
            }
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out LegacyDynamic value))
            {
                LegacyDynamic constant = this.constant;
                if (usePPV && caller.TryGetPPV(ppv, out LegacyDynamic other))
                    constant = other;

                value /= constant;
                caller.ppv[varName] = value;
            }
            else
            {
                throw new TokenException(this, $"Variable {varName} doesn't exist.");
            }
        }
        public override string ToString()
        {
            if (usePPV)
                return $"[PP] Divide {varName} with the value of {ppv}";
            return $"[PP] Divide {varName} with {constant}";
        }
    }
    public class LegacyTokenPPMOD : LegacyToken
    {
        public string varName;

        public bool usePPV = false;
        public LegacyDynamic constant;
        public string ppv;

        public LegacyTokenPPMOD(string expression)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPMOD;

            int index = expression.IndexOf(' ');
            varName = expression.Substring(0, index);
            constant = LegacyDynamic.Parse(expression.Substring(index + 1));

            if (constant.type == LegacyDynamic.Type.STRING)
            {
                ppv = constant.data.s;
                usePPV = true;
            }
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out LegacyDynamic value))
            {
                LegacyDynamic temp = constant;
                if (temp.type == LegacyDynamic.Type.STRING &&
                caller.TryGetPPV(temp.data.s, out LegacyDynamic other))
                    temp = other;

                value %= temp;
                caller.ppv[varName] = value;
            }
            else
            {
                throw new TokenException(this, $"Variable {varName} doesn't exist.");
            }
        }
        public override string ToString()
        {
            if (usePPV)
                return $"[PP] Modulo {varName} with {ppv}";
            return $"[PP] Modulo {varName} with {constant}";
        }
    }
    public class LegacyTokenPPIF : LegacyToken
    {
        public LegacyDynamic constantA, constantB;
        public Operator comparison;

        public LegacyTokenPPIF(string expression)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPIF;

            string[] parts = expression.Split(' ');

            if(parts.Length < 3)
                throw new TokenException(this, "Incomplete PPIF statement.");

            constantA = LegacyDynamic.Parse(parts[0]);
            comparison = Operator.Parse(parts[1]);
            constantB = LegacyDynamic.Parse(parts[2]);

            if(comparison == null)
                throw new TokenException(this, $"Invalid comparison operator \"{parts[1]}\"");
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            LegacyToken potential = tokens.Peek();
            if(potential != null && potential is LegacyTokenBlock)
            {
                LegacyTokenBlock runBlock = potential as LegacyTokenBlock;
                
                LegacyDynamic a = constantA;
                LegacyDynamic b = constantB;

                if (a.type == LegacyDynamic.Type.STRING &&
                caller.TryGetPPV(a.data.s, out LegacyDynamic aAlt))
                    a = aAlt;

                if (b.type == LegacyDynamic.Type.STRING &&
                caller.TryGetPPV(b.data.s, out LegacyDynamic bAlt))
                    b = bAlt;

                if (comparison.Compare(a, b))
                    runBlock.Execute(caller, null);
                else
                {
                    // Search for PPELSE statement and block.
                    potential = tokens.Peek();
                    if (potential == null)
                        return;
                    if (!(potential is LegacyTokenPPELSE))
                        return;
                    tokens.Next();
                    potential = tokens.Peek();
                    if (potential == null || !(potential is LegacyTokenBlock))
                        throw new TokenException(this, "No block after PPELSE statement.");
                    LegacyTokenBlock elseBlock = tokens.Next() as LegacyTokenBlock;
                    elseBlock.Execute(caller, null);
                }

            } else throw new TokenException(this, "No block after PPIF statement.");
        }
        public override string ToString()
        {
            return $"[PP] If {constantA} {comparison} {constantB}:";
        }
    }
    public class LegacyTokenPPELSE : LegacyToken
    {
        public LegacyTokenPPELSE()
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPELSE;
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            return;
        }
        public override string ToString()
        {
            return $"[PP] Otherwise:";
        }
    }
    public class LegacyTokenPPREP : LegacyToken
    {
        readonly string amount;

        public LegacyTokenPPREP(string amount)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPREP;

            this.amount = amount;
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            int count = 0;

            if (caller.TryGetPPV(amount, out LegacyDynamic value))
            {
                if (value.type == LegacyDynamic.Type.STRING)
                    throw new TokenException(this, $"PPREP value cannot be a string. It must be a whole number. Variable \"{amount}\" with value {value.data.s}");
                count = (int)value.data.i;
            }
            else if (!int.TryParse(amount, out count))
                throw new TokenException(this, $"PPREP input couldn't be parsed. \"{amount}\"");

            LegacyToken potentialBlock = tokens.Peek();
            if(potentialBlock != null && potentialBlock is LegacyTokenBlock)
            {
                LegacyTokenBlock block = tokens.Next() as LegacyTokenBlock;
                for (int r = 0; r < count; r++)
                    block.Execute(caller, null);
            } else throw new TokenException(this, "No block after PPREP statement.");
            return;
        }
        public override string ToString()
        {
            return $"[PP] Repeat {amount} times:";
        }
    }
    public class LegacyTokenPPLOG : LegacyToken
    {
        readonly string text;

        public LegacyTokenPPLOG(string text)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPLOG;

            this.text = text;
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            string temp = caller.ReplacePPV(text);
            Console.WriteLine("[LOG] {0}", temp);
        }
        public override string ToString()
        {
            return $"[PP] Log '{text}'";
        }
    }
    public class LegacyTokenPPFILE : LegacyToken
    {
        readonly string fileOffset;

        public LegacyTokenPPFILE(string fileOffset)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.UNKNOWN; // TODO

            this.fileOffset = fileOffset;
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            //string temp = caller.ReplacePPV(fileOffset);
            //caller.NewFileOffset(temp);
        }
        public override string ToString()
        {
            return $"[PP] Set file offset to '{fileOffset}'";
        }
    }
    public class LegacyTokenFUNCTION : LegacyToken
    {
        readonly string signature;

        public LegacyTokenFUNCTION(string signature)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.FUNCTION;

            this.signature = signature;
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            string temp = caller.ReplacePPV(signature);
            LegacyFunctionDefinition definition = LegacyFunctionDefinition.Parse(temp);

            if (!(tokens.Peek() is LegacyTokenBlock))
                throw new TokenException(this, "Function definition doesn't have a block after it.");
            if(caller.functionsDefined.Any(f => f.name.Equals(definition.name)))
                throw new TokenException(this, $"Function \"{definition.name}\" is already defined.");

            caller.functionsDefined.Add(definition);

            if (caller.debug)
                Console.WriteLine($"Defined Function \"{definition.FullName}\"");

            LegacyTokenBlock block = tokens.Next() as LegacyTokenBlock;
            block.PlaceInFunction(caller, tokens, definition);
        }
        public override string ToString()
        {
            return $"Define Function: {signature}";
        }
    }
    public class LegacyTokenCALL : LegacyToken
    {
        readonly string name;
        readonly string[] args;

        public LegacyTokenCALL(string signature)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.FUNCTION;

            int space = signature.IndexOf(' ');
            if (space == -1)
                throw new TokenException(this, "No function specified to call.");
            name = signature.Substring(0, space);
            args = signature.Substring(space + 1).Split(' ');
        }
        public LegacyTokenCALL(string name, string[] args)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.CALL;
            this.name = name;
            this.args = args;
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            string callName = caller.ReplacePPV(name);

            if (!caller.functionsDefined.Any(df => df.name.Equals(callName)))
                throw new TokenException(this, $"Function \"{callName}\" is not defined.");

            LegacyFunctionDefinition function = caller.functionsDefined.First(df => df.name.Equals(callName));

            // Set the input args.
            for(int i = 0; i < args.Length; i++)
            {
                // Throw out extra specified args.
                if (i >= function.args.Length)
                    break;

                string selector = "@" + caller.SelectionReference;
                string sourceValue = function.args[i];
                string inputValue = caller.ReplacePPV(args[i]);
                LegacyDynamic inputConstant = LegacyDynamic.Parse(inputValue);

                // Define the target scoreboard(s) if they haven't already.
                LegacyTokenDEFINE init = new LegacyTokenDEFINE(sourceValue);
                init.Execute(caller, tokens);

                // Check the input type to decide how to pass in this argument.
                if (inputConstant.type == LegacyDynamic.Type.STRING)
                {
                    // This is a value instead.
                    if (!caller.values.TryGetValue(inputValue, out LegacyValue value))
                        throw new TokenException(this, $"Value \"{inputValue}\" passed into function call doesn't exist.");
                    foreach (string line in LegacyValueManager.ExpressionSetValue
                        (new LegacyValue(sourceValue, inputConstant), value, selector))
                    {
                        caller.FinishRaw(line, false);
                    }
                }
                else
                {
                    foreach(string line in LegacyValueManager.ExpressionSetConstant
                        (new LegacyValue(sourceValue, inputConstant), selector, inputConstant))
                    {
                        caller.FinishRaw(line, false);
                    }
                }
            }

            // Actually call the function.
            caller.FinishRaw($"function {function.FullName}");
        }
        public override string ToString()
        {
            if (args.Length == 0 || args[0].Equals(""))
                return $"Call {name}";
            else
                return $"Call {name} with args: {string.Join(", ", args)}";
        }
    }
    public class LegacyTokenPPMACRO : LegacyToken
    {
        public readonly string name;
        public readonly string[] args;

        public LegacyTokenPPMACRO(string expression)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPMACRO;

            expression = expression.Trim();
            int space = expression.IndexOf(' ');

            if(space == -1)
            {
                name = expression;
                args = new string[0];
            } else
            {
                name = expression.Substring(0, space);
                string temp = expression.Substring(space + 1);
                args = Tokenizer.GetArguments(temp);
                foreach (string arg in args)
                    Tokenizer.guessedPPValues.Add(arg);
            }
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            // Macro definition case.
            LegacyToken potentialBlock = tokens.Peek();
            if(potentialBlock != null && potentialBlock is LegacyTokenBlock)
            {
                LegacyTokenBlock block = tokens.Next() as LegacyTokenBlock;
                Macro macro = new Macro(name, args, block.contents.GetArray());

                if (caller.debug)
                    Console.WriteLine("Defined macro '{0}' with {1} argument(s) and {2} statements inside.",
                        name, args.Length, block.contents.length);

                caller.macros.Add(name.Trim().ToUpper(), macro);
                return;
            }
            
            // Macro call case.
            if(caller.macros.TryGetValue(name.Trim().ToUpper(), out Macro find))
            {
                if (args.Length < find.args.Length)
                    throw new TokenException(this, $"Not enough arguments specified for macro {name}. " +
                        $"Needs {find.args.Length} but got {args.Length}");

                // Save previous definitions from being overwritten in a macro.
                Dictionary<string, LegacyDynamic> existedBefore = new Dictionary<string, LegacyDynamic>();
                List<string> deleteVars = new List<string>();
                foreach (string overwrite in find.args)
                    if (caller.TryGetPPV(overwrite, out LegacyDynamic add))
                        existedBefore[overwrite] = add;
                    else
                        deleteVars.Add(overwrite);

                for (int i = 0; i < find.args.Length; i++)
                {
                    string argName = find.args[i];
                    LegacyDynamic value = LegacyDynamic.Parse(args[i]);

                    if (value.type == LegacyDynamic.Type.STRING && caller.TryGetPPV(value.data.s, out LegacyDynamic otherValue))
                        value = otherValue;

                    caller.ppv[argName] = value;
                }

                int hash = find.name.GetHashCode();
                if (caller.currentMacroHash == hash)
                    throw new TokenException(this, "Macro cannot be recursively called.");

                int previousHash = caller.currentMacroHash;
                caller.currentMacroHash = hash;
                caller.RunSection(new LegacyTokenFeeder(find.execute));
                caller.currentMacroHash = previousHash;

                // Return argument-passed variables to how they were before the macro call.
                foreach(var entry in existedBefore)
                    caller.ppv[entry.Key] = entry.Value;

            } else throw new TokenException(this,
                $"No macro named \"{name}\". Make sure it's defined above this line rather than below.");
        }
        public override string ToString()
        {
            return $"[PP] Call/Define macro '{name}' with {args.Length} argument(s)";
        }
    }
    public class LegacyTokenHALT : LegacyToken
    {
        public LegacyTokenHALT()
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.HALT;
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (!caller.HasCreatedTemplate(LegacyExecutor.HALT_FUNCTION))
            {
                // Spam 10,000 /help commands.
                long count = caller.HaltFunctionCount;
                List<string> lines = new List<string>(((int)count) + 3)
                {
                    "# This file will run 10 thousand help commands, causing the file to halt.",
                    "# This will only work if functionCommandLimit is still at the default.",
                    "# If not, use 'ppv functionCommandLimit <amount>' to manually set this number."
                };
                for(int i = 0; i < count; i++)
                    lines.Add("help");

                caller.CreateTemplate(LegacyExecutor.HALT_FUNCTION, lines.ToArray(), true);
            }

            caller.FinishRaw("function " + LegacyExecutor.HALT_FUNCTION);
            return;
        }
        public override string ToString() => "Halt Execution";
    }
    public class LegacyTokenPPFRIENDLY : LegacyToken
    {
        readonly string variable;

        public LegacyTokenPPFRIENDLY(string variable)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPFRIENDLY;

            this.variable = variable.Trim();
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if(caller.TryGetPPV(variable, out LegacyDynamic value))
            {
                if (value.type != LegacyDynamic.Type.STRING)
                    throw new TokenException(this, $"Variable \"{variable}\" was not a string so it can't be converted. (IS {value.type})");
                string str = value.data.s;
                string[] parts = str.Split('_', '-', ' ');
                for(int i = 0; i < parts.Length; i++)
                {
                    char[] part = parts[i].ToCharArray();
                    for (int c = 0; c < part.Length; c++)
                        part[c] = (c == 0) ? char.ToUpper(part[c]) : char.ToLower(part[c]);
                    parts[i] = new string(part);
                }
                value.data.s = string.Join(" ", parts);
                caller.ppv[variable] = value;
            }
            else
                throw new TokenException(this, $"Variable \"{variable}\" has not been defined.");
        }
        public override string ToString()
        {
            return $"[PP] Friendly-name '{variable}'";
        }
    }
    public class LegacyTokenPPUPPER : LegacyToken
    {
        readonly string variable;

        public LegacyTokenPPUPPER(string variable)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPUPPER;

            this.variable = variable.Trim();
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (caller.TryGetPPV(variable, out LegacyDynamic value))
            {
                if (value.type != LegacyDynamic.Type.STRING)
                    throw new TokenException(this, $"Variable \"{variable}\" was not a string so it can't be converted. (IS {value.type})");
                string str = value.data.s;
                value.data.s = str.ToUpper();
                caller.ppv[variable] = value;
            }
            else
                throw new TokenException(this, $"Variable \"{variable}\" has not been defined.");
        }
        public override string ToString()
        {
            return $"[PP] Uppercase '{variable}'";
        }
    }
    public class LegacyTokenPPLOWER : LegacyToken
    {
        readonly string variable;

        public LegacyTokenPPLOWER(string variable)
        {
            line = Tokenizer.CURRENT_LINE;
            type = LEGACYTOKENTYPE.PPLOWER;

            this.variable = variable.Trim();
        }
        public override void Execute(LegacyExecutor caller, LegacyTokenFeeder tokens)
        {
            if (caller.TryGetPPV(variable, out LegacyDynamic value))
            {
                if (value.type != LegacyDynamic.Type.STRING)
                    throw new TokenException(this, $"Variable \"{variable}\" was not a string so it can't be converted. (IS {value.type})");
                string str = value.data.s;
                value.data.s = str.ToLower();
                caller.ppv[variable] = value;
            }
            else
                throw new TokenException(this, $"Variable \"{variable}\" has not been defined.");
        }
        public override string ToString()
        {
            return $"[PP] Lowercase '{variable}'";
        }
    }
}