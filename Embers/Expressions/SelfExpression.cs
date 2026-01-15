namespace Embers.Expressions;
/// <summary>
/// SelfExpression represents the 'self' keyword in the Embers language.
/// </summary>
/// <seealso cref="BaseExpression" />
/// <seealso cref="INamedExpression" />
public class SelfExpression : BaseExpression, INamedExpression
{
    private static readonly int hashcode = typeof(SelfExpression).GetHashCode();

    public SelfExpression()
    {
    }

    public IExpression? TargetExpression { get { return null; } }

    public string Name { get { return "self"; } }

    public override object Evaluate(Context context) => context.Self;

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        return obj is SelfExpression;
    }

    public override int GetHashCode() => hashcode;
}
