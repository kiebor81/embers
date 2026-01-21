namespace Embers.Expressions;

public sealed class SplatExpression(IExpression expression) : BaseExpression
{
    private readonly IExpression expression = expression;

    public IExpression Expression => expression;

    public override object Evaluate(Context context) => expression.Evaluate(context);

    public override bool Equals(object obj) => obj is SplatExpression other && expression.Equals(other.expression);

    public override int GetHashCode() => typeof(SplatExpression).GetHashCode() ^ expression.GetHashCode();
}
