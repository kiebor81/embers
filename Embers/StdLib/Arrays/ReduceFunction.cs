using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("reduce", "inject", TargetTypes = new[] { "Array", "Range" })]
public class ReduceFunction : StdFunction
{
    [Comments("Combines elements by applying a block to an accumulator and each element.")]
    [Arguments(ParamNames = new[] { "collection", "initial" }, ParamTypes = new[] { typeof(Array), typeof(object) })]
    [Returns(ReturnType = typeof(object))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("reduce expects a collection argument");

        if (context.Block == null)
            throw new ArgumentError("reduce expects a block");

        if (values[0] is IEnumerable enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            object accumulator;

            if (values.Count > 1)
            {
                accumulator = values[1];
            }
            else
            {
                if (!enumerator.MoveNext())
                    return null;

                accumulator = enumerator.Current;
            }

            while (enumerator.MoveNext())
                accumulator = context.Block.Apply(self, context, [accumulator, enumerator.Current]);

            return accumulator;
        }

        throw new TypeError("reduce expects an array or range");
    }
}
