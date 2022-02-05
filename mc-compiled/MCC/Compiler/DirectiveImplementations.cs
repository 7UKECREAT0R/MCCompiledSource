using mc_compiled.Commands;
using mc_compiled.Commands.Native;
using mc_compiled.Commands.Selectors;
using mc_compiled.Json;
using mc_compiled.Modding;
using mc_compiled.NBT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    public static class DirectiveImplementations
    {
        public static int scatterFile = 0;

        public static void _var(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object value = tokens.Next<IObjectable>().GetObject();

            executor.SetPPV(varName, value);
        }
        public static void _inc(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value++;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _dec(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value--;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _add(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object other = tokens.Next<IObjectable>().GetObject();

            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value += other;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _sub(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object other = tokens.Next<IObjectable>().GetObject();

            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value -= other;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _mul(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object other = tokens.Next<IObjectable>().GetObject();

            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value *= other;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _div(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object other = tokens.Next<IObjectable>().GetObject();

            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value /= other;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _mod(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            object other = tokens.Next<IObjectable>().GetObject();

            if (executor.TryGetPPV(varName, out dynamic value))
            {
                value %= other;
                executor.SetPPV(varName, value);
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");
        }
        public static void _swap(Executor executor, Statement tokens)
        {
            string aName = tokens.Next<TokenIdentifier>().word;
            string bName = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(aName, out dynamic a))
            {
                if (executor.TryGetPPV(bName, out dynamic b))
                {
                    executor.SetPPV(aName, b);
                    executor.SetPPV(bName, a);
                }
                else
                    throw new StatementException(tokens, "Preprocessor variable '" + bName + "' does not exist.");
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + aName + "' does not exist.");
        }
        public static void _if(Executor executor, Statement tokens)
        {
            string varName = tokens.Next<TokenIdentifier>().word;
            TokenCompare compare = tokens.Next<TokenCompare>();
            object other = tokens.Next<IObjectable>().GetObject();

            // if the next block/statement should be run
            bool run = false;

            if(executor.TryGetPPV(varName, out dynamic a))
            {
                switch (compare.GetCompareType())
                {
                    case TokenCompare.Type.EQUAL:
                        run = a == other;
                        break;
                    case TokenCompare.Type.NOT_EQUAL:
                        run = a != other;
                        break;
                    case TokenCompare.Type.LESS_THAN:
                        run = a < other;
                        break;
                    case TokenCompare.Type.LESS_OR_EQUAL:
                        run = a <= other;
                        break;
                    case TokenCompare.Type.GREATER_THAN:
                        run = a > other;
                        break;
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                        run = a >= other;
                        break;
                    default:
                        run = false;
                        break;
                }
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + varName + "' does not exist.");

            if (!executor.HasNext)
                throw new StatementException(tokens, "End of file after $if statement.");

            executor.SetLastIfResult(run);

            if (executor.NextIs<StatementOpenBlock>())
            {
                StatementOpenBlock block = executor.Peek<StatementOpenBlock>();
                block.aligns = false;
                block.shouldRun = run;
                return;
            }
            else if(!run)
                executor.Next(); // skip the next statement
        }
        public static void _else(Executor executor, Statement tokens)
        {
            bool run = !executor.GetLastIfResult();

            if (executor.NextIs<StatementOpenBlock>())
            {
                StatementOpenBlock block = executor.Peek<StatementOpenBlock>();
                block.aligns = false;
                block.shouldRun = run;
                return;
            }
            else if (!run)
                executor.Next(); // skip the next statement
        }
        public static void _repeat(Executor executor, Statement tokens)
        {
            int amount = tokens.Next<TokenIntegerLiteral>();
            string tracker = null;

            if (tokens.HasNext && tokens.NextIs<TokenIdentifier>())
                tracker = tokens.Next<TokenIdentifier>().word;

            int skipAfter = 0;
            Statement[] statements;

            if (executor.NextIs<StatementOpenBlock>())
            {
                StatementOpenBlock block = executor.Next<StatementOpenBlock>();
                skipAfter = block.statementsInside;
                statements = executor.Peek(skipAfter);
                executor.Next<StatementOpenBlock>(); // skip that
            } else
            {
                skipAfter = 0;
                statements = new[] { executor.Next() };
            }

            for (int i = 0; i < amount; i++)
            {
                if (tracker != null)
                    executor.SetPPV(tracker, i);
                executor.ExecuteSubsection(statements);
            }

            for (int i = 0; i < skipAfter; i++)
                executor.Next();
        }
        public static void _log(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            Console.WriteLine("[LOG] {0}", str);
        }
        public static void _macro(Executor executor, Statement tokens)
        {
            if (executor.NextIs<StatementOpenBlock>())
                _macrodefine(executor, tokens);
            else
                _macrocall(executor, tokens);
        }
        public static void _macrodefine(Executor executor, Statement tokens)
        {
            string macroName = tokens.Next<TokenIdentifier>().word;

            List<string> args = new List<string>();
            while (tokens.HasNext && tokens.NextIs<TokenIdentifier>())
                args.Add(tokens.Next<TokenIdentifier>().word);

            StatementOpenBlock block = executor.Next<StatementOpenBlock>();
            int count = block.statementsInside;
            Statement[] statements = executor.Peek(count);

            if (count < 1)
                throw new StatementException(tokens, "Cannot have empty macro.");
            for (int i = 0; i < count; i++)
                executor.Next(); // skip over those

            executor.Next<StatementCloseBlock>();

            Macro macro = new Macro(macroName, args.ToArray(), statements);
            executor.RegisterMacro(macro);
        }
        public static void _macrocall(Executor executor, Statement tokens)
        {
            string macroName = tokens.Next<TokenIdentifier>().word;
            Macro? _lookedUp = executor.LookupMacro(macroName);

            if (!_lookedUp.HasValue)
                throw new StatementException(tokens, "Macro '" + macroName + "' does not exist.");

            Macro lookedUp = _lookedUp.Value;
            string[] argNames = lookedUp.argNames;
            object[] args = new object[argNames.Length];

            // get input variables
            for (int i = 0; i < argNames.Length; i++) {
                if (!tokens.HasNext)
                    throw new StatementException(tokens, "Missing argument '" + argNames[i] + "' in macro call.");
                if(!tokens.NextIs<IObjectable>())
                    throw new StatementException(tokens, "Invalid argument type for '" + argNames[i] + "' in macro call.");
                args[i] = tokens.Next<IObjectable>().GetObject();
            }

            // save variables which collide with this macro's args.
            Dictionary<string, dynamic> collidedValues
                = new Dictionary<string, dynamic>();
            foreach (string arg in lookedUp.argNames)
                if (executor.TryGetPPV(arg, out dynamic value))
                    collidedValues[arg] = value;

            // set input variables
            for (int i = 0; i < argNames.Length; i++)
                executor.SetPPV(argNames[i], args[i]);

            // call macro
            executor.ExecuteSubsection(lookedUp.statements);

            // restore variables
            foreach (var kv in collidedValues)
                executor.SetPPV(kv.Key, kv.Value);
        }
        public static void _include(Executor executor, Statement tokens)
        {
            string file = tokens.Next<TokenStringLiteral>();

            if (!System.IO.File.Exists(file))
                throw new StatementException(tokens, "Cannot find file '" + file + "'.");

            Token[] includedTokens = Tokenizer.TokenizeFile(file);
            // TODO assemble statements
            // executor.ExecuteSubsection(statements);
        }
        public static void _strfriendly(Executor executor, Statement tokens)
        {
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

            if(executor.TryGetPPV(input, out dynamic value))
            {
                string str = value.ToString();
                string[] parts = str.Split('_', '-', ' ');
                for (int i = 0; i < parts.Length; i++)
                {
                    char[] part = parts[i].ToCharArray();
                    for (int c = 0; c < part.Length; c++)
                        part[c] = (c == 0) ? char.ToUpper(part[c]) : char.ToLower(part[c]);
                    parts[i] = new string(part);
                }
                executor.SetPPV(output, string.Join(" ", parts));
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _strupper(Executor executor, Statement tokens)
        {
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(input, out dynamic value))
            {
                string str = value.ToString();
                executor.SetPPV(output, str.ToUpper());
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }
        public static void _strlower(Executor executor, Statement tokens)
        {
            string input = tokens.Next<TokenIdentifier>().word;
            string output = tokens.Next<TokenIdentifier>().word;

            if (executor.TryGetPPV(input, out dynamic value))
            {
                string str = value.ToString();
                executor.SetPPV(output, str.ToLower());
            }
            else
                throw new StatementException(tokens, "Preprocessor variable '" + input + "' does not exist.");
        }

        public static void mc(Executor executor, Statement tokens)
        {
            string command = tokens.Next<TokenStringLiteral>();
            executor.AddCommand(command);
        }
        public static void select(Executor executor, Statement tokens)
        {
            TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
            if (!selector.simple)
                Console.WriteLine("WARNING: Advanced selectors cannot be used in 'select' directive.");

            executor.ActiveSelector = selector;
        }
        public static void globalprint(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            RawTextJsonBuilder builder = new RawTextJsonBuilder();
            builder.AddTerms(executor.FString(str));

            string output = builder.BuildString();
            executor.AddCommand(Command.Tellraw(output));
        }
        public static void print(Executor executor, Statement tokens)
        {
            string str = tokens.Next<TokenStringLiteral>();
            RawTextJsonBuilder builder = new RawTextJsonBuilder();
            builder.AddTerms(executor.FString(str));

            string output = builder.BuildString();
            string selector = executor.ActiveSelectorStr;
            executor.AddCommand(Command.Tellraw(selector, output));
        }
        public static void define(Executor executor, Statement tokens)
        {
            if(tokens.NextIs<TokenIdentifierStruct>())
            {
                TokenIdentifierStruct _struct = tokens.Next<TokenIdentifierStruct>();
                TokenStringLiteral @string = tokens.Next<TokenStringLiteral>();
                ScoreboardValueStruct sbValue = new ScoreboardValueStruct
                    (@string, _struct.@struct, executor.scoreboard);
                executor.scoreboard.Add(sbValue);
                return;
            }

            const int TYPE_INT = 0;
            const int TYPE_DECIMAL = 1;
            const int TYPE_BOOL = 2;
            const int TYPE_TIME = 3;

            int type = TYPE_INT;

            if(tokens.NextIs<TokenIdentifier>())
            {
                TokenIdentifier identifier = tokens.Next<TokenIdentifier>();
                string typeWord = identifier.word.ToUpper();
                switch (typeWord)
                {
                    case "INT":
                        type = TYPE_INT;
                        break;
                    case "DECIMAL":
                        type = TYPE_DECIMAL;
                        break;
                    case "BOOL":
                        type = TYPE_BOOL;
                        break;
                    case "TIME":
                        type = TYPE_TIME;
                        break;
                    default:
                        throw new StatementException(tokens, $"Invalid type identifier '{typeWord}'.");
                }
            }

            if (type == TYPE_DECIMAL)
            {
                if (!tokens.NextIs<TokenIntegerLiteral>())
                    throw new StatementException(tokens, $"No precision specified for decimal value");
                int precision = tokens.Next<TokenIntegerLiteral>();
                string decimalName = tokens.Next<TokenStringLiteral>();
                ScoreboardValueDecimal decimalValue = new ScoreboardValueDecimal
                    (decimalName, precision, executor.scoreboard);
                executor.scoreboard.Add(decimalValue);
                return;
            }

            string name = tokens.Next<TokenStringLiteral>();
            ScoreboardValue value;

            if (type == TYPE_INT)
                value = new ScoreboardValueInteger(name, executor.scoreboard);
            else if (type == TYPE_BOOL)
                value = new ScoreboardValueBoolean(name, executor.scoreboard);
            else if (type == TYPE_TIME)
                value = new ScoreboardValueTime(name, executor.scoreboard);
            else
                throw new StatementException(tokens, $"Variable type corrupted for '{name}'.");

            executor.scoreboard.Add(value);
        }
        public static void init(Executor executor, Statement tokens)
        {
            TokenIdentifierValue _value = tokens.Next<TokenIdentifierValue>();
            executor.AddCommands(_value.value.CommandsInit());
        }
        public static void @if(Executor executor, Statement tokens) =>
            @if(executor, tokens, false);
        public static void @if(Executor executor, Statement tokens, bool invert)
        {
            Selector selector = new Selector();
            selector.core = executor.ActiveSelector;
            Token[] tokensUsed = tokens.GetRemainingTokens();

            executor.scoreboard.PushTempState();
            List<string> commands = new List<string>();

            do
            {
                if (tokens.NextIs<TokenAnd>())
                    tokens.Next();

                string entity = executor.ActiveSelectorStr;
                bool not = invert;
                bool isScore = tokens.NextIs<TokenIdentifierValue>();
                TokenIdentifier currentToken = tokens.Next<TokenIdentifier>();
                string word = currentToken.word.ToUpper();

                // check for word "not"

                if (word.ToUpper().Equals("NOT"))
                {
                    not = !invert;
                    isScore = tokens.NextIs<TokenIdentifierValue>();
                    currentToken = tokens.Next<TokenIdentifier>();
                    word = currentToken.word.ToUpper();
                }

                if (isScore)
                {
                    TokenIdentifierValue a = currentToken as TokenIdentifierValue;

                    // if <boolean> {}
                    if (!tokens.HasNext || !tokens.NextIs<TokenCompare>())
                    {
                        selector.scores.checks.Add(new ScoresEntry(a.value, new Range(1, not)));
                    }
                    // if <value> <comp> <other>
                    else if (tokens.NextIs<TokenCompare>())
                    {
                        TokenCompare compare = tokens.Next<TokenCompare>();
                        TokenCompare.Type ctype = compare.GetCompareType();
                        
                        // invert the type (bad code on their part tbh)
                        if(not)
                            switch (ctype)
                            {
                                case TokenCompare.Type.EQUAL:
                                    ctype = TokenCompare.Type.NOT_EQUAL;
                                    break;
                                case TokenCompare.Type.NOT_EQUAL:
                                    ctype = TokenCompare.Type.EQUAL;
                                    break;
                                case TokenCompare.Type.LESS_THAN:
                                    ctype = TokenCompare.Type.GREATER_OR_EQUAL;
                                    break;
                                case TokenCompare.Type.LESS_OR_EQUAL:
                                    ctype = TokenCompare.Type.GREATER_THAN;
                                    break;
                                case TokenCompare.Type.GREATER_THAN:
                                    ctype = TokenCompare.Type.LESS_OR_EQUAL;
                                    break;
                                case TokenCompare.Type.GREATER_OR_EQUAL:
                                    ctype = TokenCompare.Type.LESS_THAN;
                                    break;
                                default:
                                    break;
                            }

                        // if <value> <comp> identifier
                        if (tokens.NextIs<TokenIdentifierValue>())
                        {
                            TokenIdentifierValue b = tokens.Next<TokenIdentifierValue>();
                            ScoreboardValue temp = executor.scoreboard.RequestTemp(a.value);
                            commands.AddRange(temp.CommandsSet(entity, a.value, a.word, b.word));
                            commands.AddRange(temp.CommandsSub(entity, b.value, a.word, b.word));
                            Range check;

                            switch (ctype)
                            {
                                case TokenCompare.Type.EQUAL:
                                    check = new Range(0, false);
                                    break;
                                case TokenCompare.Type.NOT_EQUAL:
                                    check = new Range(0, true);
                                    break;
                                case TokenCompare.Type.LESS_THAN:
                                    check = new Range(null, -1);
                                    break;
                                case TokenCompare.Type.LESS_OR_EQUAL:
                                    check = new Range(null, 0);
                                    break;
                                case TokenCompare.Type.GREATER_THAN:
                                    check = new Range(1, null);
                                    break;
                                case TokenCompare.Type.GREATER_OR_EQUAL:
                                    check = new Range(0, null);
                                    break;
                                default:
                                    check = new Range();
                                    break;
                            }
                            selector.scores.checks.Add(new ScoresEntry(temp, check));
                            // if <vale> <comp> 1234.5
                        }
                        else if (tokens.NextIs<TokenNumberLiteral>())
                        {
                            TokenNumberLiteral number = tokens.Next<TokenNumberLiteral>();
                            var output = a.value.CompareToLiteral(a.word, entity, ctype, number);
                            selector.scores.checks.AddRange(output.Item1);
                            commands.AddRange(output.Item2);
                        }
                        else throw new StatementException(tokens, "Attempted to compare value with invalid token.");
                    }
                    else break;
                }

                if (word.Equals("BLOCK"))
                {
                    Coord x = tokens.Next<TokenCoordinateLiteral>();
                    Coord y = tokens.Next<TokenCoordinateLiteral>();
                    Coord z = tokens.Next<TokenCoordinateLiteral>();
                    string block = tokens.Next<TokenStringLiteral>();
                    int? data = null;

                    if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                        data = tokens.Next<TokenIntegerLiteral>();

                    BlockCheck blockCheck = new BlockCheck(x, y, z, block, data);

                    if (not)
                    {
                        ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                        commands.AddRange(new[] {
                            Command.ScoreboardSet(entity, inverter, 0),
                            blockCheck.AsStoreIn(entity, inverter)
                        });
                        selector.blockCheck = BlockCheck.DISABLED;
                        selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                    }
                    else
                        selector.blockCheck = blockCheck;
                }
                else if (word.Equals("NEAR"))
                {
                    Coord x = tokens.Next<TokenCoordinateLiteral>();
                    Coord y = tokens.Next<TokenCoordinateLiteral>();
                    Coord z = tokens.Next<TokenCoordinateLiteral>();
                    int radius = tokens.Next<TokenIntegerLiteral>();

                    int? minRadius = null;
                    if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                        minRadius = tokens.Next<TokenIntegerLiteral>();

                    Area area = new Area(x, y, z, minRadius, radius);

                    if (not && minRadius != null)
                    {
                        ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                        commands.AddRange(new[] {
                            Command.ScoreboardSet(entity, inverter, 0),
                            area.AsStoreIn(entity, inverter)
                        });
                        selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                    }
                    else if (not)
                    {
                        area.radiusMin = area.radiusMax;
                        area.radiusMax = 999999999f;
                        selector.area = area;
                    }
                    else
                        selector.area = area;
                }
                else if (word.Equals("INSIDE"))
                {
                    Coord x = tokens.Next<TokenCoordinateLiteral>();
                    Coord y = tokens.Next<TokenCoordinateLiteral>();
                    Coord z = tokens.Next<TokenCoordinateLiteral>();
                    int sizeX = tokens.Next<TokenIntegerLiteral>();
                    int sizeY = tokens.Next<TokenIntegerLiteral>();
                    int sizeZ = tokens.Next<TokenIntegerLiteral>();

                    Area area = new Area(x, y, z, null, null, sizeX, sizeY, sizeZ);

                    if (not)
                    {
                        ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                        commands.AddRange(new[] {
                            Command.ScoreboardSet(entity, inverter, 0),
                            area.AsStoreIn(entity, inverter)
                        });
                        selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                    }
                    else
                        selector.area = area;
                }
                else if (word.Equals("TYPE"))
                {
                    string type = tokens.Next<TokenStringLiteral>();
                    if (not) type = '!' + type;
                    selector.entity.type = type;
                }
                else if (word.Equals("FAMILY"))
                {
                    string family = tokens.Next<TokenStringLiteral>();
                    if (not) family = '!' + family;
                    selector.entity.type = family;
                }
                else if (word.Equals("TAG"))
                {
                    string tag = tokens.Next<TokenStringLiteral>();
                    selector.tags.Add(new Tag(tag, not));
                }
                else if (word.Equals("MODE"))
                {
                    GameMode gameMode;

                    if (tokens.NextIs<TokenIdentifierEnum>())
                        gameMode = (GameMode)tokens.Next<TokenIdentifierEnum>().value;
                    else
                        gameMode = (GameMode)tokens.Next<TokenIntegerLiteral>().number;

                    selector.player.gamemode = gameMode;
                    selector.player.gamemodeNot = not;
                }
                else if (word.Equals("LEVEL"))
                {
                    int levelMin = tokens.Next<TokenIntegerLiteral>();
                    int? levelMax = null;

                    if (tokens.HasNext)
                        levelMax = tokens.Next<TokenIntegerLiteral>();

                    if (not && levelMax == null)
                    {
                        selector.player.levelMin = 0;
                        selector.player.levelMax = levelMin;
                    }
                    else if (not)
                    {
                        Player invertCondition = new Player(null, levelMin, levelMax);
                        ScoreboardValue inverter = executor.scoreboard.RequestTemp();
                        commands.AddRange(new[] {
                            Command.ScoreboardSet(entity, inverter, 0),
                            invertCondition.AsStoreIn(entity, inverter)
                        });
                        selector.scores.checks.Add(new ScoresEntry(inverter, new Range(0, false)));
                    }
                    else
                    {
                        selector.player.levelMin = levelMin;
                        selector.player.levelMax = levelMax;
                    }
                }
                else if (word.Equals("NAME"))
                {
                    string name = tokens.Next<TokenStringLiteral>();
                    if (not) name = '!' + name;
                    selector.entity.name = name;
                }
                else if (word.Equals("LIMIT"))
                {
                    if (not)
                        throw new StatementException(tokens, "Cannot invert a limit check.");

                    int count = tokens.Next<TokenIntegerLiteral>();
                    selector.count = new Count(count);
                }
            // repeat all that as long as there continues to be an &
            } while (tokens.HasNext && tokens.NextIs<TokenAnd>());

            if (commands.Count > 0)
                executor.AddCommands(commands);

            // done with temporary variables
            executor.scoreboard.PopTempState();

            // the selector is now ready to use and commands are setup
            executor.SetLastCompare(tokensUsed);
            string prefix = selector.GetAsPrefix();
            executor.SetCommandPrepend(prefix);

            if (executor.NextIs<StatementOpenBlock>())
            {
                CommandFile nextBranchFile = StatementOpenBlock.GetNextBranchFile();
                StatementOpenBlock openBlock = executor.Peek<StatementOpenBlock>();
                openBlock.aligns = true;
                openBlock.shouldRun = true;
                openBlock.TargetFile = nextBranchFile;
                executor.AddCommand(Command.Function(nextBranchFile));
                return;
            } else
            {
                executor.PushSelector(true);
                executor.PopSelectorAfterNext();
            }
        }
        public static void @else(Executor executor, Statement tokens)
        {
            Token[] toRun = executor.GetLastCompare();
            @if(executor, new StatementDirective(null, toRun), true);
        }
        public static void give(Executor executor, Statement tokens)
        {
            string itemName = tokens.Next<TokenStringLiteral>();
            bool needsStructure = false;

            int count = 1;
            int data = 0;
            bool keep = false;
            bool lockInventory = false;
            bool lockSlot = false;
            List<string> canPlaceOn = new List<string>();
            List<string> canDestroy = new List<string>();
            List<Tuple<Enchantment, int>> enchants = new List<Tuple<Enchantment, int>>();
            string displayName = null;


            if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
            {
                count = tokens.Next<TokenIntegerLiteral>();

                if(tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                    data = tokens.Next<TokenIntegerLiteral>();
            }

            while(tokens.HasNext && tokens.NextIs<TokenBuilderIdentifier>())
            {
                TokenBuilderIdentifier builderIdentifier = tokens.Next<TokenBuilderIdentifier>();
                string builderField = builderIdentifier.builderField.ToUpper();

                switch(builderField)
                {
                    case "KEEP":
                        keep = true;
                        needsStructure = true;
                        break;
                    case "LOCKINVENTORY":
                        lockInventory = true;
                        needsStructure = true;
                        break;
                    case "LOCKSLOT":
                        lockSlot = true;
                        needsStructure = true;
                        break;
                    case "CANPLACEON":
                        canPlaceOn.Add(tokens.Next<TokenStringLiteral>());
                        break;
                    case "CANDESTROY":
                        canDestroy.Add(tokens.Next<TokenStringLiteral>());
                        break;
                    case "ENCHANT":
                        Enchantment enchantment = (Enchantment)tokens.Next<TokenIdentifierEnum>().value;
                        int level = tokens.Next<TokenIntegerLiteral>();
                        enchants.Add(new Tuple<Enchantment, int>(enchantment, level));
                        needsStructure = true;
                        break;
                    case "NAME":
                        displayName = tokens.Next<TokenStringLiteral>();
                        needsStructure = true;
                        break;
                    default:
                        break;
                }
            }

            // create a structure file since this item is too complex
            if(needsStructure)
            {
                ItemStack item = new ItemStack()
                {
                    id = itemName,
                    count = count,
                    damage = data,
                    keep = keep,
                    lockMode = lockInventory ? NBT.ItemLockMode.LOCK_IN_INVENTORY :
                        lockSlot ? NBT.ItemLockMode.LOCK_IN_SLOT : NBT.ItemLockMode.NONE,
                    displayName = displayName,
                    enchantments = enchants.Select(e => new EnchantmentEntry(e.Item1, e.Item2)).ToArray(),
                    canPlaceOn = canPlaceOn.ToArray(),
                    canDestroy = canDestroy.ToArray()
                };
                StructureFile file = new StructureFile(item.GenerateUID(), StructureNBT.SingleItem(item));
                executor.AddExtraFile(file);
                Selector.Core core = executor.ActiveSelector;

                string cmd = Command.StructureLoad(file.name, Coord.here, Coord.here, Coord.here,
                    StructureRotation._0_degrees, StructureMirror.none, true, false);

                if(core != Selector.Core.s && core != Selector.Core.p)
                    executor.AddCommand(Command.Execute("@" + core, Coord.here, Coord.here, Coord.here, cmd));
                else
                    executor.AddCommand(cmd);
            }

            List<string> json = new List<string>();

            if (keep)
                json.Add("\"keep_on_death\":{}");

            if (lockSlot)
                json.Add("\"item_lock\":{\"mode\":\"lock_in_slot\"}");
            else if (lockInventory)
                json.Add("\"item_lock\":{\"mode\":\"lock_in_inventory\"}");

            if (canPlaceOn.Count > 0)
            {
                string blocks = string.Join(",", canPlaceOn.Select(c => $"\"{c}\""));
                json.Add($"\"minecraft:can_place_on\":{{\"blocks\":[{blocks}]}}");
            }
            if (canDestroy.Count > 0)
            {
                string blocks = string.Join(",", canDestroy.Select(c => $"\"{c}\""));
                json.Add($"\"minecraft:can_destroy\":{{\"blocks\":[{blocks}]}}");
            }

            string command = Command.Give(executor.ActiveSelectorStr, itemName, count, data);
            if(json.Count > 0)
                command += $" {{{string.Join(",", json)}}}";

            executor.AddCommand(command);
        }
        public static void tp(Executor executor, Statement tokens)
        {
            if(tokens.NextIs<TokenSelectorLiteral>())
            {
                TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
                executor.AddCommand(Command.Teleport(selector.selector.ToString()));
            } else
            {
                Coord x = tokens.Next<TokenCoordinateLiteral>();
                Coord y = tokens.Next<TokenCoordinateLiteral>();
                Coord z = tokens.Next<TokenCoordinateLiteral>();

                if(tokens.HasNext && tokens.NextIs<TokenCoordinateLiteral>())
                {
                    Coord ry = tokens.Next<TokenCoordinateLiteral>();
                    Coord rx = tokens.Next<TokenCoordinateLiteral>();
                    executor.AddCommand(Command.Teleport(x, y, z, ry, rx));
                }
                else
                    executor.AddCommand(Command.Teleport(x, y, z));
            }
        }
        public static void tphere(Executor executor, Statement tokens)
        {
            Selector selector = tokens.Next<TokenSelectorLiteral>();
            Coord offsetX = Coord.here;
            Coord offsetY = Coord.here;
            Coord offsetZ = Coord.here;

            if(tokens.HasNext && tokens.NextIs<TokenCoordinateLiteral>())
            {
                offsetX = tokens.Next<TokenCoordinateLiteral>();
                offsetY = tokens.Next<TokenCoordinateLiteral>();
                offsetZ = tokens.Next<TokenCoordinateLiteral>();
            }

            executor.AddCommand(Command.Teleport(selector.ToString(), offsetX, offsetY, offsetZ));
        }
        public static void move(Executor executor, Statement tokens)
        {
            string direction = tokens.Next<TokenIdentifier>().word.ToUpper();
            float amount = tokens.Next<TokenNumberLiteral>().GetNumber();

            Coord x = Coord.here;
            Coord y = Coord.here;
            Coord z = Coord.here;

            switch(direction)
            {
                case "LEFT":
                    x = new Coord(amount, true, false, true);
                    break;
                case "RIGHT":
                    x = new Coord(-amount, true, false, true);
                    break;
                case "UP":
                    y = new Coord(amount, true, false, true);
                    break;
                case "DOWN":
                    y = new Coord(-amount, true, false, true);
                    break;
                case "FORWARD":
                case "FORWARDS":
                    z = new Coord(amount, true, false, true);
                    break;
                case "BACKWARD":
                case "BACKWARDS":
                    z = new Coord(-amount, true, false, true);
                    break;
            }

            executor.AddCommand(Command.Teleport(x, y, z));
        }
        public static void face(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenSelectorLiteral>())
            {
                TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
                executor.AddCommand(Command.TeleportFacing(Coord.here, Coord.here, Coord.here, selector.ToString()));
            }
            else
            {
                Coord x = tokens.Next<TokenCoordinateLiteral>();
                Coord y = tokens.Next<TokenCoordinateLiteral>();
                Coord z = tokens.Next<TokenCoordinateLiteral>();
                executor.AddCommand(Command.TeleportFacing(Coord.here, Coord.here, Coord.here, x, y, z));
            }
        }
        public static void facehere(Executor executor, Statement tokens)
        {
            TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
            List<string> commands = new List<string>();
            commands.AddRange(Command.UTIL.RequestPoint(out Selector point));
            commands.Add(Command.Execute(selector.ToString(), Coord.here, Coord.here, Coord.here,
                Command.TeleportFacing(Coord.here, Coord.here, Coord.here, point.ToString())));
            commands.AddRange(Command.UTIL.ReleasePoint());
            commands.AddRange(commands);
        }
        public static void rotate(Executor executor, Statement tokens)
        {
            TokenNumberLiteral number = tokens.Next<TokenNumberLiteral>();
            Coord ry, rx = Coord.here;

            if (number is TokenDecimalLiteral)
                ry = new Coord(number.GetNumber(), true, true, false);
            else
                ry = new Coord(number.GetNumberInt(), false, true, false);

            if(tokens.HasNext && tokens.NextIs<TokenNumberLiteral>())
            {
                number = tokens.Next<TokenNumberLiteral>();
                if (number is TokenDecimalLiteral)
                    rx = new Coord(number.GetNumber(), true, true, false);
                else
                    rx = new Coord(number.GetNumberInt(), false, true, false);
            }

            executor.AddCommand(Command.Teleport(Coord.here, Coord.here, Coord.here, ry, rx));
        }
        public static void block(Executor executor, Statement tokens)
        {
            OldObjectHandling handling = OldObjectHandling.replace;
            
            if(tokens.NextIs<TokenIdentifierEnum>())
                handling = (OldObjectHandling)tokens.Next<TokenIdentifierEnum>().value;

            string block = tokens.Next<TokenStringLiteral>();
            Coord x = tokens.Next<TokenCoordinateLiteral>();
            Coord y = tokens.Next<TokenCoordinateLiteral>();
            Coord z = tokens.Next<TokenCoordinateLiteral>();

            int data = 0;
            if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                data = tokens.Next<TokenIntegerLiteral>();

            executor.AddCommand(Command.SetBlock(x, y, z, block, data, handling));
        }
        public static void fill(Executor executor, Statement tokens)
        {
            OldObjectHandling handling = OldObjectHandling.replace;

            if (tokens.NextIs<TokenIdentifierEnum>())
                handling = (OldObjectHandling)tokens.Next<TokenIdentifierEnum>().value;

            string block = tokens.Next<TokenStringLiteral>();
            Coord x1 = tokens.Next<TokenCoordinateLiteral>();
            Coord y1 = tokens.Next<TokenCoordinateLiteral>();
            Coord z1 = tokens.Next<TokenCoordinateLiteral>();
            Coord x2 = tokens.Next<TokenCoordinateLiteral>();
            Coord y2 = tokens.Next<TokenCoordinateLiteral>();
            Coord z2 = tokens.Next<TokenCoordinateLiteral>();

            int data = 0;
            if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                data = tokens.Next<TokenIntegerLiteral>();

            executor.AddCommand(Command.Fill(x1, y1, z1, x2, y2, z2, block, data, handling));
        }
        public static void scatter(Executor executor, Statement tokens)
        {
            string block = tokens.Next<TokenStringLiteral>();
            int percent = tokens.Next<TokenIntegerLiteral>();
            int data = tokens.Next<TokenIntegerLiteral>();
            Coord x1 = tokens.Next<TokenCoordinateLiteral>();
            Coord y1 = tokens.Next<TokenCoordinateLiteral>();
            Coord z1 = tokens.Next<TokenCoordinateLiteral>();
            Coord x2 = tokens.Next<TokenCoordinateLiteral>();
            Coord y2 = tokens.Next<TokenCoordinateLiteral>();
            Coord z2 = tokens.Next<TokenCoordinateLiteral>();

            string seed = null;
            if (tokens.HasNext && tokens.NextIs<TokenStringLiteral>())
                seed = tokens.Next<TokenStringLiteral>();

            // generate a structure file for this zone.
            if (Program.DEBUG)
                Console.WriteLine("Attempting to build scatter file... This may take a couple minutes.");
            int sizeX = Math.Abs(x2.valuei - x1.valuei);
            int sizeY = Math.Abs(y2.valuei - y1.valuei);
            int sizeZ = Math.Abs(z2.valuei - z1.valuei);
            if (sizeX > 64 || sizeY > 256 || sizeZ > 64)
                throw new StatementException(tokens, "Scatter zone size cannot be larger than 64x256x64.");

            int[,,] blocks = new int[sizeX, sizeY, sizeZ];
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    for (int z = 0; z < sizeZ; z++)
                        blocks[x, y, z] = 0;

            StructureNBT structure = new StructureNBT()
            {
                formatVersion = 1,
                size = new VectorIntNBT(sizeX, sizeY, sizeZ),
                worldOrigin = new VectorIntNBT(0, 0, 0),

                palette = new PaletteNBT(new PaletteEntryNBT(block)),
                entities = new EntityListNBT(new EntityNBT[0]),
                indices = new BlockIndicesNBT(blocks)
            };
            string fileName = "scatter_" + scatterFile++;
            StructureFile file = new StructureFile(fileName, structure);
            executor.WriteFileNow(file);

            blocks = null;
            structure = new StructureNBT();
            file = new StructureFile();

            Console.WriteLine("Cleaning up from scatter file...");
            GC.Collect();

            Coord minX = Coord.Min(x1, x2);
            Coord minY = Coord.Min(y1, y2);
            Coord minZ = Coord.Min(z1, z2);

            if(seed == null)
            {
                executor.AddCommand(Command.StructureLoad(fileName, minX, minY, minZ,
                    StructureRotation._0_degrees, StructureMirror.none, false, true, percent));
            } else
            {
                executor.AddCommand(Command.StructureLoad(fileName, minX, minY, minZ,
                    StructureRotation._0_degrees, StructureMirror.none, false, true, percent, seed));
            }
        }
        public static void replace(Executor executor, Statement tokens)
        {
            string src = tokens.Next<TokenStringLiteral>();
            int srcData = -1;
            if (tokens.NextIs<TokenIntegerLiteral>())
                srcData = tokens.Next<TokenIntegerLiteral>();

            Coord x1 = tokens.Next<TokenCoordinateLiteral>();
            Coord y1 = tokens.Next<TokenCoordinateLiteral>();
            Coord z1 = tokens.Next<TokenCoordinateLiteral>();
            Coord x2 = tokens.Next<TokenCoordinateLiteral>();
            Coord y2 = tokens.Next<TokenCoordinateLiteral>();
            Coord z2 = tokens.Next<TokenCoordinateLiteral>();

            string dst = tokens.Next<TokenStringLiteral>();
            int dstData = -1;
            if (tokens.HasNext && tokens.NextIs<TokenIntegerLiteral>())
                dstData = tokens.Next<TokenIntegerLiteral>();

            executor.AddCommand(Command.Fill(x1, y1, z1, x2, y2, z2, src, srcData, dst, dstData));
        }
        public static void kill(Executor executor, Statement tokens)
        {
            if(tokens.HasNext && tokens.NextIs<TokenSelectorLiteral>())
            {
                Selector selector = tokens.Next<TokenSelectorLiteral>();
                executor.AddCommand(Command.Kill(selector.ToString()));
                return;
            }
            executor.AddCommand(Command.Kill());
        }
        public static void remove(Executor executor, Statement tokens)
        {
            if (tokens.HasNext && tokens.NextIs<TokenSelectorLiteral>())
            {
                Selector selector = tokens.Next<TokenSelectorLiteral>();
                CommandFile file = new CommandFile("silent_remove", "_branching");
                file.Add(new[] {
                    Command.Teleport(Coord.here, new Coord(-9999, false, true, false), Coord.here),
                    Command.Kill()
                });
                executor.DefineSTDFile(file);
                executor.AddCommand(Command.Execute(selector.ToString(),
                    Coord.here, Coord.here, Coord.here, Command.Function(file)));
                return;
            }

            executor.AddCommands(new[] {
                Command.Teleport(Coord.here, new Coord(-9999, false, true, false), Coord.here),
                Command.Kill()
            });
        }
        public static void globaltitle(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                RawTextJsonBuilder builder = new RawTextJsonBuilder();
                builder.AddTerms(executor.FString(str));

                string output = builder.BuildString();
                executor.AddCommand(Command.Title("@a", output));
                return;
            }

            string word = tokens.Next<TokenIdentifier>().word.ToUpper();
            if (word.Equals("TIMES"))
            {
                int fadeIn = tokens.Next<TokenIntegerLiteral>();
                int stay = tokens.Next<TokenIntegerLiteral>();
                int fadeOut = tokens.Next<TokenIntegerLiteral>();
                executor.AddCommand(Command.TitleTimes("@a", fadeIn, stay, fadeOut));
                return;
            } else if (word.Equals("SUBTITLE"))
            {
                string str = tokens.Next<TokenStringLiteral>();
                RawTextJsonBuilder builder = new RawTextJsonBuilder();
                builder.AddTerms(executor.FString(str));
                string output = builder.BuildString();
                executor.AddCommand(Command.TitleSubtitle("@a", output));
                return;
            } else
                throw new StatementException(tokens, $"Invalid globaltitle subcommand '{word}'. Must be 'times' or 'subtitle'.");
        }
        public static void title(Executor executor, Statement tokens)
        {
            string selector = executor.ActiveSelectorStr;

            if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                RawTextJsonBuilder builder = new RawTextJsonBuilder();
                builder.AddTerms(executor.FString(str));

                string output = builder.BuildString();
                executor.AddCommand(Command.Title(selector, output));
                return;
            }

            string word = tokens.Next<TokenIdentifier>().word.ToUpper();
            if (word.Equals("TIMES"))
            {
                int fadeIn = tokens.Next<TokenIntegerLiteral>();
                int stay = tokens.Next<TokenIntegerLiteral>();
                int fadeOut = tokens.Next<TokenIntegerLiteral>();
                executor.AddCommand(Command.TitleTimes(selector, fadeIn, stay, fadeOut));
                return;
            }
            else if (word.Equals("SUBTITLE"))
            {
                string str = tokens.Next<TokenStringLiteral>();
                RawTextJsonBuilder builder = new RawTextJsonBuilder();
                builder.AddTerms(executor.FString(str));
                string output = builder.BuildString();
                executor.AddCommand(Command.TitleSubtitle(selector, output));
                return;
            }
            else
                throw new StatementException(tokens, $"Invalid title subcommand '{word}'. Must be 'times' or 'subtitle'.");
        }
        public static void globalactionbar(Executor executor, Statement tokens)
        {
            if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                RawTextJsonBuilder builder = new RawTextJsonBuilder();
                builder.AddTerms(executor.FString(str));

                string output = builder.BuildString();
                executor.AddCommand(Command.TitleActionBar("@a", output));
                return;
            }

            string word = tokens.Next<TokenIdentifier>().word.ToUpper();
            int fadeIn = tokens.Next<TokenIntegerLiteral>();
            int stay = tokens.Next<TokenIntegerLiteral>();
            int fadeOut = tokens.Next<TokenIntegerLiteral>();
            executor.AddCommand(Command.TitleTimes("@a", fadeIn, stay, fadeOut));
            return;
        }
        public static void actionbar(Executor executor, Statement tokens)
        {
            string selector = executor.ActiveSelectorStr;

            if (tokens.NextIs<TokenStringLiteral>())
            {
                string str = tokens.Next<TokenStringLiteral>();
                RawTextJsonBuilder builder = new RawTextJsonBuilder();
                builder.AddTerms(executor.FString(str));

                string output = builder.BuildString();
                executor.AddCommand(Command.TitleActionBar(selector, output));
                return;
            }

            string word = tokens.Next<TokenIdentifier>().word.ToUpper();
            int fadeIn = tokens.Next<TokenIntegerLiteral>();
            int stay = tokens.Next<TokenIntegerLiteral>();
            int fadeOut = tokens.Next<TokenIntegerLiteral>();
            executor.AddCommand(Command.TitleTimes(selector, fadeIn, stay, fadeOut));
            return;
        }
        public static void halt(Executor executor, Statement tokens)
        {
            CommandFile file = new CommandFile("halt_execution", "_misc");

            if (!executor.HasSTDFile(file))
            {
                // recursively call self until function command limit reached
                file.Add(Command.Function(file));
                executor.DefineSTDFile(file);
            }

            executor.AddCommand(Command.Function(file));
        }
    }
}