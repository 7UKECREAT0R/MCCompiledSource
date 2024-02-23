using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Selectors;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A set of comparisons.
    /// </summary>
    public class ComparisonSet : List<Comparison>
    {
        /// <summary>
        /// Instantiates an empty ComparisonSet.
        /// </summary>
        public ComparisonSet() : base() { }

        public string GetDescription() => "[if " + string.Join(", ", this.Select(c => c.GetDescription())) + ']';
        
        /// <summary>
        /// Instantiates a ComparisonSet with an array of items in it to start.
        /// </summary>
        /// <param name="comparisons"></param>
        public ComparisonSet(params Comparison[] comparisons) : base(comparisons) { }
        /// <summary>
        /// Instantiates a ComparisonSet with a set of items in it to start.
        /// </summary>
        /// <param name="comparisons"></param>
        public ComparisonSet(IEnumerable<Comparison> comparisons) : base(comparisons) { }
        /// <summary>
        /// Returns all targets (scoreboard values) which should show up in an assertion of this comparison.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<ScoreboardValue> GetAssertionTargets() =>
            this.SelectMany(comparison => comparison.GetAssertionTargets());

        /// <summary>
        /// Pulls tokens identifying with comparisons from a statement and return a new ComparisonSet holding them.
        /// </summary>
        /// <returns></returns>
        public static ComparisonSet GetComparisons(Executor executor, Statement tokens)
        {
            if (!tokens.HasNext)
                return new ComparisonSet();

            var set = new ComparisonSet();
            bool invertNext = false;

            // ! read at your own risk !

            do
            {
                Token currentToken = tokens.Next();

                switch (currentToken)
                {
                    case TokenNot _:
                        invertNext = !invertNext;
                        continue;
                    case TokenIdentifierValue identifierValue:
                    {
                        ScoreboardValue value = identifierValue.value;
                        
                        // ComparisonValue
                        // if <score> <operator> <value>
                        if (tokens.NextIs<TokenCompare>())
                        {
                            TokenCompare.Type comparison = tokens.Next<TokenCompare>().GetCompareType();
                            Token b = tokens.Next();
                            var field = new ComparisonValue(identifierValue, comparison, b, invertNext);
                            invertNext = false;
                            set.Add(field);
                        }
                        else if (value.type.CanCompareAlone)
                        {
                            // if <score>
                            var comparisonAlone = new ComparisonAlone(value, invertNext);
                            invertNext = false;
                            set.Add(comparisonAlone);
                        }
                        
                        break;
                    }
                    case TokenSelectorLiteral selectorLiteral:
                    {
                        // ComparisonSelector
                        // if <@selector>
                        var comparison = new ComparisonSelector(selectorLiteral.selector, invertNext);
                        invertNext = false;
                        set.Add(comparison);
                        break;
                    }
                    case TokenIdentifier identifier:
                    {
                        string word = identifier.word.ToUpper();

                        switch (word)
                        {
                            case "COUNT":
                            {
                                // ComparisonCount
                                // if count <@selector> <operator> <value>
                                var selector = tokens.Next<TokenSelectorLiteral>();
                                var comparison = tokens.Next<TokenCompare>();
                                Token b = tokens.Next();

                                var count = new ComparisonCount(selector,
                                    comparison.GetCompareType(), b, invertNext);

                                invertNext = false;
                                set.Add(count);
                                break;
                            }
                            case "ANY":
                            {
                                // ComparisonAny
                                // if any <@selector>
                                var selector = tokens.Next<TokenSelectorLiteral>();
                                var any = new ComparisonAny(selector, invertNext);

                                invertNext = false;
                                set.Add(any);
                                break;
                            }
                            case "BLOCK":
                            {
                                // ComparisonBlock
                                // if block <x, y, z> <block> [data]
                                Coordinate x = tokens.Next<TokenCoordinateLiteral>();
                                Coordinate y = tokens.Next<TokenCoordinateLiteral>();
                                Coordinate z = tokens.Next<TokenCoordinateLiteral>();
                                string block = tokens.Next<TokenStringLiteral>();

                                int? data = null;
                                if (tokens.NextIs<TokenIntegerLiteral>())
                                    data = tokens.Next<TokenIntegerLiteral>();

                                var blockCheck = new ComparisonBlock(x, y, z, block, data, invertNext);

                                invertNext = false;
                                set.Add(blockCheck);
                                break;
                            }
                            case "BLOCKS":
                            {
                                // ComparisonBlocks
                                // if blocks <start x, y, z> <end x, y, z> <dest x, y, z> <ScanMode>
                                Coordinate startX = tokens.Next<TokenCoordinateLiteral>();
                                Coordinate startY = tokens.Next<TokenCoordinateLiteral>();
                                Coordinate startZ = tokens.Next<TokenCoordinateLiteral>();
                                Coordinate endX = tokens.Next<TokenCoordinateLiteral>();
                                Coordinate endY = tokens.Next<TokenCoordinateLiteral>();
                                Coordinate endZ = tokens.Next<TokenCoordinateLiteral>();
                                Coordinate destX = tokens.Next<TokenCoordinateLiteral>();
                                Coordinate destY = tokens.Next<TokenCoordinateLiteral>();
                                Coordinate destZ = tokens.Next<TokenCoordinateLiteral>();

                                var scanMode = BlocksScanMode.all;
                                if(tokens.NextIs<TokenIdentifierEnum>())
                                {
                                    ParsedEnumValue parsed = tokens.Next<TokenIdentifierEnum>().value;
                                    parsed.RequireType<BlocksScanMode>(tokens);
                                    scanMode = (BlocksScanMode)parsed.value;
                                }

                                var blockCheck = new ComparisonBlocks(startX, startY, startZ,
                                    endX, endY, endZ, destX, destY, destZ, scanMode, invertNext);

                                invertNext = false;
                                set.Add(blockCheck);
                                break;
                            }
                        }

                        break;
                    }
                }

                if (!tokens.HasNext)
                    break;

                currentToken = tokens.Next();

                // loop again if an AND operator is present
                if (currentToken is TokenAnd)
                    continue;
                break;
                
            } while (tokens.HasNext);

            return set;
        }


        /// <summary>
        /// Run this ComparisonSet and apply it to an executor.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="callingStatement"></param>
        public void Run(Executor executor, Statement callingStatement)
        {
            if (this.IsEmpty)
                throw new StatementException(callingStatement, "No valid conditions specified.");

            if (!executor.HasNext)
                throw new StatementException(callingStatement, "Unexpected end of file when running comparison.");

            // get the statement at the end of the execution set, and detect if it's an else-statement.
            int endOfExecutionSet = FindEndOfExecutionSet(executor);
            Statement atEndOfExecutionSet = executor.PeekSkip(endOfExecutionSet);
            bool usesElse = atEndOfExecutionSet is StatementDirective directive &&
                            directive.HasAttribute(DirectiveAttribute.INVERTS_COMPARISON);

            var commands = new List<string>();
            var chunks = new List<Subcommand>();

            bool cancel = false;
            foreach (Comparison comparison in this)
            {
                IEnumerable<string> partCommands = comparison.GetCommands(executor, callingStatement, usesElse, out bool cancel0);
                Subcommand[] localChunks = comparison.GetExecuteChunks(executor, callingStatement, usesElse, out bool cancel1);

                cancel |= cancel0 | cancel1;

                if (partCommands != null)
                    commands.AddRange(partCommands);
                if (localChunks != null && localChunks.Length > 0)
                    chunks.AddRange(localChunks);
            }

            // add commands to a file
            CommandFile prepFile = null;
            if (commands.Count > 0 || usesElse)
            {
                prepFile = Executor.GetNextGeneratedFile("comparisonSetup");
                if(Program.DECORATE)
                {
                    // attempt to add extra detail to the output file for reading users
                    string comparisonString = "[if " + string.Join(", ", this.Select(c => c.GetDescription())) + ']';
                    prepFile.Add($"# Setup for comparison {comparisonString}");
                    prepFile.AddTrace(executor.CurrentFile);
                }
                prepFile.Add(commands);
                executor.AddExtraFile(prepFile);
                executor.AddCommandClean(Command.Function(prepFile));
            }

            // apply the comparisons.
            if (usesElse)
                ApplyComparisonToWithElse(chunks, prepFile, callingStatement, executor, cancel);
            else
                ApplyComparisonToSolo(chunks, callingStatement, executor, cancel);
        }

        /// <summary>
        /// Run this ComparisonSet and run a command if it evaluates true.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <param name="executor">The executor to run the command.</param>
        /// <param name="callingStatement">The calling statement.</param>
        public void RunCommand(string command, Executor executor, Statement callingStatement)
        {
            if (this.IsEmpty)
                throw new StatementException(callingStatement, "No valid conditions specified.");

            var commands = new List<string>();
            var chunks = new List<Subcommand>();

            foreach (Comparison comparison in this)
            {
                IEnumerable<string> partCommands = comparison.GetCommands(executor, callingStatement, false, out bool _);
                Subcommand[] localChunks = comparison.GetExecuteChunks(executor, callingStatement, false, out bool _);

                if (partCommands != null)
                    commands.AddRange(partCommands);
                if (localChunks != null && localChunks.Length > 0)
                    chunks.AddRange(localChunks);
            }

            // add commands to a file
            CommandFile prepFile = null;
            if (commands.Count > 0)
            {
                prepFile = Executor.GetNextGeneratedFile("comparisonSetup");
                if (Program.DECORATE)
                {
                    // attempt to add extra detail to the output file for reading users
                    string comparisonString = "[if " + string.Join(", ", this.Select(c => c.GetDescription())) + ']';
                    prepFile.Add($"# Setup for comparison {comparisonString}");
                    prepFile.AddTrace(executor.CurrentFile);
                }
                prepFile.Add(commands);
                executor.AddExtraFile(prepFile);
                executor.AddCommandClean(Command.Function(prepFile));
            }

            // apply the comparisons.
            string finalExecute = new ExecuteBuilder()
                .WithSubcommands(chunks)
                .Run(command);
            executor.AddCommand(finalExecute);
        }
        private static int FindEndOfExecutionSet(Executor executor)
        {
            Statement next = executor.Peek();

            if (next is StatementOpenBlock openBlock)
                return openBlock.statementsInside + 2;

            Statement lastStatement;
            int count = 0;

            do
            {
                lastStatement = executor.PeekSkip(count++);
            } while (lastStatement.Skip);

            return count;
        }
        /// <summary>
        /// Applies the given comparison subcommands to the executor.
        /// </summary>
        /// <param name="chunks">The chunks to add.</param>
        /// <param name="forExceptions">For exceptions.</param>
        /// <param name="executor">The executor to modify.</param>
        /// <param name="cancel">Whether to cancel the execution by skipping the statement.</param>
        private void ApplyComparisonToSolo(IEnumerable<Subcommand> chunks, Statement forExceptions, Executor executor, bool cancel)
        {
            // get the next statement to determine how to run this comparison
            Statement next = executor.Seek();

            if (next is StatementOpenBlock openBlock)
            {
                // only do the block stuff if necessary.
                if (openBlock.statementsInside > 0)
                {
                    if (cancel)
                    {
                        openBlock.openAction = (e) =>
                        {
                            openBlock.CloseAction = null;
                            for (int i = 0; i < openBlock.statementsInside; i++)
                                e.Next();
                        };
                    }
                    else
                    {
                        string finalExecute = new ExecuteBuilder()
                            .WithSubcommands(chunks)
                            .WithSubcommand(new SubcommandRun())
                            .Build(out _);

                        if (openBlock.meaningfulStatementsInside == 1)
                        {
                            // modify prepend buffer as if 1 statement was there
                            executor.AppendCommandPrepend(finalExecute);
                            openBlock.openAction = null;
                            openBlock.CloseAction = null;
                        }
                        else
                        {
                            CommandFile blockFile = Executor.GetNextGeneratedFile("branch");

                            if (Program.DECORATE)
                            {
                                blockFile.Add($"# Run after comparison {GetDescription()}");
                                blockFile.AddTrace(executor.CurrentFile);
                            }

                            string command = finalExecute + Command.Function(blockFile);
                            executor.AddCommand(command);

                            openBlock.openAction = (e) =>
                            {
                                e.PushFile(blockFile);
                            };
                            openBlock.CloseAction = (e) =>
                            {
                                e.PopFile();
                            };
                        }
                    }
                } else
                {
                    openBlock.openAction = null;
                    openBlock.CloseAction = null;
                }
            }
            else
            {
                if (cancel)
                {
                    while (executor.HasNext && executor.Peek().Skip)
                        executor.Next();
                    executor.Next();
                }
                else
                {
                    string finalExecute = new ExecuteBuilder()
                        .WithSubcommands(chunks)
                        .WithSubcommand(new SubcommandRun())
                        .Build(out _);
                    executor.AppendCommandPrepend(finalExecute);
                }
            }
        }

        /// <summary>
        /// Applies the given comparison subcommands to the executor, assuming there will be an else statement.
        /// </summary>
        /// <param name="chunks">The chunks to add.</param>
        /// <param name="setupFile">The init file with all the setup commands. (runs once per world)</param>
        /// <param name="forExceptions">For exceptions.</param>
        /// <param name="executor">The executor to modify.</param>
        /// <param name="cancel">Whether to cancel the execution by skipping the statement.</param>
        private void ApplyComparisonToWithElse(IEnumerable<Subcommand> chunks, CommandFile setupFile, Statement forExceptions, Executor executor, bool cancel)
        {
            // get the next statement to determine how to run this comparison
            Statement next = executor.Seek();

            var record = new PreviousComparisonStructure(executor.scoreboard.temps, forExceptions, executor.ScopeLevel, GetDescription());
            ScoreboardValue resultObjective = record.resultStore;

            setupFile.Add(Command.ScoreboardSet(resultObjective, 0));
            setupFile.Add(Command.Execute().WithSubcommands(chunks).Run(Command.ScoreboardSet(resultObjective, 1)));
            
            ConditionalSubcommand used = ConditionalSubcommandScore.New(resultObjective, new Range(1, false));
            record.conditionalUsed = used;
            executor.SetLastCompare(record);
            
            string executePrefix = new ExecuteBuilder()
                .WithSubcommand(new SubcommandIf(used))
                .Run();
            
            if (next is StatementOpenBlock openBlock)
            {
                // only do the block stuff if necessary.
                if (openBlock.statementsInside > 0)
                {
                    if (cancel)
                    {
                        openBlock.openAction = (e) =>
                        {
                            openBlock.CloseAction = null;
                            for (int i = 0; i < openBlock.statementsInside; i++)
                                e.Next();
                        };
                    }
                    else
                    {

                        if (openBlock.meaningfulStatementsInside == 1)
                        {
                            // modify prepend buffer as if 1 statement was there
                            executor.AppendCommandPrepend(executePrefix);
                            openBlock.openAction = null;
                            openBlock.CloseAction = null;
                        }
                        else
                        {
                            CommandFile blockFile = Executor.GetNextGeneratedFile("branch");

                            if (Program.DECORATE)
                            {
                                blockFile.Add($"# Run after comparison {GetDescription()}");
                                blockFile.AddTrace(executor.CurrentFile);
                            }

                            string command = executePrefix + Command.Function(blockFile);
                            executor.AddCommand(command);

                            openBlock.openAction = (e) =>
                            {
                                e.PushFile(blockFile);
                            };
                            openBlock.CloseAction = (e) =>
                            {
                                e.PopFile();
                            };
                        }
                    }
                }
                else
                {
                    openBlock.openAction = null;
                    openBlock.CloseAction = null;
                }
            }
            else
            {
                if (cancel)
                {
                    while(executor.HasNext && executor.Peek().Skip)
                        executor.Next();
                    executor.Next();
                }
                else
                    executor.AppendCommandPrepend(executePrefix);
            }
        }

        /// <summary>
        /// Set the inversion on all elements.
        /// </summary>
        /// <param name="invert"></param>
        public void InvertAll(bool invert)
        {
            foreach (Comparison item in this)
                item.SetInversion(invert);
        }

        public bool IsEmpty
        {
            get => this.Count == 0;
        }
    }

    /// <summary>
    /// Represents a generic comparison in an if-statement.
    /// </summary>
    public abstract class Comparison
    {
        readonly bool originallyInverted;

        /// <summary>
        /// Encodes a selector's hash and depth together with a prefix. (not actually 'encoding')
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="depth"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string DepthEncode(string prefix, int depth, Selector selector)
        {
            return prefix + depth + '_' + selector.GetHashCode().ToString().Replace('-', '0');;
        }
        public Comparison(bool invert)
        {
            this.originallyInverted = invert;
            this.inverted = invert;
        }

        /// <summary>
        /// If this comparison is inverted.
        /// </summary>
        public bool inverted;
        /// <summary>
        /// Toggles the inversion of this comparison.
        /// </summary>
        public void SetInversion(bool invert) => this.inverted = invert ? !this.originallyInverted : this.originallyInverted;

        /// <summary>
        /// Get the commands needed, if any, to set up this comparison. May return null if no commands are needed.
        /// </summary>
        /// <param name="executor">The calling executor.</param>
        /// <param name="callingStatement">The calling statement.</param>
        /// <param name="willBeInverted">If this comparison will be inverted later on using e.g., an else statement.</param>
        /// <returns></returns>
        /// <param name="cancel">Output parameter signaling to cancel the entire statement, like if a compile-time comparison fails.</param>
        public abstract IEnumerable<string> GetCommands(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel);
        /// <summary>
        /// Gets the execute chunk needed to perform this comparison. May return null.
        /// </summary>
        /// <param name="executor">The calling executor.</param>
        /// <param name="callingStatement">The calling statement.</param>
        /// <param name="willBeInverted">If this comparison will be inverted later on using e.g., an else statement.</param>
        /// <param name="cancel">Output parameter signaling to cancel the entire statement, like if a compile-time comparison fails.</param>
        public abstract Subcommand[] GetExecuteChunks(Executor executor, Statement callingStatement, bool willBeInverted, out bool cancel);

        public abstract string GetDescription();
        public abstract IEnumerable<ScoreboardValue> GetAssertionTargets();
    }
}
