namespace Embers.Expressions;
/// <summary>
/// A primitive base class for all expressions in the Embers language.
/// </summary>
/// <seealso cref="IExpression" />
public abstract class BaseExpression : IExpression
{
    public abstract object Evaluate(Context context);

    public IList<string>? GetLocalVariables() => null;

    internal static IList<string>? GetLocalVariables(IList<IExpression> expressions)
    {
        IList<string> varnames = [];

        foreach (var expression in expressions)
        {
            if (expression == null)
                continue;

            var vars = expression.GetLocalVariables();

            if (vars == null || vars.Count == 0)
                continue;

            foreach (var name in vars)
                if (!varnames.Contains(name))
                    varnames.Add(name);
        }

        if (varnames.Count == 0)
            return null;

        return varnames;
    }
}
