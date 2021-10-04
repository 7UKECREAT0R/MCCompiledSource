using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    public class TokenUnknown : Token
    {
        public TokenUnknown()
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.UNKNOWN;
        }
        public override string ToString()
        {
            return "SERIOUS PROBLEM";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            return;
        }
    }
    public class TokenComment : Token
    {
        public readonly string comment;
        public TokenComment(string comment)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.COMMENT;
            this.comment = comment;
        }
        public override string ToString()
        {
            return "COMMENT: " + comment;
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            return;
        }
    }
    public class TokenBlock : Token
    {
        public readonly TokenFeeder contents;
        public TokenBlock(Token[] contents)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.BLOCK;
            this.contents = new TokenFeeder(contents);
        }
        public override string ToString()
        {
            return "Block with " + contents.length + " elements in it.";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            contents.Reset();
            caller.RunSection(contents);
        }
    }

    public class TokenPPV : Token
    {
        public string name;
        public Dynamic value;

        public TokenPPV(string expression)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPV;

            int index = expression.IndexOf(' ');
            name = expression.Substring(0, index);
            value = Dynamic.Parse(expression.Substring(index + 1));
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if (value.type == Dynamic.Type.STRING && caller.TryGetPPV(value.data.s, out Dynamic source))
                caller.ppv[name] = source;
            else
                caller.ppv[name] = value;
        }
        public override string ToString()
        {
            return $"[PP] Set variable {name} to {value}";
        }
    }
    public class TokenPPINC : Token
    {
        public string varName;

        public TokenPPINC(string varName)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPINC;
            this.varName = varName;
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if(caller.TryGetPPV(varName, out Dynamic value))
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
    public class TokenPPDEC : Token
    {
        public string varName;

        public TokenPPDEC(string varName)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPDEC;
            this.varName = varName;
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out Dynamic value))
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
    public class TokenPPADD : Token
    {
        public string varName;

        public bool usePPV = false;
        public Dynamic constant;
        public string ppv;

        public TokenPPADD(string expression)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPADD;

            int index = expression.IndexOf(' ');
            varName = expression.Substring(0, index);
            constant = Dynamic.Parse(expression.Substring(index + 1));

            if (constant.type == Dynamic.Type.STRING)
            {
                ppv = constant.data.s;
                usePPV = true;
            }
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out Dynamic value))
            {
                Dynamic constant = this.constant;
                if(usePPV && caller.TryGetPPV(ppv, out Dynamic other))
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
    public class TokenPPSUB : Token
    {
        public string varName;

        public bool usePPV = false;
        public Dynamic constant;
        public string ppv;

        public TokenPPSUB(string expression)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPSUB;

            int index = expression.IndexOf(' ');
            varName = expression.Substring(0, index);
            constant = Dynamic.Parse(expression.Substring(index + 1));

            if (constant.type == Dynamic.Type.STRING)
            {
                ppv = constant.data.s;
                usePPV = true;
            }
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out Dynamic value))
            {
                Dynamic constant = this.constant;
                if (usePPV && caller.TryGetPPV(ppv, out Dynamic other))
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
    public class TokenPPMUL : Token
    {
        public string varName;

        public bool usePPV = false;
        public Dynamic constant;
        public string ppv;

        public TokenPPMUL(string expression)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPMUL;

            int index = expression.IndexOf(' ');
            varName = expression.Substring(0, index);
            constant = Dynamic.Parse(expression.Substring(index + 1));

            if (constant.type == Dynamic.Type.STRING)
            {
                ppv = constant.data.s;
                usePPV = true;
            }
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out Dynamic value))
            {
                Dynamic constant = this.constant;
                if (usePPV && caller.TryGetPPV(ppv, out Dynamic other))
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
    public class TokenPPDIV : Token
    {
        public string varName;

        public bool usePPV = false;
        public Dynamic constant;
        public string ppv;

        public TokenPPDIV(string expression)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPDIV;

            int index = expression.IndexOf(' ');
            varName = expression.Substring(0, index);
            constant = Dynamic.Parse(expression.Substring(index + 1));

            if (constant.type == Dynamic.Type.STRING)
            {
                ppv = constant.data.s;
                usePPV = true;
            }
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out Dynamic value))
            {
                Dynamic constant = this.constant;
                if (usePPV && caller.TryGetPPV(ppv, out Dynamic other))
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
    public class TokenPPMOD : Token
    {
        public string varName;

        public bool usePPV = false;
        public Dynamic constant;
        public string ppv;

        public TokenPPMOD(string expression)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPMOD;

            int index = expression.IndexOf(' ');
            varName = expression.Substring(0, index);
            constant = Dynamic.Parse(expression.Substring(index + 1));

            if (constant.type == Dynamic.Type.STRING)
            {
                ppv = constant.data.s;
                usePPV = true;
            }
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if (caller.TryGetPPV(varName, out Dynamic value))
            {
                Dynamic temp = constant;
                if (temp.type == Dynamic.Type.STRING &&
                caller.TryGetPPV(temp.data.s, out Dynamic other))
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
    public class TokenPPIF : Token
    {
        public Dynamic constantA, constantB;
        public Operator comparison;

        public TokenPPIF(string expression)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPIF;

            string[] parts = expression.Split(' ');

            if(parts.Length < 3)
                throw new TokenException(this, "Incomplete PPIF statement.");

            constantA = Dynamic.Parse(parts[0]);
            comparison = Operator.Parse(parts[1]);
            constantB = Dynamic.Parse(parts[2]);

            if(comparison == null)
                throw new TokenException(this, $"Invalid comparison operator \"{parts[1]}\"");
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            Token potential = tokens.Peek();
            if(potential != null && potential is TokenBlock)
            {
                TokenBlock runBlock = potential as TokenBlock;
                
                Dynamic a = constantA;
                Dynamic b = constantB;

                if (a.type == Dynamic.Type.STRING &&
                caller.TryGetPPV(a.data.s, out Dynamic aAlt))
                    a = aAlt;

                if (b.type == Dynamic.Type.STRING &&
                caller.TryGetPPV(b.data.s, out Dynamic bAlt))
                    b = bAlt;

                if (comparison.Compare(a, b))
                    runBlock.Execute(caller, null);
                else
                {
                    // Search for PPELSE statement and block.
                    potential = tokens.Peek();
                    if (potential == null)
                        return;
                    if (!(potential is TokenPPELSE))
                        return;
                    tokens.Next();
                    potential = tokens.Peek();
                    if (potential == null || !(potential is TokenBlock))
                        throw new TokenException(this, "No block after PPELSE statement.");
                    TokenBlock elseBlock = tokens.Next() as TokenBlock;
                    elseBlock.Execute(caller, null);
                }

            } else throw new TokenException(this, "No block after PPIF statement.");
        }
        public override string ToString()
        {
            return $"[PP] If {constantA} {comparison} {constantB}:";
        }
    }
    public class TokenPPELSE : Token
    {
        public TokenPPELSE()
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPELSE;
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            return;
        }
        public override string ToString()
        {
            return $"[PP] Otherwise:";
        }
    }
    public class TokenPPREP : Token
    {
        readonly string amount;

        public TokenPPREP(string amount)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPREP;

            this.amount = amount;
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            int count = 0;

            if (caller.TryGetPPV(amount, out Dynamic value))
            {
                if (value.type == Dynamic.Type.STRING)
                    throw new TokenException(this, $"PPREP value cannot be a string. It must be a whole number. Variable \"{amount}\" with value {value.data.s}");
                count = (int)value.data.i;
            }
            else if (!int.TryParse(amount, out count))
                throw new TokenException(this, $"PPREP input couldn't be parsed. \"{amount}\"");

            Token potentialBlock = tokens.Peek();
            if(potentialBlock != null && potentialBlock is TokenBlock)
            {
                TokenBlock block = tokens.Next() as TokenBlock;
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
    public class TokenPPLOG : Token
    {
        readonly string text;

        public TokenPPLOG(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPLOG;

            this.text = text;
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string temp = caller.ReplacePPV(text);
            Console.WriteLine("[LOG] {0}", temp);
        }
        public override string ToString()
        {
            return $"[PP] Log '{text}'";
        }
    }
    public class TokenPPFILE : Token
    {
        readonly string fileOffset;

        public TokenPPFILE(string fileOffset)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.UNKNOWN; // TODO

            this.fileOffset = fileOffset;
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string temp = caller.ReplacePPV(fileOffset);
            caller.NewFileOffset(temp);
        }
        public override string ToString()
        {
            return $"[PP] Set file offset to '{fileOffset}'";
        }
    }
    public class TokenFUNCTION : Token
    {
        readonly string signature;

        public TokenFUNCTION(string signature)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.FUNCTION; // TODO

            this.signature = signature;
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string temp = caller.ReplacePPV(signature);
            FunctionDefinition definition = FunctionDefinition.Parse(temp);

        }
        public override string ToString()
        {
            return $"[PP] Define Function: {signature}";
        }
    }
    public class TokenCALL : Token
    {
        readonly string name;
        readonly string args;

        public TokenCALL(string signature)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.FUNCTION; // TODO

            int space = signature.IndexOf(' ');
            if (space == -1)
                throw new TokenException(this, "No function specified to call.");
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string temp = caller.ReplacePPV(signature);
            FunctionDefinition definition = FunctionDefinition.Parse(temp);

        }
        public override string ToString()
        {
            return $"[PP] Define Function: {signature}";
        }
    }
    public class TokenPPMACRO : Token
    {
        public readonly string name;
        public readonly string[] args;

        public TokenPPMACRO(string expression)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPMACRO;

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
                args = Compiler.GetArguments(temp);
                foreach (string arg in args)
                    Compiler.guessedPPValues.Add(arg);
            }
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            // Macro definition case.
            Token potentialBlock = tokens.Peek();
            if(potentialBlock != null && potentialBlock is TokenBlock)
            {
                TokenBlock block = tokens.Next() as TokenBlock;
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
                Dictionary<string, Dynamic> existedBefore = new Dictionary<string, Dynamic>();
                List<string> deleteVars = new List<string>();
                foreach (string overwrite in find.args)
                    if (caller.TryGetPPV(overwrite, out Dynamic add))
                        existedBefore[overwrite] = add;
                    else
                        deleteVars.Add(overwrite);

                for (int i = 0; i < find.args.Length; i++)
                {
                    string argName = find.args[i];
                    Dynamic value = Dynamic.Parse(args[i]);

                    if (value.type == Dynamic.Type.STRING && caller.TryGetPPV(value.data.s, out Dynamic otherValue))
                        value = otherValue;

                    caller.ppv[argName] = value;
                }

                int hash = find.name.GetHashCode();
                if (caller.currentMacroHash == hash && !Compiler.DISABLE_MACRO_GUARD)
                    throw new TokenException(this, "Macro cannot be recursively called. Compile with -r to disable this guard.");

                int previousHash = caller.currentMacroHash;
                caller.currentMacroHash = hash;
                caller.RunSection(new TokenFeeder(find.execute));
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
    public class TokenPPFRIENDLY : Token
    {
        readonly string variable;

        public TokenPPFRIENDLY(string variable)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPFRIENDLY;

            this.variable = variable.Trim();
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if(caller.TryGetPPV(variable, out Dynamic value))
            {
                if (value.type != Dynamic.Type.STRING)
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
    public class TokenPPUPPER : Token
    {
        readonly string variable;

        public TokenPPUPPER(string variable)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPUPPER;

            this.variable = variable.Trim();
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if (caller.TryGetPPV(variable, out Dynamic value))
            {
                if (value.type != Dynamic.Type.STRING)
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
    public class TokenPPLOWER : Token
    {
        readonly string variable;

        public TokenPPLOWER(string variable)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PPLOWER;

            this.variable = variable.Trim();
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if (caller.TryGetPPV(variable, out Dynamic value))
            {
                if (value.type != Dynamic.Type.STRING)
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