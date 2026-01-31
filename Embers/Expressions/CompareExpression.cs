using Microsoft.VisualBasic.CompilerServices;

namespace Embers.Expressions;
/// <summary>
/// CompareExpression is an expression that compares two values using a specified comparison operator.
/// </summary>
/// <seealso cref="BinaryExpression" />
public class CompareExpression(IExpression left, IExpression right, CompareOperator @operator) : BinaryExpression(left, right)
{
    private static readonly IDictionary<CompareOperator, Func<object, object, object>> functions = new Dictionary<CompareOperator, Func<object, object, object>>();
    private readonly CompareOperator @operator = @operator;

    static CompareExpression()
    {
        functions[CompareOperator.Equal] = (left, right) => Operators.CompareObjectEqual(left, right, false);
        functions[CompareOperator.NotEqual] = (left, right) => Operators.CompareObjectNotEqual(left, right, false);
        functions[CompareOperator.Less] = (left, right) => Operators.CompareObjectLess(left, right, false);
        functions[CompareOperator.Greater] = (left, right) => Operators.CompareObjectGreater(left, right, false);
        functions[CompareOperator.LessOrEqual] = (left, right) => Operators.CompareObjectLessEqual(left, right, false);
        functions[CompareOperator.GreaterOrEqual] = (left, right) => Operators.CompareObjectGreaterEqual(left, right, false);
    }

    public override object Apply(object leftvalue, object rightvalue) => functions[@operator](leftvalue, rightvalue);

    public override bool Equals(object? obj)
    {
        if (!base.Equals(obj))
            return false;

        return @operator == ((CompareExpression)obj).@operator;
    }

    public override int GetHashCode() => base.GetHashCode() + (int)@operator;
}
