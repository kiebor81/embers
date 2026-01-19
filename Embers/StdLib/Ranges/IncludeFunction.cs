using Embers.Annotations;
using Embers.StdLib.Numeric;
using Embers.Exceptions;

namespace Embers.StdLib.Ranges;

[StdLib("include?", "cover?", TargetType = "Range")]
public class IncludeFunction : StdFunction
{
    [Comments("Returns true if the range includes the given value.")]
    [Arguments(ParamNames = new[] { "range", "value" }, ParamTypes = new[] { typeof(Language.Primitive.Range), typeof(object) })]
    [Returns(ReturnType = typeof(Boolean))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 2)
            throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 1)");

        var range = values[0] as Language.Primitive.Range ?? throw new TypeError("range must be a Range");
        if (!NumericCoercion.TryGetDouble(values[1], out var testValue))
            return false;

        if (range.TryGetDoubleBounds(out var start, out var end))
        {
            if (start > end)
                return false;
            return testValue >= start && testValue <= end;
        }

        throw new TypeError("range must be numeric");
    }
}
