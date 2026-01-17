using Embers.Signals;

namespace Embers.Expressions;
/// <summary>
/// UntilExpression represents a loop that continues executing a command until a specified condition is met.
/// </summary>
/// <seealso cref="IExpression" />
public class UntilExpression(IExpression condition, IExpression command) : IExpression
{
    private static readonly int hashcode = typeof(UntilExpression).GetHashCode();

    private readonly IExpression condition = condition;
    private readonly IExpression command = command;

    public object? Evaluate(Context context)
    {
        object? result = null;

        while (true)
        {
            var value = condition.Evaluate(context);

            // Run the loop while condition is falsy
            if (value != null && !false.Equals(value))
                break;

            try
            {
                result = command.Evaluate(context);
            }
            catch (NextSignal)
            {
                continue; // skip to next iteration
            }
            catch (RedoSignal)
            {
                continue; // redo current iteration
            }
            catch (BreakSignal br)
            {
                return br.Value; // exit loop early
            }
        }

        return result;
    }

    //public object? Evaluate(Context context)
    //{
    //    for (object value = condition.Evaluate(context); value == null || false.Equals(value); value = condition.Evaluate(context))
    //        command.Evaluate(context);

    //    return null;
    //}

    public IList<string> GetLocalVariables()
    {
        var list = new List<IExpression>() { condition, command };

        return BaseExpression.GetLocalVariables(list);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is UntilExpression cmd)
        {
            return condition.Equals(cmd.condition) && command.Equals(cmd.command);
        }

        return false;
    }

    public override int GetHashCode() => condition.GetHashCode() + command.GetHashCode() + hashcode;
}
