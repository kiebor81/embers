using Embers.Exceptions;

namespace Embers.Expressions;
/// <summary>
/// ClassExpression is used to execute a class definition or public class method definition.
/// </summary>
/// <seealso cref="BaseExpression" />
public class ClassExpression(INamedExpression namedexpression, IExpression expression, INamedExpression superclassexpression = null) : BaseExpression
{
    private static readonly int hashcode = typeof(ClassExpression).GetHashCode();
    private readonly INamedExpression namedexpression = namedexpression;
    private readonly IExpression expression = expression;
    private readonly INamedExpression superclassexpression = superclassexpression;

    public override object? Evaluate(Context context)
    {
        object value = null;
        DynamicClass target = null;

        if (namedexpression.TargetExpression == null)
        {
            if (context.Module != null)
            {
                if (context.Module.Constants.HasLocalValue(namedexpression.Name))
                    value = context.Module.Constants.GetLocalValue(namedexpression.Name);
            }
            else if (context.HasValue(namedexpression.Name))
                value = context.GetValue(namedexpression.Name);
        }
        else
        {
            object targetvalue = namedexpression.TargetExpression.Evaluate(context);

            if (targetvalue is not DynamicClass)
                throw new TypeError(string.Format("{0} is not a class/module", targetvalue.ToString()));

            target = (DynamicClass)targetvalue;

            if (target.Constants.HasLocalValue(namedexpression.Name))
                value = target.Constants.GetLocalValue(namedexpression.Name);
        }

        if (value == null || value is not DynamicClass)
        {
            var classclass = (DynamicClass)context.RootContext.GetLocalValue("Class");
            var superclass = (DynamicClass)context.RootContext.GetLocalValue("Object");
            string name = namedexpression.Name;
            var parent = target ?? context.Module;

            if (superclassexpression != null)
                superclass = (DynamicClass)superclassexpression.Evaluate(context);
            
            var newclass = new DynamicClass(classclass, name, superclass, parent);

            if (parent == null)
                context.RootContext.SetLocalValue(name, newclass);
            else
                parent.Constants.SetLocalValue(name, newclass);

            value = newclass;
        }

        var dclass = (DynamicClass)value;

        Context classcontext = new(dclass, context);
        classcontext.Self = dclass;

        //expression.Evaluate(classcontext);

        if (expression is CompositeExpression composite)
        {
            // Pass 1: evaluate all method definitions first
            foreach (var expr in composite.Commands)
            {
                if (expr is DefExpression)
                    expr.Evaluate(classcontext);
            }

            // Pass 2: evaluate other expressions (alias, constants, etc.)
            foreach (var expr in composite.Commands)
            {
                if (expr is not DefExpression)
                    expr.Evaluate(classcontext);
            }
        }
        else
        {
            // Fallback: not a composite block
            expression.Evaluate(classcontext);
        }

        return null;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj is ClassExpression expr)
        {
            return namedexpression.Equals(expr.namedexpression) && expression.Equals(expr.expression);
        }

        return false;
    }

    public override int GetHashCode() => namedexpression.GetHashCode() + expression.GetHashCode() + hashcode;
}
