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

        if (!NumericCoercion.TryGetDouble(values[0], out var a))
            throw new TypeError("lerp expects numeric arguments");

        if (!NumericCoercion.TryGetDouble(values[1], out var b))
            throw new TypeError("lerp expects numeric arguments");

        if (!NumericCoercion.TryGetDouble(values[2], out var t))
            throw new TypeError("lerp expects numeric arguments");

        return a + (b - a) * t;
    }
}
