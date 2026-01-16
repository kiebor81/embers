namespace Embers.Expressions;
/// <summary>
/// AssignExpression for setting a global variable.
/// </summary>
/// <seealso cref="IExpression" />
public class AssignGlobalVarExpression(string name, IExpression expression) : IExpression
{
    private static readonly int hashtag = typeof(AssignGlobalVarExpression).GetHashCode();

    private readonly string name = name;
    private readonly IExpression expression = expression;

    public string Name { get { return name; } }

    public IExpression Expression { get { return expression; } }

    public IList<string>? GetLocalVariables() => expression.GetLocalVariables();

    public object? Evaluate(Context context)
    {
        object? value = expression.Evaluate(context);
        context.SetGlobalValue(name, value);
        return value;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is AssignGlobalVarExpression)
        {
            var cmd = (AssignGlobalVarExpression)obj;

            return Name.Equals(cmd.name) && Expression.Equals(cmd.Expression);
        }

        return false;
    }

    public override int GetHashCode() => Name.GetHashCode() + Expression.GetHashCode() + hashtag;
}
