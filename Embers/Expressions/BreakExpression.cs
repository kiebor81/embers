using Embers.Signals;

namespace Embers.Expressions;
/// <summary>
/// Evaluates a break expression, which is used to exit from loops or control structures.
/// </summary>
/// <seealso cref="BaseExpression" />
public class BreakExpression(IExpression? expr) : BaseExpression
{
    public override object? Evaluate(Context context)
    {
        object? value = expr?.Evaluate(context);
        throw new BreakSignal(value);
    }
}
