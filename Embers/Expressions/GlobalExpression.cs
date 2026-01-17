using Embers.Exceptions;

namespace Embers.Expressions;
/// <summary>
/// GlobalExpression represents a global variable expression.
/// </summary>
/// <seealso cref="BaseExpression" />
public class GlobalExpression(string name) : BaseExpression
{
    private static readonly int hashcode = typeof(GlobalExpression).GetHashCode();
    private readonly string name = name;

    public string Name { get { return name; } }

    public override object Evaluate(Context context)
    {
        if (context.HasGlobalValue(name))
            return context.GetGlobalValue(name);

        throw new NameError(string.Format("undefined global variable '{0}'", name));
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj is GlobalExpression expr)
        {
            return Name.Equals(expr.Name);
        }

        return false;
    }

    public override int GetHashCode() => name.GetHashCode() + hashcode;
}
