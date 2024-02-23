﻿using mc_compiled.Commands.Selectors;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;

namespace mc_compiled.Commands.Execute
{
    internal class ConditionalSubcommandBlock : ConditionalSubcommand
    {
        internal Coordinate x, y, z;
        internal string block;
        internal int? data = null;

        public ConditionalSubcommandBlock() { }
        private ConditionalSubcommandBlock(Coordinate x, Coordinate y, Coordinate z, string block, int? data)
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
        internal static ConditionalSubcommandBlock New(Coordinate x, Coordinate y, Coordinate z, string block, int? data = null)
        {
            return new ConditionalSubcommandBlock(x, y, z, block, data);
        }

        public override TypePattern[] Patterns => new TypePattern[]
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
            this.x = tokens.Next<TokenCoordinateLiteral>();
            this.y = tokens.Next<TokenCoordinateLiteral>();
            this.z = tokens.Next<TokenCoordinateLiteral>();
            this.block = tokens.Next<TokenStringLiteral>();

            if(tokens.NextIs<TokenIntegerLiteral>()) this.data = tokens.Next<TokenIntegerLiteral>();
        }
        public override string ToMinecraft()
        {
            if(this.data.HasValue)
                return $"block {this.x} {this.y} {this.z} {this.block} {this.data.Value}";

            return $"block {this.x} {this.y} {this.z} {this.block}";
        }
    }
    internal class ConditionalSubcommandBlocks : ConditionalSubcommand
    {
        internal Coordinate beginX, beginY, beginZ;
        internal Coordinate endX, endY, endZ;
        internal Coordinate destX, destY, destZ;
        internal BlocksScanMode scanMode;

        public ConditionalSubcommandBlocks() { }
        private ConditionalSubcommandBlocks(
            Coordinate beginX, Coordinate beginY, Coordinate beginZ,
            Coordinate endX, Coordinate endY, Coordinate endZ,
            Coordinate destX, Coordinate destY, Coordinate destZ,
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
            Coordinate beginX, Coordinate beginY, Coordinate beginZ,
            Coordinate endX, Coordinate endY, Coordinate endZ,
            Coordinate destX, Coordinate destY, Coordinate destZ,
            BlocksScanMode scanMode)
        {
            return new ConditionalSubcommandBlocks(beginX, beginY, beginZ, endX, endY, endZ, destX, destY, destZ, scanMode);
        }

        

        public override TypePattern[] Patterns => new TypePattern[]
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
            $"blocks {this.beginX} {this.beginY} {this.beginZ} {this.endX} {this.endY} {this.endZ} {this.destX} {this.destY} {this.destZ} {this.scanMode}";
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

        public override TypePattern[] Patterns => new TypePattern[]
        {
            new TypePattern(
                new NamedType(typeof(TokenSelectorLiteral), "target")
            )
        };
        public override string Keyword => "entity";
        public override bool TerminatesChain => false;

        public override void FromTokens(Statement tokens)
        {
            this.entity = tokens.Next<TokenSelectorLiteral>();
        }
        public override string ToMinecraft() => $"entity {this.entity}";
    }
    internal class ConditionalSubcommandScore : ConditionalSubcommand
    {
        internal bool comparesRange;

        internal bool SourceIsGlobal => this.sourceSelector.Equals(Executor.FAKEPLAYER_NAME);
        internal Clarifier SourceClarifier => new Clarifier(this.SourceIsGlobal, this.sourceSelector);
        
        // The reason this isn't a ScoreboardValue is because sometimes the user
        // uses a different selector and it's not worth cloning it.
        internal string sourceSelector;
        internal string sourceValue;

        // if comparesRange
        internal Range range;

        // if !comparesRange
        internal TokenCompare.Type comparisonType;
        
        internal bool OtherIsGlobal => this.otherSelector.Equals(Executor.FAKEPLAYER_NAME);
        internal Clarifier OtherClarifier => new Clarifier(this.OtherIsGlobal, this.otherSelector);
        internal string otherSelector;
        internal string otherValue;

        public ConditionalSubcommandScore() { }
        private ConditionalSubcommandScore(bool comparesRange, ScoreboardValue sourceValue, Range range, TokenCompare.Type comparisonType, ScoreboardValue otherValue)
        {
            this.comparesRange = comparesRange;

            this.sourceSelector = sourceValue.clarifier.CurrentString;
            this.sourceValue = sourceValue.InternalName;

            this.range = range;
            this.comparisonType = comparisonType;

            if (otherValue != null)
            {
                this.otherSelector = otherValue.clarifier.CurrentString;
                this.otherValue = otherValue.InternalName;
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

        public override TypePattern[] Patterns => new TypePattern[]
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
            this.sourceValue = tokens.Next<TokenIdentifierValue>().value.InternalName;

            // thisScore == otherScore
            if(tokens.NextIs<TokenCompare>(false))
            {
                this.comparesRange = false;
                this.comparisonType = tokens.Next<TokenCompare>().GetCompareType();
                this.otherValue = tokens.Next<TokenIdentifierValue>().value.InternalName;
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
                return $"score {this.sourceSelector} {this.sourceValue} matches {this.range.ToString()}";

            string operatorString = TokenCompare.GetMinecraftOperator(this.comparisonType);
            return $"score {this.sourceSelector} {this.sourceValue} {operatorString} {this.otherSelector} {this.otherValue}";
        }
    }
}
