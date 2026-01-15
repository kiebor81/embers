using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

[StdLib("lerp", TargetTypes = new[] { "Fixnum", "Float" })]
public class LerpFunction : StdFunction
{
    [Comments("Performs linear interpolation between a and b using parameter t.")]
    [Arguments(ParamNames = new[] { "a", "b", "t" }, ParamTypes = new[] { typeof(Number), typeof(Number), typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 3 || values[0] == null || values[1] == null || values[2] == null)
            throw new ArgumentError("lerp expects a, b, and t arguments");

        double a, b, t;
        if (values[0] is long l1) a = l1;
        else if (values[0] is int i1) a = i1;
        else if (values[0] is double d1) a = d1;
        else throw new TypeError("lerp expects numeric arguments");

        if (values[1] is long l2) b = l2;
        else if (values[1] is int i2) b = i2;
        else if (values[1] is double d2) b = d2;
        else throw new TypeError("lerp expects numeric arguments");

        if (values[2] is long l3) t = l3;
        else if (values[2] is int i3) t = i3;
        else if (values[2] is double d3) t = d3;
        else throw new TypeError("lerp expects numeric arguments");

        return a + (b - a) * t;
    }
}
