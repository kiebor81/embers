namespace Embers.Expressions
{
    /// <summary>
    /// IfExpression represents a conditional expression in the Embers language.
    /// </summary>
    /// <seealso cref="Embers.Expressions.IExpression" />
    public class IfExpression(IExpression condition, IExpression thencommand, IExpression elsecommand) : IExpression
    {
        private static readonly int hashcode = typeof(IfExpression).GetHashCode();

        private readonly IExpression condition = condition;
        private readonly IExpression thencommand = thencommand;
        private readonly IExpression elsecommand = elsecommand;

        public IfExpression(IExpression condition, IExpression thencommand)
            : this(condition, thencommand, null)
        {
        }

        public object? Evaluate(Context context)
        {
            object value = condition.Evaluate(context);

            if (value == null || false.Equals(value))
                if (elsecommand == null)
                    return null;
                else
                    return elsecommand.Evaluate(context);

            return thencommand.Evaluate(context);
        }

        public IList<string> GetLocalVariables()
        {
            var list = new List<IExpression>() { condition, thencommand, elsecommand };

            return BaseExpression.GetLocalVariables(list);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is IfExpression)
            {
                var cmd = (IfExpression)obj;

                if (elsecommand == null)
                {
                    if (cmd.elsecommand != null)
                        return false;
                }
                else if (!elsecommand.Equals(cmd.elsecommand))
                    return false;

                return condition.Equals(cmd.condition) && thencommand.Equals(cmd.thencommand);
            }

            return false;
        }

        public override int GetHashCode() => condition.GetHashCode() + thencommand.GetHashCode() + hashcode;
    }
}
