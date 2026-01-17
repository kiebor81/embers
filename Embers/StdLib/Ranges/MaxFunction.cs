using Embers.Annotations;
using Embers.Exceptions;

namespace Embers.StdLib.Ranges;

[StdLib("max", TargetType = "Range")]
public class MaxFunction : StdFunction
{
    [Comments("Returns the maximum element of the range.")]
    [Arguments(ParamNames = new[] { "range" }, ParamTypes = new[] { typeof(Language.Primitive.Range) })]
    [Returns(ReturnType = typeof(object))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

        var range = values[0] as Language.Primitive.Range;
        if (range == null)
            throw new TypeError("range must be a Range");
        if (range.TryGetIntBounds(out var intStart, out var intEnd))
            return intStart > intEnd ? null : intEnd;

        if (range.TryGetLongBounds(out var longStart, out var longEnd))
            return longStart > longEnd ? null : longEnd;

        if (range.TryGetDoubleBounds(out var doubleStart, out var doubleEnd))
            return doubleStart > doubleEnd ? null : doubleEnd;

        throw new TypeError("range must be numeric");
    }
}
