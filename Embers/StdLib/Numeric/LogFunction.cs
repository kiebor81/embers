using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

[StdLib("log", TargetTypes = new[] { "Fixnum", "Float" })]
public class LogFunction : StdFunction
{
    [Comments("Calculates the natural logarithm (base e) of a positive number.")]
    [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("log expects a numeric argument");

        var value = values[0];
        if (!NumericCoercion.TryGetDouble(value, out var d))
            throw new TypeError("log expects a numeric argument");

        if (d <= 0)
            throw new ArgumentError("log expects a positive number");

        return Math.Log(d);
    }
}
