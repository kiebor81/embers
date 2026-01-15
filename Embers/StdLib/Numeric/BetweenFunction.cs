using Embers.Exceptions;
using Embers.Language;
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

        if (!double.TryParse(values[0].ToString(), out var number))
            throw new ArgumentError("between? expects number to be numeric");

        if (!double.TryParse(values[1].ToString(), out var min))
            throw new ArgumentError("between? expects min to be numeric");

        if (!double.TryParse(values[2].ToString(), out var max))
            throw new ArgumentError("between? expects max to be numeric");

        return number >= min && number <= max;
    }
}
