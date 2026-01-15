using Embers.Signals;
using System.Collections;

namespace Embers.Expressions;
/// <summary>
/// ForInExpression represents a "for in" loop in the expression tree.
/// A "for in" loop iterates over a collection, executing a command for each element in the collection.
/// </summary>
/// <seealso cref="IExpression" />
public class ForInExpression(string name, IExpression expression, IExpression command) : IExpression
{
    private static readonly int hashcode = typeof(ForInExpression).GetHashCode();

    private readonly string name = name;
    private readonly IExpression expression = expression;
    private readonly IExpression command = command;

    //public object? Evaluate(Context context)
    //{
    //    IEnumerable elements = (IEnumerable)expression.Evaluate(context);

    //    foreach (var element in elements) 
    //    {
    //        context.SetLocalValue(name, element);
    //        command.Evaluate(context);
    //    }

    //    return null;
    //}

    public object? Evaluate(Context context)
    {
        IEnumerable elements = (IEnumerable)expression.Evaluate(context);
        object? result = null;

        foreach (var element in elements)
        {
            context.SetLocalValue(name, element);

            try
            {
                result = command.Evaluate(context);
            }
            catch (NextSignal)
            {
                continue; // Skip to next element
            }
            catch (RedoSignal)
            {
                // Re-run current iteration
                context.SetLocalValue(name, element);
                goto retry;
            }
            catch (BreakSignal br)
            {
                return br.Value;
            }

            continue;

        retry:
            try
            {
                result = command.Evaluate(context);
            }
            catch (NextSignal)
            {
                continue;
            }
            catch (RedoSignal)
            {
                goto retry;
            }
            catch (BreakSignal br)
            {
                return br.Value;
            }
        }

        return result;
    }

    public IList<string> GetLocalVariables()
    {
        var list = new List<IExpression>() { new AssignExpression(name, null), expression, command };

        return BaseExpression.GetLocalVariables(list);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj is ForInExpression)
        {
            var cmd = (ForInExpression)obj;
            if (!name.Equals(cmd.name))
                return false;
            if (!expression.Equals(cmd.expression))
                return false;
            return command.Equals(cmd.command);
        }

        return false;
    }

    public override int GetHashCode() => hashcode + name.GetHashCode() + expression.GetHashCode() + command.GetHashCode();
}
