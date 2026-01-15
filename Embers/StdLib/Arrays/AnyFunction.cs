using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("any?", TargetTypes = new[] { "Array", "Range" })]
public class AnyFunction : StdFunction
{
    [Comments("Returns true if any element (or block result) is truthy.")]
    [Arguments(ParamNames = new[] { "collection" }, ParamTypes = new[] { typeof(Array) })]
    [Returns(ReturnType = typeof(Boolean))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("any? expects a collection argument");

        if (values[0] is IEnumerable enumerable)
        {
            if (context.Block != null)
            {
                foreach (var item in enumerable)
                    if (IsTruthy(context.Block.Apply(self, context, [item])))
                        return true;

                return false;
            }

            foreach (var item in enumerable)
                if (IsTruthy(item))
                    return true;

            return false;
        }

        throw new TypeError("any? expects an array or range");
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
