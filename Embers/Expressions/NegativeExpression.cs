namespace Embers.Expressions
{
    /// <summary>
    /// NegativeExpression represents a negative expression in the Embers language.
    /// A negative expression negates the value of the given expression.
    /// </summary>
    /// <seealso cref="Embers.Expressions.IExpression" />
    public class NegativeExpression(IExpression expression) : IExpression
    {
        private static readonly int hashcode = typeof(NegativeExpression).GetHashCode();

        private readonly IExpression expression = expression;

        public IExpression Expression { get { return expression; } }

        public object Evaluate(Context context)
        {
            var value = expression.Evaluate(context);
            if (value is int i) return -i;
            if (value is double d) return -d;
            return -(int)value;
        }

        public IList<string> GetLocalVariables()
        {
            return expression.GetLocalVariables();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is NegativeExpression)
            {
                var expr = (NegativeExpression)obj;

                return expression.Equals(expr.expression);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Expression.GetHashCode() + hashcode;
        }
    }
}
