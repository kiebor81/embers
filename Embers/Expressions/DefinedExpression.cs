namespace Embers.Expressions
{
    /// <summary>
    /// DefinedExpression checks if a given expression is defined in the current context.
    /// It can be used to determine if a variable, method, or class is defined within the scope of the current execution context.
    /// It represents the `defined?` operator for primitive introspection.
    /// </summary>
    /// <seealso cref="Embers.Expressions.BaseExpression" />
    public class DefinedExpression(IExpression expression) : BaseExpression
    {
        private readonly IExpression inner = expression;

        public override object? Evaluate(Context context)
        {
            if (inner is NameExpression name)
            {
                if (context.HasLocalValue(name.Name))
                    return "local-variable";
            }

            // Optional: method/class/module detection could go here later

            return null;
        }

        public override bool Equals(object? obj) => obj is DefinedExpression other &&
                   Equals(inner, other.inner);

        public override int GetHashCode() => inner.GetHashCode() ^ typeof(DefinedExpression).GetHashCode();
    }
}
