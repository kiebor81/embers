using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

[StdLib("sin", TargetTypes = new[] { "Fixnum", "Float" })]
public class SinFunction : StdFunction
{
    [Comments("Returns the sine of a number (in radians).")]
    [Arguments(ParamNames = new[] { "number" }, ParamTypes = new[] { typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("sin expects a numeric argument");

        var value = values[0];
        if (!NumericCoercion.TryGetDouble(value, out var d))
            throw new TypeError("sin expects a numeric argument");

        return Math.Sin(d);
    }
}
