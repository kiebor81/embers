using Embers.Language;
using System.Collections;

namespace Embers.Expressions;
/// <summary>
/// ArrayExpression represents an array of expressions that can be evaluated to produce a dynamic array.
/// </summary>
/// <seealso cref="BaseExpression" />
public class ArrayExpression(IList<IExpression> expressions) : BaseExpression
{
    private static readonly int hashcode = typeof(DotExpression).GetHashCode();

    private readonly IList<IExpression> expressions = expressions;

    public override object Evaluate(Context context)
    {
        IList result = new DynamicArray();

        foreach (var expr in expressions)
            result.Add(expr.Evaluate(context));

        return result;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is ArrayExpression)
        {
            var expr = (ArrayExpression)obj;

            if (expressions.Count != expr.expressions.Count)
                return false;

            for (int k = 0; k < expressions.Count; k++)
                if (!expressions[k].Equals(expr.expressions[k]))
                    return false;

            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        int result = hashcode;

        foreach (var expr in expressions)
        {
            result += expr.GetHashCode();
            result *= 3;
        }

        return result;
    }
}