using mc_compiled.Commands.Selectors;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Commands.Execute
{
    internal class ConditionalSubcommandBlock : ConditionalSubcommand
    {
        internal Coord x, y, z;
        internal string block;
        internal int? data = null;

        public ConditionalSubcommandBlock() { }
        private ConditionalSubcommandBlock(Coord x, Coord y, Coord z, string block, int? data)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.block = block;
            this.data = data;
        }

        /// <summary>
        /// Create a ConditionalSubcommandBlock with the given parameters.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="block"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static ConditionalSubcommandBlock New(Coord x, Coord y, Coord z, string block, int? data = null)
        {
            return new ConditionalSubcommandBlock(x, y, z, block, data);
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            new TypePattern(
                new NamedType(typeof(TokenCoordinateLiteral), "x"),
                new NamedType(typeof(TokenCoordinateLiteral), "y"),
                new NamedType(typeof(TokenCoordinateLiteral), "z"),
                new NamedType(typeof(TokenStringLiteral), "block")
            )
        };
        public override string Keyword => "block";

        public override void FromTokens(Statement tokens)
        {
            x = tokens.Next<TokenCoordinateLiteral>();
            y = tokens.Next<TokenCoordinateLiteral>();
            z = tokens.Next<TokenCoordinateLiteral>();
            block = tokens.Next<TokenStringLiteral>();

            if(tokens.NextIs<TokenIntegerLiteral>())
                data = tokens.Next<TokenIntegerLiteral>();
        }
        public override string ToMinecraft()
        {
            if(data.HasValue)
                return $"block {x} {y} {z} {block} {data.Value}";

            return $"block {x} {y} {z} {block}";
        }
    }
    internal class ConditionalSubcommandBlocks : ConditionalSubcommand
    {
        internal Coord beginX, beginY, beginZ;
        internal Coord endX, endY, endZ;
        internal Coord destX, destY, destZ;
        internal BlocksScanMode scanMode;

        public ConditionalSubcommandBlocks() { }
        private ConditionalSubcommandBlocks(
            Coord beginX, Coord beginY, Coord beginZ,
            Coord endX, Coord endY, Coord endZ,
            Coord destX, Coord destY, Coord destZ,
            BlocksScanMode scanMode)
        {
            this.beginX = beginX;
            this.beginY = beginY;
            this.beginZ = beginZ;
            this.endX = endX;
            this.endY = endY;
            this.endZ = endZ;
            this.destX = destX;
            this.destY = destY;
            this.destZ = destZ;
            this.scanMode = scanMode;
        }

        /// <summary>
        /// Create a ConditionalSubcommandBlocks with the given parameters.
        /// </summary>
        /// <param name="beginX"></param>
        /// <param name="beginY"></param>
        /// <param name="beginZ"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <param name="endZ"></param>
        /// <param name="destX"></param>
        /// <param name="destY"></param>
        /// <param name="destZ"></param>
        /// <param name="scanMode"></param>
        /// <returns></returns>
        internal static ConditionalSubcommandBlocks New(
            Coord beginX, Coord beginY, Coord beginZ,
            Coord endX, Coord endY, Coord endZ,
            Coord destX, Coord destY, Coord destZ,
            BlocksScanMode scanMode)
        {
            return new ConditionalSubcommandBlocks(beginX, beginY, beginZ, endX, endY, endZ, destX, destY, destZ, scanMode);
        }

        

        public override TypePattern[] Pattern => new TypePattern[]
        {
            new TypePattern(
                new NamedType(typeof(TokenCoordinateLiteral), "begin x"),
                new NamedType(typeof(TokenCoordinateLiteral), "begin y"),
                new NamedType(typeof(TokenCoordinateLiteral), "begin z"),
                new NamedType(typeof(TokenCoordinateLiteral), "end x"),
                new NamedType(typeof(TokenCoordinateLiteral), "end y"),
                new NamedType(typeof(TokenCoordinateLiteral), "end z"),
                new NamedType(typeof(TokenCoordinateLiteral), "dest x"),
                new NamedType(typeof(TokenCoordinateLiteral), "dest y"),
                new NamedType(typeof(TokenCoordinateLiteral), "dest z"),
                new NamedType(typeof(TokenIdentifierEnum), "scan mode")
            )
        };
        public override string Keyword => "block";

        public override void FromTokens(Statement tokens)
        {
            this.beginX = tokens.Next<TokenCoordinateLiteral>();
            this.beginY = tokens.Next<TokenCoordinateLiteral>();
            this.beginZ = tokens.Next<TokenCoordinateLiteral>();

            this.endX = tokens.Next<TokenCoordinateLiteral>();
            this.endY = tokens.Next<TokenCoordinateLiteral>();
            this.endZ = tokens.Next<TokenCoordinateLiteral>();
            
            this.destX = tokens.Next<TokenCoordinateLiteral>();
            this.destY = tokens.Next<TokenCoordinateLiteral>();
            this.destZ = tokens.Next<TokenCoordinateLiteral>();

            ParsedEnumValue parsedEnum = tokens.Next<TokenIdentifierEnum>().value;
            parsedEnum.RequireType<BlocksScanMode>(tokens);
            this.scanMode = (BlocksScanMode)parsedEnum.value;
        }
        public override string ToMinecraft() =>
            $"blocks {beginX} {beginY} {beginZ} {endX} {endY} {endZ} {destX} {destY} {destZ} {scanMode}";
    }
    internal class ConditionalSubcommandEntity : ConditionalSubcommand
    {
        internal Selector entity;

        public ConditionalSubcommandEntity() { }
        private ConditionalSubcommandEntity(Selector entity)
        {
            this.entity = entity;
        }

        /// <summary>
        /// Create a ConditionalSubcommandEntity that contains the given entity.
        /// </summary>
        /// <param name="entity">The entity(ies) to check for existing.</param>
        /// <returns></returns>
        internal static ConditionalSubcommandEntity New(Selector entity)
        {
            return new ConditionalSubcommandEntity(entity);
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            new TypePattern(
                new NamedType(typeof(TokenSelectorLiteral), "target")
            )
        };
        public override string Keyword => "entity";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            entity = tokens.Next<TokenSelectorLiteral>();
        }
        public override string ToMinecraft() => $"entity {entity}";
    }
    internal class ConditionalSubcommandScore : ConditionalSubcommand
    {
        internal bool comparesRange;

        internal string sourceSelector;
        internal string sourceValue;

        // if comparesRange
        internal Range range;

        // if !comparesRange
        internal TokenCompare.Type comparisonType;
        internal string otherSelector;
        internal string otherValue;

        public ConditionalSubcommandScore() { }
        private ConditionalSubcommandScore(bool comparesRange, ScoreboardValue sourceValue, Range range, TokenCompare.Type comparisonType, ScoreboardValue otherValue)
        {
            this.comparesRange = comparesRange;

            this.sourceSelector = sourceValue.clarifier.CurrentString;
            this.sourceValue = sourceValue.Name;

            this.range = range;
            this.comparisonType = comparisonType;

            if (otherValue != null)
            {
                this.otherSelector = otherValue.clarifier.CurrentString;
                this.otherValue = otherValue.Name;
            } else
            {
                this.otherSelector = null;
                this.otherValue = null;
            }
        }
        private ConditionalSubcommandScore(bool comparesRange, string sourceSelector, string sourceValue, Range range, TokenCompare.Type comparisonType, string otherSelector, string otherValue)
        {
            this.comparesRange = comparesRange;

            this.sourceSelector = sourceSelector;
            this.sourceValue = sourceValue;

            this.range = range;
            this.comparisonType = comparisonType;

            this.otherSelector = otherSelector;
            this.otherValue = otherValue;
        }

        /// <summary>
        /// Create a ConditionalSubcommandScore that compares a scoreboard value with a range.
        /// </summary>
        /// <param name="source">The first value.</param>
        /// <param name="range">The range to compare against.</param>
        /// <returns></returns>
        internal static ConditionalSubcommandScore New(ScoreboardValue source, Range range)
        {
            return new ConditionalSubcommandScore(true, source, range, 0, null);
        }
        /// <summary>
        /// Create a ConditionalSubcommandScore that compares a scoreboard value with another scoreboard value.
        /// </summary>
        /// <param name="source">The first value.</param>
        /// <param name="type">The comparison type/operator used.</param>
        /// <param name="other">The other value.</param>
        /// <returns></returns>
        internal static ConditionalSubcommandScore New(ScoreboardValue source, TokenCompare.Type type, ScoreboardValue other)
        {
            return new ConditionalSubcommandScore(false, source, Range.zero, type, other);
        }
        /// <summary>
        /// Create a ConditionalSubcommandScore that compares a scoreboard value with a range.
        /// </summary>
        /// <param name="source">The first value.</param>
        /// <param name="range">The range to compare against.</param>
        /// <returns></returns>
        internal static ConditionalSubcommandScore New(string selector, string objective, Range range)
        {
            return new ConditionalSubcommandScore(true, selector, objective, range, 0, null, null);
        }
        /// <summary>
        /// Create a ConditionalSubcommandScore that compares a scoreboard value with another scoreboard value.
        /// </summary>
        /// <param name="source">The first value.</param>
        /// <param name="type">The comparison type/operator used.</param>
        /// <param name="other">The other value.</param>
        /// <returns></returns>
        internal static ConditionalSubcommandScore New(string sourceSelector, string sourceObjective, TokenCompare.Type type, string otherSelector, string otherObjective)
        {
            return new ConditionalSubcommandScore(false, sourceSelector, sourceObjective, Range.zero, type, otherSelector, otherObjective);
        }

        public override TypePattern[] Pattern => new TypePattern[]
        {
            new TypePattern(
                new NamedType(typeof(TokenIdentifierValue), "source"),
                new NamedType(typeof(TokenIdentifier), "matches"),
                new NamedType(typeof(TokenRangeLiteral), "range")
            ),
            new TypePattern(
                new NamedType(typeof(TokenIdentifierValue), "source"),
                new NamedType(typeof(TokenCompare), "comparison"),
                new NamedType(typeof(TokenIdentifierValue), "other")
            )
        };
        public override string Keyword => "score";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            this.sourceValue = tokens.Next<TokenIdentifierValue>().value;

            // thisScore == otherScore
            if(tokens.NextIs<TokenCompare>(false))
            {
                this.comparesRange = false;
                this.comparisonType = tokens.Next<TokenCompare>().GetCompareType();
                this.otherValue = tokens.Next<TokenIdentifierValue>().value;
                return;
            }

            // thisScore matches 1..10
            tokens.Next(); // skip "matches"

            this.comparesRange = true;
            this.range = tokens.Next<TokenRangeLiteral>().range;
        }
        public override string ToMinecraft()
        {
            if (this.comparesRange)
                return $"score {sourceSelector} {sourceValue} matches {range.ToString()}";

            string operatorString = TokenCompare.GetMinecraftOperator(comparisonType);
            return $"score {sourceSelector} {sourceValue} {operatorString} {otherSelector} {otherValue}";
        }
    }
}
