using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("size", "length", TargetTypes = new[] { "Array" })]
public class SizeFunction : StdFunction
{
    [Comments("Returns the number of elements in the array.")]
    [Arguments(ParamNames = new[] { "array_data" }, ParamTypes = new[] { typeof(Array) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("size expects an array argument");

        if (values[0] is IList list)
            return list.Count;

        throw new TypeError("size expects an array");
    }
}
