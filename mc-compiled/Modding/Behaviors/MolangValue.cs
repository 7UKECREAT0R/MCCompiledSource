using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors
{
    /// <summary>
    /// A Molang expression or an implicit value.
    /// </summary>
    public class MolangValue
    {
        public MolangValue(string expression)
        {
            this.isExpression = true;
            this._valueExpr = expression;
        }
        public MolangValue(float value)
        {
            this.isExpression = false;
            this._valueNum = value;
        }

        public bool isExpression;
        string _valueExpr;
        float _valueNum;

        public static implicit operator string(MolangValue ml) => ml._valueExpr;
        public static implicit operator float(MolangValue ml) => ml._valueNum;
        public static explicit operator MolangValue(string expr) => new MolangValue(expr);
        public static explicit operator MolangValue(float num) => new MolangValue(num);
        public static explicit operator MolangValue(int num) => new MolangValue(num);
        public static explicit operator MolangValue(bool boolean) => new MolangValue(boolean ? 1 : 0);

        /// <summary>
        /// Convert to the appropriate JSON token.
        /// </summary>
        /// <returns></returns>
        public JToken ToJSON() => this.isExpression ? this._valueExpr : this._valueNum;

        /// <summary>
        /// Convert to a string representing this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.isExpression)
                return this._valueExpr;

            return this._valueNum.ToString();
        }
    }
}
