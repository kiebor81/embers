using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

/// <summary>
/// Returns the minimum value from a list of numbers.
/// </summary>
[StdLib("min", TargetTypes = new[] { "Fixnum", "Float" })]
public class MinFunction : StdFunction
{
    [Comments("Returns the minimum value from a list of numbers.")]
    [Arguments(ParamNames = new[] { "values" }, ParamTypes = new[] { typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0)
            throw new ArgumentError("min expects at least one argument");

        double? min = null;

        foreach (var value in values)
        {
            if (!NumericCoercion.TryGetDouble(value, out var d))
                throw new TypeError("min expects numeric arguments");

            if (min == null || d < min.Value)
                min = d;
        }

        return min ?? throw new TypeError("min expects numeric arguments");
    }
}
