using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.MCC
{
    public class Operator
    {
        public OperatorType type;
        private Operator(OperatorType type)
        {
            this.type = type;
        }

        /// <summary>
        /// Parse an operator from a string of text.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>The operator from the specified string. Null if no operator could be found in the string.</returns>
        public static Operator Parse(string str)
        {
            OperatorType type = OperatorType._UNKNOWN;

            str = str.Trim();
            if (str.Length > 2)
                str = str.Substring(0, 2);

            switch (str)
            {
                case "=":
                case "==":
                    type = OperatorType.EQUAL;
                    break;
                case "~=":
                case "!=":
                    type = OperatorType.NOT_EQUAL;
                    break;
                case ">":
                    type = OperatorType.GREATER_THAN;
                    break;
                case ">=":
                    type = OperatorType.GREATER_OR_EQUAL;
                    break;
                case "<":
                    type = OperatorType.LESS_THAN;
                    break;
                case "<=":
                    type = OperatorType.LESS_OR_EQUAL;
                    break;
                default:
                    break;
            }

            if (type == OperatorType._UNKNOWN)
                return null;
            return new Operator(type);
        }

        public override string ToString()
        {
            switch (type)
            {
                case OperatorType._UNKNOWN:
                    return "??";
                case OperatorType.EQUAL:
                    return "=";
                case OperatorType.NOT_EQUAL:
                    return "!=";
                case OperatorType.LESS_THAN:
                    return "<";
                case OperatorType.LESS_OR_EQUAL:
                    return "<=";
                case OperatorType.GREATER_THAN:
                    return ">";
                case OperatorType.GREATER_OR_EQUAL:
                    return ">=";
                default:
                    return "??";
            }
        }
    }
    public enum OperatorType
    {
        _UNKNOWN,

        EQUAL,
        NOT_EQUAL,
        LESS_THAN,
        LESS_OR_EQUAL,
        GREATER_THAN,
        GREATER_OR_EQUAL
    }
}