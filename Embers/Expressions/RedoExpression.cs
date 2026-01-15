using Embers.Signals;

namespace Embers.Expressions;
/// <summary>
/// RedoExpression is used to redo the last operation or iteration in the Embers language.
/// </summary>
/// <seealso cref="BaseExpression" />
public class RedoExpression : BaseExpression
{
    public override object? Evaluate(Context context) => throw new RedoSignal();
}
