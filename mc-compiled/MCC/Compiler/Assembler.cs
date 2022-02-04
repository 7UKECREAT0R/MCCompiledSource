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
            List<List<Token>> lines = new List<List<Token>>();
            List<Token> buffer = new List<Token>();

            Stack<StatementOpenBlock> blocks = new Stack<StatementOpenBlock>();

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
                        statements.Add(block);
                        blocks.Push(block);
                        continue;
                    }
                    else if (current is TokenCloseBlock)
                    {
                        StatementCloseBlock closeBlock = new StatementCloseBlock();
                        StatementOpenBlock opener = blocks.Pop();
                        opener.statementsInside = statements.Count - opener.statementsInside;
                        statements.Add(closeBlock);
                        continue;
                    }
                    continue;
                } else
                    buffer.Add(current);
            }
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

            if (firstToken is TokenDirective)
            {
                Token[] rest = line.Skip(1).ToArray();
                Directive directive = (firstToken as TokenDirective).directive;
                StatementDirective add = new StatementDirective(directive, rest);
                statements.Add(add);
                return;
            }

            if (line.Count <= 1)
            {
                if (Program.DEBUG)
                    Console.WriteLine($"Skipping garbage token {firstToken} because it's not valid alone.");
                return;
            }
            if (!(firstToken is TokenIdentifier))
            {
                if (Program.DEBUG)
                    Console.WriteLine($"Skipping garbage tokens beginning with {firstToken} because it's not a valid identifier.");
                return;
            }

            TokenIdentifier identifier = firstToken as TokenIdentifier;
            Token secondToken = line[1];

            if (secondToken is IAssignment)
                statements.Add(new StatementOperation(line.ToArray()));
            else if (secondToken is TokenOpenParenthesis)
                statements.Add(new StatementFunctionCall(line.ToArray()));
            else
            {
                if (Program.DEBUG)
                    Console.WriteLine($"Skipping garbage tokens beginning with {firstToken} {secondToken} because it's unidentifiable.");
                return;
            }
        }
    }
}
