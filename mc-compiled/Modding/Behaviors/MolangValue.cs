using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding.Behaviors;

/// <summary>
///     A Molang expression or an implicit value.
/// </summary>
public class MolangValue
{
    private readonly string _valueExpr;
    private readonly float _valueNum;

    public bool isExpression;
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

    public static implicit operator string(MolangValue ml)
    {
        return ml._valueExpr;
    }
    public static implicit operator float(MolangValue ml)
    {
        return ml._valueNum;
    }
    public static explicit operator MolangValue(string expr)
    {
        return new MolangValue(expr);
    }
    public static explicit operator MolangValue(float num)
    {
        return new MolangValue(num);
    }
    public static explicit operator MolangValue(int num)
    {
        return new MolangValue(num);
    }
    public static explicit operator MolangValue(bool boolean)
    {
        return new MolangValue(boolean ? 1 : 0);
    }

    /// <summary>
    ///     Convert to the appropriate JSON token.
    /// </summary>
    /// <returns></returns>
    public JToken ToJSON()
    {
        return this.isExpression ? this._valueExpr : this._valueNum;
    }

    /// <summary>
    ///     Convert to a string representing this expression.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (this.isExpression)
            return this._valueExpr;

        return this._valueNum.ToString();
    }
}