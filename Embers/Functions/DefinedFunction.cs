using Embers.Annotations;
using Embers.Expressions;
using Embers.Signals;

namespace Embers.Functions;
/// <summary>
/// DefinedFunction represents a user-defined function with a body and parameters.
/// This function can be called with a set of values and an optional block.
/// The existence of a block allows for more complex behaviour, such as passing in a closure or additional context.
/// </summary>
/// <seealso cref="ICallableWithBlock" />
[ScannerIgnore]
public class DefinedFunction(IExpression body, IList<string> parameters, Context context, string? blockParameterName = null) : ICallableWithBlock
{
    private readonly IExpression body = body;
    private readonly IList<string> parameters = parameters;
    private readonly Context context = context;
    private readonly string? blockParameterName = blockParameterName;

    public object ApplyWithBlock(DynamicObject self, Context context, IList<object> values, IFunction? block)
    {
        Context newcontext = new(self, this.context, block);

        int k = 0;
        foreach (var parameter in parameters)
        {
            newcontext.SetLocalValue(parameter, values[k]);
            k++;
        }

        // If there's a block parameter name, convert the block to a Proc and bind it
        if (blockParameterName != null)
        {
            if (block != null)
            {
                var proc = new Proc(block);
                newcontext.SetLocalValue(blockParameterName, proc);
            }
            else
            {
                // Block parameter defined but no block passed - set to nil
                newcontext.SetLocalValue(blockParameterName, null);
            }
        }

        try
        {
            return body.Evaluate(newcontext);
        }
        catch (ReturnSignal ret)
        {
            return ret.Value;
        }
    }

    public object Apply(DynamicObject self, Context context, IList<object> values) => ApplyWithBlock(self, context, values, null);

}

