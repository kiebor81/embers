using Embers.Annotations;
using Embers.Expressions;
using Embers.Language;

namespace Embers.Functions;
/// <summary>
/// BlockFunction represents a block of code that can be executed with a specific context.
/// </summary>
/// <seealso cref="IFunction" />
[ScannerIgnore]
public class BlockFunction(BlockExpression block) : IFunction
{
    private readonly BlockExpression block = block;

    public object Apply(DynamicObject self, Context caller, IList<object> values)
    {
        var context = new BlockContext(caller);

        var parameters = block.Parameters ?? [];

        // Bind parameters if needed
        for (int i = 0; i < parameters.Count && i < values.Count; i++)
            context.SetLocalValue(parameters[i], values[i]);

        return block.Body.Evaluate(context);
    }
}

