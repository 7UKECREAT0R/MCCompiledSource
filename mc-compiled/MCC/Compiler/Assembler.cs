using System.Collections.Generic;
using System.Linq;
using mc_compiled.MCC.Language;

namespace mc_compiled.MCC.Compiler;

/// <summary>
///     Utility that assembles arbitrary tokens into full, comprehensive statements.
/// </summary>
public static class Assembler
{
    /// <summary>
    ///     Assemble an array of tokens into a complete set of statements.
    /// </summary>
    /// <param name="tokens">The tokens to assemble.</param>
    /// <param name="fileName">
    ///     The name of the file which these tokens came from. <br />
    ///     Used for setting <see cref="Statement.SourceFile" />.
    /// </param>
    /// <returns></returns>
    /// <exception cref="TokenizerException"></exception>
    public static Statement[] AssembleTokens(Token[] tokens, string fileName)
    {
        var statements = new List<Statement>();
        var buffer = new List<Token>();
        var blocks = new Stack<StatementOpenBlock>();

        // used for exceptions
        int highestLevelOpenerCount = 0;
        StatementOpenBlock highestLevelOpener = null;

        // assemble all lines via TryAssembleLine()
        int i = -1;
        Statement assembledStatement;
        while (++i < tokens.Length)
        {
            Token current = tokens[i];

            if (current is ITerminating)
            {
                if (buffer.Count > 0)
                {
                    assembledStatement = TryAssembleLine(buffer.ToArray(), fileName);
                    statements.Add(assembledStatement);
                    buffer.Clear();
                }

                switch (current)
                {
                    case TokenOpenBlock:
                    {
                        var block = new StatementOpenBlock(statements.Count + 1);
                        block.SetSource([current.lineNumber], "{", fileName);

                        statements.Add(block);
                        blocks.Push(block);

                        // Track highest level opening bracket 
                        int count = blocks.Count;
                        if (count >= highestLevelOpenerCount)
                        {
                            highestLevelOpenerCount = count;
                            highestLevelOpener = block;
                        }

                        continue;
                    }
                    case TokenCloseBlock:
                    {
                        var closer = new StatementCloseBlock();
                        closer.SetSource([current.lineNumber], "}", fileName);

                        if (blocks.Count == 0)
                            throw new TokenizerException("Unused closing bracket.", [current.lineNumber]);

                        if (!blocks.TryPop(out StatementOpenBlock opener))
                            throw new TokenizerException("Closing bracket without an associated open bracket.",
                                [current.lineNumber]);

                        int statementCountRaw = statements.Count - opener.statementsInside;
                        int statementCount = statements
                            .Skip(opener.statementsInside)
                            .Count(s => s is not StatementComment);

                        opener.statementsInside = statementCountRaw;
                        opener.meaningfulStatementsInside = statementCount;

                        // pointers
                        opener.closer = closer;
                        closer.opener = opener;

                        statements.Add(closer);
                        continue;
                    }
                    default:
                        continue;
                }
            }

            buffer.Add(current);
        }

        if (blocks.Count > 0)
            throw new TokenizerException("No closing bracket for opening bracket.", highestLevelOpener?.Lines ??
            [
                -1
            ]);

        if (buffer.Count <= 0)
            return StatementsPrepass(statements);

        assembledStatement = TryAssembleLine(buffer.ToArray(), fileName);
        statements.Add(assembledStatement);
        buffer.Clear();

        return StatementsPrepass(statements);
    }
    /// <summary>
    ///     Runs a prepass over the given statements list before returning it as an array of statements.
    ///     As of now, this process:
    ///     <ul>
    ///         <li>
    ///             Places virtual blocks around any statements following directive calls which have the
    ///             <see cref="DirectiveAttribute.FORCE_CODE_BLOCK" /> attribute.
    ///         </li>
    ///     </ul>
    /// </summary>
    /// <param name="statements">The list of statements to run the prepass over. The list will be modified.</param>
    /// <returns>An array of <see cref="Statement" />s representing the input <paramref name="statements" /> after the prepass.</returns>
    private static Statement[] StatementsPrepass(List<Statement> statements)
    {
        // creating virtual code blocks surrounding single statements following directive calls with the FORCE_CODE_BLOCK attribute
        // iterates backwards to propagate changes to higher-scope statements to lower-scope ones
        bool firstIteration = true;
        for (int i = statements.Count - 1; i >= 1; i--)
        {
            Statement nextStatement = statements[i - 1];

            if (!nextStatement.HasAttribute(DirectiveAttribute.FORCE_CODE_BLOCK))
            {
                firstIteration = false;
                continue; // not a directive with FORCE_CODE_BLOCK, leave as-is
            }

            Statement currentStatement = statements[i];
            if (currentStatement is StatementOpenBlock)
                continue; // already a block

            Statement previousStatement = firstIteration ? null : statements[i + 1];
            firstIteration = false;

            int statementCount = 1;

            // we also want to take into account if the PREVIOUS statement is a block
            // (for example, the block following a function definition)
            if (previousStatement is StatementOpenBlock previousOpenBlock)
                statementCount += previousOpenBlock.statementsInside + 2;

            var virtualOpener = new StatementOpenBlock(statementCount);
            var virtualCloser = new StatementCloseBlock();
            virtualOpener.closer = virtualCloser;
            virtualCloser.opener = virtualOpener;
            virtualOpener.SetSource(currentStatement.Lines, "/* virtual */ {", currentStatement.SourceFile);
            virtualCloser.SetSource(currentStatement.Lines, "} /* virtual */", currentStatement.SourceFile);

            if (statementCount == 0)
                virtualOpener.meaningfulStatementsInside = 0;
            if (statementCount == 1)
            {
                bool isCurrentMeaningful = currentStatement is not StatementComment;
                virtualOpener.meaningfulStatementsInside = isCurrentMeaningful ? 1 : 0;
            }
            else
            {
                int meaningfulStatements = statements.Skip(i).Take(statementCount)
                    .Count(s => s is not StatementComment);
                virtualOpener.meaningfulStatementsInside = meaningfulStatements;
            }

            statements.Insert(i + statementCount, virtualCloser);
            statements.Insert(i, virtualOpener);
        }

        return statements.ToArray();
    }

