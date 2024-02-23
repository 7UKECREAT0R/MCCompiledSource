using mc_compiled.MCC.Functions.Types;
using System.Collections.Generic;

namespace mc_compiled.MCC.Compiler.Implementations.Functions
{
    internal abstract class FunctionGlyph : CompiletimeFunction
    {
        const int GLYPH_WIDTH = 0x10;
        protected readonly int baseCharacter;
        protected FunctionGlyph(int baseCharacter, string aliasedName, string name, string documentation) : base(aliasedName, name, "string", documentation)
        {
            this.baseCharacter = baseCharacter << 8;
            AddParameters(
                new CompiletimeFunctionParameter<TokenIntegerLiteral>("x"),
                new CompiletimeFunctionParameter<TokenIntegerLiteral>("y", new TokenIntegerLiteral(0, IntMultiplier.none, 0))
            );
        }

        public override Token CallFunction(List<string> commandBuffer, Executor executor, Statement statement)
        {
            int x = (this.Parameters[0] as CompiletimeFunctionParameter).CurrentValue as TokenIntegerLiteral;
            int y = (this.Parameters[1] as CompiletimeFunctionParameter).CurrentValue as TokenIntegerLiteral;
            int offset = y * GLYPH_WIDTH + x;
            char character = (char)(this.baseCharacter | offset);
            return new TokenStringLiteral(character.ToString(), statement.Lines[0]);
        }
    }
    internal sealed class FunctionGlyphE0 : FunctionGlyph
    {
        internal FunctionGlyphE0() : base(0xE0, "glyphE0", "accessGlyphE0_at", "Returns the character in the glyph_E0 file at the given coordinates. If one number is specified, it acts as the x coordinate of the icon, wrapping around LTR-TTB. If the second number is specified, it acts as the Y-coordinate of the icon.")
        {
        }
    }
    internal sealed class FunctionGlyphE1 : FunctionGlyph
    {
        internal FunctionGlyphE1() : base(0xE1, "glyphE1", "accessGlyphE1_at", "Returns the character in the glyph_E1 file at the given coordinates. If one number is specified, it acts as the x coordinate of the icon, wrapping around LTR-TTB. If the second number is specified, it acts as the Y-coordinate of the icon.")
        {
        }
    }
}
