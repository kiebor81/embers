namespace Embers.Expressions
{
    /// <summary>
    /// TernaryExpression represents a conditional expression that evaluates to one of two expressions based on a condition.
    /// </summary>
    /// <seealso cref="Embers.Expressions.BaseExpression" />
    public class TernaryExpression(IExpression condition, IExpression trueExpr, IExpression falseExpr) : BaseExpression
    {
        private readonly IExpression condition = condition;
        private readonly IExpression trueExpr = trueExpr;
        private readonly IExpression falseExpr = falseExpr;

        public override object? Evaluate(Context context)
        {
            var cond = condition.Evaluate(context);
            return (cond is bool b && b) ? trueExpr.Evaluate(context) : falseExpr.Evaluate(context);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not TernaryExpression other) return false;
            return Equals(condition, other.condition)
                && Equals(trueExpr, other.trueExpr)
                && Equals(falseExpr, other.falseExpr);
        }

        public override int GetHashCode() => HashCode.Combine(condition, trueExpr, falseExpr);
    }
}
