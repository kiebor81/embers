using System.Text.RegularExpressions;

namespace Embers.Expressions;

/// <summary>
/// RegexLiteralExpression represents a regular expression literal.
/// </summary>
public sealed class RegexLiteralExpression(string pattern, RegexOptions options) : IExpression
{
    private static readonly int hashcode = typeof(RegexLiteralExpression).GetHashCode();

    private readonly string pattern = pattern;
    private readonly RegexOptions options = options;

    public string Pattern => pattern;

    public RegexOptions Options => options;

    public object Evaluate(Context context) => new Regexp(pattern, options);

    public IList<string> GetLocalVariables() => [];

    public override bool Equals(object? obj)
    {
        if (obj is not RegexLiteralExpression expr)
            return false;

        return pattern == expr.pattern && options == expr.options;
    }

    public override int GetHashCode() => hashcode + pattern.GetHashCode() + (int)options;
}
