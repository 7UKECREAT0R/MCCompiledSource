using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_compiled.Modding.Behaviors
{
    /// <summary>
    /// A Molang expression or an implicit value.
    /// </summary>
    public class MolangValue
    {
        public MolangValue(string expression)
        {
            isExpression = true;
            _valueExpr = expression;
        }
        public MolangValue(float value)
        {
            isExpression = false;
            _valueNum = value;
        }

        public bool isExpression;
        string _valueExpr;
        float _valueNum;

        public static implicit operator string(MolangValue ml) => ml._valueExpr;
        public static implicit operator float(MolangValue ml) => ml._valueNum;
        public static explicit operator MolangValue(string expr) => new MolangValue(expr);
        public static explicit operator MolangValue(float num) => new MolangValue(num);

        public JToken ToJSON() =>
            isExpression ? (JToken)_valueExpr : (JToken)_valueNum;
    }
}
