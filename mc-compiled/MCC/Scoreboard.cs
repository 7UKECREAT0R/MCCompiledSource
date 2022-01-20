using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    /// <summary>
    /// A scoreboard value that can be written to.
    /// </summary>
    public abstract class ScoreboardValue
    {
        public const int MAX_NAME_LENGTH = 16;
        public readonly string baseName;

        public abstract int GetMaxNameLength();
        public abstract string[] GetInternalValues();
    }
    public sealed class ScoreboardValueInteger : ScoreboardValue
    {
        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH;
        public override string[] GetInternalValues() =>
            new[] { baseName };
    }
    public sealed class ScoreboardValueDecimal : ScoreboardValue
    {
        public const string WHOLE_SUFFIX = ":w";
        public const string DECIMAL_SUFFIX = ":d";

        public string WholeName { get => baseName + WHOLE_SUFFIX; }
        public string DecimalName { get => baseName + DECIMAL_SUFFIX; }

        public override int GetMaxNameLength() =>
           MAX_NAME_LENGTH - 2;
        public override string[] GetInternalValues() =>
            new[] { baseName, WholeName, DecimalName };
    }
    public sealed class ScoreboardValueBoolean : ScoreboardValue
    {
        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH;
        public override string[] GetInternalValues() =>
            new[] { baseName };
    }
    public sealed class ScoreboardValueStruct : ScoreboardValue
    {
        public override int GetMaxNameLength() =>
            MAX_NAME_LENGTH - 2;
        public override string[] GetInternalValues() =>
            new[] { baseName };
    }

    /// <summary>
    /// Manages the virtual scoreboard.
    /// </summary>
    public class ScoreboardManager
    {
        Dictionary<string, ScoreboardValue> values;

        public ScoreboardManager()
        {
            values = new Dictionary<string, ScoreboardValue>();
        }

        public ScoreboardValue this[string name]
        {
            get
            {
                name = name.ToUpper();
                return values[name];
            }
        }
        public ScoreboardValue this[ScoreboardValue sb]
        {
            set
            {
                values[sb.baseName.ToUpper()] = sb;
            }
        }


    }
}