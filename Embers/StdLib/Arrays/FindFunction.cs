using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("find", TargetTypes = new[] { "Array", "Range" })]
public class FindFunction : StdFunction
{
    [Comments("Returns the first element that matches the block or is truthy.")]
    [Arguments(ParamNames = new[] { "collection" }, ParamTypes = new[] { typeof(Array) })]
    [Returns(ReturnType = typeof(object))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("find expects a collection argument");

        if (values[0] is IEnumerable enumerable)
        {
            if (context.Block != null)
            {
                foreach (var item in enumerable)
                    if (IsTruthy(context.Block.Apply(self, context, [item])))
                        return item;

                return null;
            }

            foreach (var item in enumerable)
                if (IsTruthy(item))
                    return item;

            return null;
        }

        throw new TypeError("find expects an array or range");
    }

    private static bool IsTruthy(object value)
    {
        if (value == null)
            return false;
        if (value is bool b)
            return b;
        return true;
    }
}
