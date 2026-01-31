namespace Embers.Expressions;
/// <summary>
/// ConstantExpression represents a constant value or value which can be read as a constant.
/// Constant expressions are used to represent any literal value as determed by the parser.
/// </summary>
/// <seealso cref="BaseExpression" />
public class ConstantExpression(object value) : BaseExpression
{
    private readonly object value = value;

    public object Value { get { return value; } }

    public override object Evaluate(Context context) => value;

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is ConstantExpression expr)
        {
            if (value == null)
                return expr.value == null;

            return Value.Equals(expr.Value);
        }

        return false;
    }

    public override int GetHashCode()
    {
        if (value == null)
            return 0;

        return value.GetHashCode();
    }
}
