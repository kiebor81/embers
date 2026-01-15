using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("pop", TargetType = "Array")]
public class PopFunction : StdFunction
{
    [Comments("Removes and returns the last element of an array.")]
    [Arguments(ParamNames = new[] { "array_data" }, ParamTypes = new[] { typeof(Array) })]
    [Returns(ReturnType = typeof(object))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("pop expects an array argument");

        if (values[0] is IList arr)
        {
            if (arr.Count == 0)
                return null;
            var last = arr[arr.Count - 1];
            arr.RemoveAt(arr.Count - 1);
            return last;
        }

        throw new TypeError("pop expects an array");
    }
}

