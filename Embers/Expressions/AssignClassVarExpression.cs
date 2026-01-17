namespace Embers.Expressions;
/// <summary>
/// AssignExpression for setting a class variable in the current context.
/// </summary>
/// <seealso cref="IExpression" />
public class AssignClassVarExpression(string name, IExpression expression) : IExpression
{
    private static readonly int hashtag = typeof(AssignClassVarExpression).GetHashCode();

    private readonly string name = name;
    private readonly IExpression expression = expression;

    public string Name { get { return name; } }

    public IExpression Expression { get { return expression; } }

    public IList<string>? GetLocalVariables() => expression.GetLocalVariables();

    public object? Evaluate(Context context)
    {
        object? value = expression.Evaluate(context);
        var target = context.Self is DynamicClass ? context.Self : context.Self.Class;
        target.SetValue(name, value);
        return value;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is AssignClassVarExpression cmd)
        {
            return Name.Equals(cmd.name) && Expression.Equals(cmd.Expression);
        }

        return false;
    }

    public override int GetHashCode() => Name.GetHashCode() + Expression.GetHashCode() + hashtag;
}
