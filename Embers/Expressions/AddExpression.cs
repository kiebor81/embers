namespace Embers.Expressions;
/// <summary>
/// AddExpression represents an addition operation between two expressions.
/// </summary>
/// <seealso cref="BinaryExpression" />
public class AddExpression(IExpression left, IExpression right) : BinaryExpression(left, right)
{
    public override object Apply(object leftvalue, object rightvalue)
    {
        if (leftvalue is string || rightvalue is string)
            return leftvalue.ToString() + rightvalue.ToString();

        return (leftvalue, rightvalue) switch
        {
            (long l, long r) => l + r,
            (long l, int r) => l + r,
            (long l, double r) => l + r,
            (int l, long r) => l + r,
            (int l, int r) => (object)(l + r),
            (int l, double r) => l + r,
            (double l, long r) => l + r,
            (double l, int r) => l + r,
            (double l, double r) => l + r,
            _ => (double)leftvalue + (double)rightvalue
        };
    }
}
