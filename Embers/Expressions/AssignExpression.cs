namespace Embers.Expressions
{
    /// <summary>
    /// AssignExpression represents an assignment operation in the expression tree.
    /// </summary>
    /// <seealso cref="Embers.Expressions.IExpression" />
    public class AssignExpression(string name, IExpression expression) : IExpression
    {
        private static readonly int hashtag = typeof(AssignExpression).GetHashCode();

        private readonly string name = name;
        private readonly IExpression expression = expression;

        public string Name { get { return name; } }

        public IExpression Expression { get { return expression; } }

        public object Evaluate(Context context)
        {
            object value = expression.Evaluate(context);
            
            context.SetValue(name, value);
            context.SetLocalValue(name, value);

            if (char.IsUpper(name[0]) && context.Module != null)
                context.Module.Constants.SetLocalValue(name, value);

            return value;
        }

        public IList<string> GetLocalVariables() => [name];

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is AssignExpression)
            {
                var cmd = (AssignExpression)obj;

                return Name.Equals(cmd.name) && Expression.Equals(cmd.Expression);
            }

            return false;
        }

        public override int GetHashCode() => Name.GetHashCode() + Expression.GetHashCode() + hashtag;
    }
}
