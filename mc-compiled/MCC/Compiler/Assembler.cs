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

            if(firstToken is TokenComment)
            {
                TokenComment comment = firstToken as TokenComment;
                Statement add = new StatementComment(comment.contents);
                if(includeSource)
                    add.SetSource(new[] { firstToken.lineNumber }, "#" + comment.contents);
                return add;
            }
            if (firstToken is TokenDirective)
            {
                Token[] rest = line.Skip(1).ToArray();
                Directive directive = (firstToken as TokenDirective).directive;
                StatementDirective add = new StatementDirective(directive, rest);
                if(includeSource)
                    add.SetSource(new[] { firstToken.lineNumber }, string.Join(" ", from t in line select t.AsString()));
                return add;
            }

            if (line.Length <= 1 || !(firstToken is TokenIdentifier))
            {
                StatementUnknown unknown = new StatementUnknown(line);
                if(includeSource)
                    unknown.SetSource(new[] { firstToken.lineNumber }, string.Join(" ", from t in line select t.AsString()));
                return unknown;
            }

            TokenIdentifier identifier = firstToken as TokenIdentifier;
            Token secondToken = line[1];

            // strip any indexers so 'secondToken' is actually the next meaningful unit of information.
            // keep in mind that 'secondToken' is not necessarily the next sequential token.
            if(secondToken is TokenIndexer && line.Length > 2)
            {
                int index = 2;

                do secondToken = line[index++];
                while(secondToken is TokenIndexer && line.Length > index);
            }


            if (secondToken is IAssignment)
            {
                StatementOperation statement = new StatementOperation(line);
                if(includeSource)
                    statement.SetSource(new[] { firstToken.lineNumber }, string.Join(" ", from t in line select t.AsString()));
                return statement;
            }
            else if (secondToken is TokenOpenParenthesis)
            {
                StatementFunctionCall statement = new StatementFunctionCall(line);
                if (includeSource)
                    statement.SetSource(new[] { firstToken.lineNumber }, string.Join(" ", from t in line select t.AsString()));
                return statement;
            }
            else
            {
                StatementUnknown unknown = new StatementUnknown(line);
                if (includeSource)
                    unknown.SetSource(new[] { firstToken.lineNumber }, string.Join(" ", from t in line select t.AsString()));
                return unknown;
            }
        }
    }
}
