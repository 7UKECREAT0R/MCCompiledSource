using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Compares if a boolean value is true.
    /// </summary>
    public class ComparisonAlone : Comparison
    {
        private readonly ScoreboardValue score;
        public ComparisonAlone(ScoreboardValue score, bool invert) : base(invert)
        {
            this.score = score;
        }
        public override IEnumerable<string> GetCommands(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            cancel = false;
            return null;
        }
        public override Subcommand[] GetExecuteChunks(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            cancel = false;
            return score.CompareAlone(this.inverted).Cast<Subcommand>().ToArray();
        }

        public override string GetDescription() => inverted ?
            $"not {score.Name}.":
            $"{score.Name}.";
    }

    /// <summary>
    /// Compares two values.
    /// </summary>
    public class ComparisonValue : Comparison
    {
        private readonly Token a;
        private readonly TokenCompare.Type comparison;
        private readonly Token b;
        private SideType aType;
        private SideType bType;

        /// <summary>
        /// Returns if both sides of the comparison are values.
        /// </summary>
        private bool BothValues => aType == SideType.Variable && bType == SideType.Variable;
        /// <summary>
        /// Returns if the sides of the comparison need to be swapped.
        /// </summary>
        private bool RequiresSwap => aType == SideType.Constant && bType == SideType.Variable;

        private void FindAType()
        {
            switch (a)
            {
                case TokenIdentifierValue _:
                    aType = SideType.Variable;
                    break;
                case TokenNumberLiteral _:
                    aType = SideType.Constant;
                    break;
                default:
                    aType = SideType.Unknown;
                    break;
            }
        }

        private void FindBType()
        {
            switch (b)
            {
                case TokenIdentifierValue _:
                    bType = SideType.Variable;
                    break;
                case TokenNumberLiteral _:
                    bType = SideType.Constant;
                    break;
                default:
                    bType = SideType.Unknown;
                    break;
            }
        }
        public ComparisonValue(Token a, TokenCompare.Type comparison, Token b, bool invert) : base(invert)
        {
            this.a = a;
            this.b = b;
            this.comparison = comparison;

            // get types of each side
            FindAType();
            FindBType();

            if (!RequiresSwap)
                return;
            
            // swap tokens for simplicity
            (bType, aType) = (aType, bType);
            this.a = b;
            this.b = a;
            this.comparison = comparison.InvertDirection();
        }

        private Tuple<string[], ConditionalSubcommandScore[]> entries;
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
                ScoreboardValue a = ((TokenIdentifierValue) this.a).value;
                ScoreboardValue b = ((TokenIdentifierValue) this.b).value;
                var commands = new List<string>();

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
                ScoreboardValue a = ((TokenIdentifierValue)this.a).value;
                TokenNumberLiteral b = (TokenNumberLiteral)this.b;
                this.entries = a.CompareToLiteral(localComparison, b);

                if(willBeInverted)
                {
                    var commands = new List<string>(this.entries.Item1);
                    temp = executor.scoreboard.temps.Request(true);
                    commands.Add(Command.ScoreboardSet(temp, 0));

                    ExecuteBuilder builder = Command.Execute();
                    foreach (ConditionalSubcommandScore entry in this.entries.Item2)
                    {
                        // retrieve the scoreboard value
                        if (entry.comparesRange)
                        {
                            string scoreName = entry.sourceValue;

                            if (!executor.scoreboard.TryGetByInternalName(scoreName, out ScoreboardValue score))
                                throw new StatementException(callingStatement, $"Unknown value: '{scoreName}'. Is it defined above this line?");

                            score = score.Clone(callingStatement, newClarifier: entry.SourceClarifier);
                            builder.IfScore(score, entry.range);
                        }
                        else
                        {
                            string sourceName = entry.sourceValue;
                            string otherName = entry.otherValue;
                            
                            if (!executor.scoreboard.TryGetByInternalName(sourceName, out ScoreboardValue source))
                                throw new StatementException(callingStatement, $"Unknown value: '{sourceName}'. Is it defined above this line?");
                            if (!executor.scoreboard.TryGetByInternalName(otherName, out ScoreboardValue other))
                                throw new StatementException(callingStatement, $"Unknown value: '{otherName}'. Is it defined above this line?");

                            source = source.Clone(callingStatement, newClarifier: entry.SourceClarifier);
                            other = other.Clone(callingStatement, newClarifier: entry.OtherClarifier);
                            builder.IfScore(source, entry.comparisonType, other);
                        }

                    }

                    commands.Add(builder.Run(Command.ScoreboardSet(temp, 1)));
                    return commands;
                }

                return this.entries.Item1;
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
                ScoreboardValue a = ((TokenIdentifierValue) this.a).value;
                ScoreboardValue b = ((TokenIdentifierValue) this.b).value;

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
                    return this.entries.Item2.Cast<Subcommand>().ToArray();
                }
            }

            // "constant constant" comparison.
            var numberA = this.a as TokenNumberLiteral;
            var numberB = this.b as TokenNumberLiteral;

            // ReSharper disable once PossibleNullReferenceException
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
        private const string ERROR_MESSAGE = "[if <selector>] format should only use @s selectors. Use [if any <selector>] to more concisely show checking for any selector match.";
        private readonly Selector selector;
        private ScoreboardValue temp;

        public ComparisonSelector(Selector selector, bool invert) : base(invert)
        {
            this.selector = selector;
        }

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
                var range = new Range(1, inverted);
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
        private readonly Selector selector;
        private TokenCompare.Type comparison;

        private readonly Token goalCount;
        private SideType goalType;

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

        ScoreboardValue temp;
        public override IEnumerable<string> GetCommands(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel)
        {
            if (goalType == SideType.Unknown)
                throw new StatementException(callingStatement, $"Unexpected token on right-hand side of condition: {goalCount.AsString()}");

            // count number of entities
            var commands = new List<string>();

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
                ScoreboardValue b = ((TokenIdentifierValue)this.goalCount).value;
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
