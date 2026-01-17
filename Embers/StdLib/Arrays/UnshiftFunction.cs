using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("unshift", TargetType = "Array")]
public class UnshiftFunction : StdFunction
{
    [Comments("Adds an element to the beginning of an array in-place and returns the modified array.")]
    [Arguments(ParamNames = new[] { "array_data", "value" }, ParamTypes = new[] { typeof(Array), typeof(object) })]
    [Returns(ReturnType = typeof(Array))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null)
            throw new ArgumentError("unshift expects an array and a value");

        if (values[0] is IList arr)
        {
            arr.Insert(0, values[1]);
            return arr;
        }

        throw new TypeError("unshift expects an array as first argument");
    }
}

