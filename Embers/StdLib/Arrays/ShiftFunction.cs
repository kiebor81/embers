using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("shift", TargetType = "Array")]
public class ShiftFunction : StdFunction
{
    [Comments("Removes and returns the first element of an array.")]
    [Arguments(ParamNames = new[] { "array_data" }, ParamTypes = new[] { typeof(Array) })]
    [Returns(ReturnType = typeof(object))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("shift expects an array argument");

        if (values[0] is IList arr)
        {
            if (arr.Count == 0)
                return null;
            var first = arr[0];
            arr.RemoveAt(0);
            return first;
        }

        throw new TypeError("shift expects an array");
    }
}

