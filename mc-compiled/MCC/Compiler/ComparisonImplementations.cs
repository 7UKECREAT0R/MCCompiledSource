using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Native;
using mc_compiled.Commands.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Compares if a boolean value is true.
    /// </summary>
    public class ComparisonBoolean : Comparison
    {
        readonly ScoreboardValueBoolean boolean;
        public ComparisonBoolean(ScoreboardValueBoolean boolean, bool invert) : base(invert)
        {
            this.boolean = boolean;
        }
        public override IEnumerable<string> GetCommands(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            cancel = false;
            return null;
        }
        public override Subcommand[] GetExecuteChunks(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            cancel = false;

            string name = boolean.Name;
            Range range = new Range(1, inverted);

            return new Subcommand[] {
                new SubcommandIf(ConditionalSubcommandScore.New(boolean, range))
            };
        }

        public override string GetDescription() => inverted?
            $"{boolean.AliasName} ({boolean.Name}) is false.":
            $"{boolean.AliasName} ({boolean.Name}) is true.";
    }

    /// <summary>
    /// Compares two values.
    /// </summary>
    public class ComparisonValue : Comparison
    {
        public Token LeftToken
        {
            get => a;
        }
        public TokenCompare.Type ComparisonMethod
        {
            get => comparison;
        }
        public Token RightToken
        {
            get => b;
        }

        Token a;
        TokenCompare.Type comparison;
        Token b;
        SideType aType;
        SideType bType;
        bool BothValues
        {
            get => aType == SideType.Variable && bType == SideType.Variable;
        }
        bool RequiresSwap
        {
            get => aType == SideType.Constant && bType == SideType.Variable;
        }

        void FindAType()
        {
            if (a is TokenIdentifierValue)
                aType = SideType.Variable;
            else if (a is TokenNumberLiteral)
                aType = SideType.Constant;
            else
                aType = SideType.Unknown;
        }
        void FindBType()
        {
            if (b is TokenIdentifierValue)
                bType = SideType.Variable;
            else if (b is TokenNumberLiteral)
                bType = SideType.Constant;
            else
                bType = SideType.Unknown;
        }
        public ComparisonValue(Token a, TokenCompare.Type comparison, Token b, bool invert) : base(invert)
        {
            this.a = a;
            this.b = b;
            this.comparison = comparison;

            // get types of each side
            FindAType();
            FindBType();

            // swap for code simplicity
            if (RequiresSwap)
            {
                SideType temp = bType;
                bType = aType;
                aType = temp;
                this.a = b;
                this.b = a;
                this.comparison = comparison.InvertDirection();
            }
        }

        private Tuple<ScoresEntry[], string[]> entries;
        private ScoreboardValue temp;

        public override IEnumerable<string> GetCommands(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            cancel = false;

            if (aType == SideType.Unknown)
                throw new StatementException(callingStatement, $"Unexpected token on left-hand side of condition: {a.AsString()}");
            if (bType == SideType.Unknown)
                throw new StatementException(callingStatement, $"Unexpected token on right-hand side of condition: {b.AsString()}");

            var localComparison = this.comparison;
            if (inverted)
                localComparison = localComparison.InvertComparison();

            if (aType == SideType.Variable && bType == SideType.Variable)
            {
                ScoreboardValue a = (this.a as TokenIdentifierValue).value;
                ScoreboardValue b = (this.b as TokenIdentifierValue).value;
                List<string> commands = new List<string>();

                if (willBeInverted)
                {
                    temp = executor.scoreboard.temps.Request(true);
                    commands.Add(Command.ScoreboardSet(temp, 0));

                    if (localComparison == TokenCompare.Type.NOT_EQUAL)
                    {
                        commands.Add(Command.Execute()
                            .UnlessScore(a, TokenCompare.Type.EQUAL, b)
                            .Run(Command.ScoreboardSet(temp, 1)));
                    }
                    else
                    {
                        commands.Add(Command.Execute()
                            .IfScore(a, localComparison, b)
                            .Run(Command.ScoreboardSet(temp, 1)));
                    }

                    return commands;
                }
            }

            if (aType == SideType.Variable && bType == SideType.Constant)
            {
                ScoreboardValue a = (this.a as TokenIdentifierValue).value;
                TokenNumberLiteral b = (this.b as TokenNumberLiteral);
                this.entries = a.CompareToLiteral(localComparison, b);

                if(willBeInverted)
                {
                    List<string> commands = new List<string>(this.entries.Item2);
                    temp = executor.scoreboard.temps.Request(true);
                    commands.Add(Command.ScoreboardSet(temp, 0));

                    ExecuteBuilder builder = Command.Execute();
                    foreach (ScoresEntry entry in this.entries.Item1)
                    {
                        // retrieve the scoreboard value
                        string scoreName = entry.name;

                        if (!executor.scoreboard.TryGetByName(scoreName, out ScoreboardValue score))
                            throw new StatementException(callingStatement, $"Unknown scoreboard value: '{scoreName}'. Is it defined above this line?");

                        builder.IfScore(score, entry.value);
                    }

                    commands.Add(builder.Run(Command.ScoreboardSet(temp, 1)));
                    return commands;
                }

                return this.entries.Item2;
            }

            return null;
        }
        public override Subcommand[] GetExecuteChunks(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            var localComparison = this.comparison;
            if(inverted)
                localComparison = localComparison.InvertComparison();

            cancel = false;

            if (BothValues)
            {
                // "value value" comparison
                ScoreboardValue a = (this.a as TokenIdentifierValue).value;
                ScoreboardValue b = (this.b as TokenIdentifierValue).value;

                if(willBeInverted)
                {
                    return new Subcommand[] {
                        new SubcommandIf(ConditionalSubcommandScore.New(temp, new Range(1, false)))
                    };
                }
                else
                {
                    if (localComparison == TokenCompare.Type.NOT_EQUAL)
                    {
                        return new Subcommand[] {
                            // mojang doesn't support != so invert the root of the comparison
                            new SubcommandUnless(ConditionalSubcommandScore.New(a, TokenCompare.Type.EQUAL, b))
                        };
                    }

                    return new Subcommand[] {
                        new SubcommandIf(ConditionalSubcommandScore.New(a, localComparison, b))
                    };
                }

            }
            
            if(aType == SideType.Variable)
            {
                if (willBeInverted)
                {
                    return new Subcommand[] {
                        new SubcommandIf(ConditionalSubcommandScore.New(temp, new Range(1, false)))
                    };
                }
                else
                {
                    // "value constant" comparison
                    List<Subcommand> scores = new List<Subcommand>(this.entries.Item1.Length);

                    foreach (ScoresEntry scoreTest in this.entries.Item1)
                    {
                        string scoreName = scoreTest.name;

                        if (!executor.scoreboard.TryGetByName(scoreName, out ScoreboardValue score))
                            throw new StatementException(callingStatement, $"Unknown scoreboard value: '{scoreName}'. Is it defined above this line?");

                        scores.Add(new SubcommandIf(ConditionalSubcommandScore.New(score, scoreTest.value)));
                    }

                    return scores.ToArray();
                }
            }

            // "constant constant" comparison.
            TokenNumberLiteral numberA = this.a as TokenNumberLiteral;
            TokenNumberLiteral numberB = this.b as TokenNumberLiteral;

            bool result = numberA.CompareWithOther(localComparison, numberB);
            cancel = !result;
            return null;
        }

        public override string GetDescription() => inverted?
            $"{a.AsString()} is not {comparison.ToString().Replace("_", "").ToLower()} to {b.AsString()}":
            $"{a.AsString()} is {comparison.ToString().Replace("_", "").ToLower()} to {b.AsString()}";
    }
    public class ComparisonSelector : Comparison
    {
        public const string ERROR_MESSAGE = "[if <selector>] format should only use @s selectors. Use [if any <selector>] to more concisely show checking for a selector match.";
        public readonly Selector selector;

        public ComparisonSelector(Selector selector, bool invert) : base(invert)
        {
            this.selector = selector;
        }

        ScoreboardValue temp;
        public override IEnumerable<string> GetCommands(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            if (selector.core != Selector.Core.s)
                throw new StatementException(callingStatement, ERROR_MESSAGE);

            if (!willBeInverted && !inverted)
            {
                cancel = false;
                return null;
            }

            List<string> commands = new List<string>();

            temp = executor.scoreboard.temps.Request(true);
            commands.Add(Command.ScoreboardSet(temp, 0));
            commands.Add(Command.Execute()
                .As(selector)
                .Run(Command.ScoreboardSet(temp, 1)));

            cancel = false;
            return commands;
        }
        public override Subcommand[] GetExecuteChunks(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            if (selector.core != Selector.Core.s)
                throw new StatementException(callingStatement, ERROR_MESSAGE);

            cancel = false;

            if (willBeInverted || inverted)
            {
                Range range = new Range(1, inverted);
                return new Subcommand[] { new SubcommandIf(ConditionalSubcommandScore.New(temp, range)) };
            }

            return new Subcommand[] { new SubcommandAs(selector) };
        }

        public override string GetDescription() => inverted?
            $"{selector} does not match the executing entity":
            $"{selector} matches the executing entity";
    }
    public class ComparisonCount : Comparison
    {
        public readonly Selector selector;
        public TokenCompare.Type comparison;

        public readonly Token goalCount;
        public SideType goalType;

        public bool RequiresSubtraction
        {
            get => goalType == SideType.Variable;
        }
        void FindGoalType()
        {
            if (goalCount is TokenIdentifierValue)
                goalType = SideType.Variable;
            else if (goalCount is TokenNumberLiteral)
                goalType = SideType.Constant;
            else
                goalType = SideType.Unknown;
        }
        public ComparisonCount(Selector selector, TokenCompare.Type comparison, Token goalCount, bool invert) : base(invert)
        {
            this.selector = selector;
            this.comparison = comparison;
            this.goalCount = goalCount;

            FindGoalType();
        }

        ScoreboardValueInteger temp;
        public override IEnumerable<string> GetCommands(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            if (goalType == SideType.Unknown)
                throw new StatementException(callingStatement, $"Unexpected token on right-hand side of condition: {goalCount.AsString()}");

            // count number of entities
            List<string> commands = new List<string>();

            temp = executor.scoreboard.temps.Request(true);
            commands.Add(Command.ScoreboardSet(temp, 0));
            commands.Add(Command.Execute()
                .As(selector)
                .Run(Command.ScoreboardAdd(temp, 1)));

            cancel = false;
            return commands;
        }
        public override Subcommand[] GetExecuteChunks(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            cancel = false;

            var localComparison = this.comparison;
            if (inverted)
                localComparison = localComparison.InvertComparison();

            if (goalType == SideType.Variable)
            {
                ScoreboardValue b = (this.goalCount as TokenIdentifierValue).value;
                return new Subcommand[] { new SubcommandIf(ConditionalSubcommandScore.New(temp, localComparison, b)) };
            } else
            {
                TokenNumberLiteral _b = this.goalCount as TokenNumberLiteral;
                int b = _b.GetNumberInt();

                Range range;

                switch (localComparison)
                {
                    case TokenCompare.Type.EQUAL:
                        range = new Range(b, false);
                        break;
                    case TokenCompare.Type.NOT_EQUAL:
                        range = new Range(b, true);
                        break;
                    case TokenCompare.Type.LESS:
                        range = new Range(null, b - 1);
                        break;
                    case TokenCompare.Type.LESS_OR_EQUAL:
                        range = new Range(null, b);
                        break;
                    case TokenCompare.Type.GREATER:
                        range = new Range(b + 1, null);
                        break;
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                        range = new Range(b, null);
                        break;
                    default:
                        throw new StatementException(callingStatement, $"Unexpected comparison type: {localComparison}.");
                }

                return new Subcommand[] { new SubcommandIf(ConditionalSubcommandScore.New(temp, range)) };
            }
        }

        public override string GetDescription() => inverted?
            $"count of {selector} is not {comparison.ToString().Replace("_", "").ToLower()} to {goalCount.AsString()}":
            $"count of {selector} is {comparison.ToString().Replace("_", "").ToLower()} to {goalCount.AsString()}";
    }
    public class ComparisonAny : Comparison
    {
        public readonly Selector selector;

        public ComparisonAny(Selector selector, bool invert) : base(invert)
        {
            if(selector.count.count == 1)
                this.selector = selector;
            else
            {
                this.selector = new Selector(selector);
                this.selector.count.count = 1;
            }
        }

        ScoreboardValue temp;
        public override IEnumerable<string> GetCommands(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            if (!willBeInverted)
            {
                cancel = false;
                return null;
            }

            List<string> commands = new List<string>();

            temp = executor.scoreboard.temps.Request(true);
            commands.Add(Command.ScoreboardSet(temp, 0));
            commands.Add(Command.Execute()
                .IfEntity(selector)
                .Run(Command.ScoreboardSet(temp, 1)));

            cancel = false;
            return commands;
        }
        public override Subcommand[] GetExecuteChunks(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            cancel = false;

            if (willBeInverted)
            {
                Range range = new Range(1, inverted);
                return new Subcommand[] { new SubcommandIf(ConditionalSubcommandScore.New(temp, range)) };
            }

            if(inverted)
                return new Subcommand[] { new SubcommandUnless(ConditionalSubcommandEntity.New(selector)) };
            else
                return new Subcommand[] { new SubcommandIf(ConditionalSubcommandEntity.New(selector)) };

        }

        public override string GetDescription() => inverted ?
            $"{selector} does not match any entity":
            $"{selector} matches any entity";
    }
    public class ComparisonBlock : Comparison
    {
        readonly Coord x;
        readonly Coord y;
        readonly Coord z;
        readonly string block;
        readonly int? data;

        public ComparisonBlock(Coord x, Coord y, Coord z, string block, int? data, bool invert) : base(invert)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.block = block;
            this.data = data;
        }

        ScoreboardValue temp;
        public override IEnumerable<string> GetCommands(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            if (!willBeInverted)
            {
                cancel = false;
                return null;
            }

            List<string> commands = new List<string>();

            temp = executor.scoreboard.temps.Request(true);
            commands.Add(Command.ScoreboardSet(temp, 0));
            commands.Add(Command.Execute()
                .IfBlock(x, y, z, block, data)
                .Run(Command.ScoreboardSet(temp, 1)));

            cancel = false;
            return commands;
        }
        public override Subcommand[] GetExecuteChunks(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            cancel = false;

            if(willBeInverted)
            {
                Range range = new Range(1, inverted);
                return new Subcommand[] { new SubcommandIf(ConditionalSubcommandScore.New(temp, range)) };
            }

            if (inverted)
                return new Subcommand[] { new SubcommandUnless(ConditionalSubcommandBlock.New(x, y, z, block, data)) };
            else
                return new Subcommand[] { new SubcommandIf(ConditionalSubcommandBlock.New(x, y, z, block, data)) };
        }

        public override string GetDescription() => inverted?
            $"block at ({x} {y} {z}) is not {block}:{data}":
            $"block at ({x} {y} {z}) is {block}:{data}";
    }
    public class ComparisonBlocks : Comparison
    {
        readonly Coord beginX, beginY, beginZ;
        readonly Coord endX, endY, endZ;
        readonly Coord destX, destY, destZ;
        readonly BlocksScanMode scanMode;

        public ComparisonBlocks(Coord beginX, Coord beginY, Coord beginZ, Coord endX, Coord endY, Coord endZ,
            Coord destX, Coord destY, Coord destZ, BlocksScanMode scanMode, bool invert) : base(invert)
        {
            this.beginX = beginX;
            this.beginY = beginY;
            this.beginZ = beginZ;
            this.endX = endX;
            this.endY = endY;
            this.endZ = endZ;
            this.destX = destX;
            this.destY = destY;
            this.destZ = destZ;
            this.scanMode = scanMode;
        }

        ScoreboardValue temp;
        public override IEnumerable<string> GetCommands(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            if (!willBeInverted)
            {
                cancel = false;
                return null;
            }

            List<string> commands = new List<string>();

            temp = executor.scoreboard.temps.Request(true);
            commands.Add(Command.ScoreboardSet(temp, 0));
            commands.Add(Command.Execute()
                .IfBlocks(beginX, beginY, beginZ, endX, endY, endZ, destX, destY, destZ, scanMode)
                .Run(Command.ScoreboardSet(temp, 1)));

            cancel = false;
            return commands;
        }
        public override Subcommand[] GetExecuteChunks(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            cancel = false;

            if (willBeInverted)
            {
                Range range = new Range(1, inverted);
                return new Subcommand[] { new SubcommandIf(ConditionalSubcommandScore.New(temp, range)) };
            }

            if (inverted)
                return new Subcommand[] { new SubcommandUnless(ConditionalSubcommandBlocks.New(beginX, beginY, beginZ, endX, endY, endZ, destX, destY, destZ, scanMode)) };
            else
                return new Subcommand[] { new SubcommandIf(ConditionalSubcommandBlocks.New(beginX, beginY, beginZ, endX, endY, endZ, destX, destY, destZ, scanMode)) };
        }

        public override string GetDescription() => inverted ?
            $"{scanMode} blocks between ({beginX} {beginY} {beginZ}) and ({endX} {endY} {endZ}) do not match the blocks at ({destX} {destY} {destZ})" :
            $"{scanMode} blocks between ({beginX} {beginY} {beginZ}) and ({endX} {endY} {endZ}) match the blocks at ({destX} {destY} {destZ})";
    }

    /// <summary>
    /// The type of an operand in a comparison-based statement
    /// </summary>
    public enum SideType
    {
        Unknown, Constant, Variable
    }
}
