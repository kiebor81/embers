namespace Embers.Expressions
{
    /// <summary>
    /// BlockArgumentExpression represents a block being passed as an argument using the & prefix.
    /// Example: helper(&block) passes the block parameter as a block to the method.
    /// </summary>
    public class BlockArgumentExpression(IExpression expression) : BaseExpression
    {
        private readonly IExpression expression = expression;

        public IExpression Expression => expression;

        public override object Evaluate(Context context)
        {
            // Evaluate the expression to get the value (should be a Proc)
            return expression.Evaluate(context);
        }

        public override bool Equals(object? obj)
        {
            return obj is BlockArgumentExpression other && expression.Equals(other.expression);
        }

        public override int GetHashCode()
        {
            return expression.GetHashCode();
        }

        public override string ToString()
        {
            return $"&{expression}";
        }
    }
}
