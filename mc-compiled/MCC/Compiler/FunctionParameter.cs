using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC.Compiler
{
    /// <summary>
    /// A 'type' of function parameter.
    /// </summary>
    public enum FunctionParameterType
    {
        /// <summary>
        /// A traditional scoreboard argument.
        /// </summary>
        Scoreboard,
        /// <summary>
        /// 
        /// </summary>
        PPV
    }
    /// <summary>
    /// A single parameter in a function. Uses factory methods.
    /// </summary>
    public struct FunctionParameter
    {
        public readonly FunctionParameterType type;

        /// <summary>
        /// Returns if this parameter is scoreboard-based.
        /// </summary>
        public bool IsScoreboard
        {
            get => type == FunctionParameterType.Scoreboard;
        }
        /// <summary>
        /// Returns if this parameter is PPV-based.
        /// </summary>
        public bool IsPPV
        {
            get => type == FunctionParameterType.PPV;
        }
        /// <summary>
        /// Returns if this parameter has a default value to fall back to if unspecified at a call-site.
        /// </summary>
        public bool HasDefault
        {
            get => defaultValue != null;
        }

        /// <summary>
        /// The scoreboard value that this parameter sets, if
        /// <code>
        ///     this.type == FunctionParameterType.Scoreboard
        /// </code>
        /// </summary>
        public readonly ScoreboardValue scoreboard;
        /// <summary>
        /// The default value passed into this parameter, if any.
        /// </summary>
        public readonly Token defaultValue;
        /// <summary>
        /// The name of the PPV this parameter sets, if
        /// <code>
        ///     this.type == FunctionParameterType.PPV
        /// </code>
        /// </summary>
        public readonly string ppvName;

        private FunctionParameter(FunctionParameterType type, ScoreboardValue scoreboard, Token defaultValue, string ppvName)
        {
            this.type = type;
            this.scoreboard = scoreboard;
            this.defaultValue = defaultValue;
            this.ppvName = ppvName;
        }

        /// <summary>
        /// Create a scoreboard-based function parameter.
        /// </summary>
        /// <param name="scoreboard"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static FunctionParameter CreateScoreboard(ScoreboardValue scoreboard, Token defaultValue) =>
            new FunctionParameter(FunctionParameterType.Scoreboard, scoreboard, defaultValue, null);

        /// <summary>
        /// Create a ppv-based function parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static FunctionParameter CreatePPV(string name, Token defaultValue) =>
            new FunctionParameter(FunctionParameterType.PPV, null, defaultValue, name);

        public override string ToString()
        {
            switch (type)
            {
                case FunctionParameterType.Scoreboard:
                    return '[' + scoreboard.GetTypeKeyword() + ": " + scoreboard.AliasName + ']';
                case FunctionParameterType.PPV:
                    return '[' + ppvName + ']';
                default:
                    return null;
            }
        }
    }
}
