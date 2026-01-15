using System.Text;

namespace Embers.Expressions;
/// <summary>
/// interpolated string expression that evaluates a list of expressions and concatenates their results into a single string.
/// An interpolated string expression can contain multiple parts, each of which can be a literal string or another expression.
/// </summary>
/// <seealso cref="BaseExpression" />
public class InterpolatedStringExpression(IList<IExpression> parts) : BaseExpression
{
    private readonly IList<IExpression> parts = parts;

    public override object Evaluate(Context context)
    {
        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            var value = part.Evaluate(context);
            sb.Append(value?.ToString() ?? "nil");
        }
        return sb.ToString();
    }

    public override bool Equals(object obj)
    {
        if (obj is not InterpolatedStringExpression other || parts.Count != other.parts.Count)
            return false;
        for (int i = 0; i < parts.Count; i++)
            if (!parts[i].Equals(other.parts[i]))
                return false;
        return true;
    }

    public override int GetHashCode()
    {
        int hash = typeof(InterpolatedStringExpression).GetHashCode();
        foreach (var part in parts)
            hash = hash * 31 + part.GetHashCode();
        return hash;
    }
}
