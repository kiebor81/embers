using System.Collections;
using Embers.Exceptions;
using Embers.Utilities;

namespace Embers.Expressions;
/// <summary>
/// Assigns a value to an indexed expression, such as an element in a list or a character in a string.
/// </summary>
/// <seealso cref="BaseExpression" />
public class AssignIndexedExpression(IExpression leftexpression, IExpression indexexpression, IExpression expression) : BaseExpression
{
    private static readonly int hashcode = typeof(IndexedExpression).GetHashCode();
    private readonly IExpression leftexpression = leftexpression;
    private readonly IExpression indexexpression = indexexpression;
    private readonly IExpression expression = expression;

    public override object? Evaluate(Context context)
    {
        var leftvalue = (IList)leftexpression.Evaluate(context);
        object indexvalue = indexexpression.Evaluate(context);
        var newvalue = expression.Evaluate(context);

        int index = indexvalue switch
        {
            int i => i,
            long l => (int)l,
            _ => throw new TypeError("index must be an integer")
        };

        if ((leftvalue is DynamicArray dynArray && dynArray.IsFrozen) || FrozenState.IsFrozen(leftvalue))
            throw new FrozenError("can't modify frozen Array");

        leftvalue[index] = newvalue;

        return newvalue;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is AssignIndexedExpression expr)
        {
            return leftexpression.Equals(expr.leftexpression) && expression.Equals(expr.expression) && indexexpression.Equals(expr.indexexpression);
        }

        return false;
    }

    public override int GetHashCode() => leftexpression.GetHashCode() + expression.GetHashCode() + indexexpression.GetHashCode() + hashcode;
}
