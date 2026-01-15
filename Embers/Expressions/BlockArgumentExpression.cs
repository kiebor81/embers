namespace Embers.Expressions;
/// <summary>
/// BlockArgumentExpression represents a block being passed as an argument using the & prefix.
/// Example: helper(&block) passes the block parameter as a block to the method.
/// </summary>
public class BlockArgumentExpression(IExpression expression) : BaseExpression
{
    private readonly IExpression expression = expression;

    public IExpression Expression => expression;

    public override object Evaluate(Context context) =>
        // Evaluate the expression to get the value (should be a Proc)
        expression.Evaluate(context);

    public override bool Equals(object? obj) => obj is BlockArgumentExpression other && expression.Equals(other.expression);

    public override int GetHashCode() => expression.GetHashCode();

    public override string ToString() => $"&{expression}";
}
