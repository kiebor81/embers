using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Arrays;

[StdLib("index", TargetTypes = new[] { "Array" })]
public class IndexFunction : StdFunction
{
    [Comments("Returns the index of the first occurrence of the given element, or nil.")]
    [Arguments(ParamNames = new[] { "array_data", "value" }, ParamTypes = new[] { typeof(Array), typeof(object) })]
    [Returns(ReturnType = typeof(Number))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null)
            throw new ArgumentError("index expects an array and a value");

        if (values[0] is IList list)
        {
            var index = list.IndexOf(values[1]);
            return index < 0 ? null : (long)index;
        }

        throw new TypeError("index expects an array");
    }
}
