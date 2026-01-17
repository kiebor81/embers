namespace Embers.Expressions;
/// <summary>
/// InstanceVarExpression represents an expression that retrieves the value of an instance variable.
/// This expression is used to access instance variables of the current object (self) in the context of the execution.
/// </summary>
/// <seealso cref="BaseExpression" />
public class InstanceVarExpression(string name) : BaseExpression
{
    private static readonly int hashcode = typeof(InstanceVarExpression).GetHashCode();
    private readonly string name = name;

    public string Name { get { return name; } }

    public override object Evaluate(Context context)
    {
        var result = context.Self.GetValue(name);

        return result;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj is InstanceVarExpression expr)
        {
            return Name.Equals(expr.Name);
        }

        return false;
    }

    public override int GetHashCode() => name.GetHashCode() + hashcode;
}
