using Range = Embers.Language.Primitive.Range;

namespace Embers.Expressions;
/// <summary>
/// Represents a range expression in the Embers language. 
/// A range expression defines a range of integers from a starting value to an ending value.
/// </summary>
/// <seealso cref="IExpression" />
public class RangeExpression(IExpression fromexpression, IExpression toexpression) : IExpression
{
    private static readonly int hashcode = typeof(RangeExpression).GetHashCode();

    private readonly IExpression fromexpression = fromexpression;
    private readonly IExpression toexpression = toexpression;

    public object Evaluate(Context context)
    {
        var fromValue = fromexpression.Evaluate(context);
        var toValue = toexpression.Evaluate(context);
        
        return new Range(fromValue, toValue);
    }

    public IList<string> GetLocalVariables()
    {
        var list = new List<IExpression>() { fromexpression, toexpression };
        return BaseExpression.GetLocalVariables(list);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not RangeExpression)
            return false;

        var rexpr = (RangeExpression)obj;

        return fromexpression.Equals(rexpr.fromexpression) && toexpression.Equals(rexpr.toexpression);
    }

    public override int GetHashCode() => hashcode + fromexpression.GetHashCode() + 7 * toexpression.GetHashCode();
}
