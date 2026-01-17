namespace Embers.Expressions;
/// <summary>
/// Represents a block expression in the Embers language.
/// </summary>
/// <seealso cref="BaseExpression" />
public class BlockExpression(IList<string> paramnames, IExpression expression) : BaseExpression
{
    private readonly IList<string> paramnames = paramnames;
    private readonly IExpression expression = expression;

    public IList<string> Parameters => paramnames;
    public IExpression Body => expression;


    public override object Evaluate(Context context) => new Block(paramnames, expression, context);

    public override bool Equals(object obj)
    {
        if (obj == null || obj is not BlockExpression)
            return false;

        var bexpr = (BlockExpression)obj;

        if (paramnames == null && bexpr.paramnames != null)
            return false;

        if (paramnames != null && bexpr.paramnames == null)
            return false;

        if (paramnames != null && paramnames.Count != bexpr.paramnames.Count)
            return false;

        if (!expression.Equals(bexpr.expression))
            return false;

        if (paramnames != null)
            for (int k = 0; k < paramnames.Count; k++)
                if (!paramnames[k].Equals(bexpr.paramnames[k]))
                    return false;

        return true;
    }

    public override int GetHashCode()
    {
        int result = typeof(BlockExpression).GetHashCode() + expression.GetHashCode();

        foreach (var paramname in paramnames) 
        {
            result *= 7;
            result += paramname.GetHashCode();
        }

        return result;
    }
}
