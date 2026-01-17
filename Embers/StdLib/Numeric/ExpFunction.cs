using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

[StdLib("exp", TargetTypes = new[] { "Fixnum", "Float" })]
public class ExpFunction : StdFunction
{
    [Comments("Returns e raised to the power of the given number.")]
    [Arguments(ParamNames = new[] { "number" }, ParamTypes = new[] { typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("exp expects a numeric argument");

        var value = values[0];
        if (!NumericCoercion.TryGetDouble(value, out var d))
            throw new TypeError("exp expects a numeric argument");

        return Math.Exp(d);
    }
}
