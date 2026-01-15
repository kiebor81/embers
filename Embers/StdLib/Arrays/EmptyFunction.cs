using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("empty?", TargetTypes = new[] { "Array" })]
public class EmptyFunction : StdFunction
{
    [Comments("Returns true if the array has no elements.")]
    [Arguments(ParamNames = new[] { "array_data" }, ParamTypes = new[] { typeof(Array) })]
    [Returns(ReturnType = typeof(Boolean))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("empty? expects an array argument");

        if (values[0] is IList list)
            return list.Count == 0;

        throw new TypeError("empty? expects an array");
    }
}
