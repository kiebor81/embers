using Embers.Functions;

namespace Embers.Expressions;
/// <summary>
/// DefExpression represents a function definition.
/// A DefExpression consists of a named expression, a list of parameters, and an expression that defines the function body.
/// </summary>
/// <seealso cref="BaseExpression" />
public class DefExpression(INamedExpression namedexpression, IList<string> parameters, string? splatParameterName, string? kwargsParameterName, IExpression expression, string? blockParameterName = null) : BaseExpression
{
    private static readonly int hashcode = typeof(DefExpression).GetHashCode();

    private readonly INamedExpression namedexpression = namedexpression;
    private readonly IList<string> parameters = parameters;
    private readonly string? splatParameterName = splatParameterName;
    private readonly string? kwargsParameterName = kwargsParameterName;
    private readonly IExpression expression = expression;
    private readonly string? blockParameterName = blockParameterName;

    public IList<string> Parameters => parameters;
    public string? SplatParameterName => splatParameterName;
    public string? KwargsParameterName => kwargsParameterName;
    public string? BlockParameterName => blockParameterName;

    public override object? Evaluate(Context context)
    {
        var result = new DefinedFunction(expression, parameters, splatParameterName, kwargsParameterName, context, blockParameterName);

        if (namedexpression.TargetExpression == null)
        {
            if (context.Module != null)
                context.Module.SetInstanceMethod(namedexpression.Name, result);
            else
                context.Self.Class.SetInstanceMethod(namedexpression.Name, result);
                //((DynamicClass)context.Self).SetInstanceMethod(namedexpression.Name, result);
        }
        else
        {
            var target = (DynamicObject)namedexpression.TargetExpression.Evaluate(context);
            target.SingletonClass.SetInstanceMethod(namedexpression.Name, result);
        }

        return null;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj is DefExpression expr)
        {
            if (parameters.Count != expr.parameters.Count)
                return false;

            for (int k = 0; k < parameters.Count; k++)
                if (parameters[k] != expr.parameters[k])
                    return false;

            return namedexpression.Equals(expr.namedexpression)
                && expression.Equals(expr.expression)
                && splatParameterName == expr.splatParameterName
                && kwargsParameterName == expr.kwargsParameterName
                && blockParameterName == expr.blockParameterName;
        }

        return false;
    }

    public override int GetHashCode()
    {
        int result = hashcode + namedexpression.GetHashCode() + expression.GetHashCode();

        foreach (var parameter in parameters)
            result += parameter.GetHashCode();

        if (splatParameterName != null)
            result += splatParameterName.GetHashCode();

        if (kwargsParameterName != null)
            result += kwargsParameterName.GetHashCode();

        if (blockParameterName != null)
            result += blockParameterName.GetHashCode();

        return result;
    }
}
