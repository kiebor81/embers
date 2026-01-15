using Embers.Signals;

namespace Embers.Expressions;
/// <summary>
/// Evaluates a return expression, which exits from the current method.
/// </summary>
/// <seealso cref="BaseExpression" />
public class ReturnExpression(IExpression? expr) : BaseExpression
{
    private static readonly int hashcode = typeof(ReturnExpression).GetHashCode();
    private readonly IExpression? expression = expr;

    public override object? Evaluate(Context context)
    {
        object? value = expression?.Evaluate(context);
        throw new ReturnSignal(value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ReturnExpression other)
            return false;

        if (expression == null && other.expression == null)
            return true;

        if (expression == null || other.expression == null)
            return false;

        return expression.Equals(other.expression);
    }

    public override int GetHashCode()
    {
        int result = hashcode;
        if (expression != null)
            result += expression.GetHashCode();
        return result;
    }
}
