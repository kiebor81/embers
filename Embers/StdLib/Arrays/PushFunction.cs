using Embers.Exceptions;
using Embers.Annotations;
using Embers.Utilities;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("push", TargetType = "Array")]
public class PushFunction : StdFunction
{
    [Comments("Adds an element to the end of an array and returns the array.")]
    [Arguments(ParamNames = new[] { "array_data", "value" }, ParamTypes = new[] { typeof(Array), typeof(object) })]
    [Returns(ReturnType = typeof(Array))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null)
            throw new ArgumentError("push expects an array and a value");

        if (values[0] is IList arr)
        {
            if ((arr is DynamicArray dynArray && dynArray.IsFrozen) || FrozenState.IsFrozen(arr))
                throw new FrozenError("can't modify frozen Array");

            arr.Add(values[1]);
            return arr;
        }

        throw new TypeError("push expects an array as first argument");
    }
}

