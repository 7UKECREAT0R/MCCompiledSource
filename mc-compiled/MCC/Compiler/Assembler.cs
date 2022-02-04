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

            // fetch all lines into a nice array
            int i = -1;
            while(++i < tokens.Length)
            {
                Token current = tokens[i];

                if (current is ITerminating)
                {
                    if (buffer.Count > 0)
                    {
                        lines.Add(new List<Token>(buffer));
                        buffer.Clear();
                    }
                    continue;
                }

                buffer.Add(current);
            }
            if (buffer.Count > 0)
            {
                lines.Add(new List<Token>(buffer));
                buffer.Clear();
            }

            // parse the collected lines into a set of statements
            foreach(List<Token> line in lines)
            {
                Token firstToken = line[0];

            }

            return statements.ToArray();
        }
    }
}
