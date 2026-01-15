using Embers.Exceptions;

namespace Embers.Expressions;
/// <summary>
/// YieldExpression is used to yield control to a block of code, passing arguments to it.
/// </summary>
/// <seealso cref="BaseExpression" />
public class YieldExpression(IList<IExpression> args) : BaseExpression
{
    private readonly IList<IExpression> arguments = args;

    public override object? Evaluate(Context context)
    {
        if (context.Block == null)
            throw new NameError("no block given");

        var values = new List<object>();
        foreach (var arg in arguments)
            values.Add(arg.Evaluate(context));

        return context.Block.Apply(context.Self, context, values);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not YieldExpression other || arguments.Count != other.arguments.Count)
            return false;

        for (int i = 0; i < arguments.Count; i++)
            if (!arguments[i].Equals(other.arguments[i]))
                return false;

        return true;
    }

    public override int GetHashCode()
    {
        int result = typeof(YieldExpression).GetHashCode();
        foreach (var arg in arguments)
            result ^= arg.GetHashCode();
        return result;
    }
}
