using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

[StdLib("clamp", TargetTypes = new[] { "Fixnum", "Float" })]
public class ClampFunction : StdFunction
{
    [Comments("Clamps a number between a minimum and maximum value.")]
    [Arguments(ParamNames = new[] { "value", "min", "max" }, ParamTypes = new[] { typeof(Number), typeof(Number), typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 3 || values[0] == null || values[1] == null || values[2] == null)
            throw new ArgumentError("clamp expects value, min, and max arguments");

        if (!NumericCoercion.TryGetDouble(values[0], out var value))
            throw new TypeError("clamp expects numeric arguments");

        if (!NumericCoercion.TryGetDouble(values[1], out var min))
            throw new TypeError("clamp expects numeric arguments");

        if (!NumericCoercion.TryGetDouble(values[2], out var max))
            throw new TypeError("clamp expects numeric arguments");

        if (min > max)
            throw new ArgumentError("clamp: min must be less than or equal to max");

        return Math.Min(Math.Max(value, min), max);
    }
}
