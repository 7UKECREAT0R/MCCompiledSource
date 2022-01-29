﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A directive that is the root of a statement.
    /// </summary>
    public class Directive
    {
        /// <summary>
        /// An implementation of a directive call.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="tokens"></param>
        public delegate void DirectiveImpl(Executor executor, Statement tokens);


        private static short nextIndex = 0;
        internal Directive(DirectiveImpl call, string identifier,
            string fullName, params TypePattern[] patterns)
        {
            index = nextIndex++;
            this.call = call;
            this.identifier = identifier;
            this.fullName = fullName;
            this.patterns = patterns;
        }

        /// <summary>
        /// Get the key that should be used in a dictionary.
        /// </summary>
        public string DictValue
        {
            get
            {
                return identifier.ToUpper();
            }
        }

        public readonly short index;
        public readonly string identifier;
        public readonly string fullName;
        public readonly DirectiveImpl call;
        public readonly TypePattern[] patterns;

        public override int GetHashCode() => identifier.GetHashCode();
    }
    public static class Directives
    {
        public static Directive[] REGISTRY =
        {
            new Directive(DirectiveImplementations._var, "$var", "Set Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._inc, "$inc", "Increment Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._dec, "$dec", "Decrement Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._add, "$add", "Add to Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._sub, "$sub", "Subtract from Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._mul, "$mul", "Multiply with Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._div, "$div", "Divide Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._mod, "$mod", "Modulo Preprocessor Variable",
                new TypePattern(typeof(TokenIdentifier), typeof(IObjectable))),
            new Directive(DirectiveImplementations._swap, "$swap", "Swap Preprocessor Variables",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._if, "$if", "Preprocessor If",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenCompare), typeof(IObjectable))),
            new Directive(DirectiveImplementations._else, "$else", "Preprocessor Else"),
            new Directive(DirectiveImplementations._repeat, "$repeat", "Preprocessor Repeat",
                new TypePattern(typeof(TokenIntegerLiteral)).Optional<TokenIdentifier>()),
            new Directive(DirectiveImplementations._log, "$log", "Preprocessor Log to Console",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations._macro, "$macro", "Define/Call Preprocessor Macro",
                new TypePattern(typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._include, "$include", "Include other File",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations._strfriendly, "$strfriendly", "Preprocessor String Friendly Name",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._strupper, "$strupper", "Preprocessor String Uppercase",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),
            new Directive(DirectiveImplementations._strlower, "$strlower", "Preprocessor String Lowercase",
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier))),

            new Directive(DirectiveImplementations.mc, "mc", "Minecraft Command",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.select, "select", "Select Target",
                new TypePattern(typeof(TokenSelectorLiteral))),
            new Directive(DirectiveImplementations.print, "print", "Print to All Chat",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.printp, "printp", "Print to Selected Entity",
                new TypePattern(typeof(TokenStringLiteral))),
            new Directive(DirectiveImplementations.define, "define", "Define Variable",
                new TypePattern(typeof(TokenIdentifierStruct), typeof(TokenStringLiteral)),
                new TypePattern().Optional<TokenIdentifier>().Optional<TokenIntegerLiteral>().And<TokenStringLiteral>()),
            new Directive(DirectiveImplementations.init, "init", "Initialize Variable to 0",
                new TypePattern(typeof(TokenIdentifierValue))),
            new Directive(DirectiveImplementations.@if, "if", "If Directive",
                new TypePattern(typeof(TokenIdentifierValue), typeof(TokenCompare), typeof(Token)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenStringLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenIntegerLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenStringLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifierEnum)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIntegerLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenStringLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenIntegerLiteral)).Optional<TokenIntegerLiteral>(),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral), typeof(TokenIntegerLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenStringLiteral)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenIdentifierEnum)),
                new TypePattern(typeof(TokenIdentifier), typeof(TokenIdentifier), typeof(TokenIntegerLiteral)).Optional<TokenIntegerLiteral>()),
            new Directive(DirectiveImplementations.@else, "else", "Else Directive"),
            new Directive(DirectiveImplementations.give, "give", "Give Item to Selected",
                new TypePattern(typeof(TokenStringLiteral)).Optional<TokenIntegerLiteral>().Optional<TokenIntegerLiteral>()),
            new Directive(DirectiveImplementations.tp, "tp", "Teleport Selected Entity",
                new TypePattern(typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral), typeof(TokenCoordinateLiteral))),
            new Directive(DirectiveImplementations.face, "face", "Face Selected Entity in Direction"),
            new Directive(DirectiveImplementations.place, "place", "Place Block"),
            new Directive(DirectiveImplementations.fill, "fill", "Fill Region of Blocks"),
            new Directive(DirectiveImplementations.replace, "replace", "Replace Region of Blocks"),
            new Directive(DirectiveImplementations.kill, "kill", "Kill Selected Entity"),
            new Directive(DirectiveImplementations.title, "title", "Show Title"),
            new Directive(DirectiveImplementations.halt, "halt", "Halt Execution"),
        };

        static readonly Dictionary<string, Directive> directiveLookup = new Dictionary<string, Directive>();
        static Directives()
        {
            foreach (Directive directive in REGISTRY)
                directiveLookup.Add(directive.DictValue, directive);
        }

        /// <summary>
        /// Query for a directive that matches this token contents. Case insensitive.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Directive Query(string token)
        {
            if (directiveLookup.TryGetValue(token.ToUpper(), out Directive directive))
                return directive;
            return null;
        }
    }
}
