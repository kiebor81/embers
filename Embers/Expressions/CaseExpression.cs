using System;
using System.Collections;
using Embers.Exceptions;
using Embers.Language.Dynamic;
using Embers.Language.Native;
using Embers.Language.Primitive;
using Embers.StdLib.Numeric;
using Microsoft.VisualBasic.CompilerServices;
using Range = Embers.Language.Primitive.Range;

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
    {
        bool hasSubject = subject != null;
        object? subjectValue = hasSubject ? subject!.Evaluate(context) : null;

        foreach (var clause in clauses)
        {
            if (clause is CaseWhenClause whenClause)
            {
                if (MatchesWhenClause(context, whenClause, hasSubject, subjectValue))
                    return whenClause.Body.Evaluate(context);

                continue;
            }

            if (clause is CaseInClause inClause)
            {
                if (MatchesInClause(context, inClause.Pattern, subjectValue, out var bindings))
                {
                    ApplyBindings(context, bindings);
                    return inClause.Body.Evaluate(context);
                }
            }
        }

        return elseExpression?.Evaluate(context);
    }

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

    private static bool MatchesWhenClause(Context context, CaseWhenClause clause, bool hasSubject, object? subjectValue)
    {
        foreach (var pattern in clause.Patterns)
        {
            if (pattern is not ExpressionPattern exprPattern)
                throw new NotSupportedError("unsupported case pattern in when clause");

            var patternValue = exprPattern.Expression.Evaluate(context);
            bool matched = hasSubject
                ? CaseMatches(context, patternValue, subjectValue)
                : Predicates.IsTrue(patternValue);

            if (matched)
                return true;
        }

        return false;
    }

    private static bool MatchesInClause(Context context, ICasePattern pattern, object? subjectValue, out List<(string Name, object? Value)> bindings)
    {
        bindings = [];
        return MatchPattern(context, pattern, subjectValue, bindings);
    }

    private static bool MatchPattern(Context context, ICasePattern pattern, object? subjectValue, List<(string Name, object? Value)> bindings)
    {
        if (pattern is ExpressionPattern expressionPattern)
        {
            var patternValue = expressionPattern.Expression.Evaluate(context);
            return CaseMatches(context, patternValue, subjectValue);
        }

        if (pattern is BindingPattern bindingPattern)
        {
            bindings.Add((bindingPattern.Name, subjectValue));
            return true;
        }

        if (pattern is HashPattern hashPattern)
            return MatchHashPattern(context, hashPattern, subjectValue, bindings);

        throw new NotSupportedError("unsupported case pattern in in clause");
    }

    private static bool MatchHashPattern(Context context, HashPattern pattern, object? subjectValue, List<(string Name, object? Value)> bindings)
    {
        if (subjectValue is not IDictionary dictionary)
            return false;

        foreach (var entry in pattern.Entries)
        {
            var key = entry.Key.Evaluate(context);
            if (!dictionary.Contains(key))
                return false;

            var value = dictionary[key];
            if (!MatchPattern(context, entry.Pattern, value, bindings))
                return false;
        }

        return true;
    }

    private static void ApplyBindings(Context context, List<(string Name, object? Value)> bindings)
    {
        foreach (var (name, value) in bindings)
            context.SetLocalValue(name, value!);
    }

    private static bool CaseMatches(Context context, object? patternValue, object? subjectValue)
    {
        if (patternValue is Regexp regexp)
        {
            if (subjectValue == null)
                return false;

            return regexp.Regex.IsMatch(subjectValue.ToString() ?? string.Empty);
        }

        if (patternValue is Proc proc)
        {
            var result = proc.Call([subjectValue]);
            return Predicates.IsTrue(result);
        }

        if (patternValue is Range range)
        {
            if (!NumericCoercion.TryGetDouble(subjectValue, out var testValue))
                return false;

            if (!range.TryGetDoubleBounds(out var start, out var end))
                throw new TypeError("range must be numeric");

            if (start > end)
                return false;

            return testValue >= start && testValue <= end;
        }

        if (patternValue is DynamicClass patternClass)
            return IsInstanceOfClass(context, subjectValue, patternClass);

        if (patternValue is Type type)
            return subjectValue != null && type.IsInstanceOfType(subjectValue);

        if (TryInvokeCaseMethod(context, patternValue, subjectValue, out bool methodResult))
            return methodResult;

        return Operators.CompareObjectEqual(patternValue, subjectValue, false) is true;
    }

    private static bool TryInvokeCaseMethod(Context context, object? patternValue, object? subjectValue, out bool result)
    {
        if (patternValue is DynamicObject dynamicPattern)
        {
            var method = dynamicPattern.GetMethod("===");
            if (method != null)
            {
                result = Predicates.IsTrue(method.Apply(dynamicPattern, context, [subjectValue]));
                return true;
            }
        }
        else
        {
            var nativeClass = NativeClassResolver.Resolve(context, patternValue);
            if (nativeClass != null)
            {
                var method = nativeClass.GetInstanceMethodNoSuper("===");
                if (method != null)
                {
                    var nativeObj = new NativeObject(nativeClass, patternValue);
                    result = Predicates.IsTrue(method.Apply(nativeObj, context, [subjectValue]));
                    return true;
                }
            }
        }

        result = false;
        return false;
    }

    private static bool IsInstanceOfClass(Context context, object? subjectValue, DynamicClass patternClass)
    {
        if (subjectValue is DynamicObject dynamicObject)
            return IsClassMatch(dynamicObject.Class, patternClass);

        var nativeClass = NativeClassResolver.Resolve(context, subjectValue);
        if (nativeClass != null)
            return IsClassMatch(nativeClass, patternClass);

        return false;
    }

    private static bool IsClassMatch(DynamicClass? current, DynamicClass patternClass)
    {
        for (; current != null; current = current.SuperClass)
        {
            if (ReferenceEquals(current, patternClass))
                return true;

            if (IncludesMixin(current, patternClass))
                return true;
        }

        return false;
    }

    private static bool IncludesMixin(DynamicClass current, DynamicClass patternClass)
    {
        foreach (var mixin in current.Mixins)
        {
            if (ReferenceEquals(mixin, patternClass))
                return true;

            if (IncludesMixin(mixin, patternClass))
                return true;
        }

        return false;
    }
}
