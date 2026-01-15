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

        double value, min, max;
        
        if (values[0] is long l1) value = l1;
        else if (values[0] is int i1) value = i1;
        else if (values[0] is double d1) value = d1;
        else throw new TypeError("clamp expects numeric arguments");

        if (values[1] is long l2) min = l2;
        else if (values[1] is int i2) min = i2;
        else if (values[1] is double d2) min = d2;
        else throw new TypeError("clamp expects numeric arguments");

        if (values[2] is long l3) max = l3;
        else if (values[2] is int i3) max = i3;
        else if (values[2] is double d3) max = d3;
        else throw new TypeError("clamp expects numeric arguments");

        if (min > max)
            throw new ArgumentError("clamp: min must be less than or equal to max");

        return Math.Min(Math.Max(value, min), max);
    }
}
