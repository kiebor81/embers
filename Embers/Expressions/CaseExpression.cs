using Embers.Exceptions;

namespace Embers.Expressions;

/// <summary>
/// Represents a pattern used in CASE clauses.
/// </summary>
public interface ICasePattern { }

/// <summary>
/// Represents a case clause with a body.
/// </summary>
public interface ICaseClause
{
    IExpression Body { get; }
}

/// <summary>
/// Represents a pattern that matches a specific expression.
/// </summary>
public sealed class ExpressionPattern(IExpression expression) : ICasePattern
{
    private static readonly int hashcode = typeof(ExpressionPattern).GetHashCode();
    private readonly IExpression expression = expression;

    public IExpression Expression => expression;

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
        => obj is ExpressionPattern pattern && expression.Equals(pattern.expression);

    public override int GetHashCode() => hashcode + expression.GetHashCode();
}

/// <summary>
/// Represents a pattern that binds a value to a local name.
/// </summary>
public sealed class BindingPattern(string name) : ICasePattern
{
    private static readonly int hashcode = typeof(BindingPattern).GetHashCode();
    private readonly string name = name;

    public string Name => name;

    public override bool Equals(object? obj)
        => obj is BindingPattern pattern && name == pattern.name;

    public override int GetHashCode() => hashcode + name.GetHashCode();
}

/// <summary>
/// Represents an entry in a hash pattern.
/// </summary>
public sealed class HashPatternEntry(IExpression key, ICasePattern pattern)
{
    private static readonly int hashcode = typeof(HashPatternEntry).GetHashCode();
    private readonly IExpression key = key;
    private readonly ICasePattern pattern = pattern;

    public IExpression Key => key;
    public ICasePattern Pattern => pattern;

    public override bool Equals(object? obj)
        => obj is HashPatternEntry entry
            && key.Equals(entry.key)
            && pattern.Equals(entry.pattern);

    public override int GetHashCode()
        => hashcode + key.GetHashCode() + pattern.GetHashCode();
}

/// <summary>
/// Represents a hash destructuring pattern.
/// </summary>
public sealed class HashPattern(IReadOnlyList<HashPatternEntry> entries) : ICasePattern
{
    private static readonly int hashcode = typeof(HashPattern).GetHashCode();
    private readonly IReadOnlyList<HashPatternEntry> entries = entries;

    public IReadOnlyList<HashPatternEntry> Entries => entries;

    public override bool Equals(object? obj)
    {
        if (obj is not HashPattern pattern)
            return false;

        if (entries.Count != pattern.entries.Count)
            return false;

        for (int i = 0; i < entries.Count; i++)
        {
            if (!entries[i].Equals(pattern.entries[i]))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int result = hashcode;

        foreach (var entry in entries)
            result += entry.GetHashCode();

        return result;
    }
}

/// <summary>
/// Represents a WHEN clause with match patterns.
/// </summary>
public sealed class CaseWhenClause(IReadOnlyList<ICasePattern> patterns, IExpression body) : ICaseClause
{
    private static readonly int hashcode = typeof(CaseWhenClause).GetHashCode();
    private readonly IReadOnlyList<ICasePattern> patterns = patterns;
    private readonly IExpression body = body;

    public IReadOnlyList<ICasePattern> Patterns => patterns;
    public IExpression Body => body;

    public override bool Equals(object? obj)
    {
        if (obj is not CaseWhenClause clause)
            return false;

        if (!body.Equals(clause.body))
            return false;

        if (patterns.Count != clause.patterns.Count)
            return false;

        for (int i = 0; i < patterns.Count; i++)
        {
            if (!patterns[i].Equals(clause.patterns[i]))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int result = hashcode + body.GetHashCode();

        foreach (var pattern in patterns)
            result += pattern.GetHashCode();

        return result;
    }
}

/// <summary>
/// Represents an IN clause with a single pattern.
/// </summary>
public sealed class CaseInClause(ICasePattern pattern, IExpression body) : ICaseClause
{
    private static readonly int hashcode = typeof(CaseInClause).GetHashCode();
    private readonly ICasePattern pattern = pattern;
    private readonly IExpression body = body;

    public ICasePattern Pattern => pattern;
    public IExpression Body => body;

    public override bool Equals(object? obj)
        => obj is CaseInClause clause
            && pattern.Equals(clause.pattern)
            && body.Equals(clause.body);

    public override int GetHashCode()
        => hashcode + pattern.GetHashCode() + body.GetHashCode();
}

/// <summary>
/// Represents a CASE expression with optional subject, multiple WHEN clauses, and an optional ELSE expression.
/// </summary>
public sealed class CaseExpression(IExpression? subject, IReadOnlyList<ICaseClause> clauses, IExpression? elseExpression) : BaseExpression
{
    private static readonly int hashcode = typeof(CaseExpression).GetHashCode();
    private readonly IExpression? subject = subject;
    private readonly IReadOnlyList<ICaseClause> clauses = clauses;
    private readonly IExpression? elseExpression = elseExpression;

    public IExpression? Subject => subject;
    public IReadOnlyList<ICaseClause> Clauses => clauses;
    public IExpression? ElseExpression => elseExpression;

    public override object? Evaluate(Context context)
        => throw new NotSupportedError("case expressions are not implemented yet");

    public override bool Equals(object? obj)
    {
        if (obj is not CaseExpression expression)
            return false;

        if (subject == null && expression.subject != null)
            return false;

        if (subject != null && !subject.Equals(expression.subject))
            return false;

        if (elseExpression == null && expression.elseExpression != null)
            return false;

        if (elseExpression != null && !elseExpression.Equals(expression.elseExpression))
            return false;

        if (clauses.Count != expression.clauses.Count)
            return false;

        for (int i = 0; i < clauses.Count; i++)
        {
            if (!clauses[i].Equals(expression.clauses[i]))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int result = hashcode;

        if (subject != null)
            result += subject.GetHashCode();

        if (elseExpression != null)
            result += elseExpression.GetHashCode();

        foreach (var clause in clauses)
            result += clause.GetHashCode();

        return result;
    }
}
