using Embers.Utilities;

namespace Embers.Expressions;
/// <summary>
/// CompareThreeWayExpression compares two values and returns -1, 0, 1, or nil when not comparable.
/// </summary>
public class CompareThreeWayExpression(IExpression left, IExpression right) : BinaryExpression(left, right)
{
    public override object? Apply(object leftvalue, object rightvalue)
    {
        if (!ComparisonUtilities.TryCompare(leftvalue, rightvalue, out var result))
            return null;

        return (long)result;
    }
}
