using mc_compiled.Commands;
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
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string output = caller.ReplacePPV(command);
            caller.FinishRaw(output);
        }
    }
    public class TokenSELECT : Token
    {
        string selectCore;

        public TokenSELECT(string selectCore)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.SELECT;

            this.selectCore = selectCore;
        }
        public override string ToString()
        {
            return $"Select @{selectCore}";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            caller.selection = Selector.ParseCore(caller.ReplacePPV(selectCore));
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

            string sel = "@" + caller.SelectionReference;
            foreach (Match match in matches)
            {
                if (match.Index != 0 && pieces.Count > 0)
                    terms.Add(new JSONText(pieces.Pop()));
                pieces.Pop();

                string src = match.Value;
                string valueName = match.Groups[1].Value;
                if (caller.values.TryGetValue(valueName, out Value output))
                    terms.AddRange(output.ToRawText(caller.values, sel));
                else terms.Add(new JSONText(src));
            }

            while (pieces.Count > 0)
                terms.Add(new JSONText(pieces.Pop()));

            return terms;
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
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
        public override void Execute(Executor caller, TokenFeeder tokens)
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
            type = TOKENTYPE._LIMIT;

            limit = text;
            none = text.ToUpper().Equals("NONE");
        }
        public override string ToString()
        {
            return none ? "Limit any count" : $"Limit {limit}";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            /*string output = caller.ReplacePPV(limit);

            try
            {
                var selector = caller.selection;
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

            return;*/
        }
    }
    public class TokenDEFINE : Token
    {
        public readonly string valueName;
        public string ValueName
        {
            get
            {
                int index = valueName.LastIndexOf(' ');
                if (index == -1)
                    return valueName;
                else return valueName.Substring(index + 1);
            }
        }
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
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string output = caller.ReplacePPV(valueName);

            foreach(string command in caller.values.DefineValue(output))
                caller.FinishRaw(command, false);

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
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string output = caller.ReplacePPV(valueName);

            if (!caller.values.TryGetValue(output, out Value value))
                throw new TokenException(this, $"No value exists with the name \"{output}\"");

            string[] scores = value.GetScoreboards(caller.values);
            foreach (string score in scores)
                caller.FinishRaw($"scoreboard players add @a \"{score}\" 0");
            return;
        }
    }
    public class TokenVALUE : Token
    {
        enum DisplayMode
        {
            LIST,
            SIDEBAR,
            BELOWNAME
        }
        enum DisplayDirection
        {
            ASCENDING,
            DESCENDING
        }
        static DisplayMode ParseDisplayMode(string mode)
        {
            switch(mode.ToUpper())
            {
                case "L":
                case "LIST":
                case "PLAYERLIST":
                case "PAUSE":
                case "PAUSEMENU":
                    return DisplayMode.LIST;
                case "S":
                case "SIDE":
                case "SIDEBAR":
                case "RIGHT":
                    return DisplayMode.SIDEBAR;
                case "N":
                case "BELOWNAME":
                case "UNDERNAME":
                case "NAME":
                    return DisplayMode.BELOWNAME;
                default:
                    return DisplayMode.SIDEBAR;
            }
        }
        static DisplayDirection ParseDisplayDirection(string direction)
        {
            switch (direction.ToUpper())
            {
                case "HIGH":
                case "ASC":
                case "ASCENDING":
                    return DisplayDirection.ASCENDING;
                default:
                    return DisplayDirection.DESCENDING;
            }
        }

        static string OperationString(ValueOperation op)
        {
            switch (op)
            {
                case ValueOperation.ADD:
                    return "+=";
                case ValueOperation.SUB:
                    return "-=";
                case ValueOperation.MUL:
                    return "*=";
                case ValueOperation.DIV:
                    return "/=";
                case ValueOperation.MOD:
                    return "%=";
                case ValueOperation.SET:
                    return "=";
                default:
                    return null;
            }
        }

        // value display <list|sidebar|below> [value] [ascending|descending]
        bool isDisplay;                     // If this is a dispay changing statement.
        DisplayMode displayMode;            // The display mode to set.
        string displayValue;                // If null, reset display mode.
        DisplayDirection displayDirection;  // The sorting direction to display as.

        // value <a> <operation> <b>
        string valueName;
        ValueOperation operation;
        bool bIsConstant; // Decides if you should use valueB or constantB
        bool bIsPPV;      // Decides if should still evaluate valueB
        string valueB;
        Dynamic constantB;

        public TokenVALUE(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.VALUE;

            string[] parts = text.Split(' ');

            if (parts.Length < 1)
                throw new TokenException(this, "Not enough arguments specified.");

            if(parts[0].ToUpper().Equals("DISPLAY"))
            {
                isDisplay = true;
                if(parts.Length < 2)
                    throw new TokenException(this, "Not enough arguments specified for display statement.");
                string _displayMode = parts[1];
                displayMode = ParseDisplayMode(_displayMode);

                displayValue = (parts.Length > 2) ?
                    parts[2] : null;

                displayDirection = (parts.Length > 3) ?
                    ParseDisplayDirection(parts[3]) : DisplayDirection.DESCENDING;
                return;
            } else
            {
                isDisplay = false;

                if (parts.Length < 3)
                    throw new TokenException(this, "Not enough arguments specified for value statement.");

                valueName = parts[0];
                string operationString = parts[1].ToUpper();
                string bString = parts[2];

                switch (operationString)
                {
                    case "ADD":
                        operation = ValueOperation.ADD;
                        break;
                    case "SUB":
                        operation = ValueOperation.SUB;
                        break;
                    case "MUL":
                        operation = ValueOperation.MUL;
                        break;
                    case "DIV":
                        operation = ValueOperation.DIV;
                        break;
                    case "MOD":
                        operation = ValueOperation.MOD;
                        break;
                    case "SET":
                        operation = ValueOperation.SET;
                        break;
                    case "+=":
                        operation = ValueOperation.ADD;
                        break;
                    case "-=":
                        operation = ValueOperation.SUB;
                        break;
                    case "*=":
                        operation = ValueOperation.MUL;
                        break;
                    case "/=":
                        operation = ValueOperation.DIV;
                        break;
                    case "%=":
                        operation = ValueOperation.MOD;
                        break;
                    case "=":
                        operation = ValueOperation.SET;
                        break;
                    default:
                        throw new TokenException(this, $"Invalid value operation {operationString}");
                }

                if (Compiler.guessedPPValues.Contains(bString.TrimStart('$')))
                {
                    bIsPPV = true;
                    bIsConstant = true;
                    valueB = bString;
                    return;
                }

                if ((bIsConstant = long.TryParse(bString, out long lparsed)))
                    constantB = new Dynamic(lparsed);
                else if (bIsConstant = double.TryParse(bString, out double dparsed))
                    constantB = new Dynamic(dparsed);
                else
                    valueB = bString;
            }
        }
        public override string ToString()
        {
            return $"Perform value operation {valueName} {OperationString(operation)} {(bIsConstant ? constantB.ToString() : valueB)}";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            if(isDisplay)
            {
                List<string> parts = new List<string>()
                {
                    "scoreboard",
                    "objectives",
                    "setdisplay",
                    displayMode.ToString().ToLower()
                };
                if(displayValue != null)
                {
                    parts.Add(displayValue);
                    if (displayMode != DisplayMode.BELOWNAME)
                        parts.Add(displayDirection.ToString().ToLower());
                }
                caller.FinishRaw(string.Join(" ", parts));
                return;
            }

            string _sourceValue = caller.ReplacePPV(valueName);
            string secondValue = bIsConstant ? null : caller.ReplacePPV(valueB);
            string selector = '@' + caller.selection.ToString();

            if (!caller.values.HasValue(_sourceValue))
                throw new TokenException(this, $"No value exists with the name \"{_sourceValue}\"");
            if (secondValue != null && !caller.values.HasValue(secondValue) && !bIsPPV)
                throw new TokenException(this, $"No value exists with the name \"{secondValue}\"");

            Value sourceValue = caller.values[_sourceValue];

            // Resolve PPV value
            if (bIsPPV)
                constantB = caller.ppv[valueB.Substring(1)];

            // Create temp objective for more complex operations
            if (!caller.HasCreatedTemplate(Executor.MATH_TEMP) && (((byte)operation) % 2 == 1) && bIsConstant)
                caller.CreateTemplate(Executor.MATH_TEMP, new[] { $"scoreboard objectives add {Executor.MATH_TEMP} dummy" });

            // Create decimal unit objective for fixed point operations
            if(sourceValue.type == ValueType.DECIMAL)
            {
                caller.CreateTemplate(Executor.MATH_TEMP, new[] { $"scoreboard objectives add {Executor.MATH_TEMP} dummy" });
                caller.CreateTemplate(Executor.DECIMAL_UNIT, new[] { $"scoreboard objectives add {Executor.DECIMAL_UNIT} dummy" });
            }


            if (bIsConstant)
            {
                switch (operation)
                {
                    case ValueOperation.ADD:
                        foreach (string line in ValueManager.ExpressionAddConstant(sourceValue, selector, constantB))
                            caller.FinishRaw(line, false);
                        return;
                    case ValueOperation.SUB:
                        if (sourceValue.type == ValueType.DECIMAL)
                        {
                            string functionName = Executor.DECIMAL_SUB_CARRY + sourceValue.name;
                            caller.CreateTemplate(functionName, new string[]
                            {
                                $"scoreboard players operation {selector} {Executor.DECIMAL_UNIT} += {selector} {sourceValue.DecimalPart}",
                                $"scoreboard players add {selector} {sourceValue.WholePart} -1",
                                $"scoreboard players operation {selector} {sourceValue.DecimalPart} = {selector} {Executor.DECIMAL_UNIT}"
                            }, true);
                            foreach (string line in ValueManager.ExpressionSubtractConstant(sourceValue, selector, constantB))
                                caller.FinishRaw(line, false);
                        }
                        return;
                    case ValueOperation.MUL:
                        foreach (string line in ValueManager.ExpressionMultiplyConstant(sourceValue, selector, constantB))
                            caller.FinishRaw(line, false);
                        return;
                    case ValueOperation.SET:
                        foreach (string line in ValueManager.ExpressionSetConstant(sourceValue, selector, constantB))
                            caller.FinishRaw(line, false);
                        return;
                    case ValueOperation.DIV:
                        foreach (string line in ValueManager.ExpressionDivideConstant(sourceValue, selector, constantB))
                            caller.FinishRaw(line, false);
                        return;
                    case ValueOperation.MOD:
                        foreach (string line in ValueManager.ExpressionModuloConstant(sourceValue, selector, constantB))
                            caller.FinishRaw(line, false);
                        return;
                    default:
                        break;
                }
            }
            else
            {
                Value otherValue = caller.values[secondValue];
                switch (operation)
                {
                    case ValueOperation.ADD:
                        foreach (string line in ValueManager.ExpressionAddValue(sourceValue, otherValue, selector))
                            caller.FinishRaw(line, false);
                        break;
                    case ValueOperation.SUB:
                        string functionName = Executor.DECIMAL_SUB_CARRY + sourceValue.name;
                        caller.CreateTemplate(functionName, new string[]
                        {
                                $"scoreboard players operation {selector} {Executor.DECIMAL_UNIT} += {selector} {sourceValue.DecimalPart}",
                                $"scoreboard players add {selector} {sourceValue.WholePart} -1",
                                $"scoreboard players operation {selector} {sourceValue.DecimalPart} = {selector} {Executor.DECIMAL_UNIT}"
                        }, true);
                        foreach (string line in ValueManager.ExpressionSubtractValue(sourceValue, otherValue, selector))
                            caller.FinishRaw(line, false);
                        break;
                    case ValueOperation.MUL:
                        foreach (string line in ValueManager.ExpressionMultiplyValue(sourceValue, otherValue, selector))
                            caller.FinishRaw(line, false);
                        break;
                    case ValueOperation.SET:
                        foreach (string line in ValueManager.ExpressionSetValue(sourceValue, otherValue, selector))
                            caller.FinishRaw(line, false);
                        break;
                    case ValueOperation.DIV:
                        foreach (string line in ValueManager.ExpressionDivideValue(sourceValue, otherValue, selector))
                            caller.FinishRaw(line, false);
                        break;
                    case ValueOperation.MOD:
                        foreach (string line in ValueManager.ExpressionModuloValue(sourceValue, otherValue, selector))
                            caller.FinishRaw(line, false);
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
            NAME,
            LIMIT
        }
        struct IFStatement
        {
            static readonly char[] alphabet = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
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
                        case Type.LIMIT:
                            return 1;
                        default:
                            return 0;
                    }
                }
            }
            /// <summary>
            /// A short unique identifier for this if-statement.
            /// </summary>
            public int UID
            {
                get
                {
                    int hash = string.Join("", args).GetHashCode();
                    hash ^= type.ToString().GetHashCode();
                    return hash;
                }
            }

            public bool not;
            public Type type;
            public string[] args;

            public void ParseIntoSelector(ref Selector selector, ref Executor context)
            {
                int scope = context.CurrentFunctionScope;

                for (int i = 0; i < args.Length; i++)
                    args[i] = context.ReplacePPV(args[i]);

                switch (type)
                {
                    case Type.none:
                        return;
                    case Type.VALUE:
                        string valueName = args[0];
                        Operator op = Operator.Parse(args[1]);
                        string checkValue = args[2];
                        if (int.TryParse(checkValue, out int otherInt))
                        {
                            OperatorType type = op.type;
                            if (not) // this would be really bad code but make it work anyways...
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
                            string inverterName = Executor.MATH_INVERTER + scope;
                            context.CreateTemplate(inverterName, new[] {
                                $"scoreboard objectives add {inverterName} dummy"
                            });
                            context.FinishRaw($"scoreboard players set @{context.SelectionReference} {inverterName} 0", false);
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
                        if (not) // Would need to check for both sides of the range.
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
                        if (not) // Would require a 6-region check which I'm just not ready to do
                            throw new NotSupportedException("IN if-statments cannot be inverted.");
                        string _sizeX = args[0];
                        string _sizeY = args[1];
                        string _sizeZ = args[2];
                        int sizeX = int.Parse(_sizeX);
                        int sizeY = int.Parse(_sizeY);
                        int sizeZ = int.Parse(_sizeZ);
                        _x = args.Length > 3 ? args[3] : null;
                        _y = args.Length > 4 ? args[4] : null;
                        _z = args.Length > 5 ? args[5] : null;
                        selector.area.radiusMax = null;
                        selector.area.radiusMin = null;
                        if (sizeX != 0)
                        {
                            selector.area.volumeX = sizeX;
                            selector.area.x = CoordinateValue.Parse(_x);
                        }
                        if (sizeY != 0)
                        {
                            selector.area.volumeY = sizeY;
                            selector.area.y = CoordinateValue.Parse(_y);
                        }
                        if (sizeZ != 0)
                        {
                            selector.area.volumeZ = sizeZ;
                            selector.area.z = CoordinateValue.Parse(_z);
                        }
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
                    case Type.LIMIT:
                        selector.count.count = int.Parse(args[0]);
                        return;
                    default:
                        throw new MissingFieldException("Invalid if-statement criteria");
                }
            }
        }

        public string eval;
        public bool forceInvert = false;
        List<IFStatement> statements = new List<IFStatement>();
        /// <summary>
        /// Creates a unique function name for the conditions inside this.
        /// </summary>
        public string BranchFunctionName(TokenBlock block)
        {
            int hash = 0;
            statements.ForEach(s =>
            {
                if (hash == 0)
                    hash = s.UID;
                else hash ^= s.UID;
            });

            hash ^= block.GetHashCode();
            return (forceInvert ? "f" : "n") + type.ToString() + '_' + hash;
        }
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
                case "LIMIT":
                case "COUNT":
                    current.type = Type.LIMIT;
                    break;
                default:
                    current.type = Type.VALUE;
                    break;
            }

            int minArgCount = current.MinimumArgCount;
            int removed = current.not ? 2 : 1;
            // Value statements are inferred and don't use an identifier.
            if (current.type == Type.VALUE)
                removed--;
            if (args.Length - removed < minArgCount)
                throw new TokenException(this, $"Too few arguments for if statement type {current.type}");

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
        public Selector ConstructSelector(Selector.Core core, ref Executor caller)
        {
            Selector selector = new Selector() { core = core };

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
                    clone.ParseIntoSelector(ref selector, ref caller);
                }
                catch (NotSupportedException nse)
                {
                    if (forceInvert)
                        continue;
                    throw nse;
                }
            }

            return selector;
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            Selector selector = ConstructSelector(caller.selection, ref caller);

            if (forceInvert)
                return;

            Token potentialBlock = tokens.Peek();
            if (potentialBlock != null)
            {
                if(potentialBlock is TokenBlock)
                {
                    TokenBlock block = tokens.Next() as TokenBlock;
                    FunctionDefinition a = new FunctionDefinition()
                    {
                        name = BranchFunctionName(block),
                        isNamespaced = true,
                        theNamespace = "_branching",
                        isDelay = false,
                        isTick = false,
                        security = FunctionSecurity.NONE
                    };
                    // Compile rest of branch into function.
                    block.PlaceInFunction(caller, tokens, a);
                    string command = $"function {a.CommandName}";
                    caller.FinishRaw(selector.GetAsPrefix() + command, false);
                } else
                {
                    caller.SetRaw(selector.GetAsPrefix());
                    tokens.Next().Execute(caller, tokens);
                }

                Token potentialElse = tokens.Peek();
                if (potentialElse == null || !(potentialElse is TokenELSE))
                    return;
                tokens.Next();
                potentialBlock = tokens.Peek();
                if (potentialBlock == null)
                    throw new NullReferenceException("Reached end of file after else-statement.");
                if(potentialBlock is TokenBlock)
                {
                    TokenBlock elseBlock = tokens.Next() as TokenBlock;
                    forceInvert = true;
                    selector = ConstructSelector(caller.selection, ref caller);
                    FunctionDefinition b = new FunctionDefinition()
                    {
                        name = BranchFunctionName(elseBlock),
                        isNamespaced = true,
                        theNamespace = "_branching",
                        isDelay = false,
                        isTick = false,
                        security = FunctionSecurity.NONE
                    };
                    elseBlock.PlaceInFunction(caller, tokens, b);
                    caller.FinishRaw(selector.GetAsPrefix() + $"function {b.CommandName}", false);
                    forceInvert = false;
                } else
                {
                    forceInvert = true;
                    selector = ConstructSelector(caller.selection, ref caller);
                    caller.SetRaw(selector.GetAsPrefix());
                    tokens.Next().Execute(caller, tokens);
                    forceInvert = false;
                }
                return;
            }
            else
            {
                throw new NullReferenceException("Reached end of file after if-statement.");
            }
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
            return $"Else:";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
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
        public override void Execute(Executor caller, TokenFeeder tokens)
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
    public class TokenTP : Token
    {
        string target = null;

        string
            x = null,
            y = null,
            z = null;
        string
            xRot = null,
            yRot = null;
        public TokenTP(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.TP;

            if(string.IsNullOrWhiteSpace(text))
                throw new TokenException(this, "Insufficient arguments.");

            string[] args = text.Split(' ');

            if (args.Length == 1 || text.StartsWith("@") || CoordinateValue.Parse(args[0]) == null)
            {
                target = text;
                return;
            }

            if (args.Length < 3)
                throw new TokenException(this, "Insufficient arguments.");

            x = args[0];
            y = args[1];
            z = args[2];

            if(args.Length > 4)
            {
                xRot = args[3];
                yRot = args[4];
            }
        }
        public override string ToString()
        {
            if (target != null)
                return $"Teleport to {target}";
            else
            {
                if(yRot != null)
                    return $"Teleport to {x} {y} {z} rotated {xRot} {yRot}";
                else return $"Teleport to {x} {y} {z}";
            }
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string sel = '@' + caller.SelectionReference.ToString();
            if (target == null)
            {
                string _x = caller.ReplacePPV(x);
                string _y = caller.ReplacePPV(y);
                string _z = caller.ReplacePPV(z);
                // null coalescing was throwing for some reason so
                string _xRot = xRot == null ? null : caller.ReplacePPV(xRot);
                string _yRot = yRot == null ? null : caller.ReplacePPV(yRot);
                bool hasRotation = yRot != null;
                try
                {
                    CoordinateValue px = CoordinateValue.Parse(_x).Value;
                    CoordinateValue py = CoordinateValue.Parse(_y).Value;
                    CoordinateValue pz = CoordinateValue.Parse(_z).Value;
                    if(hasRotation)
                    {
                        CoordinateValue prx = CoordinateValue.Parse(_xRot).Value;
                        CoordinateValue pry = CoordinateValue.Parse(_yRot).Value;
                        caller.FinishRaw($"tp {sel} {px} {py} {pz} {prx} {pry}");
                    } else
                    {
                        caller.FinishRaw($"tp {sel} {px} {py} {pz}");
                    }
                } catch(Exception)
                {
                    if(hasRotation)
                        throw new TokenException(this, $"Couldn't parse XYZR position/rotation ({_x}, {_y}, {_z}) ({_xRot}, {_yRot})");
                    else
                        throw new TokenException(this, $"Couldn't parse XYZ position ({_x}, {_y}, {_z})");
                }
            }
            else
            {
                string location = caller.ReplacePPV(target);
                caller.FinishRaw($"tp {sel} {location}");
            }
            return;
        }
    }
    public class TokenTITLE : Token
    {
        string text;
        public TokenTITLE(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PRINTP;

            this.text = text;
        }
        public override string ToString()
        {
            return $"Title to Selector: \"{text}\"";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string selector = "@" + caller.SelectionReference;

            caller.FinishRaw($"title {selector} title {text}");
        }
    }
    public class TokenKICK : Token
    {
        string text;
        string reason;
        string player;
        public TokenKICK(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PRINTP;

            string[] args = Compiler.GetArguments(text);

            if (args.Length < 1)
                throw new TokenException(this, "\"KICK\" statement needs an Player to Kick.");

            else if (args.Length < 2)
                throw new TokenException(this, "\"KICK\" statement needs an Kick Reason.");

            player = args[0];
            reason = args[1];
            this.text = text;
        }
        public override string ToString()
        {
            return $"Kick Player: \"{text}\", Reason: \"{reason}\"";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string selector = "@" + caller.SelectionReference;

            caller.FinishRaw($"kick {player} {reason}");
        }
    }
    public class TokenGAMEMODE : Token
    {
        string text;
        string gamemode;
        string player;
        public TokenGAMEMODE(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PRINTP;

            string[] args = Compiler.GetArguments(text);

            if (args.Length < 1)
                throw new TokenException(this, "\"GAMEMODE\" statement needs an Player to Gamemode.");

            else if (args.Length < 2)
                throw new TokenException(this, "\"GAMEMODE\" statement needs an Gamemode ID.");

            player = args[0];
            gamemode = args[1];
            this.text = text;
        }
        public override string ToString()
        {
            return $"Gamemode Player \"{player}\" to \"{gamemode}\"";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            caller.FinishRaw($"gamemode {player} {gamemode}");
        }
    }
    public class TokenTIME : Token
    {
        string uvar1;
        string uvar2;
        string text;
        public TokenTIME(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PRINTP;

            string[] args = Compiler.GetArguments(text);

            if (args.Length < 1)
                throw new TokenException(this, "\"TIME\" statement needs a Mode.");

            else if (args.Length < 2)
                throw new TokenException(this, "\"TIME\" statement needs a Time.");

            uvar1 = args[0];
            uvar2 = args[1];
            this.text = text;
        }
        public override string ToString()
        {
            return $"Change Time to \"{uvar2}\" with mode \"{uvar1}\"";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            caller.FinishRaw($"time {uvar1} {uvar2}");
        }
    }
    public class TokenDIFFICULTY : Token
    {
        string text;
        string difficulty;
        public TokenDIFFICULTY(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PRINTP;

            string[] args = Compiler.GetArguments(text);

            if (args.Length < 1)
                throw new TokenException(this, "\"DIFFICULTY\" statement needs an Difficulty.");

            difficulty = args[0];
            this.text = text;
        }
        public override string ToString()
        {
            return $"Change Difficulty to \"{difficulty}\" ";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            caller.FinishRaw($"difficulty {difficulty}");
        }
    }
    public class TokenWEATHER : Token
    {
        string text;
        string weather;
        public TokenWEATHER(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PRINTP;

            string[] args = Compiler.GetArguments(text);

            if (args.Length < 1)
                throw new TokenException(this, "\"WEATHER\" statement needs an Weather.");

            weather = args[0];
            this.text = text;
        }
        public override string ToString()
        {
            return $"Change Weather to \"{weather}\" ";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            caller.FinishRaw($"weather {weather}");
        }
    }
    public class TokenMOVE : Token
    {
        enum MoveDirection
        {
            FORWARDS,
            BACKWARDS,
            LEFT,
            RIGHT,
            UP,
            DOWN,

            NONE
        }
        static MoveDirection ParseDirection(string str)
        {
            switch (str.ToUpper())
            {
                case "F":
                case "FORWARD":
                case "FORWARDS":
                    return MoveDirection.FORWARDS;
                case "B":
                case "BACKWARD":
                case "BACKWARDS":
                    return MoveDirection.BACKWARDS;
                case "L":
                case "LEFT":
                    return MoveDirection.LEFT;
                case "R":
                case "RIGHT":
                    return MoveDirection.RIGHT;
                case "U":
                case "UP":
                    return MoveDirection.UP;
                case "D":
                case "DOWN":
                    return MoveDirection.DOWN;
                default:
                    return MoveDirection.NONE;
            }
        }

        string direction;
        string amount;
        public TokenMOVE(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.MOVE;

            if (string.IsNullOrWhiteSpace(text))
                throw new TokenException(this, "Insufficient arguments.");

            string[] args = text.Split(' ');

            if (args.Length < 2)
                throw new TokenException(this, "Insufficient arguments.");

            direction = args[0];
            amount = args[1];
        }
        public override string ToString()
        {
            return $"Move selected entity {direction} {amount} blocks.";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string inputDirection = caller.ReplacePPV(direction);
            string inputAmount = caller.ReplacePPV(amount);
            MoveDirection d = ParseDirection(inputDirection);

            if (d == MoveDirection.NONE)
                throw new TokenException(this, "Invalid move direction.");

            string amountString;
            string selector = "@" + caller.SelectionReference;
            if (int.TryParse(inputAmount, out int i))
                amountString = i.ToString();
            else if (float.TryParse(inputAmount, out float f))
                amountString = f.ToString();
            else
                throw new TokenException(this, $"Invalid move amount \"{inputAmount}\"");

            switch (d)
            {
                case MoveDirection.FORWARDS:
                    caller.FinishRaw($"tp {selector} ^ ^ ^{amountString}");
                    break;
                case MoveDirection.BACKWARDS:
                    caller.FinishRaw($"tp {selector} ^ ^ ^-{amountString}");
                    break;
                case MoveDirection.LEFT:
                    caller.FinishRaw($"tp {selector} ^{amountString} ^ ^");
                    break;
                case MoveDirection.RIGHT:
                    caller.FinishRaw($"tp {selector} ^-{amountString} ^ ^");
                    break;
                case MoveDirection.UP:
                    caller.FinishRaw($"tp {selector} ^ ^{amountString} ^");
                    break;
                case MoveDirection.DOWN:
                    caller.FinishRaw($"tp {selector} ^ ^-{amountString} ^");
                    break;
                case MoveDirection.NONE:
                    break;
            }
            return;
        }
    }
    public class TokenFACE : Token
    {
        bool looksAtTarget = false;

        string target = null;
        string
            x = null,
            y = null,
            z = null;

        public TokenFACE(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.FACE;

            if (string.IsNullOrWhiteSpace(text))
                throw new TokenException(this, "Insufficient arguments.");

            string[] args = text.Split(' ');

            if (args.Length == 1 || text.StartsWith("@") || CoordinateValue.Parse(args[0]) == null)
            {
                looksAtTarget = true;
                target = text;
                return;
            }

            if (args.Length < 3)
                throw new TokenException(this, "Insufficient arguments.");

            x = args[0];
            y = args[1];
            z = args[2];
        }
        public override string ToString()
        {
            if (looksAtTarget)
                return $"Face entity {target}";
            else
            {
                return $"Face location {x} {y} {z}";
            }
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string sel = '@' + caller.SelectionReference.ToString();
            if (target == null)
            {
                string _x = caller.ReplacePPV(x);
                string _y = caller.ReplacePPV(y);
                string _z = caller.ReplacePPV(z);
                try
                {
                    CoordinateValue px = CoordinateValue.Parse(_x).Value;
                    CoordinateValue py = CoordinateValue.Parse(_y).Value;
                    CoordinateValue pz = CoordinateValue.Parse(_z).Value;
                    caller.FinishRaw($"tp {sel} ~~~ facing {px} {py} {pz}");
                }
                catch (Exception)
                {
                    throw new TokenException(this, $"Couldn't parse XYZ position ({_x}, {_y}, {_z})");
                }
            }
            else
            {
                string location = caller.ReplacePPV(target);
                caller.FinishRaw($"tp {sel} ~~~ facing {location}");
            }
            return;
        }
    }
    public class TokenPLACE : Token
    {
        string
            block = null,
            x = null,
            y = null,
            z = null,
            data = null,
            destroyMode = null;
        public TokenPLACE(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.PLACE;
            
            string[] args = text.Split(' ');

            if (args.Length < 4)
                throw new TokenException(this, "Insufficient arguments.");

            block = args[0]; 
            x = args[1];
            y = args[2];
            z = args[3];

            if (args.Length > 4)
                data = args[4];
            if (args.Length > 5)
                destroyMode = args[5];
        }
        public override string ToString()
        {
            if (data != null)
                return $"Place block {block} at {x} {y} {z} with data {data}";
            else
                return $"Place block {block} at {x} {y} {z}";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string _block = caller.ReplacePPV(block);
            string _x = caller.ReplacePPV(x);
            string _y = caller.ReplacePPV(y);
            string _z = caller.ReplacePPV(z);
            try
            {
                CoordinateValue px = CoordinateValue.Parse(_x).Value;
                CoordinateValue py = CoordinateValue.Parse(_y).Value;
                CoordinateValue pz = CoordinateValue.Parse(_z).Value;
                if (data == null)
                    caller.FinishRaw($"setblock {px} {py} {pz} {_block} 0 replace");
                else if (destroyMode == null)
                {
                    string _data = caller.ReplacePPV(data);
                    caller.FinishRaw($"setblock {px} {py} {pz} {_block} {_data} replace");
                } else
                {
                    string _data = caller.ReplacePPV(data);
                    Block.DestroyMode mode = Block.ParseDestroyMode(caller.ReplacePPV(destroyMode));
                    caller.FinishRaw($"setblock {px} {py} {pz} {_block} {_data} {mode.ToString().ToLower()}");
                }
            }
            catch (Exception)
            {
                throw new TokenException(this, $"Couldn't parse XYZ position ({_x}, {_y}, {_z})");
            }
            return;
        }
    }
    public class TokenFILL : Token
    {
        public enum FillMode
        {
            REPLACE,
            KEEP,
            DESTROY,
            OUTLINE,
            HOLLOW
        }
        public static FillMode ParseFillMode(string str)
        {
            switch (str.ToUpper())
            {
                case "R":
                case "REPLACE":
                case "DEFAULT":
                case "REMOVE":
                    return FillMode.REPLACE;
                case "K":
                case "KEEP":
                case "AIR":
                case "PRESERVE":
                    return FillMode.KEEP;
                case "D":
                case "DESTROY":
                case "BREAK":
                case "SIMULATE":
                    return FillMode.DESTROY;
                case "O":
                case "OUTLINE":
                case "WALLS":
                    return FillMode.OUTLINE;
                case "H":
                case "HOLLOW":
                case "ROOM":
                    return FillMode.HOLLOW;
                default:
                    return FillMode.REPLACE;
            }
        }

        string
            block = null,
            x1 = null,
            y1 = null,
            z1 = null,
            x2 = null,
            y2 = null,
            z2 = null,
            data = null,
            fillMode = null;
        public TokenFILL(string text)
        {
            line = Compiler.CURRENT_LINE;
            type = TOKENTYPE.FILL;

            string[] args = text.Split(' ');

            if (args.Length < 7)
                throw new TokenException(this, "Insufficient arguments.");

            block = args[0];
            x1 = args[1];
            y1 = args[2];
            z1 = args[3];
            x2 = args[4];
            y2 = args[5];
            z2 = args[6];

            if (args.Length > 7)
                data = args[7];
            if (args.Length > 8)
                fillMode = args[8];
        }
        public override string ToString()
        {
            if (data != null)
                return $"Fill area [({x1}, {y1}, {z1}), ({x2}, {y2}, {z2})] with {block}";
            else
                return $"Fill area [({x1}, {y1}, {z1}), ({x2}, {y2}, {z2})] with {block} and data {data}";
        }
        public override void Execute(Executor caller, TokenFeeder tokens)
        {
            string _block = caller.ReplacePPV(block);
            string _x1 = caller.ReplacePPV(x1);
            string _y1 = caller.ReplacePPV(y1);
            string _z1 = caller.ReplacePPV(z1);
            string _x2 = caller.ReplacePPV(x2);
            string _y2 = caller.ReplacePPV(y2);
            string _z2 = caller.ReplacePPV(z2);
            try
            {
                CoordinateValue px1 = CoordinateValue.Parse(_x1).Value;
                CoordinateValue py1 = CoordinateValue.Parse(_y1).Value;
                CoordinateValue pz1 = CoordinateValue.Parse(_z1).Value;
                CoordinateValue px2 = CoordinateValue.Parse(_x2).Value;
                CoordinateValue py2 = CoordinateValue.Parse(_y2).Value;
                CoordinateValue pz2 = CoordinateValue.Parse(_z2).Value;

                if (data == null)
                    caller.FinishRaw($"fill {px1} {py1} {pz1} {px2} {py2} {pz2} {_block} 0 replace");
                else if (fillMode == null)
                {
                    string _data = caller.ReplacePPV(data);
                    caller.FinishRaw($"fill {py1} {pz1} {px2} {py2} {pz2} {_block} {_data} replace");
                }
                else
                {
                    string _data = caller.ReplacePPV(data);
                    FillMode mode = ParseFillMode(caller.ReplacePPV(fillMode));
                    caller.FinishRaw($"fill {py1} {pz1} {px2} {py2} {pz2} {_block} {_data} {mode.ToString().ToLower()}");
                }
            }
            catch (Exception)
            {
                throw new TokenException(this, $"Couldn't parse positions [({_x1}, {_y1}, {_z1}), ({_x2}, {_y2}, {_z2})]");
            }
            return;
        }
    }
}
