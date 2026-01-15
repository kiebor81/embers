namespace Embers.Expressions;
/// <summary>
/// BinaryExpression is a base class for expressions that operate on two operands.
/// It provides a structure for evaluating expressions that involve two expressions, such as arithmetic operations, comparisons, and logical operations.
/// </summary>
/// <seealso cref="BaseExpression" />
public abstract class BinaryExpression(IExpression left, IExpression right) : BaseExpression
{
    private readonly IExpression left = left;
    private readonly IExpression right = right;

    public IExpression LeftExpression { get { return left; } }

    public IExpression RightExpression { get { return right; } }

    public override object Evaluate(Context context)
    {
        var lvalue = left.Evaluate(context);
        var rvalue = right.Evaluate(context);

        return Apply(lvalue, rvalue);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj.GetType() == GetType())
        {
            var expr = (BinaryExpression)obj;

            return left.Equals(expr.left) && right.Equals(expr.right);
        }

        return false;
    }

    public override int GetHashCode() => LeftExpression.GetHashCode() + RightExpression.GetHashCode() + GetType().GetHashCode();

    public abstract object Apply(object leftvalue, object rightvalue);
}
