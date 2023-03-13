using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                        TryAssembleLine(new List<Token>(buffer), ref statements);
                        buffer.Clear();
                    }
                    if (current is TokenOpenBlock)
                    {
                        StatementOpenBlock block = new StatementOpenBlock(statements.Count + 1, null);
                        block.SetSource(current.lineNumber, "{");

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
                        closer.SetSource(current.lineNumber, "}");

                        if (blocks.Count == 0)
                        {
                            throw new TokenizerException("Unused closing bracket.", current.lineNumber);
                        }

                        StatementOpenBlock opener = blocks.Pop();
                        opener.statementsInside = statements.Count - opener.statementsInside;

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
                throw new TokenizerException("No closing bracket for opening bracket.", highestLevelOpener.Line);

            if (buffer.Count > 0)
            {
                TryAssembleLine(new List<Token>(buffer), ref statements);
                buffer.Clear();
            }

            return statements.ToArray();
        }
        public static void TryAssembleLine(List<Token> line, ref List<Statement> statements)
        {
            Token firstToken = line[0];

            if(firstToken is TokenComment)
            {
                TokenComment comment = firstToken as TokenComment;
                Statement add = new StatementComment(comment.contents);
                add.SetSource(firstToken.lineNumber, "#" + comment.contents);
                statements.Add(add);
                return;
            }
            if (firstToken is TokenDirective)
            {
                List<Token> rest = line.Skip(1).ToList();
                Directive directive = (firstToken as TokenDirective).directive;
                StatementDirective add = new StatementDirective(directive, rest.ToArray());
                add.SetSource(firstToken.lineNumber, string.Join(" ", from t in line select t.AsString()));
                statements.Add(add);
                return;
            }

            if (line.Count <= 1 || !(firstToken is TokenIdentifier))
            {
                StatementUnknown unknown = new StatementUnknown(line.ToArray());
                unknown.SetSource(firstToken.lineNumber, string.Join(" ", from t in line select t.AsString()));
                statements.Add(unknown);
                return;
            }

            TokenIdentifier identifier = firstToken as TokenIdentifier;
            Token secondToken = line[1];

            // strip any indexers so 'secondToken' is actually the next meaningful unit of information.
            // keep in mind that 'secondToken' is not necessarily the next sequential token.
            if(secondToken is TokenIndexer && line.Count > 2)
            {
                int index = 2;

                do secondToken = line[index++];
                while(secondToken is TokenIndexer && line.Count > index);
            }


            if (secondToken is IAssignment)
            {
                StatementOperation statement = new StatementOperation(line.ToArray());
                statement.SetSource(firstToken.lineNumber, string.Join(" ", from t in line select t.AsString()));
                statements.Add(statement);
            }
            else if (secondToken is TokenOpenParenthesis)
            {
                StatementFunctionCall statement = new StatementFunctionCall(line.ToArray());
                statement.SetSource(firstToken.lineNumber, string.Join(" ", from t in line select t.AsString()));
                statements.Add(statement);
            }
            else
            {
                StatementUnknown unknown = new StatementUnknown(line.ToArray());
                unknown.SetSource(firstToken.lineNumber, string.Join(" ", from t in line select t.AsString()));
                statements.Add(unknown);
                return;
            }
        }
    }
}
