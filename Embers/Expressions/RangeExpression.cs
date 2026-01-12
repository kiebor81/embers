using Range = Embers.Language.Range;

namespace Embers.Expressions
{
    /// <summary>
    /// Represents a range expression in the Embers language. 
    /// A range expression defines a range of integers from a starting value to an ending value.
    /// </summary>
    /// <seealso cref="Embers.Expressions.IExpression" />
    public class RangeExpression(IExpression fromexpression, IExpression toexpression) : IExpression
    {
        private static readonly int hashcode = typeof(RangeExpression).GetHashCode();

        private readonly IExpression fromexpression = fromexpression;
        private readonly IExpression toexpression = toexpression;

        public object Evaluate(Context context)
        {
            var fromValue = fromexpression.Evaluate(context);
            var toValue = toexpression.Evaluate(context);
            
            // Convert long to int for range (arrays need int indices)
            int from = fromValue is long l1 ? (int)l1 : (int)fromValue;
            int to = toValue is long l2 ? (int)l2 : (int)toValue;
            
            return new Range(from, to);
        }

        public IList<string> GetLocalVariables()
        {
            var list = new List<IExpression>() { fromexpression, toexpression };
            return BaseExpression.GetLocalVariables(list);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not RangeExpression)
                return false;

            var rexpr = (RangeExpression)obj;

            return fromexpression.Equals(rexpr.fromexpression) && toexpression.Equals(rexpr.toexpression);
        }

        public override int GetHashCode()
        {
            return hashcode + fromexpression.GetHashCode() + 7 * toexpression.GetHashCode();
        }
    }
}
