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
public class DefinedFunction(IExpression body, IList<string> parameters, string? splatParameterName, string? kwargsParameterName, Context context, string? blockParameterName = null) : ICallableWithBlock
{
    private readonly IExpression body = body;
    private readonly IList<string> parameters = parameters;
    private readonly string? splatParameterName = splatParameterName;
    private readonly string? kwargsParameterName = kwargsParameterName;
    private readonly Context context = context;
    private readonly string? blockParameterName = blockParameterName;

    public object ApplyWithBlock(DynamicObject self, Context context, IList<object> values, IFunction? block)
    {
        Context newcontext = new(self, this.context, block);

        var argumentValues = values != null ? new List<object>(values) : [];
        KeywordArguments? keywordArguments = null;

        if (argumentValues.Count > 0 && argumentValues[^1] is KeywordArguments kwArgs)
        {
            keywordArguments = kwArgs;
            argumentValues.RemoveAt(argumentValues.Count - 1);
        }

        if (keywordArguments != null && kwargsParameterName == null)
        {
            argumentValues.Add(keywordArguments.Values);
            keywordArguments = null;
        }

        int k = 0;
        foreach (var parameter in parameters)
        {
            newcontext.SetLocalValue(parameter, k < argumentValues.Count ? argumentValues[k] : null);
            k++;
        }

        if (splatParameterName != null)
        {
            var rest = new DynamicArray();
            for (int i = parameters.Count; i < argumentValues.Count; i++)
                rest.Add(argumentValues[i]);
            newcontext.SetLocalValue(splatParameterName, rest);
        }

        if (kwargsParameterName != null)
            newcontext.SetLocalValue(kwargsParameterName, keywordArguments?.Values ?? new DynamicHash());

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

