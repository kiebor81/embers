using Embers.Annotations;
using Embers.Exceptions;

namespace Embers.StdLib.Ranges;

[StdLib("size", "count", TargetType = "Range")]
public class SizeFunction : StdFunction
{
    [Comments("Returns the number of elements in the range.")]
    [Arguments(ParamNames = new[] { "range" }, ParamTypes = new[] { typeof(Language.Primitive.Range) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

        if (values[0] is not Language.Primitive.Range range)
            throw new TypeError("range must be a Range");

        if (range.TryGetIntBounds(out var intStart, out var intEnd))
            return intStart > intEnd ? 0 : (intEnd - intStart + 1);

        if (range.TryGetLongBounds(out var longStart, out var longEnd))
            return longStart > longEnd ? 0L : (longEnd - longStart + 1);

        if (range.TryGetDoubleBounds(out var doubleStart, out var doubleEnd))
        {
            if (doubleStart > doubleEnd)
                return 0L;

            var count = (long)Math.Floor(doubleEnd - doubleStart) + 1;
            return count < 0 ? 0L : count;
        }

        throw new TypeError("range must be numeric");
    }
}
