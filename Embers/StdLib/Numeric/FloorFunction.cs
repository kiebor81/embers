using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

[StdLib("floor", TargetTypes = new[] { "Fixnum", "Float" })]
public class FloorFunction : StdFunction
{
    [Comments("Returns the largest integer less than or equal to the given number.")]
    [Arguments(ParamNames = new[] { "number" }, ParamTypes = new[] { typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("floor expects a numeric argument");

        var value = values[0];
        if (!NumericCoercion.TryGetDouble(value, out var d))
            throw new TypeError("floor expects a numeric argument");

        return (long)Math.Floor(d);
    }
}
