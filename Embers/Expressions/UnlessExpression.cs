namespace Embers.Expressions
{
    /// <summary>
    /// UnlessExpression represents a conditional expression that executes a block if the condition is false.
    /// </summary>
    /// <seealso cref="Embers.Expressions.BaseExpression" />
    public class UnlessExpression(IExpression condition, IExpression thenBlock, IExpression? elseBlock) : BaseExpression
    {
        private readonly IExpression condition = condition;
        private readonly IExpression thenBlock = thenBlock;
        private readonly IExpression? elseBlock = elseBlock;

        public override object? Evaluate(Context context)
        {
            var cond = condition.Evaluate(context);
            bool isTrue = cond is bool b && b;
            if (!isTrue)
                return thenBlock.Evaluate(context);
            else if (elseBlock != null)
                return elseBlock.Evaluate(context);
            return null;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not UnlessExpression other) return false;
            return Equals(condition, other.condition)
                && Equals(thenBlock, other.thenBlock)
                && Equals(elseBlock, other.elseBlock);
        }

        public override int GetHashCode() => condition.GetHashCode() ^ thenBlock.GetHashCode() ^ (elseBlock?.GetHashCode() ?? 0);
    }
}
