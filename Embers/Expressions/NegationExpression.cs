namespace Embers.Expressions;
/// <summary>
/// NegationExpression represents a negation operation in the Embers language.
/// A negation expression evaluates to true if the inner expression evaluates to false or null.
/// </summary>
/// <seealso cref="IExpression" />
public class NegationExpression(IExpression expression) : IExpression
{
    private static readonly int hashcode = typeof(NegationExpression).GetHashCode();

    private readonly IExpression expression = expression;

    public IExpression Expression { get { return expression; } }

    public object Evaluate(Context context)
    {
        var value = expression.Evaluate(context);

        if (value == null || false.Equals(value))
            return true;

        return false;
    }

    public IList<string> GetLocalVariables() => expression.GetLocalVariables();

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj is NegationExpression)
        {
            var expr = (NegationExpression)obj;

            return expression.Equals(expr.expression);
        }

        return false;
    }

    public override int GetHashCode() => Expression.GetHashCode() + hashcode;
}
