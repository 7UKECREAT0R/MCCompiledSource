using System;
using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Commands.Selectors;
using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Compiler.Async;

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
        public ComparisonSet() { }

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
                        if (invertNext)
                            throw new StatementException(tokens, "Double-negative.");
                        invertNext = true;
                        continue;
                    case TokenIdentifierValue identifierValue:
                    {
                        ScoreboardValue value = identifierValue.value;
                        
                        // ComparisonValue
                        // if <score> <operator> <value>
                        if (tokens.NextIs<TokenCompare>(false))
                        {
                            TokenCompare.Type comparison = tokens.Next<TokenCompare>("comparison operator").GetCompareType();
                            Token b = tokens.Next();
                            var field = new ComparisonValue(identifierValue, comparison, b, invertNext);
                            invertNext = false;
                            set.Add(field);
                            break;
                        }

                        if (!value.type.CanCompareAlone)
                            throw new StatementException(tokens, $"Cannot compare value '{value.Name}' alone.");
                        
                        // if <score>
                        var comparisonAlone = new ComparisonAlone(value, invertNext);
                        invertNext = false;
                        set.Add(comparisonAlone);
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
                                var selector = tokens.Next<TokenSelectorLiteral>("selector");
                                var comparison = tokens.Next<TokenCompare>("comparison operator");
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
                                var selector = tokens.Next<TokenSelectorLiteral>("selector");
                                var any = new ComparisonAny(selector, invertNext);

                                invertNext = false;
                                set.Add(any);
                                break;
                            }
                            case "BLOCK":
                            {
                                // ComparisonBlock
                                // if block <x, y, z> <block> [data]
                                Coordinate x = tokens.Next<TokenCoordinateLiteral>("x");
                                Coordinate y = tokens.Next<TokenCoordinateLiteral>("y");
                                Coordinate z = tokens.Next<TokenCoordinateLiteral>("z");
                                string block = tokens.Next<TokenStringLiteral>("block");

                                int? data = null;
                                if (tokens.NextIs<TokenIntegerLiteral>(false))
                                    data = tokens.Next<TokenIntegerLiteral>("data");

                                var blockCheck = new ComparisonBlock(x, y, z, block, data, invertNext);

                                invertNext = false;
                                set.Add(blockCheck);
                                break;
                            }
                            case "BLOCKS":
                            {
                                // ComparisonBlocks
                                // if blocks <start x, y, z> <end x, y, z> <dest x, y, z> <ScanMode>
                                Coordinate startX = tokens.Next<TokenCoordinateLiteral>("start x");
                                Coordinate startY = tokens.Next<TokenCoordinateLiteral>("start y");
                                Coordinate startZ = tokens.Next<TokenCoordinateLiteral>("start z");
                                Coordinate endX = tokens.Next<TokenCoordinateLiteral>("end x");
                                Coordinate endY = tokens.Next<TokenCoordinateLiteral>("end y");
                                Coordinate endZ = tokens.Next<TokenCoordinateLiteral>("end z");
                                Coordinate destX = tokens.Next<TokenCoordinateLiteral>("destination x");
                                Coordinate destY = tokens.Next<TokenCoordinateLiteral>("destination y");
                                Coordinate destZ = tokens.Next<TokenCoordinateLiteral>("destination z");

                                var scanMode = BlocksScanMode.all;
                                if (tokens.NextIs<TokenIdentifierEnum>(false))
                                {
                                    ParsedEnumValue parsed = tokens.Next<TokenIdentifierEnum>("scan mode").value;
                                    parsed.RequireType<BlocksScanMode>(tokens);
                                    scanMode = (BlocksScanMode)parsed.value;
                                }

                                var blockCheck = new ComparisonBlocks(startX, startY, startZ,
                                    endX, endY, endZ, destX, destY, destZ, scanMode, invertNext);

                                invertNext = false;
                                set.Add(blockCheck);
                                break;
                            }
                            default:
                                throw new StatementException(tokens, $"Invalid condition: '{currentToken.AsString()}'.");
                        }
                        break;
                    }
                    default:
                        throw new StatementException(tokens, $"Invalid condition: '{currentToken.AsString()}'.");
                }

                if (!tokens.HasNext)
                    break;

                currentToken = tokens.Next();

                // loop again if an AND operator is present
                if (!(currentToken is TokenAnd))
                    throw new StatementException(tokens, "Missing 'and' between conditions.");
                if (!tokens.HasNext)
                    throw new StatementException(tokens, "No condition specified after 'and'.");
            } while (tokens.HasNext);

            if (invertNext)
                throw new StatementException(tokens, "No condition specified after 'not'.");
            
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
            if (commands.Count > 0 || usesElse || executor.async.IsInAsync)
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
            if (usesElse || executor.async.IsInAsync) // if we're in async, we need implicit else for skipping stages
                ApplyComparisonToWithElse(chunks, prepFile, callingStatement, executor, cancel, usesElse);
            else
                ApplyComparisonToSolo(chunks, executor, cancel);
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
            if (commands.Count > 0)
            {
                CommandFile prepFile = Executor.GetNextGeneratedFile("comparisonSetup");
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
        /// <param name="executor">The executor to modify.</param>
        /// <param name="cancel">Whether to cancel the execution by skipping the statement.</param>
        private void ApplyComparisonToSolo(IEnumerable<Subcommand> chunks, Executor executor, bool cancel)
        {
            string prepend = new ExecuteBuilder()
                .WithSubcommands(chunks)
                .WithSubcommand(new SubcommandRun())
                .Build(out _);
            
            InjectBranch(executor, prepend, cancel);
        }

        /// <summary>
        /// Applies the given comparison subcommands to the executor, assuming there will be an else statement.
        /// </summary>
        /// <param name="chunks">The chunks to add.</param>
        /// <param name="setupFile">The init file with all the setup commands. (runs once per world)</param>
        /// <param name="forExceptions">For exceptions.</param>
        /// <param name="executor">The executor to modify.</param>
        /// <param name="cancel">Whether to cancel the execution by skipping the statement.</param>
        /// <param name="usesElse"></param>
        private void ApplyComparisonToWithElse(IEnumerable<Subcommand> chunks, CommandFile setupFile,
            Statement forExceptions, Executor executor, bool cancel, bool usesElse)
        {
            var record = new PreviousComparisonStructure(executor.scoreboard.temps, forExceptions, executor.ScopeLevel, GetDescription());
            ScoreboardValue resultObjective = record.resultStore;

            setupFile.Add(Command.ScoreboardSet(resultObjective, 0));
            setupFile.Add(Command.Execute().WithSubcommands(chunks).Run(Command.ScoreboardSet(resultObjective, 1)));
            
            ConditionalSubcommand used = ConditionalSubcommandScore.New(resultObjective, Range.Of(1));
            record.conditionalUsed = used;
            executor.SetLastCompare(record);
            
            string executePrefix = new ExecuteBuilder()
                .WithSubcommand(new SubcommandIf(used))
                .Run();
            
            InjectBranch(executor, executePrefix, cancel, record);
        }
        
        private void InjectBranch(Executor executor, string prepend, bool cancel, PreviousComparisonStructure elseInfo = null)
        {
            // get the next statement to determine how to inject the comparison
            Statement next = executor.Seek();
            
            if (next is StatementOpenBlock openBlock)
                InjectBranchBlock(executor, prepend, cancel, openBlock, elseInfo);
            else
                InjectBranchSingle(executor, prepend, cancel, elseInfo);
        }
        private void InjectBranchBlock(Executor executor, string prepend, bool cancel, StatementOpenBlock openBlock, PreviousComparisonStructure elseInfo = null)
        {
            if (cancel)
            {
                openBlock.CloseAction = null;
                openBlock.openAction = (e) =>
                {
                    for (int i = 0; i < openBlock.statementsInside; i++)
                        e.Next();
                };
                return;
            }

            if (openBlock.meaningfulStatementsInside == 0)
            {
                openBlock.openAction = null;
                openBlock.CloseAction = null;
                return;
            }

            // special case if we're in an async context.
            if (executor.async.IsInAsync)
            {
                // we only want to do this if there will actually be an async split inside
                Statement[] containingStatements = executor.Peek(1, openBlock.statementsInside);
                if (containingStatements.Any(s => s.DoesAsyncSplit))
                {
                    // there will be an async stage allocated for us to use by the time the OpenAction gets hit.
                    string currentFunction = executor.async.CurrentFunction.escapedFunctionName;
                    int nextStage = executor.async.CurrentFunction.NextStageIndex;
                    string nextStageName = AsyncStage.NameStageFunction(currentFunction, nextStage, true);
                    executor.AddCommand(prepend + Command.Function(nextStageName));
                
                    // the file that enters into either the 'true async stage' or 'false async stage'.
                    // the 'false async stage' will need to be set when the block closes, so store a reference of it.
                    CommandFile entryFile = executor.CurrentFile;

                    openBlock.CloseAction = (e) =>
                    {
                        if (elseInfo == null)
                            throw new Exception("Did not get PreviousComparisonStructure for async branch evaluation.");
                    
                        AsyncStage closingStage = e.async.CurrentFunction.ActiveStage;
                        string elseCommand = new ExecuteBuilder()
                            .WithSubcommand(new SubcommandUnless(elseInfo.conditionalUsed))
                            .Run(Command.Function(closingStage.file));
                    
                        entryFile.Add(elseCommand);
                    };
                    return;
                }
            }
            
            if (openBlock.meaningfulStatementsInside == 1)
            {
                // modify prepend buffer as if 1 statement was there
                executor.AppendCommandPrepend(prepend);
                openBlock.openAction = null;
                openBlock.CloseAction = null;
                return;
            }

            CommandFile blockFile = Executor.GetNextGeneratedFile("branch");

            if (Program.DECORATE)
            {
                blockFile.Add($"# Run after comparison {GetDescription()}");
                blockFile.AddTrace(executor.CurrentFile);
            }

            string command = prepend + Command.Function(blockFile);
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
        private static void InjectBranchSingle(Executor executor, string prepend, bool cancel, PreviousComparisonStructure elseInfo = null)
        {
            if (cancel)
            {
                while(executor.HasNext && executor.Peek().Skip)
                    executor.Next();
                executor.Next();
                return;
            }
            
            // special case if we're in an async context.
            if (executor.async.IsInAsync)
            {
                // we only want to do this if there will actually be an async split inside
                Statement nextStatement = executor.Seek();
                if (nextStatement.DoesAsyncSplit)
                {
                    // we need to start a new async stage to hold the single affected stage.
                    CommandFile originalFile = executor.CurrentFile;
                    string currentFunction = executor.async.CurrentFunction.escapedFunctionName;
                    int nextStage = executor.async.CurrentFunction.NextStageIndex;
                    string nextStageName = AsyncStage.NameStageFunction(currentFunction, nextStage, true);
                    executor.AddCommand(prepend + Command.Function(nextStageName));

                    // TODO: when I get back
                    // there's a possibility the fix I made this morning causes normal if-statements not to work as a new
                    // stage isn't allocated for when it ends. make sure they work, as well as when explicit `else` is used

                    // start the new one
                    executor.async.CurrentFunction.FinishStageImmediate();
                    executor.async.CurrentFunction.StartNewStage();

                    // defer ending the stage until next statement
                    executor.DeferAction((e) =>
                    {
                        if (elseInfo == null)
                            throw new Exception("Did not get PreviousComparisonStructure for async branch evaluation.");

                        AsyncStage closingStage = e.async.CurrentFunction.ActiveStage;
                        string elseCommand = new ExecuteBuilder()
                            .WithSubcommand(new SubcommandUnless(elseInfo.conditionalUsed))
                            .Run(Command.Function(closingStage.file));
                        originalFile.Add(elseCommand);
                    });
                    return;
                }
            }
            
            executor.AppendCommandPrepend(prepend);
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

        private bool IsEmpty => this.Count == 0;
    }

    /// <summary>
    /// Represents a generic comparison in an if-statement.
    /// </summary>
    public abstract class Comparison
    {
        private readonly bool originallyInverted;

        /// <summary>
        /// Encodes a selector's hash and depth together with a prefix. (not actually 'encoding')
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="depth"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static string DepthEncode(string prefix, int depth, Selector selector)
        {
            return prefix + depth + '_' + selector.GetHashCode().ToString().Replace('-', '0');
        }
        protected Comparison(bool invert)
        {
            this.originallyInverted = invert;
            this.inverted = invert;
        }

        /// <summary>
        /// If this comparison is inverted.
        /// </summary>
        protected bool inverted;
        
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
