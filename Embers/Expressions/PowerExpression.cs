using Embers.Exceptions;

namespace Embers.Expressions;
/// <summary>
/// PowerExpression represents the power operation (exponentiation) between two expressions.
/// </summary>
/// <seealso cref="BinaryExpression" />
public class PowerExpression(IExpression left, IExpression right)
    : BinaryExpression(left, right)
{
    public override object Apply(object leftvalue, object rightvalue)
    {
        if (leftvalue is long ll && rightvalue is long rl)
            return (long)Math.Pow(ll, rl);

        if (leftvalue is int li && rightvalue is int ri)
            return (long)Math.Pow(li, ri);

        if (leftvalue is double ld && rightvalue is double rd)
            return Math.Pow(ld, rd);

        // Handle mixed int/long
        if ((leftvalue is int or long) && (rightvalue is int or long))
        {
            long l = Convert.ToInt64(leftvalue);
            long r = Convert.ToInt64(rightvalue);
            return (long)Math.Pow(l, r);
        }

        throw new TypeError("Power operator requires numeric operands");
    }
}