    private static Statement TryAssembleLine(Token[] line, string fileName)
    {
        Token firstToken = line[0];

        switch (firstToken)
        {
            case TokenComment comment:
            {
                Statement add = new StatementComment(comment.contents);
                add.SetSource([comment.lineNumber], "#" + comment.contents, fileName);
                return add;
            }
            case TokenDirective tokenDirective:
            {
                Token[] rest = line.Skip(1).ToArray();
                Directive directive = tokenDirective.directive;
                var add = new StatementDirective(directive, rest);
                add.SetSource([tokenDirective.lineNumber], string.Join(" ", from t in line select t.AsString()),
                    fileName);
                return add;
            }
        }

        if (line.Length <= 1 || firstToken is not TokenIdentifier identifier)
        {
            var unknown = new StatementUnknown(line);
            unknown.SetSource([firstToken.lineNumber], string.Join(" ", from t in line select t.AsString()), fileName);
            return unknown;
        }

        Token secondToken = line[1];

        // strip any indexers so 'secondToken' is actually the next meaningful unit of information.
        // keep in mind that 'secondToken' is not necessarily the next sequential token.
        if (secondToken is TokenIndexer && line.Length > 2)
        {
            int index = 2;

            do
            {
                secondToken = line[index++];
            } while (secondToken is TokenIndexer && line.Length > index);
        }

        for (int lineIndex = 1; lineIndex < line.Length; lineIndex++)
        {
            bool endOfLine = lineIndex == line.Length - 1;
            Token currentToken = line[lineIndex];

            if (currentToken is TokenIndexer)
            {
                if (endOfLine)
                    break;
                continue;
            }

            // want to find an assignment token
            if (currentToken is not IAssignment)
                continue;

            var statement = new StatementOperation(line);
            statement.SetSource([identifier.lineNumber], string.Join(" ", from t in line select t.AsString()),
                fileName);
            return statement;
        }

        if (secondToken is TokenOpenParenthesis)
        {
            var statement = new StatementFunctionCall(line);
            statement.SetSource([identifier.lineNumber], string.Join(" ", from t in line select t.AsString()),
                fileName);
            return statement;
        }

        {
            var unknown = new StatementUnknown(line);
            unknown.SetSource([identifier.lineNumber], string.Join(" ", from t in line select t.AsString()), fileName);
            return unknown;
        }
    }
}