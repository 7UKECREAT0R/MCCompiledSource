using mc_compiled.Commands.Selectors;
using mc_compiled.MCC;
using mc_compiled.MCC.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.Commands.Execute
{
    /// <summary>
    /// Class with chain methods for building execute commands.
    /// </summary>
    public class ExecuteBuilder
    {
        private readonly List<Subcommand> subcommands;
        
        /// <summary>
        /// Create a deep copy of this ExecuteBuilder.
        /// </summary>
        /// <returns></returns>
        public ExecuteBuilder Clone()
        {
            return new ExecuteBuilder().WithSubcommands(this.subcommands);
        }

        /// <summary>
        /// Adds the given subcommand to this ExecuteBuilder and returns this instance.
        /// </summary>
        /// <param name="subcommand">The subcommand to add.</param>
        /// <returns></returns>
        internal ExecuteBuilder WithSubcommand(Subcommand subcommand)
        {
            this.subcommands.Add(subcommand);
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
        /// <param name="terminated">If the chain was terminated properly.</param>
        public string Build(out bool terminated)
        {
            var parts = this.subcommands.Select(subcommand => subcommand.ToMinecraft());
            terminated = this.subcommands.Last().TerminatesChain;
            return "execute " + string.Join(" ", parts);
        }
        /// <summary>
        /// Build just the execute builder's subcommands.
        /// </summary>
        /// <returns></returns>
        /// <param name="terminated">If the chain was terminated properly.</param>
        public string BuildClean(out bool terminated)
        {
            var parts = this.subcommands.Select(subcommand => subcommand.ToMinecraft());
            terminated = this.subcommands.Last().TerminatesChain;
            return "execute " + string.Join(" ", parts);
        }

        /// <summary>
        /// Aligns the executing coordinates on the given axes.
        /// </summary>
        /// <param name="axes"></param>
        /// <returns></returns>
        public ExecuteBuilder Align(Axes axes)
        {
            this.subcommands.Add(new SubcommandAlign(axes));
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
            this.subcommands.Add(new SubcommandAlign(parsed));
            return this;
        }

        /// <summary>
        /// Anchors the executing coordinates to the eyes/feet of the executing entity.
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public ExecuteBuilder Anchored(AnchorPosition anchor)
        {
            this.subcommands.Add(new SubcommandAnchored(anchor));
            return this;
        }

        /// <summary>
        /// Executes as a given entity/set of entities.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder As(Selector entity)
        {
            this.subcommands.Add(new SubcommandAs(entity));
            return this;
        }

        /// <summary>
        /// Executes at the location of the given entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder At(Selector entity)
        {
            this.subcommands.Add(new SubcommandAt(entity));
            return this;
        }
        /// <summary>
        /// Executes at the location of @s. Shorthand for <b>.At(Selector.SELF)</b>
        /// </summary>
        /// <returns></returns>
        public ExecuteBuilder AtSelf()
        {
            this.subcommands.Add(new SubcommandAt(Selector.SELF));
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
            this.subcommands.Add(new SubcommandFacing(true, entity, anchor, Coordinate.zero, Coordinate.zero, Coordinate.zero));
            return this;
        }
        /// <summary>
        /// Executes facing a certain location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public ExecuteBuilder Facing(Coordinate x, Coordinate y, Coordinate z)
        {
            this.subcommands.Add(new SubcommandFacing(false, null, AnchorPosition.feet, x, y, z));
            return this;
        }

        /// <summary>
        /// Executes in a certain dimension.
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public ExecuteBuilder In(Dimension dimension)
        {
            this.subcommands.Add(new SubcommandIn(dimension));
            return this;
        }

        /// <summary>
        /// Executes positioned at a certain entity. Same as <see cref="At(Selector)"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder PositionedAs(Selector entity)
        {
            this.subcommands.Add(new SubcommandPositioned(true, entity, Coordinate.zero, Coordinate.zero, Coordinate.zero));
            return this;
        }
        /// <summary>
        /// Executes positioned at a certain location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public ExecuteBuilder Positioned(Coordinate x, Coordinate y, Coordinate z)
        {
            this.subcommands.Add(new SubcommandPositioned(false, null, x, y, z));
            return this;
        }

        /// <summary>
        /// Executes with the same rotation as the given entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ExecuteBuilder RotatedAs(Selector entity)
        {
            this.subcommands.Add(new SubcommandRotated(true, entity, Coordinate.zero, Coordinate.zero));
            return this;
        }
        /// <summary>
        /// Executes with the given rotation.
        /// </summary>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <returns></returns>
        public ExecuteBuilder Rotated(Coordinate yaw, Coordinate pitch)
        {
            this.subcommands.Add(new SubcommandRotated(false, null, yaw, pitch));
            return this;
        }

        /// <summary>
        /// Run a command with all the previous subcommands applied. Calls <see cref="Build"/> and returns it, as 'run' is terminating.
        /// </summary>
        /// <param name="command">The command to run with this execute context.</param>
        /// <returns></returns>
        public string Run(string command)
        {
            this.subcommands.Add(new SubcommandRun(command));
            return Build(out _);
        }
        /// <summary>
        /// Place a 'run' clause with no command and a whitespace at the end.
        /// </summary>
        /// <returns></returns>
        public string Run()
        {
            this.subcommands.Add(new SubcommandRun());
            return Build(out _);
        }

        /// <summary>
        /// Same as <see cref="Run(string)"/>, but applies to every command in the array individually.
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        public string[] RunOver(string[] commands)
        {
            string[] outputs = new string[commands.Length];

            int workingIndex = this.subcommands.Count;
            bool hasCapacity = false;

            for(int i = 0; i < commands.Length; i++)
            {
                string command = commands[i];

                if (!hasCapacity)
                {
                    this.subcommands.Add(new SubcommandRun(command));
                    hasCapacity = true;
                }
                else
                    this.subcommands[workingIndex] = new SubcommandRun(command);

                outputs[i] = Build(out _);
            }

            return outputs;
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
        public ExecuteBuilder IfBlock(Coordinate x, Coordinate y, Coordinate z, string block, int? data = null)
        {
            this.subcommands.Add(new SubcommandIf(ConditionalSubcommandBlock.New(x, y, z, block, data)));
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
            Coordinate beginX, Coordinate beginY, Coordinate beginZ,
            Coordinate endX, Coordinate endY, Coordinate endZ,
            Coordinate destX, Coordinate destY, Coordinate destZ,
            BlocksScanMode scanMode = BlocksScanMode.all)
        {
            this.subcommands.Add(new SubcommandIf(ConditionalSubcommandBlocks.New
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
            this.subcommands.Add(new SubcommandIf(ConditionalSubcommandEntity.New(entity)));
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
            this.subcommands.Add(new SubcommandIf(ConditionalSubcommandScore.New(source, range)));
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

            this.subcommands.Add(new SubcommandIf(ConditionalSubcommandScore.New(source, type, other)));
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
        public ExecuteBuilder UnlessBlock(Coordinate x, Coordinate y, Coordinate z, string block, int? data = null)
        {
            this.subcommands.Add(new SubcommandUnless(ConditionalSubcommandBlock.New(x, y, z, block, data)));
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
            Coordinate beginX, Coordinate beginY, Coordinate beginZ,
            Coordinate endX, Coordinate endY, Coordinate endZ,
            Coordinate destX, Coordinate destY, Coordinate destZ,
            BlocksScanMode scanMode = BlocksScanMode.all)
        {
            this.subcommands.Add(new SubcommandUnless(ConditionalSubcommandBlocks.New
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
            this.subcommands.Add(new SubcommandUnless(ConditionalSubcommandEntity.New(entity)));
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
            this.subcommands.Add(new SubcommandUnless(ConditionalSubcommandScore.New(source, range)));
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

            this.subcommands.Add(new SubcommandUnless(ConditionalSubcommandScore.New(source, type, other)));
            return this;
        }
    }
}
