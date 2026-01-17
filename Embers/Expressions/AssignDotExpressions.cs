using Embers.Exceptions;
using Embers.Utilities;

namespace Embers.Expressions;
/// <summary>
/// AssignDotExpressions represents an assignment operation to a property or field of an object,
/// it allows you to set a value to a property or field accessed via a dot notation.
/// Together with the DotExpression, it provides a way to manipulate object properties dynamically,
/// and enables interoperability with .NET types.
/// </summary>
/// <seealso cref="BaseExpression" />
public class AssignDotExpressions(DotExpression leftvalue, IExpression expression) : BaseExpression
{
    private static readonly int hashtag = typeof(AssignDotExpressions).GetHashCode();

    private readonly DotExpression leftvalue = leftvalue;
    private readonly IExpression expression = expression;

    public DotExpression LeftValue { get { return leftvalue; } }

    public IExpression Expression { get { return expression; } }

    public override object? Evaluate(Context context)
    {
        object? target = leftvalue.TargetExpression.Evaluate(context);

        if (target is DynamicObject)
        {
            var obj = (DynamicObject)target;
            var method = obj.GetMethod(leftvalue.Name + "=");

            if (method != null)
                return method.Apply(obj, context, [expression.Evaluate(context)]);

            throw new NoMethodError(leftvalue.Name + "=");
        }

        var newvalue = expression.Evaluate(context);
        ObjectUtilities.SetValue(target, leftvalue.Name, newvalue);

        return newvalue;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is AssignDotExpressions)
        {
            var cmd = (AssignDotExpressions)obj;

            return leftvalue.Equals(cmd.leftvalue) && Expression.Equals(cmd.Expression);
        }
        
        return false;
    }

    public override int GetHashCode() => LeftValue.GetHashCode() + Expression.GetHashCode() + hashtag;
}
