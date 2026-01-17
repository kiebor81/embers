using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("each_value", TargetTypes = new[] { "Hash" })]
public class EachValueFunction : StdFunction
{
    [Comments("Iterates over values, yielding each value to the block.")]
    [Arguments(ParamNames = new[] { "hash" }, ParamTypes = new[] { typeof(Hash) })]
    [Returns(ReturnType = typeof(object))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("each_value expects a hash argument");

        if (context.Block == null)
            throw new ArgumentError("each_value expects a block");

        if (values[0] is IDictionary hash)
        {
            foreach (var value in hash.Values)
                context.Block.Apply(self, context, [value]);

            return values[0];
        }

        throw new TypeError("each_value expects a hash");
    }
}
