namespace Embers.Expressions;
/// <summary>
/// AndExpression represents a logical AND operation between two expressions.
/// </summary>
/// <seealso cref="BaseExpression" />
public class AndExpression(IExpression left, IExpression right) : BaseExpression
{
    private readonly IExpression left = left;
    private readonly IExpression right = right;

    public override object? Evaluate(Context context)
    {
        var lval = left.Evaluate(context);
        if (lval is bool lb && !lb)
            return false;

        var rval = right.Evaluate(context);
        return rval is bool rb && rb;
    }

    public override bool Equals(object? obj) => obj is AndExpression other &&
               Equals(left, other.left) &&
               Equals(right, other.right);

    public override int GetHashCode() => HashCode.Combine(left, right);
}

