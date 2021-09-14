﻿using mc_compiled.Commands;
using mc_compiled.Commands.Native;
using mc_compiled.Json;
using mc_compiled.MCC;
using mc_compiled.NBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    public class TokenMC : Token
    {
        string command;
        public TokenMC(string command)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.MC;

            this.command = command;
        }
        public override string ToString()
        {
            return $"Run /{command}";
        }
        public override void Execute(Executor caller)
        {
            string output = caller.ReplacePPV(command);
            caller.FinishRaw(output);
        }
    }
    public class TokenSELECT : Token
    {
        Selector.Core selectCore;

        public TokenSELECT(string selectCore)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.SELECT;

            this.selectCore = Selector.ParseCore(selectCore);
        }
        public override string ToString()
        {
            return $"Select @{selectCore}";
        }
        public override void Execute(Executor caller)
        {
            caller.selection.Peek().core = selectCore;
        }
    }
    public class TokenPRINT : Token
    {
        string text;
        public TokenPRINT(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PRINT;

            this.text = text;
        }
        public override string ToString()
        {
            return $"Print \"{text}\"";
        }
        public static readonly Regex PRINT_VALUE = new Regex("{([a-zA-Z-:._]{1,16})}");
        public static List<JSONRawTerm> TokenizePrint(string str, Executor caller)
        {
            MatchCollection matches = PRINT_VALUE.Matches(str);
            if (matches.Count < 1)
                return new List<JSONRawTerm>() { new JSONText(str) };

            List<JSONRawTerm> terms = new List<JSONRawTerm>();
            Stack<string> pieces = new Stack<string>(PRINT_VALUE.Split(str).Reverse());

            string sel = '@' + caller.selection.Peek().core.ToString();
            foreach (Match match in matches)
            {
                if (match.Index != 0 && pieces.Count > 0)
                    terms.Add(new JSONText(pieces.Pop()));
                pieces.Pop();

                string src = match.Value;
                string valueName = match.Groups[1].Value;
                if (caller.valueCache.Contains(valueName))
                    terms.Add(new JSONScore(sel, valueName));
                else terms.Add(new JSONText(src));
            }

            while (pieces.Count > 0)
                terms.Add(new JSONText(pieces.Pop()));

            return terms;
        }
        public override void Execute(Executor caller)
        {
            string output = caller.ReplacePPV(text);

            RawTextJsonBuilder builder = new RawTextJsonBuilder();
            List<JSONRawTerm> terms = TokenizePrint(output, caller);
            foreach (JSONRawTerm term in terms)
                builder.AddTerm(term);
            string final = builder.BuildString();

            caller.FinishRaw($"tellraw @a {final}");
        }
    }
    public class TokenPRINTP : Token
    {
        string text;
        public TokenPRINTP(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PRINTP;

            this.text = text;
        }
        public override string ToString()
        {
            return $"Print to Selected \"{text}\"";
        }
        public override void Execute(Executor caller)
        {
            string output = caller.ReplacePPV(text);

            RawTextJsonBuilder builder = new RawTextJsonBuilder();
            List<JSONRawTerm> terms = TokenPRINT.TokenizePrint(output, caller);
            foreach (JSONRawTerm term in terms)
                builder.AddTerm(term);
            string final = builder.BuildString();
            string selector = "@" + caller.SelectionReference;

            caller.FinishRaw($"tellraw {selector} {final}");
        }
    }
    public class TokenLIMIT : Token
    {
        string limit;
        bool none;
        public TokenLIMIT(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.LIMIT;

            limit = text;
            none = text.ToUpper().Equals("NONE");
        }
        public override string ToString()
        {
            return none ? "Limit any count" : $"Limit {limit}";
        }
        public override void Execute(Executor caller)
        {
            string output = caller.ReplacePPV(limit);

            try
            {
                var selector = caller.selection.Peek();
                if (none)
                {
                    selector.count.count = Commands.Limits.Count.NONE;
                } else
                {
                    int parsed = int.Parse(output);
                    if (parsed < 1)
                        parsed = Commands.Limits.Count.NONE;
                    selector.count.count = parsed;
                }

            } catch (FormatException)
            {
                throw new TokenException(this, $"Cannot set limit to \"{output}\", needs an integer.");
            }

            return;
        }
    }
    public class TokenDEFINE : Token
    {
        public readonly string valueName;
        public TokenDEFINE(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.DEFINE;

            valueName = text;
        }
        public override string ToString()
        {
            return $"Define value {valueName}";
        }
        public override void Execute(Executor caller)
        {
            string output = caller.ReplacePPV(valueName);

            if (output.Length > CommandLimits.SCOREBOARD_LIMIT)
                throw new TokenException(this, $"Value name \"{output}\" is too long ({CommandLimits.SCOREBOARD_LIMIT} maximum)");
            if (output.Any(c => !CommandLimits.SCOREBOARD_ALLOWED.Contains(c)))
                throw new TokenException(this, $"Disallowed characters in value name \"{output}\"");

            caller.valueCache.Add(output);
            caller.FinishRaw($"scoreboard objectives add \"{output}\" dummy");
            return;
        }
    }
    public class TokenINITIALIZE : Token
    {
        string valueName;
        public TokenINITIALIZE(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.INITIALIZE;

            valueName = text;
        }
        public override string ToString()
        {
            return $"Initialize value {valueName}";
        }
        public override void Execute(Executor caller)
        {
            string output = caller.ReplacePPV(valueName);

            if (!caller.valueCache.Contains(output))
                throw new TokenException(this, $"No value exists with the name \"{output}\"");

            caller.FinishRaw($"scoreboard players add @a \"{output}\" 0");
            return;
        }
    }
    public class TokenVALUE : Token
    {
        enum OPERATION : byte
        {
            ADD = 0,
            SUB = 2,
            MUL = 3,
            SET = 4,
            DIV = 5,
            MOD = 7,
        }
        static string OperationString(OPERATION op)
        {
            switch (op)
            {
                case OPERATION.ADD:
                    return "+=";
                case OPERATION.SUB:
                    return "-=";
                case OPERATION.MUL:
                    return "*=";
                case OPERATION.DIV:
                    return "/=";
                case OPERATION.MOD:
                    return "%=";
                case OPERATION.SET:
                    return "=";
                default:
                    return null;
            }
        }

        string valueName;
        OPERATION operation;

        bool bIsConstant; // Decides if you should use valueB or constantB
        bool bIsPPV;      // Decides if should still evaluate valueB
        string valueB;
        long constantB;

        public TokenVALUE(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.VALUE;

            string[] parts = text.Split(' ');
            if (parts.Length < 3)
                throw new TokenException(this, "Not enough arguments specified.");

            valueName = parts[0];
            string operationString = parts[1].ToUpper();
            string bString = parts[2];

            switch (operationString)
            {
                case "ADD":
                    operation = OPERATION.ADD;
                    break;
                case "SUB":
                    operation = OPERATION.SUB;
                    break;
                case "MUL":
                    operation = OPERATION.MUL;
                    break;
                case "DIV":
                    operation = OPERATION.DIV;
                    break;
                case "MOD":
                    operation = OPERATION.MOD;
                    break;
                case "SET":
                    operation = OPERATION.SET;
                    break;
                case "+=":
                    operation = OPERATION.ADD;
                    break;
                case "-=":
                    operation = OPERATION.SUB;
                    break;
                case "*=":
                    operation = OPERATION.MUL;
                    break;
                case "/=":
                    operation = OPERATION.DIV;
                    break;
                case "%=":
                    operation = OPERATION.MOD;
                    break;
                case "=":
                    operation = OPERATION.SET;
                    break;
                default:
                    throw new TokenException(this, $"Invalid value operation {operationString}");
            }

            if (Compiler.guessedPPValues.Contains(bString))
            {
                bIsPPV = true;
                bIsConstant = true;
                valueB = bString;
                return;
            }

            if ((bIsConstant = int.TryParse(bString, out int parsed)))
                constantB = parsed;
            else
                valueB = bString;
        }
        public override string ToString()
        {
            return $"Perform value operation {valueName} {OperationString(operation)} {(bIsConstant ? constantB.ToString() : valueB)}";
        }
        public override void Execute(Executor caller)
        {
            string sourceValue = caller.ReplacePPV(valueName);
            string secondValue = bIsConstant ? null : caller.ReplacePPV(valueB);
            string selector = '@' + caller.selection.Peek().core.ToString();

            if (!caller.valueCache.Contains(sourceValue))
                throw new TokenException(this, $"No value exists with the name \"{sourceValue}\"");
            if (secondValue != null && !caller.valueCache.Contains(secondValue) && !bIsPPV)
                throw new TokenException(this, $"No value exists with the name \"{secondValue}\"");

            // Create temp objective for more complex operations
            bool needsTemp = (((byte)operation) % 2 == 1) && bIsConstant;
            if (needsTemp && !caller.hasCreatedMath)
            {
                caller.AddLineTop($"scoreboard objectives add {Executor.NAME_ARITHMETIC} dummy");
                caller.hasCreatedMath = true;
            }

            if (bIsConstant)
            {
                if (bIsPPV) // resolve PPV value
                    constantB = caller.ppv[valueB].data.i;

                switch (operation)
                {
                    case OPERATION.ADD:
                        caller.FinishRaw($"scoreboard players add {selector} {sourceValue} {constantB}");
                        return;
                    case OPERATION.SUB:
                        if (constantB < 0)
                            caller.FinishRaw($"scoreboard players add {selector} {sourceValue} {constantB.ToString().Substring(1)}");
                        else caller.FinishRaw($"scoreboard players add {selector} {sourceValue} -{constantB}");
                        return;
                    case OPERATION.MUL:
                        caller.FinishRaw($"scoreboard players set {selector} {Executor.NAME_ARITHMETIC} {constantB}", false);
                        caller.FinishRaw($"scoreboard players operation {selector} {sourceValue} *= {selector} {Executor.NAME_ARITHMETIC}");
                        return;
                    case OPERATION.SET:
                        caller.FinishRaw($"scoreboard players set {selector} {sourceValue} {constantB}");
                        return;
                    case OPERATION.DIV:
                        caller.FinishRaw($"scoreboard players set {selector} {Executor.NAME_ARITHMETIC} {constantB}", false);
                        caller.FinishRaw($"scoreboard players operation {selector} {sourceValue} /= {selector} {Executor.NAME_ARITHMETIC}");
                        return;
                    case OPERATION.MOD:
                        caller.FinishRaw($"scoreboard players set {selector} {Executor.NAME_ARITHMETIC} {constantB}", false);
                        caller.FinishRaw($"scoreboard players operation {selector} {sourceValue} %= {selector} {Executor.NAME_ARITHMETIC}");
                        return;
                    default:
                        break;
                }
            }
            else
            {
                switch (operation)
                {
                    case OPERATION.ADD:
                        caller.FinishRaw($"scoreboard players operation {selector} {sourceValue} += {selector} {secondValue}");
                        break;
                    case OPERATION.SUB:
                        caller.FinishRaw($"scoreboard players operation {selector} {sourceValue} -= {selector} {secondValue}");
                        break;
                    case OPERATION.MUL:
                        caller.FinishRaw($"scoreboard players operation {selector} {sourceValue} *= {selector} {secondValue}");
                        break;
                    case OPERATION.SET:
                        caller.FinishRaw($"scoreboard players operation {selector} {sourceValue} = {selector} {secondValue}");
                        break;
                    case OPERATION.DIV:
                        caller.FinishRaw($"scoreboard players operation {selector} {sourceValue} /= {selector} {secondValue}");
                        break;
                    case OPERATION.MOD:
                        caller.FinishRaw($"scoreboard players operation {selector} {sourceValue} %= {selector} {secondValue}");
                        break;
                    default:
                        break;
                }
            }

            return;
        }
    }
    public class TokenIF : Token
    {
        enum Type : int
        {
            none,

            VALUE,
            BLOCK,
            TYPE,
            FAMILY,
            TAG,
            GAMEMODE,
            NEAR,
            IN,
            LEVEL,
            NAME
        }
        struct IFStatement
        {
            public int MinimumArgCount
            {
                get
                {
                    switch (type)
                    {
                        case Type.none:
                            return 0;
                        case Type.VALUE:
                            return 3;
                        case Type.BLOCK:
                            return 4;
                        case Type.TYPE:
                            return 1;
                        case Type.FAMILY:
                            return 1;
                        case Type.TAG:
                            return 1;
                        case Type.GAMEMODE:
                            return 1;
                        case Type.NEAR:
                            return 4;
                        case Type.IN:
                            return 6;
                        case Type.LEVEL:
                            return 2;
                        case Type.NAME:
                            return 1;
                        default:
                            return 0;
                    }
                }
            }

            public bool not;
            public Type type;
            public string[] args;

            public void Evaluate(ref Selector selector, ref Executor context)
            {
                int currentScope = context.selection.Count - 1;
                switch (type)
                {
                    case Type.none:
                        return;
                    case Type.VALUE:
                        string valueName = context.ReplacePPV(args[0]);
                        Operator op = Operator.Parse(args[1]);
                        string checkValue = context.ReplacePPV(args[2]);
                        if (int.TryParse(checkValue, out int otherInt))
                        {
                            OperatorType type = op.type;
                            if (not) // this would be bad code but make it work anyways...
                                switch (type)
                                {
                                    case OperatorType._UNKNOWN:
                                    case OperatorType.EQUAL:
                                    case OperatorType.NOT_EQUAL:
                                        break;
                                    case OperatorType.LESS_THAN:
                                        type = OperatorType.GREATER_OR_EQUAL;
                                        break;
                                    case OperatorType.LESS_OR_EQUAL:
                                        type = OperatorType.GREATER_THAN;
                                        break;
                                    case OperatorType.GREATER_THAN:
                                        type = OperatorType.LESS_OR_EQUAL;
                                        break;
                                    case OperatorType.GREATER_OR_EQUAL:
                                        type = OperatorType.LESS_THAN;
                                        break;
                                    default:
                                        break;
                                }
                            Range range;
                            if (type == OperatorType.EQUAL || type == OperatorType.NOT_EQUAL)
                                range = new Range(otherInt, not);
                            else if (type == OperatorType.LESS_THAN)
                                range = new Range(null, otherInt - 1);
                            else if (type == OperatorType.LESS_OR_EQUAL)
                                range = new Range(null, otherInt);
                            else if (type == OperatorType.GREATER_THAN)
                                range = new Range(otherInt + 1, null);
                            else if (type == OperatorType.GREATER_OR_EQUAL)
                                range = new Range(otherInt, null);
                            else range = new Range(otherInt, false);
                            selector.scores.checks.Add(new Commands.Limits.ScoresEntry(valueName, range));
                        }
                        return;
                    case Type.BLOCK:
                        string x = args[0], y = args[1], z = args[2];
                        string block = args[3];
                        string data = "0";
                        if (args.Length > 4)
                            data = args[4];
                        BlockCheck blockCheck = new BlockCheck(x, y, z, block, data);
                        if (not) // perform block inversion
                        {
                            string inverterName = Executor.NAME_INVERTER + currentScope;
                            if (!context.hasCreatedInv[currentScope])
                            {
                                context.hasCreatedInv[currentScope] = true;
                                context.AddLineTop($"scoreboard objectives add {inverterName} dummy");
                            }
                            context.AddLineTop($"scoreboard players set {selector.core} {inverterName} 0");
                            context.FinishRaw(blockCheck.AsStoreIn(inverterName), false); // will set to 1 if found
                            selector.blockCheck = BlockCheck.DISABLED;
                            selector.scores.checks.Add(new Commands.Limits.ScoresEntry(inverterName, new Range(0, false)));
                        }
                        else
                            selector.blockCheck = blockCheck;
                        return;
                    case Type.TYPE:
                        selector.entity.type = (not ? "!" : "") + args[0];
                        return;
                    case Type.FAMILY:
                        selector.entity.family = (not ? "!" : "") + args[0];
                        return;
                    case Type.TAG:
                        selector.tags.Add(new Commands.Limits.Tag(args[0], not));
                        return;
                    case Type.GAMEMODE:
                        selector.player.ParseGamemode(args[0], not);
                        return;
                    case Type.NEAR:
                        if (not)
                            throw new NotSupportedException("NEAR if-statments cannot be inverted.");
                        string _x = args[0];
                        string _y = args[1];
                        string _z = args[2];
                        string _radius = args[3];
                        string _rmin = args.Length > 4 ? args[4] : null;
                        selector.area.volumeX = null;
                        selector.area.volumeY = null;
                        selector.area.volumeZ = null;
                        selector.area.x = CoordinateValue.Parse(_x);
                        selector.area.y = CoordinateValue.Parse(_y);
                        selector.area.z = CoordinateValue.Parse(_z);
                        selector.area.radiusMax = int.Parse(_radius);
                        if (_rmin == null)
                            selector.area.radiusMin = null;
                        else selector.area.radiusMin = int.Parse(_rmin);
                        return;
                    case Type.IN:
                        if (not)
                            throw new NotSupportedException("IN if-statments cannot be inverted.");
                        string _sizeX = args[0];
                        string _sizeY = args[1];
                        string _sizeZ = args[2];
                        _x = args.Length > 3 ? args[3] : null;
                        _y = args.Length > 4 ? args[4] : null;
                        _z = args.Length > 5 ? args[5] : null;
                        selector.area.radiusMax = null;
                        selector.area.radiusMin = null;
                        selector.area.volumeX = int.Parse(_sizeX);
                        selector.area.volumeY = int.Parse(_sizeY);
                        selector.area.volumeZ = int.Parse(_sizeZ);
                        selector.area.x = CoordinateValue.Parse(_x);
                        selector.area.y = CoordinateValue.Parse(_y);
                        selector.area.z = CoordinateValue.Parse(_z);
                        return;
                    case Type.LEVEL:
                        if (not)
                            throw new NotSupportedException("LEVEL if-statments cannot be inverted.");
                        string _level = args[0];
                        string _lMax = args.Length > 1 ? args[1] : null;
                        selector.player.levelMin = int.Parse(_level);
                        if (_lMax == null)
                            selector.player.levelMax = null;
                        else selector.player.levelMax = int.Parse(_lMax);
                        return;
                    case Type.NAME:
                        selector.entity.name = (not ? "!" : "") + string.Join(" ", args);
                        return;
                    default:
                        throw new MissingFieldException("Invalid if-statement criteria");
                }
            }
        }

        public string eval;
        public bool forceInvert = false;
        List<IFStatement> statements = new List<IFStatement>();
        void ParseStatement(string statement)
        {
            string[] args = statement.Split(' ');
            if (args.Length < 1)
                return;
            IFStatement current = new IFStatement();
            string _type = args[0].ToUpper();
            if (_type.Equals("NOT"))
            {
                current.not = true;
                _type = args[1].ToUpper();
            }
            switch (_type)
            {
                case "BLOCK":
                    current.type = Type.BLOCK;
                    break;
                case "TYPE":
                    current.type = Type.TYPE;
                    break;
                case "FAMILY":
                    current.type = Type.FAMILY;
                    break;
                case "TAG":
                    current.type = Type.TAG;
                    break;
                case "MODE":
                    current.type = Type.GAMEMODE;
                    break;
                case "NEAR":
                    current.type = Type.NEAR;
                    break;
                case "IN":
                    current.type = Type.IN;
                    break;
                case "LEVEL":
                    current.type = Type.LEVEL;
                    break;
                case "NAME":
                    current.type = Type.NAME;
                    break;
                default:
                    current.type = Type.VALUE;
                    break;
            }

            int minArgCount = current.MinimumArgCount;
            int removed = current.not ? 1 : 0;
            if (args.Length - removed < minArgCount)
                throw new TokenException(this, $"Too little arguments for if statement type {current.type}");

            int sel = 0;
            current.args = new string[args.Length - removed];
            for (int i = removed; i < args.Length; i++)
                current.args[sel++] = args[i];

            statements.Add(current);
        }
        public TokenIF(string expression)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.IF;

            eval = expression;
            string[] _statements = expression.Split('&')
                .Select(s => s.Trim()).ToArray();
            foreach (string s in _statements)
                ParseStatement(s);
        }
        public override string ToString()
        {
            return $"If {eval}:";
        }
        public override void Execute(Executor caller)
        {
            Selector next = caller.PushSelectionStack();
            for (int i = 0; i < statements.Count; i++)
            {
                IFStatement statement = statements[i];
                IFStatement clone = new IFStatement()
                {
                    args = statement.args,
                    type = statement.type,
                    not = forceInvert ? !statement.not : statement.not
                };

                // control NSE throws
                try
                {
                    clone.Evaluate(ref next, ref caller);
                } catch (NotSupportedException nse)
                {
                    if (forceInvert)
                        continue;
                    throw nse;
                } catch (Exception e)
                {
                    throw e;
                }
            }
            return;
        }
    }
    public class TokenELSE : Token
    {
        public TokenELSE()
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.ELSE;
        }
        public override string ToString()
        {
            return $"Otherwise:";
        }
        public override void Execute(Executor caller)
        {
            return;
        }
    }
    public class TokenGIVE : Token
    {
        string preview;

        string item;
        string count = "1";
        string damage = "0";

        bool keepOnDeath;
        bool lockInventory;
        bool lockSlot;
        List<string> canPlaceOn = new List<string>();
        List<string> canDestroy = new List<string>();
        List<Enchantment> enchants = new List<Enchantment>();
        string displayName;

        bool structureExists;
        bool useStructure;  // If a structure needs to be loaded
        public TokenGIVE(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.GIVE;

            preview = text;
            string[] args = Compiler.GetArguments(text);

            if (args.Length < 1)
                throw new TokenException(this, "Give statement needs an item to give.");

            item = args[0];

            // Check if descriptor item

            if (args.Length > 1)
                count = args[1];
            if (args.Length > 2)
                damage = args[2];

            for (int i = 3; i < args.Length; i++)
            {
                string str = args[i].ToUpper();
                if (str.Equals("KEEP"))
                    keepOnDeath = true;
                else if (str.Equals("LOCKINVENTORY"))
                    lockInventory = true;
                else if (str.Equals("LOCKSLOT"))
                    lockSlot = true;
                else if(str.Equals("CANPLACEON"))
                {
                    if (i + 1 >= args.Length)
                        throw new TokenException(this, "No block specified for CanPlaceOn.");
                    string block = args[++i];
                    canPlaceOn.Add(block);
                } else if (str.Equals("CANDESTROY"))
                {
                    if (i + 1 >= args.Length)
                        throw new TokenException(this, "No block specified for CanDestroy.");
                    string block = args[++i];
                    canDestroy.Add(block);
                } else if (str.Equals("ENCHANT"))
                {
                    if (i + 2 >= args.Length)
                        throw new TokenException(this, "Enchant modifier needs type and level.");

                    string enchantment = args[++i];
                    string level = args[++i];

                    enchants.Add(new Enchantment(enchantment, level));
                    useStructure = true;

                } else if (str.Equals("NAME"))
                {
                    if (i + 1 >= args.Length)
                        throw new TokenException(this, "No name specified for Item Name.");
                    displayName = args[++i];
                    useStructure = true;
                }
            }
        }
        public override string ToString()
        {
            return $"Give {preview}";
        }
        public override void Execute(Executor caller)
        {
            if(useStructure)
            {
                // Generate structure
                ItemStack item = new ItemStack()
                {
                    id = caller.ReplacePPV(this.item),
                    count = int.Parse(caller.ReplacePPV(count)),
                    damage = int.Parse(caller.ReplacePPV(damage)),

                    keep = keepOnDeath,
                    lockMode = lockInventory ? ItemLockMode.LOCK_IN_INVENTORY :
                        lockSlot ? ItemLockMode.LOCK_IN_SLOT : ItemLockMode.NONE,
                    displayName = displayName,
                    enchantments = enchants.Select(e => e.Resolve(caller)).ToArray(),
                    canDestroy = canDestroy.ToArray(),
                    canPlaceOn = canPlaceOn.ToArray()
                };
                var tuple = new Tuple<string, ItemStack>(item.GenerateUID(), item);
                string exportedName = tuple.Item1;
                caller.itemsToBeWritten.Add(tuple);

                Selector selector = caller.selection.Peek();
                bool aligned = caller.TargetPositionAligned;
                string last = $"structure load {exportedName} ~~~ 0_degrees none true false";
                if(aligned)
                    caller.FinishRaw(last);
                else
                    caller.FinishRaw($"execute @{caller.SelectionReference} ~~~ {last}");
                return;
            }
            List<string> parts = new List<string>();
            List<string> json = new List<string>();

            parts.Add("give");
            parts.Add("@" + caller.SelectionReference);
            parts.Add(caller.ReplacePPV(item));
            parts.Add(caller.ReplacePPV(count));
            parts.Add(caller.ReplacePPV(damage));

            if (keepOnDeath)
                json.Add("\"keep_on_death\":{}");
            if (lockInventory)
                json.Add("\"item_lock\":{\"mode\":\"lock_in_inventory\"}");
            if (lockSlot)
                json.Add("\"item_lock\":{\"mode\":\"lock_in_slot\"}");

            if(canPlaceOn != null && canPlaceOn.Count > 0)
            {
                string blocks = string.Join(",", canPlaceOn.Select(c => $"\"{caller.ReplacePPV(c)}\""));
                json.Add($"\"minecraft:can_place_on\":{{\"blocks\":[{blocks}]}}");
            }
            if (canDestroy != null && canDestroy.Count > 0)
            {
                string blocks = string.Join(",", canDestroy.Select(c => $"\"{caller.ReplacePPV(c)}\""));
                json.Add($"\"minecraft:can_destroy\":{{\"blocks\":[{blocks}]}}");
            }

            string command = string.Join(" ", parts);
            if (json.Count > 0)
                command += $" {{{string.Join(",", json)}}}";
            caller.FinishRaw(command);
        }
    }
}
