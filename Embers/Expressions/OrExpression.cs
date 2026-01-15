namespace Embers.Expressions;
/// <summary>
/// OrExpression represents a logical OR operation between two expressions.
/// </summary>
/// <seealso cref="BaseExpression" />
public class OrExpression(IExpression left, IExpression right) : BaseExpression
{
    private readonly IExpression left = left;
    private readonly IExpression right = right;

    public override object? Evaluate(Context context)
    {
        var lval = left.Evaluate(context);
        if (lval is bool lb && lb)
            return true;

        var rval = right.Evaluate(context);
        return rval is bool rb && rb;
    }

    public override bool Equals(object? obj) => obj is OrExpression other &&
               Equals(left, other.left) &&
               Equals(right, other.right);

    public override int GetHashCode() => HashCode.Combine(left, right);
}


