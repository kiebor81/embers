namespace Embers.Expressions;
/// <summary>
/// ModuleExpression is used to execute a module definition or public module method definition.
/// </summary>
/// <seealso cref="BaseExpression" />
public class ModuleExpression(string name, IExpression expression) : BaseExpression
{
    private static readonly int hashcode = typeof(ClassExpression).GetHashCode();

    private readonly string name = name;
    private readonly IExpression expression = expression;

    public override object Evaluate(Context context)
    {
        object value = null;

        if (context.Module != null)
        {
            if (context.Module.Constants.HasLocalValue(name))
                value = context.Module.Constants.GetLocalValue(name);
        }
        else if (context.HasLocalValue(name))
            value = context.GetLocalValue(name);

        DynamicClass module;

        if (value == null || value is not DynamicClass)
        {
            DynamicClass modclass = (DynamicClass)context.RootContext.GetLocalValue("Module");
            var superclass = (DynamicClass)context.RootContext.GetLocalValue("Object");
            module = new DynamicClass(modclass, name, superclass, context.Module);

            if (context.Module != null)
                context.Module.Constants.SetLocalValue(name, module);
            else
                context.RootContext.SetLocalValue(name, module);
        }
        else
            module = (DynamicClass)value;

        Context newcontext = new(module, context.RootContext);
        return expression.Evaluate(newcontext);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is ModuleExpression expr)
        {
            return name == expr.name && expression.Equals(expr.expression);
        }

        return false;
    }

    public override int GetHashCode() => name.GetHashCode() + expression.GetHashCode() + hashcode;
}
