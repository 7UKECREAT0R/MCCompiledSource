using mc_compiled.Commands;
using mc_compiled.Commands.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public override IEnumerable<string> GetCommands(Executor executor) => null;
        public override Selector GetSelector(Executor executor, Statement forExceptions)
        {
            string name = boolean.Name;
            int checkFor = inverted ? 0 : 1;

            Range range = new Range(checkFor, false);
            ScoresEntry entry = new ScoresEntry(name, range);

            Selector selector = new Selector(executor.ActiveSelector);
            selector.scores.checks.Add(entry);
            return selector;
        }
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
        bool RequiresTemp
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
            if(RequiresSwap)
            {
                SideType temp = bType;
                bType = aType;
                aType = temp;
                this.a = a;
                this.b = b;
                this.comparison = SelectorUtils.InvertComparison(comparison);
            }
        }

        ScoreboardValue temp;
        ScoresEntry[] scoresEntries;
        public override IEnumerable<string> GetCommands(Executor executor)
        {
            if (RequiresTemp)
            {
                List<string> commands = new List<string>();

                TokenIdentifierValue tiv = this.a as TokenIdentifierValue;
                ScoreboardValue a = tiv.value;
                ScoreboardValue b = (this.b as TokenIdentifierValue).value;
                ScoreboardValue temp = executor.scoreboard.RequestTemp(a); // temps are auto-released at the end

                commands.AddRange(temp.CommandsSet(tiv.references.ToString(), a, null, null));
                commands.AddRange(temp.CommandsSub("@s", b, null, null));
                this.temp = temp;
            }
            else if(aType == SideType.Variable)
            {
                TokenCompare.Type comparison = this.comparison;
                if (inverted)
                    comparison = SelectorUtils.InvertComparison(comparison);

                TokenIdentifierValue tiv = this.a as TokenIdentifierValue;
                ScoreboardValue a = tiv.value;
                TokenNumberLiteral b = (this.b as TokenNumberLiteral);
                var entries = a.CompareToLiteral(null, tiv.references.ToString(), comparison, b);

                this.scoresEntries = entries.Item1;
                return entries.Item2;
            }

            return null;
        }
        public override Selector GetSelector(Executor executor, Statement callingStatement)
        {
            if (aType == SideType.Unknown)
                throw new StatementException(callingStatement, $"Token {this.a.DebugString()} is not a valid number or variable.");
            if (bType == SideType.Unknown)
                throw new StatementException(callingStatement, $"Token {this.b.DebugString()} is not a valid number or variable.");

            var comparison = this.comparison;
            if (inverted)
                comparison = SelectorUtils.InvertComparison(comparison);

            Selector selector = new Selector(executor.ActiveSelector);

            if(RequiresTemp)
            {
                Range range;
                switch (comparison)
                {
                    case TokenCompare.Type.EQUAL:
                        range = new Range(0, false);
                        break;
                    case TokenCompare.Type.NOT_EQUAL:
                        range = new Range(0, true);
                        break;
                    case TokenCompare.Type.LESS_THAN:
                        range = new Range(null, -1);
                        break;
                    case TokenCompare.Type.LESS_OR_EQUAL:
                        range = new Range(null, 0);
                        break;
                    case TokenCompare.Type.GREATER_THAN:
                        range = new Range(1, null);
                        break;
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                        range = new Range(0, null);
                        break;
                    default:
                        range = default;
                        break;
                }
                ScoresEntry entry = new ScoresEntry(temp.Name, range);
                selector.scores.checks.Add(entry);
                return selector;
            }

            if(aType == SideType.Variable)
            {
                // bType will be a Constant
                // scoresEntries will not be null
                selector.scores.checks.AddRange(scoresEntries);
                return selector;
            }

            // Both are constant
            TokenLiteral a = (this.a as TokenLiteral);
            TokenLiteral b = (this.b as TokenLiteral);
            bool result = a.CompareWithOther(comparison, b);
            
            // skip next code
            if(!result)
            {
                Statement next = executor.Next();
                if(next is StatementOpenBlock)
                {
                    StatementOpenBlock block = next as StatementOpenBlock;
                    for (int i = 0; i < block.statementsInside; i++)
                        executor.Next();
                    executor.Next(); // close block
                }
            }

            return null;
        }
    }
    public class ComparisonSelector : Comparison
    {
        public const string TAG_PREFIX = "__invert";
        public readonly Selector selector;

        public ComparisonSelector(Selector selector, bool invert) : base(invert)
        {
            this.selector = selector;
        }

        string tagName;
        public override IEnumerable<string> GetCommands(Executor executor)
        {
            if (!inverted)
                return null;

            // Special case for inversion.
            List<string> commands = new List<string>();

            string tagTargets = selector.core.AsCommandString();
            this.tagName = DepthEncode(TAG_PREFIX, executor.Depth, selector);

            commands.Add(Command.Tag(tagTargets, tagName));
            commands.Add(Command.Execute(selector.ToString(),
                selector.offsetX, selector.offsetY, selector.offsetZ,
                Command.TagRemove("@s", tagName)));

            executor.definedTags.Add(tagName);
            return commands;
        }
        public override Selector GetSelector(Executor executor, Statement forExceptions)
        {
            if (inverted)
            {
                Selector picker = new Selector(selector.core);
                picker.tags.Add(new Tag(tagName));
                return picker;
            }
            else
                return new Selector(selector);
        }
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
        public override IEnumerable<string> GetCommands(Executor executor)
        {
            // count number of entities
            List<string> commands = new List<string>();
            temp = executor.scoreboard.RequestTemp();
            string selectorString = selector.ToString();
            commands.Add(Command.ScoreboardSet(Executor.FAKEPLAYER_NAME, temp.Name, 0));
            commands.Add(Command.Execute(selectorString, selector.offsetX, selector.offsetY, selector.offsetZ,
                Command.ScoreboardAdd(Executor.FAKEPLAYER_NAME, temp.Name, 1)));
            commands.Add(Command.ScoreboardOpSet(executor.ActiveSelectorStr, temp.Name, Executor.FAKEPLAYER_NAME, temp.Name));

            // requires subtraction for it to work properly
            if (RequiresSubtraction)
            {
                TokenIdentifierValue value = goalCount as TokenIdentifierValue;
                commands.AddRange(temp.CommandsSub(value.references.ToString(), value.value, null, null));
            }

            return commands;
        }
        public override Selector GetSelector(Executor executor, Statement callingStatement)
        {
            if (goalType == SideType.Unknown)
                throw new StatementException(callingStatement, $"Token {goalCount.DebugString()} is not a valid number or variable.");

            var comparison = this.comparison;
            if (inverted)
                comparison = SelectorUtils.InvertComparison(comparison);

            Selector selector = new Selector(executor.ActiveSelector);

            Range range;
            if (RequiresSubtraction)
            {
                switch (comparison)
                {
                    case TokenCompare.Type.EQUAL:
                        range = new Range(0, false);
                        break;
                    case TokenCompare.Type.NOT_EQUAL:
                        range = new Range(0, true);
                        break;
                    case TokenCompare.Type.LESS_THAN:
                        range = new Range(null, -1);
                        break;
                    case TokenCompare.Type.LESS_OR_EQUAL:
                        range = new Range(null, 0);
                        break;
                    case TokenCompare.Type.GREATER_THAN:
                        range = new Range(1, null);
                        break;
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                        range = new Range(0, null);
                        break;
                    default:
                        range = default;
                        break;
                }
            }
            else
            {
                TokenNumberLiteral number = goalCount as TokenNumberLiteral;
                int value = number.GetNumberInt();

                switch (comparison)
                {
                    case TokenCompare.Type.EQUAL:
                        range = new Range(value, false);
                        break;
                    case TokenCompare.Type.NOT_EQUAL:
                        range = new Range(value, true);
                        break;
                    case TokenCompare.Type.LESS_THAN:
                        range = new Range(null, value - 1);
                        break;
                    case TokenCompare.Type.LESS_OR_EQUAL:
                        range = new Range(null, value);
                        break;
                    case TokenCompare.Type.GREATER_THAN:
                        range = new Range(value + 1, null);
                        break;
                    case TokenCompare.Type.GREATER_OR_EQUAL:
                        range = new Range(value, null);
                        break;
                    default:
                        range = default;
                        break;
                }
            }

            ScoresEntry entry = new ScoresEntry(temp.Name, range);
            selector.scores.checks.Add(entry);
            return selector;
        }
    }
    public class ComparisonAny : Comparison
    {
        public const string TAG_PREFIX = "__no_matches";
        public readonly Selector selector;

        public ComparisonAny(Selector selector, bool invert) : base(invert)
        {
            this.selector = selector;
        }

        string tagName;
        public override IEnumerable<string> GetCommands(Executor executor)
        {
            // tag all possible entities
            Selector active = executor.ActiveSelector;
            string tagTargets = active.core.AsCommandString();
            this.tagName = DepthEncode(TAG_PREFIX, executor.Depth, selector);

            selector.count = new Count(1);

            // remove tag on matching entities
            string[] commands = new[] {
                Command.Tag(tagTargets, tagName),
                Command.Execute(selector.ToString(), active.offsetX, active.offsetY, active.offsetZ, Command.TagRemove("*", tagName))
            };

            executor.definedTags.Add(tagName);
            return commands;
        }
        public override Selector GetSelector(Executor executor, Statement callingStatement)
        {
            Selector selector = new Selector(executor.ActiveSelector);
            selector.tags.Add(new Tag(this.tagName, !inverted));
            return selector;
        }
    }
    public class ComparisonBlock : Comparison
    {
        public const string TAG_PREFIX = "__not_block";

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

        string tagName;
        public override IEnumerable<string> GetCommands(Executor executor)
        {
            if (inverted)
            {
                Selector active = executor.ActiveSelector;
                string selector = active.ToString();
                this.tagName = TAG_PREFIX + executor.depth;
                int data = this.data ?? -1;
                executor.definedTags.Add(tagName);

                return new[] {
                    Command.Tag(selector, tagName),
                    Command.Execute(selector, active.offsetX, active.offsetY, active.offsetZ,
                        x, y, z, block, data,
                        Command.TagRemove("@s", tagName))
                };
            }

            return null;
        }
        public override Selector GetSelector(Executor executor, Statement callingStatement)
        {
            Selector selector = new Selector(executor.ActiveSelector);

            if (inverted)
                selector.tags.Add(new Tag(this.tagName, false));
            else
                selector.blockCheck = new BlockCheck(x, y, z, block, data);

            return selector;
        }
    }
    public class ComparisonPositioned : Comparison
    {
        readonly Coord x, y, z;

        public ComparisonPositioned(Coord x, Coord y, Coord z, bool invert) : base(invert)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override IEnumerable<string> GetCommands(Executor executor)
        {
            return null;
        }
        public override Selector GetSelector(Executor executor, Statement callingStatement)
        {
            Selector selector = new Selector(Selector.Core.s);
            selector.offsetX = x;
            selector.offsetY = y;
            selector.offsetZ = z;

            return selector;
        }
    }

    /// <summary>
    /// The type of an operand in a comparison-based statement
    /// </summary>
    public enum SideType
    {
        Unknown, Constant, Variable
    }
}
