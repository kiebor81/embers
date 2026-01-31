namespace Embers.Expressions;

/// <summary>
/// Represents a keyword splat expression (e.g., **kwargs) in the Embers language.
/// </summary>
public sealed class KeywordSplatExpression(IExpression expression) : BaseExpression
{
    private readonly IExpression expression = expression;

    public IExpression Expression => expression;

    public override object Evaluate(Context context) => expression.Evaluate(context);

    public override bool Equals(object? obj) => obj is KeywordSplatExpression other && expression.Equals(other.expression);

    public override int GetHashCode() => typeof(KeywordSplatExpression).GetHashCode() ^ expression.GetHashCode();
}
