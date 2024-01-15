using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// Utility that assembles arbitrary tokens into full, comprehensive statements.
    /// </summary>
    public static class Assembler
    {
        public static Statement[] AssembleTokens(Token[] tokens)
        {
            List<Statement> statements = new List<Statement>();
            List<Token> buffer = new List<Token>();
            Stack<StatementOpenBlock> blocks = new Stack<StatementOpenBlock>();

            // used for exceptions
            int highestLevelOpenerCount = 0;
            StatementOpenBlock highestLevelOpener = null;

            // assemble all lines via TryAssembleLine()
            int i = -1;
            while(++i < tokens.Length)
            {
                Token current = tokens[i];

                if (current is ITerminating)
                {
                    if (buffer.Count > 0)
                    {
                        Statement assembledStatement = TryAssembleLine(buffer.ToArray());
                        statements.Add(assembledStatement);
                        buffer.Clear();
                    }
                    if (current is TokenOpenBlock)
                    {
                        StatementOpenBlock block = new StatementOpenBlock(statements.Count + 1, null);
                        block.SetSource(new[] { current.lineNumber }, "{");

                        statements.Add(block);
                        blocks.Push(block);

                        // Track highest level opening bracket 
                        int count = blocks.Count;
                        if(count >= highestLevelOpenerCount)
                        {
                            highestLevelOpenerCount = count;
                            highestLevelOpener = block;
                        }
                        continue;
                    }
                    else if (current is TokenCloseBlock)
                    {
                        StatementCloseBlock closer = new StatementCloseBlock();
                        closer.SetSource(new[] { current.lineNumber }, "}");

                        if (blocks.Count == 0)
                        {
                            throw new TokenizerException("Unused closing bracket.", new[] { current.lineNumber });
                        }

                        StatementOpenBlock opener = blocks.Pop();
                        int statementCountRaw = statements.Count - opener.statementsInside;
                        int statementCount = statements
                            .Skip(opener.statementsInside)
                            .Count(s => !(s is StatementComment));

                        opener.statementsInside = statementCountRaw;
                        opener.meaningfulStatementsInside = statementCount;

                        // pointers
                        opener.closer = closer;
                        closer.opener = opener;

                        statements.Add(closer);
                        continue;
                    }
                    continue;
                } else
                    buffer.Add(current);
            }

            if(blocks.Count > 0)
                throw new TokenizerException("No closing bracket for opening bracket.", highestLevelOpener.Lines);

            if (buffer.Count > 0)
            {
                Statement assembledStatement = TryAssembleLine(buffer.ToArray());
                statements.Add(assembledStatement);
                buffer.Clear();
            }

            return statements.ToArray();
        }
        public static Statement TryAssembleLine(Token[] line, bool includeSource = true)
        {
            Token firstToken = line[0];

            switch (firstToken)
            {
                case TokenComment comment:
                {
                    Statement add = new StatementComment(comment.contents);
                    if(includeSource)
                        add.SetSource(new[] { comment.lineNumber }, "#" + comment.contents);
                    return add;
                }
                case TokenDirective tokenDirective:
                {
                    Token[] rest = line.Skip(1).ToArray();
                    Directive directive = tokenDirective.directive;
                    var add = new StatementDirective(directive, rest);
                    if(includeSource)
                        add.SetSource(new[] { tokenDirective.lineNumber }, string.Join(" ", from t in line select t.AsString()));
                    return add;
                }
            }

            if (line.Length <= 1 || !(firstToken is TokenIdentifier identifier))
            {
                var unknown = new StatementUnknown(line);
                if(includeSource)
                    unknown.SetSource(new[] { firstToken.lineNumber }, string.Join(" ", from t in line select t.AsString()));
                return unknown;
            }

            Token secondToken = line[1];

            // strip any indexers so 'secondToken' is actually the next meaningful unit of information.
            // keep in mind that 'secondToken' is not necessarily the next sequential token.
            if(secondToken is TokenIndexer && line.Length > 2)
            {
                int index = 2;
                
                do
                    secondToken = line[index++];
                while(secondToken is TokenIndexer && line.Length > index);
            }

            for (int lineIndex = 1; lineIndex < line.Length; lineIndex++)
            {
                bool endOfLine = lineIndex == line.Length - 1;
                Token currentToken = line[lineIndex];

                if(currentToken is TokenIndexer)
                {
                    if (endOfLine)
                        break;
                    continue;
                }
                
                // want to find an assignment token
                if (!(currentToken is IAssignment))
                    continue;
                
                var statement = new StatementOperation(line);
                if(includeSource)
                    statement.SetSource(new[] { identifier.lineNumber }, string.Join(" ", from t in line select t.AsString()));
                return statement;
            }
            
            if (secondToken is TokenOpenParenthesis)
            {
                var statement = new StatementFunctionCall(line);
                if (includeSource)
                    statement.SetSource(new[] { identifier.lineNumber }, string.Join(" ", from t in line select t.AsString()));
                return statement;
            }
            else
            {
                var unknown = new StatementUnknown(line);
                if (includeSource)
                    unknown.SetSource(new[] { identifier.lineNumber }, string.Join(" ", from t in line select t.AsString()));
                return unknown;
            }
        }
    }
}
