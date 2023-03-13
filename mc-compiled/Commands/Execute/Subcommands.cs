using mc_compiled.Commands.Selectors;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace mc_compiled.Commands.Execute
{
    /// <summary>
    /// [Flags] Represents an alignment to x/y/z.
    /// </summary>
    [Flags]
    public enum Axes : byte
    {
        none = 0,
        x = 1 << 0,
        y = 1 << 1,
        z = 1 << 2
    }
    internal class SubcommandAlign : Subcommand
    {
        /// <summary>
        /// Parse a set of axes based on a string input.
        /// </summary>
        /// <returns></returns>
        internal static Axes ParseAxes(string input)
        {
            char[] characters = input.ToCharArray();
            Axes axes = Axes.none;

            foreach (char c in characters)
            {
                switch (c)
                {
                    case 'x':
                        axes |= Axes.x;
                        break;
                    case 'y':
                        axes |= Axes.y;
                        break;
                    case 'z':
                        axes |= Axes.z;
                        break;
                }
            }

            return axes;
        }
        /// <summary>
        /// Convert a set of axes flags to
        /// </summary>
        /// <param name="axes"></param>
        /// <returns></returns>
        internal static string FromAxes(Axes axes)
        {
            StringBuilder sb = new StringBuilder();

            if((axes & Axes.x) != 0)
                sb.Append("x");
            if ((axes & Axes.y) != 0)
                sb.Append("y");
            if ((axes & Axes.z) != 0)
                sb.Append("z");

            return sb.ToString();
        }

        internal Axes axes { get; private set; }

        internal SubcommandAlign(Axes axes)
        {
            this.axes = axes;
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            new TypePattern(
                new NamedType(typeof(TokenStringLiteral), "axes")
            )
        };
        public override string Keyword => "align";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            TokenStringLiteral literal = tokens.Next<TokenStringLiteral>();
            string axesString = literal.text;
            this.axes = ParseAxes(axesString);
        }
        public override string ToMinecraft() => $"align {FromAxes(this.axes)}";
    }
    internal class SubcommandAnchored : Subcommand
    {
        internal AnchorPosition anchor;

        internal SubcommandAnchored(AnchorPosition anchor)
        {
            this.anchor = anchor;
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            new TypePattern(
                new NamedType(typeof(TokenIdentifierEnum), "anchor")
            )
        };
        public override string Keyword => "anchored";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            TokenIdentifierEnum @enum = tokens.Next<TokenIdentifierEnum>();
            ParsedEnumValue parsedEnum = @enum.value;

            parsedEnum.RequireType<AnchorPosition>(tokens);

            anchor = (AnchorPosition)parsedEnum.value;
        }
        public override string ToMinecraft() => $"anchored {anchor}";
    }
    internal class SubcommandAs : Subcommand
    {
        internal Selector entity;

        internal SubcommandAs(Selector entity)
        {
            this.entity = entity;
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            new TypePattern(
                new NamedType(typeof(TokenSelectorLiteral), "target")
            )
        };
        public override string Keyword => "as";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
            this.entity = selector.selector;
        }
        public override string ToMinecraft() => $"as {entity.ToString()}";
    }
    internal class SubcommandAt : Subcommand
    {
        internal Selector entity;

        internal SubcommandAt(Selector entity)
        {
            this.entity = entity;
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            new TypePattern(
                new NamedType(typeof(TokenSelectorLiteral), "target")
            )
        };
        public override string Keyword => "at";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            TokenSelectorLiteral selector = tokens.Next<TokenSelectorLiteral>();
            this.entity = selector.selector;
        }
        public override string ToMinecraft() => $"at {entity.ToString()}";
    }
    internal class SubcommandFacing : Subcommand
    {
        internal bool isEntity;

        internal Selector entity;
        internal AnchorPosition anchor;

        internal Coord x, y, z;

        internal SubcommandFacing(bool isEntity, Selector entity, AnchorPosition anchor, Coord x, Coord y, Coord z)
        {
            this.isEntity = isEntity;
            this.entity = entity;
            this.anchor = anchor;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            // coordinate
            new TypePattern(
                new NamedType(typeof(TokenCoordinateLiteral), "x"),
                new NamedType(typeof(TokenCoordinateLiteral), "y"),
                new NamedType(typeof(TokenCoordinateLiteral), "z")
            ),
            // entity
            new TypePattern(
                new NamedType(typeof(TokenSelectorLiteral), "entity"),
                new NamedType(typeof(TokenIdentifierEnum), "anchor")
            )
        };
        public override string Keyword => "facing";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            // entity
            if(tokens.NextIs<TokenSelectorLiteral>())
            {
                isEntity = true;

                this.entity = tokens.Next<TokenSelectorLiteral>();

                ParsedEnumValue parsedEnum = tokens.Next<TokenIdentifierEnum>().value;
                parsedEnum.RequireType<AnchorPosition>(tokens);
                this.anchor = (AnchorPosition)parsedEnum.value;
                return;
            }

            // coordinate
            isEntity = false;

            this.x = tokens.Next<TokenCoordinateLiteral>();
            this.y = tokens.Next<TokenCoordinateLiteral>();
            this.z = tokens.Next<TokenCoordinateLiteral>();
        }
        public override string ToMinecraft()
        {
            if (isEntity)
                return $"facing entity {entity.ToString()} {anchor}";

            return $"facing {x} {y} {z}";
        }
    }
    internal class SubcommandIn : Subcommand
    {
        internal Dimension dimension;

        internal SubcommandIn(Dimension dimension)
        {
            this.dimension = dimension;
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            new TypePattern(
                new NamedType(typeof(TokenIdentifierEnum), "dimension")
            )
        };
        public override string Keyword => "in";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            TokenIdentifierEnum @enum = tokens.Next<TokenIdentifierEnum>();
            ParsedEnumValue parsedEnum = @enum.value;

            parsedEnum.RequireType<Dimension>(tokens);

            dimension = (Dimension)parsedEnum.value;
        }
        public override string ToMinecraft() => $"in {dimension}";
    }
    internal class SubcommandPositioned : Subcommand
    {
        internal bool asEntity;
        internal Selector entity;
        internal Coord x, y, z;

        internal SubcommandPositioned(bool asEntity, Selector entity, Coord x, Coord y, Coord z)
        {
            this.asEntity = asEntity;
            this.entity = entity;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            // coordinate
            new TypePattern(
                new NamedType(typeof(TokenCoordinateLiteral), "x"),
                new NamedType(typeof(TokenCoordinateLiteral), "y"),
                new NamedType(typeof(TokenCoordinateLiteral), "z")
            ),
            // entity
            new TypePattern(
                new NamedType(typeof(TokenSelectorLiteral), "entity")
            )
        };
        public override string Keyword => "positioned";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            // entity
            if(tokens.NextIs<TokenSelectorLiteral>())
            {
                asEntity = true;
                this.entity = tokens.Next<TokenSelectorLiteral>();
                return;
            }

            // coords
            this.x = tokens.Next<TokenCoordinateLiteral>();
            this.y = tokens.Next<TokenCoordinateLiteral>();
            this.z = tokens.Next<TokenCoordinateLiteral>();
        }
        public override string ToMinecraft()
        {
            if (asEntity)
                return $"positioned as {entity.ToString()}";

            return $"positioned {x} {y} {z}";
        }
    }
    internal class SubcommandRotated : Subcommand
    {
        internal bool asEntity;
        internal Selector entity;
        internal Coord yaw, pitch;

        internal SubcommandRotated(bool asEntity, Selector entity, Coord yaw, Coord pitch)
        {
            this.asEntity = asEntity;
            this.entity = entity;
            this.yaw = yaw;
            this.pitch = pitch;
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            // coordinate
            new TypePattern(
                new NamedType(typeof(TokenCoordinateLiteral), "yaw"),
                new NamedType(typeof(TokenCoordinateLiteral), "pitch")
            ),
            // entity
            new TypePattern(
                new NamedType(typeof(TokenSelectorLiteral), "entity")
            )
        };
        public override string Keyword => "rotated";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            // entity
            if (tokens.NextIs<TokenSelectorLiteral>())
            {
                asEntity = true;
                this.entity = tokens.Next<TokenSelectorLiteral>();
                return;
            }

            // coords
            this.yaw = tokens.Next<TokenCoordinateLiteral>();
            this.pitch = tokens.Next<TokenCoordinateLiteral>();
        }
        public override string ToMinecraft()
        {
            if (asEntity)
                return $"rotated as {entity.ToString()}";

            return $"rotated {yaw} {pitch}";
        }
    }
    internal class SubcommandRun : Subcommand
    {
        internal string command;

        internal SubcommandRun(string command)
        {
            this.command = command;
        }

        public override TypePattern[] Pattern => new TypePattern[] {
            new TypePattern(
                new NamedType(typeof(TokenStringLiteral), "command")
            )
        };
        public override string Keyword => "run";
        public override bool TerminatesChain => true;

        public override void FromTokens(Statement tokens)
        {
            command = tokens.Next<string>();
        }
        public override string ToMinecraft() => $"run {command}";
    }

    internal class SubcommandIf : Subcommand
    {
        internal ConditionalSubcommand condition;

        internal SubcommandIf(ConditionalSubcommand condition)
        {
            this.condition = condition;
        }

        public override TypePattern[] Pattern
        {
            get
            {
                IEnumerable<TypePattern> validPatterns = ConditionalSubcommand.CONDITIONAL_EXAMPLES.SelectMany(c => c.Pattern);

                foreach (TypePattern pattern in validPatterns)
                    pattern.PrependAnd(new NamedType(typeof(TokenIdentifier)), "subcommand");

                return validPatterns.ToArray();
            }
        }
        public override string Keyword => "if";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            string word = tokens.Next<TokenIdentifier>().word.ToUpper();

            // load condition
            this.condition = ConditionalSubcommand.GetSubcommandForKeyword(word, tokens);

            // load the statement's parameters based on the following input
            this.condition.FromTokens(tokens);
        }
        public override string ToMinecraft() => $"if {condition.ToMinecraft()}";
    }
    internal class SubcommandUnless : Subcommand
    {
        internal ConditionalSubcommand condition;

        internal SubcommandUnless(ConditionalSubcommand condition)
        {
            this.condition = condition;
        }

        public override TypePattern[] Pattern
        {
            get
            {
                IEnumerable<TypePattern> validPatterns = ConditionalSubcommand.CONDITIONAL_EXAMPLES.SelectMany(c => c.Pattern);

                foreach (TypePattern pattern in validPatterns)
                    pattern.PrependAnd(new NamedType(typeof(TokenIdentifier)), "subcommand");

                return validPatterns.ToArray();
            }
        }
        public override string Keyword => "unless";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            string word = tokens.Next<TokenIdentifier>().word.ToUpper();

            // load condition
            this.condition = ConditionalSubcommand.GetSubcommandForKeyword(word, tokens);

            // load the statement's parameters based on the following input
            this.condition.FromTokens(tokens);
        }
        public override string ToMinecraft() => $"unless {condition.ToMinecraft()}";
    }
}
