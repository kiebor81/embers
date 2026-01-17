using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

[StdLib("pow", TargetTypes = new[] { "Fixnum", "Float" })]
public class PowFunction : StdFunction
{
    [Comments("Raises x to the power of y (x^y).")]
    [Arguments(ParamNames = new[] { "x", "y" }, ParamTypes = new[] { typeof(Number), typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
            throw new ArgumentError("pow expects two numeric arguments");

        if (!NumericCoercion.TryGetDouble(values[0], out var x))
            throw new TypeError("pow expects numeric arguments");

        if (!NumericCoercion.TryGetDouble(values[1], out var y))
            throw new TypeError("pow expects numeric arguments");

        return Math.Pow(x, y);
    }
}
