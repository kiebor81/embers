using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

/// <summary>
/// Returns the absolute value of a number.
/// </summary>
[StdLib("abs", TargetTypes = new[] { "Fixnum", "Float" })]
public class AbsFunction : StdFunction
{
    [Comments("Returns the absolute value of the given number.")]
    [Arguments(ParamNames = new[] { "number" }, ParamTypes = new[] { typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            return null;

        var value = values[0];
        if (NumericCoercion.TryGetLong(value, out var l))
            return Math.Abs(l);
        if (NumericCoercion.TryGetDouble(value, out var d))
            return Math.Abs(d);

        throw new ArgumentError("abs expects a numeric argument");
    }
}
