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
    /// <summary>
    /// Class with chain methods for building execute commands.
    /// </summary>
    public class ExecuteBuilder
    {
        private readonly List<Subcommand> subcommands;
        
        /// <summary>
        /// Adds the given subcommand to this ExecuteBuilder and returns this instance.
        /// </summary>
        /// <param name="subcommand">The subcommand to add.</param>
        /// <returns></returns>
        internal ExecuteBuilder WithSubcommand(Subcommand subcommand)
        {
            subcommands.Add(subcommand);
            return this;
        }
        /// <summary>
        /// Adds the given subcommands to this ExecuteBuilder and returns this instance.
        /// </summary>
        /// <param name="subcommands">The subcommands to add.</param>
        /// <returns></returns>
        internal ExecuteBuilder WithSubcommands(IEnumerable<Subcommand> subcommands)
        {
            this.subcommands.AddRange(subcommands);
            return this;
        }

        /// <summary>
        /// Create a new ExecuteBuilder. Please prefer <see cref="Command.Execute"/> to create a new instance.
        /// </summary>
        internal ExecuteBuilder()
        {
            this.subcommands = new List<Subcommand>();
        }
        /// <summary>
        /// Build the execute builder into a completed command.
        /// </summary>
        /// <returns></returns>
        public string Build()
        {
            var parts = subcommands.Select(subcommand => subcommand.ToMinecraft());
            return "execute " + string.Join(" ", parts);
        }

        /// <summary>
        /// Aligns the executing coordinates on the given axes.
        /// </summary>
        /// <param name="axes"></param>
        /// <returns></returns>
        public ExecuteBuilder Align(Axes axes)
        {
            subcommands.Add(new SubcommandAlign(axes));
            return this;
        }
        /// <summary>
        /// Aligns the executing coordinates on the given axes, parsed from a string.
        /// String format: xy, zy, xyz, x, etc...
        /// </summary>
        /// <param name="axes"></param>
        /// <returns></returns>
        public ExecuteBuilder Align(string axes)
        {
            Axes parsed = SubcommandAlign.ParseAxes(axes);
            subcommands.Add(new SubcommandAlign(parsed));
            return this;
        }

        /// <summary>
        /// Anchors the executing coordinates to the eyes/feet of the executing entity.
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public ExecuteBuilder Anchored(AnchorPosition anchor)
        {
            subcommands.Add(new SubcommandAnchored(anchor));
            return this;
        }

        /// <summary>
        /// Executes as a given entity/set of entities.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder As(Selector entity)
        {
            subcommands.Add(new SubcommandAs(entity));
            return this;
        }

        /// <summary>
        /// Executes at the location of the given entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder At(Selector entity)
        {
            subcommands.Add(new SubcommandAt(entity));
            return this;
        }
        /// <summary>
        /// Executes at the location of @s. Shorthand for <b>.At(Selector.SELF)</b>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder AtSelf()
        {
            subcommands.Add(new SubcommandAt(Selector.SELF));
            return this;
        }


        /// <summary>
        /// Executes facing a certain entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public ExecuteBuilder FacingEntity(Selector entity, AnchorPosition anchor = AnchorPosition.feet)
        {
            subcommands.Add(new SubcommandFacing(true, entity, anchor, Coord.zero, Coord.zero, Coord.zero));
            return this;
        }
        /// <summary>
        /// Executes facing a certain location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public ExecuteBuilder Facing(Coord x, Coord y, Coord z)
        {
            subcommands.Add(new SubcommandFacing(false, null, AnchorPosition.feet, x, y, z));
            return this;
        }

        /// <summary>
        /// Executes in a certain dimension.
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public ExecuteBuilder In(Dimension dimension)
        {
            subcommands.Add(new SubcommandIn(dimension));
            return this;
        }

        /// <summary>
        /// Executes positioned at a certain entity. Same as <see cref="At(Selector)"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder PositionedAs(Selector entity)
        {
            subcommands.Add(new SubcommandPositioned(true, entity, Coord.zero, Coord.zero, Coord.zero));
            return this;
        }
        /// <summary>
        /// Executes positioned at a certain location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public ExecuteBuilder Positioned(Coord x, Coord y, Coord z)
        {
            subcommands.Add(new SubcommandPositioned(false, null, x, y, z));
            return this;
        }

        /// <summary>
        /// Executes with the same rotation as the given entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder RotatedAs(Selector entity)
        {
            subcommands.Add(new SubcommandRotated(true, entity, Coord.zero, Coord.zero));
            return this;
        }
        /// <summary>
        /// Executes with the given rotation.
        /// </summary>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <returns></returns>
        public ExecuteBuilder Rotated(Coord yaw, Coord pitch)
        {
            subcommands.Add(new SubcommandRotated(false, null, yaw, pitch));
            return this;
        }

        /// <summary>
        /// Run a command with all the previous subcommands applied. Calls <see cref="Build"/> and returns it, as 'run' is terminating.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public string Run(string command)
        {
            subcommands.Add(new SubcommandRun(command));
            return Build();
        }

        /// <summary>
        /// Executes only if a block is located at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="block"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ExecuteBuilder IfBlock(Coord x, Coord y, Coord z, string block, int? data = null)
        {
            subcommands.Add(new SubcommandIf(ConditionalSubcommandBlock.New(x, y, z, block, data)));
            return this;
        }
        /// <summary>
        /// Executes only if a section of blocks matches another section.
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
        public ExecuteBuilder IfBlocks(
            Coord beginX, Coord beginY, Coord beginZ,
            Coord endX, Coord endY, Coord endZ,
            Coord destX, Coord destY, Coord destZ,
            BlocksScanMode scanMode = BlocksScanMode.all)
        {
            subcommands.Add(new SubcommandIf(ConditionalSubcommandBlocks.New
                (beginX, beginY, beginZ, endX, endY, endZ, destX, destY, destZ, scanMode)));
            return this;
        }
        /// <summary>
        /// Executes only if a selector finds a match.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder IfEntity(Selector entity)
        {
            subcommands.Add(new SubcommandIf(ConditionalSubcommandEntity.New(entity)));
            return this;
        }
        /// <summary>
        /// Executes only if a score falls under a certain range.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public ExecuteBuilder IfScore(ScoreboardValue source, Range range)
        {
            subcommands.Add(new SubcommandIf(ConditionalSubcommandScore.New(source, range)));
            return this;
        }
        /// <summary>
        /// Executes only if the given comparison passes.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public ExecuteBuilder IfScore(ScoreboardValue source, TokenCompare.Type type, ScoreboardValue other)
        {
            if (type == TokenCompare.Type.NOT_EQUAL)
            {
                // for some reason, inequalities are not supported by MC.
                // this is a workaround for that issue.
                UnlessScore(source, TokenCompare.Type.EQUAL, other);
                return this;
            }

            subcommands.Add(new SubcommandIf(ConditionalSubcommandScore.New(source, type, other)));
            return this;
        }


        /// <summary>
        /// Executes unless a block is located at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="block"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ExecuteBuilder UnlessBlock(Coord x, Coord y, Coord z, string block, int? data = null)
        {
            subcommands.Add(new SubcommandUnless(ConditionalSubcommandBlock.New(x, y, z, block, data)));
            return this;
        }
        /// <summary>
        /// Executes unless a section of blocks matches another section.
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
        public ExecuteBuilder UnlessBlocks(
            Coord beginX, Coord beginY, Coord beginZ,
            Coord endX, Coord endY, Coord endZ,
            Coord destX, Coord destY, Coord destZ,
            BlocksScanMode scanMode = BlocksScanMode.all)
        {
            subcommands.Add(new SubcommandUnless(ConditionalSubcommandBlocks.New
                (beginX, beginY, beginZ, endX, endY, endZ, destX, destY, destZ, scanMode)));
            return this;
        }
        /// <summary>
        /// Executes unless a selector finds a match.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder UnlessEntity(Selector entity)
        {
            subcommands.Add(new SubcommandUnless(ConditionalSubcommandEntity.New(entity)));
            return this;
        }
        /// <summary>
        /// Executes unless a score falls under a certain range.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public ExecuteBuilder UnlessScore(ScoreboardValue source, Range range)
        {
            subcommands.Add(new SubcommandUnless(ConditionalSubcommandScore.New(source, range)));
            return this;
        }
        /// <summary>
        /// Executes unless the given comparison passes.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public ExecuteBuilder UnlessScore(ScoreboardValue source, TokenCompare.Type type, ScoreboardValue other)
        {
            if(type == TokenCompare.Type.NOT_EQUAL)
            {
                // for some reason, inequalities are not supported by MC.
                // this is a workaround for that issue.
                IfScore(source, TokenCompare.Type.EQUAL, other);
                return this;
            }

            subcommands.Add(new SubcommandUnless(ConditionalSubcommandScore.New(source, type, other)));
            return this;
        }
    }
}
