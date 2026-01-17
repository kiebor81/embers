using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

/// <summary>
/// Returns whether a number is between two other numbers (inclusive).
/// </summary>
[StdLib("between?", TargetTypes = new[] { "Fixnum", "Float" })]
public class BetweenFunction : StdFunction
{
    [Comments("Returns true if the given number is between the two specified numbers (inclusive).")]
    [Arguments(ParamNames = new[] { "number", "min", "max" }, ParamTypes = new[] { typeof(Number), typeof(Number), typeof(Number) })]
    [Returns(ReturnType = typeof(bool))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 3 || values[0] == null || values[1] == null || values[2] == null)
            throw new ArgumentError("between? expects number, min, and max arguments");

        if (!NumericCoercion.TryGetDouble(values[0], out var number))
            throw new TypeError("between? expects number to be numeric");

        if (!NumericCoercion.TryGetDouble(values[1], out var min))
            throw new TypeError("between? expects min to be numeric");

        if (!NumericCoercion.TryGetDouble(values[2], out var max))
            throw new TypeError("between? expects max to be numeric");

        return number >= min && number <= max;
    }
}
