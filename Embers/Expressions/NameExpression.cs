using Embers.Exceptions;

namespace Embers.Expressions;
/// <summary>
/// NameExpression represents a named expression in the Embers language.
/// </summary>
/// <seealso cref="BaseExpression" />
/// <seealso cref="INamedExpression" />
public class NameExpression(string name) : BaseExpression, INamedExpression
{
    private static readonly int hashcode = typeof(NameExpression).GetHashCode();
    private static readonly IList<object> emptyvalues = [];
    private readonly string name = name;

    public IExpression? TargetExpression { get { return null; } }

    public string Name { get { return name; } }

    public override object Evaluate(Context context)
    {
        bool isglobal = char.IsUpper(name[0]);

        if (!isglobal)
        {
            if (context.HasLocalValue(name))
                return context.GetLocalValue(name);

            if (context.HasValue(name))
                return context.GetValue(name);

            if (context.Self != null)
            {
                var method = context.Self.GetMethod(name);

                if (method != null)
                    return method.Apply(context.Self, context, emptyvalues);
            }

            throw new NameError(string.Format("undefined local variable or method '{0}'", name));
        }

        if (context.HasValue(name))
            return context.GetValue(name);
        
        throw new NameError(string.Format("unitialized constant {0}", name));
    }

    public string? AsQualifiedName()
    {
        if (!char.IsUpper(name[0]))
            return null;

        return name;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj is NameExpression expr)
        {
            return Name.Equals(expr.Name);
        }

        return false;
    }

    public override int GetHashCode() => name.GetHashCode() + hashcode;
}
