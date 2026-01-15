using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("each_with_index", TargetTypes = new[] { "Array", "Range" })]
public class EachWithIndexFunction : StdFunction
{
    [Comments("Iterates over elements, yielding each value with its index to the block.")]
    [Arguments(ParamNames = new[] { "collection" }, ParamTypes = new[] { typeof(Array) })]
    [Returns(ReturnType = typeof(object))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("each_with_index expects a collection argument");

        if (context.Block == null)
            throw new ArgumentError("each_with_index expects a block");

        if (values[0] is IEnumerable enumerable)
        {
            long index = 0;
            foreach (var item in enumerable)
            {
                context.Block.Apply(self, context, [item, index]);
                index++;
            }

            return values[0];
        }

        throw new TypeError("each_with_index expects an array or range");
    }
}
