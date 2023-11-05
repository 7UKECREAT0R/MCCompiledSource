using mc_compiled.Commands;
using mc_compiled.Commands.Execute;
using mc_compiled.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler.TypeSystem.Implementations
{
    internal sealed class TypedefTime : TypedefInteger
    {
        private const string SB_HOURS = "_mcc_t_hrs";
        private const string SB_MINUTES = "_mcc_t_min";
        private const string SB_SECONDS = "_mcc_t_sec";
        private const string SB_TEMP = "_mcc_t_temp";
        private const string SB_CONST = "_mcc_t_const";

        public override ScoreboardManager.ValueType TypeEnum => ScoreboardManager.ValueType.TIME;
        public override string TypeKeyword => "time";

        internal override Tuple<string[], JSONRawTerm[]> ToRawText(ScoreboardValue value, ref int index)
        {
            ScoreboardManager manager = value.manager;

            string timeFormatString = manager.executor.ppv["_timeformat"][0] as string;
            TimeFormat format = TimeFormat.Parse(timeFormatString);

            string _temporary = SB_TEMP + index;
            string _constant = SB_CONST + index;

            ScoreboardValue
                scoreboardHours = null,
                scoreboardMinutes = null,
                scoreboardSeconds = null;

            if (format.HasOption(TimeOption.h))
            {
                // TODO update this with new scoreboard stuff
                string _hours = SB_HOURS + index;
                scoreboardHours = new ScoreboardValueInteger(_hours, false, manager);
                scoreboardHours.clarifier.CopyFrom(value.clarifier);
            }
            if (format.HasOption(TimeOption.m))
            {
                string _minutes = SB_MINUTES + index;
                scoreboardMinutes = new ScoreboardValueInteger(_minutes, false, manager);
                scoreboardMinutes.clarifier.CopyFrom(value.clarifier);
            }
            if (format.HasOption(TimeOption.s))
            {
                string _seconds = SB_SECONDS + index;
                scoreboardSeconds = new ScoreboardValueInteger(_seconds, false, manager);
                scoreboardSeconds.clarifier.CopyFrom(value.clarifier);
            }

            ScoreboardValue temporary = new ScoreboardValueInteger(_temporary, true, manager);
            ScoreboardValue constant = new ScoreboardValueInteger(_constant, true, manager);

            manager.DefineMany(scoreboardHours, scoreboardMinutes, scoreboardSeconds, temporary, constant);

            var commands = new List<string>()
            {
                Command.ScoreboardSet(constant, 20),
                Command.ScoreboardOpSet(temporary, value),
                Command.ScoreboardOpDiv(temporary, constant),
                Command.ScoreboardSet(constant, 60)
            };

            if (scoreboardSeconds != null)
            {
                commands.Add(Command.ScoreboardOpSet(scoreboardSeconds, temporary));
            }

            if (scoreboardMinutes != null)
            {
                commands.Add(Command.ScoreboardOpDiv(temporary, constant));
                commands.Add(Command.ScoreboardOpSet(scoreboardMinutes, temporary));

                if (scoreboardSeconds != null)
                {
                    commands.Add(Command.ScoreboardOpMul(temporary, constant));
                    commands.Add(Command.ScoreboardOpSub(scoreboardSeconds, temporary));
                    commands.Add(Command.ScoreboardOpDiv(temporary, constant));
                }
            }

            if (scoreboardHours != null)
            {
                commands.Add(Command.ScoreboardOpDiv(temporary, constant));

                if (scoreboardMinutes == null)
                    commands.Add(Command.ScoreboardOpDiv(temporary, constant));

                commands.Add(Command.ScoreboardOpSet(scoreboardHours, temporary));

                if (scoreboardMinutes != null)
                {
                    commands.Add(Command.ScoreboardOpMul(temporary, constant));
                    commands.Add(Command.ScoreboardOpSub(scoreboardMinutes, temporary));
                }
            }

            string hours = SB_HOURS + index;
            string minutes = SB_MINUTES + index;
            string seconds = SB_SECONDS + index;

            var terms = new List<JSONRawTerm>();

            bool hasHours = format.HasOption(TimeOption.h),
               hasMinutes = format.HasOption(TimeOption.m),
               hasSeconds = format.HasOption(TimeOption.s);

            var buffer = new List<ConditionalTerm>();
            var textBuffer = new StringBuilder();

            if (hasHours)
            {
                if (format.minimumHours > 1)
                {
                    int bound = IntPow(10, format.minimumHours - 1);
                    int? previousBound = null;

                    for (int digits = 0; digits < format.minimumHours; digits++)
                    {
                        buffer.Add(new ConditionalTerm(new JSONRawTerm[] { new JSONText(textBuffer.ToString()) }, ConditionalSubcommandScore.New(value.clarifier.CurrentString, hours, new Range(bound, previousBound)), false));

                        textBuffer.Append('0');
                        previousBound = bound - 1;
                        bound /= 10; // remove 1 digit

                        if (bound == 1)
                            bound = 0;
                    }

                    terms.Add(new JSONVariant(buffer));
                }

                terms.Add(new JSONScore(value.clarifier.CurrentString, hours));
            }

            if (hasMinutes)
            {
                if (format.minimumMinutes > 1)
                {
                    buffer.Clear();
                    textBuffer.Clear();
                    if (hasHours)
                        textBuffer.Append(':');

                    int bound = IntPow(10, format.minimumMinutes - 1);
                    int? previousBound = null;

                    for (int digits = 0; digits < format.minimumMinutes; digits++)
                    {
                        buffer.Add(new ConditionalTerm(new JSONRawTerm[] { new JSONText(textBuffer.ToString()) }, ConditionalSubcommandScore.New(value.clarifier.CurrentString, minutes, new Range(bound, previousBound)), false));

                        textBuffer.Append('0');
                        previousBound = bound - 1;
                        bound /= 10; // remove 1 digit

                        if (bound == 1)
                            bound = 0;
                    }

                    terms.Add(new JSONVariant(buffer));
                }
                else if (hasHours)
                {
                    terms.Add(new JSONText(":"));
                }

                terms.Add(new JSONScore(value.clarifier.CurrentString, minutes));
            }

            // ReSharper disable once InvertIf
            if (hasSeconds)
            {
                if (format.minimumSeconds > 1)
                {
                    buffer.Clear();
                    textBuffer.Clear();
                    if (hasMinutes)
                        textBuffer.Append(':');

                    int bound = IntPow(10, format.minimumSeconds - 1);
                    int? previousBound = null;

                    for (int digits = 0; digits < format.minimumSeconds; digits++)
                    {
                        buffer.Add(new ConditionalTerm(new JSONRawTerm[] { new JSONText(textBuffer.ToString()) }, ConditionalSubcommandScore.New(value.clarifier.CurrentString, seconds, new Range(bound, previousBound)), false));

                        textBuffer.Append('0');
                        previousBound = bound - 1;
                        bound /= 10; // remove 1 digit

                        if (bound == 1)
                            bound = 0;
                    }

                    terms.Add(new JSONVariant(buffer));
                }
                else if (hasMinutes)
                {
                    terms.Add(new JSONText(":"));
                }

                terms.Add(new JSONScore(value.clarifier.CurrentString, seconds));
            }

            return new Tuple<string[], JSONRawTerm[]>(
                commands.ToArray(),
                terms.ToArray()
            );

            int IntPow(int integer, int exponent)
            {
                if (integer == 0)
                    return 0;
                if (exponent < 1)
                    return integer;

                int accumulator = integer;

                for (int i = 1; i < exponent; i++)
                    accumulator *= integer;

                return accumulator;
            }
        }
    }
}
