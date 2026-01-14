using Embers.Signals;

namespace Embers.Expressions
{
    /// <summary>
    /// NextExpression is used to skip the current iteration of a loop and move to the next one.
    /// It represents the keyword "next" in the language.
    /// </summary>
    /// <seealso cref="Embers.Expressions.BaseExpression" />
    public class NextExpression : BaseExpression
    {
        public override object? Evaluate(Context context) => throw new NextSignal();
    }
}
